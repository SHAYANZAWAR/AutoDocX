#pragma warning disable CA1416


using System;
using DetOS;
using System.Diagnostics;
using Processes;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenCapture
{


    public static class _ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);



        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool captureWindowG(Process process, string ssTitle)
        {
            if (_DetOS.IsMacOS())
            {
                return CaptureMacOSWindow(process.Id, ssTitle);

            }
            if (_DetOS.IsWindows())
            {
                return CaptureWindowsWindow(process.Id, ssTitle);
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

        public static bool CaptureWindowsWindow(int processID, string ssTitle)
        {

            IntPtr targetWindowHandle = GetWindowHandleByProcessId(processID);

            if (targetWindowHandle != IntPtr.Zero)
            {
                RECT windowRect;
                GetWindowRect(targetWindowHandle, out windowRect);

                if (windowRect.Right - windowRect.Left > 0 && windowRect.Bottom - windowRect.Top > 0)
                {

                    CaptureWindow(targetWindowHandle, ssTitle + ".png");
                    return true;
                }
                
            }
            

            return false;
        }

        private static IntPtr GetWindowHandleByProcessId(int processId)
        {
            IntPtr hwnd = IntPtr.Zero;
            foreach (var process in Process.GetProcesses())
            {
                if (process.Id == processId)
                {

                    hwnd = process.MainWindowHandle;
                    break;
                }
            }
            return hwnd;
        }
        private static void CaptureWindow(IntPtr hwnd, string filePath)
        {
            RECT windowRect;
            GetWindowRect(hwnd, out windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hdcBitmap = graphics.GetHdc();

                    // Use PrintWindow for capturing window content
                    PrintWindow(hwnd, hdcBitmap, 0);

                    graphics.ReleaseHdc(hdcBitmap);
                }

                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }



    }
}

