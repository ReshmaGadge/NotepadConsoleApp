﻿// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

namespace AutoNotepadApp
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Class1
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        #region Import

        [STAThread]
        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("User32.dll")]
        public static extern IntPtr GetMenu(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        [DllImport("User32.dll")]
        public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);

        //[DllImport("user32.dll")]
        //public static extern int SendMessage(IntPtr hWnd, String wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint uMsg, uint wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        //public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public static extern int PostMessage(IntPtr hWnd, int uMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder s, int nMaxCount);
        #endregion

        const int WM_SETTEXT = 0X000C;
        private const int BN_CLICKED = 245;
        private const int BM_CLICK = 0x00F5;
        private const string FILENAME = "test.txt";

        public static string GetActiveWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        static IntPtr FindWindowByIndex(IntPtr hWndParent, int index)
        {
            if (index == 0)
                return hWndParent;
            else
            {
                int ct = 0;
                IntPtr result = IntPtr.Zero;
                do
                {
                    result = FindWindowEx(hWndParent, result, "CtrlNotifySink", null);
                    
                    if (result != IntPtr.Zero)
                        ++ct;
                }
                while (ct < index && result != IntPtr.Zero);
                return result;
            }
        }

        static void Main(string[] args)
        {
            var workingDirectory = Environment.CurrentDirectory;
            var file = $"{workingDirectory}\\{FILENAME}";

            Process p = null;
            try
            {
                Process[] processes = Process.GetProcessesByName("notepad");

                if (processes.Length == 0)
                {
                    p = new Process();
                    p.StartInfo.FileName = "notepad";
                    p.Start();
                    Console.WriteLine("Starting notepad ...");
                }
                else
                {
                    Console.WriteLine("Notepad already running !!");
                }

                processes = Process.GetProcessesByName("notepad");
                System.Threading.Thread.Sleep(100);

                if (processes.Length != 0)
                {
                    // Get Notepad window
                    IntPtr hWnd = FindWindow("Notepad", null); //to get a handle of the Notepad.

                    System.Threading.Thread.Sleep(100);
                    IntPtr child = FindWindowEx(processes[0].MainWindowHandle, new IntPtr(0), "Edit", null); //to get a handle to the Notepad Menu.

                    IntPtr menu = GetMenu(hWnd);
                    IntPtr subMenu = GetSubMenu(menu, 0);//0 = first menu item - File/New                    
                    uint menuItemID = GetMenuItemID(subMenu, 0);
                    Console.WriteLine("Selecting File > New");
                    SendMessage(hWnd, 0x0111, 0x20000000 + menuItemID, menu); // Stimulate clicking File Menu

                    Console.WriteLine("Sending 'Hello World' message to notepad");
                    SendMessage(child, WM_SETTEXT, 0, "Hello World");
                    System.Threading.Thread.Sleep(300);

                    uint menuItemID1 = GetMenuItemID(subMenu, 4);//2 = second item in submenu - File/Save As
                    //SendMessage(hWnd, 0x0111, 0x20000000 + menuItemID1, menu); // Stimulate clicking Save As submenu
                    //System.Threading.Thread.Sleep(100);

                    Console.WriteLine("Selecting File > Save As");
                    PostMessage(hWnd, 0x0111, 0x0003, 0x0);// Stimulate clicking Save As submenu

                    System.Threading.Thread.Sleep(100);

                    #region navigating through childs of save as window
                    IntPtr saveAsWnd = FindWindow("#32770", "Save As");
                    System.Threading.Thread.Sleep(100);
                    IntPtr DUIViewWnd = FindWindowEx(saveAsWnd, IntPtr.Zero, "DUIViewWndClassName", null);
                    System.Threading.Thread.Sleep(100);
                    IntPtr DirectUIHWND = FindWindowEx(DUIViewWnd, IntPtr.Zero, "DirectUIHWND", null);
                    System.Threading.Thread.Sleep(100);
                    IntPtr FloatNotifySinkWnd = FindWindowEx(DirectUIHWND, IntPtr.Zero, "FloatNotifySink", null);
                    System.Threading.Thread.Sleep(100);
                    IntPtr ComboBoxWnd = FindWindowEx(FloatNotifySinkWnd, IntPtr.Zero, "ComboBox", null);
                    System.Threading.Thread.Sleep(100);
                    IntPtr edithWnd = FindWindowEx(ComboBoxWnd, IntPtr.Zero, "Edit", null);
                    System.Threading.Thread.Sleep(100);
                    #endregion

                    //IntPtr edithWnd = IntPtr.Zero;
                    //edithWnd = FindWindowEx(saveAsWnd, new IntPtr(0), "ComboBox", "*.txt");
                    //edithWnd = FindWindowByIndex(saveAsWnd, 4);

                    Console.WriteLine("Set file path with filename");
                    SendMessage(edithWnd, WM_SETTEXT, 0, file);
                    System.Threading.Thread.Sleep(1000);

                    Console.WriteLine("Saving file by clicking save button");
                    IntPtr saveBN = FindWindowEx(saveAsWnd, IntPtr.Zero, "Button", "&Save");
                    PostMessage(saveBN, BM_CLICK, 0, 0);
                    System.Threading.Thread.Sleep(1000);

                    IntPtr confirmSaveAsWnd = FindWindow("#32770", "Confirm Save As");

                    if(confirmSaveAsWnd != IntPtr.Zero)
                    {
                        DirectUIHWND = FindWindowEx(confirmSaveAsWnd, IntPtr.Zero, "DirectUIHWND", null);
                        IntPtr CtrlNotifySink = FindWindowByIndex(DirectUIHWND, 7);
                        IntPtr yesBN = FindWindowEx(CtrlNotifySink, IntPtr.Zero, "Button", "&Yes");
                        Console.WriteLine("File already exists, overwriting by clicking 'Yes'");
                        System.Threading.Thread.Sleep(1000);
                        SendMessage(yesBN, BM_CLICK, 0, null);
                        System.Threading.Thread.Sleep(2000);
                    }

                    // to close notepad file
                    System.Threading.Thread.Sleep(100);
                    processes[0].Kill();    
                    Console.WriteLine("Notepad file closed!!!");

                    // to check whether file created or not
                    if (File.Exists(file))
                    {
                        Console.WriteLine("Task Completed Successfully!!!");
                    }
                    else
                    {
                        Console.WriteLine("Task failed :(");
                    }                                        
                }

                if (processes.Length == 0)
                {
                    throw new Exception("Could not launch notepad !!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}",
                         ex.Message, ex.StackTrace.ToString());
            }
        }


    }
}