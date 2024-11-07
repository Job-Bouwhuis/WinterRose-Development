using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using WinterRose;
using System.Linq;

namespace WinterRose.CrystalScripting.Legacy
{
    public class CodeInterpreterLegacy
    {
        private static List<string> keywords = new() { "RETURN", "FOR", "IF", "FUNCTION" };

        private Dictionary<string, double> defaultVariables;
        private Dictionary<string, double> variables;
        private Dictionary<string, Func<double[], double>> functions;
        private Dictionary<string, string> customFunctions;

        public CodeInterpreterLegacy()
        {
            defaultVariables = new();
            variables = new();
            functions = new();
            customFunctions = new();
        }

        public void AddFunction(string name, Func<double[], double> function)
        {
            functions[name] = function;
        }

        public void AddVariable(string name, double value)
        {
            defaultVariables[name] = value;
        }

        public bool FunctionExists(string name) => functions.ContainsKey(name);

        public double Evaluate(string expression, Dictionary<string, double> variables)
        {
            this.variables = variables ?? new();
            foreach (var v in defaultVariables)
                this.variables[v.Key] = v.Value;

            expression = expression.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);
            int index = 0;

            double result = ParseExpression(ref expression, ref index);
            return result;
        }

        private double ParseExpression(ref string expression, ref int index)
        {
            double left = ParseTerm(ref expression, ref index);

            while (PeekNextCharacter(ref expression, index) == '+' || PeekNextCharacter(ref expression, index) == '-')
            {
                char op = NextCharacter(ref expression, ref index);

                double right = ParseTerm(ref expression, ref index);
                if (op == '+')
                    left += right;
                else
                    left -= right;
            }

            if (PeekNextCharacter(ref expression, index) == '=')
            {
                // Extract the variable identifier from the assignment expression
                int identifierStartIndex = expression.LastIndexOf(';', index - 1) + 1;
                int identifierLength = index - identifierStartIndex;
                string identifier = expression.Substring(identifierStartIndex, identifierLength).Trim();

                if (string.IsNullOrEmpty(identifier))
                    throw new ArgumentException("Invalid variable name");

                NextCharacter(ref expression, ref index);
                // Extract the entire assignment expression
                StringBuilder assignmentExpressionBuilder = new StringBuilder();
                char currentChar = NextCharacter(ref expression, ref index);
                while (currentChar != ';' && currentChar != '\0')
                {
                    assignmentExpressionBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }

                string assignmentExpression = assignmentExpressionBuilder.ToString().Trim();
                if (string.IsNullOrEmpty(assignmentExpression))
                    throw new ArgumentException("Invalid assignment expression");

                // Evaluate the assignment expression
                double expressionResult = EvaluateExpression(assignmentExpression);

                variables[identifier] = expressionResult;
                if (index + 1 >= expression.Length)
                    return expressionResult;
            }
            string remaining = expression[index..];

            if (remaining.StartsWith("FUNCTION"))
            {
                index += 8;
                // Extract the loop variable definition
                StringBuilder functionBuilder = new StringBuilder();
                StringBuilder functionNameBuilder = new();
                StringBuilder functionParametersBuilder = new();
                char currentChar = NextCharacter(ref expression, ref index);
                int numberOfOpenBrackets = 0;
                bool openedVariableBracket = false;
                bool closedVariableBracket = false;
                while (true)
                {
                    if (currentChar == '(')
                    {
                        if (openedVariableBracket)
                            throw new InvalidOperationException("Invalid method decleration.");
                        openedVariableBracket = true;
                        currentChar = NextCharacter(ref expression, ref index);
                    }
                    if (currentChar == ')')
                    {
                        if (!openedVariableBracket)
                            throw new InvalidOperationException("Invalid method decleration.");
                        closedVariableBracket = true;
                        currentChar = NextCharacter(ref expression, ref index);
                    }

                    if (currentChar == '{')
                    {
                        numberOfOpenBrackets++;
                        currentChar = NextCharacter(ref expression, ref index);
                    }
                    if (currentChar == '}')
                    {
                        if (numberOfOpenBrackets == 1)
                            break;
                        numberOfOpenBrackets--;
                        currentChar = NextCharacter(ref expression, ref index);

                        if (numberOfOpenBrackets < 0)
                            throw new InvalidOperationException("Invalid method decleration.");
                    }

                    if (!openedVariableBracket)
                    {
                        functionNameBuilder.Append(currentChar);
                    }
                    else
                    {
                        if (closedVariableBracket)
                            functionBuilder.Append(currentChar);
                        else
                            functionParametersBuilder.Append(currentChar);
                    }
                    currentChar = NextCharacter(ref expression, ref index);
                }
            }
            if (remaining.StartsWith("IF("))
            {
                index += 3;

                // Extract the loop variable definition
                StringBuilder conditionBuilder = new StringBuilder();
                char currentChar = NextCharacter(ref expression, ref index);
                int numberOfOpenBrackets = 1;
                while (true)
                {
                    if (currentChar == '(')
                        numberOfOpenBrackets++;
                    if (currentChar == ')')
                    {
                        if (numberOfOpenBrackets == 1)
                            break;
                        numberOfOpenBrackets--;
                    }

                    conditionBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }

                string condition = conditionBuilder.ToString().Trim();
                if (string.IsNullOrEmpty(condition))
                    throw new ArgumentException("Invalid loop variable definition");

                bool conditionResult = EvaluateCondition(condition);
                index++;
                string body = ParseBody(ref expression, ref index, out int endBody);
                if (conditionResult)
                    EvaluateExpression(body);
                index = endBody;
            }
            if (remaining.StartsWith("FOR("))
            {
                index += 4;

                // Extract the loop variable definition
                StringBuilder loopVariableBuilder = new StringBuilder();
                char currentChar = NextCharacter(ref expression, ref index);
                while (currentChar != ')' && currentChar != '\0')
                {
                    loopVariableBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }

                string loopVariableDefinition = loopVariableBuilder.ToString().Trim();
                if (string.IsNullOrEmpty(loopVariableDefinition))
                    throw new ArgumentException("Invalid loop variable definition");

                //int startIndex = index;

                // Parse loop variable definition and execute the loop
                string[] parts = loopVariableDefinition.Split(';');
                if (parts.Length != 3)
                    throw new ArgumentException("Invalid loop variable definition");

                string loopVariable = parts[0].Trim()[..parts[0].IndexOf('=')];
                double loopInitialValue = EvaluateExpression(parts[0][(parts[0].IndexOf('=') + 1)..].Trim());
                variables[loopVariable] = loopInitialValue;

                string loopCondition = parts[1].Trim();
                string loopIncrement = parts[2].Trim();
                index++;

                int endBody = 0;

                // Evaluate loop condition
                while (EvaluateCondition(loopCondition))
                {
                    // Execute the loop body
                    EvaluateExpression(ParseBody(ref expression, ref index, out endBody));

                    // Evaluate and apply loop increment
                    EvaluateExpression(loopIncrement);
                }

                variables.Remove(loopVariable);

                index = endBody;
                if (index + 1 >= expression.Length)
                    return 0;
            }
            if (remaining.StartsWith("RETURN"))
            {
                index += 6; // Skip "return" keyword

                // Extract the expression after the "return" keyword
                StringBuilder returnExpressionBuilder = new StringBuilder();
                char currentChar = NextCharacter(ref expression, ref index);
                while (currentChar != ';' && currentChar != '\0')
                {
                    returnExpressionBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }

                string returnExpression = returnExpressionBuilder.ToString().Trim();
                if (string.IsNullOrEmpty(returnExpression))
                    throw new ArgumentException("Invalid return expression");

                // Evaluate and return the expression after "return" keyword
                return EvaluateExpression(returnExpression);
            }

            // Check if there are more characters in the expression
            if (index + 1 < expression.Length)
            {
                // Continue parsing the remaining expression
                double remainingResult = ParseExpression(ref expression, ref index);
                return remainingResult;
            }

            return left;
        }

