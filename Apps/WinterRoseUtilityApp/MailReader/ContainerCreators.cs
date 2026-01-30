using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRoseUtilityApp.MailReader.Models;
using WinterRoseUtilityApp.MailReader.Readers;

namespace WinterRoseUtilityApp.MailReader;

internal class ContainerCreators
{
    public static UIWindow MailPreferencesWindow()
    {
        AssetHeader header = Assets.GetHeader(nameof(MailPreferences));
        MailPreferences prefs = MailPreferences.LoadAsset(header);

        UIWindow window = new UIWindow("Mail Preferences", 600, 700);

        // --- Check interval ---
        UIText intervalInfo = new UIText(
            "Mail check interval (changes are saved immediately)",
            UIFontSizePreset.Text
        );
        window.AddContent(intervalInfo);

        UIValueSlider<int> intervalSlider = new UIValueSlider<int>
        {
            Label = "Check interval (minutes)",
            MinValue = 5,
            MaxValue = 180,
            Step = 5,
            SnapToStep = true
        };

        intervalSlider.SetValue(prefs.CheckIntervalMinutes, false);
        intervalSlider.OnValueChangedBasic.Subscribe(Invocation.Create<int>(v =>
        {
            prefs.CheckIntervalMinutes = v;
            MailPreferences.SaveAsset(header, prefs);
        }));

        window.AddContent(intervalSlider);

        // --- Sender lists ---
        UIColumns lists = new UIColumns();
        window.AddContent(lists);

        // IMPORTANT SENDERS
        lists.AddToColumn(0, BuildSenderEditor(
            "Important Senders",
            prefs.ImportantSenders,
            header,
            prefs
        ));

        // IGNORED SENDERS
        lists.AddToColumn(1, BuildSenderEditor(
            "Ignored Senders",
            prefs.IgnoredSenders,
            header,
            prefs
        ));

        return window;

        static UIContent BuildSenderEditor(
            string title,
            HashSet<string> backingSet,
            AssetHeader header,
            MailPreferences prefs
        )
        {
            UIColumns root = new UIColumns();

            root.AddToColumn(0, new UIText(title, UIFontSizePreset.Title));

            UIColumns listColumn = new UIColumns();
            root.AddToColumn(0, listColumn);

            UITextInput input = new UITextInput
            {
                Placeholder = "example@gmail.com",
                AllowMultiline = false,
                MinLines = 1
            };

            root.AddToColumn(0, input);

            UIButton addButton = new UIButton("Add", (c, b) =>
            {
                string value = input.Text?.Trim();
                if (string.IsNullOrWhiteSpace(value)) return;

                if (backingSet.Add(value))
                {
                    MailPreferences.SaveAsset(header, prefs);
                    input.Text = string.Empty;
                    Rebuild();
                }
            });

            root.AddToColumn(0, addButton);

            void Rebuild()
            {
                listColumn.ClearColumn(0);

                if (backingSet.Count == 0)
                {
                    listColumn.AddToColumn(0, new UIText("— none —", UIFontSizePreset.Subtext));
                    return;
                }

                foreach (string sender in backingSet.OrderBy(s => s))
                {
                    UIColumns row = new UIColumns();

                    row.AddToColumn(0, new UIText(sender, UIFontSizePreset.Text));

                    row.AddToColumn(1, new UIButton("Remove", (c, b) =>
                    {
                        if (backingSet.Remove(sender))
                        {
                            MailPreferences.SaveAsset(header, prefs);
                            Rebuild();
                        }
                    }));

                    listColumn.AddToColumn(0, row);
                }
            }

            Rebuild();
            return root;
        }
    }

    public static Dialog AddEmailDialog(MailWatcher watcher, string reason = null)
    {
        Dialog d = new Dialog("Add Email Account", DialogPlacement.TopSmall, DialogPriority.Normal);
        UIColumns root = new UIColumns();
        d.AddContent(root);

        if (!string.IsNullOrWhiteSpace(reason))
        {
            UIText info = new UIText("\\c[yellow]" + reason);
            root.AddToColumn(0, info);
        }

        UIDropdown<string> emailType = new();
        emailType.AddOption("Outlook");
        emailType.AddOption("Gmail");
        emailType.AddOption("ProtonMail");

        UIColumns buttonCols = new UIColumns();
        buttonCols.AddToColumn(0, new UIButton("Cancel", (c, b) => c.Close()));
        buttonCols.AddToColumn(1, new UIButton("Connect", async (c, b) =>
        {
            if (emailType.SelectedItem is null)
            {
                Toasts.Error("You need to select an email type!");
                return;
            }

            Toasts.Info("Opening browser for authentication...");

            try
            {
                bool success = await EmailAuthManager.TryLoginAsync(emailType.SelectedItem);

                if (!success)
                {
                    Toasts.Error("Login failed. Check your account or network and try again.");
                    return;
                }

                Toasts.Success($"{emailType.SelectedItem} account connected!");
                c.Close();
            }
            catch (Exception ex)
            {
                Toasts.Error("Authentication failed: " + ex.Message);
            }
        }));
        root.AddToColumn(0, buttonCols);
        root.AddToColumn(0, emailType);

        return d;
    }

