// code taking care of screencapturing...

using DetOS;
using System.Diagnostics;
using Processes;

namespace ScreenCapture {
    public static class _ScreenCapture
    {
        public static bool captureWindow(Process process, string ssTitle)
        {
            if (_DetOS.IsMacOS())
            {
                return CaptureMacOSWindow(process.Id, ssTitle);
                
            }
            else if (_DetOS.IsWindows())
            {
                return CaptureCMD(process, ssTitle);
                
            }
        
            Console.WriteLine("Unsupported operating system.");
            return false;


        }
        public static bool CaptureMacOSWindow(int processID, string ssTitle) {
            // var captureProcess = new Process();
            // captureProcess.StartInfo.FileName = "/bin/bash";
            // // captureProcess.StartInfo.Arguments = $"-c \"screencapture -oxl$(osascript -e 'tell app \\\"{application}\\\" to id of window 1') {savePath}\"";
            // captureProcess.StartInfo.Arguments = $"-c \"screencapture -oxl{processID} {ssTitle + ".png"}\"";
            string errorMsg = "";
            // captureProcess.Start();
            bool doesCaptured = _Processes.runProcess("/bin/bash", $"-c \"screencapture -oxl{processID} {ssTitle + ".png"}\"", ref errorMsg);

            return doesCaptured;
        }

        
        public static bool CaptureCMD(Process process, string screenshotFileName)
        {
            return true;
        }



    }
}


// nircmd.exe savescreenshotwin screenshot.png id process_id

// nircmd.exe savescreenshot [output_file_path] [x_pos] [y_pos] [width] [height] [process_id]

// ScreenshotCmd.exe --pid [Your_Process_ID] --output [Output_File_Path]

// wmic process where ProcessId="process_id" call GetPackageFamilyName

// nircmd.exe savescreenshotwin [outputFileName]

// command to get the list of dependencies of a c++ program
// g++ -MM testing.cpp *.cpp | findstr /V /C:"testing.cpp"