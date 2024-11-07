using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Monogame.WinterThornPort
{
    internal class InputPort : CSharpClass
    {
        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            Class result = new("Input", "Handles getting of input");
            result.CSharpClass = this;
            result.DeclareFunction(new Function("GetKeyDown", "Gets if the key is pressed the current frame", AccessControl.Public)
            {
                CSharpFunction = (Variable var) =>
                {
                    // get if the varible is a string
                    if (var.Type == VariableType.String)
                    {
                        // get the string value
                        string value = var.Value.ToString();
                        // get the key
                        Keys key = (Keys)Enum.Parse(typeof(Keys), value);
                        // return if the key is down
                        return new Variable("GetKeyDownResult", "", Input.GetKeyDown(key), AccessControl.Private);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "GetKeyDown only takes a string");
                }
            });

            result.DeclareFunction(new Function("GetKey", "Gets if the key is pressed", AccessControl.Public)
            {
                CSharpFunction = (Variable var) =>
                {
                    // get if the varible is a string
                    if (var.Type == VariableType.String)
                    {
                        // get the string value
                        string value = var.Value.ToString();
                        // get the key
                        Keys key = (Keys)Enum.Parse(typeof(Keys), value);
                        // return if the key is down
                        return new Variable("GetKeyResult", "", Input.GetKey(key), AccessControl.Private);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "GetKeyDown only takes a string");
                }
            });

            result.DeclareFunction(new Function("GetKeyUp", "Gets if the key is released", AccessControl.Public)
            {
                CSharpFunction = (Variable var) =>
                {
                    // get if the varible is a string
                    if (var.Type == VariableType.String)
                    {
                        // get the string value
                        string value = var.Value.ToString();
                        // get the key
                        Keys key = (Keys)Enum.Parse(typeof(Keys), value);
                        // return if the key is down
                        return new Variable("GetKeyUpResult", "", Input.GetKeyUp(key), AccessControl.Private);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "GetKeyDown only takes a string");
                }
            });

            result.DeclareFunction(new Function("GetMouseButton", "Gets if the mouse button is pressed", AccessControl.Public)
            {
                CSharpFunction = (Variable var) =>
                {
                    // get if the varible is a string
                    if (var.Type == VariableType.String)
                    {
                        // get the string value
                        string value = var.Value.ToString();
                        // get the key
                        MouseButton button = (MouseButton)Enum.Parse(typeof(MouseButton), value);
                        // return if the key is down
                        return new Variable("GetMouseButtonResult", "", Input.GetMouse(button), AccessControl.Private);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "GetKeyDown only takes a string");
                }
            });

            result.DeclareVariable(new Variable("MousePosition", "", AccessControl.Private)
            {
                Value = () =>
                {
                    Vector2I pos = Input.MousePosition;

                    return 0;
                }
            });

            return result;
        }
    }
}
