using Microsoft.Xna.Framework;
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
    internal class ThornTransform(Transform trans) : CSharpClass
    {
        Transform transform = trans;

        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            var newTrans = new ThornTransform(transform);
            Class result = new("Transform", "A transform component");
            result.CSharpClass = newTrans;
            result.DeclareVariable(new Variable("position", "The position of the transform", AccessControl.Public)
            {
                Value = () =>
                {
                    Vector2 pos = transform.position;

                    ThornVector newVec = new()
                    {
                        x = pos.X,
                        y = pos.Y
                    };

                    return newVec.GetClass();
                },
                Setter = (Class var) =>
                {
                    if (var.Name is "Vector2")
                    {
                        double x = (double)var.Block["x"].Value, y = (double)var.Block["y"].Value;
                        transform.position = new Vector2((float)x, (float)y);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Transform.position can only be set to a Vector2");
                }
            });
            result.DeclareVariable(new Variable("scale", "The scale of the transform", AccessControl.Public)
            {
                Value = () =>
                {
                    Vector2 scale = transform.scale;

                    ThornVector newVec = new()
                    {
                        x = scale.X,
                        y = scale.Y
                    };

                    return newVec.GetClass();
                },
                Setter = (Class var) =>
                {
                    if (var.Name is "Vector2")
                    {
                        double x = (double)var.Block["x"].Value, y = (double)var.Block["y"].Value;
                        transform.scale = new Vector2((float)x, (float)y);
                    }
                    else throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "Transform.position can only be set to a Vector2");
                }
            });
            result.DeclareVariable(new Variable("rotation", "The rotation of the transform", AccessControl.Public)
            {
                Value = () => (double)transform.rotation,
                Setter = (double d) => transform.rotation = (float)d
            });
            result.DeclareVariable(new Variable("children", "the children on this transform", AccessControl.Public)
            {
                Value = () =>
                {
                    List<Class> transforms = [];
                    foreach (var child in transform)
                        transforms.Add(new ThornTransform(child).GetClass());

                    return new ReadonlyCollection(transforms.Select((x, i) => new Variable($"TransformChildrenCollection{i}", $"The item at index {1}", x, AccessControl.Private)).ToArray()).GetClass();
                },
                Setter = (Class var) =>
                {
                    Debug.LogWarning("Transform.children in WinterThorn should not be set to a new value.");
                }
            });


            return result;
        }
    }
}
