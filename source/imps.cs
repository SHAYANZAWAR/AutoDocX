//         // private static int GetWindowsTerminalProcessId()
//         // {
//         //     Process process = new Process();
//         //     ProcessStartInfo startInfo = new ProcessStartInfo
//         //     {
//         //         FileName = "tasklist",
//         //         RedirectStandardOutput = true,
//         //         UseShellExecute = false,
//         //         CreateNoWindow = true
//         //     };

//         //     process.StartInfo = startInfo;
//         //     process.Start();

//         //     string output = process.StandardOutput.ReadToEnd();
//         //     process.WaitForExit();

//         //     // Find the terminal process in the output
//         //     string terminalProcessName = "cmd.exe"; // Change this if using a different terminal
//         //     int terminalProcessId = -1;

//         //     string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//         //     foreach (string line in lines)
//         //     {
//         //         if (line.Contains(terminalProcessName))
//         //         {
//         //             terminalProcessId = int.Parse(line.Substring(0, line.IndexOf(" ")));
//         //             break;
//         //         }
//         //     }

//         //     return terminalProcessId;
//         // }






//         // private static int GetWindowsTerminalProcessId()
//         // {
//         //     Process process = new Process();
//         //     ProcessStartInfo startInfo = new ProcessStartInfo
//         //     {
//         //         FileName = "tasklist",
//         //         RedirectStandardOutput = true,
//         //         UseShellExecute = false,
//         //         CreateNoWindow = true
//         //     };

//         //     process.StartInfo = startInfo;
//         //     process.Start();

//         //     string output = process.StandardOutput.ReadToEnd();
//         //     process.WaitForExit();

//         //     // Find the terminal process in the output
//         //     string terminalProcessName = "cmd"; // Change this if using a different terminal
//         //     int terminalProcessId = -1;

//         //     string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//         //     foreach (string line in lines)
//         //     {
//         //         if (line.Contains(terminalProcessName))
//         //         {
//         //             string[] parts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
//         //             terminalProcessId = int.Parse(parts[1]);
//         //             break;
//         //         }
//         //     }

//         //     return terminalProcessId;
//         // }

//         // private static int GetMacOSTerminalProcessId()
//         // {
//         //     Process process = new Process();
//         //     ProcessStartInfo startInfo = new ProcessStartInfo
//         //     {
//         //         FileName = "pgrep",
//         //         Arguments = "Terminal",
//         //         RedirectStandardOutput = true,
//         //         UseShellExecute = false,
//         //         CreateNoWindow = true
//         //     };

//         //     process.StartInfo = startInfo;
//         //     process.Start();

//         //     string output = process.StandardOutput.ReadToEnd();
//         //     process.WaitForExit();

//         //     // Parse the PID of the terminal process
//         //     int terminalProcessId = -1;
//         //     if (int.TryParse(output, out terminalProcessId))
//         //     {
//         //         return terminalProcessId;
//         //     }

//         //     return -1;
//         // }


// public static void ExecuteAdditionalCommand(int processId, string command)
//         {
//             Console.WriteLine("hello");
//             IntPtr hWnd = FindWindow(null, $"Command Prompt - {processId}");
//             if (hWnd != IntPtr.Zero)
//             {
//                 SetForegroundWindow(hWnd);
//                 Thread.Sleep(500); // Give focus time to switch

//                 foreach (char c in command)
//                 {
//                     SendMessage(hWnd, WM_KEYDOWN, (IntPtr)c, IntPtr.Zero);
//                     SendMessage(hWnd, WM_KEYUP, (IntPtr)c, IntPtr.Zero);
//                 }

//                 SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
//                 SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_RETURN, IntPtr.Zero);
//             } else {
//                 Console.WriteLine("ptr is zero");
//             }
//         }





// using System;
// using System.Drawing;
// using System.Runtime.InteropServices;

// namespace Win32Capture {
//     static public class _Win32Capture
//     {
//         // Import the necessary Windows API functions
//         [DllImport("user32.dll")]
//         private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

//         [DllImport("user32.dll")]
//         private static extern IntPtr GetWindowDC(IntPtr hWnd);

//         [DllImport("gdi32.dll")]
//         private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

//         [DllImport("gdi32.dll")]
//         private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

//         [DllImport("gdi32.dll")]
//         private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

//         [DllImport("gdi32.dll")]
//         private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
//                                         IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

//         [DllImport("user32.dll")]
//         private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

//         [DllImport("gdi32.dll")]
//         private static extern int DeleteDC(IntPtr hdc);

//         [DllImport("gdi32.dll")]
//         private static extern int DeleteObject(IntPtr hObject);

//         // Function to take a screenshot of the window associated with a given PID
//         static public void TakeScreenshot(int pid, string outputFileName)
//         {
//             try
//             {
//                 // Step 1: Find the window handle (HWND) from the process ID (PID)
//                 IntPtr hWnd = FindWindow(null, pid.ToString());

//                 // Step 2: Use the window handle (HWND) to capture the screenshot
//                 IntPtr hDC = GetWindowDC(hWnd);
//                 IntPtr hDestDC = CreateCompatibleDC(hDC);

//                 // Get window dimensions
//                 RECT rect;
//                 GetWindowRect(hWnd, out rect);
//                 int width = rect.right - rect.left;
//                 int height = rect.bottom - rect.top;


//                 width = 600;
//                 height = 600;
//                 // Create a bitmap to hold the screenshot
//                 IntPtr hBitmap = CreateCompatibleBitmap(hDC, width, height);
//                 SelectObject(hDestDC, hBitmap);

//                 // Copy the screenshot into the bitmap
//                 BitBlt(hDestDC, 0, 0, width, height, hDC, 0, 0, 0x00CC0020);

//                 // Step 3: Save the screenshot to a file
//                 Image.FromHbitmap(hBitmap).Save(outputFileName);

//                 // Cleanup
//                 ReleaseDC(hWnd, hDC);
//                 DeleteDC(hDestDC);
//                 DeleteObject(hBitmap);

//                 Console.WriteLine("Screenshot captured and saved successfully.");
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine("Error capturing the screenshot: " + e.Message);
//             }
//         }

//         // Structure to represent the window dimensions
//         struct RECT
//         {
//             public int left;
//             public int top;
//             public int right;
//             public int bottom;
//         }

//         // Import the GetWindowRect function from user32.dll
//         [DllImport("user32.dll")]
//         private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
//     }

// }



