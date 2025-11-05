using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRoseUtilityApp.MailReader.Models;

namespace WinterRoseUtilityApp.MailReader;
internal class ContainerCreators
{
    public static Dialog AddEmailDialog(string reason = null)
    {
        Dialog d = new Dialog("Add Email Account", DialogPlacement.CenterSmall, DialogPriority.Normal);
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

    public static UIWindow EmailAccountsWindow()
    {
        UIWindow window = new UIWindow("Email Accounts", 600, 400);
        UIColumns rootCols = new();
        window.AddContent(rootCols);

        var accounts = EmailAuthManager.GetSavedAccounts();

        // Column 0: Existing accounts
        UIColumns accountCols = new();
        rootCols.AddToColumn(0, accountCols);

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
        UIButton addBtn = new UIButton("Add Email Account", (c, b) =>
        {
            var dialog = AddEmailDialog();
            dialog.Show();
        });

        rootCols.AddToColumn(1, addBtn);

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
            var folders = await MailReader.FetchFolders(account); // returns List<MailFolder>
            foreach (var folder in folders)
            {
                UITreeNode folderNode = new(folder.DisplayName);

                folderNode.ClickInvocation.Subscribe(async node =>
                {
                    rightEmailsCol.Clear();
                    List<MailMessage> emails = await MailReader.FetchEmails(account, folder.Id);
                    foreach (var mail in emails)
                    {
                        string btnText = $"{mail.From}: {mail.Subject}";
                        UIButton emailBtn = new(btnText, (c, b) =>
                        {
                            Toasts.Info(mail.BodyPreview[..Math.Min(300, mail.BodyPreview.Length)]);
                        });
                        rightEmailsCol.AddToColumn(0, emailBtn);
                    }
                });

                foldersRoot.AddChild(folderNode);
            }
        });

        return window;
    }
}