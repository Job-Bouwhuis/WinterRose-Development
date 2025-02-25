

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{

    [IncludePrivateFields]
    public sealed class CrystalCodeBody
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ulong id;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Token> bodyTokens;
        [DebuggerBrowsable(DebuggerBrowsableState.Never), ExcludeFromSerialization]
        private CrystalCodeBody? parent;
        internal ulong parentID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CrystalScope publicIdeintifiers;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CrystalScope privateIdentifiers;
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private CrystalIfStatement? conditionToExecute;

        public ulong Id => id;
        public List<Token> BodyTokens => bodyTokens;
        public CrystalCodeBody Parent => parent;
        public CrystalScope PublicIdeintifiers => publicIdeintifiers;
        public CrystalScope PrivateIdentifiers => privateIdentifiers;

        public CrystalCodeBody(List<Token> tokens, CrystalCodeBody? parent = null)
        {
            id = GenerateBlockID();
            bodyTokens = tokens;
            this.parent = parent;
            CrystalScope? scopepub = parent?.PublicIdeintifiers;
            CrystalScope? scopepriv = parent?.PrivateIdentifiers;

            publicIdeintifiers = new(scopepub);
            privateIdentifiers = new(scopepub);

            if (parent is not null)
            {
                parentID = parent.id;
            }
        }
        /// this exists for serialization
        private CrystalCodeBody() { }

        internal static ulong GenerateBlockID()
        {
            byte[] buffer = new byte[8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return BitConverter.ToUInt64(buffer, 0);
        }

        internal void SetParent(CrystalCodeBody? parent)
        {
            this.parent = parent;
            publicIdeintifiers.SetParent(parent.PublicIdeintifiers);
            privateIdentifiers.SetParent(parent.PrivateIdentifiers);
        }

        public CrystalCodeBody Copy()
        {
            // write code that copies the code body

            return new CrystalCodeBody(bodyTokens, parent)
            {
                id = Id,
                publicIdeintifiers = publicIdeintifiers.Copy(),
                privateIdentifiers = privateIdentifiers.Copy()
            };
        }
    }
}

