using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRoseUtilityApp.MailReader.Models;
using WinterRoseUtilityApp.MailReader.Readers;

namespace WinterRoseUtilityApp.MailReader;

internal class ContainerCreators
{
    public static UIWindow CreateMailWindow(EmailAccount account)
    {
        UIWindow window = new UIWindow($"{account.Provider}: {account.Address}", 800, 600);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        UITreeNode foldersRoot = new("Folders");
        rootCols.AddToColumn(0, foldersRoot);

        Task.Run(async () =>
        {
            List<MailFolder> folders = await OutlookMailReader.FetchFolders(account);
            foreach (var folder in folders)
            {
                UITreeNode folderNode = new(folder.DisplayName);

                folderNode.ClickInvocation.Subscribe(node =>
                {
                    rootCols.ClearColumn(1);
                    ForgeWardenEngine.Current.GlobalThreadLoom
                        .InvokeOn(ForgeWardenEngine.ENGINE_POOL_NAME, async void () =>
                        {
                            List<MailMessage> emails = await OutlookMailReader.FetchEmails(account, folder);

                            rootCols.ClearColumn(1);

                            if (emails.Count == 0)
                            {
                                rootCols.AddToColumn(1, new UIText("This folder has no mail!"));
                                return;
                            }

                            // Search bar (placeholder)
                            UITextInput searchInput = new UITextInput()
                            {
                                Placeholder = "Search (not implemented)"
                            };
                            rootCols.AddToColumn(1, searchInput);

                            const int PAGE_SIZE = 100;
                            int currentPage = 0;
                            Action renderPage = null!;
                            renderPage = async () =>
                            {
                                rootCols.ClearColumn(1);

                                rootCols.AddToColumn(1, searchInput);

                                int start = currentPage * PAGE_SIZE;
                                var pageItems = emails.Skip(start).Take(PAGE_SIZE);

                                foreach (var mail in pageItems)
                                {
                                    string btnText = $"{mail.From}:\n{mail.Subject}";
                                    UIButton emailBtn = new(btnText, (c, b) =>
                                    {
                                        CreateMailDetailsWindow(mail).ShowMaximized();
                                    });

                                    rootCols.AddToColumn(1, emailBtn);

                                    if (!mail.IsRead)
                                    {
                                        emailBtn.Style.ButtonBackground.Override = Color.Pink;
                                    }
                                    await Task.Delay(10);
                                }

                                UIColumns navRow = new UIColumns();

                                UIButton prevBtn = new UIButton("Previous", (c, b) =>
                                {
                                    if (currentPage > 0)
                                    {
                                        currentPage--;
                                        renderPage();
                                    }
                                });

                                UIButton nextBtn = new UIButton("Next", (c, b) =>
                                {
                                    if ((currentPage + 1) * PAGE_SIZE < emails.Count)
                                    {
                                        currentPage++;
                                        renderPage();
                                    }
                                });

                                navRow.AddToColumn(0, prevBtn);
                                navRow.AddToColumn(1, nextBtn);

                                rootCols.AddToColumn(1, new UISpacer());
                                rootCols.AddToColumn(1, navRow);
                            };

                            renderPage();
                        });
                    rootCols.AddToColumn(1, new UIText("Loading emails \\e[]"));
                });

                foldersRoot.AddChild(folderNode);
                await Task.Delay(100);
            }
        });

        return window;
    }

