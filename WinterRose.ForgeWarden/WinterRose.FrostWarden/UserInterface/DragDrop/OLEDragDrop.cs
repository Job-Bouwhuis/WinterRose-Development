using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Input;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.UserInterface.DragDrop;

using static Win;

file class Win
{
    public const bool DEBUG = true;

    // OLE/drag-drop constants
    public const int WM_DROPFILES = 0x0233;
    public const short CF_HDROP = 15;
    public const short CF_UNICODETEXT = 13;
    public const int TYMED_HGLOBAL = 1;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x20;
    public const int WS_EX_LAYERED = 0x80000;
    public const int WH_MOUSE_LL = 14;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;

    // COM / Win32 externs (used by file extraction + init/cleanup)
    [DllImport("ole32.dll")]
    public static extern int RegisterDragDrop(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] IDropTarget pDropTarget);

    [DllImport("ole32.dll")]
    public static extern int RevokeDragDrop(IntPtr hwnd);

    [DllImport("ole32.dll")]
    public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);

    [DllImport("ole32.dll")]
    public static extern int OleInitialize(IntPtr pvReserved);

    [DllImport("ole32.dll")]
    public static extern void OleUninitialize();

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    // P/Invoke for GlobalLock/Unlock used by text extraction
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalUnlock(IntPtr hMem);
}

internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

[StructLayout(LayoutKind.Sequential)]
internal struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int X;
    public int Y;
}

// POINTL used by IDropTarget methods
[StructLayout(LayoutKind.Sequential)]
internal struct POINTL
{
    public int x;
    public int y;
}

