using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.FileServer.Common;
using WinterRose.Networking.TCP;

namespace WinterRose.FileServer;

public class FileServerClient
{
    public TCPUser Client { get; private set; } = new TCPUser();

    public Action ServerClosed = delegate { };

    public ServerDirectoryInfo Root { get; set; }

    public FileServerClient(IPAddress address, int port)
    {
        Root = new(this, "root", "root", DateTime.Now);
        Client.OnServerClosed += Client_OnServerClosed;

        if (!Client.Connect(address, port))
        {
            Windows.MessageBox("Failed to connect to server.", "Error", Windows.MessageBoxButtons.OK, Windows.MessageBoxIcon.Error);
            Client.Dispose();
        }
    }

    private void Client_OnServerClosed()
    {
        Client.Dispose();
        ServerClosed();
    }

    /// <summary>
    /// Retrieves only the file and directory information, does not download the files right away. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ServerDirectoryInfo GetFolder(string path)
    {
        Packet response = Client.SendAndResponse("*getdir*" + path);
        if (response.Payload is null)
        {
            Client.Dispose();
            ServerClosed();
            return null;
        }
        // response format: @dirs@subdir1,lastmodified;subdir2,lastmodified;@files@file1,lastmodified,size;file2,lastmodified,size;

        ServerDirectoryInfo info = path is "root" ? Root : new ServerDirectoryInfo(this, FileManager.PathLast(path), path, DateTime.Now);

        List<string> split = response.Payload.Split('@', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (split.Count is 0 or 1 or 2)
        {
            return info;
        }

        int index = 0;
        for (int i = 0; i < split.Count; i++)
        {
            if (split[index] is "dirs" or "files")
            {
                split.RemoveAt(index);

                index = index is 0 ? 0 : index - 1;

                if (i != 0)
                    break;
                else if (split[index] is "files")
                    split.Insert(0, "");
            }
            else index++;
        }
        if (split.Count is 2 && split[1] is "files")
            split[1] = "";

        string[] dirs = split[0].Split(';', StringSplitOptions.RemoveEmptyEntries);
        string[] files = split[1].Split(';', StringSplitOptions.RemoveEmptyEntries);


        info.Directories.Clear();
        foreach (string dir in dirs)
        {
            string[] dirSplit = dir.Split(',');
            DateTime lastModified;
            if (!DateTime.TryParse(dirSplit[1], out lastModified))
            {
                lastModified = DateTime.MinValue;
            }
            string name = dirSplit[0];
            string dirPath = path + "\\" + name;

            info.Directories.Add(new ServerDirectoryInfo(this, name, dirPath, lastModified));
        }

        info.Files.Clear();
        foreach (string file in files)
        {
            string[] fileSplit = file.Split(',');

            string name = fileSplit[0];
            string filePath = path + "\\" + name;
            int size = int.Parse(fileSplit[1]);
            DateTime lastModified;
            if (!DateTime.TryParse(fileSplit[2], out lastModified))
            {
                lastModified = DateTime.MinValue;
            }

            info.Files.Add(new ServerFileInfo(this, name, filePath, size, lastModified));
        }

        return info;
    }

    public byte[] DownloadFile(string path)
    {
        Packet response = Client.SendAndResponse("*download*" + path);
        if (response.Payload is null)
        {
            Client.Dispose();
            ServerClosed();
            return null;
        }

        if (response.Payload.EndsWith(" is not a valid path"))
            return null;

        return Convert.FromBase64String(response.Payload);
    }

    public void UploadFile(string path, byte[] data)
    {
        Packet response = Client.SendAndResponse("*upload*" + path + "*" + Convert.ToBase64String(data));
        if (response.Payload is null)
        {
            Client.Dispose();
            ServerClosed();
            return;
        }

        if (response.Payload.EndsWith(" is not a valid path"))
        {
            Windows.MessageBox("The path is not valid.", "Error", Windows.MessageBoxButtons.OK, Windows.MessageBoxIcon.Error);
        }
    }

    public ServerDirectoryInfo GetRootFolder() => GetFolder("root");

    public bool Disconnect()
    {
        try
        {
            return Client.Disconnect();
        }
        catch (Exception) // exception thrown if the server is already closed.
        {
            return false;
        }
    }

        public ServerDirectoryInfo GetParentFolder(ServerDirectoryInfo selectedFolder)
        {
            return GetFolder(FileManager.PathOneUp(selectedFolder.Path));
        }
    }
