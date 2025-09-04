using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeSignal;

/// <summary>
/// Marks a method to allow on another thread when used as a subscribed method in a <see cref="Signal"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AllowMultiThreadAttribute : Attribute;
