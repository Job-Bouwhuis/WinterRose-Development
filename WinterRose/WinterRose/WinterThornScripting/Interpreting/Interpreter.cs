using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.Expressions;
using WinterRose.WinterThornScripting.Generation;

namespace WinterRose.WinterThornScripting.Interpreting;

/// <summary>
/// Interprets code from WinterThorn
/// </summary>
/// <param name="block"></param>
public class Interpreter(Block block)
{
    /// <summary>
    /// All tokens in the block. we will iterate through these and execute them.
    /// </summary>
    List<Token> Tokens => block.Tokens;
    int tokenIndex = 0;

    /// <summary>
    /// The current token we are executing.
    /// </summary>
    private Token CurrentToken => Tokens[tokenIndex];
    /// <summary>
    /// The next token we will execute. Sometimes we need to look ahead.
    /// </summary>
    private Token NextToken => Tokens[tokenIndex + 1];
    /// <summary>
    /// The previous token we executed. Sometimes we need to look back.
    /// </summary>
    private Token PreviousToken => Tokens[tokenIndex - 1];

    /// <summary>
    /// The current value we are working with. This is used for operators, assignments, function returns, etc.
    /// </summary>
    Variable workingValue = Variable.Null;

    private Class sender;

    public string ReadableTokens => humanReadableTokenList();

    string humanReadableTokenList()
    {
        StringBuilder result = new();
        foreach (var token in Tokens)
        {
            if (token.Identifier is ";" || token.Identifier.EndsWith(";") || token.Identifier.EndsWith(':'))
                result.Append(token.Identifier + "\n");
            else if (token.Identifier is "{" or "}")
                result.Append("\n" + token.Identifier + "\n");
            else
                result.Append(token.Identifier + " ");
        }
        return result.ToString();
    }


    /// <summary>
    /// Interprets the block. This is the main entry point for the interpreter.
    /// </summary>
    /// <param name="options"></param>
    /// <returns>Whatever the result was of the interpreting based on whether it was called from the interpeter itself, a function, or a loop</returns>
    internal Variable Interpret(Class sender, InterpretOptions? options = null)
    {
        this.sender = sender;
        options ??= new();
        for (tokenIndex = 0; tokenIndex < Tokens.Count; tokenIndex++)
        {
            Token token = CurrentToken;

            if (workingValue is GotoBreak gotoBreak)
            {
                if (gotoBreak.CountOfBlocksToBreakOut == 0)
                {
                    tokenIndex = gotoBreak.LabelIndex;
                    workingValue = Variable.Null;
                    continue;
                }
                else
                {
                    gotoBreak.CountOfBlocksToBreakOut--;
                    return gotoBreak;
                }
            }
           
            switch (token.Type)
            {
                case TokenType.AccessControl:
                    break;
                case TokenType.IfClause:
                    var res = HandleIfClause();
                    if (res.Value is SpecialKeyword)
                        return res;
                    if (res is GotoBreak gtb)
                    {
                        workingValue = gtb;
                        continue;
                    }
                    break;
                case TokenType.Loop: 
                    break;
                case TokenType.Operator:
                    if (options.FromFunction) 
                        break;
                    bool b = HandleOperator();
                    if (b)
                        goto EndOfLoop;
                    break;
                case TokenType.AssignVariable:
                    workingValue = HandleVariableAssignment();
                    break;
                case TokenType.Comment:
                    break;
                case TokenType.Identifier:
                case TokenType.FunctionParameter:
                case TokenType.String:
                case TokenType.Boolean:
                case TokenType.Number:
                    workingValue = GetIdentifierValue(token.Identifier);
                    break;
                case TokenType.Function:
                    HandleFunctionCall();
                    break;
                case TokenType.Return:
                    return HandleReturn();
                case TokenType.New:
                    return HandleCreateInstance();
                case TokenType.Accessor:
                    if (HandleAccessor(out var var))
                        return var;
                    break;
                case TokenType.Break:
                    return Variable.Break;
                case TokenType.Continue:
                    return Variable.Continue;
                case TokenType.Goto:
                    // if we get a goto, we search for the label, if the label is not in the current block, we return a goto break.
                    // if the label is in the current block, we set the token index to the label index and continue.
                    if (HandleGoto() is GotoBreak e)
                    {
                        e.CountOfBlocksToBreakOut--;
                        return e;
                    }
                    break;
                case TokenType.Label: break; // we dont do anything with labels, they are just a marker for gotos
                default:
                    break;
            }
        }
EndOfLoop:
        if (options.FromFunction)
            return Variable.Null;
        return workingValue;
    }

