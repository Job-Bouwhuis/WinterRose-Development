using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.DamageSystem;

/// <summary>
/// Destroys the object when its <see cref="Vitality.OnDeath"/> is called
/// </summary>
[RequireComponent<Vitality>(AutoAdd = false)]
public class DestroyOnDeath : ObjectComponent
{
    protected override void Start()
    {
        Vitality vitality = FetchComponent<Vitality>();
        vitality.OnDeath += () => Destroy(owner);
    }
}
