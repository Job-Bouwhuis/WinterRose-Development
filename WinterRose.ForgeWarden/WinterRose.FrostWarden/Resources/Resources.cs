using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Resources
{
    internal static class Resources
    {
        internal static string GetResourceText(string assemblyName, string resourceName)
        {
            resourceName = resourceName.Replace(" ", "_").Replace("-", "_").Replace("\\", ".").Replace("/", ".");
            string fullResourceName = assemblyName + resourceName;

            var assembly = typeof(Resources).Assembly;

            using Stream? stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                string[] allResources = assembly.GetManifestResourceNames();
                string found = allResources.Search(fullResourceName);
                throw new Exception($"Resource \"{fullResourceName}\" not found!\n\nPerhaps did you mean: \"{found}\"?");
            }

            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }
}
