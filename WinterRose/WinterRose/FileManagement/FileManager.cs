using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Exceptions;

namespace WinterRose.FileManagement
{
    /// <summary>
    /// Allows for easy file manipulation. For suggestions please relay them to <b>TheSnowOwl</b>
    /// </summary>
    public static class FileManager
    {
        #region read/write
        /// <summary>
        /// Write a given string to a text file. include the .txt in the path. Method creates new file and directory if either does not exists.
        /// </summary>
        public static void Write(string path, string content, bool overrideFile = false)
        {
            List<string> paths = path.Split(new char[] { '/', '\\' }).ToList();
            if (paths.Count > 1)
            {
                paths.RemoveAt(paths.Count - 1);

                string directory = string.Join("/", paths);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

            }

            StreamWriter sw = new StreamWriter(path, !overrideFile);

            sw.Write(content);
            sw.Close();
            sw.Dispose();
        }
        public static void WriteBytes(string path, params byte[] bytes)
        {
            var file = File.OpenWrite(path);

            bytes.Foreach(x => file.WriteByte(x));
        }
        public static void Write(string path, StringBuilder content, bool overrideFile = false)
        {
            List<string> paths = path.Split(new char[] { '/', '\\' }).ToList();
            if (paths.Count > 1)
            {
                paths.RemoveAt(paths.Count - 1);

                string directory = string.Join("/", paths);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            StreamWriter sw = new(path, !overrideFile);

            sw.Write(content);
            sw.Close();
            sw.Dispose();
        }
        /// <summary>
        /// Write a given string to a text file on a new line. include the .txt in the path. Method creates new file and directory if either does not exists.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="overrideFile"></param>
        public static void WriteLine(string path, string content, bool overrideFile = false)
        {
            List<string> paths = path.Split('/').ToList();
            if (paths.Count > 1)
            {
                paths.RemoveAt(paths.Count - 1);

                string directory = string.Join("/", paths);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

            }

            FileStream fs;
            if (overrideFile)
                fs = File.Open(path, FileMode.Create);
            else
                fs = File.Open(path, FileMode.OpenOrCreate);
            fs.Close();
            fs.Dispose();

            StreamWriter sw = new StreamWriter(path, append: true);
            sw.WriteLine(content);
            sw.Close();
            sw.Dispose();
        }

        /// <summary>
        /// Enumerates over all lines in the file at the given path giving each line and its number every yield
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerator<(string, int)> EnumerateNumberedLines(string path)
        {
            FileOutput[] array = ReadAllLines(path);
            for (int i = 0; i < array.Length; i++)
            {
                FileOutput line = array[i];
                yield return (line, i);
            }
        }

        /// <summary>
        /// Enumerates over all lines in the file at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerator<string> EnumerateLines(string path)
        {
            FileOutput[] array = ReadAllLines(path);
            for (int i = 0; i < array.Length; i++)
            {
                FileOutput line = array[i];
                yield return line;
            }
        }

        /// <summary>
        /// Reads all text from the FileStream from beginning to end
        /// </summary>
        /// <returns>one conplete string of all text in the file</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static FileOutput Read(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Target file does not exist: " + path);

            StreamReader sr = new StreamReader(path);
            string content = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return content;
        }

        public static void CreateFileOfSize(string path, long size)
        {
            if (File.Exists(path))
                File.Delete(path);

            FileStream fs = File.Create(path);
            fs.SetLength(size);
            fs.Close();
            fs.Dispose();
        }

