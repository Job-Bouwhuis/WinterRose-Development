namespace WinterRose.ForgeWarden.Input;

public class InputBinding
{
    public InputDeviceType DeviceType { get; set; }
    public int Code { get; set; } // keycode, button index, axis index
    public InputAxisRelation Relation { get; set; } = InputAxisRelation.Positive;
    public float Threshold { get; set; } = 0.5f; // for analog axes

    public InputBinding(InputDeviceType deviceType, int code, InputAxisRelation relation, float threshold)
    {
        DeviceType = deviceType;
        Code = code;
        Relation = relation;
        Threshold = threshold;
    }
    public InputBinding(InputDeviceType deviceType, int code)
    {
        DeviceType = deviceType;
        Code = code;
    }
    private InputBinding() { } // for serialization
}
