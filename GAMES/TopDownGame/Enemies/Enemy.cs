using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Enemies.Movement;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.StatusSystem;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Enemies;

[RequireComponent<AIMovementController>]
[RequireComponent<Vitality>] // technically enforced by StatusEffector. but here for explicitness
[RequireComponent<StatusEffector>]
internal class Enemy : ObjectBehavior
{
    public Weapon Weapon { get; set; }
    public Vitality vitality { get; private set; }
    public StatusEffector effector { get; private set; }
    public string Name { get; set; }
    public int Level { get; set; }

    public AIMovementController MovementController { get; private set; }

    protected override void Awake()
    {
        MovementController = FetchComponent<AIMovementController>();
    }

    protected override void Update()
    {
        if(Weapon is not null)
        {
            Weapon.DoShot();
            Weapon.ResetShooting();
        }
    }
}
