using EnvDTE;
using Microsoft.DiaSymReader;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
public static class IDENavigator
{
    public static void Open(Type type, string methodName = null, int relativeLine = 0)
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            return;

        string filePath = null;
        int line = 1;

        if (methodName != null)
        {
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                (filePath, line) = GetMethodSourceLocation(method);
            }
        }

        // fallback if no method found
        if (filePath == null)
        {
            var symbols = type.Assembly.GetTypes().FirstOrDefault()?.GetMethods().FirstOrDefault();
            (filePath, line) = GetMethodSourceLocation(symbols);
        }

        if (filePath != null)
        {
            IDEHelper.OpenFileAt(filePath, line + relativeLine);
        }
    }

    private static unsafe (string filePath, int line) GetMethodSourceLocation(MethodInfo method)
    {
        try
        {
            var assemblyPath = method.DeclaringType.Assembly.Location;
            var pdbPath = Path.ChangeExtension(assemblyPath, "pdb");

            using var peStream = File.OpenRead(assemblyPath);
            using var pdbStream = File.OpenRead(pdbPath);

            var peReader = new PEReader(peStream);
            var pdbReader = MetadataReaderProvider.FromPortablePdbStream(pdbStream).GetMetadataReader();

            var handle = (MethodDefinitionHandle)MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
            var methodDef = pdbReader.GetMethodDefinition(handle);

            var debugInfo = pdbReader.GetMethodDebugInformation(handle.ToDebugInformationHandle());
            var Document = pdbReader.GetDocument(debugInfo.Document);
            string fileName = pdbReader.GetString(Document.Name);

            foreach (var spHandle in debugInfo.GetSequencePoints())
            {
                var sp = debugInfo.GetSequencePoints();
                foreach(var point in sp)
                {
                    return (fileName, point.StartLine);
                }
            }
        }
        catch { }

        return (null, 1);
    }
}
