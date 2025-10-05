
namespace WinterRose.ForgeWarden.Input;

[Serializable]
internal class InvalidInputException : Exception
{
    public InvalidInputException(string controlName) 
        : base($"Input of name {controlName} was not found in the input system. Did you forget to register it?")
    {
    }

}