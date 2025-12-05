using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;
using WinterRose.Recordium;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeWarden.AssetPipeline
{
    public static class Assets
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SaveMethodH">saves based on header</param>
        /// <param name="SaveMethodS">saves based on asset name</param>
        /// <param name="LoadMethod"></param>
        /// <param name="InitializeNewAssetMethod"></param>
        /// <param name="interestedExtensions"></param>
        private record HandlerEntry(MethodInfo SaveMethodH, MethodInfo SaveMethodS, MethodInfo LoadMethod, MethodInfo InitializeNewAssetMethod, string[] interestedExtensions);
        private static Log log = new Log("Assets");
        private static Dictionary<string, AssetHeader> assetHeaders = [];
        private static Dictionary<Type, HandlerEntry> handerTypeMap = [];
        private const string ASSET_HEADER_EXTENSION = ".fwah";
        private const string ASSET_EXTENSION = ".fwasset";
        private const string ASSET_ROOT = "Assets/";

        internal static void BuildAssetIndexes()
        {
            log.Info("Registering asset handlers");

            Type[] types = TypeWorker.FindTypesWithInterface(typeof(IAssetHandler<>));

            foreach (Type type in types)
            {
                Type[] interfaces = type.GetInterfaces();
                foreach (Type interf in interfaces.Where(i => i.Name.Contains("IAssetHandler")))
                {
                    Type assetType = interf.GetGenericArguments()[0];
                    MethodInfo? saveMethodH = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .FirstOrDefault(m =>
                            m.Name == "SaveAsset" &&
                            m.GetParameters().Length == 2 &&
                            m.GetParameters()[0].ParameterType == typeof(AssetHeader) &&
                            m.GetParameters()[1].ParameterType == assetType &&
                            m.ReturnType == typeof(bool));

                    MethodInfo? saveMethodS = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .FirstOrDefault(m =>
                            m.Name == "SaveAsset" &&
                            m.GetParameters().Length == 2 &&
                            m.GetParameters()[0].ParameterType == typeof(string) &&
                            m.GetParameters()[1].ParameterType == assetType &&
                            m.ReturnType == typeof(bool));

                    MethodInfo? loadMethod = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .FirstOrDefault(m =>
                            m.Name.EndsWith("LoadAsset") &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(AssetHeader) &&
                            m.ReturnType == assetType);

                    MethodInfo? initializeNewAssetMethod = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .FirstOrDefault(m =>
                            m.Name == "InitializeNewAsset" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(AssetHeader) &&
                            m.ReturnType == typeof(bool));

                    if (saveMethodH == null 
                        || loadMethod == null 
                        || saveMethodS == null 
                        || initializeNewAssetMethod == null)
                        throw new InvalidOperationException($"Asset handler {type.FullName} does not have required methods.");

                    var rh = new ReflectionHelper(type);
                    var mem = rh.GetMember("InterestedInExtensions");
                    var interestedExtensions = (string[])mem.GetValue();
                    handerTypeMap[assetType] = new HandlerEntry(
                        saveMethodH,
                        saveMethodS,
                        loadMethod,
                        initializeNewAssetMethod,
                        interestedExtensions
                        );
                }
            }

            if (!System.IO.Directory.Exists(ASSET_ROOT))
            {
                log.Warning("Asset folder does not exist. No assets will be loaded");
                return;
            }

            FileInfo[] headerFiles = new DirectoryInfo(ASSET_ROOT).GetFiles($"*{ASSET_HEADER_EXTENSION}");

            log.Info("Registering asset headers");

            foreach (FileInfo file in headerFiles)
            {
                // Deserialize the asset header from the file
                AssetHeader header = WinterForge.DeserializeFromFile<AssetHeader>(file.FullName);
                if (header.IsValid)
                {
                    assetHeaders.Add(header.Name, header);
                }
            }


            log.Info("Indexing unknown asset files");

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
                    if (kvp.Value is not null)
                        kvp.Value.InitializeNewAssetMethod.Invoke(null, [header]);
                    string headerPath = ASSET_ROOT + header.Name + ASSET_HEADER_EXTENSION;
                    WinterForge.SerializeToFile(header, headerPath);
                    assetHeaders.Add(header.Name, header);
                }
            }

            log.Info("Asset database initialized");
        }

        /// <summary>
        /// Finalizes all asset headers, ensuring they are written to disk. <br></br>
        /// Called by <see cref="Application"/> at the end of the application lifecycle.
        /// </summary>
        internal static void FinalizeHeaders()
        {
            foreach (AssetHeader header in assetHeaders.Values)
            {
                // Serialize the header to a file
                string headerPath = ASSET_ROOT + header.Name + ASSET_HEADER_EXTENSION;
                WinterForge.SerializeToFile(header, headerPath);
            }
        }

        private static AssetHeader CreateHeader(string name, string? path)
        {
            path ??= ASSET_ROOT + $"{name}{ASSET_HEADER_EXTENSION}";
            if (!System.IO.Directory.Exists(ASSET_ROOT))
                System.IO.Directory.CreateDirectory(ASSET_ROOT);
            return new AssetHeader(name, path, [], new Anonymous());
        }

        public static AssetHeader CreateAsset<T>(T asset, string name)
        {
            if (!System.IO.Directory.Exists(ASSET_ROOT))
                System.IO.Directory.CreateDirectory(ASSET_ROOT);

            AssetHeader assetHeader = CreateHeader(name, null);
            assetHeaders.Add(assetHeader.Name, assetHeader);

            string assetPath = ASSET_ROOT + name + ASSET_EXTENSION;
            assetHeader.Path = assetPath;

            File.Create(assetPath).Close();

            Assets.GetHandler<T>().SaveMethodH.Invoke(null, [assetHeader, asset]);
            return assetHeader;
        }

        private static HandlerEntry GetHandler<T>()
        {
            if (handerTypeMap.TryGetValue(typeof(T), out HandlerEntry handler))
                return handler;

            throw new AssetException($"Handler for type {typeof(T).FullName} not found");
        }

        public static AssetHeader GetHeader(string name)
        {
            assetHeaders.TryGetValue(name, out AssetHeader header);
            if (!header.IsValid)
                throw new AssetNotFoundException(name);
            return header;
        }

        public static bool Exists(string name) => assetHeaders.ContainsKey(name);

        public static T Load<T>(AssetHeader assetHeader) where T : class
        {
            if (string.IsNullOrEmpty(assetHeader.Path))
                throw new InvalidOperationException("Asset path is not set in the header.");
            if (!handerTypeMap.TryGetValue(typeof(T), out HandlerEntry? entry))
                throw new InvalidOperationException($"No handler found for asset type {typeof(T).FullName}.");

            object? assetResult;
            try
            {
                assetResult = entry.LoadMethod.Invoke(null, [assetHeader]);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is not null)
                    throw e.InnerException;
                throw;
            }

            if (assetResult is null)
                return null;

            return Unsafe.As<T>(assetResult);
        }

        public static T Load<T>(string name) where T : class => Load<T>(GetHeader(name));
        public static void Save<T>(string name, T accounts)
        {
            if(!Exists(name))
            {
                CreateAsset(accounts, name);
            }
            else
            {
                var handler = GetHandler<T>();
                handler.SaveMethodS.Invoke(null, [name, accounts]);
            }
        }
    }
}
