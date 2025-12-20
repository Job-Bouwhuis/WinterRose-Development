namespace WinterRose.WinterThornScripting.DefaultLibrary;

internal class Math : CSharpClass
{
    public void Constructor(Variable[] args)
    {
    }

    public Class GetClass()
    {
        Math newMath = new();
        Class c = new("Math", "A class that contains math functions");
        c.CSharpClass = newMath;

        c.DeclareFunction(new Function("Abs", "Returns the absolute value of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Abs(d)
        });
        c.DeclareFunction(new Function("Sqrt", "Returns the square root of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Sqrt(d)
        });
        c.DeclareFunction(new Function("Pow", "Raises a number to the power of another", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.Pow(x, y)
        });
        c.DeclareFunction(new Function("Log", "Returns the logarithm of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Log(d)
        });
        c.DeclareFunction(new Function("Sin", "Returns the sinus of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Sin(d)
        });
        c.DeclareFunction(new Function("Cos", "Returns the cosinus of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Cos(d)
        });
        c.DeclareFunction(new Function("Tan", "Returns the tangens of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Tan(d)
        });
        c.DeclareFunction(new Function("Asin", "Returns the arcus sinus of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Asin(d)
        });
        c.DeclareFunction(new Function("Acos", "Returns the arcus cosinus of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Acos(d)
        });
        c.DeclareFunction(new Function("Atan", "Returns the arcus tangens of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Atan(d)
        });
        c.DeclareFunction(new Function("Atan2", "Returns the arcus tangens of a number", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.Atan2(x, y)
        });
        c.DeclareFunction(new Function("Ceiling", "Returns the smallest integer greater than or equal to the specified number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Ceiling(d)
        });
        c.DeclareFunction(new Function("Floor", "Returns the largest integer less than or equal to the specified number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Floor(d)
        });
        c.DeclareFunction(new Function("Round", "Rounds a value to the nearest integer or to the specified number of fractional digits", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Round(d)
        });
        c.DeclareFunction(new Function("RoundBy", "Rounds a value to the nearest integer or to the specified number of fractional digits", AccessControl.Public)
        {
            CSharpFunction = (double d, double decimals) => System.Math.Round(d, (int)decimals)
        });
        c.DeclareFunction(new Function("Truncate", "Calculates the integral part of a specified decimal number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Truncate(d)
        });
        c.DeclareFunction(new Function("Min", "Returns the smaller of two numbers", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.Min(x, y)
        });
        c.DeclareFunction(new Function("Max", "Returns the larger of two numbers", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.Max(x, y)
        });
        c.DeclareFunction(new Function("Clamp", "Clamps a number between two values", AccessControl.Public)
        {
            CSharpFunction = (double x, double min, double max) => System.Math.Clamp(x, min, max)
        });
        c.DeclareFunction(new Function("Sign", "Returns a value indicating the sign of a number", AccessControl.Public)
        {
            CSharpFunction = (double d) => System.Math.Sign(d)
        });
        c.DeclareFunction(new Function("CopySign", "Returns a value with the magnitude of x and the sign of y", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.CopySign(x, y)
        });
        c.DeclareFunction(new Function("IEEERemainder", "Returns the remainder resulting from the division of a specified number by another specified number", AccessControl.Public)
        {
            CSharpFunction = (double x, double y) => System.Math.IEEERemainder(x, y)
        });
        c.DeclareFunction(new Function("Percentage", "Returns the percentage of what cur is in max", AccessControl.Public)
        {
            CSharpFunction = (double cur, double max) => cur / max * 100
        });

        // Mathematical constants
        c.DeclareVariable(new Variable("PI", "The value of PI", System.Math.PI, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("PI2", "The value of PI times 2", System.Math.PI * 2, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("PIover2", "The value of PI", System.Math.PI / 2, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("PIover4", "The value of PI", System.Math.PI / 4, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("E", "The value of E", System.Math.E, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("Tau", "The value of Tau", System.Math.Tau, AccessControl.Public) { ReadOnly = true });
        c.DeclareVariable(new Variable("GoldenRatio", "The golden ratio", 1.618033988749895, AccessControl.Public) { ReadOnly = true });

        return c;
    }
}
