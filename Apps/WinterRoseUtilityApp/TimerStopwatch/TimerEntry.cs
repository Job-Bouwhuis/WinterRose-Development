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
        UIColumns cols = new UIColumns();
        cols.AddToColumn(0, new UIButton("Create Timer", (c, b) => ContainerCreators.AddTimer().Show()));
        cols.AddToColumn(1, new UIButton("View all timers", (c, b) => ContainerCreators.ViewTimers().Show()));
        Program.Current.AddTrayItem(cols);
    }

    public override void Init() { }
    public override void Update() 
    {
        TimerManager.Update();
    }
    public override void Destroy() { }
}
