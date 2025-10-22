using System.Text;

namespace WinterRose.Recordium;

public class FileDestination : ILogDestination
{
    private Stream fileStream;

    public FileDestination(string fileDirectory)
    {
        string date =  DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss");

        DirectoryInfo di =  new DirectoryInfo(fileDirectory);
        if (!di.Exists)
            di.Create();

        string fileName = Path.Combine(di.FullName, date + ".log");

        FileInfo fi = new FileInfo(fileName);
        fileStream = new FileStream(fileName, FileMode.Create);

        // create shortcut in same dir to latest file (the one were creating now)
        FileInfo shortcut = new  FileInfo(Path.Combine(di.FullName, "latest.log"));
        if(shortcut.Exists)
            shortcut.Delete();
        shortcut.CreateAsSymbolicLink(fi.FullName);
    }

    public bool Invalidated { get; set; }
    public async Task WriteAsync(LogEntry entry)
    {
        await fileStream.WriteAsync(Encoding.UTF8.GetBytes(entry.ToString(LogVerbosity.Detailed) + Environment.NewLine));
        await fileStream.FlushAsync();
    }
}