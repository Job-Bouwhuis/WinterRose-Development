using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;

namespace WinterRose.CrystalScripting.Legacy.Interpreting
{
    public class CrystalTokenInterpreter
    {
        private int currentIndex;
        private List<Token> tokens;
        private CrystalScope scope;

        public CrystalTokenInterpreter(ref CrystalScope scope) => this.scope = scope;

        public CrystalVariable ExecuteTokens(List<Token> tokens)
        {
            this.tokens = tokens;
            currentIndex = 0;

            CrystalVariable result = CrystalVariable.Null;

            while (currentIndex < tokens.Count)
            {
                Token token = tokens[currentIndex];

                switch (token.Type)
                {
                    case TokenType.Identifier:
                        result = HandleIdentifier(token);
                        break;

                    case TokenType.Keyword:
                        HandleKeyword(token);
                        break;

                    case TokenType.Number:

                    case TokenType.String:
                        // Handle literals
                        result = new CrystalVariable("VariableLiteralObject", CrystalType.ValueFromString(token.Lexeme));
                        break;

                    case TokenType.Operator:
                    case TokenType.Addition:
                    case TokenType.Subtraction:
                    case TokenType.Multiplication:
                    case TokenType.Division:
                    case TokenType.IsEqual:
                    case TokenType.IsInequal:
                    case TokenType.GreaterThan:
                    case TokenType.LessThan:
                    case TokenType.GreaterThanOrEqual:
                    case TokenType.LessThanOrEqual:
                    case TokenType.And:
                    case TokenType.Or:
                        //case TokenType.Not:
                        //case TokenType.Xor:
                        //case TokenType.Modulo:
                        //case TokenType.Increment:
                        //case TokenType.Decrement:
                        //case TokenType.AssignAddition:
                        //case TokenType.AssignSubtraction:
                        //case TokenType.AssignMultiplication:
                        //case TokenType.AssignDivision:
                        //case TokenType.AssignModulo:
                        HandleOperator(token, ref result);
                        break;

                    case TokenType.AssignVariable:
                        HandleVariableAssignment(token, ref result);
                        break;

                    default:
                        // Ignore unknown token types
                        break;
                }

                currentIndex++;
            }
            try
            {
                return result;
            }
            finally
            {
                WinterUtils.Repeat(() => GC.Collect(), 6);
            }
        }

        private Token? FindToken(int startIndex, TokenType type)
        {
            for (int i = startIndex; i < tokens.Count; i++)
                if (tokens[i].Type == type)
                    return tokens[i];
            return null;
        }

        private CrystalVariable HandleVariableAssignment(Token token, ref CrystalVariable varToSet)
        {
            string name = varToSet.Name;
            currentIndex--;
            Token prev = tokens[currentIndex];

            CrystalVariable value = HandleExpression(prev, false);
            value.Name = name;

            if (scope.TryGetVariable(name, out CrystalVariable variable))
            {
                variable.Type = value.Type;
                return variable;
            }
            else
            {
                scope.DeclareVariable(value);
                return value;
            }
        }

        private CrystalVariable HandleVariableAssignment(Token token)
        {
            if (currentIndex + 1 >= tokens.Count)
            {
                var variable = new CrystalVariable(token.Lexeme, CrystalType.FromObject(null));
                scope.DeclareVariable(variable);
                return variable;
            }

            if (tokens[currentIndex + 1].Type is not TokenType.AssignVariable)
            {
                throw new Exception($"Identifier '{token.Lexeme}' not recognized.");
            }

            return HandleExpression(token);
        }

