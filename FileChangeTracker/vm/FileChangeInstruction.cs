using System.Text;

namespace FileChangeTracker;

public abstract class FileChangeInstruction()
{
    public long offset { get; set; }
    public byte[]? data { get; set; }
    public string? path { get; set; }
    public abstract FileChangeOpcode opcode { get; }
    public abstract void Execute(FileChangeContext context);

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder($"opcode: {opcode}, offset: {offset} ");
        if (data is not null)
        {
            sb.Append("data: [");
            foreach (byte b in data)
                sb.Append($"{b} ");
            sb.Append(']');
        }
        else if(path is not null)
        {
            sb.Append($"path: {path}");
        }
        return sb.ToString();
    }
}
