using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    public static partial class Windows
    {
        public sealed class WindowsHandle
        {
            /// <summary>
            /// The name of the process.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The handle of a managed window connected to the process.
            /// </summary>
            public IntPtr Handle { get; }

            public WindowsHandle(string Name, IntPtr Handle)
            {
                this.Name = Name;
                this.Handle = Handle;
            }

            /// <summary>
            /// Minimizes the window.
            /// </summary>
            public void Minimize()
            {
                MinimizeWindow(Handle);
            }

            /// <summary>
            /// Maximizes the window.
            /// </summary>
            public void Maximize()
            {
                MaximizeWindow(Handle);
            }

            /// <summary>
            /// Focuses the window.
            /// </summary>
            public void Focus()
            {
                RaiseWindowToForeground(Handle);
            }

            /// <summary>
            /// Brings the window up from being minimized.
            /// </summary>
            public void Show()
            {
                ShowWindow(Handle);
            }

            /// <summary>
            /// Makes the application critical. this means that the application, if closed, will cause windows to crash.<br></br><br></br>
            /// 
            /// requires administratve privileges for the application that runs this method (not the application that is made critical).
            /// </summary>
            public void MakeCritical()
            {
                int isCritical = 1;  // we want this to be a Critical Process
                int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

                Process.EnterDebugMode();  //acquire Debug Privileges

                // setting the BreakOnTermination = 1 for the current process
                NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
            }
        }
    }
}
