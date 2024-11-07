namespace WinterRose.WinterThornScripting.Interpreting;

public class Operator
{
    public string Identifier { get; set; }
    public int Precedence { get; set; }

    public Operator(string identifier)
    {
        Identifier = identifier;
        Precedence = identifier switch
        {
            "+" => 1,
            "-" => 1,
            "*" => 2,
            "/" => 2,
            _ => throw new WinterThornExecutionError(ThornError.SyntaxError, "WT-0004", $"Invalid operator {identifier} found in equation."),
        };
    }

    public static bool IsOperator(string identifier)
    {
        return identifier is "+" or "-" or "*" or "/";
    }

    public static implicit operator Operator(Token token) => new(token.Identifier);
}