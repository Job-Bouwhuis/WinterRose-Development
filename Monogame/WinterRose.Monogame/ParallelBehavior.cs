using System;

namespace WinterRose.Monogame;

/// <summary>
/// This <see cref="ObjectBehavior"/> will have its update loop be executed in parallel. This attribute will do nothing on a <see cref="ObjectComponent"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ParallelBehavior : Attribute
{
    public ParallelBehavior() 
    {

    }
}