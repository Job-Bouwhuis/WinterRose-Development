using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRose.ForgeWarden.UserInterface
{
    public enum ExplorerMode
    {
        Browse, // select file or folder, caller determines which. Default "Select" action text.
        SelectFile, // only allow selecting files. "Select File" action text.
        SelectFolder, // only allow selecting folders. "Select Folder" action text.
        SaveFile // select a folder and enter a file name to save. "Save" action text.
    }
}

namespace WinterRose.ForgeWarden
{
    public static class Extensions
    {
        extension(IUIContainer container)
        {
            public void AddFileExplorer(ExplorerMode mode = ExplorerMode.Browse,
            string? startPath = null,
            Action<string>? onSelect = null)
            {
                container.AddContent(CreateFileExplorerRoot(container, mode, startPath, onSelect));
            }

            public void AddTextEditor(string filePath, bool asEmbed)
            {
                CreateAndAddTextEditor(container, filePath, asEmbed);
            }
        }

        private static UIColumns CreateFileExplorerRoot(
            IUIContainer container,
            ExplorerMode mode = ExplorerMode.Browse,
            string startPath = null,
            Action<string> onSelect = null)
        {
            string defaultPath;

            if (OperatingSystem.IsWindows())
                defaultPath = "C:\\";
            else
                defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string currentPath = !string.IsNullOrWhiteSpace(startPath) && Directory.Exists(startPath)
                ? startPath
                : defaultPath;

            UIColumns folderColumn = null!;
            UIColumns fileColumn = null!;

            string selectedPath = string.Empty;
            UIButton lastSelectedButton = null!;
            UITextInput saveNameInput = null!;

            UIColumns root = new UIColumns();
            root.ColumnScrollEnabled[0] = true;
            root.ColumnScrollEnabled[1] = true;

            // Top row: breadcrumb + controls
            UIColumns topRow = new UIColumns();
            root.AddToColumn(0, topRow);

            UIText breadcrumb = new UIText("");
            topRow.AddToColumn(0, breadcrumb);

            UIColumns topButtons = new UIColumns();
            topButtons.ColumnScrollEnabled[0] = false;
            topButtons.ColumnScrollEnabled[1] = false;
            topButtons.AddToColumn(0, new UIButton("Up", (c, b) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(currentPath)) return;
                    var parent = Directory.GetParent(currentPath);
                    if (parent != null) SetCurrentPath(parent.FullName);
                }
                catch (Exception ex) { Toasts.Error("Failed to go up: " + ex.Message); }
            }));
            topButtons.AddToColumn(1, new UIButton("Refresh", (c, b) => RefreshAll()));
            topButtons.AddToColumn(1, new UIButton("Search", (c, b) => Toasts.Info("Search mode not implemented")));
            topRow.AddToColumn(1, topButtons);

            // Middle: folder tree (left) + listing (right)
            UIColumns middle = new UIColumns();
            middle.ColumnScrollEnabled[0] = true;
            middle.ColumnScrollEnabled[1] = true;
            root.AddToColumn(0, middle);

            folderColumn = new UIColumns();
            folderColumn.ColumnScrollEnabled[0] = true;
            folderColumn.ColumnScrollEnabled[1] = true;
            middle.AddToColumn(0, folderColumn);

            fileColumn = new UIColumns();
            fileColumn.ColumnScrollEnabled[0] = true;
            fileColumn.ColumnScrollEnabled[1] = true;
            middle.AddToColumn(1, fileColumn);

            // Bottom: actions
            UIColumns bottomRow = new UIColumns();
            bottomRow.ColumnScrollEnabled[0] = false;
            bottomRow.ColumnScrollEnabled[1] = false;
            root.AddToColumn(0, bottomRow);

            UIButton cancelBtn = new UIButton("Cancel", (c, b) =>
            {
                Toasts.Info("Explorer cancelled");
            });
            bottomRow.AddToColumn(0, cancelBtn);

            UIButton actionBtn = new UIButton("Select", (c, b) =>
            {
                if (string.IsNullOrWhiteSpace(selectedPath))
                {
                    Toasts.Error("Nothing selected");
                    return;
                }

                if (mode == ExplorerMode.SelectFolder)
                {
                    if (Directory.Exists(selectedPath))
                    {
                        onSelect?.Invoke(selectedPath);
                        Toasts.Success("Folder selected");
                    }
                    else Toasts.Error("Selected path is not a folder");
                }
                else if (mode == ExplorerMode.SaveFile)
                {
                    if (string.IsNullOrWhiteSpace(saveNameInput?.Text))
                    {
                        Toasts.Error("Enter a file name to save");
                        return;
                    }

                    string targetFolder = Directory.Exists(selectedPath) ? selectedPath : currentPath;
                    string final = Path.Combine(targetFolder, saveNameInput.Text);
                    onSelect?.Invoke(final);
                    Toasts.Success("Save path chosen");
                }
                else // SelectFile or Browse
                {
                    if (File.Exists(selectedPath))
                    {
                        onSelect?.Invoke(selectedPath);
                        Toasts.Success("File selected");
                    }
                    else if (Directory.Exists(selectedPath) && mode == ExplorerMode.Browse)
                    {
                        // allow browsing selection in browse-mode
                        SetCurrentPath(selectedPath);
                    }
                    else
                    {
                        Toasts.Error("Selected path is not a file");
                    }
                }
            });
            bottomRow.AddToColumn(1, actionBtn);

            if (mode == ExplorerMode.SaveFile)
            {
                saveNameInput = new UITextInput()
                {
                    Placeholder = "file.txt",
                    AllowMultiline = false,
                    MinLines = 1
                };
                bottomRow.AddToColumn(0, saveNameInput);
                actionBtn.Text = "Save";
            }
            else if (mode == ExplorerMode.SelectFolder)
            {
                actionBtn.Text = "Select Folder";
            }
            else if (mode == ExplorerMode.SelectFile)
            {
                actionBtn.Text = "Select File";
            }

            // --- Helpers ---

            void SetBreadcrumb(string path)
            {
                breadcrumb.Text = path;
            }

            void SetCurrentPath(string path)
            {
                currentPath = path;
                SetBreadcrumb(path);
                RefreshFolderTreeSelection(path);
                Task.Run(() => RefreshFileList());
            }

            void RefreshAll()
            {
                Task.Run(() =>
                {
                    RefreshFolderTreeRoot();
                    RefreshFileList();
                });
            }

            void RefreshFolderTreeRoot()
            {
                folderColumn.ClearColumn(0);
                folderColumn.AddToColumn(0, new UIText("Folders", UIFontSizePreset.Title));

                try
                {
                    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    UIButton homeBtn = new UIButton("Home", (c, b) => SetCurrentPath(home));
                    folderColumn.AddToColumn(0, homeBtn);
                }
                catch { }

                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                        {
                            string drivePath = drive.RootDirectory.FullName;
                            UITreeNode driveNode = new UITreeNode(drivePath);
                            driveNode.ClickInvocation.Subscribe(node =>
                            {
                                var dirPath = drivePath;
                                SetCurrentPath(dirPath);
                                EnsureTreeNodePopulated(driveNode, dirPath);
                            });
                            folderColumn.AddToColumn(0, driveNode);
                        }

                        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                        List<(string Label, string Path)> cloudLocations = new()
                {
                    ("OneDrive", Path.Combine(userHome, "OneDrive")),
                    ("Proton Drive", Path.Combine(userHome, "Proton Drive")),
                    ("Dropbox", Path.Combine(userHome, "Dropbox")),
                    ("Google Drive", Path.Combine(userHome, "Google Drive")),
                    ("Proton Drive", "C:\\ProtonDrive")
                };

                        foreach (var location in cloudLocations)
                        {
                            if (Directory.Exists(location.Path))
                            {
                                UITreeNode cloudNode = new UITreeNode(location.Label);
                                cloudNode.ClickInvocation.Subscribe(node =>
                                {
                                    SetCurrentPath(location.Path);
                                    EnsureTreeNodePopulated(cloudNode, location.Path);
                                });

                                folderColumn.AddToColumn(0, cloudNode);
                            }
                        }

                    }
                    else
                    {
                        UITreeNode rootNode = new UITreeNode("/");
                        rootNode.ClickInvocation.Subscribe(node =>
                        {
                            SetCurrentPath("/");
                            EnsureTreeNodePopulated(rootNode, "/");
                        });
                        folderColumn.AddToColumn(0, rootNode);
                    }
                }
                catch { }

                Task.Run(() => ExpandTreeToPath(currentPath));
            }

            void EnsureTreeNodePopulated(UITreeNode node, string path)
            {
                Task.Run(() =>
                {
                    List<string> subdirs = new();
                    try
                    {
                        foreach (var d in Directory.GetDirectories(path))
                        {
                            subdirs.Add(d);
                        }
                        subdirs.Sort(StringComparer.InvariantCultureIgnoreCase);
                    }
                    catch { }

                    node.ClearChildren();
                    foreach (var sub in subdirs)
                    {
                        string name = Path.GetFileName(sub);
                        if (string.IsNullOrEmpty(name)) name = sub;

                        UITreeNode child = new UITreeNode(name);
                        child.ClickInvocation.Subscribe(n =>
                        {
                            SetCurrentPath(sub);
                            EnsureTreeNodePopulated(child, sub);
                        });
                        node.AddChild(child);
                    }
                });
            }

            void ExpandTreeToPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                try
                {
                    // keep simple: refresh root only
                }
                catch { }
            }

            void RefreshFolderTreeSelection(string path)
            {
                // noop for now
            }

            UIButton CreateFileButton(string file, bool openDirectlyAsText)
            {
                string display = Path.GetFileName(file);
                UIButton fileBtn = null!;

                if (openDirectlyAsText)
                {
                    fileBtn = new UIButton(display, (c, b) =>
                    {
                        selectedPath = file;
                        HighlightSelection(fileBtn);
                        container.AddTextEditor(file, true);
                    });
                }
                else
                {
                    fileBtn = new UIButton(display, (c, b) =>
                    {
                        selectedPath = file;
                        HighlightSelection(fileBtn);
                        Toasts.Info("Selected: " + display);
                    });
                }

                fileBtn.OnTooltipConfigure = Invocation.Create((Tooltip tip) =>
                {
                    tip.AddText(file);
                    tip.AddText($"Size: {(new FileInfo(file).Length)} bytes");

                    UIColumns tipCols = new();

                    tipCols.AddToColumn(0, new UIButton("Open externally", (cc, bb) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = file,
                                UseShellExecute = true
                            });
                            if (container is UIWindow wind && ForgeWardenEngine.Current.Window.ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
                                wind.Collapsed = true;
                            //Toasts.Info("Opened file externally");
                        }
                        catch (Exception ex)
                        {
                            Toasts.Error("Failed to open: " + ex.Message);
                        }
                    }));

                    if (!openDirectlyAsText)
                    {
                        tipCols.AddToColumn(0, new UIButton("Open as text", (cc, bb) =>
                        {
                            container.AddTextEditor(file, true);
                        }));
                    }

                    tip.AddContent(tipCols);
                });

                return fileBtn;
            }


            void RefreshFileList()
            {
                fileColumn.ClearColumn(0);
                fileColumn.AddToColumn(0, new UIText("Contents", UIFontSizePreset.Title));

                UIButton pathBtn = null!;
                pathBtn = new UIButton("[Current Folder] " + currentPath, (c, b) =>
                {
                    selectedPath = currentPath;
                    HighlightSelection(pathBtn);
                    Toasts.Info("Selected folder: " + currentPath);
                });
                fileColumn.AddToColumn(0, pathBtn);

                List<string> directories = new();
                List<string> files = new();

                try
                {
                    directories.AddRange(Directory.GetDirectories(currentPath));
                    files.AddRange(Directory.GetFiles(currentPath));
                }
                catch (Exception ex)
                {
                    Toasts.Error("Failed listing contents: " + ex.Message);
                }

                directories.Sort(StringComparer.InvariantCultureIgnoreCase);
                files.Sort(StringComparer.InvariantCultureIgnoreCase);

                foreach (var dir in directories)
                {
                    string display = Path.GetFileName(dir);
                    if (string.IsNullOrWhiteSpace(display)) display = dir;

                    UIButton dirBtn = new UIButton(display + "/", (c, b) =>
                    {
                        SetCurrentPath(dir);
                    });

                    dirBtn.OnTooltipConfigure = Invocation.Create((Tooltip tip) =>
                    {
                        tip.AddText(dir);
                        UIColumns tipCols = new();
                        tipCols.AddToColumn(0, new UIButton("Select folder", (cc, bb) =>
                        {
                            selectedPath = dir;
                            HighlightSelection(dirBtn);
                            Toasts.Success("Folder selected");
                        }));
                        tip.AddContent(tipCols);
                    });

                    fileColumn.AddToColumn(0, dirBtn);
                }

                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    bool openDirectlyAsText = ext == ".txt" || ext == ".md";

                    UIButton fileBtn = CreateFileButton(file, openDirectlyAsText);
                    fileColumn.AddToColumn(0, fileBtn);
                }

            }

            void HighlightSelection(UIButton btn)
            {
                lastSelectedButton = btn;
            }

            // initial population
            Task.Run(() =>
            {
                try
                {
                    RefreshFolderTreeRoot();
                    SetCurrentPath(currentPath);
                }
                catch (Exception ex)
                {
                    Toasts.Error("Explorer init failed: " + ex.Message);
                }
            });

            return root;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="filePath"></param>
        /// <param name="asEmbed">When true, the close button should remove the elements this method created from the container rather than closing the container</param>
        private static void CreateAndAddTextEditor(IUIContainer container, string filePath, bool asEmbed)
        {
            string content = string.Empty;
            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Toasts.Error("Failed to read file: " + ex.Message);
                return;
            }

            // editor root content
            UIColumns editorRoot = new UIColumns();
            editorRoot.ColumnScrollEnabled[0] = true;
            editorRoot.ColumnScrollEnabled[1] = true;

            UITextInput editorInput = new UITextInput()
            {
                Text = content,
                AllowMultiline = true,
                MinLines = 20
            };

            editorRoot.AddToColumn(0, editorInput);

            UIColumns editorButtons = new UIColumns();
            editorButtons.ColumnScrollEnabled[0] = false;
            editorButtons.ColumnScrollEnabled[1] = false;

            UIButton saveBtn = new UIButton("Save", (c, b) =>
            {
                try
                {
                    File.WriteAllText(filePath, editorInput.Text);
                    Toasts.Success("File saved");
                }
                catch (Exception ex)
                {
                    Toasts.Error("Save failed: " + ex.Message);
                }
            });

            UIButton closeBtn = new UIButton("Close", (c, b) =>
            {
                if (asEmbed)
                    container.RemoveContent(editorRoot);
                else
                    container.Close();
            });


            editorButtons.AddToColumn(0, saveBtn);
            editorButtons.AddToColumn(1, closeBtn);

            editorRoot.AddToColumn(1, editorButtons);

            try
            {
                container.AddContent((UIColumns)editorRoot); // keep editor visible if window APIs not present
            }
            catch
            {
                try { container.AddContent(editorRoot); } catch { Toasts.Error("Failed to open editor UI"); }
            }
        }


    }
}


