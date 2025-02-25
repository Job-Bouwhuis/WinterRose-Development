using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// A web URL.
    /// </summary>
    public class URL
    {
        string? url;

        public URL(string url)
        {
            this.url = url;
        }

        public void OpenInDefaultBrowser()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public override string ToString()
        {
            return url;
        }

        public static implicit operator string(URL url)
        {
            return url.ToString();
        }

        public static implicit operator URL(string url)
        {
            return new URL(url);
        }
    }
}
