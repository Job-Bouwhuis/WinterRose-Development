using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.WinterThornScripting.Interpreting;
using SDir = System.IO.Directory;

namespace WinterRose.WinterThornScripting.DefaultLibrary.FileSystem;

internal class Directory : CSharpClass
{
    DirectoryInfo dir;

    public void Constructor(Variable[] args)
    {
        if (args.Length is not 1)
        {
            Throw();
        }

        dir = new DirectoryInfo(args[0].Value!.ToString()!);

        void Throw()
        {
            throw new WinterThornExecutionError(ThornError.InvalidParameters, "WR-638", "Directory class requires 1 string type parameter");
        }
    }

    public Class GetClass()
    {
        Directory dir = new Directory();
        Class c = new(nameof(Directory), "Represents a directory in the file system and allows interaction with it.");
        c.CSharpClass = dir;

        Variable name = new("name", "The name of the file, with extension", () => dir.dir.Name, AccessControl.Public)
        {
            Setter = (string newName) => FileManager.Rename(dir.dir, newName)
        };
        Variable exists = new("exists", "Whether or not the directory exists.", () => dir.dir.Exists, AccessControl.Public);
        Variable path = new("path", "The path of the directory.", () => dir.dir.FullName, AccessControl.Public);
        Variable parent = new("parent", "The parent directory.", () =>
        {
            if (dir.dir.Parent != null)
                return new Directory().GetClass().CreateInstance([new Variable("", "", dir.dir.Parent.FullName, AccessControl.Public)]);
            return null;
        }, AccessControl.Public);
        Variable fileCount = new("fileCount", "The number of files in this directory.", () => dir.dir.GetFiles().Count(), AccessControl.Public);
        Variable dirCount = new("directoryCount", "The number of sub-directories in this directory.", () => dir.dir.GetDirectories().Count(), AccessControl.Public);
        Variable files = new Variable("files", "A collection of files that are in this directory.", () =>
        {
            List<Variable> fileVars = [];
            foreach (var file in dir.dir.GetFiles())
            {
                Class fc = new File().GetClass().CreateInstance([new Variable("", "", file.FullName)]);
                fileVars.Add(new(file.FullName, "", fc));
            }
            Class c = new Collection().GetClass().CreateInstance([.. fileVars]);
            return c;
        }, AccessControl.Public);
        Variable dirs = new("directories", "A collection of sub-directores that are in this directory.", () =>
        {
            List<Variable> dirVars = [];
            foreach (var dir in dir.dir.GetDirectories())
            {
                Class dc = new Directory().GetClass().CreateInstance([new Variable("", "", dir.FullName)]);
                dirVars.Add(new(dir.FullName, "", dc));
            }
            Class c = new Collection().GetClass().CreateInstance([.. dirVars]);
            return c;
        }, AccessControl.Public);

        Function create = new Function("Create", "Creates the directory. Does nothing if the directory already exists.", AccessControl.Public)
        {
            CSharpFunction = () => dir.dir.Create()
        };
        Function createSub = new Function("CreateSubdirectory", "Creates a sub-directory within this one. Does nothing if the directory already exists.", AccessControl.Public)
        {
            CSharpFunction = (string name) =>
            {
                var info = dir.dir.CreateSubdirectory(name);
                Class dc = new Directory().GetClass().CreateInstance([new Variable("", "", info.FullName)]);
                return dc;
            }
        };
        Function delete = new Function("Delete", "Deletes the directory if it is empty.", AccessControl.Public)
        {
            CSharpFunction = () => dir.dir.Delete()
        };
        Function deleteRecursive = new Function("DeleteRecursive", "Deletes the directory recursively.", AccessControl.Public)
        {
            CSharpFunction = () => dir.dir.Delete(true)
        };

        c.DeclareVariable(exists);
        c.DeclareVariable(path);
        c.DeclareVariable(parent);
        c.DeclareVariable(fileCount);
        c.DeclareVariable(dirCount);
        c.DeclareVariable(files);
        c.DeclareVariable(dirs);

        c.DeclareFunction(create);
        c.DeclareFunction(createSub);
        c.DeclareFunction(delete);
        c.DeclareFunction(deleteRecursive);
        return c;
    }
}
