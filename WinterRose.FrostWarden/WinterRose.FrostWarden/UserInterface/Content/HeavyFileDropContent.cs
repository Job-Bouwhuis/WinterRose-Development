using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.UserInterface.DragDrop;

public class HeavyFileDropContent : UIContent
{
    private OLEDragDrop oleDragDrop;

    public HeavyFileDropContent(OLEDragDrop? manager = null)
    {
        oleDragDrop = new OLEDragDrop();
    }

    protected internal override void Update()
    {
        oleDragDrop.Update();
    }

    protected internal override void OnOwnerClosing()
    {
        oleDragDrop.Dispose();
        base.OnOwnerClosing();
    }

    protected internal override float GetHeight(float maxWidth) => 200;

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(availableArea.Width, GetHeight(availableArea.Width));
    }

    protected override void Draw(Rectangle bounds)
    {
        // Example: use OLEDragDrop state to color border
        Color border = Color.White;
        bool localIsDraggingNow;
        bool localIsAccepting;
        bool localAnyGlobal;

        lock (oleDragDrop.stateLock)
        {
            localIsDraggingNow = oleDragDrop.IsDraggingNow;
            localIsAccepting = oleDragDrop.IsAcceptingCurrentDrag;
        }

        lock (oleDragDrop.hookLock)
        {
            localAnyGlobal = oleDragDrop.AnyGlobalDragActive;
        }

        if (IsHovered && (localIsDraggingNow || localAnyGlobal))
        {
            border = localIsDraggingNow ? (localIsAccepting ? Color.Green : Color.Red) : Color.Gray;
        }
        else
        {
            border = Color.White;
        }

        Raylib.DrawRectangleLines((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height, border);

        if (IsHovered && (localIsDraggingNow || localAnyGlobal))
        {
            string text = localIsDraggingNow ? (localIsAccepting ? "Drop to upload" : "Not supported") : "Dragging...";
            Raylib.DrawText(text, (int)bounds.X + 8, (int)bounds.Y + 8, 14, Color.White);
        }
    }
}

//public class HeavyFileDropContent : UIContent
//{
//    // --- DEBUG switch ---
//    private const bool DEBUG = true;

//    // helper to restore passthrough immediately
//    internal void RestorePassthroughImmediate()
//    {
//        try
//        {
//            IntPtr hwnd = Windows.MyHandle.Handle;
//            if (hwnd == IntPtr.Zero) return;

//            // If InputManager explicitly wants passthrough on, keep it that way.
//            if (typeof(InputManager).GetProperty("EnablePassThrough") != null)
//            {
//                bool enable = InputManager.EnablePassThrough;
//                if (enable)
//                {
//                    if (DEBUG) Console.WriteLine("[Passthrough] InputManager.EnablePassThrough == true, leaving passthrough enabled");
//                    return;
//                }
//            }

//            // restore passthrough to enabled state
//            SetWindowPassthrough(hwnd, true);
//            passthroughTemporarilyDisabled = false;
//            awaitingDropCheck = false;
//            awaitingDropFrames = 0;
//            if (DEBUG) Console.WriteLine("[Passthrough] Restored passthrough immediately after detection");
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[Passthrough] RestorePassthroughImmediate exception: " + ex);
//        }
//    }

//    // helper to ask whether cursor is inside our window bounds
//    internal bool IsCursorWithinWindow()
//    {
//        try
//        {
//            IntPtr hwnd = Windows.MyHandle.Handle;
//            if (hwnd == IntPtr.Zero) return false;
//            if (!GetCursorPos(out POINT p)) return false;
//            if (!GetWindowRect(hwnd, out RECT r)) return false;
//            return p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    internal void SetWindowPassthrough(IntPtr hwnd, bool enable)
//    {
//        if (!Application.Current.Window.ConfigFlags.HasFlag(ConfigFlags.TransparentWindow))
//        {
//            // attempt at making it so passthrough is ignored when the window is not having the flag to be transparent
//            return;
//        }

