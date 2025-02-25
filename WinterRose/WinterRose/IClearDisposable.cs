using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose;

/// <summary>
/// An interface that has a mandatory IsDisposed property.
/// <br></br> Implements <see cref="IDisposable"/>
/// </summary>
public interface IClearDisposable : IDisposable
{
    /// <summary>
    /// Whether this object has been disposed.
    /// </summary>
    public bool IsDisposed { get; }
}