    public static UIWindow MailPreferencesWindow()
    {
        AssetHeader header = Assets.GetHeader(nameof(MailPreferences));
        MailPreferences prefs = MailPreferences.LoadAsset(header);

        UIWindow window = new UIWindow("Mail Preferences", 1200, 900);
        window.Style.ShowVerticalScrollBar = true;
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
            MailWatcher.Current.UpdateCheckInterval(TimeSpan.FromMinutes(v));
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
            root.ColumnScrollEnabled[0] = false; // root handles all scrolling
            root.ColumnScrollEnabled[1] = false; // root handles all scrolling

            // --- Title ---
            root.AddToColumn(0, new UIText(title, UIFontSizePreset.Title));

            // --- Input + Add button ---
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

            // --- List of senders ---
            void Rebuild()
            {
                // Clear everything after title + input + button
                // Remove all but first 3 contents
                while (root.ColumnsContents[0].Count > 3)
                    root.ColumnsContents[0].RemoveAt(3);

                if (backingSet.Count == 0)
                {
                    root.AddToColumn(0, new UIText("No mails listed!", UIFontSizePreset.Text));
                    return;
                }

                foreach (string sender in backingSet.OrderBy(s => s))
                {
                    // Create a single horizontal row
                    UIColumns row = new UIColumns();
                    row.ColumnScrollEnabled[0] = false;
                    row.ColumnScrollEnabled[1] = false;

                    row.AddToColumn(0, new UIText(sender, UIFontSizePreset.Text));
                    row.AddToColumn(1, new UIButton("Remove", (c, b) =>
                    {
                        if (backingSet.Remove(sender))
                        {
                            MailPreferences.SaveAsset(header, prefs);
                            Rebuild();
                        }
                    }));

                    root.AddToColumn(0, row);
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
        UIWindow window = new UIWindow("Email Accounts", 600, 550);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        var accounts = EmailAuthManager.GetSavedAccounts();

        UIButton addBtn = new UIButton("Add Email Account", (c, b) =>
        {
            var dialog = AddEmailDialog(watcher);
            dialog.Show();
        });
        rootCols.AddToColumn(0, addBtn);

        UIButton configBtn = new UIButton("Settings", (c, b) =>
        {
            MailPreferencesWindow().Show();
        });
        rootCols.AddToColumn(0, configBtn);

        rootCols.AddToColumn(0, new UIButton("Unread Mail", (c, b) =>
        {
            CreateUnreadMailWindow().Show();
        }));
        rootCols.AddToColumn(0, new UISpacer());

        if (accounts.Count == 0)
        {
            UIText noAccounts = new UIText("No accounts added yet.");
            rootCols.AddToColumn(0, noAccounts);
        }
        else
        {
            foreach (var account in accounts)
            {
                rootCols.AddToColumn(0, new UIButton(
                    $"{account.Provider}: {account.Address}",
                    (c, b) =>
                    {
                        CreateMailWindow(account).Show();
                    }));
            }
        }
        UICircleProgress checkTimerProgress = null!;
        UIText timerHeadText = new UIText("Time until next check:", UIFontSizePreset.Subtitle);

        var sub = MailWatcher.Current.OnFetchProgress.Subscribe(progress =>
        {
            checkTimerProgress.ProgressValue = progress.OverallProgress;
            timerHeadText.Text = $"{progress.Stage}\n{progress.ProcessedMessages}/{progress.TotalMessages}";
        });

        rootCols.AddToColumn(1, timerHeadText);
        checkTimerProgress = new(0, (bar, currentVal) =>
        {
            if (bar.Owner.IsClosing)
                sub.Unsubscribe();

            if (watcher.MonitorCount is 0)
            {
                bar.Text = "Add an email to start!";
                return 0;
            }

            if (watcher.LastCheck == DateTime.MinValue)
            {
                bar.Text = "Currently scanning \\e[]";
                return -2;
            }

            timerHeadText.Text = "Time until next check:";

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

        rootCols.AddToColumn(1, checkTimerProgress);

        if (accounts.Count != 0)
        {
            rootCols.AddToColumn(1, new UISpacer());
            rootCols.AddToColumn(1, new UIButton("Scan now", (c, b) =>
            {
                watcher.TriggerImmediateCheck();
            }));
        }

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

        UIColumns actionCols = new();
        actionCols.AddToColumn(0, new UIButton("Reply", (c, b) =>
        {
            Toasts.Info("Reply not implemented yet");
        }));
        actionCols.AddToColumn(1, new UIButton("Forward", (c, b) =>
        {
            Toasts.Info("Forward not implemented yet");
        }));
        rootCols.AddToColumn(0, actionCols);

        var body = email.Body;
        if (!body.Ready)
        {
            email.OnBodyReady = (body) =>
            {
                rootCols.ClearColumn(1);
                if (string.IsNullOrWhiteSpace(body.Content))
                    rootCols.AddToColumn(1, new UIText("\\c[FFFF00]No email content!", UIFontSizePreset.Subtitle));
                else
                    rootCols.AddToColumn(1, new HTMLContent(email.Body.Content));
                email.OnBodyReady = null!;
            };
        }
        string htmlContent = body.Ready ? body.Content : "Loading mail \\e[]";
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            htmlContent = email.BodyPreview ?? "\\c[FFFF00]No email content!";
            rootCols.AddToColumn(1, new UIText(htmlContent, UIFontSizePreset.Subtitle));
        }
        else if (body.Ready)
            rootCols.AddToColumn(1, new HTMLContent(htmlContent));
        else
            rootCols.AddToColumn(1, new UIText(htmlContent));

        return window;
    }

    private static UIWindow CreateUnreadMailWindow()
    {
        UIWindow window = new UIWindow("Unread Mail", 1000, 700);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        rootCols.AddToColumn(0, new UIText("Unread messages across all accounts", UIFontSizePreset.Title));

        UIColumns topRow = new UIColumns();
        rootCols.AddToColumn(0, topRow);

        // backing index of unread messages (monitor + folder + message)
        var unreadIndex = new List<(IMailMonitor monitor, MailFolder folder, MailMessage message)>();
        object unreadLock = new();

        UIColumns listColumn = new UIColumns();
        rootCols.AddToColumn(0, listColumn);

        var prefsHeader = Assets.GetHeader(nameof(MailPreferences));
        MailPreferences prefs = MailPreferences.LoadAsset(prefsHeader);

        UIButton markAllBtn = new UIButton("Mark all as read", (c, b) =>
        {
            // Confirmation dialog
            Dialog confirm = new Dialog("Confirm mark all as read", DialogPlacement.CenterSmall, DialogPriority.Normal);
            UIColumns col = new();
            confirm.AddContent(col);

            col.AddToColumn(0, new UIText("This will mark all unread messages shown here as read."));

            UIButton sureBtn = new UIButton("Are you sure?", (cc, bb) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        List<(IMailMonitor monitor, MailFolder folder, MailMessage message)> toMark;
                        lock (unreadLock)
                        {
                            toMark = unreadIndex.ToList();
                        }

                        foreach (var item in toMark)
                        {
                            item.monitor.MarkAsRead(item.folder, item.message);
                            item.message.IsRead = true;
                        }

                        lock (unreadLock)
                        {
                            unreadIndex.Clear();
                        }

                        Rebuild();
                        Toasts.Success("All messages marked as read");
                    }
                    catch (Exception ex)
                    {
                        Toasts.Error("Failed to mark all as read: " + ex.Message);
                    }
                });

                confirm.Close();
            });

            col.AddToColumn(0, sureBtn);
            confirm.Show();
        });