// IDropTarget COM interface (GUID for IDropTarget)
[ComImport,
 Guid("00000122-0000-0000-C000-000000000046"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDropTarget
{
    [PreserveSig]
    int DragEnter([In] IDataObject pDataObj, int grfKeyState, POINTL pt, ref int pdwEffect);

    [PreserveSig]
    int DragOver(int grfKeyState, POINTL pt, ref int pdwEffect);

    [PreserveSig]
    int DragLeave();

    [PreserveSig]
    int Drop([In] IDataObject pDataObj, int grfKeyState, POINTL pt, ref int pdwEffect);
}

// Managed wrapper that implements IDropTarget and forwards events to the outer content instance
[ComVisible(true)]
internal class DropTargetImpl : IDropTarget
{
    static Log log = new Log("Win-DropTarget");

    private readonly OLEDragDrop parent;

    public DropTargetImpl(OLEDragDrop parent)
    {
        this.parent = parent;
        if (Win.DEBUG) 
            log.Debug("[DropTargetImpl] created");
    }

    public int DragEnter(IDataObject pDataObj, int grfKeyState, POINTL pt, ref int pdwEffect)
    {
        if (Win.DEBUG) log.Debug("DragEnter called");

        try
        {
            // If parent is currently in detection-awaiting state, treat this DragEnter
            // as *the* evidence that this was a genuine OLE drag. Do NOT run content validation here.
            bool wasAwaiting = false;
            try
            {
                // access awaiting flag on parent (internal state - safe to read)
                wasAwaiting = parent.awaitingDropCheck;
            }
            catch { wasAwaiting = false; }

            if (wasAwaiting)
            {
                // Mark we received OLE DragEnter during detection window
                if (DEBUG) log.Debug("DragEnter arrived while awaiting detection window - confirming OLE drag");

                // notify parent immediately that the heuristic drag was genuine (true)
                parent.HandleGlobalDragValidation(true);

                // restore passthrough immediately unless InputManager requests passthrough
                parent.RestorePassthroughImmediate();

                // clear awaiting flag so we don't double-handle
                parent.awaitingDropCheck = false;

                // signal that we "accept" for the shell but we are not performing content validation here
                pdwEffect = 1; // DROPEFFECT_COPY suggestion
                return 0; // S_OK
            }

            // Normal flow (not detection-only): try to extract files first, fallback to generic data
            List<string> files = null;
            try
            {
                files = ExtractFileListFromDataObject(pDataObj);
                if (DEBUG) log.Debug($"DragEnter extracted {files?.Count ?? 0} file(s)");
            }
            catch (Exception ex)
            {
                if (DEBUG) log.Debug("ExtractFileListFromDataObject failed: " + ex);
                files = null;
            }

            if (files != null && files.Count > 0)
            {
                parent.OnDragEnterFiles(files);
                pdwEffect = parent.CanAcceptFiles(files) ? 1 : 0;
                if (DEBUG) log.Debug($"DragEnter -> file path branch pdwEffect={pdwEffect}");
            }
            else
            {
                parent.OnDragEnterData(pDataObj);
                pdwEffect = parent.CanAcceptData(pDataObj) ? 1 : 0;
                if (DEBUG) log.Debug($"DragEnter -> generic data branch pdwEffect={pdwEffect}");
            }
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("DragEnter exception: " + ex);
            pdwEffect = 0;
        }
        return 0;
    }

    public int DragOver(int grfKeyState, POINTL pt, ref int pdwEffect)
    {
        try
        {
            // Prefer file-state if parent has recent file info; otherwise query CanAcceptData
            if (parent.HasRecentFileDrag())
            {
                pdwEffect = parent.Internal_GetEffectForFiles() ? 1 : 0;
            }
            else
            {
                pdwEffect = parent.CanAcceptData(null) ? 1 : 0;
            }
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("DragOver exception: " + ex);
            pdwEffect = 0;
        }
        return 0;
    }

    public int DragLeave()
    {
        if (DEBUG) log.Debug("DragLeave called");
        try
        {
            parent.IOnDragLeave();
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("DragLeave exception: " + ex);
        }
        return 0;
    }

    public int Drop(IDataObject pDataObj, int grfKeyState, POINTL pt, ref int pdwEffect)
    {
        if (DEBUG) log.Debug("Drop called");
        try
        {
            List<string> files = null;
            try
            {
                files = ExtractFileListFromDataObject(pDataObj);
                if (DEBUG) log.Debug($"Drop extracted {files?.Count ?? 0} file(s)");
            }
            catch (Exception ex)
            {
                if (DEBUG) log.Debug("ExtractFileListFromDataObject failed during Drop: " + ex);
                files = null;
            }

            if (files != null && files.Count > 0)
            {
                parent.IOnDropFiles(files);
                pdwEffect = parent.CanAcceptFiles(files) ? 1 : 0;
                if (DEBUG) log.Debug($"Drop -> file branch pdwEffect={pdwEffect}");
            }
            else
            {
                // Generic data drop
                parent.IOnDropData(pDataObj);
                pdwEffect = parent.CanAcceptData(pDataObj) ? 1 : 0;
                if (DEBUG) log.Debug($"Drop -> generic data branch pdwEffect={pdwEffect}");
            }
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("Drop exception: " + ex);
            pdwEffect = 0;
        }
        finally
        {
            try
            {
                parent.IOnDragLeave(); // clear state
            }
            catch (Exception ex)
            {
                if (DEBUG) log.Debug("OnDragLeave during Drop finally failed: " + ex);
            }
        }
        return 0;
    }

    // Helper: try to extract CF_HDROP file paths from IDataObject
    public static List<string> ExtractFileListFromDataObject(IDataObject dataObj)
    {
        var results = new List<string>();
        if (DEBUG) log.Debug("entry");

        // Build FORMATETC for CF_HDROP, TYMED_HGLOBAL
        FORMATETC fmt = new FORMATETC
        {
            cfFormat = CF_HDROP,
            ptd = IntPtr.Zero,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            tymed = (TYMED)TYMED_HGLOBAL
        };

        STGMEDIUM medium;
        try
        {
            if (DEBUG) log.Debug("calling dataObj.GetData");
            dataObj.GetData(ref fmt, out medium);
            if (DEBUG) log.Debug("GetData succeeded for CF_HDROP");
        }
        catch (COMException comEx)
        {
            // If the format is not present or invalid, try to fallback to text extraction. Don't rethrow.
            if (DEBUG) log.Debug("GetData failed for CF_HDROP: " + comEx);
            // try text fallback below
            return TryExtractTextAsPseudoFileList(dataObj);
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("GetData unexpected exception: " + ex);
            return TryExtractTextAsPseudoFileList(dataObj);
        }

        try
        {
            if ((int)medium.tymed == TYMED_HGLOBAL)
            {
                IntPtr hDrop = medium.unionmember; // handle to HDROP
                if (DEBUG) log.Debug("hDrop = " + hDrop);
                if (hDrop != IntPtr.Zero)
                {
                    uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                    if (DEBUG) log.Debug("DragQueryFile count = " + count);
                    for (uint i = 0; i < count; i++)
                    {
                        int len = (int)DragQueryFile(hDrop, i, null, 0);
                        var sb = new StringBuilder(len + 1);
                        if (DragQueryFile(hDrop, i, sb, sb.Capacity) > 0)
                        {
                            var path = sb.ToString();
                            if (DEBUG) log.Debug("file[" + i + "] = " + path);
                            results.Add(path);
                        }
                        else
                        {
                            if (DEBUG) log.Debug("DragQueryFile returned 0 for index " + i);
                        }
                    }
                }
                else
                {
                    if (DEBUG) log.Debug("hDrop was IntPtr.Zero");
                }
            }
            else
            {
                if (DEBUG) log.Debug("medium.tymed != TYMED_HGLOBAL");
                // fallback to text extraction
                return TryExtractTextAsPseudoFileList(dataObj);
            }
        }
        finally
        {
            // release the STGMEDIUM
            try
            {
                ReleaseStgMedium(ref medium);
                if (DEBUG) log.Debug("ReleaseStgMedium called");
            }
            catch (Exception ex)
            {
                if (DEBUG) log.Debug("ReleaseStgMedium failed: " + ex);
            }
        }

        if (DEBUG) log.Debug("returning " + results.Count + " file(s)");
        return results;
    }

    // If no CF_HDROP, attempt to extract CF_UNICODETEXT and return it as a single "pseudo-file" string (prefix so consumer can detect)
    private static List<string> TryExtractTextAsPseudoFileList(IDataObject dataObj)
    {
        var results = new List<string>();
        if (DEBUG) log.Debug("trying CF_UNICODETEXT fallback");

        FORMATETC fmtText = new FORMATETC
        {
            cfFormat = CF_UNICODETEXT,
            ptd = IntPtr.Zero,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            tymed = (TYMED)TYMED_HGLOBAL
        };

        STGMEDIUM medium;
        try
        {
            dataObj.GetData(ref fmtText, out medium);
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("GetData for CF_UNICODETEXT failed: " + ex);
            return results;
        }

        try
        {
            if ((int)medium.tymed == TYMED_HGLOBAL)
            {
                IntPtr hGlobal = medium.unionmember;
                if (hGlobal != IntPtr.Zero)
                {
                    IntPtr ptr = GlobalLock(hGlobal);
                    if (ptr != IntPtr.Zero)
                    {
                        try
                        {
                            string text = Marshal.PtrToStringUni(ptr);
                            if (!string.IsNullOrEmpty(text))
                            {
                                if (DEBUG) log.Debug("extracted text: " + text);
                                // mark it so the consumer knows it's text not a file path
                                results.Add("TEXT://" + text);
                            }
                        }
                        finally
                        {
                            GlobalUnlock(hGlobal);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("exception while extracting text: " + ex);
        }
        finally
        {
            try { ReleaseStgMedium(ref medium); } catch { }
        }

        if (DEBUG) log.Debug("returning " + results.Count + " pseudo-file(s)");
        return results;
    }
}

public class OLEDragDrop : IDisposable
{
    static Log log = new Log("Win-OLEDragDrop");

    // --- Public events for users of this class ---
    public event Action OnDragDetected;
    public event Action OnDragStopped;
    public event Action OnDragOver;
    public event Action<List<string>> OnDropFiles;
    public event Action<string> OnDropData;
    public event Action OnDragLeave;
    public event Func<List<string>, bool> ValidateDropCandidate;

    private const bool DEBUG = true;

    // --- Fields for global hook state ---
    internal LowLevelMouseProc mouseProcDelegate;
    internal IntPtr mouseHook = IntPtr.Zero;

    internal bool mouseDown = false;
    internal int mouseDownStartX = 0;
    internal int mouseDownStartY = 0;
    internal readonly object hookLock = new object();
    internal bool AnyGlobalDragActive = false;

    // --- Fields for drop target ---
    internal IDropTarget dropTarget;
    internal object dropTargetRCW;
    internal bool IsAcceptingCurrentDrag = false;
    internal bool IsDraggingNow = false;
    internal List<string> lastDraggedPaths = null;
    internal readonly object stateLock = new();

    // --- Passthrough temporal control ---
    internal bool passthroughTemporarilyDisabled = false;
    internal bool awaitingDropCheck = false;
    internal int awaitingDropFrames = 0;
    internal const int DISABLE_FRAMES = 5;

    public static void InitOle()
    {
        if (DEBUG) log.Debug("calling OleInitialize");
        int hr = OleInitialize(IntPtr.Zero);
        if (DEBUG) log.Debug("OleInitialize returned 0x" + hr.ToString("X8"));
        if (hr < 0) throw new Exception($"OleInitialize failed with HRESULT 0x{hr:X8}");
    }

    public OLEDragDrop()
    {
        IntPtr hwnd = Windows.MyHandle.Handle;

        try { InitOle(); } catch (Exception ex) { if (DEBUG) log.Debug("InitOle failed: " + ex); }

        try
        {
            var impl = new DropTargetImpl(this);
            dropTarget = impl as IDropTarget;
            dropTargetRCW = impl;
            int hr = RegisterDragDrop(hwnd, dropTarget);
            if (hr != 0)
            {
                dropTarget = null;
                dropTargetRCW = null;
            }
        }
        catch { dropTarget = null; dropTargetRCW = null; }

        try
        {
            mouseProcDelegate = MouseHookCallback;
            IntPtr hModule = GetModuleHandle(null);
            mouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseProcDelegate, hModule, 0);
        }
        catch { }
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            lock (hookLock)
            {
                if (msg == WM_LBUTTONDOWN)
                {
                    mouseDown = true;
                    mouseDownStartX = data.pt.X;
                    mouseDownStartY = data.pt.Y;
                }
                else if (msg == WM_MOUSEMOVE && mouseDown)
                {
                    int dx = data.pt.X - mouseDownStartX;
                    int dy = data.pt.Y - mouseDownStartY;
                    if ((dx * dx + dy * dy) > 16 && !AnyGlobalDragActive)
                    {
                        AnyGlobalDragActive = true;
                        StartTemporaryPassthroughDisable();
                    }
                }
                else if (msg == WM_LBUTTONUP && (mouseDown || AnyGlobalDragActive))
                {
                    mouseDown = false;
                    AnyGlobalDragActive = false;
                    lock (stateLock)
                    {
                        if (awaitingDropCheck)
                        {
                            awaitingDropCheck = false;
                            RestorePassthroughImmediate();
                            HandleGlobalDragValidation(false);
                        }
                    }
                }
            }
        }
        return CallNextHookEx(mouseHook, nCode, wParam, lParam);
    }

    private void StartTemporaryPassthroughDisable()
    {
        lock (stateLock)
        {
            if (passthroughTemporarilyDisabled) return;
            IntPtr hwnd = Windows.MyHandle.Handle;
            if (hwnd == IntPtr.Zero) return;

            SetWindowPassthrough(hwnd, false);
            passthroughTemporarilyDisabled = true;
            awaitingDropCheck = true;
            awaitingDropFrames = DISABLE_FRAMES;
        }
    }

    internal void RestorePassthroughImmediate()
    {
        try
        {
            IntPtr hwnd = Windows.MyHandle.Handle;
            if (hwnd == IntPtr.Zero) return;

            if (InputManager.EnablePassThrough || !ForgeWardenEngine.Current.Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow)) return;

            SetWindowPassthrough(hwnd, true);
            passthroughTemporarilyDisabled = false;
            awaitingDropCheck = false;
            awaitingDropFrames = 0;
        }
        catch { }
    }

    internal void SetWindowPassthrough(IntPtr hwnd, bool enable)
    {
        if (!ForgeWardenEngine.Current.Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow)) return;

        try
        {
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            if (enable) style |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
            else style &= ~WS_EX_TRANSPARENT;
            SetWindowLong(hwnd, GWL_EXSTYLE, style);
        }
        catch { }
    }

    public void Update()
    {
        if (awaitingDropCheck && passthroughTemporarilyDisabled)
        {
            bool cursorInside = IsCursorWithinWindow();
            if (cursorInside)
            {
                awaitingDropFrames--;
                if (awaitingDropFrames <= 0)
                {
                    awaitingDropCheck = false;
                    passthroughTemporarilyDisabled = false;
                    try { RestorePassthroughImmediate(); } catch { }
                    HandleGlobalDragValidation(false);
                }
            }
        }
    }

    public virtual void HandleGlobalDragValidation(bool valid)
    {
        if (DEBUG) log.Debug("heuristic drag validity = " + valid);
        if(valid)
            OnDragDetected?.Invoke();
        else
            OnDragStopped?.Invoke();
    }

    internal bool IsCursorWithinWindow()
    {
        try
        {
            IntPtr hwnd = Windows.MyHandle.Handle;
            if (hwnd == IntPtr.Zero) return false;
            if (!GetCursorPos(out POINT p)) return false;
            if (!GetWindowRect(hwnd, out RECT r)) return false;
            return p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
        }
        catch { return false; }
    }

    internal bool CanAcceptData(IDataObject dataObj)
    {
        return ValidateDropCandidate(DropTargetImpl.ExtractFileListFromDataObject(dataObj));
    }

    //Called by DropTargetImpl
    internal void OnDragEnterFiles(List<string> files)
    {
        lock (stateLock)
        {
            IsDraggingNow = true;
            lastDraggedPaths = files;
            try
            {
                IsAcceptingCurrentDrag = CanAcceptFiles(files);
            }
            catch { IsAcceptingCurrentDrag = false; }

            if (awaitingDropCheck)
            {
                awaitingDropCheck = false;
                HandleGlobalDragValidation(IsAcceptingCurrentDrag);
            }
        }

        if (IsAcceptingCurrentDrag)
            OnDragOver?.Invoke();
    }


    // simple acceptance policy: override/replace with your own logic (extensions, sizes, etc.)
    internal bool CanAcceptFiles(List<string> files)
    {
        if (DEBUG) log.Debug("validating " + (files?.Count ?? 0) + " files");
        if (files == null || files.Count == 0)
        {
            if (DEBUG) log.Debug("no files -> false");
            return false;
        }

        foreach (var path in files)
        {
            try
            {
                if (DEBUG) log.Debug("candidate: " + path);
            }
            catch (Exception ex)
            {
                if (DEBUG) log.Debug("exception validating " + path + " : " + ex);
                return false;
            }
        }

        if (DEBUG) log.Debug("returning true");
        return true; // default: accept
    }

    // Generic IDataObject entry point (non-file)
    internal void OnDragEnterData(IDataObject dataObj)
    {
        if (DEBUG) log.Debug("OnDragEnterData called (generic)");
        // Default behavior: do nothing. Consumers can override to inspect IDataObject.

        // If we were awaiting a drop-check, call validation hook for generic data
        lock (stateLock)
        {
            if (awaitingDropCheck)
            {
                awaitingDropCheck = false;
                bool accepted = CanAcceptData(dataObj);
                if (DEBUG) log.Debug("Generic DragEnter arrived during awaiting window -> genuine generic drag detected accepted=" + accepted);
                HandleGlobalDragValidation(accepted);
                if(accepted)
                    log.Debug("Drag enter data accepted");
            }
        }
    }

    internal bool HasRecentFileDrag()
    {
        lock (stateLock) return lastDraggedPaths != null && lastDraggedPaths.Count > 0;
    }

    internal bool Internal_GetEffectForFiles()
    {
        lock (stateLock) return IsAcceptingCurrentDrag;
    }

    // Called by DropTargetImpl
    internal void IOnDragLeave()
    {
        if (DEBUG) log.Debug("OnDragLeave called");
        lock (stateLock)
        {
            IsDraggingNow = false;
            lastDraggedPaths = null;
            IsAcceptingCurrentDrag = false;
        }
    }

    // Generic IDataObject drop
    internal void IOnDropData(IDataObject dataObj)
    {
        if (DEBUG) log.Debug("OnDropData called (generic)");
        // Default: try to extract text and treat it as a pseudo-file (consumer may override)
        try
        {
            var fmt = new FORMATETC
            {
                cfFormat = CF_UNICODETEXT,
                ptd = IntPtr.Zero,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                tymed = (TYMED)TYMED_HGLOBAL
            };

            STGMEDIUM medium;
            dataObj.GetData(ref fmt, out medium);
            try
            {
                if ((int)medium.tymed == TYMED_HGLOBAL)
                {
                    IntPtr hGlobal = medium.unionmember;
                    IntPtr ptr = GlobalLock(hGlobal);
                    if (ptr != IntPtr.Zero)
                    {
                        try
                        {
                            string text = Marshal.PtrToStringUni(ptr);
                            if (!string.IsNullOrEmpty(text))
                                HandleDroppedText(text);
                        }
                        finally { GlobalUnlock(hGlobal); }
                    }
                }
            }
            finally { try { ReleaseStgMedium(ref medium); } catch { } }
        }
        catch (Exception ex)
        {
            if (DEBUG) log.Debug("generic extraction failed: " + ex);
        }
    }

    // Called by DropTargetImpl
    internal void IOnDropFiles(List<string> files)
    {
        if (DEBUG) log.Debug("OnDropFiles called with files count: " + (files?.Count ?? 0));
        if (files == null || files.Count == 0) return;

        if (CanAcceptFiles(files))
        {
            if (DEBUG) log.Debug("OnDropFiles accepted files");
            foreach (var f in files)
            {
                try { HandleDroppedFile(f); }
                catch (Exception ex)
                {
                    if (DEBUG) log.Debug("HandleDroppedFile threw for " + f + " : " + ex);
                }
            }
        }
        else
        {
            if (DEBUG) log.Debug("OnDropFiles rejected files");
        }
    }

    // actual dropped-file handler — do your upload / enqueue etc. here
    internal void HandleDroppedFile(string path)
    {
        if (DEBUG) log.Debug("HeavyFileDropContent: dropped -> " + path);
    }

    // handle dropped text (from CF_UNICODETEXT)
    protected virtual void HandleDroppedText(string text)
    {
        if (DEBUG) log.Debug("HeavyFileDropContent: dropped text -> " + text);
    }

    public void Dispose()
    {
        try
        {
            IntPtr hwnd = Windows.MyHandle.Handle;
            if (hwnd != IntPtr.Zero) RevokeDragDrop(hwnd);
            OleUninitialize();
        }
        catch { }

        try
        {
            if (mouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(mouseHook);
                mouseHook = IntPtr.Zero;
            }
        }
        catch { }

        try
        {
            IntPtr hwnd = Windows.MyHandle.Handle;
            if (hwnd != IntPtr.Zero && passthroughTemporarilyDisabled)
            {
                SetWindowPassthrough(hwnd, true);
                passthroughTemporarilyDisabled = false;
                awaitingDropCheck = false;
                awaitingDropFrames = 0;
            }
        }
        catch { }

        dropTarget = null;
        dropTargetRCW = null;
    }
}