    private GotoBreak? HandleGoto()
    {
        // search for a label with the same name as the identifier of the next token.
        // this label can be anywhere in the block, so we need to search the entire block, or even parent blocks

        // get the identifier of the next token
        string identifier = NextToken.Identifier;

        // search for the label
        var index = block.GetLabel(identifier);

        // if the label was not found, throw an error
        if (index.LabelIndex == -1)
            throw new WinterThornCompilationError(ThornError.NullReference, "WS-0012", $"Label {identifier} not found.");

        // set the token index to the index of the label
        if (index.CountOfBlocksToBreakOut == 0)
        {
            tokenIndex = index.LabelIndex;

            return null;
        }
        else
        {
            return index;
        }

    }
    /// <summary>
    /// Handles operator expressions.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="WinterThornCompilationError"></exception>
    /// <exception cref="WinterThornExecutionError"></exception>
    private bool HandleOperator()
    {
        if (Tokens.All(x => x.Type is TokenType.Identifier or TokenType.Operator or TokenType.OpenParenthesis or TokenType.CloseParenthesis) && Tokens.Any(x => MathSolver.OperatorPrecedence.ContainsKey(x.Identifier)))
        {
            workingValue = MathSolver.Solve(Tokens, ref block);
            tokenIndex = Tokens.FindIndex(tokenIndex, token => token.Type == TokenType.Semicolon);
            return tokenIndex == -1;
        }
        if (Tokens.All(x => x.Type is
        TokenType.Identifier or
        TokenType.Operator or
        TokenType.And or
        TokenType.Or or
        TokenType.Null or
        TokenType.Xor or
        TokenType.Boolean) && Tokens.Any(x => BooleanSolver.BooleanOperators.Contains(x.Identifier)))
        {
            workingValue = BooleanSolver.Solve(Tokens, ref block);
            // set token index to the next semicolon
            tokenIndex = Tokens.FindIndex(tokenIndex, token => token.Type == TokenType.Semicolon);

            return tokenIndex == -1;
        }
        if (workingValue.Type == VariableType.String)
        {
            return StringConcatenation();
        }

        return false;
    }

