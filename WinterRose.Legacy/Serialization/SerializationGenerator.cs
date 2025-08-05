//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.CSharp;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using WinterRose.Reflection;
//using WinterRose.SourceGeneration;
//using WinterRose.Legacy.Serialization.Things;

//namespace WinterRose.Legacy.Serialization
//{
//    public sealed class SerializationGenerator : ICodeGenerator
//    {
//        private List<string> namespacesToImport = new();
//        /// <summary>
//        /// A list of additional types to generate serializers for, use this for types that are not in your control, such as types from another library. 
//        /// <br></br> Make sure to add the type to this list before calling <see cref="Generate"/> or it will not be included. <br></br>
//        /// For your own types, use the <see cref="GenerateSerializerAttribute"/> instead.
//        /// </summary>
//        public static List<Type> AdditionalTypes { get; } = new();
//        /// <summary>
//        /// Exists for debugging purposes, if true, the generated sources will be emitted to the current directorh: <see cref="Environment.CurrentDirectory"/>/Generated. Defaults to false.
//        /// </summary>
//        public static bool DoEmitFiles { get; set; } = false;
//        public void Initialize(SourceContext context)
//        {
//            context.ProduceFile = DoEmitFiles;
//        }
//        public void Generate(SourceContext context)
//        {
//            // Analyze types and generate custom serialization code
//            foreach (var type in TypeWorker.FindTypesWithAttribute<GenerateSerializerAttribute>())
//            {
//                // Generate serialization code for 'type' using SyntaxFactory
//                context.AddCodeSource(GenerateSerializationCodeForType(type));
//                namespacesToImport.Clear();
//            }
//        }
//        private SyntaxTree GenerateSerializationCodeForType(Type type)
//        {
//            //create parameter list for the serialize method to take in an object named 'obj' and a SerializerSettings named 'settings'
//            ParameterListSyntax serializeMethodParameters = SyntaxFactory.ParameterList()
//                .AddParameters(
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("obj"))
//                         .WithType(SyntaxFactory.ParseTypeName(type.FullName)),
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("settings"))
//                         .WithType(SyntaxFactory.ParseTypeName("SerializerSettings")),
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("depth"))
//                         .WithType(SyntaxFactory.ParseTypeName("Int32")));

//            ParameterListSyntax deserializeMethodParameters = SyntaxFactory.ParameterList()
//                .AddParameters(
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("data"))
//                       .WithType(SyntaxFactory.ParseTypeName("string")),
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("settings"))
//                         .WithType(SyntaxFactory.ParseTypeName("SerializerSettings")),
//                     SyntaxFactory.Parameter(SyntaxFactory.Identifier("depth"))
//                       .WithType(SyntaxFactory.ParseTypeName("Int32")));

//            // serialize method
//            MethodDeclarationSyntax serializeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("StringBuilder"), "Serialize")
//                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
//                .WithParameterList(serializeMethodParameters)
//                .WithBody(CreateSerializerBody(type));

//            MethodDeclarationSyntax DeserializeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(type.Name), "Deserialize")
//                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
//                .WithParameterList(deserializeMethodParameters)
//                .WithBody(CreateDeserializeBody(type));

//            var arg1 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("SerializationGenerator"));
//            var arg2 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("0.0.0.1"));
//            var arg3 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(type.FullName));

//            var argList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(new SyntaxNodeOrToken[]
//                {
//                    SyntaxFactory.AttributeArgument(arg1),
//                    SyntaxFactory.Token(SyntaxKind.CommaToken),
//                    SyntaxFactory.AttributeArgument(arg2),
//                    SyntaxFactory.Token(SyntaxKind.CommaToken),
//                    SyntaxFactory.AttributeArgument(arg3)
//                }));

//            // create an attribute to go on the class that will be generated
//            AttributeSyntax generatedSerializerAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("GeneratedAttribute"))
//                .WithArgumentList(argList);

//            // Construct the full class or structure syntax with serialization methods and attributes
//            ClassDeclarationSyntax serializationClass = SyntaxFactory.ClassDeclaration($"GeneratedSerializer_{type.Name}")
//                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
//                .AddMembers(serializeMethod, DeserializeMethod)
//                .NormalizeWhitespace()
//                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(generatedSerializerAttribute)));

//            NamespaceDeclarationSyntax serializationNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("WinterRose.SourceGenerated.Serializer_" + GetCodeFriendlyTypeAndNamespace(type)))
//                .AddMembers(serializationClass)
//                .NormalizeWhitespace();

