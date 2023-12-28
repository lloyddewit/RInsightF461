﻿using Microsoft.VisualBasic;

namespace RInsightF461
{

// ---------------------------------------------------------------------------------------------------
// file:		clsRElement.vb
// 
// summary:	TODO
// ---------------------------------------------------------------------------------------------------
public class RElement
{
    /// <summary> The text representation of the element (e.g. '+', '/', 'myFunction', 
///           '"my string constant"' etc.). </summary>
    public string strTxt;

    /// <summary> If true, then the element is surrounded by round brackets. For example, if the 
///           script is 'a*(b+c)', then the element representing the '+' operator will have 
///           'bBracketed' set to true. </summary>
    public bool bBracketed;

    /// <summary> 
/// Any formatting text that precedes the element. The formatting text may consist of spaces, 
/// comments and new lines to make the script more readable for humans. For example, in the 
/// example below, 'strprefix' for the 'myFunction' element shall be set to 
/// "#comment1\n  #comment2\n  ".<code>
/// 
/// #comment1<para>
///   #comment2</para><para>
///   myFunction()</para></code></summary>
    public string strPrefix = "";

    public RElement(RToken clsToken, bool bBracketedNew = false, string strPackagePrefix = "")
    {
        strTxt = clsToken.Lexeme.Text;
        bBracketed = bBracketedNew;
        strPrefix = strPackagePrefix + (clsToken.ChildTokens.Count > 0 && clsToken.ChildTokens[0].TokenType == RToken.TokenTypes.RPresentation ? clsToken.ChildTokens[0].Lexeme.Text : "");

    }
}
}