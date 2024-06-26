
//////////////////////////////////////////////////////////////////////////////

/*

Main file of "AutoDocX" , CLI (AutoDocXCLI.docx) directly calls its functions
Main Functions:
1. addToDoc
2. updateInDocx
3. RemoveInDocX

These functions are wrappers around the functions in "utils/docx.cs"
*/

/////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using ScreenCapture;
using docx;
using DetOS;
using Processes;

namespace AutoDocx
{
    class _AutoDocX
    {
        public static bool addToDoc(string wordFilePath, string inputFilePath, string outputHeading, bool isMultipleFile, string? avoidFiles)
        {


            // checking wether "gcc compiler" exists
            // and also wether inputFilePath exists
            if (initialCheckup(inputFilePath) == false)
            {
                return false;
            }


            // extract filename from the given Path (The case when path is provided instead of filename directly)
            string fileName = Path.GetFileName(inputFilePath);

            string compiledFileName = GetFileNameWithoutExtension(fileName);



            // compiling the program and checking the status
            if (handleCompilation(inputFilePath, isMultipleFile) == false)
            {
                return false;
            }

            Process? process;

            try
            {
                // if compilation is successfull, then execute
                process = _Processes.RunCPlusPlusFile(compiledFileName);
            }
            catch (Exception ex)
            {
                logError($"An error occured while executing the {compiledFileName + ".exe"} file: " + ex.Message);
                return false;
            }

            // if for some reason process is not created.
            if (process == null)
            {
                logError("c++ file not executed (reason: unknown)");
                return false;
            }



            // asking the user (are they sure to add?)
            if (getChoice("Are you sure to add?"))
            {
                // Checking if the process is still up or user has closed it
                if (!process.HasExited)
                {
                    // taking screenshot of the code execution process and
                    // setting same name as compiled file
                    _ScreenCapture.CaptureWindowG(process, compiledFileName);

                }
                else
                {
                    logError("Output Terminal has been closed, Can't take its screenshot");
                    logFixes("Fix: Don't close the output Terminal, AutoDocX will close it automatically");
                    // checking if the screenshot exists previously
                    if (doesFileExists(compiledFileName + ".png"))
                    {
                        logError("Using previously captured screenshot");
                    }

                }

                bool isWordFileFound = doesFileExists(wordFilePath);
                if (outputHeading == "default")
                {
                    outputHeading = fileName + ":";
                }
                string[] avoidFileList;
                if (avoidFiles != null)
                    avoidFileList = avoidFiles.Split(' ');
                else
                    avoidFileList = Array.Empty<string>();

                try
                {
                    if (!doesFileExists(compiledFileName + ".png"))
                    {
                        logError($"Screenshot not captured! Not added in {wordFilePath}");
                        _docx.appendToDocX(isWordFileFound, isMultipleFile, wordFilePath, inputFilePath, outputHeading, null, avoidFileList);

                    }
                    else
                    {
                        _docx.appendToDocX(isWordFileFound, isMultipleFile, wordFilePath, inputFilePath, outputHeading, (compiledFileName + ".png"), avoidFileList);
                    }

                }
                catch (Exception ex)
                {
                    logError("Word Document can't be created: " + ex.Message);
                }



                _Processes.KillProcess(process);


            }

            return true;
        }




        public static bool updateInDocx(bool isMultipleFile, string wordFilePath, string oldFileName, string newFilePath, string avoidFiles)
        {

            if (initialCheckup(newFilePath) == false)
            {
                return false;
            }

            if (checkFile(wordFilePath) == false)
            {
                return false;
            }

            // first of all executing the file provided
            string fileName = Path.GetFileName(newFilePath);
            string compiledFileName = GetFileNameWithoutExtension(fileName);


            if (handleCompilation(newFilePath, isMultipleFile) == false)
            {
                return false;
            }

            Process? process = null; // process object to contain the execution process

            try
            {

                process = _Processes.RunCPlusPlusFile(compiledFileName);
            }
            catch (Exception ex)
            {
                logError($"An error occured while executing the {compiledFileName + ".exe"} file: " + ex.Message);
                return false;
            }

            // if for some reasons proccess is not started
            if (process == null)
            {
                logError("c++ file not executed (reason: unknown)");
                return false;
            }


            // adding 1 second delay before checking if image exists, granting it time to be captured
            System.Threading.Thread.Sleep(1000);


            if (getChoice("Are you sure to update?"))
            {
                // Checking if the process is still up or user has closed it
                if (!process.HasExited)
                {
                    // taking screenshot of the code execution process and
                    // setting same name as compiled file
                    _ScreenCapture.CaptureWindowG(process, compiledFileName);

                }
                else
                {
                    logError("Output Terminal has been closed, Can't take its screenshot");
                    logFixes("Fix: Don't close the output Terminal, AutoDocX will close it automatically");
                    // checking if the screenshot exists previously
                    if (doesFileExists(compiledFileName + ".png"))
                    {
                        logError("Using previously captured screenshot");

                    }

                }

            }
            else
            {
                return false;
            }

            string[] avoidFileList;
            if (avoidFiles != null)
                avoidFileList = avoidFiles.Split(' ');
            else
                avoidFileList = Array.Empty<string>();


            if (!doesFileExists(compiledFileName + ".png"))
            {

                logError($"Screenshot of new file not captured! Not added in {wordFilePath}");

                _docx.updateDocX(isMultipleFile, wordFilePath, oldFileName, newFilePath, false, avoidFileList);

            }
            else
            {
                _docx.updateDocX(isMultipleFile, wordFilePath, oldFileName, newFilePath, true, avoidFileList);

            }



            // killing the execution process
            _Processes.KillProcess(process);

            return true;
        }