//        try
//        {
//            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
//            if (enable)
//            {
//                style |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
//            }
//            else
//            {
//                style &= ~WS_EX_TRANSPARENT;
//            }
//            SetWindowLong(hwnd, GWL_EXSTYLE, style);
//            if (DEBUG) Console.WriteLine($"[Passthrough] SetWindowPassthrough({enable}) -> style 0x{style:X8}");
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[Passthrough] SetWindowPassthrough exception: " + ex);
//        }
//    }

//    internal LowLevelMouseProc mouseProcDelegate;
//    internal IntPtr mouseHook = IntPtr.Zero;

//    // Global hook state (simple heuristic)
//    internal bool mouseDown = false;
//    internal int mouseDownStartX = 0;
//    internal int mouseDownStartY = 0;
//    internal readonly object hookLock = new object();
//    internal bool AnyGlobalDragActive = false; // true when we heuristically detect a drag started anywhere

//    // instance fields
//    internal IDropTarget dropTarget;                 // RCW for our drop target
//    internal object dropTargetRCW;                   // keep strong reference so COM isn't GC'd
//    internal bool IsAcceptingCurrentDrag = false;    // set during DragEnter/DragOver
//    internal bool IsDraggingNow = false;             // whether a drag session is active (enter without leave)
//    internal List<string> lastDraggedPaths = null;   // last paths enumerated on DragEnter/Drop
//    internal readonly object stateLock = new();

//    // --- NEW: passthrough temporal control fields ---
//    internal bool passthroughTemporarilyDisabled = false;
//    internal bool awaitingDropCheck = false;
//    internal int awaitingDropFrames = 0;
//    internal const int DISABLE_FRAMES = 5; // tune this small - number of Update() frames to keep passthrough off

//    public static void InitOle()
//    {
//        if (DEBUG) Console.WriteLine("[InitOle] calling OleInitialize");
//        int hr = OleInitialize(IntPtr.Zero);
//        if (DEBUG) Console.WriteLine("[InitOle] OleInitialize returned 0x" + hr.ToString("X8"));
//        if (hr < 0) // FAILED(hr)
//        {
//            if (DEBUG) Console.WriteLine("[InitOle] OleInitialize failed HRESULT=0x" + hr.ToString("X8"));
//            throw new Exception($"OleInitialize failed with HRESULT 0x{hr:X8}");
//        }
//    }

//    // ctor: register as drop target and install global mouse hook
//    public HeavyFileDropContent()
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] ctor start");
//        IntPtr hwnd = Windows.MyHandle.Handle; // user-provided accessor you used previously
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] using hwnd = " + hwnd);

//        try
//        {
//            InitOle();
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] InitOle failed: " + ex);
//            // still attempt registration to see behavior
//        }

//        try
//        {
//            var impl = new DropTargetImpl(this);
//            dropTarget = impl as IDropTarget;
//            dropTargetRCW = impl;
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] created DropTargetImpl and stored RCW");

//            int hr = RegisterDragDrop(hwnd, dropTarget);
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] RegisterDragDrop returned 0x" + hr.ToString("X8"));
//            if (hr != 0)
//            {
//                dropTarget = null;
//                dropTargetRCW = null;
//                if (DEBUG) Console.WriteLine("[HeavyFileDropContent] RegisterDragDrop failed - cleared references");
//            }
//            else
//            {
//                if (DEBUG) Console.WriteLine("[HeavyFileDropContent] RegisterDragDrop succeeded");
//            }
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] exception during RegisterDragDrop: " + ex);
//            dropTarget = null;
//            dropTargetRCW = null;
//        }

//        // install global mouse hook to detect drag starts anywhere
//        try
//        {
//            mouseProcDelegate = MouseHookCallback;
//            IntPtr hModule = GetModuleHandle(null);
//            mouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseProcDelegate, hModule, 0);
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] SetWindowsHookEx returned " + mouseHook);
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] failed to install mouse hook: " + ex);
//        }

