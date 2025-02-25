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
    internal class ThornColor : CSharpClass
    {
        public Color color = new(255, 255, 255, 255);

        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            ThornColor newColor = new();
            Class result = new("Color", "A color");
            result.CSharpClass = newColor;

            result.DeclareConstructor(new Constructor("Color", "Creates a new color", AccessControl.Public)
            {
                CSharpFunction = (object r, object g, object b, object a) =>
                {
                    if (r is int && g is int && b is int && a is int)
                    {
                        newColor.color = new Color((int)r, (int)g, (int)b, (int)a);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Color constructor can only take ints");
                }
            });
            result.DeclareConstructor(new Constructor("Color", "Creates a new color", AccessControl.Public)
            {
                CSharpFunction = (object r, object g, object b) =>
                {
                    if (r is int && g is int && b is int)
                    {
                        newColor.color = new Color((int)r, (int)g, (int)b);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Color constructor can only take ints");
                }
            });
            result.DeclareVariable(new Variable("r", "The red value of the color", AccessControl.Public)
            {
                Value = () => newColor.color.R,
                Setter = (double var) => newColor.color.R = (byte)var
            });
            result.DeclareVariable(new Variable("g", "The green value of the color", AccessControl.Public)
            {
                Value = () => newColor.color.G,
                Setter = (double var) => newColor.color.G = (byte)var
            });
            result.DeclareVariable(new Variable("b", "The blue value of the color", AccessControl.Public)
            {
                Value = () => newColor.color.B,
                Setter = (double var) => newColor.color.B = (byte)var
            });
            result.DeclareVariable(new Variable("a", "The alpha value of the color", AccessControl.Public)
            {
                Value = () => newColor.color.A,
                Setter = (double var) => newColor.color.A = (byte)var
            });
            result.DeclareFunction(new Function("ToString", "Returns the string representation of the color", AccessControl.Public)
            {
                CSharpFunction = () => newColor.ToString()
            });
            result.DeclareVariable(new Variable("packedValue", "The packed value of this color", AccessControl.Public)
            {
                Value = () => newColor.color.PackedValue,
                Setter = (double var) => newColor.color.PackedValue = (uint)var
            });

            return result;
        }

        public override string ToString()
        {
            return color.ToString();
        }
    }
}