//            // add the using namespaces that are required in this specific generated class to a list
//            if (type.Namespace is not null && !namespacesToImport.Contains(type.Namespace))
//                namespacesToImport.Add(type.Namespace);
//            if (!namespacesToImport.Contains("System.Text"))
//                namespacesToImport.Add("System.Text");
//            if (!namespacesToImport.Contains("WinterRose.Serialization"))
//                namespacesToImport.Add("WinterRose.Serialization");
//            if (!namespacesToImport.Contains("WinterRose.SourceGeneration"))
//                namespacesToImport.Add("WinterRose.SourceGeneration");
//            if (!namespacesToImport.Contains("System"))
//                namespacesToImport.Add("System");

//            // fetch the using directives from the list and add them to the namespace
//            var usingDirectives = new List<UsingDirectiveSyntax>();
//            foreach (var ns in namespacesToImport)
//                usingDirectives.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)));

//            // submit the using directives to the namespace
//            serializationNamespace = serializationNamespace.AddUsings([.. usingDirectives]);

//            // create the compilation unit and add the namespace to it
//            var newCompilationUnit = SyntaxFactory.CompilationUnit().AddMembers(serializationNamespace)
//                .NormalizeWhitespace();

//            // make a syntax tree from the compilation unit and return it
//            return SyntaxFactory.SyntaxTree(newCompilationUnit);
//        }
//        private BlockSyntax CreateDeserializeBody(Type type)
//        {
//            // check if the type has an empty constructor
//            bool hasEmptyConstructor = type.GetConstructor(Type.EmptyTypes) is not null;
//            Assert.True(hasEmptyConstructor, $"Type {type.FullName} does not have an empty constructor, this is required for deserialization.");

//            // check if the type has the IncludePrivateFieldsAttribute, if it does, include private fields in the serialization
//            bool includeprivates = type.GetCustomAttributes<IncludePrivateFieldsAttribute>().Count() is 1;

//            ReflectionHelper rh = new(type);
//            rh.IncludePrivateFields = includeprivates;

//            // get all members of the type, this includes fields and properties
//            var members = rh.GetMembers();

//            // create a list of statements that will be added to the body of the serialize method
//            List<StatementSyntax> statements =
//            [
//                SyntaxFactory.ParseStatement($"{type.Name} result = new();"),
//                SyntaxFactory.ParseStatement("Dictionary<string, string?> values = new();"),
//                SyntaxFactory.ParseStatement("string[] splits = data.Split($\"#{depth}\");"),
//                SyntaxFactory.ParseStatement("foreach (string item in splits)"),
//                SyntaxFactory.ParseStatement("{"),
//                SyntaxFactory.ParseStatement("\tstring[] split = item.Split($\"|{depth}\");"),
//                SyntaxFactory.ParseStatement("\tif (split.Length is 1 && split[0][0] == '@')"),
//                SyntaxFactory.ParseStatement("\t\tcontinue;"),
//                SyntaxFactory.ParseStatement("\tvalues.Add(split[0], split[1] ?? null);"),
//                SyntaxFactory.ParseStatement("}"),
//                SyntaxFactory.ParseStatement("foreach (var item in values)"),
//                SyntaxFactory.ParseStatement("{"),
//            ];

//            foreach (var member in members)
//            {
//                if (member is { IsStatic: true } or { CanWrite: false }) continue;
//                statements.Add(SyntaxFactory.ParseStatement($"\tif (item.Key == \"{member.Name}\")"));

//                statements.Add(SyntaxFactory.ParseStatement("{"));
//                statements.Add(SyntaxFactory.ParseStatement($"\t\tif (item.Value is null)"));
//                statements.Add(SyntaxFactory.ParseStatement("\t\t\tcontinue;"));
//                statements.Add(SyntaxFactory.ParseStatement(
//                    $"\t\tresult.{member.Name} = SnowSerializerWorkers.DeserializeField<{GetTypeName(member.Type)}>(item.Value!, typeof({GetTypeName(member.Type)}), depth, settings);"));
//                statements.Add(SyntaxFactory.ParseStatement("}"));
//            }

//            // close the foreach loop
//            statements.Add(SyntaxFactory.ParseStatement("}"));
//            // add the return statement to the list of statements
//            statements.Add(SyntaxFactory.ParseStatement("return result;"));

