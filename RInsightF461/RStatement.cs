using System;
using System.Collections.Generic;
using System.Linq;

namespace RInsightF461
{
    /// <summary>
    /// Represents a single valid R statement.
    /// todo: a statement is essentially a single token. Remove this class and move the 
    ///       functionality to the RToken class?
    /// </summary>
    public class RStatement
    {
        /// <summary> True if this statement is an assignment statement </summary>
        public bool IsAssignment { get => GetIsAssignment(); }

        /// <summary> The position in the script where this statement starts </summary>
        public uint StartPos { get { return _token.ScriptPosStartStatement; } }

        /// <summary> The position in the script where this statement ends </summary>
        public uint EndPos { get { return _token.ScriptPosEndStatement; } }

        /// <summary>
        /// The text representation of this statement, including all formatting information (comments,
        /// spaces, extra newlines etc.).
        /// </summary>
        public string Text => GetText(_token);

        /// <summary>
        /// The text representation of this statement, excluding all formatting information (comments,
        /// spaces, extra newlines etc.).
        /// </summary>
        public string TextNoFormatting => GetTextNoFormatting();

        /// <summary>
        /// The statement is represented by a recursive tree of tokens. This is the root of the tree.
        /// </summary>
        private RToken _token;
        
        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Constructs an object representing a valid R statement from the <paramref name="token"/> 
        /// token tree. </summary>
        /// <param name="token">  The tree of R tokens to process </param>
        /// ----------------------------------------------------------------------------------------
        internal RStatement(RToken token)
        {            
            _token = token;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// For every token in the <paramref name="token"/> token tree, if the token's start 
        /// position in the script is greater than or equal to <paramref name="scriptPosMin"/>, 
        /// then adjust the start position by <paramref name="adjustment"/>.
        /// </summary>
        /// <param name="adjustment">   If positive, then increase the each token's start position 
        ///     by this amount; if negative, then reduce each token's start position by this amount.
        ///     </param>
        /// <param name="scriptPosMin"> If the token's start position is less than this, then do 
        ///     nothing</param>
        /// <param name="token">        The root of the token tree to traverse. If not specified or 
        ///     null, then traverse the statement's root token</param>
        /// ----------------------------------------------------------------------------------------

        internal void AdjustStartPos(int adjustment, uint scriptPosMin = 0, RToken token = null)
        {
            token = token ?? _token;
            List<RToken> tokensFlat = GetTokensFlat(token);
            foreach (RToken tokenFlat in tokensFlat)
            {
                if (tokenFlat.ScriptPos >= scriptPosMin)
                    tokenFlat.ScriptPos = (uint)((int)tokenFlat.ScriptPos + adjustment);
            }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// This function is only used for regression testing.
        /// For each token in the statement, checks that the token's script position is consistent 
        /// with the position and length of the previous token.
        /// </summary>
        /// <returns> True if all positions are consistent, else false.</returns>
        /// ----------------------------------------------------------------------------------------
        internal bool AreScriptPositionsConsistent()
        {
            List<RToken> tokensFlat = GetTokensFlat(_token);
            uint scriptPosExpected = tokensFlat[0].ScriptPos;
            foreach (RToken tokenFlat in tokensFlat)
            {
                if (tokenFlat.ScriptPos != scriptPosExpected)
                    return false;

                scriptPosExpected += (uint)tokenFlat.Lexeme.Text.Length;
            }
            return true;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the parameter named <paramref name="parameterName"/> to the function named 
        /// <paramref name="functionName"/>. The value of the parameter is set to 
        /// <paramref name="parameterValue"/>. If <paramref name="isQuoted"/> is true then encloses 
        /// the parameter value in double quotes. Returns the difference between the function's text  
        /// length before/after adding the parameter. 
        /// If the function is not found, then throws an exception.
        /// todo update comment for adding param with no name; rename function to FunctionAddParam?
        /// </summary>
        /// <param name="functionName">    The name of the function to add the parameter to</param>
        /// <param name="parameterName">   The name of the function parameter to add</param>
        /// <param name="parameterValue">  The new value of the added parameter</param>
        /// <param name="parameterNumber"> The number of the existing parameter to add the new 
        ///     parameter in front of. If zero, then adds the parameter before any existing 
        ///     parameters. If greater than the number of existing parameters, then adds the 
        ///     parameter after the last existing parameter.</param>
        /// <param name="isQuoted">        If true, then encloses the parameter value in double 
        ///     quotes</param>
        /// <returns> The difference between the function's text length before/after adding the 
        ///     parameter. For example if parameter `c` with value 3 is added to `fn(a=1, b=2)`, 
        ///     then the resulting function is `fn(a=1, b=2, c=3)` and 5 is returned 
        ///     (the length of `, c=3`). </returns>
        /// ----------------------------------------------------------------------------------------
        internal int FunctionAddParamByName(string functionName, string parameterName,
                                            string parameterValue, uint parameterNumber,
                                            bool isQuoted)
        {
            RToken tokenFunction = GetTokenFunction(_token, functionName);
            if (tokenFunction is null)
                throw new Exception("Function not found.");

            RToken tokenBracketOpen = GetFirstNonPresentationChild(tokenFunction);
            parameterValue = isQuoted ? "\"" + parameterValue + "\"" : parameterValue;
            parameterNumber = Math.Min(parameterNumber,
                                       (uint)tokenBracketOpen.ChildTokens.Count - 1);

            // find position in the statement to insert new function param
            RToken tokenParameter = tokenBracketOpen.ChildTokens[(int)parameterNumber];
            int insertPos = (int)tokenParameter.ScriptPosStartStatement
                            - (int)_token.ScriptPosStartStatement;

            // create new statement script that includes new function parameter
            string paramNameAndValue = string.IsNullOrEmpty(parameterName)
                                       ? parameterValue
                                       : $"{parameterName}={parameterValue}";
            if (parameterNumber == 0)
                paramNameAndValue = tokenBracketOpen.ChildTokens.Count < 2
                    ? paramNameAndValue
                    : $"{paramNameAndValue}, ";
            else
                paramNameAndValue = $", {paramNameAndValue}";

            int adjustment = paramNameAndValue.Length;
            string statementScriptNew = Text.Insert(insertPos, paramNameAndValue);

            // make token tree for new statement
            RToken tokenStatementNew = GetTokenStatement(statementScriptNew);
            AdjustStartPos(adjustment: (int)_token.ScriptPosStartStatement,
                           scriptPosMin: 0,
                           token: tokenStatementNew);
            _token = tokenStatementNew;

            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Removes the parameter named <paramref name="parameterName"/> from the function named 
        /// <paramref name="functionName"/>. Returns the difference between the function's text  
        /// length before/after removing the parameter. If the function or parameter is not found, 
        /// then does nothing and returns 0.
        /// </summary>
        /// <param name="functionName">  The name of the function to remove the parameter from</param>
        /// <param name="parameterName"> The name of the function parameter to remove</param>
        /// <returns> The difference between the function's text length before/after removing the 
        ///     parameter. This is always negative or zero. For example if parameter `b` is removed 
        ///     from `fn(a=1, b=2)`, then the resulting function is `fn(a=b)` and -5 is returned 
        ///     (the length of `, b=2`). </returns>
        /// ----------------------------------------------------------------------------------------
        internal int FunctionRemoveParamByName(string functionName, string parameterName)
        {
            int adjustment = 0;
            RToken tokenFunction = GetTokenFunction(_token, functionName);
            RToken tokenBracketOpen = GetFirstNonPresentationChild(tokenFunction);

            bool isFirstParameter = true;
            foreach (RToken token in tokenBracketOpen.ChildTokens)
            {
                if (token.IsPresentation) continue;

                RToken tokenParameterAssignment = token;
                if (token.TokenType == RToken.TokenTypes.RSeparator 
                    && token.Lexeme.Text == ",")
                {
                    tokenParameterAssignment = GetFirstNonPresentationChild(token);
                }

                if (tokenParameterAssignment.TokenType != RToken.TokenTypes.ROperatorBinary 
                    || tokenParameterAssignment.Lexeme.Text != "=")
                {
                    isFirstParameter = false;
                    continue;
                }

                RToken tokenParameterName = GetFirstNonPresentationChild(tokenParameterAssignment);
                if (tokenParameterName.TokenType != RToken.TokenTypes.RSyntacticName 
                    || tokenParameterName.Lexeme.Text != parameterName)
                {
                    isFirstParameter = false;
                    continue;
                }

                // if we are removing the first parameter then we also need to remove the comma
                // before the second parameter. E.g. `fn(a=1, b=2)` becomes `fn(b=2)`.
                int numParams = tokenBracketOpen.ChildTokens.Count - 1;
                numParams -= tokenBracketOpen.ChildTokens[0].IsPresentation ? 1 : 0;
                if (isFirstParameter && numParams > 1)
                {
                    RToken tokenParam2 = tokenBracketOpen.ChildTokens[
                            tokenBracketOpen.ChildTokens[0].IsPresentation ? 2 : 1];
                    RToken tokenParam2NoComma = tokenParam2.ChildTokens[
                            tokenParam2.ChildTokens[0].IsPresentation ? 1 : 0];

                    // remove any presentation tokens from the parameter
                    // todo This assumes that the 2nd parameter is a named parameter.
                    //      Handle case when 2nd parameter is not named (e.g. `fn(a=1, 2)`)
                    RToken tokenParam2Name = GetFirstNonPresentationChild(tokenParam2NoComma);
                    if (tokenParam2Name.ChildTokens.Count > 0 
                        && tokenParam2Name.ChildTokens[0].IsPresentation)
                    {
                        // adjust because we removed the space(s)
                        adjustment -= GetText(tokenParam2Name.ChildTokens[0]).Length;
                        tokenParam2Name.ChildTokens.Remove(tokenParam2Name.ChildTokens[0]);
                    }
                    tokenBracketOpen.ChildTokens[
                            tokenBracketOpen.ChildTokens[0].IsPresentation ? 2 : 1] 
                            = tokenParam2NoComma;
                    adjustment--; // adjust because we removed the comma
                }

                tokenBracketOpen.ChildTokens.Remove(token);
                adjustment -= GetText(token).Length;
                AdjustStartPos(adjustment: adjustment,
                               scriptPosMin: token.ScriptPosStartStatement,
                               token: _token);
                isFirstParameter = false;
                break;
            }

            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of the specified token to <paramref name="parameterValue"/>. The token to 
        /// update is specified by <paramref name="functionName"/>, <paramref name="occurence"/> and 
        /// <paramref name="parameterNumber"/>. 
        /// If <paramref name="functionName"/> is not found, then does nothing and returns zero. 
        /// Else returns the difference in length between the token's old value, and the token's 
        /// new value.
        /// </summary>
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
        /// <returns>                      The difference in length between the token's old value, 
        ///     and the token's new value. A negative number indicates that the new value is shorter 
        ///     than the new value.</returns>
        /// ----------------------------------------------------------------------------------------
        internal int FunctionUpdateParamValue(string functionName, uint parameterNumber,
                                              string parameterValue, bool isQuoted,
                                              uint occurence)
        {
            RToken tokenFunction = GetTokenFunction(_token, functionName, occurence);
            if (tokenFunction is null)
                return 0;

            RToken tokenParameterValue = GetTokenParameterFunction(tokenFunction, parameterNumber);

            parameterValue = isQuoted ? "\"" + parameterValue + "\"" : parameterValue;
            int adjustment = parameterValue.Length - tokenParameterValue.Lexeme.Text.Length;
            tokenParameterValue.Lexeme.Text = parameterValue;

            // update the script position of any subsequent tokens in statement
            AdjustStartPos(adjustment, tokenParameterValue.ScriptPos + 1);

            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Searches the statement for operator <paramref name="operatorName"/> and then inserts 
        /// <paramref name="parameterScript"/> just before (zero-based) occurence 
        /// <paramref name="parameterNumber"/>. Returns the length of the script added to the 
        /// statement. 
        /// If the operator is not found, then throws an exception. <para>
        /// Special case: If the new parameter is appended to the end of the statement, and 
        /// <paramref name="parameterScript"/> has presentation information appended to it 
        /// (e.g. spaces, newlines, comments etc.), then this presentation information is discarded. 
        /// This is because in the script's token tree, this presentation information should be 
        /// part of the next statement.</para>
        /// todo currently only binary operators supported
        /// </summary>
        /// <param name="operatorName">    The operator to search for (e.g. '+')</param>
        /// <param name="parameterNumber"> The parameter number to insert the new parameter in 
        ///     front of. If zero inserts in front of the first parameter (e.g. in front of `a` in 
        ///     `a+b`). If greater than or equal to the number of parameters, then appends the new 
        ///     parameter.</param>
        /// <param name="parameterScript"> The length of the new parameter</param>
        /// <returns>                      The length of the script added to the statement</returns>
        /// ----------------------------------------------------------------------------------------
        internal int OperatorAddParam(string operatorName, uint parameterNumber,
                                      string parameterScript)
        {
            // find all occurences of the operator in the statement
            List<RToken> operators = GetTokensOperators(_token, operatorName);
            if (operators.Count == 0)
                throw new Exception("Operator not found.");

            // find position in the statement to insert new operator param
            int insertPos;
            if (parameterNumber == 0)
            {
                insertPos = (int)operators[0].ScriptPosStartStatement;
            }
            else if (parameterNumber > operators.Count)
            {
                insertPos = (int)operators[(int)operators.Count - 1].ScriptPosEndStatement;
            }
            else
            {
                RToken tokenOperatorInsert = operators[(int)parameterNumber - 1];
                if (tokenOperatorInsert.ChildTokens.Count < 1
                    || tokenOperatorInsert.ChildTokens[0].TokenType != RToken.TokenTypes.RPresentation)
                    insertPos = (int)tokenOperatorInsert.ScriptPos;
                else
                    insertPos = (int)tokenOperatorInsert.ChildTokens[0].ScriptPos;
            }
            insertPos -= (int)_token.ScriptPosStartStatement;

            // create new statement script that includes new operator parameter
            string operatorAndParam = parameterNumber == 0 ? $"{parameterScript} {operatorName} " :
                                                            $" {operatorName} {parameterScript}";
            int adjustment = operatorAndParam.Length;
            string statementScriptNew = Text.Insert(insertPos, operatorAndParam);

            // make token tree for new statement
            RTokenList tokenList = new RTokenList(statementScriptNew);
            if (tokenList.Tokens.Count != 1)
            {
                if (tokenList.Tokens.Count == 2
                        && tokenList.Tokens[1].TokenType == RToken.TokenTypes.REmpty
                        && tokenList.Tokens[1].ChildTokens.Count == 1
                        && tokenList.Tokens[1].ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation)
                    adjustment -= tokenList.Tokens[1].ChildTokens[0].Lexeme.Text.Length;
                else
                    throw new Exception("Token list must have only a single entry.");
            }
            RToken tokenStatementNew = tokenList.Tokens[0];
            AdjustStartPos(adjustment: (int)_token.ScriptPosStartStatement,
                           scriptPosMin: 0,
                           token: tokenStatementNew);
            _token = tokenStatementNew;

            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Searches the statement for the first occurence of <paramref name="operatorName"/> and 
        /// then replaces  the operator's parameter <paramref name="parameterNumber"/> with 
        /// <paramref name="parameterScript"/>. Returns the difference in length between the old 
        /// parameter and the new parameter. <para>
        /// Any presentation information (spaces, line breaks, comments) associated with the 
        /// operator, or replaced operand, is preserved. </para><para>
        /// If the operator is not found, then throws an exception. 
        /// If the operator is unary and <paramref name="parameterNumber"/> is not zero, then throws
        /// an exception. </para>
        /// </summary>
        /// <param name="operatorName">    The operator to search for (e.g. '+')</param>
        /// <param name="parameterNumber"> Zero for the left hand parameter (e.g. `a` in `a+b`), 
        ///         1 for the right hand parameter (e.g. `b` in `a+b`)
        ///         2 or more for any following parameters (e.g. `c` in `a+b+c`)(</param>
        /// <param name="parameterScript"> The new parameter value</param>
        /// <returns>                      The difference in length between the old parameter and 
        ///                                the new parameter. A negative value indicates that the 
        ///                                new parameter is shorter than the old parameter.</returns>
        /// ----------------------------------------------------------------------------------------
        internal int OperatorUpdateParam(string operatorName, uint parameterNumber,
                                         string parameterScript)
        {
            List<RToken> operators = GetTokensOperators(_token, operatorName);
            if (operators.Count == 0)
                throw new Exception("Operator not found.");

            int insertPos;
            int paramToReplaceLength;
            RToken tokenOperandLeftMost;

            // if parameter number is zero then we may be updating unary or binary operator
            if (parameterNumber == 0)
            {
                // update first operand of first operator
                RToken tokenOperatorToUpdate = operators[0];
                RToken tokenOperand = GetFirstNonPresentationChild(tokenOperatorToUpdate);
                tokenOperandLeftMost = GetTokenLeftMost(tokenOperand);

                insertPos = (int)tokenOperandLeftMost.ScriptPos;

                // if unary right operator, then operand will be on the right of the operator
                if (tokenOperatorToUpdate.TokenType == RToken.TokenTypes.ROperatorUnaryRight)
                    paramToReplaceLength = GetText(tokenOperand).Length;
                else
                    paramToReplaceLength = (int)tokenOperatorToUpdate.ScriptPos - insertPos;

                // keep any presentation information that comes before the operator
                if (tokenOperatorToUpdate.TokenType != RToken.TokenTypes.ROperatorUnaryRight
                        && tokenOperatorToUpdate.ChildTokens.Count >= 1
                        && tokenOperatorToUpdate.ChildTokens[0].TokenType == 
                            RToken.TokenTypes.RPresentation)
                {
                    int presentationLength = GetText(tokenOperatorToUpdate.ChildTokens[0]).Length;
                    paramToReplaceLength -= presentationLength;
                }                  
            }
            else // else we must be updating a binary operator
            {
                if (operators[0].TokenType != RToken.TokenTypes.ROperatorBinary)
                    throw new Exception("For a unary operator, parameter number must be zero.");

                RToken tokenOperatorToUpdate;
                if (parameterNumber > operators.Count)              
                    // update right-hand operand of last operator
                    tokenOperatorToUpdate = operators[operators.Count - 1];
                else
                    // update right-hand operand of operator that precedes the parameter number
                    tokenOperatorToUpdate = operators[(int)parameterNumber-1];

                int paramToReplaceIndex = GetIndexFirstNonPresentationChild(tokenOperatorToUpdate) + 1;
                RToken tokenOperand = tokenOperatorToUpdate.ChildTokens[paramToReplaceIndex];
                insertPos = (int)tokenOperand.ScriptPosStartStatement;
                paramToReplaceLength = GetText(tokenOperand).Length;

                tokenOperandLeftMost = GetTokenLeftMost(tokenOperand);
            }

            // keep any presentation info that comes before the operand
            if (tokenOperandLeftMost.TokenType == RToken.TokenTypes.RPresentation)
            {
                int presentationLength = GetText(tokenOperandLeftMost).Length;
                insertPos += presentationLength;
                paramToReplaceLength -= presentationLength;
            }

            insertPos -= (int)_token.ScriptPosStartStatement;
            string statementScriptNew = Text.Remove(insertPos, paramToReplaceLength)
                                            .Insert(insertPos, parameterScript);            
            RToken tokenStatementNew = GetTokenStatement(statementScriptNew);
            AdjustStartPos(adjustment: (int)_token.ScriptPosStartStatement,
                           scriptPosMin: 0,
                           token: tokenStatementNew);

            int adjustment = GetText(tokenStatementNew).Length - Text.Length;
            _token = tokenStatementNew;

            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the first child of <paramref name="token"/> that is not a presentation token.
        /// </summary>
        /// <param name="token"> the token to search for a non-presentation child</param>
        /// <returns> The first child of <paramref name="token"/> that is not a presentation token</returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetFirstNonPresentationChild(RToken token)
        {

            if (token.ChildTokens is null
                    || token.ChildTokens.Count == 0
                    || (token.ChildTokens.Count == 1 && token.ChildTokens[0].IsPresentation))
                throw new System.Exception("Token has no non-presentation children.");

            return token.ChildTokens[GetIndexFirstNonPresentationChild(token)];
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the index of the first child of <paramref name="token"/> that is not a 
        /// presentation token.
        /// </summary>
        /// <param name="token"> the token to search for a non-presentation child</param>
        /// <returns> The index of the first child of <paramref name="token"/> that is not a 
        ///           presentation token</returns>
        /// ----------------------------------------------------------------------------------------
        private static int GetIndexFirstNonPresentationChild(RToken token)
        {
            return token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Return true if this statement is an assignment statement.
        /// </summary>
        /// <returns>True if this statement is an assignment statement.</returns>
        /// ----------------------------------------------------------------------------------------
        private bool GetIsAssignment()
        {
            var assignments = new HashSet<string> { "->", "->>", "<-", "<<-", "=" };
            return _token.TokenType == RToken.TokenTypes.ROperatorBinary
                                       && assignments.Contains(_token.Lexeme.Text);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the text representation of the statement (or part of the statement) represented 
        /// by <paramref name="token"/>. The returned text includes all formatting information 
        /// (comments, spaces, extra newlines etc.).
        /// </summary>
        /// <param name="token"> The token to convert to R script</param>
        /// <returns>The text representation of this statement, including all formatting information 
        /// (comments, spaces, extra newlines etc.).</returns>
        /// ----------------------------------------------------------------------------------------
        private static string GetText(RToken token)
        {
            string text = "";
            List<RToken> tokensFlat = GetTokensFlat(token);
            foreach (RToken tokenFlat in tokensFlat)
            {
                text += tokenFlat.Lexeme.Text;
            }            
            return text;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the text representation of this statement, excluding all formatting information 
        /// (comments, spaces, extra newlines etc.).
        /// </summary>
        /// <returns>The text representation of this statement, excluding all formatting information 
        /// (comments, spaces, extra newlines etc.).</returns>
        /// ----------------------------------------------------------------------------------------
        private string GetTextNoFormatting()
        {
            string text = "";
            List<RToken> tokensFlat = GetTokensFlat(_token);
            foreach (RToken token in tokensFlat)
            {
                if (token.TokenType == RToken.TokenTypes.REmpty)
                    continue;

                if (token.TokenType == RToken.TokenTypes.REndStatement)
                    text += ";";
                else if (token.TokenType == RToken.TokenTypes.RKeyWord
                         && (token.Lexeme.Text == "else"
                             || token.Lexeme.Text == "in"
                             || token.Lexeme.Text == "repeat"))
                    text += " " + token.Lexeme.Text + " ";
                else if (!token.IsPresentation) // ignore presentation tokens
                    text += token.Lexeme.Text;
            }
            // remove final trailing `;` (only needed to separate internal compound statements)
            text = text.Trim(';');
            return text;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Searches the token tree with root <paramref name="token"/> and returns the token 
        /// representing a function called <paramref name="functionName"/>.
        /// If the function token is not found, then returns null.
        /// </summary>
        /// <param name="token">        The root of the function tree</param>
        /// <param name="functionName"> The name of the function to search for</param>
        /// <param name="occurence">    Only needed if the statement contains more than one call 
        ///     to <paramref name="functionName"/>. Specifies which occurence of the function to 
        ///     update (zero is the first occurence of the function in the statement).</param>
        /// <returns>                   The first token found that represents a function called 
        ///                             <paramref name="functionName"/>. If the function token is 
        ///                             not found, then returns null.</returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetTokenFunction(RToken token, 
                                               string functionName, 
                                               uint occurence = 0)
        {
            List <RToken> tokenFunctions = GetTokensFunctions(token, functionName);
            if (tokenFunctions.Count <= occurence)
                return null;
                        
            return tokenFunctions[(int)occurence];
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the token in token tree <paramref name="token"/> that has the smallest script 
        /// position (i.e. the left-most token).
        /// </summary>
        /// <param name="token"> The token in token tree <paramref name="token"/> that has the 
        ///     smallest script position (i.e. the left-most token).</param>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetTokenLeftMost(RToken token)
        {
            RToken tokenLeftMost = token;
            foreach (RToken child in token.ChildTokens)
            {
                RToken childLeftMost = GetTokenLeftMost(child);
                if (childLeftMost.ScriptPos < tokenLeftMost.ScriptPos)
                    tokenLeftMost = childLeftMost;
            }
            return tokenLeftMost;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the token that represents the value of parameter number 
        /// <paramref name="parameterNumber"/> in the function represented by <paramref name="token"/>.
        /// todo handle illegal parameter numbers
        /// </summary>
        /// <param name="token">           A token that represents a function</param>
        /// <param name="parameterNumber"> The number of the parameter to update. The first 
        ///                                parameter is 0. </param>
        /// <returns>                      The token that represents the value of parameter number 
        ///                                <paramref name="parameterNumber"/> in the function 
        ///                                represented by <paramref name="token"/>.</returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetTokenParameterFunction(RToken token, uint parameterNumber)
        {
            RToken tokenBracket = GetFirstNonPresentationChild(token);
            if (parameterNumber == 0)
                return GetTokenParameterFunctionValue(tokenBracket);

            RToken tokenComma = tokenBracket.ChildTokens[GetIndexFirstNonPresentationChild(token)   
                                                               + (int)parameterNumber];
            return GetTokenParameterFunctionValue(tokenComma);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the token that represents the value of the parameter specified by <paramref name="token"/>.
        /// </summary>
        /// <param name="token"> A token that represents either a function opening bracket '(' or a 
        ///                      comma that separates 2 function parameters.</param>
        /// <returns>            The token that represents the value of the parameter specified by 
        ///                      <paramref name="token"/>.</returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetTokenParameterFunctionValue(RToken token)
        {
            RToken tokenParameter = GetFirstNonPresentationChild(token);
            if (tokenParameter.TokenType == RToken.TokenTypes.ROperatorBinary
                && tokenParameter.Lexeme.Text == "=")
            {
                int posParameterValue = GetIndexFirstNonPresentationChild(tokenParameter) + 1;
                return tokenParameter.ChildTokens[posParameterValue];
            }
            return tokenParameter;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Recursively traverses the token tree represented by <paramref name="token"/> and returns
        /// <paramref name="token"/> and all its children as a flat list. The tokens in the list are 
        /// ordered by their position in the script.
        /// </summary>
        /// <param name="token"> The root of the token tree</param>
        /// <returns>            <paramref name="token"/> and all its children as a flat list. The 
        ///                      tokens in the list are ordered by their position in the script.</returns>
        /// ----------------------------------------------------------------------------------------
        private static List<RToken> GetTokensFlat(RToken token)
        {
            var tokens = new List<RToken> { token };
            foreach (RToken child in token.ChildTokens)
            {
                tokens.AddRange(GetTokensFlat(child));
            }

            tokens.Sort((a, b) => a.ScriptPos.CompareTo(b.ScriptPos));
            return tokens;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Recursively searches for the function <paramref name="functionName"/> in the 
        /// token tree represented by <paramref name="token"/> and returns a list of all the 
        /// occurences found. The tokens in the list are ordered by their position in the script.
        /// </summary>
        /// <param name="token">        The root of the token tree</param>
        /// <param name="functionName"> The function name to search for (e.g. 'ggplot')</param>
        /// <returns> A list of all the occurences found. The tokens in the list are ordered by 
        ///           their position in the script.</returns>
        /// ----------------------------------------------------------------------------------------
        private static List<RToken> GetTokensFunctions(RToken token, string functionName)
        {
            var tokens = new List<RToken>();

            if ((token.TokenType == RToken.TokenTypes.RFunctionName
                        || token.TokenType == RToken.TokenTypes.ROperatorBinary
                        || token.TokenType == RToken.TokenTypes.ROperatorUnaryLeft
                        || token.TokenType == RToken.TokenTypes.ROperatorUnaryRight)
                    && token.Lexeme.Text == functionName)
                tokens.Add(token);

            foreach (var child in token.ChildTokens)
            {
                tokens.AddRange(GetTokensFunctions(child, functionName));
            }

            tokens.Sort((x, y) => x.ScriptPos.CompareTo(y.ScriptPos));
            return tokens;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Recursively searches for the binary operator <paramref name="operatorText"/> in the 
        /// token tree represented by <paramref name="token"/> and returns a list of all the 
        /// occurences found. The tokens in the list are ordered by their position in the script.
        /// </summary>
        /// <param name="token">        The root of the token tree</param>
        /// <param name="operatorText"> The binary operator to search for (e.g. '+')</param>
        /// <returns> A list of all the occurences found. The tokens in the list are ordered by 
        ///           their position in the script.</returns>
        /// ----------------------------------------------------------------------------------------
        private static List<RToken> GetTokensOperators(RToken token, string operatorText)
        {
            var tokens = new List<RToken>();

            if ((token.TokenType == RToken.TokenTypes.ROperatorBinary
                        || token.TokenType == RToken.TokenTypes.ROperatorUnaryLeft
                        || token.TokenType == RToken.TokenTypes.ROperatorUnaryRight)
                    && token.Lexeme.Text == operatorText)
                tokens.Add(token);

            foreach (var child in token.ChildTokens)
            {
                tokens.AddRange(GetTokensOperators(child, operatorText));
            }

            tokens.Sort((x, y) => x.ScriptPos.CompareTo(y.ScriptPos));
            return tokens;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Converts <paramref name="statementScript"/> into a token tree and returns the token at 
        /// the root of the tree.
        /// If <paramref name="statementScript"/> consists of more than one statement, then throws 
        /// an exception.
        /// </summary>
        /// <param name="statementScript"></param>
        /// <returns> A token that represents <paramref name="statementScript"/></returns>
        /// ----------------------------------------------------------------------------------------
        private static RToken GetTokenStatement(string statementScript)
        {
            RTokenList tokenList = new RTokenList(statementScript);

            if (tokenList.Tokens.Count != 1)
            {
                // edge case: if the script is a single statement followed by an empty statement
                // containing presentation information (spaces, comments, newlines ec.),
                // then ignore the empty statement
                if (tokenList.Tokens.Count != 2
                        || tokenList.Tokens[1].TokenType != RToken.TokenTypes.REmpty
                        || tokenList.Tokens[1].ChildTokens.Count != 1
                        || tokenList.Tokens[1].ChildTokens[0].TokenType != RToken.TokenTypes.RPresentation)
                    throw new Exception("Script must be a single legal statement.");
            }

            return tokenList.Tokens[0];
        }

    }
}