        private bool CrystalFunctionExists(string name)
        {
            return scope.TryGetFunction(name, out _);
        }
        private bool CrystalVariableExists(string name)
        {
            return scope.TryGetVariable(name, out _);
        }
        private Token? GetIdentifyerTokenAtOffset1(Token token)
        {
            if (CrystalFunctionExists(tokens[tokens.IndexOf(token) + 1].Lexeme))
                return tokens[tokens.IndexOf(token) + 1];

            else if (CrystalVariableExists(tokens[tokens.IndexOf(token) + 1].Lexeme))
                return tokens[tokens.IndexOf(token) + 1];

            else if (CrystalVariableExists(tokens[tokens.IndexOf(token) - 1].Lexeme))
                return tokens[tokens.IndexOf(token) - 1];

            else if (CrystalFunctionExists(tokens[tokens.IndexOf(token)].Lexeme))
                return tokens[tokens.IndexOf(token)];

            else
                return null;
        }
        private CrystalVariable HandleExpression(Token token, bool declareVar = true)
        {
            //if(GetIdentifyerTokenAtOffset1(token) is Token t)
            //{
            //    return HandleIdentifier(t);
            //}
            Token semicolonToken = GetNextSemicolon(currentIndex);
            int startIndex;
            if (token.Type is TokenType.Identifier && tokens[tokens.IndexOf(token) + 1].Type is TokenType.AssignVariable || CrystalFunctionExists(tokens[tokens.IndexOf(token) + 1].Lexeme))
                startIndex = tokens.IndexOf(token) + 2;
            else
                startIndex = tokens.IndexOf(token) + 1;

            int semicolonIndex = tokens.IndexOf(semicolonToken);

            if (semicolonIndex == -1)
            {
                throw new Exception("how did you get here");
            }

            List<Token> variableAssignmentTokens = new();
            // fill the list above with everything between 
            variableAssignmentTokens.AddRange(tokens.GetRange(startIndex, semicolonIndex - startIndex + 1));

            CrystalTokenInterpreter interpreter = new(ref scope);
            CrystalVariable result = interpreter.ExecuteTokens(variableAssignmentTokens);
            if (token.Type is TokenType.Identifier)
                result.Name = token.Lexeme;
            else if (token.Type is TokenType.AssignVariable)
            {
                result.Name = tokens[tokens.IndexOf(token) - 1].Lexeme;
            }
            else
                result.Name = tokens[tokens.IndexOf(token) + 1].Lexeme;
            currentIndex = semicolonIndex - 1;
            if (declareVar)
            {
                if (scope.TryGetVariable(result.Name, out CrystalVariable variable))
                {
                    variable.Type = result.Type;
                    return variable;
                }
                else
                {
                    scope.DeclareVariable(result);
                    return result;
                }
            }
            return result;
        }

        private CrystalVariable HandleIdentifier(Token token)
        {
            string identifier = token.Lexeme;
            if (scope.TryGetVariable(identifier, out CrystalVariable value))
            {
                return value;
            }

            // Check if the identifier corresponds to a function
            if (scope.TryGetFunction(identifier, out CrystalFunction function))
            {
                List<CrystalVariable> arguments = new List<CrystalVariable>();
                List<List<Token>> argumentTokens = new List<List<Token>>();
                currentIndex += 2; // Move to the next token
                if (tokens[currentIndex].Type == TokenType.RightParenthesis)
                {
                    currentIndex--;
                }
                if (tokens[currentIndex].Type is TokenType.LeftParenthesis)
                {
                    currentIndex++;
                }
                int argumentSectionEndIndex = tokens.FindIndex(currentIndex, t => t.Type == TokenType.RightParenthesis);

                List<Token> argumentSection = tokens.GetRange(currentIndex, argumentSectionEndIndex - currentIndex + 1);

                List<Token> current = new();
                foreach (Token arg in argumentSection)
                {
                    TokenType t = TokenType.Crystal | TokenType.String | TokenType.Number | TokenType.Boolean | TokenType.Identifier | TokenType.ArgumentSeperator | TokenType.RightParenthesis;
                    if (arg.Type is TokenType.ArgumentSeperator or TokenType.RightParenthesis)
                    {
                        argumentTokens.Add(current);
                        current = new();
                    }
                    else
                    {
                        current.Add(arg);
                    }
                }
                if (current.Count > 0)
                {
                    argumentTokens.Add(current);
                }

                for (int i = 0; i < argumentTokens.Count; i++)
                {
                    List<Token> argTokens = argumentTokens[i];

                    CrystalTokenInterpreter interpreter = new(ref scope);
                    var arg = interpreter.ExecuteTokens(argTokens);
                    if (arg.Type.Name != function.Arguments[i].Type.Name)
                    {
                        throw new CrystalInterpretingException(00003, $"Argument {i} is not of type {function.Arguments[i].Type.Name}, which the function requires");
                    }
                    arguments.Add(arg);
                }

                try
                {
                    return function.Invoke(arguments.ToArray());
                }
                finally
                {
                    currentIndex = argumentSectionEndIndex + 1;
                }
            }

            return HandleVariableAssignment(token);
        }

