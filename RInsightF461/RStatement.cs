using System.Collections.Generic;

namespace RInsightF461
{
    /// <summary>
    /// Represents a single valid R statement.
    /// </summary>
    public class RStatement
    {
        /// <summary> True if this statement is an assignment statement (e.g. x <- 1). </summary>
        public bool IsAssignment { get => GetIsAssignment(); }

        /// <summary> The position in the script where this statement starts. </summary>
        public uint StartPos { get { return _token.ScriptPosStartStatement; } }

        /// <summary>
        /// The text representation of this statement, including all formatting information (comments,
        /// spaces, extra newlines etc.).
        /// </summary>
        public string Text => GetText();

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
        /// For every token in the token tree, if the token's start position in the script is 
        /// greater than <paramref name="scriptPosMin"/>, then adjust the start position by 
        /// <paramref name="adjustment"/>.
        /// </summary>
        /// <param name="adjustment">   If positive, then increase the each token's start position 
        ///     by this amount; if negative, then reduce each token's start position by this amount.
        ///     </param>
        /// <param name="scriptPosMin"> If the token's start position is less than or equal to this, 
        ///     then do nothing</param>
        /// ----------------------------------------------------------------------------------------
        internal void AdjustStartPos(int adjustment, uint scriptPosMin = 0)
        {
            List<RToken> tokensFlat = GetTokensFlat(_token);
            foreach (RToken token in tokensFlat)
            {
                if (token.ScriptPos > scriptPosMin)
                {
                    token.ScriptPos = (uint)((int)token.ScriptPos + adjustment);
                }
            }
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of the specified token to <paramref name="parameterValue"/>. The token to 
        /// update is specified by <paramref name="functionName"/>, and <paramref name="parameterNumber"/>.
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
        internal int SetToken(string functionName, int parameterNumber, string parameterValue, 
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
            AdjustStartPos(adjustment, tokenParameterValue.ScriptPos);
            
            return adjustment;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private bool GetIsAssignment()
        {
            var assignments = new HashSet<string> { "->", "->>", "<-", "<<-", "=" };
            return _token.TokenType == RToken.TokenTypes.ROperatorBinary
                                       && assignments.Contains(_token.Lexeme.Text);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private string GetText()
        {
            // create a lossless text representation of the statement including all presentation
            // information (e.g. spaces, newlines, comments etc.)
            string text = "";
            List<RToken> tokensFlat = GetTokensFlat(_token);
            foreach (RToken tokenFlat in tokensFlat)
            {
                text += tokenFlat.Lexeme.Text;
            }            
            return text;
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a text representation of the statement, excluding all formatting information 
        /// (e.g. spaces, newlines, comments etc.).
        /// </summary>
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
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
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
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="iParameterNumber"></param>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenParameterFunction(RToken token, int iParameterNumber)
        {
            int posFirstNonPresentationChild =
                    token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;

            RToken tokenBracket = token.ChildTokens[posFirstNonPresentationChild];

            if (iParameterNumber == 0)
            {
                return GetTokenParameterValue(tokenBracket);
            }

            RToken tokenComma = tokenBracket.ChildTokens[posFirstNonPresentationChild + iParameterNumber];
            return GetTokenParameterValue(tokenComma);
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="iParameterNumber"></param>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenParameterOperator(RToken token, int iParameterNumber)
        {
            int posFirstNonPresentationChild =
                    token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;

            return token.ChildTokens[posFirstNonPresentationChild + iParameterNumber];
        }

        /// ----------------------------------------------------------------------------------------
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------------------
        private RToken GetTokenParameterValue(RToken token)
        {
            int posFirstNonPresentationChild = token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;

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
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
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