//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] ctor end");
//    }

//    // mouse hook callback: heuristic drag detect (mouse down + move beyond small threshold)
//    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
//    {
//        try
//        {
//            if (nCode >= 0)
//            {
//                int msg = wParam.ToInt32();
//                var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
//                lock (hookLock)
//                {
//                    if (msg == WM_LBUTTONDOWN)
//                    {
//                        mouseDown = true;
//                        mouseDownStartX = data.pt.X;
//                        mouseDownStartY = data.pt.Y;
//                        //if (DEBUG) Console.WriteLine($"[MouseHook] LBUTTONDOWN at {mouseDownStartX},{mouseDownStartY}");
//                    }
//                    else if (msg == WM_MOUSEMOVE)
//                    {
//                        if (mouseDown)
//                        {
//                            int dx = data.pt.X - mouseDownStartX;
//                            int dy = data.pt.Y - mouseDownStartY;
//                            if ((dx * dx + dy * dy) > (4 * 4)) // threshold 4px
//                            {
//                                if (!AnyGlobalDragActive)
//                                {
//                                    AnyGlobalDragActive = true;
//                                    if (DEBUG) Console.WriteLine("[MouseHook] Heuristic drag started (global)");

//                                    // kick off a short passthrough-disable cycle so OLE can deliver DragEnter if it is real
//                                    StartTemporaryPassthroughDisable();
//                                }
//                            }
//                        }
//                    }
//                    else if (msg == WM_LBUTTONUP)
//                    {
//                        if (mouseDown || AnyGlobalDragActive)
//                        {
//                            mouseDown = false;
//                            AnyGlobalDragActive = false;
//                            if (DEBUG) Console.WriteLine("[MouseHook] LBUTTONUP - drag ended (global)");

//                            // if we were awaiting a detection window, cancel it now and restore passthrough (treat as false)
//                            lock (stateLock)
//                            {
//                                if (awaitingDropCheck)
//                                {
//                                    awaitingDropCheck = false;
//                                    // restore passthrough unless input manager explicitly wants passthrough
//                                    try { RestorePassthroughImmediate(); } catch { }
//                                    // notify invalid (user stopped dragging)
//                                    HandleGlobalDragValidation(false);
//                                    //if (DEBUG) Console.WriteLine("[Passthrough] Mouse up while awaiting detection -> cancelled (treated not genuine)");
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[MouseHook] exception: " + ex);
//        }

//        return CallNextHookEx(mouseHook, nCode, wParam, lParam);
//    }

//    // Start a short window-of-opportunity in which passthrough is disabled so DragEnter may arrive
//    private void StartTemporaryPassthroughDisable()
//    {
//        lock (stateLock)
//        {
//            if (passthroughTemporarilyDisabled) return;

//            IntPtr hwnd = Windows.MyHandle.Handle;
//            if (hwnd == IntPtr.Zero) return;

//            // disable passthrough for a few frames
//            SetWindowPassthrough(hwnd, false);
//            passthroughTemporarilyDisabled = true;
//            awaitingDropCheck = true;
//            awaitingDropFrames = DISABLE_FRAMES;
//            if (DEBUG) Console.WriteLine("[Passthrough] Temporarily disabled passthrough, awaiting drop check for " + DISABLE_FRAMES + " frames");
//        }
//    }

//    // Called by DropTargetImpl
//    internal void OnDragEnterFiles(List<string> files)
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDragEnterFiles called with files count: " + (files?.Count ?? 0));
//        lock (stateLock)
//        {
//            IsDraggingNow = true;
//            lastDraggedPaths = files;
//            try
//            {
//                IsAcceptingCurrentDrag = CanAcceptFiles(files);
//            }
//            catch (Exception ex)
//            {
//                if (DEBUG) Console.WriteLine("[HeavyFileDropContent] CanAcceptFiles threw: " + ex);
//                IsAcceptingCurrentDrag = false;
//            }

