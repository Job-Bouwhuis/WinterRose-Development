using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;

namespace WinterRose
{
    /// <summary>
    /// Provides extra functionality for <see cref="ZipArchive"/>.
    /// </summary>
    public static class ZipArchiveExtensions
    {
        /// <summary>
        /// Extracts all files in the <see cref="ZipArchive"/> to the specified directory.
        /// </summary>
        /// <param name="archive">The <see cref="ZipArchive"/> to extract files from.</param>
        /// <param name="destination">The directory to extract the files to.</param>
        public static void ExtractToDirectory(this ZipArchive archive, string destination)
        {
            var dir = new DirectoryInfo(destination);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string entryPath = Path.Combine(dir.FullName, entry.FullName);

                if (entry.FullName.EndsWith("/"))
                {
                    dir.CreateSubdirectory(entry.FullName);
                    continue;
                }

                entry.ExtractToFile(entryPath, true);
            }

            archive.Dispose();
        }

        /// <summary>
        /// Extracts the given file to the destination provided it is found in the <see cref="ZipArchive"/>.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="entryName"></param>
        /// <param name="destination"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void ExtractFile(this ZipArchive archive, string entryName, string destination)
        {
            var entry = archive.FindEntry(entryName) 
                ?? throw new FileNotFoundException("The specified file was not found in the archive.");
            
            entry.ExtractToFile(destination, true);
        }

        /// <summary>
        /// Adds a directory and all its contents to the <see cref="ZipArchive"/>.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="sourceDir"></param>
        /// <param name="entryName"></param>
        public static void AddDirectory(this ZipArchive archive, string sourceDir, string entryName = "")
        {
            var dir = new DirectoryInfo(sourceDir);

            foreach (var file in dir.GetFiles())
            {
                archive.CreateEntryFromFile(file.FullName, Path.Combine(entryName, file.Name));
            }

            foreach (var subDir in dir.GetDirectories())
            {
                archive.AddDirectory(subDir.FullName, Path.Combine(entryName, subDir.Name));
            }
        }

        /// <summary>
        /// Adds the specified file to the <see cref="ZipArchive"/>.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="sourceFile"></param>
        /// <param name="entryName"></param>
        public static ZipArchiveEntry AddFile(this ZipArchive archive, string sourceFile, string? entryName = null)
        {
            entryName ??= Path.GetFileName(sourceFile);
            return archive.CreateEntryFromFile(sourceFile, entryName);
        }

        /// <summary>
        /// Adds multiple files to the <see cref="ZipArchive"/>.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="sourceFiles"></param>
        /// <param name="entryName"></param>
        public static void AddFiles(this ZipArchive archive, IEnumerable<string> sourceFiles, string entryName = "")
        {
            foreach (var file in sourceFiles)
            {
                archive.CreateEntryFromFile(file, Path.Combine(entryName, Path.GetFileName(file)));
            }
        }

        /// <summary>
        /// Finds the specified entry in the <see cref="ZipArchive"/>. (does not need to be the full name)
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static ZipArchiveEntry? FindEntry(this ZipArchive archive, string entry)
        {
            // entry is at least the last part of the FullName

            return archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(entry));
        }
    }
}
