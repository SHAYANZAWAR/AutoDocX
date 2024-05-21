#pragma warning disable CA1416


using System;
using DetOS;
using System.Diagnostics;
using Processes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace ScreenCapture
{


    public static class _ScreenCapture
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;


        const int SRCCOPY = 0x00CC0020;

        const int CAPTUREBLT = 0x40000000;


        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);


        public static bool CaptureWindowG(Process process, string ssTitle)
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
            try
            {
                CaptureWindow(processID, ssTitle);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        private static IntPtr GetWindowHandleByProcessId(int processId)
        {
            IntPtr hwnd = IntPtr.Zero;
            foreach (var process in Process.GetProcesses())
            {
                if (process.Id == processId)
                {

                    hwnd = process.MainWindowHandle;
                    if (hwnd == IntPtr.Zero)
                    {
                        Console.WriteLine("Main window handle not found.");
                        return IntPtr.Zero; 
                    }
                    break;
                }
            }
            return hwnd;
        }
        private static Bitmap CaptureRegion(int processID)
        {
            
            Bitmap result;            


            IntPtr hwnd = GetWindowHandleByProcessId(processID);

            if (!IsWindowVisible(hwnd))
            {
                Console.WriteLine("Window is not visible.");
                
                ShowWindow(hwnd, SW_RESTORE);

            }
            Console.WriteLine(hwnd);
            RECT region;
            Rectangle rect;
            GetWindowRect(hwnd, out region);
            rect = Rectangle.FromLTRB(region.Left, region.Top, region.Right, region.Bottom);

            Console.WriteLine(rect);

            IntPtr hdcSrc = GetWindowDC(hwnd);

            Console.WriteLine(hdcSrc);


            IntPtr memoryDC = CreateCompatibleDC(hdcSrc);

            IntPtr bitmap = CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);

            IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

            bool success = BitBlt(memoryDC, 0, 0, rect.Width, rect.Height, hdcSrc, rect.Left, rect.Top, SRCCOPY | CAPTUREBLT);


            try {
                if (!success) {
                    throw new Win32Exception("Failed to capture the screen.");
                }
                Console.WriteLine("Success");
                result = Image.FromHbitmap(bitmap);
            }
            finally {
                SelectObject(memoryDC, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDC);
                ReleaseDC(hwnd, hdcSrc);
            }


            return result;


        }

        

        public static void CaptureWindow(int processID, string ssTitle)
        {
            Bitmap bitmap = CaptureRegion(processID);
            bitmap.Save(ssTitle + ".png");
    
        }


    }
}

