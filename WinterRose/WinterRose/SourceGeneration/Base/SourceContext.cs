using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterRose.SourceGeneration;

public sealed class SourceContext
{
    public bool ProduceFile { get; set; } = false;
    public string? FileContent { get; set; } = null;
    public List<SyntaxTree> SyntaxTrees { get; private set; } = [];

    public List<Diagnostic> Diagnostics { get; private set; } = [];
    /// <summary>
    /// Reports a diagnostic of the generation process.
    /// </summary>
    /// <param name="diagnostic"></param>
    public void ReportDiagnostic(Diagnostic diagnostic) => Diagnostics.Add(diagnostic);
    /// <summary>
    /// Adds the specified code to the context
    /// </summary>
    /// <param name="code"></param>
    public void AddCodeSource(string code)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxTrees.Add(tree);
    }

    public void AddCodeSource(SyntaxTree syntaxTree)
    {
        // Create a new compilation unit
        //var newCompilationUnit = SyntaxFactory.CompilationUnit();

        //var root = syntaxTree.GetRoot();
        //var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
        //newCompilationUnit = newCompilationUnit.AddMembers();
        //var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        //foreach (var @class in classes)
        //{
        //    newCompilationUnit = newCompilationUnit.AddMembers(@class);
        //}
        //// Create a new syntax tree with the compilation unit as the root
        //var newSyntaxTree = SyntaxFactory.SyntaxTree(newCompilationUnit);
        SyntaxTrees.Add(syntaxTree);
    }
}