        public static bool RemoveInDocX(string wordFile, string codeFilePath)
        {
            if (checkFile(wordFile) == false)
            {
                return false;
            }
            _docx.removeFromDocX(wordFile, codeFilePath);
            return true;
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


        public static bool doesGppCompilerExists()
        {
            try
            {
                string errorMsg = "";
                bool res = _Processes.runProcess("g++", "--version", ref errorMsg);
                if (!res)
                {
                    logError("g++ compiler does not exist on your system");
                    logFixes("Setup it first before using AutoDocX");
                }
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        private static bool doesFileExists(string fileName)
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

        private static bool checkFile(string fileName)
        {
            if (!doesFileExists(fileName))
            {
                logError($"File Error: {fileName} not found");
                Console.WriteLine("Either the file does not exist (could be spelling mistake)\nOr you are providing name of file in another directory");
                logFixes("Possible Fix: Provide full path or change to that directory");
                return false;
            }
            return true;
        }
        private static bool initialCheckup(string fileName)
        {

            return checkFile(fileName) && doesGppCompilerExists();
        }

        private static bool handleCompilation(string inputFilePath, bool isMultipleFile)
        {
            string compiledFileName = GetFileNameWithoutExtension(Path.GetFileName(inputFilePath));
            string errorMsg = "";

            string[] compilationDeps = _Processes.getFileDependencies(inputFilePath);

            //! if user did'nt specified --mfile , but its still a multiple file
            //~ then for compilation we need to make isMultipleFile = true
            if (compilationDeps.Length > 1)
            {
                isMultipleFile = true;
            }

            if (isMultipleFile)
            {

                compilationDeps = compilationDeps.Where(fileName => fileName.EndsWith(".h", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".hpp", StringComparison.OrdinalIgnoreCase)).ToArray();

                string cppFileNames = "";
                string objFileNames = "";
                foreach (string file in compilationDeps)
                {
                    string fileNameWithoutExtention = GetFileNameWithoutExtension(file);
                    string fileNameWithCpp = fileNameWithoutExtention + ".cpp";
                    if (doesFileExists(fileNameWithCpp))
                    {
                        cppFileNames += fileNameWithCpp + " ";
                        objFileNames += fileNameWithoutExtention + ".o" + " ";

                    }
                    else
                    {
                        logError($"Implementation file \'{fileNameWithCpp}\' for \'{file}\' does not exists!");
                        logFixes($"Please make sure that implementation file for \'{file}\' is named \'{fileNameWithCpp}\'...");
                        return false;
                    }
                }
                cppFileNames = cppFileNames.Trim();
                objFileNames = objFileNames.Trim();

                if (_Processes.runProcess("g++", $"-c {inputFilePath} {cppFileNames}", ref errorMsg))
                {
                    if (_Processes.runProcess("g++", $"-o {compiledFileName} {objFileNames} {compiledFileName + ".o"}", ref errorMsg)) { }
                    else
                    {
                        logError("Some error occurred while linking: " + errorMsg);
                        return false;
                    }
                }
                else
                {
                    logError("Some error occurred while compiling: " + errorMsg);
                    return false;
                }
            }
            else
            {

                // "g++ fileName -o outputFileName" - command for compilation
                if (_Processes.runProcess("g++", $"{inputFilePath} -o {compiledFileName}", ref errorMsg)) { }
                else
                {
                    logError($"Given {inputFilePath} file compilation was unsuccesfull.\nError:\n");
                    Console.WriteLine(errorMsg);
                    return false;
                }

            }
            return true;

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

        static bool IsExeInPath(string exePath)
        {
            string? userSystemVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            if (String.IsNullOrEmpty(userSystemVariable))
            {
                return false;
            }
            string[] paths = userSystemVariable.Split(';');

            string exeFileName = Path.GetFileName(exePath);

            return paths.Any(path => path.Equals(exePath, StringComparison.OrdinalIgnoreCase) || path.EndsWith(exeFileName, StringComparison.OrdinalIgnoreCase));
        }

        static void AddExeToPath(string exePath)
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty;
            pathVariable = $"{exePath};{pathVariable}";

            Environment.SetEnvironmentVariable("PATH", pathVariable, EnvironmentVariableTarget.User);
        }

        static void addExeToDirectory(string destinationDirectory, string fileName)
        {
            string? projectDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sourcePath = Path.Combine(projectDirectory ?? "", "lib", fileName);
            string destinationPath = Path.Combine(destinationDirectory, fileName);

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            if (File.Exists(sourcePath) && !File.Exists(destinationPath))
            {
                File.Copy(sourcePath, destinationPath, true);
                Console.WriteLine($"{fileName} copied to {destinationDirectory}");
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



    }
}