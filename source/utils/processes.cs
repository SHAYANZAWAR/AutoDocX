using System;
using System.Diagnostics;
using DetOS;
using System.Runtime.InteropServices;

namespace Processes
{
    public static class _Processes
    {

        public static Process? RunCPlusPlusFile(string fileName)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ((_DetOS.IsMacOS()) ? "/usr/bin/open" : "cmd"),
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false, // set this to true if you want to hide the terminal window
                    UseShellExecute = true
                };

                Process process = new Process();
                process.StartInfo = startInfo;

                // process.StartInfo.Arguments = ((_DetOS.IsWindows()) ? $"/c .\\Win32Dependencies\\process.bat {fileName} test.png" 
                // : $"-a Terminal.app {fileName}"); 
                string nircmdPath = System.IO.Directory.GetCurrentDirectory() + "/lib/nircmd.exe";
                process.StartInfo.Arguments = ((_DetOS.IsWindows()) ? $"/k {fileName} && timeout /t 1 /nobreak >nul && {nircmdPath} savescreenshotwin {fileName + ".png"}"
                : $"-a Terminal.app {fileName}");


                process.Start();

                // Wait a moment to give the terminal window time to start
                System.Threading.Thread.Sleep(1000);


                return process;
            }
            catch (Exception)
            {

                throw;
            }
        }




        public static bool runProcess(string fileName, string arguments, ref string errorMsg)
        {

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    errorMsg = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while compiling the cpp file: " + ex.Message);
                return false;
            }

        }



        public static void KillProcess(Process process)
        {
            try
            {
                process.Kill();
                
                process.CloseMainWindow();
                process.Dispose();

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static string[] getFileDependencies(string mainFileName)
        {


            string command = "g++";
            // Create a new process start info
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = $"-MM {mainFileName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };

            // starting the process
            process.Start();


            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            string[] depFileNames = output.Split(' ');


            string[] filteredFileNames = depFileNames
                .Where(fileName => !fileName.EndsWith(".o:"))
                .Select(e => e.Trim())
                .ToArray();

            // reversing the array, fileNames from least order to most
            Array.Reverse(filteredFileNames);

            return filteredFileNames;


        }


    }
}




// PING -n 1 127.0.0.1>nul && nircmd.exe savescreenshotwin {fileName} 