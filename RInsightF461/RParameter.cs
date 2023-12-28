using Microsoft.VisualBasic;

namespace RInsightF461
{
    public class RParameter
    {
        public string strArgName; // TODO spaces around '=' as option?
        public RElement clsArgValue;
        public RElement clsArgValueDefault;
        public int iArgPos;
        public int iArgPosDefinition;
        public string strPrefix = "";
    }
}