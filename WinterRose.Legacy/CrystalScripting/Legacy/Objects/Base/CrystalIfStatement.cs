using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{
    public class CrystalIfStatement
    {
        private CrystalCodeBody body;
        private CrystalScope scope;
        private CrystalConditionExpression condition;

        public CrystalConditionExpression Condition => condition;

        public CrystalIfStatement(CrystalConditionExpression condition, CrystalCodeBody body)
        {
            this.condition = condition;
        }
    }
}


