using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting.Interpreting;
using SFile = System.IO.File;
using SDir = System.IO.Directory;
using WinterRose.FileManagement;

namespace WinterRose.WinterThornScripting.DefaultLibrary.FileSystem
{
    internal class File : CSharpClass
    {
        FileInfo file;
        bool overrideFile = false;

        public void Constructor(Variable[] args)
        {
            if(args.Length is not 1)
            {
                Throw();
            }

            file = new FileInfo(args[0].Value!.ToString()!);

            void Throw()
            {
                throw new WinterThornExecutionError(ThornError.InvalidParameters, "WR-638", "File class requires 1 string type parameter");
            }
        }

        public Class GetClass()
        {
            File file = new File();
            Class c = new(nameof(File), "");
            c.CSharpClass = file;

            Variable name = new("name", "The name of the file, with extension", () => file.file.Name, AccessControl.Public)
            {
                Setter = (string newName) => FileManager.Rename(file.file, newName)
            };
            Variable nameWithoutExtension = new("nameWithoutExtension", "The name of the file, without extension", () => Path.GetFileNameWithoutExtension(file.file.FullName), AccessControl.Public);
            Variable exists = new("exists", "", () => file.file.Exists, AccessControl.Public);
            Variable size = new("size", "", () => file.file.Length, AccessControl.Public);
            Variable path = new("path", "", () => file.file.FullName, AccessControl.Public);
            Variable directory = new("directory", "", () => new Directory().GetClass().CreateInstance([new Variable("", "", file.file.DirectoryName, AccessControl.Public)]), AccessControl.Public);
            Variable doOverriding = new("override", "", () => file.overrideFile, AccessControl.Public)
            {
                Setter = (bool value) => file.overrideFile = value
            };

            Function create = new("Create", "", AccessControl.Public)
            {
                CSharpFunction = () => file.file.Create().Close()
            };
            Function write = new("Write", "", AccessControl.Public)
            {
                CSharpFunction = (string content) => FileManager.Write(file.file.FullName, content, file.overrideFile)
            };
            Function writeLine = new("WriteLine", "", AccessControl.Public)
            {
                CSharpFunction = (string content) => FileManager.WriteLine(file.file.FullName, content, file.overrideFile)
            };
            Function read = new("Read", "", AccessControl.Public)
            {
                CSharpFunction = () => FileManager.Read(file.file.FullName)
            };
            Function readLine = new("ReadLine", "", AccessControl.Public)
            {
                CSharpFunction = (double lineNumber) => FileManager.ReadLine(file.file.FullName, lineNumber.FloorToInt())
            };
            Function readAllLines = new("ReadAllLines", "", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    string[] lines = FileManager.ReadAllLines(file.file.FullName).ToStringArray();
                    Collection col = new();
                    return col.GetClass().CreateInstance(lines.Select(line => new Variable("fileLine", "", line, AccessControl.Private)).ToArray());
                }
            };
            Function clear = new("Clear", "", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    file.file.Delete();
                    file.file.Create().Close();
                }
            };
            Function delete = new("Delete", "", AccessControl.Public)
            {
                CSharpFunction = () => file.file.Delete()
            };

            c.DeclareVariable(exists);
            c.DeclareVariable(size);
            c.DeclareVariable(path);
            c.DeclareVariable(directory);
            c.DeclareVariable(doOverriding);
            c.DeclareVariable(name);
            c.DeclareVariable(nameWithoutExtension);

            c.DeclareFunction(create);
            c.DeclareFunction(write);
            c.DeclareFunction(writeLine);
            c.DeclareFunction(read);
            c.DeclareFunction(delete);
            c.DeclareFunction(readLine);
            c.DeclareFunction(readAllLines);
            c.DeclareFunction(clear);

            return c;
        }
    }
}
