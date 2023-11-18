
using DetOS;
using System.Diagnostics;
using Processes;

namespace ScreenCapture
{
    public static class _ScreenCapture
    {
        public static bool captureWindow(Process process, string ssTitle)
        {
            if (_DetOS.IsMacOS())
            {
                return CaptureMacOSWindow(process.Id, ssTitle);

            }
            Console.WriteLine("Unsupported operating system.");
            return false;

        }
        public static bool CaptureMacOSWindow(int processID, string ssTitle)
        {
            string errorMsg = "";
            bool doesCaptured = _Processes.runProcess("/bin/bash", $"-c \"screencapture -oxl{processID} {ssTitle + ".png"}\"", ref errorMsg);
            return doesCaptured;
        }

    }
}