//            // If we were awaiting a drop-check, treat this as a genuine drag and validate
//            if (awaitingDropCheck)
//            {
//                awaitingDropCheck = false;
//                if (DEBUG) Console.WriteLine("[Passthrough] DragEnter arrived during awaiting window -> genuine drag detected");
//                HandleGlobalDragValidation(IsAcceptingCurrentDrag);
//                // we'll re-enable passthrough in Update once awaitingDropFrames countdown hits 0
//            }
//        }
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDragEnterFiles result IsAcceptingCurrentDrag=" + IsAcceptingCurrentDrag);
//    }

//    // Generic IDataObject entry point (non-file)
//    internal void OnDragEnterData(IDataObject dataObj)
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDragEnterData called (generic)");
//        // Default behavior: do nothing. Consumers can override to inspect IDataObject.

//        // If we were awaiting a drop-check, call validation hook for generic data
//        lock (stateLock)
//        {
//            if (awaitingDropCheck)
//            {
//                awaitingDropCheck = false;
//                bool accepted = CanAcceptData(dataObj);
//                if (DEBUG) Console.WriteLine("[Passthrough] Generic DragEnter arrived during awaiting window -> genuine generic drag detected accepted=" + accepted);
//                HandleGlobalDragValidation(accepted);
//            }
//        }
//    }

//    // Called by DropTargetImpl
//    internal void OnDragLeave()
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDragLeave called");
//        lock (stateLock)
//        {
//            IsDraggingNow = false;
//            lastDraggedPaths = null;
//            IsAcceptingCurrentDrag = false;
//        }
//    }

//    // Generic IDataObject drop
//    internal void OnDropData(IDataObject dataObj)
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDropData called (generic)");
//        // Default: try to extract text and treat it as a pseudo-file (consumer may override)
//        try
//        {
//            var fmt = new FORMATETC
//            {
//                cfFormat = CF_UNICODETEXT,
//                ptd = IntPtr.Zero,
//                dwAspect = DVASPECT.DVASPECT_CONTENT,
//                lindex = -1,
//                tymed = (TYMED)TYMED_HGLOBAL
//            };

//            STGMEDIUM medium;
//            dataObj.GetData(ref fmt, out medium);
//            try
//            {
//                if ((int)medium.tymed == TYMED_HGLOBAL)
//                {
//                    IntPtr hGlobal = medium.unionmember;
//                    IntPtr ptr = GlobalLock(hGlobal);
//                    if (ptr != IntPtr.Zero)
//                    {
//                        try
//                        {
//                            string text = Marshal.PtrToStringUni(ptr);
//                            if (!string.IsNullOrEmpty(text))
//                                HandleDroppedText(text);
//                        }
//                        finally { GlobalUnlock(hGlobal); }
//                    }
//                }
//            }
//            finally { try { ReleaseStgMedium(ref medium); } catch { } }
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[OnDropData] generic extraction failed: " + ex);
//        }
//    }

//    internal bool CanAcceptData(IDataObject dataObj)
//    {
//        if (DEBUG) Console.WriteLine("[CanAcceptData] called");
//        // Default: reject generic data unless overridden
//        return false;
//    }

//    // Called by DropTargetImpl
//    internal void OnDropFiles(List<string> files)
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDropFiles called with files count: " + (files?.Count ?? 0));
//        if (files == null || files.Count == 0) return;

//        if (CanAcceptFiles(files))
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDropFiles accepted files");
//            foreach (var f in files)
//            {
//                try { HandleDroppedFile(f); }
//                catch (Exception ex)
//                {
//                    if (DEBUG) Console.WriteLine("[HeavyFileDropContent] HandleDroppedFile threw for " + f + " : " + ex);
//                }
//            }
//        }
//        else
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnDropFiles rejected files");
//        }
//    }

