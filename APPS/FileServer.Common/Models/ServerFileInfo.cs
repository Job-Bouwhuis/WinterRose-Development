using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FileServer.Common
{
    public class ServerFileInfo(FileServerClient server, string name, string path, int size, DateTime lastModified)
    {
        public string Name { get; } = name;
        public string Path { get; } = path;
        public int Size { get; } = size;
        public DateTime LastModified { get; } = lastModified;
        public byte[] FileData
        {
            get
            {
                return server.DownloadFile(Path);
            }
            set
            {
                server.UploadFile(Path, value);
            }
        }
    }
}
