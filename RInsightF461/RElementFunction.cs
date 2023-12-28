using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace RInsightF461
{

    public class RElementFunction : RElementProperty
    {

        public List<RParameter> lstParameters = new List<RParameter>();

        public RElementFunction(RToken clsToken, bool bBracketedNew = false, string strPackageNameNew = "", string strPackagePrefix = "", List<RElement> lstObjectsNew = null) : base(clsToken, lstObjectsNew, bBracketedNew, strPackageNameNew, strPackagePrefix)
        {
        }

    }
}