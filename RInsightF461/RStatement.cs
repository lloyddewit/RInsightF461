using System;
using System.Collections.Generic;

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
        /// Adds the parameter named <paramref name="parameterName"/> to the function named 
        /// <paramref name="functionName"/>. The value of the parameter is set to 
        /// <paramref name="parameterValue"/>. If <paramref name="isQuoted"/> is true then encloses 
        /// the parameter value in double quotes. Returns the difference between the function's text  
        /// length before/after adding the parameter. If the function is not 
        /// found, then throws an exception.<para>
        /// Preconditions: The function must have at least 1 parameter; the function must not 
        /// already have a <paramref name="parameterName"/> parameter.</para>
        /// todo - remove need for preconditions?
        /// </summary>
        /// <param name="functionName">    The name of the function to add the parameter to</param>
        /// <param name="parameterName">   The name of the function parameter to add</param>
        /// <param name="parameterValue">  The new value of the added parameter</param>
        /// <param name="parameterNumber"> The number of the existing parameter to add the new parameter in front of. If zero, then adds the parameter before any existing parameters. If greater than the number of existing parameters, then adds the parameter after the last existing parameter.</param>
        /// <param name="isQuoted">        If true, then encloses the parameter value in double 
        ///     quotes</param>
        /// <returns> The difference between the function's text length before/after adding the 
        ///     parameter. For example if parameter `c` with value 3 is added to `fn(a=1, b=2)`, 
        ///     then the resulting function is `fn(a=1, b=2, c=3)` and 5 is returned 
        ///     (the length of `, c=3`). </returns>
        /// ----------------------------------------------------------------------------------------
        internal int AddParameterByName(string functionName, string parameterName,
                                        string parameterValue, uint parameterNumber, bool isQuoted)
        {
            RToken tokenFunction = GetTokenFunction(_token, functionName);
            if (tokenFunction is null)
            {
                throw new System.Exception("Function not found.");
            }

            // create token tree for new parameter
            RToken tokenBracketOpen = GetFirstNonPresentationChild(tokenFunction);
            parameterValue = isQuoted ? "\"" + parameterValue + "\"" : parameterValue;
            parameterNumber = Math.Min(parameterNumber, (uint)tokenBracketOpen.ChildTokens.Count - 1);
            RToken tokenParameter;
            if (parameterNumber == 0)
            {
                string dummyFunction = $"fn({parameterName}={parameterValue})";
                RTokenList tokenList = new RTokenList(dummyFunction);
                tokenParameter = tokenList.Tokens[0].ChildTokens[0].ChildTokens[0];
            }
            else
            { 
                string dummyFunction = $"fn(param1=0, {parameterName}={parameterValue})";
                RTokenList tokenList = new RTokenList(dummyFunction);
                tokenParameter = tokenList.Tokens[0].ChildTokens[0].ChildTokens[1];
            }

            // find the new parameter's script position
            uint scriptPosInsertPos = 
                    tokenBracketOpen.ChildTokens[(int)parameterNumber].ScriptPosStartStatement;

            // set the correct script start position for the new parameter
            AdjustStartPos(adjustment  : (int)scriptPosInsertPos - (int)tokenParameter.ScriptPosStartStatement,
                           scriptPosMin: 0,
                           token       : tokenParameter);

            // if the new parameter is the new first parameter and the function already has at
            // least one parameter, then add a comma before the old first parameter (e.g. if we
            // add 'paramNew=0' as first param to 'fn(paramOld=1)', then we get
            // 'fn(paramNew=0, paramOld=1)'.
            int adjustment = 0;
            bool functionHasParameters = 
                    (tokenBracketOpen.ChildTokens.Count == 2
                     && !tokenBracketOpen.ChildTokens[0].IsPresentation)
                    || tokenBracketOpen.ChildTokens.Count > 2;
            if (parameterNumber == 0 && functionHasParameters)
            {
                // create a token that is the same as param 0 but preceded by a comma
                RToken tokenParam0 = GetFirstNonPresentationChild(tokenBracketOpen);
                string dummyFunction = "fn(a, " + GetText(tokenParam0) + ")";
                RTokenList tokenList = new RTokenList(dummyFunction);
                RToken tokenDummyParam1 = tokenList.Tokens[0].ChildTokens[0].ChildTokens[1];
                AdjustStartPos(adjustment: (int)tokenParam0.ScriptPosStartStatement - (int)tokenDummyParam1.ScriptPosStartStatement,
                                             scriptPosMin: 0,
                                             token: tokenDummyParam1);
                // replace the old first param with the comma preceded version
                tokenBracketOpen.ChildTokens[tokenBracketOpen.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0] = tokenDummyParam1;

                // adjust the start positions of all tokens that come after the new parameter to account for the comma that was added
                adjustment += 2; // length of ", "
                AdjustStartPos(adjustment: adjustment,
                               scriptPosMin: tokenDummyParam1.ScriptPosEndStatement,
                               token: _token);
            }

            // adjust the script start position for all tokens in the statement that come after the
            // new parameter
            adjustment += GetText(tokenParameter).Length;
            AdjustStartPos(adjustment  : adjustment,
                           scriptPosMin: scriptPosInsertPos,
                           token       : _token);

            // insert the new parameter into the function's token tree
            tokenBracketOpen.ChildTokens.Insert((int)parameterNumber, tokenParameter);
            return adjustment;
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
                {
                    tokenFlat.ScriptPos = (uint)((int)tokenFlat.ScriptPos + adjustment);
                }
            }
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
        internal int RemoveParameterByName(string functionName, string parameterName)
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
                    RToken tokenParam2 = tokenBracketOpen.ChildTokens[tokenBracketOpen.ChildTokens[0].IsPresentation ? 2 : 1];
                    RToken tokenParam2NoComma = tokenParam2.ChildTokens[tokenParam2.ChildTokens[0].IsPresentation ? 1 : 0];

                    // remove any presentation tokens from the parameter
                    // todo this assumes that the 2nd parameter is a named parameter. Handle case when 2nd parameter is not named (e.g. `fn(a=1, 2)`)
                    RToken tokenParam2Name = GetFirstNonPresentationChild(tokenParam2NoComma);
                    if (tokenParam2Name.ChildTokens.Count > 0 && tokenParam2Name.ChildTokens[0].IsPresentation)
                    {
                        adjustment -= GetText(tokenParam2Name.ChildTokens[0]).Length; // because we removed the space(s)
                        tokenParam2Name.ChildTokens.Remove(tokenParam2Name.ChildTokens[0]);
                    }
                    tokenBracketOpen.ChildTokens[tokenBracketOpen.ChildTokens[0].IsPresentation ? 2 : 1] = tokenParam2NoComma;
                    adjustment--; // because we removed the comma
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
        /// update is specified by <paramref name="functionName"/>, and <paramref name="parameterNumber"/>. todo rename to SetParameterValue?
        /// </summary>
        /// <param name="functionName">    The name of the function or operator (e.g. `+`, `-` etc.)</param>
        /// <param name="parameterNumber"> The number of the parameter to update. For a function, 
        ///     the first parameter is 0. For a binary operator the left hand parameter is 0 and the 
        ///     right hand operator is 1. For a unary operator, the parameter number must be 0.</param>
        /// <param name="parameterValue">  The token's new value</param>
        /// <param name="isQuoted">        If True then put double quotes around 
        ///     <paramref name="parameterValue"/></param>
        /// <returns>                      The difference in length between the token's old value, 
        ///     and the token's new value. A negative number indicates that the new value is shorter 
        ///     than the new value.</returns>
        /// ----------------------------------------------------------------------------------------
        internal int SetToken(string functionName, uint parameterNumber, string parameterValue, 
                              bool isQuoted = false)
        {
            RToken tokenFunction = GetTokenFunction(_token, functionName);
            RToken tokenParameterValue;
            if (tokenFunction.TokenType == RToken.TokenTypes.RFunctionName)
            {
                tokenParameterValue = GetTokenParameterFunction(tokenFunction, parameterNumber);
            }
            else
            {
                tokenParameterValue = GetTokenParameterOperator(tokenFunction, parameterNumber);
            }

            parameterValue = isQuoted ? "\"" + parameterValue + "\"" : parameterValue;
            int adjustment = parameterValue.Length - tokenParameterValue.Lexeme.Text.Length;
            tokenParameterValue.Lexeme.Text = parameterValue;

            // update the script position of any subsequent tokens in statement
            AdjustStartPos(adjustment, tokenParameterValue.ScriptPos + 1);
            
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
            {
                throw new System.Exception("Token has no non-presentation children.");
            }

            int posFirstNonPresentationChild = token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;
            return token.ChildTokens[posFirstNonPresentationChild];
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
        private string GetText(RToken token)
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
                if (token.TokenType == RToken.TokenTypes.REmpty) continue;

                if (token.TokenType == RToken.TokenTypes.REndStatement)
                {
                    text += ";";
                }
                else if (token.TokenType == RToken.TokenTypes.RKeyWord
                         && (token.Lexeme.Text == "else"
                             || token.Lexeme.Text == "in"
                             || token.Lexeme.Text == "repeat"))
                {
                    text += " " + token.Lexeme.Text + " ";
                }
                else if (!token.IsPresentation) // ignore presentation tokens
                {
                    text += token.Lexeme.Text;
                }
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
        /// <returns>                   The first token found that represents a function called 
        ///                             <paramref name="functionName"/>. If the function token is 
        ///                             not found, then returns null.</returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenFunction(RToken token, string functionName)
        {
            if ((token.TokenType == RToken.TokenTypes.RFunctionName 
                || token.TokenType == RToken.TokenTypes.ROperatorBinary) 
               && token.Lexeme.Text == functionName)
            {
                return token;
            }

            foreach (var childToken in token.ChildTokens)
            {
                var result = GetTokenFunction(childToken, functionName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the token that represents the value of parameter number 
        /// <paramref name="parameterNumber"/> in the function represented by <paramref name="token"/>.
        /// </summary>
        /// <param name="token">           A token that represents a function</param>
        /// <param name="parameterNumber"> The number of the parameter to update. The first 
        ///                                parameter is 0. </param>
        /// <returns>                      The token that represents the value of parameter number 
        ///                                <paramref name="parameterNumber"/> in the function 
        ///                                represented by <paramref name="token"/>.</returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenParameterFunction(RToken token, uint parameterNumber)
        {
            uint posFirstNonPresentationChild =
                    token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? (uint)1 : 0;

            RToken tokenBracket = token.ChildTokens[(int)posFirstNonPresentationChild];
            if (parameterNumber == 0)
            {
                return GetTokenParameterFunctionValue(tokenBracket);
            }

            RToken tokenComma = tokenBracket.ChildTokens[(int)(posFirstNonPresentationChild 
                                                               + parameterNumber)];
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
        private RToken GetTokenParameterFunctionValue(RToken token)
        {
            int posFirstNonPresentationChild = 
                token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;

            RToken tokenParameter = token.ChildTokens[posFirstNonPresentationChild];
            if (tokenParameter.TokenType == RToken.TokenTypes.ROperatorBinary && tokenParameter.Lexeme.Text == "=")
            {
                posFirstNonPresentationChild = tokenParameter.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;
                int posParameterValue = posFirstNonPresentationChild + 1;
                return tokenParameter.ChildTokens[posParameterValue];
            }
            return tokenParameter;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the token that represents the value of parameter number <paramref name="parameterNumber"/> 
        /// in the operator represented by <paramref name="token"/>.
        /// </summary>
        /// <param name="token">           A token that represents an operator</param>
        /// <param name="parameterNumber"> The number of the parameter to return. 
        ///     For a binary operator the left hand parameter is 0 and the right hand operator is 1. 
        ///     For a unary operator, the parameter number must be 0. </param>
        /// <returns>                      The token that represents the value of parameter number 
        ///                                <paramref name="parameterNumber"/> in the operator 
        ///                                represented by <paramref name="token"/>.</returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenParameterOperator(RToken token, uint parameterNumber)
        {
            uint posFirstNonPresentationChild =
                    token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? (uint)1 : 0;

            return token.ChildTokens[(int)(posFirstNonPresentationChild + parameterNumber)];
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
        private List<RToken> GetTokensFlat(RToken token)
        {
            var tokens = new List<RToken> { token };
            foreach (RToken child in token.ChildTokens)
            {
                tokens.AddRange(GetTokensFlat(child));
            }

            tokens.Sort((a, b) => a.ScriptPos.CompareTo(b.ScriptPos));
            return tokens;
        }
    }
}