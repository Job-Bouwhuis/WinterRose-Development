using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeThread;
public static class ForgeThreadExtensions
{
    /// <summary>
    /// Waits for all coroutines to be completed and blocks the caller thread
    /// </summary>
    /// <param name="couroutines"></param>
    public static void WhenAll(this List<CoroutineHandle> couroutines)
    {
        while(true)
        {
            bool done = true;
            foreach (CoroutineHandle handle in couroutines)
            {
                if (!handle.IsStopped)
                    done = false;
            }
            if (done)
                break;
        }
    }
}
