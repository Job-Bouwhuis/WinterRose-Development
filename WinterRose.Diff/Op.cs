namespace WinterRose.Diff;

public abstract class Op
{
    public long Offset { get; set; }
    protected Op(long offset) => Offset = offset;
    protected Op() { }
}

public class Delete : Op
{
    public long Length { get; set; }
    private Delete() { } // serialization
    public Delete(long offset, long length) : base(offset) => Length = length;
}

// this operation indicates that the entire file should be deleted
// it does not need any arguments, as it applies to the entire file
public class DeleteFile : Op; 

public class Insert : Op
{
    public byte[] Data { get; set; }
    private Insert() { } // serialization
    public Insert(long offset, byte[] data) : base(offset) => Data = data;
}

public class Update : Op
{
    public long Length { get; set; }      // how many old bytes to replace
    public byte[] Data { get; set; }      // new bytes to write
    private Update() { }             // serialization
    public Update(long offset, long length, byte[] data) : base(offset)
    {
        Length = length;
        Data = data;
    }
}