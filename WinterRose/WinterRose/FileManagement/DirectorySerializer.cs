using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Encryption;
using System.Diagnostics;
using WinterRose.WIP.TestClasses;
using System.Collections.Concurrent;
using WinterRose.AnonymousTypes;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.FileManagement
{
    /// <summary>
    /// Provides methods to serialize or deserialize a directory
    /// <br></br> NEEDS WORK: <br></br>
    /// Needs to be have the old serialization methods integrated so that ther is a faster less secure method, and a slower more secure method. <br></br>
    /// The faster method should be the old method, and the slower method should be the new method. <br></br>
    /// This class is now the new method. <br></br>
    /// The old method can be found in the Unity solution folder under the name "FileManagment/DirectorySerializer.cs"
    /// </summary>
    public static class DirectorySerializer
    {
        /// <summary>
        /// serializes the given directory
        /// </summary>
        /// <param name="pathToSerialize">path to the directory to be serialized</param>
        /// <returns>a string containing data of the serialized directory</returns>
        public static string SerializeDirectory(string pathToSerialize, DirectorySerializerSettings? settings = null)
        {
            settings ??= new();

            if (!Directory.Exists(pathToSerialize) && File.Exists(pathToSerialize))
            {
                return SerializeOneFile(pathToSerialize, new(FileManager.PathOneUp(pathToSerialize)), settings.Value);
            }

            if (settings.Value.UseSingleFile)
            {
                return SerializeToSingleFile(pathToSerialize, new(pathToSerialize), settings.Value);
            }
            else
            {
                DirectoryInfo info = new(pathToSerialize);
                DirectoryData directory = new();
                var s = SerializePerFile(pathToSerialize, info.Parent.Name, new(FileManager.PathOneUp(pathToSerialize)), settings.Value);
                settings.Value.Progress?.Invoke($"Done.\nEncrypted folder to '{s}'");
                return s;
            }
        }

        public static string DeserializeDirectory(string archivePath, DirectorySerializerSettings? settings = null)
        {
            settings ??= new();

                if (settings.Value.DestinationDirectory is null)
                {
                    string path = Path.Combine(FileManager.PathOneUp(archivePath), "Decrypted directory archive");
                    if (Directory.Exists(path))
                        FileManager.DirectoryDelete(new(path), progress => settings.Value.Progress?.Invoke(progress.ToString() + "%"));

                    var set = settings.Value;
                    set.DestinationDirectory = Directory.CreateDirectory(path);
                    settings = set;
                }

                if (settings.Value.UseSingleFile)
                    return DeserializeFromSingleFile(archivePath, settings.Value);
                else
                    return DeserializePerFile(new(archivePath), settings.Value.DestinationDirectory, settings.Value);
        }
        private static string SerializeToSingleFile(string PathToSerialize, DirectoryInfo parentDir, DirectorySerializerSettings settings)
        {
            SerializedDirectory dir = new(new DirectoryInfo(PathToSerialize).Attributes.HasFlag(FileAttributes.Hidden));

            string a = Path.GetFullPath(PathToSerialize);
            if (PathToSerialize.Contains("../"))
                PathToSerialize = Path.GetDirectoryName(a);

            dir.directoryName = WinterUtils.GetDirectoryName(PathToSerialize);

            var directories = Directory.GetDirectories(PathToSerialize);
            var files = Directory.GetFiles(PathToSerialize);

            foreach (var dr in directories)
            {
                dir.directories.Add(dir.directories.NextAvalible(), SerializedDirectory.SerializeDirectory(dr));
            }

            foreach (var file in files)
            {
                settings.Progress?.Invoke(file);
                dir.files.Add(dir.files.NextAvalible(), SerializedFile.SerializeFile(file));
            }

            string serialized = WinterForge.SerializeToString(dir);
            return serialized;
        }

        private static string SerializePerFile(string pathToSerialize, string relativeFolder, DirectoryInfo parentFolder, DirectorySerializerSettings settings)
        {
            if (FileManager.PathLast(pathToSerialize) == ".git")
            {
                settings.Progress?.Invoke("Skipping .git folder");
                return "";
            }
            //string path = FileManager.PathOneUp(pathToSerialize);
            DirectoryInfo directoryInfo = new(pathToSerialize);

            if (!directoryInfo.Exists)
                return $"No directory at the requested path: '{pathToSerialize}'";

            string encryptedDirName = ByteEncryptor.Encrypt(directoryInfo.Name, settings.ByteEncryptorSettings);
            string DecryptedDirName = ByteEncryptor.Decrypt(encryptedDirName, settings.ByteEncryptorSettings);
            encryptedDirName = encryptedDirName.Replace('/', '[').Replace('\\', ']');
            if (parentFolder is not null)
                encryptedDirName = Path.Combine(parentFolder.FullName, encryptedDirName);
            if (Directory.Exists(encryptedDirName))
            {
                settings.Progress?.Invoke("Existing encrypted archive exists. Deleting it...");
                FileManager.DirectoryDelete(new(encryptedDirName), progress =>
                {
                    settings.Progress?.Invoke(progress.ToString() + "%");
                });
            }
            DirectoryInfo newDir = Directory.CreateDirectory(encryptedDirName);
            if (directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                newDir.Attributes |= FileAttributes.Hidden;

            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                SerializePerFile(dir.FullName, relativeFolder, newDir, settings);
            }

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                settings.Progress?.Invoke(FileManager.PathFrom(file.FullName, relativeFolder));
                SerializeOneFile(file.FullName, newDir, settings);
            }

            return newDir.FullName;
        }
        private static string SerializeOneFile(string filePath, DirectoryInfo archiveDestination, DirectorySerializerSettings settings)
        {
            bool doReport = settings.StrongEncryptionSettings.ReportEvery != -1;

            Action<float>? progress = progress =>
            {
                settings.Progress?.Invoke($"Decrypt file: {progress}%");
            };
            if (!doReport)
                progress = null;

            string fileName = Path.GetFileName(filePath);
            var encryptedName = ByteEncryptor.Encrypt(fileName, settings.ByteEncryptorSettings);
            encryptedName = encryptedName.Replace('/', '[').Replace('\\', ']').Replace('*', '}');

            var data = WinterForge.SerializeToString(SerializedFile.SerializeFile(filePath));

            string encrypted;
            if (settings.UseStrongerEncryption)
            {
                var s = settings.StrongEncryptionSettings;
                encrypted = Encryptor.Encrypt(data, s.PublcPassword, s.PrivatePassword, s.ShiftAmount, progress, s.ReportEvery);
            }
            else
            {
                encrypted = ByteEncryptor.Encrypt(data, settings.ByteEncryptorSettings);
            }

            if (settings.UseSingleFile)
            {
                return encrypted;
            }
            else
            {
                string dir = Path.GetDirectoryName(filePath);
                if (archiveDestination is not null)
                    dir = archiveDestination.FullName;
                string encryptedFilePath = Path.Combine(dir, encryptedName + ".WinterArchive");

                StringBuilder fileContent = new();
                fileContent.Append("-c=sf <===> ");
                fileContent.Append(encrypted);
                FileManager.Write(encryptedFilePath, fileContent, true);

                if (settings.UseWindowsEncryption)
                    new FileInfo(encryptedFilePath).Encrypt();
                return "file written to " + encryptedFilePath;
            }
        }
        /// <summary>
        /// Deserialized the given directory
        /// </summary>
        /// <returns>true if the operation was succesfull, false if it wasnt</returns>

        private static string DeserializePerFile(DirectoryInfo archive, DirectoryInfo newDir, DirectorySerializerSettings settings)
        {
            if (!archive.Exists)
                return "Fo archive found";

            var name = archive.Name;
            name = name.Replace('[', '/').Replace(']', '\\').Replace('}', '*');

            string dirName = ByteEncryptor.Decrypt(name, settings.ByteEncryptorSettings);

            DirectoryInfo nextDir;
            nextDir = newDir.CreateSubdirectory(dirName);

            foreach (var dir in archive.EnumerateDirectories())
                DeserializePerFile(dir, nextDir, settings);

            foreach (var file in archive.EnumerateFiles())
                DeserializeSingleFile(file, nextDir, settings);

            return newDir.FullName;
        }

        private static void DeserializeSingleFile(FileInfo archiveFile, DirectoryInfo destination, DirectorySerializerSettings settings)
        {
            if (settings.UseWindowsEncryption)
                archiveFile.Decrypt();

            string encryptedContent;

            encryptedContent = ((string)FileManager.Read(archiveFile.FullName))[12..];
            bool doReport = settings.StrongEncryptionSettings.ReportEvery != -1;

            Action<float>? progress = progress =>
            {
                settings.Progress?.Invoke($"Decrypt file: {progress}%");
            };
            if (!doReport)
                progress = null;

            string fileContent;
            if (settings.UseStrongerEncryption)
            {
                var s = settings.StrongEncryptionSettings;
                fileContent = Encryptor.Decrypt(encryptedContent, s.PublcPassword, s.PrivatePassword, s.ShiftAmount, progress, s.ReportEvery);
            }
            else
            {
                fileContent = ByteEncryptor.Decrypt(encryptedContent, settings.ByteEncryptorSettings);
            }

            string path = Path.Combine(destination.FullName);

            SerializedFile file = WinterForge.DeserializeFromString<SerializedFile>(fileContent);
            settings.Progress?.Invoke($"{path}\\{file.fileName}{file.fileExtention}");
            SerializedFile.DeserializeFile(file, path);
        }

        private static string DeserializeFromSingleFile(string archivePath, DirectorySerializerSettings settings)
        {
            if (!File.Exists(archivePath)) return "No archive found";

            SerializedDirectory dir = WinterForge.DeserializeFromFile<SerializedDirectory>(archivePath);
            string directory = $"{settings.DestinationDirectory!.FullName}\\{dir.directoryName}";
            if (!Directory.Exists(directory))
                dir.dirInfo = Directory.CreateDirectory(directory);

            List<Task> directories = [];
            List<Task> files = [];

            foreach (var d in dir.directories.Values)
            {
                string dd = $"{directory}\\{d.directoryName}";
                if (!Directory.Exists(dd))
                    d.dirInfo = Directory.CreateDirectory(dd);
                SerializedDirectory.DeserializeDirectory(d, dd);
            }
            foreach (var d in dir.files.Values)
            {
                string filePath = Path.Combine(dir.dirInfo.FullName, d.fileName + d.fileExtention);
                settings.Progress?.Invoke(filePath);
                SerializedFile.DeserializeFile(d, directory);
            }

            return settings.DestinationDirectory!.FullName;
        }
    }

    [DebuggerDisplay("{directoryName}")]
    internal class SerializedDirectory
    {
        public string directoryName;
        public DirectoryInfo dirInfo
        {
            get => new(directoryName);
            set => directoryName = value.Name;
        }

        public Dictionary<int, SerializedDirectory> directories;
        public Dictionary<int, SerializedFile> files;
        public bool isHidden = false;

        public SerializedDirectory() : this(false) { }
        public SerializedDirectory(bool isHidden)
        {
            directories = new();
            files = new();
            directoryName = "";
            this.isHidden = isHidden;
        }
        internal static SerializedDirectory SerializeDirectory(string path)
        {
            SerializedDirectory result = new(new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.Hidden));

            result.directoryName = WinterUtils.GetDirectoryName(path);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            foreach (var x in directories)
                result.directories.Add(result.directories.NextAvalible(), SerializeDirectory(x));
            foreach (var x in files)
                result.files.Add(result.files.NextAvalible(), SerializedFile.SerializeFile(x));
            return result;
        }
        internal static bool DeserializeDirectory(SerializedDirectory data, string destination)
        {
            foreach (var dir in data.directories.Values)
            {
                string dirPath = $"{destination}\\{dir.directoryName}";
                DirectoryInfo thisInfo;
                if (!Directory.Exists(dirPath))
                    thisInfo = Directory.CreateDirectory(dirPath);
                else
                    thisInfo = new DirectoryInfo(dirPath);

                if (data.isHidden)
                    thisInfo.Attributes |= FileAttributes.Hidden;
                DeserializeDirectory(dir, dirPath);
            }
            foreach (var file in data.files.Values)
            {
                SerializedFile.DeserializeFile(file, destination);
            }
            return true;
        }

        public override string ToString()
        {
            return directoryName;
        }
    }

    /// <summary>
    /// represents a serialized file
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    internal class SerializedFile
    {
        public string fileName;
        public string fileExtention;
        public string content;
        public bool isHidden;

        /// <summary>
        /// creates a new instance of the SerializedFile class with the given properties
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileExtention"></param>
        /// <param name="content"></param>
        public SerializedFile(string fileName, string fileExtention, string content, bool isHidden)
        {
            this.fileName = fileName;
            this.fileExtention = fileExtention;
            this.content = content;
            this.isHidden = isHidden;
        }
        /// <summary>
        /// creates an empty intance of the SerializedFile class
        /// </summary>
        public SerializedFile()
        {
            fileName = "";
            fileExtention = "";
            content = "";
        }

        /// <summary>
        /// serializes the file at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the serialized file</returns>
        public static SerializedFile SerializeFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            var n = new SerializedFile(
                Path.GetFileNameWithoutExtension(path),
                Path.GetExtension(path),
                Convert.ToBase64String(bytes),
                File.GetAttributes(path).HasFlag(FileAttributes.Hidden));

            return n;
        }

        public static bool DeserializeFile(SerializedFile data, string destination)
        {
            string path = $"{destination}\\{data.fileName}{data.fileExtention}";
            var content = Convert.FromBase64String(data.content);
            File.WriteAllBytes(path, content);
            FileInfo file = new(path);
            if (data.isHidden)
                file.Attributes |= FileAttributes.Hidden;
            return true;
        }

        public override string ToString()
        {
            return $"{fileName}{fileExtention}";
        }
    }
}