//    // simple acceptance policy: override/replace with your own logic (extensions, sizes, etc.)
//    internal bool CanAcceptFiles(List<string> files)
//    {
//        if (DEBUG) Console.WriteLine("[CanAcceptFiles] validating " + (files?.Count ?? 0) + " files");
//        if (files == null || files.Count == 0)
//        {
//            if (DEBUG) Console.WriteLine("[CanAcceptFiles] no files -> false");
//            return false;
//        }

//        foreach (var path in files)
//        {
//            try
//            {
//                if (DEBUG) Console.WriteLine("[CanAcceptFiles] candidate: " + path);
//            }
//            catch (Exception ex)
//            {
//                if (DEBUG) Console.WriteLine("[CanAcceptFiles] exception validating " + path + " : " + ex);
//                return false;
//            }
//        }

//        if (DEBUG) Console.WriteLine("[CanAcceptFiles] returning true");
//        return true; // default: accept
//    }

//    // actual dropped-file handler — do your upload / enqueue etc. here
//    internal void HandleDroppedFile(string path)
//    {
//        if (DEBUG) Console.WriteLine("HeavyFileDropContent: dropped -> " + path);
//    }

//    // handle dropped text (from CF_UNICODETEXT)
//    protected virtual void HandleDroppedText(string text)
//    {
//        if (DEBUG) Console.WriteLine("HeavyFileDropContent: dropped text -> " + text);
//    }

//    // Utility inspectors used by DropTargetImpl.DragOver
//    internal bool HasRecentFileDrag()
//    {
//        lock (stateLock) return lastDraggedPaths != null && lastDraggedPaths.Count > 0;
//    }

//    internal bool Internal_GetEffectForFiles()
//    {
//        lock (stateLock) return IsAcceptingCurrentDrag;
//    }

//    // Called by MouseHook/Drop logic when we determine if heuristic drag was genuine or not.
//    // For now it just logs; you can override/use it to tie into engine behavior.
//    public virtual void HandleGlobalDragValidation(bool valid)
//    {
//        if (DEBUG) Console.WriteLine("[GlobalDragValidation] heuristic drag validity = " + valid);
//    }

//    // Clean up when owner closes — revoke drag-drop, uninstall hook, release COM object
//    protected internal override void OnOwnerClosing()
//    {
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnOwnerClosing - starting cleanup");
//        try
//        {
//            IntPtr hwnd = Windows.MyHandle.Handle;
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnOwnerClosing - hwnd = " + hwnd);
//            if (hwnd != IntPtr.Zero)
//            {
//                int hr = RevokeDragDrop(hwnd);
//                if (DEBUG) Console.WriteLine("[HeavyFileDropContent] RevokeDragDrop returned 0x" + hr.ToString("X8"));
//            }

//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] calling OleUninitialize");
//            try { OleUninitialize(); } catch (Exception ex) { if (DEBUG) Console.WriteLine("[OnOwnerClosing] OleUninitialize exception: " + ex); }
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OleUninitialize returned");
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnOwnerClosing exception: " + ex);
//        }

//        // uninstall mouse hook
//        try
//        {
//            if (mouseHook != IntPtr.Zero)
//            {
//                bool ok = UnhookWindowsHookEx(mouseHook);
//                if (DEBUG) Console.WriteLine("[HeavyFileDropContent] UnhookWindowsHookEx result = " + ok);
//                mouseHook = IntPtr.Zero;
//            }
//        }
//        catch (Exception ex)
//        {
//            if (DEBUG) Console.WriteLine("[HeavyFileDropContent] Unhook exception: " + ex);
//        }

//        // ensure passthrough restored
//        try
//        {
//            IntPtr hwnd = Windows.MyHandle.Handle;
//            if (hwnd != IntPtr.Zero && passthroughTemporarilyDisabled)
//            {
//                SetWindowPassthrough(hwnd, true);
//                passthroughTemporarilyDisabled = false;
//                awaitingDropCheck = false;
//                awaitingDropFrames = 0;
//                if (DEBUG) Console.WriteLine("[Passthrough] Restored passthrough on closing");
//            }
//        }
//        catch { }

