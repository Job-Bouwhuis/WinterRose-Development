using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Utility;
using WinterRoseUtilityApp.MailReader;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.Todo;

// Simple typed context for todos
public interface ITodoContext
{
    string DisplayLabel { get; }
    void Open();
}

public sealed class EmailContext : ITodoContext
{
    public string AccountProvider { get; init; }
    public string AccountAddress { get; init; }
    public string MessageId { get; init; }

    public string DisplayLabel => $"Email: {AccountProvider} - {AccountAddress}";

    public void Open()
    {
        Toasts.Info("Not implemented");
    }
}

public sealed class FileContext : ITodoContext
{
    public string FilePath { get; init; }
    public string DisplayLabel => $"File: {System.IO.Path.GetFileName(FilePath)}";

    public void Open()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || !System.IO.File.Exists(FilePath))
        {
            Toasts.Info("File not available.");
            return;
        }

        try
        {
            // best-effort open; if you have a file explorer / editor integration, call it here
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = FilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Toasts.Error("Failed to open file: " + ex.Message);
        }
    }
}

public sealed class WebsiteContext : ITodoContext
{
    public string Url { get; init; }
    public string DisplayLabel => $"Website: {Url}";

    public void Open()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = Url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Toasts.Error("Failed to open website: " + ex.Message);
        }
    }
}

public enum TodoState
{
    Active,
    Snoozed,
    Completed,
    Cancelled
}

public sealed class TodoItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public TodoState State { get; private set; } = TodoState.Active;
    public List<ITodoContext> Contexts { get; } = new List<ITodoContext>();

    // Mark as complete
    public void Complete()
    {
        State = TodoState.Completed;
    }

    public void Cancel()
    {
        State = TodoState.Cancelled;
    }

    public void Snooze(TimeSpan duration)
    {
        State = TodoState.Snoozed;
        if (DueDate is null) DueDate = DateTime.UtcNow + duration;
        else DueDate = DueDate.Value + duration;
    }
}

internal static class TodoManager
{
    private static readonly object SYNC_OBJECT = new();
    private static readonly List<TodoItem> todos = new();

    public static IReadOnlyList<TodoItem> Todos
    {
        get
        {
            lock (SYNC_OBJECT)
            {
                return todos.ToList().AsReadOnly();
            }
        }
    }

    public static MulticastVoidInvocation OnTodosChanged { get; } = new();

    public static void Add(TodoItem item)
    {
        lock (SYNC_OBJECT)
        {
            todos.Add(item);
            // TODO: persist
        }
        OnTodosChanged.Invoke();
    }

    public static bool Remove(TodoItem item)
    {
        lock (SYNC_OBJECT)
        {
            var removed = todos.Remove(item);
            if (removed)
            {
                // TODO: persist
                OnTodosChanged.Invoke();
            }
            return removed;
        }
    }

    public static void MarkDone(TodoItem item)
    {
        lock (SYNC_OBJECT)
        {
            var found = todos.FirstOrDefault(t => t.Id == item.Id);
            if (found is null) return;
            found.Complete();
        }
        OnTodosChanged.Invoke();
    }
}

// Subsystem entry
internal class TodoEntry : SubSystem
{
    private const string TODOS_HOTKEY = "OpenTodosHotkey";

    public TodoEntry() : base("Todos", "Centralized todo subsystem", new Version(1, 0, 0))
    {
        GlobalHotkey.RegisterHotkey(TODOS_HOTKEY, true, HotkeyScancode.LeftAlt, HotkeyScancode.T); // Alt + T
    }

    public override void Init()
    {
        base.Init();
        // nothing heavy yet
    }

