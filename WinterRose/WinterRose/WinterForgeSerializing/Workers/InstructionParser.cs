using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.WinterForgeSerializing
{
    public static class InstructionParser
    {
        public static List<Instruction> ParseOpcodes(Stream stream)
        {
            var instructions = new List<Instruction>();

            StreamReader reader = new StreamReader(stream);

            string? rawLine;
            while ((rawLine = reader.ReadLine()) != null)
            {
                var line = rawLine.Trim();

                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                // Remove inline comments
                var commentIndex = line.IndexOf("//");
                if (commentIndex >= 0)
                    line = line[..commentIndex].Trim();

                // Tokenize line: opcode and arguments
                var parts = TokenizeLine(line);
                if (parts.Length == 0)
                    continue;

                // Parse OpCode
                // enum tryparse is slow. opt for numerical opcodes in the future
                // when a parser exists to convert the human readable format into computer format

                OpCode opcode = (OpCode)int.Parse(parts[0]);

                //if (!Enum.TryParse(parts[0], ignoreCase: true, out OpCode opcode))
                    //throw new Exception($"Invalid opcode: {parts[0]}");

                // Add instruction
                instructions.Add(new Instruction(opcode, parts.Skip(1).ToArray()));
            }

            return instructions;
        }

        private static string[] TokenizeLine(string line)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool insideQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                    if (!insideQuotes && sb.Length == 0)
                    {
                        tokens.Add("");
                        continue;
                    }
                }
                else if (char.IsWhiteSpace(c) && !insideQuotes)
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens.ToArray();
        }
    }

}
