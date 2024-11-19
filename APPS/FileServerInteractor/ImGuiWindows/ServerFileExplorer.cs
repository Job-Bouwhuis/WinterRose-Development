using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.FileManagement;
using WinterRose.FileServer.Common;
using WinterRose.ImGuiApps;
using static FileServerInteractor.Globals;

namespace FileServerInteractor.ImGuiWindows;

internal class ServerFileExplorer : ImGuiWindow
{
    string searchInput = "";
    bool once = true;

    FileDropTarget dropTarget;

    public ServerFileExplorer() : base("Server File Explorer")
    {
        Flags = ImGuiWindowFlags.NoCollapse;

        selectedFolder = client.GetRootFolder();

        Timer t = new Timer(t =>
        {
            10.Repeat(x => GC.Collect());
        }, null, 0, 5000);

        dropTarget = new();
    }

    private ServerDirectoryInfo selectedFolder;

    public override void Render()
    {
        if (once)
        {
            client.ServerClosed += Application.Close;
            once = false;
        }

        gui.SetWindowFontScale(1.1f);
        gui.Text("Server files");
        gui.SetWindowFontScale(1.0f);

        gui.InputText("Search", ref searchInput, 100);


        if (gui.Button("Download all"))
        {
            FolderDownloader fd = new(selectedFolder);
            Application.AddWindow(fd);
        }
        if (gui.BeginItemTooltip())
        {
            gui.Text("Download all files and subdirectories recursively from the current directory.");
            gui.EndTooltip();
        }

        gui.SameLine();

        if (gui.Button("Upload folder"))
        {
            FolderUploader fu = new(selectedFolder.Path, () => selectedFolder = client.GetFolder(selectedFolder.Path));
            Application.AddWindow(fu);
        }

        if (gui.Button("Upload File"))
        {
            string currentWorkingDir = Directory.GetCurrentDirectory();
            _ = Task.Run(() =>
            {
                if (Windows.OpenFile(out string selectedPath, "Upload file"))
                {
                    string extensions = "." + selectedPath.Split('.').Last();
                    string path = selectedFolder.Path + "\\" + Path.GetFileName(selectedPath);
                    client.UploadFile(path, File.ReadAllBytes(selectedPath));

                    System.Diagnostics.Process.Start("explorer.exe", FileManager.PathOneUp(selectedPath));
                    Directory.SetCurrentDirectory(currentWorkingDir);
                }
            }).ContinueWith(task => 10.Repeat(x => GC.Collect()));
        }

        gui.BeginChild("files", new(300, 600));

        // This should accept files and folders
        if (gui.AcceptDragDropPayload("what do i put here, sir GPT?") is ImGuiPayloadPtr payload)
        {
            // TODO: (not for the chat-gpt) implement uploading of the file or folder
        }

        if (selectedFolder.Name != "root")
        {
            if (gui.Button("Back"))
            {
                selectedFolder = client.GetParentFolder(selectedFolder);
            }
        }

        foreach (var dir in selectedFolder.Directories)
        {
            RenderDirectory(dir);
        }

        foreach (var file in selectedFolder.Files)
        {
            RenderFile(file);
        }

        gui.EndChild();
    }

    private void RenderFile(ServerFileInfo file)
    {
        if (gui.Selectable($"{file.Name} - {file.Size} - {file.LastModified}"))
        {
            string path = file.Path;
            string name = file.Name;

            _ = Task.Run(() =>
            {
                string extensions = "." + name.Split('.').Last();

                string currentWorkingDir = Directory.GetCurrentDirectory();
                if (Windows.SaveFile(out string selectedPath, "Download file", defaultExtension: extensions))
                {
                    var fileData = client.DownloadFile(path);
                    File.WriteAllBytes(selectedPath, fileData);

                    System.Diagnostics.Process.Start("explorer.exe", FileManager.PathOneUp(selectedPath));
                    Directory.SetCurrentDirectory(currentWorkingDir);
                }
            }).ContinueWith(task => 10.Repeat(x => GC.Collect()));
        }
    }

    private void RenderDirectory(ServerDirectoryInfo dir)
    {
        if (gui.Selectable(dir.Name))
            selectedFolder = client.GetFolder(dir.Path);
    }

    public override void OnWindowClose(WindowCloseEventArgs e)
    {
        client.Disconnect();
    }
}