//            // add all the namespaces that are required for this specific generated class to a list
//            if (type.Namespace is not null && !namespacesToImport.Contains(type.Namespace))
//                namespacesToImport.Add(type.Namespace);
//            //if (!namespacesToImport.Contains("WinterRose.Serialization.Workers"))
//            //    namespacesToImport.Add("WinterRose.Serialization.Workers");
//            if (!namespacesToImport.Contains("WinterRose.Serialization"))
//                namespacesToImport.Add("WinterRose.Serialization");
//            if (!namespacesToImport.Contains("System.Collections.Generic"))
//                namespacesToImport.Add("System.Collections.Generic");

//            // add all the namespaces of the members to the list of namespaces to import
//            foreach (var member in members)
//            {
//                if (type.Namespace is not null && !namespacesToImport.Contains(member.Type.Namespace))
//                    namespacesToImport.Add(member.Type.Namespace);
//            }

//            // create a block syntax from the list of statements and return it
//            return SyntaxFactory.Block(statements);
//        }
//        private string GetTypeName(Type type)
//        {
//            if (type.IsGenericType)
//            {
//                string name = type.Name;
//                int index = name.IndexOf('`');
//                if (index > 0)
//                    name = name.Substring(0, index);
//                name += "<";
//                Type[] arguments = type.GetGenericArguments();
//                for (int i = 0; i < arguments.Length; i++)
//                {
//                    if (i > 0)
//                        name += ", ";
//                    name += GetTypeName(arguments[i]);
//                }
//                name += ">";
//                return name;
//            }
//            return type.Name;
//        }
//        private string GetCodeFriendlyTypeAndNamespace(Type type)
//        {
//            string ns = type.Namespace;
//            string name = type.Name;
//            if (type.IsGenericType)
//            {
//                int index = name.IndexOf('`');
//                if (index > 0)
//                    name = name.Substring(0, index);
//                name += "<";
//                Type[] arguments = type.GetGenericArguments();
//                for (int i = 0; i < arguments.Length; i++)
//                {
//                    if (i > 0)
//                        name += ", ";
//                    name += GetTypeName(arguments[i]);
//                }
//                name += ">";
//            }
//            if (string.IsNullOrWhiteSpace(ns))
//                return name;
//            else
//                return $"{ns.Replace('.', '_')}_{name}";
//        }
//        private BlockSyntax CreateSerializerBody(Type type)
//        {
//            // check if the type has the IncludePrivateFieldsAttribute, if it does, include private fields in the serialization
//            bool includeprivates = type.GetCustomAttributes<IncludePrivateFieldsAttribute>().Count() is 1;

//            ReflectionHelper rh = new(type);
//            rh.IncludePrivateFields = includeprivates;

//            // get all members of the type, this includes fields and properties
//            var members = rh.GetMembers();

//            // create a list of statements that will be added to the body of the serialize method
//            List<StatementSyntax> statements =
//            [
//                SyntaxFactory.ParseStatement("StringBuilder builder = new();"),
//                SyntaxFactory.ParseStatement("if (settings.IncludeType)"),
//                SyntaxFactory.ParseStatement($"builder.Append($\"@{{depth}}{$"{type.Name}--{type.Namespace}--{type.Assembly.GetName().FullName}".Base64Encode()}\");"),
//                SyntaxFactory.ParseStatement("else"),
//                SyntaxFactory.ParseStatement("builder.Append($\"@{depth}\");"),
//            ];

//            foreach (var member in members)
//            {
//                // we skip fields that are static or readonly
//                if (member
//                    is { IsStatic: true }
//                    or { CanWrite: false })
//                    continue;

//                if (member.MemberType is MemberTypes.Field && member.HasAttribute<WFExcludeAttribute>()
//                    || member.MemberType is MemberTypes.Property && !member.HasAttribute<WFIncludeAttribute>())
//                    continue;

//                // if the member has the WFIncludeAttribute, we add it to the serialization
//                string syntax = $"builder.Append(SnowSerializerWorkers.SerializeField(obj.{member.Name}, \"{member.Name}\", typeof({GetTypeName(member.Type)}), depth, settings));";
//                statements.Add(SyntaxFactory.ParseStatement(syntax));
//                // add the namespace of the member to the list of namespaces to import
//                if (type.Namespace is not null && !namespacesToImport.Contains(member.Type.Namespace))
//                    namespacesToImport.Add(member.Type.Namespace);
//            }

//            // add the return statement to the list of statements
//            statements.Add(SyntaxFactory.ParseStatement("return builder;"));

//            // create a block syntax from the list of statements and return it
//            return SyntaxFactory.Block(statements);
//        }
//    }
//}