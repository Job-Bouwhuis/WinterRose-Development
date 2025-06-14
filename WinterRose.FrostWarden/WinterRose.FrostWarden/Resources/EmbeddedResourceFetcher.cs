using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Resources
{
    internal static class EmbeddedResourceFetcher
    {
        internal static string GetResourceText(string resourceName)
        {
            resourceName = resourceName.Replace(" ", "_").Replace("-", "_").Replace("\\", ".").Replace("/", ".");
            string fullResourceName = "WinterRose.FrostWarden.Resources." + resourceName;

            var assembly = typeof(EmbeddedResourceFetcher).Assembly;

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
