using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace RandomTesting.LocalCodex.Commands;

public sealed class ExecuteCSharpCodeCommand : IAgentCommand
{
    public string Name => "execute_csharp_code";
    public string Description => "Executes a C# code blob that returns a value. The code is sandboxed and cannot access file system or system-altering classes.";

    public bool IsReadonly => true;

    // Blocked namespaces and types that could be used for system access
    private static readonly HashSet<string> BlockedNamespaces = new()
    {
        "System.IO",
        "System.IO.Compression",
        "System.IO.Compression.FileSystem",
        "System.IO.FileSystem",
        "System.IO.FileSystem.AccessControl",
        "System.IO.FileSystem.DriveInfo",
        "System.IO.FileSystem.Primitives",
        "System.IO.FileSystem.Watcher",
        "System.IO.MemoryMappedFiles",
        "System.IO.Packaging",
        "System.IO.Pipes",
        "System.IO.Ports",
        "System.DirectoryServices",
        "System.DirectoryServices.AccountManagement",
        "System.DirectoryServices.ActiveDirectory",
        "System.DirectoryServices.Protocols",
        "System.Security.AccessControl",
        "System.Security.Cryptography.X509Certificates",
        "System.Diagnostics",
        "System.Diagnostics.Eventing",
        "System.Diagnostics.PerformanceData",
        "System.Diagnostics.Tracing",
        "System.Process",
        "Microsoft.Win32",
        "System.Management",
        "System.Threading.Tasks.Dataflow",
        "System.Threading.Tasks.Parallel",
        "System.Threading.Thread",
        "System.Threading.ThreadPool",
        "System.Runtime.InteropServices",
        "System.Runtime.Remoting"
    };

    // Blocked type names (full type names)
    private static readonly HashSet<string> BlockedTypeNames = new()
    {
        "System.Environment",
        "System.AppDomain",
        "System.AppDomainManager",
        "System.Console",
        "System.Diagnostics.Process",
        "System.Diagnostics.ProcessStartInfo",
        "System.Diagnostics.ProcessModule",
        "System.Diagnostics.ProcessThread",
        "System.Diagnostics.ProcessPriorityClass",
        "System.Security.Principal.WindowsIdentity",
        "System.Security.Principal.WindowsPrincipal",
        "System.Security.Principal.WindowsImpersonationContext",
        "System.Security.Principal.GenericPrincipal",
        "System.Security.Principal.GenericIdentity",
        "System.Security.Principal.PrincipalPolicy",
        "System.Net.WebClient",
        "System.Net.WebRequest",
        "System.Net.WebResponse",
        "System.Net.Http.HttpClient",
        "System.Net.Http.HttpClientHandler",
        "System.Net.Http.HttpRequestMessage",
        "System.Net.Http.HttpResponseMessage",
        "System.Net.Sockets.Socket",
        "System.Net.Sockets.TcpClient",
        "System.Net.Sockets.TcpListener",
        "System.Net.Sockets.UdpClient",
        "System.Data.SqlClient.SqlConnection",
        "System.Data.SqlClient.SqlCommand",
        "System.Data.SqlClient.SqlDataReader",
        "System.Data.OleDb.OleDbConnection",
        "System.Data.OleDb.OleDbCommand",
        "System.Data.Odbc.OdbcConnection",
        "System.Data.Odbc.OdbcCommand",
        "System.Reflection.Assembly",
        "System.Reflection.AssemblyName",
        "System.Reflection.ConstructorInfo",
        "System.Reflection.FieldInfo",
        "System.Reflection.MethodInfo",
        "System.Reflection.PropertyInfo",
        "System.Reflection.EventInfo",
        "System.Reflection.TypeInfo",
        "System.Reflection.ParameterInfo",
        "System.Reflection.Module",
        "System.Reflection.Emit.AssemblyBuilder",
        "System.Reflection.Emit.ConstructorBuilder",
        "System.Reflection.Emit.MethodBuilder",
        "System.Reflection.Emit.FieldBuilder",
        "System.Reflection.Emit.PropertyBuilder",
        "System.Reflection.Emit.TypeBuilder",
        "System.Reflection.Emit.AssemblyBuilderAccess",
        "System.Reflection.Emit.ModuleBuilder",
        "System.Reflection.Emit.EnumBuilder",
        "System.Reflection.Emit.GenericTypeParameterBuilder",
        "System.Reflection.Emit.OpCode",
        "System.Reflection.Emit.OpCodes",
        "System.Runtime.CompilerServices.Unsafe",
        "System.Unsafe"
    };

