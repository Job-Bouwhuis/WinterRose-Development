using Raylib_cs;
using System.ComponentModel;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public abstract class ToastRegionManager
{
    protected ToastRegionManager(ToastRegion region)
    {
        Region = region;
    }

    public InputContext Input => Toasts.Input;

    public bool IsSomeoneHovered
    {
        get
        {
            foreach (var toast in activeToasts)
                if (toast.IsHovered())
                    return true;
            return false;
        }
    }

    protected Queue<Toast> pendingToasts = new Queue<Toast>();
    protected List<Toast> activeToasts = new List<Toast>();

    public ToastRegion Region { get; }

    protected abstract float GetToastXPosition(Toast toast);

    protected internal virtual void RecalculatePositions(params Toast[] except)
    {
        float screenHeight = Application.Current.Window.Height;

        // --- TOP stack ---
        float topCursor = UIConstants.TOAST_SPACING;
        for (int i = 0; i < activeToasts.Count; i++)
        {
            Toast toast = activeToasts[i];

            if (toast.StackSide != ToastStackSide.Top
                /*|| except.Contains(toast)*/
                || toast.IsClosing)
                continue;

            float accumulatedY = UIConstants.TOAST_SPACING;
            if (activeToasts.Count >= i + 1)
            {
                for (int j = i + 1; j < activeToasts.Count; j++)
                {
                    Toast previous = activeToasts[j];
                    if (previous.StackSide == ToastStackSide.Top)
                        accumulatedY += previous.Height + UIConstants.TOAST_SPACING;
                }
            }

            if (accumulatedY is not UIConstants.TOAST_SPACING)
                topCursor += toast.Height + UIConstants.TOAST_SPACING;

            if (toast.TargetPosition.Y != accumulatedY)
            {
                toast.TargetPosition = new Vector2(
                    GetToastXPosition(toast),
                    accumulatedY);
                toast.AnimationElapsed = 0;
            }
        }

        // --- BOTTOM stack ---
        float bottomCursor = screenHeight;
        for (int i = activeToasts.Count - 1; i >= 0; i--)
        {
            Toast toast = activeToasts[i];

            if (toast.StackSide != ToastStackSide.Bottom)
                continue;

            bottomCursor -= toast.Height + UIConstants.TOAST_SPACING;
            if (/*except.Contains(toast) || */toast.IsClosing)
                continue;

            if (bottomCursor < topCursor)
                bottomCursor = topCursor;

            if (bottomCursor != toast.TargetPosition.Y)
            {
                toast.TargetPosition = new Vector2(
                    GetToastXPosition(toast),
                    bottomCursor);
                toast.AnimationElapsed = 0f;
            }
        }
    }

    protected abstract Rectangle GetInitialDialogPosition(ToastStackSide side, Toast toast, float y);
    protected abstract Vector2 GetEntryPosition(ToastStackSide side, Toast toast, float y);
    protected internal abstract Rectangle GetExitPositionAndScale(Toast toast);

    private void PositionToast(Toast toast, ToastStackSide side)
    {
        float startY = side switch
        {
            ToastStackSide.Top => UIConstants.TOAST_SPACING,
            ToastStackSide.Bottom => Application.Current.Window.Height - toast.Height - UIConstants.TOAST_SPACING,
            _ => throw new InvalidEnumArgumentException(nameof(side), (int)side, typeof(ToastStackSide))
        };

        toast.CurrentPosition = GetInitialDialogPosition(side, toast, startY);

        toast.CurrentScale = toast.CurrentPosition.Size;

        toast.TargetPosition = GetEntryPosition(side, toast, startY);
        toast.TargetSize = new(Toasts.TOAST_WIDTH, toast.Height);
        toast.AnimationElapsed = 0f;

        RecalculatePositions(toast);
    }

    protected internal virtual void Update()
    {
        int queueCount = pendingToasts.Count;

        for (int i = 0; i < queueCount; i++)
        {
            Toast next = pendingToasts.Dequeue();

            // Calculate occupied space for each stack
            float topOccupied = activeToasts
                .Where(t => t.StackSide == ToastStackSide.Top)
                .Sum(t => t.Height + UIConstants.TOAST_SPACING);

            float bottomOccupied = activeToasts
                .Where(t => t.StackSide == ToastStackSide.Bottom)
                .Sum(t => t.Height + UIConstants.TOAST_SPACING);

            float totalOccupied = topOccupied + bottomOccupied;

            // Check if the new toast fits
            if (next.Height + totalOccupied > Application.Current.Window.Height)
            {
                // Not enough space, put it back in the queue
                pendingToasts.Enqueue(next);
                continue;
            }

            // Add the toast and position it according to its stack side
            activeToasts.Add(next);
            PositionToast(next, next.StackSide);
        }


        for (int i = 0; i < activeToasts.Count; i++)
        {
            Toast? toast = activeToasts[i];

            float time = toast.IsClosing ? toast.Style.AnimateOutDuration
                                         : toast.Style.AnimateInDuration;
            float tNormalized = Math.Clamp(
            toast.AnimationElapsed / time, 0f, 1f);

            // Check if closing
            if (toast.IsClosing)
            {
                toast.HandleMovement();
                if (tNormalized >= 1f)
                {
                    activeToasts.Remove(toast);
                    toast.ToastManager = null;
                    Toast? continuationToast = toast.GetContinueWithToast();
                    if (continuationToast != null)
                        EnqueueToast(continuationToast);
                }
                continue;
            }

            toast.UpdateContainer();
        }
    }

    internal void Draw()
    {
        for (int i = 0; i < activeToasts.Count; i++)
        {
            Toast? toast = activeToasts[i];
            toast.Draw();
        }
    }

    internal void RemoveImmediately(Toast toast)
    {
        activeToasts.Remove(toast);
        RecalculatePositions();
        toast.ToastManager = null;
    }

    internal void EnqueueToast(Toast toast)
    {
        pendingToasts.Enqueue(toast);
        toast.ToastManager = this;
    }

    internal void AddImmediately(Toast newToast)
    {
        activeToasts.Add(newToast);
        newToast.ToastManager = this;
        RecalculatePositions();
    }
}