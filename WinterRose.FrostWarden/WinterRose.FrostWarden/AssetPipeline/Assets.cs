using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.FrostWarden.AssetPipeline
{
    public static class Assets
    {
        private record HandlerEntry(MethodInfo SaveMethod, MethodInfo LoadMethod, MethodInfo InitializeNewAssetMethod, string[] interestedExtensions);

        private static Dictionary<string, AssetHeader> assetHeaders = [];
        private static Dictionary<Type, HandlerEntry> handerTypeMap = []; 
        private const string ASSET_HEADER_EXTENSION = ".fwassetheader";
        private const string ASSET_EXTENSION = ".fwasset";
        private const string ASSET_ROOT = "Assets/";

        internal static void BuildAssetIndexes()
        {
            Console.WriteLine("INFO: Registering asset handlers");

            Type[] types = TypeWorker.FindTypesWithInterface(typeof(IAssetHandler<>));

            foreach (Type type in types)
            {
                Type[] interfaces = type.GetInterfaces();
                Type assetType = interfaces.FirstOrDefault(i => i.Name.Contains("IAssetHandler")).GetGenericArguments()[0];
                MethodInfo saveMethod = type.GetMethod("SaveAsset", BindingFlags.Public | BindingFlags.Static);
                MethodInfo loadMethod = type.GetMethod("LoadAsset", BindingFlags.Public | BindingFlags.Static);
                MethodInfo InitializeNewAssetMethod = type.GetMethod("InitializeNewAsset", BindingFlags.Public | BindingFlags.Static);
                if (saveMethod == null || loadMethod == null)
                    throw new InvalidOperationException($"Asset handler {type.FullName} does not have required methods.");
                var rh = new ReflectionHelper(type);
                var mem = rh.GetMember("InterestedInExtensions");
                var ie = (string[])mem.GetValue();
                handerTypeMap[assetType] = new HandlerEntry(
                    saveMethod, 
                    loadMethod,
                    InitializeNewAssetMethod,
                    ie
                    );
            }

            // Ensure the asset root directory exists
            if (!System.IO.Directory.Exists(ASSET_ROOT))
                System.IO.Directory.CreateDirectory(ASSET_ROOT);

            FileInfo[] headerFiles = new DirectoryInfo(ASSET_ROOT).GetFiles($"*{ASSET_HEADER_EXTENSION}");


            Console.WriteLine("INFO: Registering asset headers");

            foreach (FileInfo file in headerFiles)
            {
                // Deserialize the asset header from the file
                AssetHeader header = WinterForge.DeserializeFromFile<AssetHeader>(file.FullName);
                if (header != null)
                {
                    assetHeaders.Add(header.Name, header);
                }
            }


            Console.WriteLine("INFO: Indexing unknown asset files");

            HashSet<string> knownHeaderPaths = new HashSet<string>(
                assetHeaders.Select(h => Path.Combine(ASSET_ROOT, h.Key + ASSET_HEADER_EXTENSION)),
                StringComparer.OrdinalIgnoreCase
            );

            FileInfo[] allFiles = new DirectoryInfo(ASSET_ROOT).GetFiles("*", SearchOption.AllDirectories)
                .Where(f => !f.Extension.Equals(ASSET_HEADER_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (FileInfo file in allFiles)
            {
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                string expectedHeaderPath = Path.Combine(ASSET_ROOT, nameWithoutExtension + ASSET_HEADER_EXTENSION);
                string extension = file.Extension.ToLowerInvariant();

                if (!knownHeaderPaths.Contains(expectedHeaderPath))
                {
                    string relativePath = Path.GetRelativePath(AppContext.BaseDirectory, file.FullName);
                    AssetHeader header = CreateHeader(nameWithoutExtension, relativePath);
                    var kvp = handerTypeMap
                        .FirstOrDefault(
                        handler => handler.Value.interestedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
                    if(kvp.Value is not null)
                    {
                        kvp.Value.InitializeNewAssetMethod.Invoke(null, [header]);
                    }
                    string headerPath = ASSET_ROOT + header.Name + ASSET_HEADER_EXTENSION;
                    WinterForge.SerializeToFile(header, headerPath);
                }
            }


            Console.WriteLine("INFO: Asset database initialized");
        }

        /// <summary>
        /// Finalizes all asset headers, ensuring they are written to disk. <br></br>
        /// Called by <see cref="Application"/> at the end of the application lifecycle.
        /// </summary>
        internal static void FinalizeHeaders()
        {
            foreach(AssetHeader header in assetHeaders.Values)
            {
                // Serialize the header to a file
                string headerPath = ASSET_ROOT + header.Name + ASSET_HEADER_EXTENSION;
                WinterForge.SerializeToFile(header, headerPath);
            }
        }

        private static AssetHeader CreateHeader(string name, string? path)
        {
            path ??= ASSET_ROOT + $"{name}{ASSET_HEADER_EXTENSION}";
            return new AssetHeader(name, path, [], new Anonymous());
        }

        public static void CreateAsset<T>(T asset, string name)
        {
            AssetHeader assetHeader = CreateHeader(name, null);
            assetHeaders.Add(assetHeader.Name, assetHeader);

            // Serialize the asset to a file
            string assetPath = ASSET_ROOT + name + ASSET_EXTENSION;
            
            // pre-create the file to ensure it exists whenever the asset is accessed
            File.Create(assetPath).Close(); 
        }

        public static AssetHeader? GetHeader(string name)
        {
            assetHeaders.TryGetValue(name, out AssetHeader? header);
            return header;
        }

        public static T Load<T>(AssetHeader assetHeader) where T : class
        {
            if (assetHeader == null)
                throw new ArgumentNullException(nameof(assetHeader), "Asset header cannot be null.");
            if (string.IsNullOrEmpty(assetHeader.Path))
                throw new InvalidOperationException("Asset path is not set in the header.");
            if (!handerTypeMap.TryGetValue(typeof(T), out HandlerEntry? entry))
                throw new InvalidOperationException($"No handler found for asset type {typeof(T).FullName}.");
            // Load the asset using the handler's load method
            return Unsafe.As<T>(entry.LoadMethod.Invoke(null, [assetHeader]));
        }

        public static T Load<T>(string name) where T : class => Load<T>(GetHeader(name));
    }
}
