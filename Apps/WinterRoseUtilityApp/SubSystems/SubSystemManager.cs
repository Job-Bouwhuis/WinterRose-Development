using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.SubSystems;
public class SubSystemManager
{
    private Log log = new Log("Subsystem Manager");

    private List<SubSystem> subSystems = new List<SubSystem>();
    public IReadOnlyList<SubSystem> SubSystems => subSystems;

    public bool Initialize()
    {
        try
        {
            Type[] types = TypeWorker.FindTypesWithBase<SubSystem>();

            foreach (Type t in types)
            {
                if (t == typeof(SubSystem))
                    continue;
                var subsys = (SubSystem)ActivatorExtra.CreateInstance(t);
                if(subsys is null)
                {
                    log.Error($"Failed to start subsystem {t.Name}");
                    continue;
                }
                subSystems.Add(subsys);
            }

            foreach (SubSystem subSystem in subSystems)
            {
                subSystem.Init();
            }
        }
        catch (Exception ex)
        {
            log.Fatal(ex, "App is unable to continue running!");
            return false;
        }



        return true;
    }

    internal void Tick()
    {
        foreach (SubSystem subSystem in subSystems)
        {
            subSystem.Update();
        }
    }

    internal void Draw()
    {

    }

    internal void Close()
    {

    }
}
