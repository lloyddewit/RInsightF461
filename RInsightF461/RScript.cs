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

        /// ----------------------------------------------------------------------------------------
        /// <summary>   Parses the R script in <paramref name="strInput"/> and populates the 
        ///             dictionary of R statements.
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
        /// ----------------------------------------------------------------------------------------
        public RScript(string strInput)
        {
            List<RToken> tokens = new RTokenList(strInput).Tokens;
            foreach (RToken token in tokens)
            {
                var statement = new RStatement(token);
                statements.Add(statement.StartPos, statement);
            }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="statementNumber"></param>
        /// <param name="functionName"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="isQuoted"></param>
        /// ----------------------------------------------------------------------------------------
        public void AddParameterByName(int statementNumber, string functionName, string parameterName,
                                       string parameterValue, bool isQuoted = false)
        {
            RemoveParameterByName(statementNumber, functionName, parameterName);

            RStatement statementToUpdate = statements[statementNumber] as RStatement;
            int adjustment = statementToUpdate.AddParameterByName(functionName, parameterName,
                                                                  parameterValue, isQuoted);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo move to alphabetic location
        /// </summary>
        /// <param name="statementNumber"></param>
        /// <param name="functionName"></param>
        /// <param name="parameterName"></param>
        /// ----------------------------------------------------------------------------------------
        public void RemoveParameterByName(int statementNumber, string functionName, string parameterName)
        {
            RStatement statementToUpdate = statements[statementNumber] as RStatement;
            int adjustment = statementToUpdate.RemoveParameterByName(functionName, parameterName);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>   Returns this object as a valid, executable R script. </summary>
        /// 
        /// <param name="bIncludeFormatting">   If True, then include all formatting information in 
        ///     returned string (comments, indents, padding spaces, extra line breaks etc.). </param>
        /// 
        /// <returns>   The current state of this object as a valid, executable R script. </returns>
        /// ----------------------------------------------------------------------------------------
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

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of the specified token to <paramref name="parameterValue"/>. The token to 
        /// update is specified by <paramref name="statementNumber"/>, 
        /// <paramref name="functionName"/>, and <paramref name="parameterNumber"/>.
        /// </summary>
        /// <param name="statementNumber"> The statement to update (0 indicates the first statement)</param>
        /// <param name="functionName">    The name of the function or operator (e.g. `+`, `-` etc.)</param>
        /// <param name="parameterNumber"> The number of the parameter to update. For a function, 
        ///     the first parameter is 0. For a binary operator the left hand parameter is 0 and the 
        ///     right hand operator is 1. For a unary operator, the parameter number must be 0.</param>
        /// <param name="parameterValue">  The token's new value</param>
        /// <param name="isQuoted">        If True then put double quotes around 
        ///     <paramref name="parameterValue"/></param>
        /// ----------------------------------------------------------------------------------------
        public void SetToken(int statementNumber, string functionName, int parameterNumber, 
                             string parameterValue, bool isQuoted = false)
        {
            RStatement statementToUpdate = statements[statementNumber] as RStatement;
            int adjustment = statementToUpdate.SetToken(functionName, parameterNumber,
                                                        parameterValue, isQuoted);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="statementNumber"></param>
        /// <param name="adjustment"></param>
        private void AdjustStatementsStartPos(int statementNumber, int adjustment)
        {
            // update the the start positions of each statement that comes after the updated
            // statement
            for (int i = statementNumber; i < statements.Count; i++)
            {
                RStatement statement = statements[i] as RStatement;
                statement.AdjustStartPos(adjustment);
            }

            // ensure that the dictionary keys are consistent with the new start positions
            OrderedDictionary statementsNew = new OrderedDictionary();
            foreach (DictionaryEntry entry in statements)
            {
                RStatement statement = entry.Value as RStatement;
                statementsNew.Add(statement.StartPos, statement);
            }
            statements = statementsNew;
        }
    }
}