        topRow.AddToColumn(1, markAllBtn);

        // Rebuild closure
        void Rebuild()
        {
            listColumn.ClearColumn(0);

            List<(IMailMonitor monitor, MailFolder folder, MailMessage message)> snapshot;
            lock (unreadLock)
            {
                snapshot = unreadIndex.ToList();
            }

            if (snapshot.Count == 0)
            {
                listColumn.AddToColumn(0, new UIText("A read inbox is a ready mind!", UIFontSizePreset.Title));
                return;
            }

            foreach (var item in snapshot.OrderByDescending(x => x.message.ReceivedDateTime))
            {
                var mail = item.message;
                string subject = string.IsNullOrWhiteSpace(mail.Subject) ? "(no subject)" : mail.Subject;
                string btnText = $"{mail.From}:\n{subject} ({item.monitor.Account.Address})";
                UIButton emailBtn = new(btnText, (c, b) =>
                {
                    CreateMailDetailsWindow(mail).ShowMaximized();
                });

                emailBtn.OnTooltipConfigure = Invocation.Create((Tooltip tip) =>
                {
                    tip.SizeConstraints.MaxSize = tip.SizeConstraints.MaxSize with { X = 700 };
                    tip.AddText($"{mail.From} — {mail.ReceivedDateTime.ToLocalTime():yyyy-MM-dd HH:mm}\nAccount: {item.monitor.Account.Address}");

                    UIColumns tipCols = new();

                    UIButton importantBtn = new("Add to Important", (c, b) =>
                    {
                        string sender = mail.From?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(sender) && prefs.ImportantSenders.Add(sender))
                        {
                            MailPreferences.SaveAsset(prefsHeader, prefs);
                            Toasts.Success("Added to Important Senders");
                        }
                    });

                    UIButton ignoreBtn = new("Add to Ignored (and mark read)", (c, b) =>
                    {
                        string sender = mail.From?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(sender) && prefs.IgnoredSenders.Add(sender))
                        {
                            MailPreferences.SaveAsset(prefsHeader, prefs);
                            Toasts.Success("Added to Ignored Senders");
                        }

                        item.monitor.MarkAsRead(item.folder, mail);
                        mail.IsRead = true;

                        lock (unreadLock)
                        {
                            unreadIndex.RemoveAll(x => x.message.Id == mail.Id);
                        }
                        Rebuild();
                        tip.Close();
                    });

                    UIButton markBtn = new("Mark as read", (c, b) =>
                    {
                        if (!mail.IsRead)
                        {
                            item.monitor.MarkAsRead(item.folder, mail);
                            mail.IsRead = true;

                            lock (unreadLock)
                            {
                                unreadIndex.RemoveAll(x => x.message.Id == mail.Id);
                            }
                            Rebuild();
                            Toasts.Success("Marked as read");
                            tip.Close();
                        }
                    });

                    tipCols.AddToColumn(0, importantBtn);
                    tipCols.AddToColumn(1, ignoreBtn);
                    tipCols.AddToColumn(2, markBtn);

                    tip.AddContent(tipCols);
                });

                listColumn.AddToColumn(0, emailBtn);
            }
        }

        // initial population: gather unread messages from all monitors
        Task.Run(() =>
        {
            try
            {
                foreach (var monitor in MailWatcher.Current.Monitors)
                {
                    var cached = monitor.GetCached();
                    foreach (var kv in cached)
                    {
                        var folder = kv.Key;
                        foreach (var mail in kv.Value)
                        {
                            if (!mail.IsRead)
                            {
                                lock (unreadLock)
                                {
                                    if (!unreadIndex.Any(x => x.message.Id == mail.Id))
                                        unreadIndex.Add((monitor, folder, mail));
                                }
                            }
                        }
                    }
                }

                Rebuild();
            }
            catch (Exception ex)
            {
                Toasts.Error("Failed loading unread mail: " + ex.Message);
            }
        });

        return window;
    }


}
