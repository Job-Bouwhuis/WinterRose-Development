using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FileServer.Common
{
    public class ServerDirectoryInfo
    {
        public ServerDirectoryInfo(FileServerClient server, string name, string path, DateTime lastModified)
        {
            Name = name;
            Path = path;
            LastModified = lastModified;
        }

        public string Name { get; }
        public string Path { get; }
        public DateTime LastModified { get; }

        public List<ServerFileInfo> Files { get; } = new();
        public List<ServerDirectoryInfo> Directories { get; } = new();
    }
}