//        // clear references to allow RCW to be freed
//        dropTarget = null;
//        dropTargetRCW = null;

//        base.OnOwnerClosing();
//        if (DEBUG) Console.WriteLine("[HeavyFileDropContent] OnOwnerClosing - finished");
//    }

//    // UI sizing as before
//    public override Vector2 GetSize(Rectangle availableArea)
//    {
//        return new Vector2(availableArea.Width, GetHeight(availableArea.Width));
//    }

//    protected internal override float GetHeight(float maxWidth)
//    {
//        return 200;
//    }

//    // IMPORTANT: you said your input manager calls Update() / Setup() etc. each frame.
//    // Use Update() to count down the short window where passthrough is disabled.
//    protected internal override void Update()
//    {
//        // If we are awaiting a drop-check (passthrough disabled), only count down when cursor is inside window.
//        if (awaitingDropCheck && passthroughTemporarilyDisabled)
//        {
//            bool cursorInside = IsCursorWithinWindow();
//            if (DEBUG) Console.WriteLine("[Passthrough] awaitingDropFrames = " + awaitingDropFrames + " cursorInside=" + cursorInside);

//            if (cursorInside)
//            {
//                // decrement frames only when cursor inside window
//                awaitingDropFrames--;
//                if (awaitingDropFrames <= 0)
//                {
//                    // timed out without DragEnter while cursor was inside -> treat as false positive
//                    awaitingDropCheck = false;
//                    passthroughTemporarilyDisabled = false;

//                    // restore passthrough (respect InputManager.EnablePassThrough)
//                    try { RestorePassthroughImmediate(); } catch (Exception ex) { if (DEBUG) Console.WriteLine("[Passthrough] restore exception: " + ex); }

//                    // notify that the global drag was not genuine
//                    HandleGlobalDragValidation(false);
//                    if (DEBUG) Console.WriteLine("[Passthrough] timeout expired while cursor inside - treated as not genuine");
//                }
//            }
//            else
//            {
//                // cursor outside window: keep waiting (do not decrement). If user stops dragging, MouseHookCallback will cancel.
//                if (DEBUG) Console.WriteLine("[Passthrough] cursor left window while awaiting DragEnter - continuing to wait");
//            }
//        }
//    }

//    // Draw shows border: WHITE idle, GREEN when hovered & can accept, RED when hovered & not acceptable
//    protected override void Draw(Rectangle bounds)
//    {
//        Color border = Color.White;
//        bool localIsDraggingNow;
//        bool localIsAccepting;
//        bool localAnyGlobal;

//        lock (stateLock)
//        {
//            localIsDraggingNow = IsDraggingNow;
//            localIsAccepting = IsAcceptingCurrentDrag;
//        }

//        lock (hookLock)
//        {
//            localAnyGlobal = AnyGlobalDragActive;
//        }

//        if (IsHovered && (localIsDraggingNow || localAnyGlobal))
//        {
//            // If we know it's a file drag entered and have an accept decision, show green/red
//            if (localIsDraggingNow)
//                border = localIsAccepting ? Color.Green : Color.Red;
//            else
//                border = Color.Gray; // global drag active but not specific to this window
//        }
//        else
//        {
//            border = Color.White;
//        }

//        // draw outline
//        Raylib.DrawRectangleLines((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height, border);

//        // optional: draw helpful text
//        if (IsHovered && (localIsDraggingNow || localAnyGlobal))
//        {
//            string text = localIsDraggingNow ? (localIsAccepting ? "Drop to upload" : "Not supported") : "Dragging...";
//            Raylib.DrawText(text, (int)bounds.X + 8, (int)bounds.Y + 8, 14, Color.White);
//        }
//    }
//}


