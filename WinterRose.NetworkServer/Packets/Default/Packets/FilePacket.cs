using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets
{
    public class FilePacket : Packet
    {
        private FilePacket() // for serialization
        {
        }

        public FilePacket(FileInfo file)
        {
            using FileStream stream = file.OpenRead();
            Header = new FileHeader();
            Content = new FileContent(stream);
        }

        public FilePacket(FileStream file)
        {
            Header = new FileHeader();
            Content = new FileContent(file);
        }

        public class FileHeader : PacketHeader
        {
            public override string GetPacketType() => "file";
        }

        public class FileContent : PacketContent
        {
            public string fileName = "";
            public int totalChunks;
            public string fileHash = "";
            [WFInclude]
            private List<FileChunk> chunks;

            public FileContent(FileStream file)
            {
                chunks = [];
                fileName = Path.GetFileName(file.Name);
                byte[] bytes = new byte[1024];
                int read = file.Read(bytes, 0, 1024);
                while (read != 0)
                {
                    chunks.Add(new FileChunk(bytes.ToArray(), (short)read));
                    read = file.Read(bytes, 0, 1024);
                }
            }

            private FileContent() { } // for serialization

            public void Write(string path)
            {
                Write(new FileInfo(path));
            }

            public void Write(FileInfo file)
            {
                using FileStream fs = file.OpenWrite();
                Write(fs);
            }

            public void Write(Stream stream)
            {
                foreach(var chunk in chunks)
                    foreach(byte b in chunk)
                        stream.WriteByte(b);
            }
        }

        private struct FileChunk
        {
            public short length;
            public byte[] data;

            public FileChunk(byte[] data, short length)
            {
                this.data = data;
                this.length = length;
            }

            public IEnumerator<byte> GetEnumerator()
            {
                for (int i = 0; i < length; i++)
                    yield return data[i];
            }
        }
    }
}
