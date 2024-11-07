using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Monogame.WinterThornPort
{
    internal class ThornVector : CSharpClass
    {
        public double x = 0, y = 0;

        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            ThornVector newVec = new();
            Class result = new("Vector2", "A vector2");
            result.CSharpClass = newVec;

            result.DeclareVariable(new Variable("x", "The x value of the vector", x, AccessControl.Public)
            {
                Value = () => x,
                Setter = (double var) => x = var
            });

            result.DeclareVariable(new Variable("y", "The y value of the vector", y, AccessControl.Public)
            {
                Value = () => y,
                Setter = (double var) => y = var
            });

            result.DeclareFunction(new Function("Normalize", "Normalizes the vector2", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    Vector2 vec = new((float)x, (float)y);
                    vec.Normalize();
                    ThornVector newVec = new()
                    {
                        x = vec.X,
                        y = vec.Y
                    };
                    return newVec.GetClass();
                }
            });

            return result;
        }

        public override string ToString()
        {
            return $"X: {x}, Y: {y}";
        }
    }
}
