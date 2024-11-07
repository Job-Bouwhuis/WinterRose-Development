using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.DefaultLibrary;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Monogame.WinterThornPort
{
    internal class ThornMonoUtils : CSharpClass
    {
        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            Class result = new("MonoUtils", "A WinterThorn port of the MonoUtils class in C#");
            result.CSharpClass = this;

            result.DeclareVariable(new Variable("isActive", "Whether the game has OS focus", AccessControl.Public)
            {
                Value = () => MonoUtils.IsActive,
                Setter = (object o) => { }
            });

            var args = MonoUtils.ProgramArguments.Select((x, i) => new Variable($"arg{i}", $"The item at index {i}", AccessControl.Private)).ToArray();
            result.DeclareVariable(new Variable("programArguments", "The command-line arguments passed with this instance of the program", AccessControl.Public)
            {
                Value = () => new ReadonlyCollection(args),
                Setter = (object o) => { }
            });
            result.DeclareVariable(new Variable("windowResulution", "The actual size of the game window", AccessControl.Public)
            {
                Value = () =>
                {
                    var vec = new ThornVector();
                    vec.x = MonoUtils.WindowResolution.X;
                    vec.y = MonoUtils.WindowResolution.Y;

                    return vec.GetClass();
                },
                Setter = (Class vec) =>
                {
                    if (vec.Name != "Vector2")
                        throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "windowResolution can only be set to a Vector2");

                    double x = (double)vec.Block["x"].Value, y = (double)vec.Block["y"].Value;

                    MonoUtils.WindowResolution = new Vector2I((int)x, (int)y);
                }
            });
            result.DeclareVariable(new Variable("screenCenter", "The actual size of the game window", AccessControl.Public)
            {
                Value = () =>
                {
                    var vec = new ThornVector();
                    vec.x = MonoUtils.ScreenCenter.X;
                    vec.y = MonoUtils.ScreenCenter.Y;

                    return vec.GetClass();
                },
                Setter = (object o) =>
                {
                }
            });
            result.DeclareVariable(new Variable("targetFramerate", "The target framerate the game will attemt to run at", AccessControl.Public)
            {
                Value = () => MonoUtils.TargetFramerate,
                Setter = (double d) => MonoUtils.TargetFramerate = (int)d
            });
            result.DeclareVariable(new Variable("isCursorVisible", "whether the cursor should be shown when it is within the game window", AccessControl.Public)
            {
                Value = () => MonoUtils.IsCursorVisible,
                Setter = (bool b) => MonoUtils.IsCursorVisible = b
            });

            return result;
        }
    }
}
