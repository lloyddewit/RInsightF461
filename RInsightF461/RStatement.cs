using System;
using System.Collections.Generic;

namespace RInsightF461
{
    /// <summary>
    /// Represents a single valid R statement.
    /// </summary>
    public class RStatement
    {
        /// <summary> True if this statement is an assignment statement (e.g. x <- 1). </summary>
        public bool IsAssignment { get; }

        /// <summary> The position in the script where this statement starts. </summary>
        public uint StartPos { get; }

        /// <summary>
        /// The text representation of this statement, including all formatting information (comments,
        /// spaces, extra newlines etc.).
        /// </summary>
        public string Text => GetText();

        /// <summary>
        /// The text representation of this statement, excluding all formatting information (comments,
        /// spaces, extra newlines etc.).
        /// </summary>
        public string TextNoFormatting{ get; } //todo call private function

        /// <summary>
        /// todo
        /// </summary>
        private RToken _token;

        private List<RToken> _tokensFlat;

        /// --------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructs an object representing a valid R statement from the <paramref name="token"/> 
        /// token tree. </summary>
        /// 
        /// <param name="token">  The tree of R tokens to process </param>
        /// <param name="tokensFlat"> A one-dimensional list of all the tokens in the script 
        ///                           containing <paramref name="token"/> (useful for conveniently 
        ///                           reconstructing the text representation of the statement).</param>
        /// --------------------------------------------------------------------------------------------
        public RStatement(RToken token, List<RToken> tokensFlat)
        {
            var assignments = new HashSet<string> { "->", "->>", "<-", "<<-", "=" };
            _token = token;
            _tokensFlat = tokensFlat;

            IsAssignment = _token.TokenType == RToken.TokenTypes.ROperatorBinary 
                           && assignments.Contains(_token.Lexeme.Text);

            StartPos = _token.ScriptPosStartStatement;

            //todo remove creation of Text
            uint endPos = _token.ScriptPosEndStatement;
            TextNoFormatting = GetTextNoFormatting(_tokensFlat, StartPos, endPos);

            // create a lossless text representation of the statement including all presentation
            // information (e.g. spaces, newlines, comments etc.)
            int startPosAdjustment = 0;
            bool tokenPrevIsEndStatement = false;
            bool firstNewLineFound = false;
            string text = "";
            foreach (RToken tokenFlat in _tokensFlat)
            {
                if (tokenFlat.TokenType == RToken.TokenTypes.REmpty) continue;

                uint tokenStartPos = tokenFlat.ScriptPosStartStatement;
                if (tokenStartPos < StartPos)
                {
                    tokenPrevIsEndStatement = tokenFlat.TokenType == RToken.TokenTypes.REndStatement;
                    continue;
                }
                string tokenText = tokenFlat.Lexeme.Text;
                if (tokenStartPos >= endPos)
                {
                    // if next statement has presentation text that belongs with the current statement
                    if (!tokenPrevIsEndStatement
                        && tokenFlat.IsPresentation
                        && tokenText.Length > 0)
                    {
                        text += tokenText;
                        if (tokenFlat.Lexeme.IsNewLine)
                        {
                            break;
                        }
                        continue;
                    }
                    break;
                }

                // ignore any presentation characters that belong to the previous statement
                if (text == ""
                    && StartPos != 0
                    && !tokenPrevIsEndStatement 
                    && !firstNewLineFound
                    && tokenFlat.IsPresentation
                    && tokenText.Length > 0)
                {
                    if (tokenFlat.Lexeme.IsNewLine)
                    {
                        firstNewLineFound = true;
                    }
                    startPosAdjustment += tokenText.Length;
                    tokenText = "";
                }
                text += tokenText;
                tokenPrevIsEndStatement = tokenFlat.TokenType == RToken.TokenTypes.REndStatement;
            }
            StartPos += (uint)startPosAdjustment;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="strFunctionName"></param>
        /// <param name="parameterNumber"></param>
        /// <returns></returns>
        public void SetToken(string functionName, int parameterNumber, string parameterValue, bool isQuoted = false)
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

            tokenParameterValue.Lexeme.Text = isQuoted ? "\"" + parameterValue + "\"" : parameterValue;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        private string GetText()
        {
            //uint startPos = _token.ScriptPosStartStatement;
            uint endPos = _token.ScriptPosEndStatement;

            // create a lossless text representation of the statement including all presentation
            // information (e.g. spaces, newlines, comments etc.)
            // todo int startPosAdjustment = 0;
            bool tokenPrevIsEndStatement = false;
            bool firstNewLineFound = false;
            string text = "";
            foreach (RToken tokenFlat in _tokensFlat)
            {
                if (tokenFlat.TokenType == RToken.TokenTypes.REmpty) continue;

                uint tokenStartPos = tokenFlat.ScriptPos;
                if (tokenStartPos < StartPos)
                {
                    tokenPrevIsEndStatement = tokenFlat.TokenType == RToken.TokenTypes.REndStatement;
                    continue;
                }
                string tokenText = tokenFlat.Lexeme.Text;
                if (tokenStartPos >= endPos)
                {
                    // if next statement has presentation text that belongs with the current statement
                    if (!tokenPrevIsEndStatement
                        && tokenFlat.IsPresentation
                        && tokenText.Length > 0)
                    {
                        text += tokenText;
                        if (tokenFlat.Lexeme.IsNewLine)
                        {
                            break;
                        }
                        continue;
                    }
                    break;
                }

                // ignore any presentation characters that belong to the previous statement
                //if (text == ""
                //    && StartPos != 0
                //    && !tokenPrevIsEndStatement
                //    && !firstNewLineFound
                //    && tokenFlat.IsPresentation
                //    && tokenText.Length > 0)
                //{
                //    if (tokenFlat.Lexeme.IsNewLine)
                //    {
                //        firstNewLineFound = true;
                //    }
                //    // todo startPosAdjustment += tokenText.Length;
                //    tokenText = "";
                //}
                text += tokenText;
                tokenPrevIsEndStatement = tokenFlat.TokenType == RToken.TokenTypes.REndStatement;
            }
            // todo StartPos += (uint)startPosAdjustment;
            return text;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="iParameterNumber"></param>
        /// <returns></returns>
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

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="iParameterNumber"></param>
        /// <returns></returns>
        private RToken GetTokenParameterOperator(RToken token, int iParameterNumber)
        {
            int posFirstNonPresentationChild =
                    token.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? 1 : 0;

            return token.ChildTokens[posFirstNonPresentationChild + iParameterNumber];
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
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

        /// --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a text representation of the statement, excluding all formatting information 
        /// (e.g. spaces, newlines, comments etc.).
        /// </summary>
        /// <param name="tokensFlat"> Flat list of all the tokens in the script</param>
        /// <param name="posStart">   The start position of the statement in the script</param>
        /// <param name="posEnd">     The end position of the statement in the script</param>
        /// <returns>                 A text representation of the statement, excluding all formatting
        ///                           information</returns>
        /// --------------------------------------------------------------------------------------------
        private string GetTextNoFormatting(List<RToken> tokensFlat, 
                                           uint posStart, uint posEnd)
        {
            string text = "";
            foreach (RToken token in tokensFlat)
            {
                if (token.TokenType == RToken.TokenTypes.REmpty) continue;

                uint tokenStartPos = token.ScriptPosStartStatement;
                if (tokenStartPos < posStart) continue;
                if (tokenStartPos >= posEnd) break;

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
    }
}