        private CrystalVariable HandleKeyword(Token token)
        {
            switch (token.Lexeme)
            {
                case "return":
                    //currentIndex++; // Move to the next token

                    // Evaluate the expression after the return keyword
                    CrystalVariable returnValue = HandleExpression(token, false);

                    // Set the result variable and exit the current function
                    throw new ReturnException(returnValue);
                case "if":
                    currentIndex++; // Move to the next token

                    List<Token> conditionTokens = new List<Token>();
                    List<Token> bodyTokens = new List<Token>();

                    int conditionEndIndex = tokens.IndexOf(FindToken(currentIndex, TokenType.LeftBrace)!);
                    int ifBlockEnd = tokens.IndexOf(FindToken(conditionEndIndex, TokenType.RightBrace)!);
                    conditionTokens.AddRange(tokens.GetRange(currentIndex, conditionEndIndex - currentIndex));
                    bodyTokens.AddRange(tokens.GetRange(conditionEndIndex + 1, tokens.IndexOf(FindToken(conditionEndIndex, TokenType.RightBrace)!) - conditionEndIndex - 1));
                    CrystalConditionExpression condition = new CrystalConditionExpression(conditionTokens, scope);

                    //Check if the condition is true
                    if (condition.Evaluate())
                    {
                        CrystalTokenInterpreter interpreter = new CrystalTokenInterpreter(ref scope);
                        interpreter.ExecuteTokens(bodyTokens);
                        currentIndex = ifBlockEnd;
                        return CrystalVariable.True;
                    }
                    else
                    {
                        currentIndex = ifBlockEnd;
                        return CrystalVariable.False;
                    }

                case "null":
                    return CrystalVariable.Null;
                case "true":
                    return new CrystalVariable("VariableLiteralObject", CrystalType.FromObject(true));
                case "false":
                    return new CrystalVariable("VariableLiteralObject", CrystalType.FromObject(false));
                // Add more cases for other keywords as needed
                default:
                    return CrystalVariable.Null;
            }
        }