    private bool StringConcatenation()
    {
        // we decrease the current index to include the current token in the tokens to be evaluated for the string
        tokenIndex--;
        // we get all the tokens until the next semicolon
        var tokens = GetTokensUntil(x => x.Type == TokenType.Semicolon, false);
        // check if all the operator tokens are + operators
        bool allAdditiveOperators = tokens.Where(x => x.Type == TokenType.Operator).All(x => x.Identifier == "+");

        // if all the operators are + operators, we can just concatenate the strings
        if (allAdditiveOperators)
        {
            string result = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                tokenIndex = i;
                Token? item = tokens[i];
                if (item.Type is not TokenType.Identifier and not TokenType.Boolean and not TokenType.Number and not TokenType.String)
                {
                    continue;
                }

                Variable v = GetIdentifierValue(item.Identifier);

                if (i + 1 != tokens.Count)
                    if (tokens[i + 1].Type == TokenType.Accessor)
                    {
                        Variable workingVar = workingValue;
                        workingValue = v;
                        tokenIndex += 1;
                        if (!HandleAccessor(out Variable va))
                            v = va;
                        else
                            v = workingValue;
                        workingValue = workingVar;
                        tokenIndex = i += 2;
                    }

                result += v.Value?.ToString()
                    ?? throw new WinterThornExecutionError(ThornError.NullReference | ThornError.StringConcatFault, "WT-0008", "Object not set to an instance of an object while concatenating a string.");
            }
            workingValue.Value = result;

            return tokenIndex == -1;
        }
        throw new WinterThornExecutionError(ThornError.StringConcatFault, "WT-00015", "Wrong operators used for string concatenations");
    }

    /// <summary>
    /// Handles the accessor operator (eg: person.age)
    /// </summary>
    /// <param name="var"></param>
    /// <returns></returns>
    /// <exception cref="WinterThornCompilationError"></exception>
    private bool HandleAccessor(out Variable var)
    {
        Variable toAccess = workingValue;

        bool wasFunction = false;

        if (toAccess.Type is VariableType.Class or VariableType.CSharpDelegate)
        {
            // consume current token
            tokenIndex++;

            // fetch the identifier to access
            string identifier = CurrentToken.Identifier;

            // fetch the variable to access
            Variable variable = (toAccess.Value as Class).Block[identifier];
            if (variable.Type is VariableType.Function)
            {
                HandleFunctionCall(variable.Value as Function);
                wasFunction = true;
                if (!(variable.Value as Function)!.ReturnsValue)
                    wasFunction = false;
            }
            else if (variable.Type is VariableType.Boolean or VariableType.Number or VariableType.String or VariableType.Class or VariableType.CSharpDelegate)
                workingValue = variable;
            else if (variable.Type is VariableType.Null && NextToken.Type == TokenType.AssignVariable)
                workingValue = variable;
            else if (variable.Type is VariableType.Null)
                throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0007", $"Cannot access null value.");
            else
                throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0006", $"Expected function, got {variable.Type}.");
        }

        var = workingValue;
        return wasFunction;
    }
    /// <summary>
    /// Handles the creation of a new instance of a class.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="WinterThornCompilationError"></exception>
    private Variable HandleCreateInstance()
    {
        if (CurrentToken.Type != TokenType.New)
        {
            throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0001", $"Expected 'new' keyword, got {CurrentToken.Type}.");
        }

        // consume this token
        tokenIndex++;

        // fetch the class name
        string className = CurrentToken.Identifier;

        // construct arguments to pass to the constructor
        List<Token> tokens = GetTokensUntil(token => token.Type == TokenType.CloseParenthesis, true);
        2.Repeat(x => tokens.RemoveAt(0));

        List<Variable> args = [];

        foreach (var token in tokens)
        {
            if (token.Identifier != ",")
            {
                Block b = new Block(block)
                {
                    Tokens = [token]
                };

                Interpreter parameterInterpreter = new(b);

                args.Add(parameterInterpreter.Interpret(sender));
            }
        }

        {
            Class? cls = block[className]?.Value as Class;
            if (cls is null)
                throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0001", $"Class '{className}' not found in the current context.");

            return new Variable(className, "", cls.CreateInstance([.. args]));
        }
    }
    /// <summary>
    /// Handles a function call.
    /// </summary>
    /// <param name="functionToInvoke"></param>
    /// <exception cref="WinterThornCompilationError"></exception>
    private void HandleFunctionCall(Function? functionToInvoke = null)
    {
        Function func;
        if (functionToInvoke is not null)
            func = functionToInvoke;
        else
            func = block[CurrentToken.Identifier]?.Value as Function
            ?? throw new WinterThornCompilationError(
                ThornError.SyntaxError, "WS-0002",
                $"Function '{CurrentToken.Identifier}' does not exist in the current context");

        // fetch the arguments to pass to the function
        List<Token> tokens = GetTokensUntil(token => token.Type == TokenType.CloseParenthesis, true);
        2.Repeat(x => tokens.RemoveAt(0));
        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Type == TokenType.Comma)
            {
                tokens.RemoveAt(i);
                i--;
            }
        }
        Parameter[] parameters = func.Parameters;
        if (parameters is null)
        {
            if (!func.IsCSharpFunction)
                throw new WinterThornExecutionError(ThornError.SyntaxError, "WS-0015", $"Function '{func.Name}' paremeter error");
            int num = func.CSharpFunction.Method.GetParameters().Length;
            parameters ??= new Parameter[num];
        }

        if (parameters.Length != tokens.Count)
            throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0001", $"Expected {parameters.Length} arguments, got {tokens.Count}.");


        Variable[] arguments = new Variable[parameters.Length];

        // assign variables to the arguments
        for (int i = 0; i < parameters.Length; i++)
        {
            arguments[i] = new Variable(parameters[i]?.Class?.Name ?? "Parameter", parameters[i]?.Name ?? "", AccessControl.Private)
            {
                Value = GetIdentifierValue(tokens[i].Identifier).Value
            };
        }

        // validate the arguments passed to the function, but only if its not a C# function
        if (!func.IsCSharpFunction)
            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Type.ToString().Equals(parameters[i].Class.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (arguments[i].Type == VariableType.Class)
                    {
                        Class c = (Class)arguments[i].Value!;
                        if (c.Name == parameters[i].Class.Name)
                            continue;
                    }

                    throw new WinterThornCompilationError(
                        ThornError.SyntaxError, "WS-0001",
                        $"Expected argument {i + 1} to be of type {parameters[i].Class.Name}, got {arguments[i].Type}.");
                }
            }

        Variable result = func.Invoke(arguments);

        if (func.ReturnsValue)
        {
            workingValue = result;
        }
    }
    /// <summary>
    /// Handles the if clause of an if statement.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="WinterThornCompilationError"></exception>
    private Variable HandleIfClause()
    {
        // consume this token
        tokenIndex++;

        // get the expression for the if clause to evaluate. (eg: 5 > 6)
        List<Token> tokens = GetTokensUntil(token => token.Type == TokenType.OpenBrace, true);
        // create a BlockMock so that we keep the reference to the variables in the current block,
        // but execute other tokens. in this case the tokens that make up the expression
        Block mblock = new(block)
        {
            Tokens = tokens
        };
        // we create a new interpreter instance to interpret the expression
        Interpreter interpreter = new(mblock);

        // and we interpret it
        Variable value = interpreter.Interpret(sender);

        // check if the expression is a boolean expression. if not, throw an error
        if (value.Type == VariableType.CSharpDelegate)
        {
            value = new Variable("delegVal", "", value.Value, AccessControl.Private);
        }
        if (value.Type != VariableType.Boolean)
            throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0005", $"Expected boolean expression, got {value.Type}.");

        // since no error was thrown we can assume the expression returned a boolean value
        // we check if the expression evaluated to true
        // if so we execute the block of code that follows the if clause

        tokens = GetBody(out int bodyEnd);

        if ((bool)value.Value)
        {
            Interpreter ifBlockInterpreter = new(new Block(block)
            {
                Tokens = tokens
            });

            var res = ifBlockInterpreter.Interpret(sender);
            if (res is GotoBreak e)
            {
                return e;
            }
            // set the token to the next closing bracket
            tokenIndex = bodyEnd;
            if (tokenIndex + 1 < Tokens.Count && NextToken.Type == TokenType.ElseClause)
            {
                tokenIndex++;
            }

            if (res.Value is Break)
                return res;
        }
        // if the expression evaluated to false we check if there is an else clause
        // if there is we execute the block of code that follows the else clause
        // 'else if' is not yet supported
        else
        {
            tokenIndex = bodyEnd;
            // this method works roughly the same as the if clause, except it doesnt evaluate an expression, it just executes the block of code
            return HandleElseClause();
        }
        return Variable.Null;
    }
    /// <summary>
    /// Handles the else clause of an if statement
    /// </summary>
    /// <returns></returns>
    private Variable HandleElseClause()
    {
        // if there is no else clause, return null
        if (tokenIndex + 1 >= Tokens.Count || NextToken.Type != TokenType.ElseClause)
            return Variable.Null;

        // we only reach this point if the if clause evaluated to false and there is an else clause.
        // we do not need to check if the expression evaluated to false

        // consume this token and the else clause token
        tokenIndex += 2;

        // fetch the else clause block
        List<Token> tokens = GetBody(out int bodyEnd);

        //get the block to execute
        Block elseBlock = new(block)
        {
            Tokens = tokens
        };

        // create a new interpreter instance to interpret the block
        Interpreter elseInterpreter = new(new Block(block)
        {
            Tokens = tokens
        });

        // interpret the block
        var res = elseInterpreter.Interpret(sender);

        // set the token index to the next closing bracket
        tokenIndex = bodyEnd;


        if (res is GotoBreak e)
            return e;
        return res;
    }
    /// <summary>
    /// Handles the <c>return</c> keyword 
    /// </summary>
    /// <returns></returns>
    private Variable HandleReturn()
    {
        // consume this token
        tokenIndex++;
        // fetch the expression to return 
        List<Token> tokens = GetTokensUntil(token => token.Type == TokenType.Semicolon, true);

        // interpret the expression as a new interpreter instance
        Block mblock = new(block)
        {
            Tokens = tokens
        };
        Interpreter interpreter = new(mblock);

        // get the value of the expression and return it
        Variable identifierValue = interpreter.Interpret(sender);
        return identifierValue;
    }
    /// <summary>
    /// Handles variable assignment, eg: <c>x = 5;</c>
    /// </summary>
    /// <returns></returns>
    private Variable HandleVariableAssignment()
    {
        // consume this token
        tokenIndex++;

        // get the expression to assign to the variable
        List<Token> tokens = GetTokensUntil(token => token.Type == TokenType.Semicolon, true);

        // if the expression involves only math, we can solve it and assign the value to the variable without creating a new interpreter instance
        if (tokens.All(x => x.Type is TokenType.Identifier or TokenType.Operator) && tokens.Any(x => x.Type == TokenType.Operator))
        {
            workingValue.Value = MathSolver.Solve(tokens, ref block).Value;
            return workingValue;
        }

        // interpret the expression as a new interpreter instance
        Block mblock = new(block)
        {
            Tokens = tokens
        };
        Interpreter interpreter = new(mblock);

        Variable identifierValue = interpreter.Interpret(sender);
        workingValue.Value = identifierValue.Value;

        return workingValue;
    }
    /// <summary>
    /// Gets all tokens until a certain token is encountered. If <paramref name="consume"/> is true, the token is also consumed
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="consume"></param>
    /// <returns></returns>
    private List<Token> GetTokensUntil(Func<Token, bool> filter, bool consume)
    {
        List<Token> tokens = [];
        int tokenIndex = this.tokenIndex;
        while (tokenIndex < Tokens.Count && !filter(CurrentToken))
        {
            tokens.Add(Tokens[tokenIndex]);
            if (consume)
            {
                this.tokenIndex++;
            }
            tokenIndex++;
        }
        return tokens;
    }
    /// <summary>
    /// Gets the body of a block of code, eg: <c>{ int x = 5; }</c>
    /// <br></br>
    /// if the body contains nested blocks, the nested blocks are included as well as the part of this block that follows the nested block
    /// </summary>
    /// <param name="consume"></param>
    /// <returns></returns>
    private List<Token> GetBody(out int bodyEnd)
    {
        int index = tokenIndex;
        int openBraces = 0;
        List<Token> tokens = [];

        while (index < Tokens.Count)
        {
            Token token = Tokens[index];
            if (token.Type == TokenType.OpenBrace)
            {
                openBraces++;
            }
            else if (token.Type == TokenType.CloseBracket)
            {
                openBraces--;
            }

            tokens.Add(token);

            if (openBraces == 0)
            {
                bodyEnd = index;
                return tokens;
            }
            index++;

        }
        bodyEnd = -1;
        return [];
    }
    /// <summary>
    /// Gets the value of an identifier
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    /// <exception cref="WinterThornCompilationError"></exception>
    private Variable GetIdentifierValue(string identifier)
    {
        if (identifier is "this")
            return new Variable("THIS variable", "", sender, AccessControl.Public);
        // Search for the identifier in 'block' and return the associated Variable
        if (block[identifier] is Variable var)
        {
            // Return the variable if found
            return var;
        }
        else if (identifier.All(x => x.IsNumber()))
        {
            if (double.TryParse(identifier, out double value))
            {
                return new Variable("number", "", value);
            }
            else
            {
                // we throw an compilation error because it is something that cant be changed at runtime
                throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0004", $"Could not parse '{identifier}' as a number.");
            }
        }
        else if (identifier.StartsWith('"') && identifier.EndsWith('"'))
        {
            return new Variable("string", "", identifier);
        }
        else
        {
            // if the next token is an assignment operator, we declare a new variable with the identifier.
            // this is done because the variable will be declared right after this statement is executed
            // and we need to return the variable so that it can be assigned a value
            if (NextToken.Type == TokenType.AssignVariable)
            {
                block.DeclareVariable(new Variable(identifier, "", AccessControl.Private));
                return block[identifier]!;
            }
            throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0003", $"Identifier '{identifier}' not found in the current block.");
        }
    }
}
