using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterThornScripting
{
    /// <summary>
    /// A wrapper class for a <see cref="Function"/> to make development on this language easier for me, the developer.
    /// </summary>
    [SerializeAs<Constructor>]
    public class Constructor : Function
    {
        [DefaultArguments("", "", AccessControl.Public)]
        public Constructor(string className, string description, AccessControl accessmodifiers) : base(className, description, accessmodifiers)
        {
        }

        /// <summary>
        /// Creates an exact copy of the constructor with the new block as the parent making it work object orjented instead of staticly
        /// </summary>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public new Constructor CreateCopy(Block newParent)
        {
            return new Constructor(Name, Description, AccessModifiers)
            {
                Body = Body.CreateCopy(newParent),
                Parameters = Parameters,
                DelcaredClass = DelcaredClass,
            };
        }
    }
}
