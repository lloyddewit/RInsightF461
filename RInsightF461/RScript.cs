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
        /// This function is only used for regression testing.
        /// It checks all the script positions in the script's token tree. It ensures that the 
        /// dictionary keys are consistent with each statement's start position. It also ensures 
        /// that each statement's start position is consistent with the previous statement's end 
        /// position, and that within each statement, each token's script position is consistent 
        /// with the position and length of the previous token.
        /// </summary>
        /// <returns> True if all positions are consistent, else false.</returns>
        /// ----------------------------------------------------------------------------------------
        public bool AreScriptPositionsConsistent()
        {
            uint EndPosPrev = 0;
            foreach (DictionaryEntry entry in statements)
            {
                if (entry.Value is null)
                {
                    throw new Exception("The dictionary entry value cannot be null.");
                }

                uint key = (uint)entry.Key;
                RStatement rStatement = (RStatement)entry.Value;
                if (key != rStatement.StartPos 
                    || rStatement.StartPos != EndPosPrev 
                    || !rStatement.AreScriptPositionsConsistent())
                {
                    return false;
                }
                EndPosPrev = rStatement.EndPos;
            }
            return true;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the parameter <paramref name="parameterValue"/> to the function named 
        /// <paramref name="functionName"/> in statement <paramref name="statementNumber"/>. 
        /// If <paramref name="parameterName"/> is specified then 
        /// adds or replaces the named parameter, else adds an unamed parameter. If <paramref name="isQuoted"/> 
        /// is true then encloses the parameter value in double quotes.
        /// If <paramref name="functionName"/> is not found, then throws an exception.
        /// </summary>
        /// <param name="statementNumber"> The number of the statement in the script to search for 
        ///     the function</param>
        /// <param name="functionName">    The name of the function to add the parameter to</param>
        /// <param name="parameterName">   The name of the function parameter to add. If empty, 
        ///     then adds an unamed parameter to the function.</param>
        /// <param name="parameterValue">  The new value of the added parameter</param>
        /// <param name="parameterNumber"> The number of the existing parameter to add the new 
        ///     parameter in front of. If zero, then adds the parameter before any existing 
        ///     parameters. If greater than the number of existing parameters, then adds the 
        ///     parameter after the last existing parameter.</param>
        /// <param name="isQuoted">        If true, then encloses the parameter value in double 
        ///     quotes</param>
        /// ----------------------------------------------------------------------------------------
        public void FunctionAddParam(uint statementNumber, string functionName,
                                     string parameterName, string parameterValue,
                                     uint parameterNumber = uint.MaxValue,
                                     bool isQuoted = false)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                FunctionRemoveParamByName(statementNumber, functionName, parameterName);
            }            

            RStatement statementToUpdate = statements[(int)statementNumber] as RStatement;
            int adjustment = statementToUpdate.FunctionAddParam(
                    functionName, parameterName, parameterValue, parameterNumber, isQuoted);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Removes parameter <paramref name="parameterName"/> from the function 
        /// <paramref name="functionName"/> in statement <paramref name="statementNumber"/>.
        /// </summary>
        /// <param name="statementNumber"> The number of the statement in the script to search for 
        ///                                the function</param>
        /// <param name="functionName">    The function to search for the parameter</param>
        /// <param name="parameterName">   The paramater to remove</param>
        /// ----------------------------------------------------------------------------------------
        public void FunctionRemoveParamByName(uint statementNumber,
                                              string functionName,
                                              string parameterName)
        {
            RStatement statementToUpdate = statements[(int)statementNumber] as RStatement;
            int adjustment = statementToUpdate.FunctionRemoveParamByName(functionName, parameterName);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of the specified token to <paramref name="parameterValue"/>. The token to 
        /// update is specified by <paramref name="statementNumber"/>, 
        /// <paramref name="functionName"/>, <paramref name="occurence"/> and <paramref name="parameterNumber"/>. 
        /// </summary>
        /// <param name="statementNumber"> The statement to update (0 indicates the first statement)</param>
        /// <param name="functionName">    The name of the function or operator (e.g. `+`, `-` etc.)</param>
        /// <param name="parameterNumber"> The number of the parameter to update. For a function, 
        ///     the first parameter is 0. For a binary operator the left hand parameter is 0 and the 
        ///     right hand operator is 1. For a unary operator, the parameter number must be 0.</param>
        /// <param name="parameterValue">  The token's new value</param>
        /// <param name="isQuoted">        If True then put double quotes around 
        ///     <paramref name="parameterValue"/></param>
        /// <param name="occurence">       Only needed if the statement contains more than one call 
        ///     to <paramref name="functionName"/>. Specifies which occurence of the function to 
        ///     update (zero is the first occurence of the function in the statement).</param>
        /// ----------------------------------------------------------------------------------------
        public void FunctionUpdateParamValue(uint statementNumber, string functionName,
                                             uint parameterNumber, string parameterValue,
                                             bool isQuoted = false, uint occurence = 0)
        {
            RStatement statementToUpdate = statements[(int)statementNumber] as RStatement;
            int adjustment = statementToUpdate.FunctionUpdateParamValue(
                    functionName, parameterNumber, parameterValue, isQuoted, occurence);
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
        /// Searches statement <paramref name="statementNumber"/> for <paramref name="operatorName"/> 
        /// and then inserts <paramref name="parameterScript"/> just before parameter 
        /// <paramref name="parameterNumber"/>. 
        /// </summary>
        /// <param name="statementNumber"> The statement to update (0 indicates the first statement)</param>
        /// <param name="operatorName">    The operator to search for (e.g. '+')</param>
        /// <param name="parameterNumber"> The parameter number to insert the new parameter in 
        ///     front of. If zero inserts in front of the first parameter (e.g. in front of `a` in 
        ///     `a+b`). If greater than or equal to the number of parameters, then appends the new 
        ///     parameter.</param>
        /// <param name="parameterScript"> The new parameter</param>
        /// ----------------------------------------------------------------------------------------
        public void OperatorAddParam(uint statementNumber,
                                     string operatorName,
                                     uint parameterNumber,
                                     string parameterScript)
        {
            RStatement statementToUpdate = statements[(int)statementNumber] as RStatement;
            int adjustment = statementToUpdate.OperatorAddParam(operatorName,
                                                                parameterNumber,
                                                                parameterScript);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Searches statement <paramref name="statementNumber"/> for the first occurence of 
        /// <paramref name="operatorName"/> and then replaces the operator's parameter 
        /// <paramref name="parameterNumber"/> with <paramref name="parameterScript"/>. 
        /// </summary>
        /// <param name="statementNumber"> The statement to update (0 indicates the first statement)</param>
        /// <param name="operatorName">    The operator to search for (e.g. '+')</param>
        /// <param name="parameterNumber"> Zero for the left hand parameter (e.g. `a` in `a+b`), 
        ///                                1 for the right hand parameter (e.g. `b` in `a+b`)</param>
        /// <param name="parameterScript"> The new parameter</param>
        /// ----------------------------------------------------------------------------------------
        public void OperatorUpdateParam(uint statementNumber,
                                        string operatorName,
                                        uint parameterNumber,
                                        string parameterScript)
        {
            RStatement statementToUpdate = statements[(int)statementNumber] as RStatement;
            int adjustment = statementToUpdate.OperatorUpdateParam(operatorName,
                                                                   parameterNumber,
                                                                   parameterScript);
            AdjustStatementsStartPos(statementNumber + 1, adjustment);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// For each statement in the script, after and including <paramref name="startStatement"/>, 
        /// adjusts the statement's start position by <paramref name="adjustment"/>. This function 
        /// is used to update the start positions of statements after an earlier statement has been 
        /// updated.
        /// </summary>
        /// <param name="startStatement"> The number of the statement in the script to start 
        /// adjusting the start positions. This is typically the statement immediately following 
        /// the updated statement</param>
        /// <param name="adjustment">     The amount to adjust the statements' start positions by</param>
        /// ----------------------------------------------------------------------------------------
        private void AdjustStatementsStartPos(uint startStatement, int adjustment)
        {
            // update the the start positions of each statement that comes after the updated
            // statement
            for (int i = (int)startStatement; i < statements.Count; i++)
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