    public override void Update()
    {
        base.Update();
        if (GlobalHotkey.IsTriggered(TODOS_HOTKEY))
        {
            ContainerCreators.TodosWindow().Show();
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}

internal static partial class ContainerCreators
{
    // Simple ordering options for the first pass
    private static readonly string[] ORDER_OPTIONS = new[] { "Newest", "Oldest", "DueSoon", "ByContext" };

    public static UIWindow CreateTodoWindow()
    {
        UIWindow window = new UIWindow("Create Todo", 600, 700);
        UIColumns cols = new UIColumns();
        window.AddContent(cols);

        // --- Title ---
        UITextInput titleInput = new UITextInput();
        titleInput.Placeholder = "Todo title...";
        titleInput.MinLines = 1;
        titleInput.AllowMultiline = false;
        cols.AddToColumn(0, titleInput);

        // --- Description ---
        UITextInput descriptionInput = new UITextInput();
        descriptionInput.Placeholder = "Description (optional)...";
        descriptionInput.MinLines = 4;
        descriptionInput.IsMultiline = true;
        cols.AddToColumn(0, descriptionInput);

        // --- Due date ---
        UIText dueLabel = new UIText("Due date", UIFontSizePreset.Subtitle);
        cols.AddToColumn(0, dueLabel);

        UIDateTimePicker duePicker = new UIDateTimePicker
        {
            Mode = UIDateTimePicker.PickerMode.DateTime,
            Use24Hour = true,
            Selected = DateTime.Now.AddHours(1)
        };
        cols.AddToColumn(0, duePicker);

        // --- Contexts ---
        UIText ctxLabel = new UIText("Context", UIFontSizePreset.Subtitle);
        cols.AddToColumn(0, ctxLabel);

        UIColumns contextList = new UIColumns();
        cols.AddToColumn(0, contextList);

        List<ITodoContext> contexts = new();

        void RefreshContextList()
        {
            contextList.ClearColumn(0);
            foreach (var ctx in contexts)
            {
                contextList.AddToColumn(0, new UIButton(ctx.DisplayLabel, (c, b) =>
                {
                    contexts.Remove(ctx);
                    RefreshContextList();
                }));
            }
        }

        // --- Add context buttons ---
        UIColumns addContextCols = new UIColumns();

        addContextCols.AddToColumn(0, new UIButton("Add Website", (c, b) =>
        {
            CreateAddWebsiteContextDialog(contexts, RefreshContextList).Show();
        }));

        addContextCols.AddToColumn(1, new UIButton("Add File", (c, b) =>
        {
            CreateAddFileContextDialog(contexts, RefreshContextList).Show();
        }));

        cols.AddToColumn(0, addContextCols);

        // --- Footer buttons ---
        UIColumns footer = new UIColumns();

        footer.AddToColumn(0, new UIButton("Cancel", (c, b) => c.Close()));

        footer.AddToColumn(1, new UIButton("Create", (c, b) =>
        {
            if (string.IsNullOrWhiteSpace(titleInput.Text))
            {
                Toasts.Error("Todo must have a title");
                titleInput.Focus();
                return;
            }

            TodoItem todo = new TodoItem
            {
                Title = titleInput.Text.Trim(),
                Description = descriptionInput.Text,
                DueDate = duePicker.Selected
            };

            foreach (var ctx in contexts)
                todo.Contexts.Add(ctx);

            TodoManager.Add(todo);
            Toasts.Success("Todo created");
            c.Close();
        }));

        cols.AddToColumn(0, footer);

        return window;
    }

    private static Dialog CreateAddFileContextDialog(
    List<ITodoContext> target,
    Action onChanged)
    {
        Dialog d = new Dialog("Add File", DialogPlacement.CenterSmall, DialogPriority.Normal);

        UIColumns cols = new UIColumns();
        d.AddContent(cols);

        UITextInput pathInput = new UITextInput();
        pathInput.Placeholder = "Full file path...";
        cols.AddToColumn(0, pathInput);

        UIColumns buttons = new UIColumns();
        buttons.AddToColumn(0, new UIButton("Cancel", (c, b) => c.Close()));
        buttons.AddToColumn(1, new UIButton("Add", (c, b) =>
        {
            if (string.IsNullOrWhiteSpace(pathInput.Text))
            {
                Toasts.Error("File path cannot be empty");
                return;
            }

            target.Add(new FileContext
            {
                FilePath = pathInput.Text.Trim()
            });

            onChanged();
            c.Close();
        }));

        cols.AddToColumn(0, buttons);
        return d;
    }


    private static Dialog CreateAddWebsiteContextDialog(
    List<ITodoContext> target,
    Action onChanged)
    {
        Dialog d = new Dialog("Add Website", DialogPlacement.CenterSmall, DialogPriority.Normal);

        UIColumns cols = new UIColumns();
        d.AddContent(cols);

        UITextInput urlInput = new UITextInput();
        urlInput.Placeholder = "https://example.com";
        cols.AddToColumn(0, urlInput);

        UIColumns buttons = new UIColumns();
        buttons.AddToColumn(0, new UIButton("Cancel", (c, b) => c.Close()));
        buttons.AddToColumn(1, new UIButton("Add", (c, b) =>
        {
            if (string.IsNullOrWhiteSpace(urlInput.Text))
            {
                Toasts.Error("URL cannot be empty");
                return;
            }

            target.Add(new WebsiteContext
            {
                Url = urlInput.Text.Trim()
            });

            onChanged();
            c.Close();
        }));

        cols.AddToColumn(0, buttons);
        return d;
    }

    public static UIWindow TodosWindow()
    {
        UIWindow window = new UIWindow("Todos", 700, 700);

        // Top controls (search + order)
        UIColumns topCols = new UIColumns();
        topCols.ColumnCount = 3;
        window.AddContent(topCols);

        UITextInput search = new UITextInput();
        search.Placeholder = "Search todos..."; // not wired up yet
        search.MinLines = 1;
        search.AllowMultiline = false;
        topCols.AddToColumn(0, search);

        topCols.AddToColumn(1, new UIButton("New Todo", (c, b) =>
        {
            CreateTodoWindow().Show();
        }));

        UIDropdown<string> order = new UIDropdown<string>();
        foreach (var o in ORDER_OPTIONS) order.AddOption(o);
        order.SelectedIndex = 0;

        topCols.AddToColumn(1, order);

      

        UIColumns listCols = new UIColumns();
        window.AddContent(listCols);

        // Helper to rebuild list based on ordering
        void RebuildList()
        {
            listCols.ClearColumn(0);

            var todos = TodoManager.Todos.ToList();
            switch (order.SelectedItem)
            {
                case "Newest": todos = todos.OrderByDescending(t => t.CreatedDate).ToList(); break;
                case "Oldest": todos = todos.OrderBy(t => t.CreatedDate).ToList(); break;
                case "DueSoon": todos = todos.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ToList(); break;
                case "ByContext": todos = todos.OrderBy(t => t.Contexts.FirstOrDefault()?.DisplayLabel ?? string.Empty).ToList(); break;
            }

            if (todos.Count == 0)
            {
                listCols.AddToColumn(0, new UIText("No todos.", UIFontSizePreset.Title));
                return;
            }

            foreach (var todo in todos)
            {
                UITreeNode node = new UITreeNode(todo.Title);
                string subtitle = todo.Description ?? string.Empty;
                if (todo.DueDate.HasValue)
                    subtitle += "  (Due: " + todo.DueDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + ")";
                node.AddChild(new UIText(subtitle, UIFontSizePreset.Subtext));

                // Context buttons
                if (todo.Contexts.Any())
                {
                    UIColumns ctxCols = new UIColumns();
                    foreach (var ctx in todo.Contexts)
                    {
                        ctxCols.AddToColumn(0, new UIButton(ctx.DisplayLabel, (c, b) => ctx.Open()));
                    }
                    node.AddChild(ctxCols);
                }

                // Action row
                UIColumns actionCols = new UIColumns();
                actionCols.AddToColumn(0, new UIButton("Mark Done", (c, b) =>
                {
                    TodoManager.MarkDone(todo);
                    Toasts.Success($"Marked '{todo.Title}' as done");
                }));

                actionCols.AddToColumn(1, new UIButton("Open", (c, b) =>
                {
                    if (todo.Contexts.FirstOrDefault() is ITodoContext ctx)
                        ctx.Open();
                    else Toasts.Info("No context to open.");
                }));

                actionCols.AddToColumn(2, new UIButton("Delete", (c, b) =>
                {
                    new ConfirmationDialog($"Confirm delete '{todo.Title}'", ConfirmationDialog.Preset.YesNo, s =>
                    {
                        if (s is "Yes")
                        {
                            TodoManager.Remove(todo);
                            Toasts.Success("Todo deleted");
                        }
                    }).Show();
                }));

                node.AddChild(actionCols);

                listCols.AddToColumn(0, node);
            }
        }

        // subscribe to changes
        MulticastVoidInvocation.Subscription sub = TodoManager.OnTodosChanged.Subscribe(Invocation.Create(() => RebuildList()));

        // wire dropdown change
        order.OnSelected += (s, e) => RebuildList();

        // initial build
        RebuildList();

        window.OnClosing.Subscribe(Invocation.Create((UIWindow w) => sub.Dispose()));

        return window;
    }
}


