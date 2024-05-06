using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RInsightF461
{
    /// <summary>
    /// Parses script written in the R programming language and creates a dictionary of R statements. 
    /// If needed, the R script can be regenerated from the dictionary.
    /// </summary>
    public class RScript
    {

        /// <summary>   
        /// The R statements in the script. The dictionary key is the start position of the statement 
        /// in the script. The dictionary value is the statement itself. </summary>
        public OrderedDictionary statements = new OrderedDictionary();

        /// <summary>
        /// todo
        /// </summary>
        private List<RToken> _tokens;
        private List<RToken> _tokensFlat;

        /// --------------------------------------------------------------------------------------------
        /// <summary>   Parses the R script in <paramref name="strInput"/> and populates the dictionary
        ///             of R statements.
        ///             <para>
        ///             This subroutine will accept, and correctly process all valid R. However, this 
        ///             class does not attempt to validate <paramref name="strInput"/>. If it is not 
        ///             valid R then this subroutine may still process the script without throwing an 
        ///             exception. In this case, the list of R statements will be undefined.
        ///             </para><para>
        ///             In other words, this subroutine should not generate false negatives (reject 
        ///             valid R) but may generate false positives (accept invalid R).
        ///             </para></summary>
        /// 
        /// <param name="strInput"> The R script to parse. This must be valid R according to the 
        ///                         R language specification at 
        ///                         https://cran.r-project.org/doc/manuals/r-release/R-lang.html 
        ///                         (referenced 01 Feb 2021).</param>
        /// --------------------------------------------------------------------------------------------
        public RScript(string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
            {
                return;
            }

            var tokenList = new RTokenList(strInput);
            _tokens = tokenList.Tokens;
            _tokensFlat = tokenList.TokensFlat;

            foreach (RToken token in _tokens)
            {
                var clsStatement = new RStatement(token, _tokensFlat);

                // Edge case: if the last statement in the script ends with a new line, and there is
                //     no comments or other text after it, then the statement will be empty. In this
                //     case, don't add it to the list of statements.
                //todo if (clsStatement.Text.Length == 0) break;
            
                statements.Add(clsStatement.StartPos, clsStatement);
            }
        }

        /// --------------------------------------------------------------------------------------------
        /// <summary>   Returns this object as a valid, executable R script. </summary>
        /// 
        /// <param name="bIncludeFormatting">   If True, then include all formatting information in 
        ///     returned string (comments, indents, padding spaces, extra line breaks etc.). </param>
        /// 
        /// <returns>   The current state of this object as a valid, executable R script. </returns>
        /// --------------------------------------------------------------------------------------------
        public string GetAsExecutableScript(bool bIncludeFormatting = true)
        {
            string strTxt = "";
            foreach (DictionaryEntry entry in statements)
            {
                if (entry.Value is null)
                {
                    throw new Exception("The dictionary entry value cannot be null.");
                }

                RStatement rStatement = (RStatement)entry.Value;
                if (bIncludeFormatting)
                {
                    strTxt += rStatement.Text;
                }
                else if (rStatement.TextNoFormatting.Length > 0)
                {
                    strTxt += rStatement.TextNoFormatting + ";";
                }
            }

            // if no formatting needed, then remove trailing `;` from script
            //     (only needed to separate previous statements).
            if (!bIncludeFormatting)
            {
                strTxt = strTxt.TrimEnd(';');
            }

            return strTxt;
        }

        public void SetToken(int statementNumber, string functionName, int parameterNumber, string parameterValue, bool isQuoted = false)
        {
            RStatement statementToUpdate = statements[statementNumber] as RStatement;
            int adjustment = statementToUpdate.SetToken(functionName, parameterNumber, parameterValue, isQuoted);

            for (int i = statementNumber + 1; i < statements.Count; i++)
            {
                RStatement statement = statements[i] as RStatement;
                int startPosNew = (int)statement.StartPos + adjustment;
                if (startPosNew < 0)
                {
                    throw new Exception("Start position of statement cannot be less than 0.");
                }
                statement.StartPos = (uint)startPosNew;
            }
        }
    }
}