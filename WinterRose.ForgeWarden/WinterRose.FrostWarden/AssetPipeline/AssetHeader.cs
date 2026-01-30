using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;

namespace WinterRose.ForgeWarden.AssetPipeline
{
    public class AssetHeader : IEquatable<AssetHeader>
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public List<string> Tag { get; internal set; }
        public Anonymous? Metadata { get => field ??= new Anonymous(); internal set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Path)
            && Tag != null;

        public bool IsReadOnly { get; internal set; }

        [WFInclude]
        internal string AssemblyName { get; set; }
        private Assembly assembly;

        /// <summary>
        /// Will be null if the asset is not an assembly resource asset
        /// </summary>
        public Assembly? SourceAssembly
        {
            get
            {
                if (assembly != null)
                    return assembly;

                return assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == AssemblyName);
            }
        }

        public FileStream Source
        {
            get
            {
                if (SourceAssembly == null)
                {
                    return File.Open(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

                Stream s = SourceAssembly.GetManifestResourceStream(Path);
                TempFile tmp = new(s, FileShare.Read, extension: System.IO.Path.GetExtension(Path));
                return tmp;
            }
        }

        public AssetHeader(string name, string path)
        {
            Name = name;
            Path = path;
            Tag = [];
        }

        /// <summary>
        /// Exists for serialization. use the other constructors
        /// </summary>
        public AssetHeader() { }

        public AssetHeader(string name, string path, List<string> tag, Anonymous? metadata = null)
        {
            Name = name;
            Path = path;
            Tag = tag;
            Metadata = metadata;
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

        /// <summary>
        /// Loads this header through the asset pipeline.
        /// </summary>
        /// <remarks>Do not call this within <see cref="IAssetHandler{T}.LoadAsset(AssetHeader)"/></remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T? LoadAs<T>()
        {
            if (string.IsNullOrEmpty(Path))
                throw new InvalidOperationException("Asset path is not set.");
            return Assets.Load<T>(this);
        }
    }

}
