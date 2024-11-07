using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowLibraryTesting
{
    internal class InterProcessCommunication
    {
        // interprogram communication

        const int WM_COPYDATA = 0x004A;
        const string WINDOW_TITLE = "ConsoleAppCluster";

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out COPYDATASTRUCT lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public static void Main()
        {
            // Start a thread for receiving messages
            Thread receiverThread = new Thread(ReceiveMessages);
            receiverThread.IsBackground = true;
            receiverThread.Start();

            // Start sending messages
            while (true)
            {
                Console.WriteLine("Enter message to send (or 'exit' to quit):");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                    break;

                // Send message to all instances (except sender)
                IntPtr[] hWnds = FindAllWindows();
                foreach (var hWnd in hWnds)
                {
                    if (hWnd != IntPtr.Zero)
                    {
                        COPYDATASTRUCT cds = CreateCopyDataStruct(input);
                        SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
                        Console.WriteLine($"Message sent to {hWnd} successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Receiver window not found.");
                    }
                }
            }
        }

        static void ReceiveMessages()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100); // Reduce CPU usage

                if (PeekMessage(out COPYDATASTRUCT cds, IntPtr.Zero, 0, 0, 1))
                {
                    string message = Marshal.PtrToStringUni(cds.lpData, cds.cbData / sizeof(char));
                    Console.WriteLine($"Received message: {message}");

                    // Send response (echo) back to sender
                    //SendMessage(result, WM_COPYDATA, IntPtr.Zero, ref cds);
                    Console.WriteLine($"Response sent to sender.");
                }
            }
        }

        static COPYDATASTRUCT CreateCopyDataStruct(string message)
        {
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(message);
            IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ptr, buffer.Length);

            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.dwData = IntPtr.Zero;
            cds.cbData = buffer.Length;
            cds.lpData = ptr;

            return cds;
        }

        static IntPtr[] FindAllWindows()
        {
            List<IntPtr> hWnds = new List<IntPtr>();

            string processName = Process.GetCurrentProcess().ProcessName;

            Process[] processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                IntPtr hWnd = process.Handle;
                if (hWnd != IntPtr.Zero)
                    hWnds.Add(hWnd);
            }
            return hWnds.ToArray();
        }
    }
}
