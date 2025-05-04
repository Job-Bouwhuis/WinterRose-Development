
namespace WinterRose.Reflection;

[Serializable]
internal class FieldNotFoundException : Exception
{
    public FieldNotFoundException()
    {
    }

    public FieldNotFoundException(string? message) : base(message)
    {
    }

    public FieldNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}