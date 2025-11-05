using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.TimerStopwatch;
internal class TimerEntry : SubSystem
{
    public TimerEntry() : base("TimerStopwatch", "Used for creating and viewing timers and stopwatches", new Version(1, 0, 0))
    {
        Program.Current.AddTrayItem(new UIButton("Create Timer", (c, b) => ContainerCreators.AddTimer().Show()));
    }

    public override void Init() { }
    public override void Update() 
    {
        TimerManager.Update();
    }
    public override void Destroy() { }
}