        private string ParseBody(ref string expression, ref int index, out int endBody)
        {
            int start = index;
            StringBuilder bodyBuilder = new StringBuilder();
            char currentChar = NextCharacter(ref expression, ref index);
            int numberOfOpenBrackets = 1;
            while (true)
            {
                if (currentChar == '{')
                    numberOfOpenBrackets++;
                if (currentChar == '}')
                {
                    if (numberOfOpenBrackets == 1)
                        break;
                    numberOfOpenBrackets--;
                }

                bodyBuilder.Append(currentChar);
                currentChar = NextCharacter(ref expression, ref index);

            }
            endBody = index;
            index = start;
            string body = bodyBuilder.ToString().Trim();

            if (string.IsNullOrEmpty(body))
                throw new ArgumentException("Invalid body");
            return body;
        }

        private double ParseTerm(ref string expression, ref int index)
        {
            double left = ParseFactor(ref expression, ref index);

            while (PeekNextCharacter(ref expression, index) == '*' || PeekNextCharacter(ref expression, index) == '/')
            {
                char op = NextCharacter(ref expression, ref index);

                double right = ParseFactor(ref expression, ref index);
                if (op == '*')
                    left *= right;
                else
                    left /= right;
            }

            return left;
        }

        private double ParseFactor(ref string expression, ref int index)
        {
            char currentChar = NextCharacter(ref expression, ref index);

            if (char.IsDigit(currentChar) || currentChar == '-')
            {
                // Parse number...
                StringBuilder numberBuilder = new StringBuilder();
                numberBuilder.Append(currentChar);

                currentChar = NextCharacter(ref expression, ref index);
                while (char.IsDigit(currentChar) || currentChar == '.')
                {
                    numberBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }
                index--;
                double number = double.Parse(numberBuilder.ToString(), CultureInfo.InvariantCulture);
                return number;
            }
            else if (char.IsLetter(currentChar))
            {
                StringBuilder identifierBuilder = new StringBuilder();
                while (char.IsLetterOrDigit(currentChar))
                {
                    identifierBuilder.Append(currentChar);
                    currentChar = NextCharacter(ref expression, ref index);
                }

                string identifier = identifierBuilder.ToString();

                if (functions.ContainsKey(identifier))
                {
                    List<double> arguments = new List<double>();

                    if (currentChar == '(')
                    {
                        StringBuilder methodBuilder = new StringBuilder();
                        currentChar = NextCharacter(ref expression, ref index);
                        int numberOfOpenBrackets = 1;
                        while (true)
                        {
                            if (currentChar == '(')
                                numberOfOpenBrackets++;
                            if (currentChar == ')')
                            {
                                if (numberOfOpenBrackets == 1)
                                    break;
                                numberOfOpenBrackets--;
                            }

                            methodBuilder.Append(currentChar);
                            currentChar = NextCharacter(ref expression, ref index);

                        }
                        var argumentStrings = methodBuilder.ToString().Split(',').ToList();
                        foreach (int i in argumentStrings.Count)
                        {
                            int tempIndex = 0;
                            string expr = argumentStrings[i];
                            arguments.Add(ParseExpression(ref expr, ref tempIndex));
                        }

                        if (currentChar != ')')
                            throw new ArgumentException("Invalid function call");
                    }

                    return functions[identifier](arguments.ToArray());
                }
                else if (variables.ContainsKey(identifier))
                {
                    // Return variable value...

                    index--;
                    return variables[identifier];
                }
                else
                {
                    if (functions.ContainsKey(identifier))
                        throw new InvalidOperationException($"You cant make a variable with the same name as a function: {identifier}");
                    if (keywords.Contains(identifier))
                        throw new InvalidOperationException($"You cant make a variable with the same name as a keyword: {identifier}");

                    variables[identifier] = 0;
                    index--; // -= identifier.Length;
                    return 0;
                    throw new ArgumentException($"Undefined identifier: {identifier}");
                }
            }
            else if (currentChar == '(')
            {
                // Parse expression within parentheses...
                double result = ParseExpression(ref expression, ref index);

                if (NextCharacter(ref expression, ref index) != ')')
                    throw new ArgumentException("Unbalanced parentheses");

                return result;
            }

            throw new ArgumentException("Invalid expression");
        }

