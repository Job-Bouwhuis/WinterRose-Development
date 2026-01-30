using System.Runtime.CompilerServices;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.Editor;

public class Vector3PropertyDrawer : InspectorPropertyDrawer<Vector3>
{
    private UINumericUpDown<float> x;
    private UINumericUpDown<float> y;
    private UINumericUpDown<float> z;

    protected override UIContent CreateContent()
    {
        UIColumns cols = new();
        x =  new UINumericUpDown<float>();
        x.Label = "x";

        y =  new UINumericUpDown<float>();
        y.Label = "y";

        z =  new UINumericUpDown<float>();
        z.Label = "z";

        var OnEditorValueUpdated = Invocation.Create(
            (IUIContainer container, UINumericUpDown<float> self,
            float newVal) =>
            {
                log.Info($"{self.Label} = {newVal}");
                Vector3 old = (Vector3)TrackedValue.Value;
                switch (self.Label)
                {
                    case "x":
                        {

                        }
                        break;
                    case "y":
                        {
                            TrackedValue.Set(old with { Y = newVal });
                        }
                        break;
                    case "z":
                        {
                            TrackedValue.Set(old with { Z = newVal });
                        }
                        break;
                }
            });

        x.OnValueChanged.Subscribe(OnEditorValueUpdated);
        y.OnValueChanged.Subscribe(OnEditorValueUpdated);
        z.OnValueChanged.Subscribe(OnEditorValueUpdated);

        cols.AddContent(x);
        cols.AddContent(y);
        cols.AddContent(z);
        return cols;
    }

    protected internal override void Init()
    {
        Vector3 val = (Vector3)TrackedValue.Value;

        x.Value = val.X;
        y.Value = val.Y;
        z.Value = val.Z;
        log.Info($"updated Value: {val}");
    }

    protected internal override void ValueUpdated()
    {
        Init();
    }
}