    // Known parameter keys that can appear before the code
    private static readonly HashSet<string> KnownParameterKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "timeout",
        "expected_return_type",
        "code",
        "body",
        "script",
        "content"
    };

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        // Parse arguments properly - code is always the last argument
        var parsedArgs = ParseArguments(arguments);

        var code = parsedArgs.Code;
        var timeoutSeconds = parsedArgs.Timeout;
        var expectedReturnType = parsedArgs.ExpectedReturnType;

        // Validate and sanitize the code
        if (string.IsNullOrWhiteSpace(code))
        {
            return FormatOperationResult(false, "Code cannot be empty");
        }

        try
        {
            // Analyze the code for blocked types and namespaces
            var validationResult = ValidateCodeSecurity(code);
            if (!validationResult.IsValid)
            {
                return FormatOperationResult(false, $"Security validation failed: {validationResult.ErrorMessage}");
            }

            // Execute the code with timeout
            var result = await ExecuteWithTimeoutAsync(
                code,
                expectedReturnType,
                TimeSpan.FromSeconds(timeoutSeconds),
                cancellationToken);

            if (result.IsSuccess)
            {
                return FormatOperationResult(true, $"Code executed successfully. Result: {result.ReturnValue}");
            }
            else
            {
                return FormatOperationResult(false, $"Execution failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            return FormatOperationResult(false, $"Error: {ex.Message}");
        }
    }

    private ParsedArguments ParseArguments(IReadOnlyDictionary<string, string> arguments)
    {
        int timeout = 30;
        string expectedReturnType = "object";
        string code = string.Empty;

        var lookup = arguments.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        if (lookup.TryGetValue("timeout", out var timeoutValue) &&
            int.TryParse(timeoutValue, out var parsedTimeout))
        {
            timeout = Math.Max(1, parsedTimeout);
        }

        if (lookup.TryGetValue("expected_return_type", out var returnTypeValue) &&
            !string.IsNullOrWhiteSpace(returnTypeValue))
        {
            expectedReturnType = returnTypeValue.Trim();
        }

        if (lookup.TryGetValue("code", out var codeValue) &&
            !string.IsNullOrWhiteSpace(codeValue))
        {
            code = codeValue;
        }
        else if (lookup.TryGetValue("body", out var bodyValue) &&
                 !string.IsNullOrWhiteSpace(bodyValue))
        {
            code = bodyValue;
        }
        else if (lookup.TryGetValue("script", out var scriptValue) &&
                 !string.IsNullOrWhiteSpace(scriptValue))
        {
            code = scriptValue;
        }
        var unknownParameters = arguments
            .Where(pair => !KnownParameterKeys.Contains(pair.Key))
            .ToList();

        foreach(var part in unknownParameters)
            if (!string.IsNullOrWhiteSpace(unknownParameters[0].Value))
            {
                code += $"\n{part.Key}={part.Value}";
            }

        return new ParsedArguments(timeout, expectedReturnType, code.Trim());
    }

    private CodeValidationResult ValidateCodeSecurity(string code)
    {
        // Parse the code to check for blocked types and namespaces
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var comp = CSharpCompilation.Create("SecurityCheck")
            .AddSyntaxTrees(tree);

        // Check for using directives with blocked namespaces
        var usingDirectives = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
            .ToList();

        foreach (var usingDirective in usingDirectives)
        {
            var namespaceName = usingDirective.Name.ToString();
            if (BlockedNamespaces.Any(bn => namespaceName.StartsWith(bn)))
            {
                return CodeValidationResult.Invalid(
                    $"Using directive for blocked namespace: {namespaceName}");
            }
        }

        // Check for type references
        var model = comp.GetSemanticModel(tree);
        var typeReferences = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax>()
            .Where(n => n.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)
            .ToList();

        foreach (var typeRef in typeReferences)
        {
            var symbolInfo = model.GetSymbolInfo(typeRef);
            var symbol = symbolInfo.Symbol;

            if (symbol != null)
            {
                var containingType = symbol.ContainingType;
                if (containingType != null)
                {
                    var fullName = containingType.ToString();
                    if (BlockedTypeNames.Any(bt => fullName.StartsWith(bt)) ||
                        BlockedNamespaces.Any(bn => fullName.StartsWith(bn)))
                    {
                        return CodeValidationResult.Invalid(
                            $"Reference to blocked type: {fullName}");
                    }
                }
            }
        }

        // Check for unsafe code blocks
        var unsafeNodes = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax>();
        if (unsafeNodes.Any())
        {
            return CodeValidationResult.Invalid("Unsafe code blocks are not allowed");
        }

        // Check for potential string-based invocation using reflection
        var stringLiterals = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax>()
            .Where(l => l.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
            .ToList();

        // Warn about potential dynamic type resolution, but don't block
        // This is a soft check - we can't catch all dynamic invocations

        return CodeValidationResult.Valid();
    }

    private async Task<ExecutionResult> ExecuteWithTimeoutAsync(
        string code,
        string expectedReturnType,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        // Build a complete compilation unit
        var codeToCompile = WrapCodeInClass(code, expectedReturnType);

        // Create syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

        // Set up references (only safe assemblies)
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Math).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.DateTime).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.ArrayList).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.Dictionary<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Concurrent.ConcurrentDictionary<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.StringBuilder).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Guid).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Random).Assembly.Location)
        };

        // Configure compilation options
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            allowUnsafe: false,
            platform: Platform.AnyCpu,
            warningLevel: 0,
            specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
            {
                { "CS1701", ReportDiagnostic.Suppress },
                { "CS1702", ReportDiagnostic.Suppress }
            });

        // Create compilation
        var compilation = CSharpCompilation.Create(
            "DynamicCode",
            new[] { syntaxTree },
            references,
            compilationOptions);

        // Emit assembly
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            return ExecutionResult.Failure($"Compilation failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);

        // Load the assembly
        var assembly = Assembly.Load(ms.ToArray());
        var type = assembly.GetType("DynamicCode.Executor");
        var method = type?.GetMethod("Execute");

        if (method == null)
        {
            return ExecutionResult.Failure("Could not find Execute method");
        }

        // Create an instance and invoke with timeout
        var instance = Activator.CreateInstance(type);

        using var cts = new CancellationTokenSource(timeout);
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cts.Token,
            cancellationToken);

        try
        {
            var task = Task.Run(() => method.Invoke(instance, null), linkedCts.Token);

            if (await Task.WhenAny(task, Task.Delay(timeout, linkedCts.Token)) == task)
            {
                if (task.Exception != null)
                {
                    return ExecutionResult.Failure($"Execution error: {task.Exception.InnerException?.Message ?? task.Exception.Message}");
                }

                var returnValue = task.Result;
                return ExecutionResult.Success(returnValue?.ToString() ?? "null");
            }
            else
            {
                return ExecutionResult.Failure($"Execution timed out after {timeout.TotalSeconds} seconds");
            }
        }
        catch (OperationCanceledException)
        {
            return ExecutionResult.Failure("Execution was cancelled");
        }
        catch (Exception ex)
        {
            return ExecutionResult.Failure($"Execution error: {ex.Message}");
        }
    }

    private string WrapCodeInClass(string code, string expectedReturnType)
    {
        var returnType = expectedReturnType?.Trim() ?? "object";
        var trimmedCode = code.Trim();

        var isExpression =
            !trimmedCode.Contains(";") &&
            !trimmedCode.Contains("{") &&
            !trimmedCode.Contains("}");

        string methodBody;

        if (isExpression)
        {
            methodBody = $"return ({trimmedCode});";
        }
        else
        {
            methodBody = trimmedCode;

            if (!trimmedCode.Contains("return "))
            {
                methodBody += Environment.NewLine + "return null;";
            }
        }

        return $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicCode
{{
    public class Executor
    {{
        public {returnType} Execute()
        {{
            {methodBody}
        }}
    }}
}}";
    }

    private static string FormatOperationResult(bool success, string message)
    {
        return "OPERATION_RESULT:" + Environment.NewLine +
               $"success={(success ? "true" : "false")}" + Environment.NewLine +
               $"message=" + (message.EndsWith("null") ? message.Replace("null", "No result. did you forget to add a return statement?") : message);
    }

    public string GetToolExample()
    {
        return
@"Tool: execute_csharp_code

Arguments:
- timeout: int (optional, default = 30, maximum execution time in seconds)
- expected_return_type: string (optional, default = 'object', the expected return type)
- code: string (required, the C# code to execute - MUST be the last argument)

IMPORTANT: The 'code' argument must be the last argument. All arguments after any known parameter are treated as part of the code.

The arguments are provided as key=value pairs. The framework splits on '=', so the 'code' argument 
can contain '=' characters and will be properly handled as long as it's the last argument.

Examples (This is a explaination of arguments, not tool format. stick to the tool format specified before):

1. Simple expression:
code=Enumerable.Range(1, 10).Sum()
expected_return_type=int

2. With timeout and complex code:
timeout=15
-xpected_return_type=string
code=new string(Enumerable.Range(65, 26).Select(i => (char)i).ToArray())

3. Multi-line code with equals signs:
expected_return_type=string
code=var dict = new Dictionary<string, string>();
    dict[""key""] = ""value with = sign"";
    dict[""another""] = ""more text"";
    return string.Join("", "", dict.Select(kv => $""{kv.Key}={kv.Value}""));

4. Using the code as the only argument (key can be anything, but 'code' is preferred):
code=Enumerable.Range(1, 100).Where(x => x % 2 == 0).Sum()

Notes:
- Code is executed in a sandboxed environment with no file system access
- No system-altering operations (processes, registry, etc.) are permitted
- The code should return a value at the end
- Simple expressions are automatically wrapped in a return statement
- Allowed: math, string manipulation, collections, LINQ, etc.
- Blocked: System.IO, System.Diagnostics, System.Net, reflection, unsafe code, etc.
- The 'code' argument is the immediate body of an executed method. You cannot write classes yourself, use dictionaries instead.
- Return values get .ToString() called on them before returning. If you need complex data structures returned, format them into a string yourself.

IMPORTANT EXECUTION RULE:
- Your code MUST explicitly contain a return statement.
- If no return statement exists, the result will be null.
- No automatic variable dumping or implicit result capture exists.
- You are responsible for formatting your output.

Failure points:
- Missing required argument: code
- Code contains blocked types or namespaces
- Compilation errors in the provided code
- Runtime exceptions during execution
- Execution timeout exceeded
- Unsafe code or prohibited operations";
    }

    private record ParsedArguments
    {
        public int Timeout { get; }
        public string ExpectedReturnType { get; }
        public string Code { get; }

        public ParsedArguments(int timeout, string expectedReturnType, string code)
        {
            Timeout = timeout;
            ExpectedReturnType = expectedReturnType;
            Code = code;
        }
    }

    private record CodeValidationResult
    {
        public bool IsValid { get; init; }
        public string ErrorMessage { get; init; }

        public static CodeValidationResult Valid() => new() { IsValid = true };
        public static CodeValidationResult Invalid(string error) => new() { IsValid = false, ErrorMessage = error };
    }

    private record ExecutionResult
    {
        public bool IsSuccess { get; init; }
        public string ReturnValue { get; init; }
        public string ErrorMessage { get; init; }

        public static ExecutionResult Success(string value) => new()
        {
            IsSuccess = true,
            ReturnValue = value
        };

        public static ExecutionResult Failure(string error) => new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };
    }
}