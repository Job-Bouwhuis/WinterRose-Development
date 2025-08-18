using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;

namespace WinterRose.ForgeWarden.AssetPipeline
{
    public sealed class AssetHeader : IEquatable<AssetHeader>
    {
        [WFInclude]
        public string Name { get; internal set; }
        [WFInclude]
        public string Path { get; internal set; }
        [WFInclude]
        public List<string> Tag { get; internal set; }
        [WFInclude]
        public Anonymous? Metadata { get; internal set; } = null;

        public AssetHeader(string name, string path)
        {
            Name = name;
            Path = path;
            Tag = [];
        }

        private AssetHeader() { } // For serialization purposes

        public AssetHeader(string name, string path, List<string> tag, Anonymous? metadata = null)
        {
            Name = name;
            Path = path;
            Tag = tag;
            Metadata = metadata;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as AssetHeader);
        }

        public bool Equals(AssetHeader? other)
        {
            if (other == null)
                return false;

            return string.Equals(Name, other.Name, StringComparison.Ordinal)
                && string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase)
                && Tag.SequenceEqual(other.Tag);
        }

        public override int GetHashCode()
        {
            int hash = StringComparer.Ordinal.GetHashCode(Name)
                     ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

            foreach (string tag in Tag)
            {
                hash ^= StringComparer.Ordinal.GetHashCode(tag);
            }

            return hash;
        }

        public T? LoadAs<T>() where T : class
        {
            if (string.IsNullOrEmpty(Path))
                throw new InvalidOperationException("Asset path is not set.");
            return Assets.Load<T>(this);
        }
    }

}
