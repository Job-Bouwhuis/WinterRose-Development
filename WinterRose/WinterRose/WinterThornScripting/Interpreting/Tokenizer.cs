using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinterRose.WinterThornScripting.Generation;

namespace WinterRose.WinterThornScripting.Interpreting;

/// <summary>
/// The tokenizer is responsible for converting the source code into a list of tokens.<br></br>
/// In WinterThorn only function bodies are tokenized, class bodies are handled by the parser.
/// </summary>
internal class Tokenizer
{
    internal static List<Token> Tokenize(string source)
    {
        string[] lines = source.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        StringBuilder sourceBuilder = new();
        foreach (string line in lines)
        {
            if (!line.StartsWith("//"))
                sourceBuilder.Append(line).Append(' ');
        }
        source = sourceBuilder.ToString();
        source = source.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
        string[] parts = SplitParts(source);
        RemoveSpecialCharacters(parts);

        return LoopOverParts(parts);
    }

    private static List<Token> LoopOverParts(string[] parts)
    {
        List<Token> tokens = [];
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (part is "=" && tokens[^1].Type != TokenType.Identifier)
            {
                i -= 2;
                continue;
            }
            if (part.StartsWith("/*"))
            {
                while (!part.EndsWith("*/"))
                {
                    part = parts[++i];
                }
                continue;
            }

            bool shouldAddSemicolonToken = part.EndsWith(';');
            string identifier = part.TrimEnd(';');

            if (identifier.StartsWith('!'))
            {
                identifier = identifier.TrimStart('!');

                bool replaceIFStatement = tokens[^1].Type == TokenType.IfClause;
                Token? ifToken = tokens[^1];
                if (replaceIFStatement)
                {
                    tokens.RemoveAt(tokens.Count - 1);

                    string booleanName = $"bool_{identifier.Replace('.', '_')}";
                    string inverseBooleanName = $"INVERSEDbool_{identifier.Replace('.', '_')}";

                    tokens.AddRange(LoopOverParts([booleanName, "=", identifier, ";"]));
                    tokens.AddRange(LoopOverParts([inverseBooleanName, "=", $"__BooleanInverse({booleanName})"]));
                    tokens.Add(new Token(";", TokenType.Semicolon));

                    if (replaceIFStatement)
                        tokens.Add(ifToken);

                    CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, inverseBooleanName);
                }
                else
                {
                    tokens.AddRange(LoopOverParts([$"__BooleanInverse({identifier})"]));
                    tokens.Add(new Token(";", TokenType.Semicolon));
                }

                continue;
            }

            if (identifier.Contains('.'))
            {
                HandleAccessorIdentifier(parts, tokens, ref i, ref shouldAddSemicolonToken, ref identifier);
                continue;
            }
            if (identifier is "while")
            {
                CreateWhileLoop(parts, tokens, ref i, ref shouldAddSemicolonToken);
                continue;
            }

            CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier);
            if (shouldAddSemicolonToken)
            {
                tokens.Add(new Token(";", TokenType.Semicolon));
            }
        }

        if (tokens[0].Type == TokenType.OpenBrace && tokens[^1].Type != TokenType.CloseBracket)
            tokens.Add(new Token("}", TokenType.CloseBracket));
        return tokens;

        void HandleAccessorIdentifier(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, ref string identifier)
        {
            int proceeded = 0;
            string next;
            if (!shouldAddSemicolonToken)
            {
                //proceeded = -1;
                do
                {
                    if ((next = parts[++i].Trim()) is not "=" and not "+" and not "return" and not "{")
                    {
                        identifier += " " + next;
                    }
                    else
                        break;
                    proceeded++;
                }
                while (!(next.EndsWith(')') || next.EndsWith(';') || shouldAddSemicolonToken));
            }
            else
                proceeded += 1;

            i += proceeded;
            string[] accessorParts = SplitAccessorsParts(identifier);

            for (int a = 0; a < accessorParts.Length; a++)
            {
                string accessorPart = accessorParts[a];
                CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, accessorPart);
                if (a != accessorParts.Length - 1)
                {
                    CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, ".");
                }
                else
                {
                    if (!accessorParts[^1].EndsWith(")"))
                        proceeded -= 1;
                }
            }
            if (shouldAddSemicolonToken)
            {
                proceeded -= 1;
                if (accessorParts[^1].EndsWith(")"))
                    proceeded -= 1;
                tokens.Add(new Token(";", TokenType.Semicolon));
            }
            i += proceeded;
        }
    }

    private static void CreateWhileLoop(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken)
    {
        // construct a loop using a label, if statement for the condition, and a goto statement to jump back to the label
        string label = "loop" + Guid.NewGuid().ToString().Replace("-", "");
        string[] partsUntil = GetPartsUntil(parts, ref i, "{");

        // add the label and the if tokens
        tokens.Add(new Token(label + ':', TokenType.Label));
        tokens.Add(new Token("if", TokenType.IfClause));

        // Add the tokens that make up the condition
        int j;
        for (j = 1; j < partsUntil.Length; j++)
        {
            string partUntil = partsUntil[j];
            CreateToken(partsUntil, tokens, ref j, ref shouldAddSemicolonToken, partUntil);
        }
        // add the amount of parts that were added to the tokens to the index so we dont get duplicate tokens
        i += j;
        // get the parts until the end of the while loop body
        partsUntil = GetBlock(parts, ref i);
        // add the amount of parts that were fetched to the index so we dont get duplicate tokens
        i += partsUntil.Length;
        // Create tokens from the parts that make up the while loop body
        var list = LoopOverParts(partsUntil);
        if (list[^1].Type != TokenType.CloseBracket)
            throw new WinterThornCompilationError(ThornError.SyntaxError, "WT-0016", "block braces count mismatch");
        list.RemoveAt(list.Count - 1);

        // add the tokens to the list of tokens
        tokens.AddRange(list);
        // add the goto statement to jump back to the label
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, "goto");
        // add the label to jump back to
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, label);
        // add the semicolon token to close the goto statement
        tokens.Add(new Token(";", TokenType.Semicolon));
        // close the if statement body
        tokens.Add(new Token("}", TokenType.CloseBracket));
        i--;
    }

    private static string[] GetBlock(string[] parts, ref int i)
    {
        // loop over the parts until we find the end of this block. blocks can be nested so we need to keep track of the amount of open braces
        int openBraces = 0;
        int j;
        for (j = i; j < parts.Length; j++)
        {
            string part = parts[j];
            if (part == "{")
            {
                openBraces++;
            }
            if (part == "}")
            {
                openBraces--;
                if (openBraces == 0)
                {
                    return parts[i..(j + 1)];
                }
            }
        }
        return parts;
    }

    private static string[] GetPartsUntil(string[] parts, ref int i, string desiredPart)
    {
        int j;
        for (j = i; j < parts.Length; j++)
        {
            string part = parts[j];
            if (part == desiredPart)
            {
                return parts[i..j];
            }
        }

        return [];
    }

    private static string[] GetPartsUntilSemicolon(string[] parts, ref int i)
    {
        const string semicolon = ";";
        int j;
        for (j = i; j < parts.Length; j++)
        {
            string part = parts[j];
            if (part == semicolon)
                return parts[i..j];
            if (part.EndsWith(semicolon))
                return parts[i..(j + 1)];
        }

        return [];
    }

    /// <summary>
    /// split the source into parts by spaces, but keep strings together
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static string[] SplitParts(string source)
    {
        List<string> parts = [];
        string current = "";
        bool insideString = false;


        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c == '"')
            {
                insideString = !insideString;
            }
            if (c == ' ' && !insideString)
            {
                if (string.IsNullOrWhiteSpace(current))
                    continue;

                parts.Add(current);
                current = "";
                continue;
            }
            current += c;
        }
        parts.Add(current);
        return parts.ToArray();
    }

    /// <summary>
    /// Split the identifier into parts by the dot, but keep strings together
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    private static string[] SplitAccessorsParts(string identifier)
    {
        List<string> parts = [];
        string current = "";
        bool insideString = false;

        for (int i = 0; i < identifier.Length; i++)
        {
            char c = identifier[i];
            if (c == '"')
            {
                insideString = !insideString;
            }
            if (c == '.' && !insideString)
            {
                parts.Add(current);
                current = "";
                continue;
            }
            current += c;
        }
        parts.Add(current);
        return parts.ToArray();
    }

    /// <summary>
    /// Create tokens based on <paramref name="parts"/> and <paramref name="i"/> if <paramref name="identifier"/> 
    /// is a keyword or a function call. otherwise just add the identifier as a token of the correct type
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="tokens"></param>
    /// <param name="i"></param>
    /// <param name="shouldAddSemicolonToken"></param>
    /// <param name="identifier"></param>
    private static void CreateToken(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier)
    {
        if (identifier.StartsWith('"'))
        {
            if (identifier.Length is not 1 && identifier.EndsWith('"'))
            {
                tokens.Add(new Token(identifier, TokenType.String));
                return;
            }
            int proceeded = -1;
            string next = "";
            do if ((next = parts[++i]) is not "=" and not "+" and not "return")
                {
                    identifier += " " + next;
                    if (identifier.EndsWith(';'))
                    {
                        shouldAddSemicolonToken = true;
                        identifier = identifier.TrimEnd(';');

                    }
                    proceeded++;
                    if (shouldAddSemicolonToken)
                        break;
                }
                else
                    break;
            while (!next.EndsWith('"'));
            i += proceeded;
            tokens.Add(new Token(identifier, TokenType.String));
            if (shouldAddSemicolonToken)
            {
                tokens.Add(new Token(";", TokenType.Semicolon));
            }
            return;
        }
        switch (identifier)
        {
            case "": // empty string part is ignored.
                break;
            case "//":
            case "/*":
            case "*/":
                tokens.Add(new Token(identifier, TokenType.Comment));
                break;
            case "namespace":
                tokens.Add(new Token(identifier, TokenType.Namespace));
                break;
            case "class":
                tokens.Add(new Token(identifier, TokenType.Class));
                break;
            case "public":
            case "private":
            case "global":
                tokens.Add(new Token(identifier, TokenType.AccessControl));
                break;
            case "goto":
                tokens.Add(new Token(identifier, TokenType.Goto));
                break;
            case "if":
                tokens.Add(new Token(identifier, TokenType.IfClause));
                break;
            case "else":
                tokens.Add(new Token(identifier, TokenType.ElseClause));
                break;
            case "foreach":
                tokens.Add(new Token(identifier, TokenType.Loop));
                break;
            case "==":
            case "!=":
            case ">":
            case "<":
            case ">=":
            case "<=":
            case "+":
            case "-":
            case "*":
            case "/":
            case "%":
                tokens.Add(new Token(identifier, TokenType.Operator));
                break;
            case "=":
                tokens.Add(new Token(identifier, TokenType.AssignVariable));
                break;
            case ".":
                tokens.Add(new Token(identifier, TokenType.Accessor));
                break;
            case "&&":
                tokens.Add(new Token(identifier, TokenType.And));
                break;
            case "||":
                tokens.Add(new Token(identifier, TokenType.Or));
                break;
            case "!":
                tokens.Add(new Token(identifier, TokenType.Not));
                break;
            case "null":
                tokens.Add(new Token(identifier, TokenType.Null));
                break;
            case "true":
            case "false":
                tokens.Add(new Token(identifier, TokenType.Boolean));
                break;
            case "return":
                tokens.Add(new Token(identifier, TokenType.Return));
                break;
            case "break":
                tokens.Add(new Token(identifier, TokenType.Break));
                break;
            case "continue":
                tokens.Add(new Token(identifier, TokenType.Continue));
                break;
            case "new":
                tokens.Add(new Token(identifier, TokenType.New));
                break;
            case "{":
                tokens.Add(new Token(identifier, TokenType.OpenBrace));
                break;
            case "}":
                tokens.Add(new Token(identifier, TokenType.CloseBracket));
                break;
            default:
                ;

                if (identifier.EndsWith(':'))
                {
                    tokens.Add(new Token(identifier.TrimEnd(':'), TokenType.Label));
                    break;
                }

                // see if its a function call
                int functionParameterStart = identifier.IndexOf('(');
                if (functionParameterStart is not -1)
                {
                    HandleFunctionCallDeclaration(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier, functionParameterStart);
                    break;
                }

                if (identifier.StartsWith("for"))
                {
                    TranslateForLoop(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier);
                    return;
                }

                if (identifier.StartsWith("["))
                {
                    shouldAddSemicolonToken = CreateCollection(parts, tokens, ref i, identifier);

                    return;
                }

                if (identifier.Contains("[") && !identifier.StartsWith("[")) // this is a call to get a value at a certain index
                {
                    if (parts[i + 1] == "=")
                        AssignCollectionVariable(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier);
                    else
                        GetCollectionVariable(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier);

                    return;
                }

                if (identifier.EndsWith(")"))
                {
                    string remaining = identifier[..(identifier.Length - 1)];
                    CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, remaining);
                    tokens.Add(new Token(")", TokenType.CloseParenthesis));
                    return;
                }

                if (identifier.EndsWith("²"))
                {
                    MathSquareFormulaTranslation(parts, tokens, ref i, ref shouldAddSemicolonToken, identifier);
                    return;
                }

                if (identifier.EndsWith("++"))
                {
                    identifier = identifier.Replace("++", "");
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                    tokens.Add(new Token("=", TokenType.AssignVariable));
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                    tokens.Add(new Token("+", TokenType.Operator));
                    tokens.Add(new Token("1", TokenType.Identifier));
                    return;
                }

                if (identifier.EndsWith("--"))
                {
                    identifier = identifier.Replace("--", "");
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                    tokens.Add(new Token("=", TokenType.AssignVariable));
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                    tokens.Add(new Token("-", TokenType.Operator));
                    tokens.Add(new Token("1", TokenType.Identifier));
                    return;
                }

                if (identifier.EndsWith(';'))
                {
                    identifier = identifier.TrimEnd(';').Trim();
                    shouldAddSemicolonToken = true;
                }

                tokens.Add(new Token(identifier, TokenType.Identifier));
                break;
        }
    }

    private static void HandleFunctionCallDeclaration(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier, int functionParameterStart)
    {
        // if it is, fetch the name
        string functionName = identifier[..functionParameterStart];
        if (functionName is null or "")
        {
            tokens.Add(new Token("(", TokenType.OpenParenthesis));
            string remaining = identifier[1..];
            CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, remaining);
            return;
        }
        // add it as its seperate token
        if (parts.Length != 1 && parts[i - 1] == "new")
            tokens.Add(new Token(functionName, TokenType.Constructor));
        else
            tokens.Add(new Token(functionName, TokenType.Function));
        // fetch the parameters
        // parameters, if there are multiple, may be seperated by spaces and thus in the next part
        List<string> parameters =
        [
            .. identifier[functionParameterStart..].TrimStart('(').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    ];
        // if there is a ) in the last part, we have all the parameters
        int proceeded = 0;
        while (!(parameters[^1].Contains(')') || parameters[^1].Contains(';')))
        {
            // add the next part
            parameters.Add(parts[++i]);
        }
        if (parameters[^1].EndsWith(';'))
            shouldAddSemicolonToken = true;
        parameters[^1] = parameters[^1].TrimEnd(';').TrimEnd(')');
        // add the open parenthesis token
        tokens.Add(new Token("(", TokenType.OpenParenthesis));
        // add each parameter as its own token
        for (int p = 0; p < parameters.Count; p++)
        {
            string str = parameters[p];
            if (string.IsNullOrWhiteSpace(str))
                continue;
            str = str.Trim();
            tokens.Add(new Token(str, TokenType.FunctionParameter));
            // add a comma token if its not the last or only parameter
            if (p < parameters.Count - 1)
            {
                tokens.Add(new Token(",", TokenType.Comma));
            }
        }
        tokens.Add(new Token(")", TokenType.CloseParenthesis));

        // if previous token is not the new keyword, remove 1 from i to prevent skipping the next token
        if (parts.Length != 1 && (i - 1 != parts.Length && parts[i - 1] != "new" && parameters.Count == 0))
            i--;
    }

    private static void MathSquareFormulaTranslation(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier)
    {
        string remaining = identifier[..(identifier.Length - 1)];

        tokens.Add(new Token("(", TokenType.OpenParenthesis));
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, remaining);
        tokens.Add(new Token("*", TokenType.Operator));
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, remaining);
        tokens.Add(new Token(")", TokenType.CloseParenthesis));
    }

    private static void GetCollectionVariable(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier)
    {
        string[] split = identifier.Split('[', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string variableName = split[0];
        string index = split[1].TrimEnd(']');

        tokens.Add(new Token(variableName, TokenType.Identifier));
        tokens.Add(new Token(".", TokenType.Accessor));
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, $"Get({index})");
    }

    private static void AssignCollectionVariable(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier)
    {
        string[] split = identifier.Split('[', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string variableName = split[0];
        string index = split[1].TrimEnd(']');

        i += 2; // consume '='

        string value = string.Join(' ', GetPartsUntilSemicolon(parts, ref i)).TrimEnd(';');

        tokens.Add(new Token(variableName, TokenType.Identifier));
        tokens.Add(new Token(".", TokenType.Accessor));
        CreateToken(parts, tokens, ref i, ref shouldAddSemicolonToken, $"Set({index}, {value})");
        tokens.Add(new Token(";", TokenType.Semicolon));
    }

    private static bool CreateCollection(string[] parts, List<Token> tokens, ref int i, string identifier)
    {
        bool shouldAddSemicolonToken;
        string collectionContent = identifier.TrimStart('[');
        int proceeded = 0;

        while (!collectionContent.EndsWith("]") && !collectionContent.EndsWith(";"))
        {
            collectionContent += " " + parts[++i]; // Concatenate until we find the closing bracket
            proceeded++;
        }

        shouldAddSemicolonToken = collectionContent.EndsWith(";");
        if (shouldAddSemicolonToken)
            collectionContent = collectionContent.TrimEnd(';');

        collectionContent = collectionContent.TrimEnd(']');
        string[] elements = collectionContent.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        tokens.Add(new Token("new", TokenType.New));
        tokens.Add(new Token("Collection", TokenType.Identifier));
        tokens.Add(new Token("(", TokenType.OpenParenthesis));

        // Add each element inside the brackets as separate tokens
        foreach (string element in elements)
        {
            tokens.Add(new Token(element, TokenType.FunctionParameter)); // Treating collection elements as parameters for simplicity
            tokens.Add(new Token(",", TokenType.Comma)); // Add a comma after each element, except the last one
        }

        // Remove the last comma token
        if (tokens[^1].Type == TokenType.Comma)
            tokens.RemoveAt(tokens.Count - 1);

        tokens.Add(new Token(")", TokenType.CloseParenthesis));
        return shouldAddSemicolonToken;
    }

    private static void TranslateForLoop(string[] parts, List<Token> tokens, ref int i, ref bool shouldAddSemicolonToken, string identifier)
    {
        i++;
        List<string> forDeclaration = [.. GetPartsUntil(parts, ref i, "{")];
        i += forDeclaration.Count + 1;
        string[] body = GetPartsUntil(parts, ref i, "}");
        i += body.Length;
        // for declaration should be at the indexes
        // 0: identifier as forloop variable. If it already exists before the forloop is encountered the starting value is that of the variable if the variable types align
        // 1: 'in' keyword. can be ignored, its just there for readablity.
        // 2: an existing identifier
        // if 3 should be a variable of the Collection type, or have a function with the exact definition "Get number index" as collection classes must implement 

        string forLoopVariable = forDeclaration[0];
        string collectionIdentifier = forDeclaration[forDeclaration.IndexOf("in") + 1];
        bool collectionIsNumber = double.TryParse(collectionIdentifier, out _);

        bool stepsKeywordPresent = forDeclaration.IndexOf("steps") != -1;
        int steps = stepsKeywordPresent ? int.Parse(forDeclaration[forDeclaration.IndexOf("steps") + 1]) : 1;

        string indexVar = $"index_{collectionIdentifier}";
        string sizeVar = $"size_{collectionIdentifier}";

        string collectionName = $"collection_{collectionIdentifier}";

        string constructedCode =
        $$""""
        {{collectionName}} = __Collection({{collectionIdentifier}}, {{steps}});
        {{indexVar}} = 0;
        {{sizeVar}} = {{collectionName}}.count;
        while {{indexVar}} < {{sizeVar}}
        {
            {{forLoopVariable}} = {{collectionName}}.Get({{indexVar}});

            BODYCODEHERE

            {{indexVar}} = {{indexVar}} + 1;
        }
        """";

        StringBuilder constructedParts = new();
        foreach (string bodyPart in body)
            constructedParts.Append(bodyPart).Append(' ');

        constructedCode = constructedCode.Replace("BODYCODEHERE", constructedParts.ToString());
        constructedCode = constructedCode.Replace("__index", indexVar);

        var forloopTokens = Tokenize(constructedCode);
        tokens.AddRange(forloopTokens);
        ;
    }

    private static void RemoveSpecialCharacters(string[] parts)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }
    }
}