    public static UIWindow EmailAccountsWindow(MailWatcher watcher)
    {
        UIWindow window = new UIWindow("Email Accounts", 600, 400);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        var accounts = EmailAuthManager.GetSavedAccounts();

        // Column 0: Existing accounts
        UIColumns accountCols = new();
        rootCols.AddToColumn(0, accountCols);

        UIButton addBtn = new UIButton("Add Email Account", (c, b) =>
        {
            var dialog = AddEmailDialog(watcher);
            dialog.Show();
        });
        accountCols.AddToColumn(0, addBtn);
        accountCols.AddToColumn(0, new UISpacer());

        if (accounts.Count == 0)
        {
            UIText noAccounts = new UIText("No accounts added yet.");
            accountCols.AddToColumn(0, noAccounts);
        }
        else
        {
            foreach (var account in accounts)
            {
                accountCols.AddToColumn(0, new UIButton(
                    $"{account.Provider}: {account.Address}",
                    (c, b) =>
                    {
                        CreateMailWindow(account).Show();
                    }));
            }
        }

        // Column 1: Add new account button



        rootCols.AddToColumn(1, new UIText("Time until next check:", UIFontSizePreset.Subtitle));

        UICircleProgress checkTimer = new(0, (bar, currentVal) =>
        {
            if (watcher.MonitorCount is 0)
            {
                bar.Text = "Add an email to start!";
                return 0;
            }

            if (watcher.LastCheck == DateTime.MinValue)
            {
                bar.Text = "A scan is busy...";
                return -1;
            }

            TimeSpan timeSinceLastCheck = DateTime.UtcNow - watcher.LastCheck;
            double intervalSeconds = watcher.checkInterval.TotalSeconds;

            double secondsRemaining = intervalSeconds - timeSinceLastCheck.TotalSeconds;
            if (secondsRemaining < 0) secondsRemaining = 0;

            TimeSpan timeLeft = TimeSpan.FromSeconds(secondsRemaining);

            // Text now shows time remaining
            bar.Text = timeLeft.Hours > 0
                ? string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds, timeLeft.Milliseconds / 100)
                : string.Format("{0:D2}:{1:D2}.{2}", timeLeft.Minutes, timeLeft.Seconds, timeLeft.Milliseconds / 100);

            return (float)(secondsRemaining / intervalSeconds); // progress counts down
        })
        {
            Text = "--:--",
            AlwaysShowText = true,
            DontShowProgressPercent = true
        };

        rootCols.AddToColumn(1, checkTimer);

        return window;
    }

    public static UIWindow CreateMailWindow(EmailAccount account)
    {
        UIWindow window = new UIWindow($"{account.Provider}: {account.Address}", 800, 600);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        // Left column for folders
        UIColumns leftCol = new();
        rootCols.AddToColumn(0, leftCol);

        UITreeNode foldersRoot = new("Folders");
        leftCol.AddToColumn(0, foldersRoot);

        // Right column for emails
        UIColumns rightEmailsCol = new();
        rootCols.AddToColumn(1, rightEmailsCol);

        // --- Fetch folders from Graph API ---
        Task.Run(async () =>
        {
            var folders = await OutlookMailReader.FetchFolders(account); // returns List<MailFolder>
            foreach (var folder in folders)
            {
                UITreeNode folderNode = new(folder.DisplayName);

                folderNode.ClickInvocation.Subscribe(async node =>
                {
                    rightEmailsCol.Clear();
                    List<MailMessage> emails = await OutlookMailReader.FetchEmails(account, folder);
                    foreach (var mail in emails)
                    {
                        string btnText = $"{mail.From}:\n{mail.Subject}";
                        UIButton emailBtn = new(btnText, (c, b) =>
                        {
                            CreateMailDetailsWindow(mail).Show();
                        });
                        rightEmailsCol.AddToColumn(0, emailBtn);
                        if (!mail.IsRead)
                        {
                            emailBtn.Style.ButtonBackground.Override = Color.DarkPurple;
                            emailBtn.Style.ButtonBackground.Override = Color.Pink;
                        }
                    }
                });

                foldersRoot.AddChild(folderNode);
            }
        });

        return window;
    }

    public static UIWindow CreateMailDetailsWindow(MailMessage email)
    {
        UIWindow window = new UIWindow($"{email.Subject ?? "Mail"}", 900, 700);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        // Header / basic info
        UIText subjectText = new UIText(email.Subject ?? "(no subject)", UIFontSizePreset.Title);
        rootCols.AddToColumn(0, subjectText);

        string fromTextValue = email.From is null ? "Unknown" : email.From.ToString();
        UIText fromText = new UIText($"From: {fromTextValue}");
        rootCols.AddToColumn(0, fromText);

        UIText receivedText = new UIText($"Received: {email.ReceivedDateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        rootCols.AddToColumn(0, receivedText);

        // Read/unread toggle
        UIButton markBtn = null;
        string MakeMarkLabel(bool isRead) => isRead ? "Mark as Unread" : "Mark as Read";

        markBtn = new UIButton(MakeMarkLabel(email.IsRead), (c, b) =>
        {
            MailWatcher.Current.MarkAsRead(email);
            markBtn.Text = MakeMarkLabel(email.IsRead);
            Toasts.Success(email.IsRead ? "Marked as read" : "Marked as unread", ToastRegion.Center, ToastStackSide.Top);
        });

        rootCols.AddToColumn(0, markBtn);

        // Small action row (Reply / Forward placeholders)
        UIColumns actionCols = new();
        actionCols.AddToColumn(0, new UIButton("Reply", (c, b) =>
        {
            // TODO: open reply composer
            Toasts.Info("Reply not implemented yet");
        }));
        actionCols.AddToColumn(1, new UIButton("Forward", (c, b) =>
        {
            // TODO: open forward composer
            Toasts.Info("Forward not implemented yet");
        }));
        rootCols.AddToColumn(0, actionCols);


        string htmlContent = email.Body?.Content;
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            htmlContent = email.BodyPreview ?? "\\c[FFFF00]No email content!";
            rootCols.AddToColumn(1, new UIText(htmlContent, UIFontSizePreset.Subtitle));
        }
        else
            rootCols.AddToColumn(1, new HTMLContent(htmlContent));

        return window;
    }
}