        private char NextCharacter(ref string expression, ref int index)
        {
            if (index < expression.Length)
                return expression[index++];
            else
                return '\0';
        }

        private char PeekNextCharacter(ref string expression, int index)
        {
            if (index < expression.Length)
                return expression[index];
            else
                return '\0';
        }

        private double EvaluateExpression(string expression)
        {
            return Evaluate(expression, variables);
        }

        private bool EvaluateCondition(string condition)
        {
            // Remove any white spaces from the condition string
            condition = condition.Replace(" ", "");

            // Split the condition string by the comparison operators
            string[] parts = condition.Split(new[] { "==", "!=", ">=", ">", "<=", "<" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid condition");

            string leftValueString = parts[0].Trim();
            string rightValueString = parts[1].Trim();

            double leftValue;
            double rightValue;
            bool isLeftValueNumber = double.TryParse(leftValueString, out leftValue);
            bool isRightValueNumber = double.TryParse(rightValueString, out rightValue);

            if (isLeftValueNumber && isRightValueNumber)
            {
                // Both values are numbers
                return EvaluateNumericCondition(leftValue, rightValue, condition);
            }
            else if (isLeftValueNumber && !isRightValueNumber)
            {
                // Left value is a number, right value is a variable
                return EvaluateVariableCondition(leftValue, rightValueString, condition);
            }
            else if (!isLeftValueNumber && isRightValueNumber)
            {
                // Left value is a variable, right value is a number
                return EvaluateVariableCondition(rightValue, leftValueString, condition);
            }
            else
            {
                // Both values are variables
                double leftVariableValue = GetVariableValue(leftValueString);
                double rightVariableValue = GetVariableValue(rightValueString);
                return EvaluateNumericCondition(leftVariableValue, rightVariableValue, condition);
            }
        }

        private double GetVariableValue(string variable)
        {
            if (variables.ContainsKey(variable))
            {
                return variables[variable];
            }
            else
            {
                throw new ArgumentException($"Undefined variable: {variable}");
            }
        }

        private bool EvaluateNumericCondition(double left, double right, string condition)
        {
            if (condition.Contains("=="))
            {
                return left == right;
            }
            else if (condition.Contains("!="))
            {
                return left != right;
            }
            else if (condition.Contains(">="))
            {
                return left >= right;
            }
            else if (condition.Contains(">"))
            {
                return left > right;
            }
            else if (condition.Contains("<="))
            {
                return left <= right;
            }
            else if (condition.Contains("<"))
            {
                return left < right;
            }
            else
            {
                throw new ArgumentException("Invalid condition");
            }
        }
        private bool EvaluateVariableCondition(double number, string variable, string condition)
        {
            double variableValue = GetVariableValue(variable);

            if (condition.Contains("=="))
            {
                return variableValue == number;
            }
            else if (condition.Contains("!="))
            {
                return variableValue != number;
            }
            else if (condition.Contains(">="))
            {
                return variableValue >= number;
            }
            else if (condition.Contains(">"))
            {
                return variableValue > number;
            }
            else if (condition.Contains("<="))
            {
                return variableValue <= number;
            }
            else if (condition.Contains("<"))
            {
                return variableValue < number;
            }
            else
            {
                throw new ArgumentException("Invalid condition");
            }
        }
    }
}


