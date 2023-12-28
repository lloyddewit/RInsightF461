using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace RInsightF461
{
    public class RElementOperator : RElementAssignable
    {
        public bool bFirstParamOnRight = false;
        public string strTerminator = ""; // only used for '[' and '[[' operators
        public List<RParameter> lstParameters = new List<RParameter>();

        public RElementOperator(RToken clsToken, bool bBracketedNew = false, bool bFirstParamOnRightNew = false) : base(clsToken, null, bBracketedNew)
        {
            bFirstParamOnRight = bFirstParamOnRightNew;
        }
    }
}