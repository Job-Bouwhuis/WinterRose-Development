using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using static FileServerInteractor.Globals;

namespace FileServerInteractor.ImGuiWindows
{
    internal class FolderUploader : ImGuiWindow
    {
        private readonly string destination;
        private readonly Action finishedCallback;
        LoadingAscii loading = new();
        Task uploadTask;


        float progress = 0;
        int totalfiles = 0;
        int filesDownloaded = 0;
        bool begun = false;

        public FolderUploader(string destination, Action finishedCallback) : base("Folder Uploader")
        {
            uploadTask = Task.Run(UploadEntireFolder);
            this.destination = destination;
            this.finishedCallback = finishedCallback;
        }

        public override void Render()
        {
            if (!begun)
            {
                gui.Text("Waiting for user to select a folder...");
                return;
            }
            progress = (float)filesDownloaded / totalfiles;
            gui.SetWindowSize(new(400, 200));
            gui.Text("Downloading folder...");
            gui.ProgressBar(progress, new(400, 20));
            gui.Text($"{progress * 100}%");
            if (progress == 1)
            {
                finishedCallback();
                Close();
            }
        }

        private async Task UploadEntireFolder()
        {
            await Task.Run(() =>
            {
                if (Windows.OpenFolder(out string selectedPath, "Download to..."))
                {
                    var dir = new DirectoryInfo(selectedPath);
                    totalfiles = Count(dir);
                    begun = true;
                    UploadFolder(dir, destination + "\\" + dir.Name);
                }

                int Count(DirectoryInfo info) => info.GetFiles().Length + info.GetDirectories().Sum(dir => Count(dir));
            });
        }

        private void UploadFolder(DirectoryInfo source, string destination)
        {
            foreach (var file in source.GetFiles())
            {
                string path = file.FullName;
                client.UploadFile(destination + "\\" + file.Name, File.ReadAllBytes(path));
                filesDownloaded++;
            }

            foreach (var dir in source.GetDirectories())
            {
                UploadFolder(dir, destination + "\\" + dir.Name);
            }
        }
    }
}