        private static async Task ReadChunk(string path, long startOffset, long endOffset, StringBuilder result, int i)
        {
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Seek(startOffset, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(fs);
                    long chunkSize = endOffset - startOffset + 1;
                    int bufferSize = 4096; // Adjust buffer size as needed

                    result ??= new();

                    while (chunkSize > 0)
                    {
                        int bytesToRead = (int)Math.Min(bufferSize, chunkSize);
                        char[] buffer = new char[bytesToRead];
                        int bytesRead = reader.Read(buffer, 0, bytesToRead);


                        lock (result)
                        {
                            result.Append(buffer, 0, bytesRead);
                        }

                        chunkSize -= bytesRead;
                    }

                    reader.Close();
                    reader.Dispose();
                }
            });
        }
        /// <summary>
        /// attempts to read the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>if the given file does not exist, or is already used, returns null</returns>
        public static FileOutput? TryRead(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;
                StreamReader sr = new StreamReader(path);
                string content = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                return content;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// reads the specified line
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lineNumber"></param>
        /// <returns>the string that exists on the specified line</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LineNumberTooGreatException"></exception>
        public static FileOutput ReadLine(string path, int lineNumber)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Target file does not exist");

            var lines = ReadAllLines(path);
            if (lines == null)
                throw new LineNumberTooGreatException("File does not contain any lines.");
            if (lineNumber > lines.Length)
                throw new LineNumberTooGreatException("the requested line number does not exist in the file");
            return lines[lineNumber];
        }
        /// <summary>
        /// reads all lines in the file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>a string array of all lines</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static FileOutput[] ReadAllLines(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Target file does not exist");

            StreamReader sr = new StreamReader(path);
            string? line;
            List<string> lines = new List<string>();
            while ((line = sr.ReadLine()) != null)
                lines.Add(line);
            sr.Close();
            sr.Dispose();
            return lines.ToArray().ToFileOutputArray();
        }

        /// <summary>
        /// Write a given string to a text file. include the .txt in the path. Method creates new file and directory if either does not exists.
        /// </summary>
        public static async Task WriteAsync(string path, string content, bool overrideFile = false)
        {
            await Task.Run(() => { Write(path, content, overrideFile); });
        }
        /// <summary>
        /// Write a given string to a text file on a new line. include the .txt in the path. Method creates new file and directory if either does not exists.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="overrideFile"></param>
        public static async Task WriteLineAsync(string path, string content, bool overrideFile = false)
        {
            await Task.Run(() => { WriteLine(path, content, overrideFile); });
        }
        /// <summary>
        /// Reads all text from the FileStream from beginning to end
        /// </summary>
        /// <returns>one conplete string of all text in the file</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<FileOutput> ReadAsync(string path) => await Task.Run(() => Read(path));
        /// <summary>
        /// reads the specified line
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lineNumber"></param>
        /// <returns>the string that exists on the specified line</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="LineNumberTooGreatException"></exception>
        public static async Task<FileOutput> ReadLineAsync(string path, int lineNumber) => await Task.Run(() => ReadLine(path, lineNumber));
        /// <summary>
        /// reads all lines in the file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>a string array of all lines</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<FileOutput[]> ReadAllLinesAsync(string path) => await Task.Run(() => ReadAllLines(path));
        #endregion

        /// <summary>
        /// Creates a new file with the given path and disposes its connection to it immidiately. if the full path does not exist, it creates it
        /// </summary>
        /// <param name="Path"></param>
        public static FileStream CreateOrOpenFile(string path, FileMode openingMode = FileMode.Create)
        {
            List<string> paths = path.Split(new char[] { '/', '\\' }).ToList();
            if (paths.Count > 1)
            {
                paths.RemoveAt(paths.Count - 1);

                string directory = string.Join("/", paths);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            return File.Open(path, openingMode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Cuts the given <paramref name="path"/> from the given <paramref name="from"/> and returns the result
        /// </summary>
        /// <param name="path"></param>
        /// <param name="from"></param>
        /// <returns>The path that remains after the cutting using <paramref name="from"/>. if <paramref name="from"/> does not exist in <paramref name="path"/> then this method returns <paramref name="path"/></returns>
        public static string PathFrom(string path, string from)
        {
            path = path.Replace('/', '\\');
            var pathPoints = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            int fromIndex = pathPoints.ToList().IndexOf(from);
            if (fromIndex is -1)
                return path;

            var newPathPoints = pathPoints[fromIndex..];
            var newPath = Path.Combine(newPathPoints);
            return newPath;
        }

        /// <summary>
        /// Takes the provided path and returns the full path except the last part
        /// </summary>
        /// <param name="path"></param>
        /// <returns>eg: 'C:\your\path\here' would turn into 'C:\your\path'</returns>
        public static string PathOneUp(string path)
        {
            path = path.Replace('/', '\\');
            var pathPoints = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var newPathPoints = pathPoints[..(pathPoints.Length - 1)];
            var newPath = Path.Combine(newPathPoints);
            return newPath;
        }
        /// <summary>
        /// Returns the last part of the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>eg: 'C:\your\path\here' returns 'here'</returns>
        public static string PathLast(string path)
        {
            path = path.Replace('/', '\\');
            var pathPoints = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            return pathPoints.Last();
        }

        /// <summary>
        /// Removes all the files that have the name '.DS_Store' from the given directory and all its subdirectories.<br></br>
        /// .DS_Store files are created by MacOS and are not needed for any other OS. they are hidden on MacOS but not on other OS's<br></br>
        /// I recommend calling this method before zipping a directory to remove all the .DS_Store files from the directory<br></br>
        /// Lastly, i personally do not use MacOS, thats why i made this method. please do not run this method over a directory shared with a MacOS user as it will screw up their directory preferences
        /// </summary>
        /// <param name="basePath"></param>
        public static void RemoveAll__DS_Store__Files(string basePath)
        {
            DirectoryInfo dir = new(basePath);

            foreach (var file in dir.GetFiles())
            {
                if (file.Name == ".DS_Store")
                    file.Delete();
            }
            foreach (var folder in dir.GetDirectories())
            {
                RemoveAll__DS_Store__Files(folder.FullName);
            }
        }

        public static void DirectoryDelete(DirectoryInfo directory, Action<float>? progressReport = null)
        {
            progressReport?.Invoke(0);
            var items = CountFilesAndDirectories(directory);
            int itemCount = items.directories + items.files;

            int completedItems = 0;
            P_DirectoryDelete(directory, progressReport, ref completedItems, itemCount);
            directory.Delete();
        }
        private static void P_DirectoryDelete(DirectoryInfo directory, Action<float>? progressReport, ref int completedItems, int totalItems)
        {
            foreach (var dir in directory.EnumerateDirectories())
            {
                P_DirectoryDelete(dir, progressReport, ref completedItems, totalItems);
                dir.Delete();
                completedItems++;
            }
            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
                completedItems++;
                progressReport?.Invoke((float)MathS.GetPercentage(completedItems, totalItems, 2));
            }
        }

        public static (int directories, int files) CountFilesAndDirectories(DirectoryInfo directory)
        {
            (int directories, int files) = (0, 0);

            var dirs = directory.GetDirectories();
            directories += dirs.Length;
            foreach (var dir in dirs)
            {
                var res = CountFilesAndDirectories(dir);
                directories += res.directories;
                files += res.files;
            }

            files += directory.GetFiles().Length;

            return (directories, files);
        }

        /// <summary>
        /// Zips the given directory, and places the created zip file in the given destination path
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="archiveDestinationPath"></param>
        /// <param name="compressionLevel"></param>
        /// <param name="overrideExistingFile"></param>
        public static void ZipDirectory(string sourceDirectory, string archiveDestinationPath, CompressionLevel compressionLevel = CompressionLevel.Optimal, bool overrideExistingFile = false)
        {
            if (File.Exists(archiveDestinationPath) && overrideExistingFile)
                File.Delete(archiveDestinationPath);
            ZipFile.CreateFromDirectory(sourceDirectory, archiveDestinationPath, compressionLevel, false);
        }
        /// <summary>
        /// unzips the given archive, and places the resulting directory at the given path
        /// </summary>
        /// <param name="sourceArchive"></param>
        /// <param name="destinationDirectoryPath"></param>
        /// <param name="overrideFiles"></param>
        public static void UnzipDirectory(string sourceArchive, string destinationDirectoryPath, bool overrideFiles = true)
        {
            ZipFile.ExtractToDirectory(sourceArchive, destinationDirectoryPath, overrideFiles);
        }

        /// <summary>
        /// Opens the file explorer at the given path
        /// </summary>
        /// <param name="path"></param>
        public static void OpenExplorer(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        /// <summary>
        /// Renames the file to the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        public static void Rename(FileInfo file, string newName)
        {
            string newPath = Path.Combine(PathOneUp(file.FullName), newName);
            file.MoveTo(newPath);
        }

        /// <summary>
        /// Renames the file to the specified file
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static void Rename(DirectoryInfo dir, string newName)
        {
            string newPath = Path.Combine(PathOneUp(dir.FullName), newName);
            dir.MoveTo(newPath);
        }
    }

    /// <summary>
    /// Represents the output from a read action from the SnowLibrary. this class can be directly assigned to and from a string. no casting needed.
    /// </summary>
    public struct FileOutput
    {
        private string value;

        /// <summary>
        /// Creates a new instance of the FileOutput class that contains a populated string
        /// </summary>
        /// <param name="s"></param>
        public FileOutput(string s) => value = s;

#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new instance of the FileOutput class that contains an empty string
        /// </summary>
        public FileOutput() => value = "";
#endif

        /// <summary>
        /// Removes any and all <b>\r\n</b> that contain within the FileOutput, then returns it as a string
        /// </summary>
        /// <returns></returns>
        public string RemoveNewlineCharacters()
        {
            string[] e = value.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            StringBuilder v = new();
            e.Foreach(x => v.Append(x));
            value = v.ToString();
            return value;
        }

        /// <summary>
        /// Get the output value as a string
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator string(FileOutput f) => f.value;
        /// <summary>
        /// Get a new FileOutput instance from a given string
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator FileOutput(string s) => new(s);

        public override string ToString() => this;
    }

    /// <summary>
    /// A class containing helpfull methods for FileOutput handling
    /// </summary>
    public static class FileOutputExtensions
    {
        /// <summary>
        /// Removes the Read anomalies from every FileOutput within the array
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static FileOutput[] RemoveReadAnomalies(this FileOutput[] values)
        {
            return Array.Empty<FileOutput>();
        }
        /// <summary>
        /// turns the complete FileOutput array into a array of strings
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string[] ToStringArray(this FileOutput[]? values)
        {
            if (values is null)
                return Array.Empty<string>();
            List<string> s = new List<string>();
            values.Foreach(x => s.Add(x));
            return s.ToArray();
        }
        /// <summary>
        /// Creates an array of FileOutput classes from an array of strings
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static FileOutput[] ToFileOutputArray(this string[]? values)
        {
            if (values is null)
                return Array.Empty<FileOutput>();
            List<FileOutput> s = new List<FileOutput>();
            values.Foreach(x => s.Add(x));
            return s.ToArray();
        }
    }

    /// <summary>
    /// Gets thrown when reading for a specific line which does not exist in the given file
    /// </summary>
    [Serializable]
    internal class LineNumberTooGreatException : WinterException
    {
        public LineNumberTooGreatException()
        {
        }

        public LineNumberTooGreatException(string? message) : base(message)
        {
        }

        public LineNumberTooGreatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
