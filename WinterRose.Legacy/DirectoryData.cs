using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FileManagement
{
    internal class DirectoryData
    {
        public List<DirectoryData> directories;
        public List<FileData> files;
        public string name;
        public string path;
    }
    internal class FileData
    {
        public string name;
        public string path;
        public string extention;
        public string data;
    }
}
