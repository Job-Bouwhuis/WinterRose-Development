using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Enemies.Movement;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Enemies;

[RequireComponent<AIMovementController>]
internal abstract class Enemy : ObjectBehavior
{
    public Weapon Weapon { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }

    public AIMovementController MovementController { get; private set; }

    protected override void Awake()
    {
        MovementController = FetchComponent<AIMovementController>();
    }

    protected override void Update()
    {
        // implement 
    }
}
