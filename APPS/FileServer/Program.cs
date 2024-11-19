using WinterRose;
using WinterRose.FileManagement;
using WinterRose.FileServer;
using WinterRose.Serialization;
using WinterRose.Vectors;

namespace FileServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region dumb tests
            //List<Vector3> vecs = [];

            //100000.Repeat(i => vecs.Add(new(i, i, i)));

            //string serialized = SnowSerializer.Serialize(vecs).Result;
            //FileManager.Write("Database\\largeVector3File.txt", serialized, true);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");
            //FileManager.Write("Database\\largeVector3File.txt", serialized);
            //FileManager.Write("Database\\largeVector3File.txt", "\n\n");

            #endregion

            var server = new ServerMain();


            Timer t = new Timer(t =>
            {
                GC.Collect();
            }, null, 0, 5000);

            while (true)
            {
                if (server.IsDisposed)
                    break;
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    server.Close();
            }
        }
    }
}
