using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting.DefaultLibrary;

internal class Random : CSharpClass
{
    int seed = -1;

    public void Constructor(Variable[] args)
    {
        if(args is [Variable var] && var.Type == VariableType.Number)
        {
            seed = int.Parse(var.Value.ToString());
        }
        throw new WinterThornExecutionError(ThornError.InvalidParameters, "WR-345", "The random class needs 1 number argument as a seed.");
    }

    public Class GetClass()
    {
        System.Random rnd = new System.Random();
        if(seed != -1)
            rnd = new System.Random(seed);

        Random resultclass = new();
        Class result = new(nameof(Random), "")
        { CSharpClass = resultclass };

        Function nextFunc = new Function("Random", "Gets a random value", AccessControl.Public);
        nextFunc.CSharpFunction = () => (double)rnd.Next();
        result.DeclareFunction(nextFunc);

        nextFunc = new Function("Max", "Gets a random value", AccessControl.Public);
        nextFunc.CSharpFunction = (double max) => (double)rnd.Next(0, (int)max);
        result.DeclareFunction(nextFunc);

        nextFunc = new Function("MinMax", "Gets a random value", AccessControl.Public);
        nextFunc.CSharpFunction = (double min, double max) => (double)rnd.Next((int)min, (int)max);
        result.DeclareFunction(nextFunc);

        return result;
    }
}
