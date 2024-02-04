
using System.Diagnostics;
using DetOS;


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
                    FileName = ((_DetOS.IsMacOS()) ? "/usr/bin/open" : "cmd.exe"),
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false, // set this to true if you want to hide the terminal window
                    UseShellExecute = true
                };

                Process process = new Process();
                process.StartInfo = startInfo;

                // string? nircmdPath = null;
                process.StartInfo.Arguments = ((_DetOS.IsWindows()) ? $"/c start /wait /b {""} {fileName} && pause"
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
            catch (Exception)
            {
                throw;
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
            filteredFileNames = filteredFileNames
                .SelectMany(fileName =>
                    (fileName.EndsWith(".h", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".hpp", StringComparison.OrdinalIgnoreCase))
                        ? new string[] { fileName, Path.ChangeExtension(fileName, ".cpp") }
                        : new string[] { fileName })
                .ToArray();

            return filteredFileNames;


        }


    }
}



