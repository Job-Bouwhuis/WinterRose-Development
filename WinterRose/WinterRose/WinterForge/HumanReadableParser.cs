using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterForge
{
    public class HumanReadableParser
    {
        private StreamReader reader = null!;
        private StreamWriter writer = null!;
        private string? currentLine;
        private int depth = 0;
        private readonly Stack<LineQueue<string>> lineBuffer = new();

        public void Parse(Stream input, Stream output)
        {
            reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
            writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);

            while ((currentLine = ReadNonEmptyLine()) != null)
            {
                ParseObjectOrAssignment();
            }

            writer.Flush();
            output.Position = 0;
        }

        private void ParseObjectOrAssignment()
        {
            string line = currentLine!.Trim();

            // Constructor Definition: Type(arguments) : ID {
            if (line.Contains('(') && line.Contains(')') && line.Contains(':') && line.Contains('{'))
            {
                int openParenIndex = line.IndexOf('(');
                int closeParenIndex = line.IndexOf(')');
                int colonIndex = line.IndexOf(':');
                int braceIndex = line.IndexOf('{');

                string type = line[..openParenIndex].Trim();
                string arguments = line.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
                string id = line.Substring(colonIndex + 1, braceIndex - colonIndex - 1).Trim();

                var args = arguments.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (string arg in args)
                    WriteLine("PUSH " + arg);
                WriteLine($"DEFINE {type} {id} {args.Length}");
                depth++;
                ParseBlock(id);
            }
            // Constructor Definition with no block: Type(arguments) : ID;
            else if (line.Contains('(') && line.Contains(')') && line.Contains(':') && line.EndsWith(";"))
            {
                int openParenIndex = line.IndexOf('(');
                int closeParenIndex = line.IndexOf(')');
                int colonIndex = line.IndexOf(':');

                string type = line[..openParenIndex].Trim();
                string arguments = line.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
                string id = line.Substring(colonIndex + 1, line.Length - colonIndex - 2).Trim();

                var args = arguments.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (string arg in args)
                    WriteLine("PUSH " + arg);
                WriteLine($"DEFINE {type} {id} {args.Length}");
            }
            // Definition: Type : ID {
            else if (line.Contains(':') && line.Contains('{'))
            {
                int colonIndex = line.IndexOf(':');
                int braceIndex = line.IndexOf('{');

                string type = line[..colonIndex].Trim();
                string id = line.Substring(colonIndex + 1, braceIndex - colonIndex - 1).Trim();

                WriteLine($"DEFINE {type} {id} 0");
                depth++;
                ParseBlock(id);
            }
            else if (line.Contains(':')) // maybe brace is on next line
            {
                string type;
                string id;

                var parts = line.Split(':');
                type = parts[0].Trim();
                id = parts[1].Trim();

                ReadNextLineExpecting("{");

                WriteLine($"DEFINE {type} {id} 0");
                depth++;

                ParseBlock(id);
            }
            else if (line.StartsWith("return"))
            {
                int trimoffEnd = 0;
                if (line.EndsWith(';'))
                    trimoffEnd = 1;
                string ID = line[6..new Index(trimoffEnd, true)].Trim();
                if (string.IsNullOrWhiteSpace(ID) || !ID.All(char.IsDigit))
                    throw new Exception("Invalid ID parameter in RETURN statement");
                WriteLine($"RET {ID}");
            }
            else
            {
                throw new Exception($"Unexpected top-level line: {line}");
            }
        }

        private void ParseBlock(string id)
        {
            while ((currentLine = ReadNonEmptyLine()) != null)
            {
                string line = currentLine.Trim();

                if (line == "}")
                {
                    depth--;
                    WriteLine($"END {id}");
                    return;
                }

                if (line.Contains('=') && line.EndsWith(";"))
                {
                    ParseAssignment(line);
                }
                else if (line.Contains(':'))
                {
                    currentLine = line;
                    ParseObjectOrAssignment(); // nested define
                }
                else if (line.Contains('['))
                {
                    currentLine = line;
                    ParseList();

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex != -1)
                    {
                        string name = line[..equalsIndex].Trim();
                        if (!string.IsNullOrEmpty(name))
                            WriteLine($"SET {name} _stack()");
                    }
                }

                else
                {
                    throw new Exception($"Unhandled block content: {line}");
                }
            }
        }

        private void ParseList()
        {
            int typeOpen = currentLine!.IndexOf("<");
            int typeClose = currentLine.LastIndexOf(">");
            if (typeOpen == -1 || typeClose == -1 || typeClose < typeOpen)
                throw new Exception("Expected <TYPE1,TYPE2,...> to indicate the type(s) of the collection before [");

            string types = currentLine.Substring(typeOpen + 1, typeClose - typeOpen - 1).Trim();
            WriteLine("LIST_START " + types);

            bool insideFunction = false;
            StringBuilder currentElement = new();

            bool collectingDefinition = false;
            int depth = 0;
            int listDepth = 1;
            char? currentChar;

            string remainingOnLine = currentLine[(typeClose + 2)..];

            do
            {
                foreach (char c in remainingOnLine)
                {
                    int res = handleChar(ref insideFunction, currentElement, ref collectingDefinition, ref depth, ref listDepth, c);
                    if (res == -1)
                        return;
                }
            } while ((remainingOnLine = ReadNonEmptyLine()) != null);

            string tmp = currentElement.ToString();

            // If we exit the loop without finding a closing bracket, throw an error
            throw new Exception("Expected ']' to close list.");

            void emitElement()
            {
                if (currentElement.Length > 0)
                {
                    string currentElementString = currentElement.ToString();

                    if (currentElementString.Contains(":"))
                    {
                        lineBuffer.Push(new LineQueue<string>());
                        var lines = currentElementString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines) // reverse because we're using a queue (FIFO)
                            lineBuffer.Peek().Enqueue(line.Trim() + '\n');

                        currentLine = ReadNonEmptyLine();
                        ParseObjectOrAssignment();
                    }


                    // Clear current element after processing
                    currentElement.Clear();
                }
            }

            int handleChar(ref bool insideFunction, StringBuilder currentElement, ref bool collectingDefinition, ref int depth, ref int listDepth, char? currentChar)
            {
                char character = currentChar.Value;

                if (!collectingDefinition && char.IsWhiteSpace(character))
                    return 1;

                if (character == '{')
                {
                    collectingDefinition = true;
                    depth++;
                }

                if (character == '}')
                {
                    depth--;
                    if (depth == 0)
                        collectingDefinition = false;

                }

                if (character == '(')
                {
                    insideFunction = true;
                    currentElement.Append(character);
                    return 1;
                }

                if (character == ')')
                {
                    insideFunction = false;
                    currentElement.Append(character);
                    return 1;
                }

                if (!insideFunction && character == ',')
                {
                    if (listDepth is not 0)
                    {
                        currentElement.Append("\n]");
                        return 1;
                    }
                    emitElement();
                    return 1;
                }

                if (!insideFunction && character == '[')
                {
                    listDepth++;
                }

                if (!insideFunction && character == ']')
                {
                    listDepth--;
                    if (listDepth is not 0)
                    {
                        currentElement.Append("\n]");
                        return 1;
                    }
                    emitElement();
                    lineBuffer.Pop();
                    WriteLine("LIST_END");
                    if (listDepth == 0)
                        return -1;
                    else return 1;
                }

                currentElement.Append(character);
                return 0;
            }

        }

        private void ParseAssignment(string line)
        {
            line = line.TrimEnd(';');
            int eq = line.IndexOf('=');
            string key = line[..eq].Trim();
            string value = line[(eq + 1)..].Trim();

            WriteLine($"SET {key} {value}");
        }

        private string? ReadNonEmptyLine()
        {
            string? line;
            do
            {
                if (lineBuffer.Count > 0)
                {
                    line = lineBuffer.Peek().Dequeue();
                }
                else
                {
                    if (reader.EndOfStream)
                        return null;
                    line = reader.ReadLine();
                }
            } while (string.IsNullOrWhiteSpace(line));

            return line;
        }


        private char? ReadNonEmptyChar(bool acceptEmptyCharsAnyway)
        {
            char? c = null;

            while (true)
            {
                if (lineBuffer.Count > 0
                    && lineBuffer.Peek().Count > 0)
                {
                    string line = lineBuffer.Peek().Peek();

                    if (line.Length > 0)
                    {
                        c = line[0];

                        // Replace the line with the rest, or remove if empty
                        if (line.Length == 1)
                            lineBuffer.Peek().Dequeue();
                        else
                            lineBuffer.Peek().InsertAt(0, line[1..]);

                        if (acceptEmptyCharsAnyway || !char.IsWhiteSpace(c.Value))
                            return c;

                        continue; // Continue if whitespace and we're not accepting empty
                    }
                    else
                    {
                        // Remove empty string
                        lineBuffer.Peek().Dequeue();
                        continue;
                    }
                }

                // Refill line buffer
                string? newLine = reader.ReadLine() + "\n";
                if (newLine == null)
                    return null;

                if (lineBuffer.Count == 0)
                    lineBuffer.Push(new());
                lineBuffer.Peek().Enqueue(newLine);
            }
        }

        private void ReadNextLineExpecting(string expected)
        {
            currentLine = ReadNonEmptyLine();
            if (currentLine == null || currentLine.Trim() != expected)
                throw new Exception($"Expected '{expected}' but got: {currentLine}");
        }

        private void WriteLine(string line)
        {
            writer.WriteLine(line);
        }
    }

}