        private void HandleOperator(Token operatorToken, ref CrystalVariable result)
        {
            // Check if the operator is an operator
            if (operatorToken.IsOperator())
            {
                // Retrieve the left operand and operator token
                CrystalVariable leftOperand = result;
                Token currentOperator = operatorToken;

                currentIndex += 1; // Move to the next token

                // check if the current operator is used for math, eg +, -, *, /
                if (currentOperator.IsMathOperator())
                {
                    // Loop until a different operator or end of tokens is encountered
                    while (currentIndex < tokens.Count)
                    {
                        Token nextToken = tokens[currentIndex];
                        if (nextToken.Type is TokenType.Semicolon)
                            break;
                        //if (nextToken.Type == TokenType.Operator || nextToken.IsOperator())
                        //{
                        //    break; // Exit the loop if a different operator is encountered
                        //}

                        Token nextOperator = nextToken;

                        if (GetOperatorPriority(nextOperator) <= GetOperatorPriority(currentOperator))
                        {
                            //currentIndex++; // Move to the next token
                            CrystalVariable rightOperand = HandleOperand(); // Retrieve the right operand

                            // Perform the operation based on the current operator
                            if (currentOperator.Type == TokenType.Multiplication)
                            {
                                leftOperand *= rightOperand;
                            }
                            else if (currentOperator.Type == TokenType.Division)
                            {
                                if (rightOperand.Type.Equal(CrystalType.FromObject(0)))
                                {
                                    throw new CrystalInterpretingException(00008, "Division by zero is not allowed.");
                                }

                                leftOperand /= rightOperand;
                            }
                            else if (currentOperator.Type == TokenType.Addition)
                            {
                                leftOperand += rightOperand;
                            }
                            else if (currentOperator.Type == TokenType.Subtraction)
                            {
                                leftOperand -= rightOperand;
                            }
                        }
                        else
                        {
                            break; // Exit the loop if the next operator has higher priority
                        }

                        currentOperator = nextOperator;
                        currentIndex++;
                    }
                }
                // if its not, then try to handle it as a boolean
                else
                {
                    CrystalVariable rightOperand = HandleOperand(); // Retrieve the right operand
                    leftOperand = new("OperantEvaluationResult", EvaluateConditionalOperator(currentOperator.Type, leftOperand, rightOperand));
                }

                result = leftOperand;
            }

            CrystalBoolean EvaluateConditionalOperator(TokenType op, CrystalVariable left, CrystalVariable right)
            {
                switch (op)
                {
                    case TokenType.IsEqual:
                        return left.Type.Equal(right.Type);
                    case TokenType.IsInequal:
                        return left.Type.NotEqual(right.Type);
                    case TokenType.GreaterThanOrEqual:
                        return left.Type.GreaterThanOrEqual(right.Type);
                    case TokenType.LessThanOrEqual:
                        return left.Type.LessThanOrEqual(right.Type);
                    case TokenType.And:
                        return right.Type is CrystalBoolean a ? left.Type.And(a) :
                            throw new CrystalInterpretingException(00004, $"left and right side operators of the AND expression must be of type bool. current: {left.Type.Name} and {right.Type.Name}");
                    case TokenType.Or:
                        return right.Type is CrystalBoolean b ? left.Type.Or(b) :
                           throw new CrystalInterpretingException(00004, $"left and right side operators of the AND expression must be of type bool. current: {left.Type.Name} and {right.Type.Name}");
                    case TokenType.GreaterThan:
                        return left.Type.GreaterThan(right.Type);
                    case TokenType.LessThan:
                        return left.Type.LessThan(right.Type);
                    case TokenType.Not:
                        return left.Type.Not();
                }
                return CrystalBoolean.False;
            }
        }

        private CrystalVariable HandleOperand()
        {
            Token operandToken = tokens[currentIndex];

            if (operandToken.Type == TokenType.Identifier)
            {
                return HandleIdentifier(operandToken);
            }
            else if (operandToken.Type is TokenType.Number or TokenType.String)
            {
                return new CrystalVariable("OperantValue", CrystalType.ValueFromString(operandToken.Lexeme));
            }

            throw new CrystalInterpretingException(00007, $"Invalid operand token type: {operandToken}");
        }

        private int GetOperatorPriority(Token operatorToken)
        {
            if (operatorToken.Type is TokenType.Multiplication or TokenType.Division)
            {
                return 2; // Higher priority
            }
            else if (operatorToken.Type is TokenType.Addition or TokenType.Subtraction)
            {
                return 1; // Lower priority
            }

            return 0; // Default priority
        }

        private Token GetNextSemicolon(int startIndex)
        {
            for (int i = Math.Clamp(startIndex, 0, int.MaxValue); i < tokens.Count; i++)
            {
                Token t = tokens[i];
                if (t.Type is TokenType.Semicolon)
                    return t;
            }
            throw new CrystalInterpretingException(0x00001, "Invalid CrystalScript Syntax.\n>> Missing expected semicolon");
        }
    }
}

