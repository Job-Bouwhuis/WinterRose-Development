using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting.DefaultLibrary;

internal class Console : CSharpClass
{
    public void Constructor(Variable[] args)
    {
    }

    public Class GetClass()
    {
        Console resultclass = new();
        Class result = new(nameof(Console), "Opens a")
        { CSharpClass = resultclass };

        Function Write = new("Write", "Writes a value to the console.", AccessControl.Public);
        Write.CSharpFunction = (Variable value) => resultclass.Write(value);
        result.DeclareFunction(Write);

        Function WriteLine = new("WriteLine", "Writes a value to the console, and then a new line.", AccessControl.Public);
        WriteLine.CSharpFunction = (Variable value) => resultclass.WriteLine(value);
        result.DeclareFunction(WriteLine);

        Function readChar = new("ReadChar", "Reads a single character from the console", AccessControl.Public)
        {
            CSharpFunction = (bool intercept) => System.Console.ReadKey(intercept).KeyChar
        };
        result.DeclareFunction(readChar);

        Function readLine = new("ReadLine", "Reads a line from the console", AccessControl.Public)
        {
            CSharpFunction = () => System.Console.ReadLine()
        };
        result.DeclareFunction(readLine);

        Function setCursorPos = new("SetCursorPosition", "Sets the cursor position in the console to the specifier row and column", AccessControl.Public)
        {
            CSharpFunction = (double left, double top) => System.Console.SetCursorPosition(left.FloorToInt(), top.FloorToInt())
        };
        result.DeclareFunction(setCursorPos);

        Function clear = new("Clear", "Clears the console", AccessControl.Public)
        {
            CSharpFunction = () => System.Console.Clear()
        };
        result.DeclareFunction(clear);

        Variable row = new("left", "The X position (left) of the console cursor", () => System.Console.CursorLeft)
        {
            Setter = (double left) => System.Console.CursorLeft = left.FloorToInt()
        };
        result.DeclareVariable(row);

        Variable top = new("top", "The Y position (top) of the console cursor", () => System.Console.CursorTop)
        {
            Setter = (double top) => System.Console.CursorTop = top.FloorToInt()
        };
        result.DeclareVariable(top);

        Function setForegroundColor = new("SetForegroundColor", "Sets the foreground color of the console based on a color name", AccessControl.Public)
        {
            CSharpFunction = (string colorName) =>
            {
                System.ConsoleColor color;
                try
                {
                    color = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), colorName);
                    System.Console.ForegroundColor = color;
                }
                catch
                {
                    throw new WinterThornExecutionError(ThornError.InvalidParameters, "WT-0010", "The specified color is not valid: " + colorName);
                }
            }
        };
        result.DeclareFunction(setForegroundColor);

        Function setBackgroundColor = new("SetBackgroundColor", "Sets the background color of the console based on a color name", AccessControl.Public)
        {
            CSharpFunction = (string colorName) =>
            {
                System.ConsoleColor color;
                try
                {
                    color = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), colorName);
                    System.Console.BackgroundColor = color;
                }
                catch
                {
                    throw new WinterThornExecutionError(ThornError.InvalidParameters, "WT-0011", "The specified color is not valid: " + colorName);
                }
            }
        };
        result.DeclareFunction(setBackgroundColor);


        result.DeclareVariable(new Variable("Black", "The color Black", () => System.ConsoleColor.Black.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkBlue", "The color DarkBlue", () => System.ConsoleColor.DarkBlue.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkGreen", "The color DarkGreen", () => System.ConsoleColor.DarkGreen.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkCyan", "The color DarkCyan", () => System.ConsoleColor.DarkCyan.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkRed", "The color DarkRed", () => System.ConsoleColor.DarkRed.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkMagenta", "The color DarkMagenta", () => System.ConsoleColor.DarkMagenta.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkYellow", "The color DarkYellow", () => System.ConsoleColor.DarkYellow.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Gray", "The color Gray", () => System.ConsoleColor.Gray.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("DarkGray", "The color DarkGray", () => System.ConsoleColor.DarkGray.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Blue", "The color Blue", () => System.ConsoleColor.Blue.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Green", "The color Green", () => System.ConsoleColor.Green.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Cyan", "The color Cyan", () => System.ConsoleColor.Cyan.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Red", "The color Red", () => System.ConsoleColor.Red.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Magenta", "The color Magenta", () => System.ConsoleColor.Magenta.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("Yellow", "The color Yellow", () => System.ConsoleColor.Yellow.ToString(), AccessControl.Public));
        result.DeclareVariable(new Variable("White", "The color White", () => System.ConsoleColor.White.ToString(), AccessControl.Public));

        return result;
    }

    public void Write(Variable value) => System.Console.Write(value.Value);
    public void WriteLine(Variable value) => System.Console.WriteLine(value.Value);
}
