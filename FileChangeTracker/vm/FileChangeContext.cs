using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public class FileChangeContext : IDisposable
    {
        private readonly Dictionary<string, Stream> openStreams = [];
        private string currentFile;

        public Stream CurrentStream => openStreams[currentFile];

        public void SetFile(string path)
        {
            if (!openStreams.TryGetValue(path, out Stream stream))
            {
                stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                openStreams[path] = stream;
            }

            currentFile = path;
        }

        public void AddFile(string path)
        {
            var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
            openStreams[path] = stream;
            currentFile = path;
        }

        public void DeleteFile(string path)
        {
            if(openStreams.TryGetValue(path, out Stream existing))
            {
                existing.Dispose();
                openStreams.Remove(path);
            }

            if (File.Exists(path))
                File.Delete(path);

            if (currentFile == path)
                currentFile = null;
        }

        public void SetFile(string path, Stream stream)
        {
            openStreams[path] = stream;
            currentFile = path;
        }

        public void Dispose()
        {
            foreach(var s in openStreams)
                s.Value.Dispose();

            openStreams.Clear();
            currentFile = null!;
        }
    }
}