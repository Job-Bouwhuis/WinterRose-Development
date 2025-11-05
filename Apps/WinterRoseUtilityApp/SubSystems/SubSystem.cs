using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.SubSystems;
public abstract class SubSystem
{
    protected Log log { get; }

    protected SubSystem(string name,  string description, Version version)
    {
        log = new(name);
        Name = name;
        Description = description;
        Version = version;
    }

    public string Name { get; }
    public string Description { get; }
    public Version Version { get; }

    public virtual void Init() { }
    public virtual void Update() { }
    public virtual void Draw() { }
    public virtual void Destroy() { }

}
