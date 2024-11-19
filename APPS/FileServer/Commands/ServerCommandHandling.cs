using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.FileServer.Common.Models;
using WinterRose.Networking.TCP;
using WinterRose.Serialization;
namespace WinterRose.FileServer
{
    public class ServerCommandHandling
    {
        private static string root => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PackageDatabase";

        public static async Task HandleCommand(string command, TCPServer server, TCPClientInfo sender, Guid responseID)
        {
            await Task.Yield();
            command = command.Replace("root", root);
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            // command structure: *command*data

            // *getdir* - get directory listing of the path specified in the data. if data is root, get root directory.
            if (command.StartsWith("*getdir*"))
            {
                string data = command[8..];
                if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                {
                    Respond(data + " is not a valid path");
                    return;
                }
                else
                {
                    Respond(DirectoryDataPacker.PackDirectoryData(data == "root" ? root : data));
                }
            }

            if (command.StartsWith("*download*"))
            {
                try
                {
                    string data = command[10..];
                    if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                    {
                        Respond(data + " is not a valid path");
                        return;
                    }
                    else
                    {
                        string path = data == "root" ? root : data;
                        string fileData = Convert.ToBase64String(File.ReadAllBytes(path));
                        long length = fileData.Length;

                        Respond(fileData);
                    }

                }
                catch (Exception ex)
                {
                    Respond("Fault: " + ex.ToString());
                }

            }

            if (command.StartsWith("*mkdir*"))
            {
                string data = command[7..];
                if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                {
                    Respond(data + " is not a valid path");
                    return;
                }
                else
                {
                    string path = data == "root" ? root : data;
                    var dir = Directory.CreateDirectory(path);

                    Respond("OK");
                }
            }
            if (command.StartsWith("*filecount*"))
            {
                // *dirfilecount*data
                string data = command[11..];
                if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                {
                    Respond(data + " is not a valid path");
                    return;
                }
                else
                {
                    string path = data == "root" ? root : data;

                    int fileCount = Count(new DirectoryInfo(path));

                    Respond(fileCount.ToString());

                    int Count(DirectoryInfo info) => info.GetFiles().Length + info.GetDirectories().Sum(dir => Count(dir));
                }
            }

            if (command.StartsWith("*upload*"))
            {
                try
                {
                    // format: *upload*path*filerawdata
                    string data = command[8..];

                    string[] split = data.Split('*');

                    if (split.Length != 2)
                    {
                        Respond("Invalid upload format");
                        return;
                    }

                    string path = split[0];

                    string fileData = split[1];

                    if (string.IsNullOrWhiteSpace(path) || !IsValidPath(path))
                    {
                        Respond(path + " is not a valid path");
                        return;
                    }

                    if (!Directory.Exists(FileManager.PathOneUp(path)))
                        Directory.CreateDirectory(FileManager.PathOneUp(path));

                    File.WriteAllBytes(path, Convert.FromBase64String(fileData));
                    Console.WriteLine("Uplaoded a file to " + path);

                    Respond("File uploaded successfully");
                }
                catch (Exception e)
                {
                    Respond("Failed: " + e.Message);
                }
            }

            if (command.StartsWith("*fileversion*"))
            {
                string data = command[13..];

                if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                {
                    Respond(data + " is not a valid path");
                    return;
                }
                else
                {
                    string path = data == "root" ? root : data;
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
                    VersionWrapper version = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
                    string serialized = SnowSerializer.Serialize(version, new SerializerSettings() { IncludeType = false }).Result;
                    Respond(serialized);
                }
            }

            if (command.StartsWith("*delete*"))
            {
                string data = command[8..];

                if (string.IsNullOrWhiteSpace(data) || !IsValidPath(data))
                {
                    Respond(data + " is not a valid path");
                    return;
                }
                else
                {
                    string path = data == "root" ? root : data;
                    File.Delete(path);
                    Respond(path + " has been deleted.");
                }
            }

            void Respond(string response)
            {
                int responseLength = response.Length;
                server.SendResponse(response, sender.Client, responseID);
            }
        }

        private static bool IsValidPath(string data)
        {
            if (data.StartsWith(root) || data.StartsWith("root"))
                return true;

            foreach (var dir in Directory.GetDirectories(root))
            {
                if (data.StartsWith(dir))
                    return true;
            }

            foreach (var file in Directory.GetFiles(root))
            {
                if (data.StartsWith(file))
                    return true;
            }

            return false;
        }
    }
}
