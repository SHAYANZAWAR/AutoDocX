using System;
using System.Diagnostics;
using ScreenCapture;
using docx;
using DetOS;
using Processes;
namespace AutoDocx
{
    class _AutoDocX
    {

        public static bool addToDoc(string wordFilePath, string inputFilePath, string outputHeading, bool isMultipleFile)
        {


            if (!isFileExists(inputFilePath))
            {
                logError($"File Error: {inputFilePath} not found");
                Console.WriteLine("Either the file does not exist (could be spelling mistake)\nOr you are providing name of file in another directory");
                logFixes("Possible Fix: Provide full path or change to that directory");
                return false;
            }


            if (!doesGppExists())
            {
                logError("g++ compiler does not exist on your system");
                logFixes("Setup it first before using AutoDocX");
                return false;
            }

            // first of all executing the file provided
            string fileName = Path.GetFileName(inputFilePath);
            string compiledFileName = GetFileNameWithoutExtension(fileName);


            // compiling the given cpp file
            // "g++ fileName -o outputFileName" - command for compilation
            string errorMsg = "";
            if (_Processes.runProcess("g++", $"{inputFilePath} -o {compiledFileName}", ref errorMsg))
            {

                Process? process;

                try
                {
                    // if compilation is successfull, then execute
                    process = _Processes.RunCPlusPlusFile(compiledFileName);
                }
                catch (Exception ex)
                {
                    logError("An error occured while executing the c++ file: " + ex.Message);
                    return false;
                }
                if (process == null)
                {
                    return false;
                }
                // asking the user (are they sure to add?)
                if (getChoice("Are you sure to add?"))
                {

                    // taking screenshot of the code execution process and
                    // setting same name as compiled file
                    if (process != null && !process.HasExited)
                    {

                        if (_DetOS.IsMacOS()) _ScreenCapture.captureWindow(process, compiledFileName);
                    }

                    bool isWordFileFound = isFileExists(wordFilePath);
                    if (outputHeading == "default")
                    {
                        outputHeading = fileName + ":";
                    }

                    _docx.appendToDocX(isWordFileFound, isMultipleFile, wordFilePath, inputFilePath, outputHeading, (compiledFileName + ".png"));

                    // killing the process otherwise users have to manually close it.
                    System.Threading.Thread.Sleep(1000);




                }
                else
                {

                    if (File.Exists((compiledFileName + ".png")))
                    {
                        File.Delete((compiledFileName + ".png"));
                    }
                }

                if (process != null && !process.HasExited)
                {
                    _Processes.KillProcess(process);
                }
            } else {
                logError("Given .cpp file compilation was unsuccesfull.\nError:\n");
                Console.WriteLine(errorMsg);
                return false;
            }
        
            return true;
        }

        


        public static bool updateInDocx(bool isMultipleFile, string wordFilePath, string oldFileName, string newFilePath)
        {
            // checking if the file exist
            if (!isFileExists(newFilePath))
            {
                logError($"File Error: {newFilePath} not found");
                Console.WriteLine("Either the file does not exist (could be spelling mistake)\nOr you are providing name of file in another directory");
                logFixes("Possible Fix: Provide full path or change to that directory");
                return false;
            }

            // does g++ compiler exist on the system
            if (!doesGppExists())
            {
                logError("g++ compiler does not exist on your system");
                logFixes("Setup it first before using AutoDocX");
                return false;
            }



            // first of all executing the file provided
            string fileName = Path.GetFileName(newFilePath);
            string compiledFileName = GetFileNameWithoutExtension(fileName);


            if (!isFileExists(compiledFileName + ".exe") && !isFileExists(compiledFileName + ".png"))
            {
                string errorMsg = "";
                bool compilationResult = _Processes.runProcess("g++", $"{newFilePath} -o {compiledFileName}", ref errorMsg);

                // escaping the function if the compilation is unsuccessfull
                if (compilationResult == false)
                {
                    return compilationResult;
                }

                Process? process = null;
                if (compilationResult) process = _Processes.RunCPlusPlusFile(compiledFileName);

                if (_DetOS.IsMacOS() && process != null) _ScreenCapture.CaptureMacOSWindow(process.Id, compiledFileName + ".png");
            }

            // if only the .exe file exist then don't need to compile the .cpp file
            else if (isFileExists(compiledFileName + ".exe"))
            {
                Process? process = _Processes.RunCPlusPlusFile(compiledFileName);
                if (_DetOS.IsMacOS() && process != null) _ScreenCapture.CaptureMacOSWindow(process.Id, compiledFileName + ".png");
            }
            // adding 1 second delay before checking if image exists, granting it time 
            System.Threading.Thread.Sleep(1000);
            if (isFileExists(compiledFileName + ".png"))
            {
                _docx.updateDocX(isMultipleFile, wordFilePath, oldFileName, newFilePath);
            }




            return true;
        }

        public static void RemoveInDocX(string wordFile, string codeFilePath) {

            _docx.removeFromDocX(wordFile, codeFilePath);
        }

        
        private static string GetFileNameWithoutExtension(string filePath)
        {
            int extensionIndex = filePath.LastIndexOf('.');
            if (extensionIndex >= 0)
            {
                return filePath.Substring(0, extensionIndex);
            }

            // No extension found, return the original string
            return filePath;
        }


        public static bool doesGppExists()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "g++";
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    // Disable output redirection by setting an empty event handler
                    process.OutputDataReceived += (sender, e) => { };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool getChoice(string message)
        {
            message += " (y or yes / n or no)\n>> ";
            Console.Write(message);

            string? input = Console.ReadLine();

            while (true)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.Write("Please give any input\n>> ");
                }
                else if (string.Equals(input.Trim(), "y", StringComparison.OrdinalIgnoreCase) || string.Equals(input.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (string.Equals(input.Trim(), "n", StringComparison.OrdinalIgnoreCase) || string.Equals(input.Trim(), "no", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    Console.Write("Invalid input, Please enter again\n>> ");
                }

                input = Console.ReadLine();
            }
        }



        // to log errors        
        public static void logError(string errMsg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{errMsg}");
            Console.ResetColor();
        }

        public static void logFixes(string errMsg)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{errMsg}");
            Console.ResetColor();
        }

        private static bool isFileExists(string fileName)
        {

            // if it is a complete path , then it will be true
            if (File.Exists(fileName))
            {
                return true;
            }
            else
            {
                // otherwise the user may provided FileName,
                // combine filename and path to current working directory
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                return File.Exists(filePath);
            }
        }



    }
}