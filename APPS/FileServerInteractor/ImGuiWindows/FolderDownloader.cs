using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.FileServer.Common;
using static FileServerInteractor.Globals;

namespace FileServerInteractor.ImGuiWindows
{
    internal class FolderDownloader(ServerDirectoryInfo selectedFolder, int a) : ImGuiWindow("Folder Downloader")
    {
        LoadingAscii loading = new();
        Task downloadTask;


        float progress = 0;
        int totalfiles = 0;
        int filesDownloaded = 0;
        bool begun = false;

        public FolderDownloader(ServerDirectoryInfo selectedFolder) : this(selectedFolder, 1)
        {
            downloadTask = Task.Run(DownloadEntireFolder);
        }

        public override void Render()
        {
            if(!begun)
            {
                gui.Text("Waiting for user to select a folder...");
                return;
            }
            progress = (float)filesDownloaded / totalfiles;
            gui.SetWindowSize(new(400, 200));
            gui.Text("Downloading folder...");
            gui.ProgressBar(progress, new(400, 20));
            gui.Text($"{progress * 100}%");
            if(progress == 1)
                Close();
        }

        private async Task DownloadEntireFolder()
        {
            await Task.Run(() =>
            {
                if (Windows.OpenFolder(out string selectedPath, "Download to..."))
                {
                    totalfiles = int.Parse(client.Client.SendAndResponse("*filecount*" + selectedFolder.Path).Payload);
                    begun = true;
                    DownloadFolder(selectedFolder, selectedPath);
                }
            });
        }

        private void DownloadFolder(ServerDirectoryInfo source, string destination)
        {
            foreach (var file in source.Files)
            {
                string path = file.Path;
                string name = file.Name;

                string extensions = "." + name.Split('.').Last();
                string currentWorkingDir = Directory.GetCurrentDirectory();
                string filePath = Path.Combine(destination, name);
                var fileData = client.DownloadFile(path);
                File.WriteAllBytes(filePath, fileData);
                filesDownloaded++;
            }

            foreach (var dir in source.Directories)
            {
                string path = dir.Path;
                string name = dir.Name;

                ServerDirectoryInfo d = client.GetFolder(path);
                DirectoryInfo localdir = Directory.CreateDirectory(Path.Combine(destination, name));

                DownloadFolder(d, localdir.FullName);
            }
        }
    }
}
