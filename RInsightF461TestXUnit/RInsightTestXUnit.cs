/*
 Copyright(C) 2023 Stephen Lloyd
 
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using RInsightF461;
using System.Collections.Specialized;
using System.Collections;
using static System.Collections.Specialized.BitVector32;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RInsightTestXUnit;

public class RInsightTestXUnit
{
    [Fact]
    public void TestGetAsExecutableScript()
    {
        string strInput, strActual;
        OrderedDictionary dctRStatements;

        strInput = " f1(f2(),f3(a),f4(b=1),f5(c=2,3),f6(4,d=5),f7(,),f8(,,),f9(,,,),f10(a,,))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);

        strInput = "f0(f1(),f2(a),f3(f4()),f5(f6(f7(b))))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "f0(o4a=o4b,o4c=(o8a+o8b)*(o8c-o8d),o4d=f4a(o6e=o6f,o6g=o6h))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a+b+c\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "2+1-10/5*3\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "1+2-3*10/5\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "(a-b)*(c+d)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a/(b)*((c))+(d-e)/f*g+(((d-e)/f)*g)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "var1<-pkg1::var2\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "var1<-pkg1::obj1$obj2$var2\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "var1<-pkg1::obj1$fun1(para1,para2)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a<-b::c(d)+e\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "f1(~a,b~,-c,+d,e~(f+g),!h,i^(-j),k+(~l),m~(~n),o/-p,q*+r)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);



        strInput = "a[1]-b[c(d)+e]/f(g[2],h[3],i[4]*j[5])-k[l[m[6]]]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[[1]]-b[[c(d)+e]]/f(g[[2]],h[[3]],i[[4]]*j[[5]])-k[[l[[m[6]]]]]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "df[[\"a\"]]\n" + "lst[[\"a\"]][[\"b\"]]"; // same as 'df$a' and 'lst$a$b'
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)10, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "x<-\"a\";df[x]"; // same as 'df$a' and 'lst$a$b'
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)7, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "df<-data.frame(x = 1:10, y = 11:20, z = letters[1:10])\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "x[3:5]<-13:15;names(x)[3]<-\"Three\"";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)14, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "x[3:5]<-13:15;\n" + "names(x)[3]<-\"Three\"";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)14, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "x[3:5]<-13:15;" + "\r\n" + "names(x)[3]<-\"Three\"";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)14, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "x[3:5]<-13:15;#comment\n" + "names(x)[3]<-\"Three\"";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)14, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = "a[]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,,,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[b,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,c]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[b,c]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[\"b\",]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,\"c\",1]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[-1,1:2,,x<5|x>7]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[-1,1:2,,f1(b,c[d], f2(e)[,,f3(f,g),,]),x<5|x>7]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " a[]#comment\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a [,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[ ,,] #comment\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[, ,,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[b, ]   #comment\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a [  ,   c    ]     \n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "#comment\n" + "a[b,c]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[ \"b\"  ,]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[,#comment\n" + "\"c\",  1 ]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[ -1 , 1  :   2    ,     ,      x <  5   |    x      > 7  ]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/lloyddewit/RScript/issues/18
        strInput = "weather[,1]<-As.Date(weather[,1],format = \"%m/%d/%Y\")\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " weather  [   ,  #comment\n" + "  1     ] <-  As.Date   (weather     [#comment\n" + " ,  1   ]    ,    format =  \"%m/%d/%Y\"    )     \n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "dat <- dat[order(dat$tree, dat$dir), ]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/africanmathsinitiative/R-Instat/pull/8551
        strInput = "d22 <- d22[order(d22$tree, d22$day),]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "res <- MCA(poison[,3:8],excl =c(1,3))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/africanmathsinitiative/R-Instat/pull/8707
        strInput = "a[][b]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a[][]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "output[][-1]\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);



        strInput = "data_book$display_daily_table(data_name = \"dodoma\", climatic_element = \"rain\", " + "date_col = \"Date\", year_col = \"year\", Misscode = \"m\", monstats = c(sum = \"sum\"))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "stringr::str_split_fixed(string = date,pattern = \" - \",n = \"5 \")\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "ggplot2::ggplot(data = c(sum = \"sum\"),mapping = ggplot2::aes(x = fert,y = size,colour = variety))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "last_graph<-ggplot2::ggplot(data = survey,mapping = ggplot2::aes(x = fert,y = size,colour = variety))" + "+ggplot2::geom_line()" + "+ggplot2::geom_rug(colour = \"orange\")" + "+theme_grey()" + "+ggplot2::theme(axis.text.x = ggplot2::element_text())" + "+ggplot2::facet_grid(facets = village~variety,space = \"fixed\")\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "dodoma <- data_book$get_data_frame(data_name = \"dodoma\", stack_data = TRUE, measure.vars = c(\"rain\", \"tmax\", \"tmin\"), id.vars = c(\"Date\"))\n" + "last_graph <- ggplot2::ggplot(data = dodoma, mapping = ggplot2::aes(x = date, y = value, colour = variable)) + ggplot2::geom_line() + " + "ggplot2::geom_rug(data = dodoma%>%filter(is.na(value)), colour = \"red\") + theme_grey() + ggplot2::theme(axis.text.x = ggplot2::element_text(), legend.position = \"none\") + " + "ggplot2::facet_wrap(scales = \"free_y\", ncol = 1, facet = ~variable) + ggplot2::xlab(NULL)\n" + "data_book$add_graph(graph_name = \"last_graph\", graph = last_graph, data_name = \"dodoma\")\n" + "data_book$get_graphs(data_name = \"dodoma\", graph_name = \"last_graph\")";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)4, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)139, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)534, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)623, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);

        strInput = "a->b\n" + "c->>d\n" + "e<-f\n" + "g<<-h\n" + "i=j";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)5, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)5, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)11, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)16, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);
        Assert.Equal((UInt32)22, dctRStatements.Cast<DictionaryEntry>().ElementAt(4).Key);

        strInput = "x<-df$`a b`\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "names(x)<-c(\"a\",\"b\")\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a<-b" + "\r" + "c(d)" + "\r\n" + "e->>f+g\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " f1(  f2(),   f3( a),  f4(  b =1))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "  f0(   o4a = o4b,  o4c =(o8a   + o8b)  *(   o8c -  o8d),   o4d = f4a(  o6e =   o6f, o6g =  o6h))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " a  /(   b)*( c)  +(   d- e)  /   f *g  +(((   d- e)  /   f)* g)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " a  +   b    +     c\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " var1  <-   pkg1::obj1$obj2$var2\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "    pkg ::obj1 $obj2$fn1 (a ,b=1, c    = 2 )\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = " f1(  ~   a,    b ~,  -   c,    + d,  e   ~(    f +  g),   !    h, i  ^(   -    j), k  +(   ~    l), m  ~(   ~    n), o  /   -    p, q  *   +    r)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = 
            " a  [\r" + 
            "   1\n" + 
            "] -  b   [c (  d   )+ e  ]   /f (  g   [2 ]  ,   h[ \r\n" + 
            "3  ]  \n" + 
            " ,i [  4   ]* j  [   5] )  -   k[ l  [   m[ 6  ]   ]   ]";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput + "x").statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)128, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strInput = 
            "#precomment1\n" + 
            " # precomment2\n" + 
            "  #  precomment3\n" + 
            @" f1(  f2(),   f3( a),  f4(  b =1))#comment1~!@#$%^&*()_[] {} \|;:',./<>?" + "\n" + 
            "  f0(   o4a = o4b,  o4c =(o8a   + o8b)  *(   o8c -  o8d),   o4d = f4a(  o6e =   o6f, o6g =  o6h)) # comment2\",\" cbaa \n" + 
            " a  /(   b)*( c)  +(   d- e)  /   f *g  +(((   d- e)  /   f)* g)   #comment3\n" + 
            "#comment 4\n" + 
            " a  +   b  +     c\n" + 
            "\n\n" + 
            "  #comment5\n" + 
            "   # comment6 #comment7\n" + 
            "endSyntacticName";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)5, (UInt32)dctRStatements.Count);

        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)118, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)236, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)313, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);
        Assert.Equal((UInt32)343, dctRStatements.Cast<DictionaryEntry>().ElementAt(4).Key);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("f1(f2(),f3(a),f4(b=1));" 
            + "f0(o4a=o4b,o4c=(o8a+o8b)*(o8c-o8d),o4d=f4a(o6e=o6f,o6g=o6h));" 
            + "a/(b)*(c)+(d-e)/f*g+(((d-e)/f)*g);" + "a+b+c;" 
            + "endSyntacticName", strActual);

        strInput = "#comment1\n" + "a#comment2" + "\r" + " b #comment3" + "\r\n" + "#comment4\n" + "  c  " + "\r\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)3, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)21, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)35, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("a;b;c", strActual);

        strInput = "#not ignored comment";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)1, (UInt32)dctRStatements.Count);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("", strActual);
        dctRStatements = new RScript(strActual).statements;
        Assert.Equal((UInt32)0, (UInt32)dctRStatements.Count);

        strInput = "#not ignored comment\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)1, (UInt32)dctRStatements.Count);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("", strActual);

        strInput = "f1()\n" + "# not ignored comment" + "\r\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)5, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("f1()", strActual);

        strInput = "f1()\n" + "# not ignored comment\n" + "# not ignored comment2" + "\r" + " " + "\r\n" + "# not ignored comment3";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)2, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)5, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);

        // issue lloyddewit/rscript#20
        strInput = "# Code run from Script Window (all text)" + Environment.NewLine + "1";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)1, (UInt32)dctRStatements.Count);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("", strActual);

        strInput = "";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)0, (UInt32)dctRStatements.Count);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("", strActual);


        // Test string constants that contain line breaks
        strInput = "x <- \"a\n\"\n" 
            + "fn1(\"bc \nd\", e)\n" 
            + "fn2( \"f gh\n\",i)\n" 
            + "x <- 'a\r\r'\n" 
            + "fn1('bc \r\r\r\rd', e)\n" 
            + "fn2( 'f gh\r',i)\n" 
            + "x <- `a\r\n`\n" 
            + "fn1(`bc \r\nj\r\nd`, e)\n" 
            + "fn2( `f gh\r\nkl\r\nmno\r\n`,i)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)9, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)10, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)26, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)42, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);
        Assert.Equal((UInt32)53, dctRStatements.Cast<DictionaryEntry>().ElementAt(4).Key);
        Assert.Equal((UInt32)72, dctRStatements.Cast<DictionaryEntry>().ElementAt(5).Key);
        Assert.Equal((UInt32)88, dctRStatements.Cast<DictionaryEntry>().ElementAt(6).Key);
        Assert.Equal((UInt32)99, dctRStatements.Cast<DictionaryEntry>().ElementAt(7).Key);
        Assert.Equal((UInt32)119, dctRStatements.Cast<DictionaryEntry>().ElementAt(8).Key);

        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("x<-\"a\n\";"
            + "fn1(\"bc \nd\",e);"
            + "fn2(\"f gh\n\",i);"
            + "x<-'a\r\r';"
            + "fn1('bc \r\r\r\rd',e);"
            + "fn2('f gh\r',i);"
            + "x<-`a\r\n`;"
            + "fn1(`bc \r\nj\r\nd`,e);"
            + "fn2(`f gh\r\nkl\r\nmno\r\n`,i)", strActual);

        // https://github.com/africanmathsinitiative/R-Instat/issues/7095  
        strInput = "data_book$import_data(data_tables =list(data3 =clipr::read_clip_tbl(x =\"Category    Feature    Ease_of_Use     Operating Systems\n" + "\", header =TRUE)))\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/africanmathsinitiative/R-Instat/issues/7095  
        strInput = "Data <- data_book$get_data_frame(data_name = \"Data\")\n" + "last_graph <- ggplot2::ggplot(data = Data |> dplyr::filter(rain > 0.85), mapping = ggplot2::aes(y = rain, x = make_factor(\"\")))" + " + ggplot2::geom_boxplot(varwidth = TRUE, coef = 2) + theme_grey()" + " + ggplot2::theme(axis.text.x = ggplot2::element_text(angle = 90, hjust = 1, vjust = 0.5))" + " + ggplot2::xlab(NULL) + ggplot2::facet_wrap(facets = ~ Name, drop = FALSE)\n" + "data_book$add_graph(graph_name = \"last_graph\", graph = last_graph, data_name = \"Data\")\n" + "data_book$get_graphs(data_name = \"Data\", graph_name = \"last_graph\")";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)4, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)53, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)412, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)499, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);

        // https://github.com/africanmathsinitiative/R-Instat/issues/7095  
        strInput = "ifelse(year_2 > 30, 1, 0)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/africanmathsinitiative/R-Instat/issues/7377
        strInput = "(year-1900)*(year<2000)+(year-2000)*(year>1999)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // Test string constants that have quotes embedded inside
        strInput = @"a("""", ""\""\"""", ""b"", ""c(\""d\"")"", ""'"", ""''"", ""'e'"", ""`"", ""``"", ""`f`"")" + "\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = @"a('', '\'\'', 'b', 'c(\'d\')', '""', '""""', '""e""', '`', '``', '`f`')" + "\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = @"a(``, `\`\``, `b`, `c(\`d\`)`, `""`, `""""`, `""e""`, `'`, `''`, `'f'`)" + "\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "x<-\"she said 'hello'\"\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "read_clip_tbl(x = \"Ease_of_Use" + "\t" + @"Hides R by default to prevent \""code shock\""" + "\t" + "  1\", header = TRUE)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // issue lloyddewit/rscript#17
        strInput = "?log\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // issue lloyddewit/rscript#21
        strInput = "?a\n" + "? b\n" + " +  c\n" + "  -   d +#comment1\n" + "(!e) - #comment2\n" + "(~f) +\n" + "(+g) - \n" + "(-h)";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)4, (UInt32)dctRStatements.Count);

        // issue lloyddewit/rscript#32
        strInput = "??log\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "??a\n" + "?? b\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // issue lloyddewit/rscript#16
        strInput = "\"a\"+\"b\"\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "  tfrmt(\n" + "  # specify columns in the data\n" + "  group = c(rowlbl1, grp),\n" + "  label = rowlbl2,\n" + "  column = column, \n" + "  param = param,\n" + "  value = value,\n" + "  sorting_cols = c(ord1, ord2),\n" + "  # specify value formatting \n" + "  body_plan = body_plan(\n" + "  frmt_structure(group_val = \".default\", label_val = \".default\", frmt_combine(\"{n} ({pct} %)\",\n" + "                                                                    n = frmt(\"xxx\"),\n" + "                                                                                pct = frmt(\"xx.x\"))),\n" + "    frmt_structure(group_val = \".default\", label_val = \"n\", frmt(\"xxx\")),\n" + "    frmt_structure(group_val = \".default\", label_val = c(\"Mean\", \"Median\", \"Min\", \"Max\"), frmt(\"xxx.x\")),\n" + "    frmt_structure(group_val = \".default\", label_val = \"SD\", frmt(\"xxx.xx\")),\n" + "    frmt_structure(group_val = \".default\", label_val = \".default\", p = frmt_when(\">0.99\" ~ \">0.99\",\n" + "                                                                                 \"<0.001\" ~ \"<0.001\",\n" + "                                                                                 TRUE ~ frmt(\"x.xxx\", missing = \"\"))))) %>% \n" + "  print_to_gt(data_demog) %>% \n" + "  tab_options(\n" + "    container.width = 900)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // issue lloyddewit/rscript#14
        // Examples from https://www.tidyverse.org/blog/2023/04/base-vs-magrittr-pipe/
        strInput = "x %>% f(1, .)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "x |> f(1, y = _)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "df %>% split(.$var)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "{a\nb}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("{a;b}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "df %>% {split(.$x, .$y)}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("df%>%{split(.$x,.$y)}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "mtcars %>% .$cyl\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);


        // Examples from https://stackoverflow.com/questions/67744604/what-does-pipe-greater-than-mean-in-r
        strInput = "c(1:3, NA_real_) |> sum(na.rm = TRUE)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "split(x = iris[-5], f = iris$Species) |> lapply(min) |> Do.call(what = rbind)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "iris[iris$Sepal.Length > 7,] %>% subset(.$Species==\"virginica\")\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "1:3 |> sum\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        // https://github.com/africanmathsinitiative/R-Instat/issues/8533
        strInput = "{a}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "a<-b(c,{d})";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "Nightingale <- within(Nightingale, {\n" +
                   "Total <- Disease + Wounds + Other\n" +
                   "Disease.pct <- 100*Disease/Total\n" +
                   "Wounds.pct <- 100*Wounds/Total\n" +
                   "Other.pct <- 100*Other/Total\n" +
                   "})\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "colors <- c(\"blue\", \"red\", \"black\")\r\n"
            + "with(Nightingale, {\r\n"
            + "\tplot(Date, Disease.pct, type=\"n\",  ylim=c(0,100), cex.lab=1.25,\r\n"
            + "\t\tylab=\"Percent deaths\", xlab=\"Date\", xaxt=\"n\",\r\n"
            + "\t\tmain=\"Percentage of Deaths by Cause\");\r\n"
            + "\t# background, to separate before, after\r\n"
            + "\trect(as.Date(\"1854/4/1\"), -10, as.Date(\"1855/3/1\"), \r\n"
            + "\t\t1.02*max(Disease.rate), col=gray(.90), border=\"transparent\");\r\n"
            + "\ttext( as.Date(\"1854/4/1\"), .98*max(Disease.pct), \"Before Sanitary\\nCommission\", pos=4);\r\n"
            + "\ttext( as.Date(\"1855/4/1\"), .98*max(Disease.pct), \"After Sanitary\\nCommission\", pos=4);\r\n"
            + "\t# plot the data\r\n"
            + "\tpoints(Date, Disease.pct, type=\"b\", col=colors[1], lwd=3);\r\n"
            + "\tpoints(Date, Wounds.pct, type=\"b\", col=colors[2], lwd=2);\r\n"
            + "\tpoints(Date, Other.pct, type=\"b\", col=colors[3], lwd=2)\r\n"
            + "\t}\r\n)";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);

        strInput = "means <- by(cic[,5], cic[,c(2,1)], function(x) mean(x,na.rm=TRUE))";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);


        // if-else statements
        strInput = "if(a)b";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(a)b";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a)b", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(a)\nb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a)b", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(a){b}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(a){b}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a){b}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(a)\n{b}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a){b}", (dctRStatements[0] as RStatement)?.TextNoFormatting);


        strInput = "if(x>10){fn1(paste(x,\"is greater than 10\"))}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(x>10){fn1(paste(x,\"is greater than 10\"))}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(val > 5) break";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(val>5)break", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if (x %% 2 == 0) \n    return(\"even\")";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(x%%2==0)return(\"even\")", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "    if (i == 8)\r\n        next\n    if(i == 5)\n        break";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("if(i==8)next;if(i==5)break", strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("if(i==8)next", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(i==5)break", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "if(a<b){c}\n" +
            "        if(d<=e){f}\n" +
            "        if(g==h) { #1\n" +
            "          i } #2\n" +
            "          if (j >= k)\n" +
            "        {\n" +
            "          l   #3  \n" +
            "        }\n" +
            "        if (m)\n" +
            "              #4\n" +
            "          n+\n" +
            "            o  #5\n" +
            "        if(p!=q)\n" +
            "        {\n" +
            "        q1()[id \n" +
            "                       ]\n" +
            "        q2([[j[k]]])  \n\n" +
            "        }\r\n\r\n" +
            "        if(e=f)\n" +
            "                        break\n" +
            "        next\n" +
            "        if (function(fn1(g,fn2=function(h)fn3(i/sum(j)*100)))fnBody)\n" +
            "            return(k)";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal((UInt32)9, (UInt32)dctRStatements.Count);
        Assert.Equal((UInt32)0, dctRStatements.Cast<DictionaryEntry>().ElementAt(0).Key);
        Assert.Equal((UInt32)11, dctRStatements.Cast<DictionaryEntry>().ElementAt(1).Key);
        Assert.Equal((UInt32)31, dctRStatements.Cast<DictionaryEntry>().ElementAt(2).Key);
        Assert.Equal((UInt32)70, dctRStatements.Cast<DictionaryEntry>().ElementAt(3).Key);
        Assert.Equal((UInt32)131, dctRStatements.Cast<DictionaryEntry>().ElementAt(4).Key);
        Assert.Equal((UInt32)194, dctRStatements.Cast<DictionaryEntry>().ElementAt(5).Key);
        Assert.Equal((UInt32)298, dctRStatements.Cast<DictionaryEntry>().ElementAt(6).Key);
        Assert.Equal((UInt32)346, dctRStatements.Cast<DictionaryEntry>().ElementAt(7).Key);
        Assert.Equal((UInt32)359, dctRStatements.Cast<DictionaryEntry>().ElementAt(8).Key);

        strInput = "if(a)b else c";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(d)e else f";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(d)e else f", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(g)\nh else i";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(g)h else i", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if\n(j)\nk else\nl";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(j)k else l", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        // key word snippets from https://github.com/africanmathsinitiative/R-Instat/pull/8707
        strInput = "## Don't show: \n" +
            "if (requireNamespace(\"dplyr\", quietly = TRUE)) (if (getRversion() >= \"3.4\") withAutoprint else force)({ # examplesIf\n" +
            "## End(Don't show)\n\n" +
            "library(dplyr)\n\n" +
            "austen_books() %>%\n" +
            "    group_by(book) %>%\n" +
            "    summarise(total_lines = n())\n" +
            "## Don't show: \n" +
            "}) # examplesIf\n" +
            "## End(Don't show)\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(3, dctRStatements.Count);
        Assert.Equal("if(requireNamespace(\"dplyr\",quietly=TRUE))(if(getRversion()>=\"3.4\")withAutoprint else force)", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("({library(dplyr);austen_books()%>%group_by(book)%>%summarise(total_lines=n())})", (dctRStatements[1] as RStatement)?.TextNoFormatting);
        Assert.Equal("", (dctRStatements[2] as RStatement)?.TextNoFormatting);

        strInput = "if(x > 10){\n" +
            "fn1(paste(x, \"is greater than 10\"))\n" +
            "} else\n" +
            "{\n" +
            "    fn2(paste(x, \"Is less than 10\"))\n" +
            "} \n" +
            "if(x %% 2 == 0)\n" +
            "    return(\"even\") else\n" +
            "    return(\"odd\")";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("if(x>10){fn1(paste(x,\"is greater than 10\"))} else {fn2(paste(x,\"Is less than 10\"))}", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(x%%2==0)return(\"even\") else return(\"odd\")", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "if(a<b){c}" +
        "\nif(d<=e){f}" +
        "\nif(g==h) { #1" +
        "\n i } #2" +
        "\n if (j >= k)" +
        "\n{" +
        "\nl   #3  " +
        "\n}" +
        "\nif (m)\n#4" +
        "\n  n+\n  o  #5" +
        "\nif(p!=q)" +
        "\n{" +
        "\nincomplete()[id \n]" +
        "\nincomplete([[j[k]]]  \n)" +
        "\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(6, dctRStatements.Count);
        Assert.Equal("if(a<b){c}", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(d<=e){f}", (dctRStatements[1] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(g==h){i}", (dctRStatements[2] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(j>=k){l}", (dctRStatements[3] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(m)n+o", (dctRStatements[4] as RStatement)?.TextNoFormatting);
        Assert.Equal("if(p!=q){incomplete()[id];incomplete([[j[k]]])}", (dctRStatements[5] as RStatement)?.TextNoFormatting);


        // test correct identification of end statements
        strInput = "a;b";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("b", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "a;\nb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("b", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "a\rb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("b", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "a#1\r\nb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("b", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "a#1\n\rb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("b", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "a#1\r\n\r\n#2 b";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = " a";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = " \na";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = " \n\r\r\na";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "{c\n" +
            "d+e}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        strActual = new RScript(strInput).GetAsExecutableScript(false);
        Assert.Equal("{c;d+e}", strActual);

        strInput = "a;\nb";
        strActual = new RScript(strInput).GetAsExecutableScript();
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("a;", (dctRStatements[0] as RStatement)?.Text);
        Assert.Equal("\nb", (dctRStatements[1] as RStatement)?.Text);
        Assert.Equal(0, (int)((dctRStatements[0] as RStatement)?.StartPos ?? 999));
        Assert.Equal(2, (int)((dctRStatements[1] as RStatement)?.StartPos ?? 999));
        Assert.Equal(strInput, strActual);


        strInput = "for(a in 1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a in 1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a in 1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a  in  1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a  in  1 :5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in  1  : 5 ) a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for\n(a in 1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a \nin 1:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a in 1\n:5)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a  in  1:5\n)a\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a  in  1 :5)\na\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for\n(\na \nin  \n1  \n: \n5 \n) \na\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5)a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in 1:5){a}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a in 1:5){a\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a in 1:5){a;b\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a;b}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a  in  1:5){a\nb\nc\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a;b;c}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in 1:5)\n{a}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (a in 1:5)\n{\na\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for ( a in 1:5)\n\n{a;b\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in 1:5){a;b}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(a)b else if(c)d else e";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in 1:2)if(b)c else d";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in 1:2)if(b)for(c in 5:6)d";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for(a in 1:2)for(b in 3:4)for(c in 5:6)d";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        // example from https://stackoverflow.com/questions/63663191/complex-for-loop-in-r
        strInput = "cnt = c(5, 10, 15)\n" +
            "length = 0\n" +
            "for (i in 1:length(cnt))\n" +
            "{\n" +
            "    for\n(\nj \nin\n 1\n:\ncnt\n[\ni\n]\n)\n" +
            "    {\n" +
            "        length = length + 1\n" +
            "    }\n" +
            "\n" +
            "    for (inner in 1:length)\n" +
            "    {\n" +
            "        print(inner)\n" +
            "    }\n" +
            "}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(3, dctRStatements.Count);
        Assert.Equal("for(i in 1:length(cnt)){for(j in 1:cnt[i]){length=length+1};for(inner in 1:length){print(inner)}}", (dctRStatements[2] as RStatement)?.TextNoFormatting);

        strInput =
            "vec < -c(\"apple\", \"banana\", \"cherry\")\n" +
            "for (fruit in vec)\n" +
            "{\n" +
            "  print(fruit)" +
            "\n}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("for(fruit in vec){print(fruit)}", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput =
            "for (i in 1:3) {\r\n" +
            "  for (j in 1:3) {\r\n" +
            "    print(paste(\"i is\", i, \"and j is\", j))\r\n" +
            "  }\r\n}\r\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(i in 1:3){for(j in 1:3){print(paste(\"i is\",i,\"and j is\",j))}}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "for (i in val)\n" +
            "{\n" +
            "    if (i == 8)\n" +
            "        next\n" +
            "    if(i == 5)\n" +
            "        break\n" +
            "}\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(i in val){if(i==8)next;if(i==5)break}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "for (i in 1:r) print(t(plots[,,i]))";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(i in 1:r)print(t(plots[,,i]))", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "\nfor (i in val)\n" +
            "{\n" +
            "    if (i == 8)\n" +
            "        next\n" +
            "    if(i == 5)\n" +
            "        break\n" +
            "}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(i in val){if(i==8)next;if(i==5)break}", (dctRStatements[0] as RStatement)?.TextNoFormatting);


        // key word snippets from https://github.com/africanmathsinitiative/R-Instat/pull/8707
        strInput = "dim(plots) <-c(k, s, r)\n" +
            "for (i in 1:k) for (j in 1:s)\n" +
            "outdesign$sketch\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("for(i in 1:k)for(j in 1:s)outdesign$sketch", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "npoints < -length(w)\n" +
            "for (i in 1:npoints)\n" +
            "{\n" +
            "  segments(w[i], Min[i], w[i], Max[i], lwd = 1.5, col = \"blue\")\n" +
            "}\n" +
            "legend(\"topleft\", c(\"Disease progress curves\", \"Weather-Severity\"),\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(3, dctRStatements.Count);
        Assert.Equal("for(i in 1:npoints){segments(w[i],Min[i],w[i],Max[i],lwd=1.5,col=\"blue\")}", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput =
        "function(" +
                 "fn1(a,fn2=function(b)" +
                                   "fn3(c/fn3(d)*e)" +
                    ")" +
                ")" +
                "f";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "a=function(b)c";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "for(a in b)\n" +
            "    while(c<d)\n" +
            "        repeat\n" +
            "            if(e=f)\n" +
            "                break\n" +
            "            else\n" +
            "                next\n";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("for(a in b)while(c<d) repeat if(e=f)break else next", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "evenOdd = function(x){\n" +
            "  if(x %% 2 == 0)\n" +
            "    return(\"even\")\n" +
            "  else\n" +
            "    return(\"odd\")\n" +
            "}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("evenOdd=function(x){" +
            "if(x%%2==0)" +
            "return(\"even\")" +
            " else " +
            "return(\"odd\")" +
            "}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "function(fn1(g,fn2=function(h)fn3(i/sum(j)*100))))";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput =
            "if (function(fn1(g,fn2=function(h)fn3(i/sum(j)*100))))" +
            "\n    return(k)";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(function(fn1(g,fn2=function(h)fn3(i/sum(j)*100))))" +
            "return(k)", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "while (val <= 5 )\n" +
            "{\n    # statements\n" +
            "    fn3(val)\n" +
            "    val = val + 1\n" +
            "}" +
            "\nrepeat\n" +
            "{\n" +
            "    if(val > 5) break\n" +
            "}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(2, dctRStatements.Count);
        Assert.Equal("while(val<=5)" +
            "{fn3(val);val=val+1" +
            "}", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal(" repeat " +
            "{" +
            "if(val>5)break" +
            "}", (dctRStatements[1] as RStatement)?.TextNoFormatting);

        strInput = "if(x > 10){\n" +
            "    fn1(paste(x, \"is greater than 10\"))}else\n" +
            "{\n" +
            "    fn2(paste(x, \"Is less than 10\"))\n" +
            "} while (val <= 5 )\n" +
            "{\n" +
            "    # statements\n" +
            "    fn3(val)\n" +
            "    val = val + 1\n" +
            "}repeat\n" +
            "{\n" +
            "    if(val > 5) break\n" +
            "}for (val in 1:5) {}\n" +
            "evenOdd = function(x){if(x %% 2 == 0)\n" +
            "    return(\"even\")\n" +
            "else    return(\"odd\")\n" +
            "}for (i in val)\n" +
            "{\n" +
            "    if (i == 8)        next\n" +
            "    if(i == 5)\n" +
            "        break\n" +
            "}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(6, dctRStatements.Count);
        Assert.Equal("if(x>10){fn1(paste(x,\"is greater than 10\"))} else {fn2(paste(x,\"Is less than 10\"))}", (dctRStatements[0] as RStatement)?.TextNoFormatting);
        Assert.Equal("while(val<=5){fn3(val);val=val+1}", (dctRStatements[1] as RStatement)?.TextNoFormatting);
        Assert.Equal(" repeat {if(val>5)break}", (dctRStatements[2] as RStatement)?.TextNoFormatting);
        Assert.Equal("for(val in 1:5){}", (dctRStatements[3] as RStatement)?.TextNoFormatting);
        Assert.Equal("evenOdd=function(x){if(x%%2==0)return(\"even\") else return(\"odd\")}", (dctRStatements[4] as RStatement)?.TextNoFormatting);
        Assert.Equal("for(i in val){if(i==8)next;if(i==5)break}", (dctRStatements[5] as RStatement)?.TextNoFormatting);

        strInput = "function(x, label = deparse(x)) {\nlabel\nx <- x + 1\nprint(label)\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("function(x,label=deparse(x)){label;x<-x+1;print(label)}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "y <- if( any(x <= 0) ) log(1+x) else log(x)";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("y<-if(any(x<=0))log(1+x) else log(x)", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        //todo See https://github.com/lloyddewit/RInsight/issues/12
        //strInput = "a/if(b)c else d+e";
        //strActual = new RScript(strInput).GetAsExecutableScript();
        //Assert.Equal(strInput, strActual);
        //dctRStatements = new RScript(strInput).statements;
        //Assert.Single(dctRStatements);
        //Assert.Equal(strInput, (dctRStatements[0] as RStatement)?.TextNoFormatting);


        // tidyverse operators used in https://github.com/IDEMSInternational/R-Instat/issues/8657
        strInput = "!!a";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("!!a", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(!is.null(station)){data<-data%>%group_by(!!sym(station))}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(!is.null(station)){data<-data%>%group_by(!!sym(station))}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "a:=b";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("a:=b", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "binds[[i]] <- results[[i]][[j]] %>% mutate(!!sym(station) := station_name[i])";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("binds[[i]]<-results[[i]][[j]]%>%mutate(!!sym(station):=station_name[i])", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "library(tidyverse) # I have most definitely missed package names in the functions so run this to be safe\r\n" +
            "library(climaemet) \r\n" +
            "\r\nprepare_walter_lieth <- function(data, month, tm_min, ta_min){\r\n  dat_long_int <- NULL\r\n  for (j in seq(nrow(data) - 1)) {\r\n    intres <- NULL\r\n    for (i in seq_len(ncol(data))) {\r\n      if (is.character(data[j, i])) {\r\n        val <- as.data.frame(data[j, i])\r\n      }\r\n      else {\r\n        interpol <- approx(x = data[c(j, j + 1), \"indrow\"],\r\n                           y = data[c(j, j + 1), i],\r\n                           n = 50)\r\n" +
                "val <- as.data.frame(interpol$y)\r\n" +
                "}\r\n      names(val) <- names(data)[i]\r\n      intres <- dplyr::bind_cols(intres, val)\r\n    }\r\n    dat_long_int <- dplyr::bind_rows(dat_long_int, intres)\r\n  }\r\n  dat_long_int$interpolate <- TRUE\r\n  dat_long_int[[month]] <- \"\"\r\n  data$interpolate <- FALSE\r\n  dat_long_int <- dat_long_int[!dat_long_int$indrow %in% data$indrow, ]\r\n  dat_long_end <- dplyr::bind_rows(data, dat_long_int)\r\n  dat_long_end <- dat_long_end[order(dat_long_end$indrow), ]\r\n  dat_long_end <- dat_long_end[dat_long_end$indrow >= 0 & dat_long_end$indrow <= 12, ]\r\n  dat_long_end <- tibble::as_tibble(dat_long_end)\r\n" +
                "\r\n  getpolymax <- function(x, y, y_lim) {\r\n    initpoly <- FALSE\r\n    yres <- NULL\r\n    xres <- NULL\r\n    for (i in seq_len(length(y))) {\r\n      lastobs <- i == length(x)\r\n      if (y[i] > y_lim[i]) {\r\n        if (isFALSE(initpoly)) {\r\n          xres <- c(xres, x[i])\r\n          yres <- c(yres, y_lim[i])\r\n          initpoly <- TRUE\r\n        }\r\n        xres <- c(xres, x[i])\r\n        yres <- c(yres, y[i])\r\n        if (lastobs) {\r\n          xres <- c(xres, x[i], NA)\r\n          yres <- c(yres, y_lim[i], NA)\r\n        }\r\n      }\r\n      else {\r\n        if (initpoly) {\r\n          xres <- c(xres, x[i - 1], NA)\r\n          " +
                "yres <- c(yres, y_lim[i - 1], NA)\r\n" +
                "initpoly <- FALSE\r\n        }\r\n" +
                "}\r\n    }\r\n    poly <- tibble::tibble(x = xres, y = yres)\r\n    return(poly)\r\n  }\r\n" +
                "getlines <- function(x, y, y_lim) {\r\n    yres <- NULL\r\n    xres <- NULL\r\n    ylim_res <- NULL\r\n    for (i in seq_len(length(y))) {\r\n      if (y[i] > y_lim[i]) {\r\n        xres <- c(xres, x[i])\r\n        yres <- c(yres, y[i])\r\n        ylim_res <- c(ylim_res, y_lim[i])\r\n      }\r\n    }\r\n    line <- tibble::tibble(x = xres, y = yres, ylim_res = ylim_res)\r\n    return(line)\r\n  }\r\n  prep_max_poly <- getpolymax(x = dat_long_end$indrow, y = pmax(dat_long_end$pm_reesc, \r\n                                                                50), y_lim = rep(50, length(dat_long_end$indrow)))\r\n  tm_max_line <- getlines(x = dat_long_end$indrow, y = dat_long_end$tm, \r\n                          y_lim = dat_long_end$pm_reesc)\r\n  pm_max_line <- getlines(x = dat_long_end$indrow, y = pmin(dat_long_end$pm_reesc, \r\n                                                            50), y_lim = dat_long_end$tm)\r\n  dat_real <- dat_long_end[dat_long_end$interpolate == FALSE, \r\n                           c(\"indrow\", ta_min)]\r\n  x <- NULL\r\n  y <- NULL\r\n  for (i in seq_len(nrow(dat_real))) {\r\n    if (dat_real[i, ][[ta_min]] < 0) {\r\n      x <- c(x, NA, rep(dat_real[i, ]$indrow - 0.5, 2), \r\n             rep(dat_real[i, ]$indrow + 0.5, 2), NA)\r\n      y <- c(y, NA, -3, 0, 0, -3, NA)\r\n    }\r\n    else {\r\n      x <- c(x, NA)\r\n      " +
                "y <- c(y, NA)\r\n" +
                "}\r\n  }\r\n  probfreeze <- tibble::tibble(x = x, y = y)\r\n  rm(dat_real)\r\n  dat_real <- dat_long_end[dat_long_end$interpolate == FALSE, \r\n                           c(\"indrow\", tm_min)]\r\n  x <- NULL\r\n  y <- NULL\r\n  for (i in seq_len(nrow(dat_real))) {\r\n    if (dat_real[i, ][[tm_min]] < 0) {\r\n      x <- c(x, NA, rep(dat_real[i, ]$indrow - 0.5, 2), \r\n             rep(dat_real[i, ]$indrow + 0.5, 2), NA)\r\n      y <- c(y, NA, -3, 0, 0, -3, NA)\r\n    }\r\n    else {\r\n      x <- c(x, NA)\r\n      " +
                "y <- c(y, NA)\r\n" +
                "}\r\n  }\r\n  surefreeze <- tibble::tibble(x = x, y = y)\r\n  return_list <- list(dat_long_end,\r\n                      tm_max_line,\r\n                      pm_max_line,\r\n                      prep_max_poly,\r\n                      probfreeze,\r\n                      surefreeze)\r\n  names(return_list) <- c(\"dat_long_end\", \"tm_max_line\", \"pm_max_line\",\r\n                          \"prep_max_poly\", \"prob_freeze\", \"surefreeze\")\r\n  return(return_list)\r\n}\r\n" +
            "\r\nggwalter_lieth <- function (data, month, station, p_mes, tm_max, tm_min, ta_min, station_name = \"\", \r\n                            alt = NA, per = NA, pcol = \"#002F70\", \r\n                            tcol = \"#ff0000\", pfcol = \"#9BAEE2\", sfcol = \"#3C6FC4\", \r\n                            shem = FALSE, p3line = FALSE, ...) \r\n{\r\n  \r\n  # Preprocess data with vectorised operations\r\n  data <- data %>%\r\n    dplyr::mutate(tm = (.data[[tm_max]] + .data[[tm_min]]) / 2,\r\n                  pm_reesc = dplyr::if_else(.data[[p_mes]] < 100, .data[[p_mes]] * 0.5, .data[[p_mes]] * 0.05 + 45),\r\n                  p3line = .data[[p_mes]] / 3) %>%\r\n    dplyr::mutate(across(.data[[month]], ~ fct_expand(.data[[month]], \"\"))) %>%\r\n    dplyr::arrange(.data[[month]])\r\n  \r\n  # do this for each station, if we have a station\r\n  if (!is.null(station)){\r\n    data <- data %>% group_by(!!sym(station))\r\n  }\r\n  data <- data %>%\r\n    group_modify(~{\r\n      # Add dummy rows at the beginning and end for each group\r\n      .x <- bind_rows(.x[nrow(.x), , drop = FALSE], .x, .x[1, , drop = FALSE])\r\n      # Clear month value for the dummy rows\r\n      .x[c(1, nrow(.x)), which(names(.x) == data[[month]])] <- \"\"\r\n      # Add an index column for plotting or further transformations\r\n      .x <- cbind(indrow = seq(-0.5, 12.5, 1), .x)\r\n      .x\r\n    })\r\n  \r\n  if (!is.null(station)){\r\n    data <- data %>% ungroup()\r\n  }\r\n  data <- data.frame(data)\r\n  \r\n  # split by station\r\n  if (is.null(station)){\r\n    data_list <- prepare_walter_lieth(data, month, tm_min, ta_min)\r\n    # data things\r\n    dat_long_end <- data_list$dat_long_end\r\n    tm_max_line <- data_list$tm_max_line\r\n    pm_max_line <- data_list$pm_max_line\r\n    prep_max_poly <- data_list$prep_max_poly\r\n    probfreeze <- data_list$prob_freeze\r\n    surefreeze <- data_list$surefreeze\r\n  } else {\r\n    results <-\r\n      map(.x = unique(data[[station]]),\r\n          .f = ~{filtered_data <- data %>% filter(!!sym(station) == .x)\r\n          prepare_walter_lieth(filtered_data, month, tm_min, ta_min)})\r\n    # Function to bind rows for a specific sub-element across all main elements\r\n    n <- length(results)\r\n    m <- length(results[[1]])\r\n    station_name <- unique(data[[station]])\r\n    binds <- NULL\r\n    combined <- NULL\r\n    for (j in 1:m){\r\n      for (i in 1:n) { # for each station data set\r\n        binds[[i]] <- results[[i]][[j]] %>% mutate(!!sym(station) := station_name[i])\r\n      }\r\n      combined[[j]] <- do.call(rbind, binds) # Combine all the sub-elements row-wise\r\n    }\r\n    # data things\r\n    dat_long_end <- combined[[1]]\r\n    tm_max_line <- combined[[2]]\r\n    pm_max_line <- combined[[3]]\r\n    prep_max_poly <- combined[[4]]\r\n    probfreeze <- combined[[5]]\r\n    surefreeze <- combined[[6]]\r\n  }\r\n  \r\n  # data frame pretty things ------------------------------------------------------\r\n  ticks <- data.frame(x = seq(0, 12), ymin = -3, ymax = 0)\r\n  title <- station_name\r\n  if (!is.na(alt)) {\r\n    title <- paste0(title, \" (\", prettyNum(alt, big.mark = \",\", \r\n                                           decimal.mark = \".\"), \" m)\")\r\n  }\r\n  if (!is.na(per)) {\r\n    title <- paste0(title, \"\\n\", per)\r\n  }\r\n  sub <- paste(round(mean(dat_long_end[dat_long_end$interpolate == FALSE, ]$tm), 1),\r\n               \"C        \",\r\n               prettyNum(round(sum(dat_long_end[dat_long_end$interpolate == FALSE, ][[p_mes]])), big.mark = \",\"), \" mm\", sep = \"\")\r\n  \r\n  maxtm <- prettyNum(round(max(dat_long_end[[tm_max]]), 1))\r\n  mintm <- prettyNum(round(min(dat_long_end[[tm_min]]), 1))\r\n  tags <- paste0(paste0(rep(\" \\n\", 6), collapse = \"\"), maxtm, \r\n                 paste0(rep(\" \\n\", 10), collapse = \"\"), mintm)\r\n  month_breaks <- dat_long_end[dat_long_end[[month]] != \"\", ]$indrow\r\n  month_labs <- dat_long_end[dat_long_end[[month]] != \"\", ][[month]]\r\n  \r\n  ymax <- max(60, 10 * floor(max(dat_long_end$pm_reesc)/10) + 10)\r\n  ymin <- min(-3, min(dat_long_end$tm))\r\n  range_tm <- seq(0, ymax, 10)\r\n  if (ymin < -3) {\r\n    ymin <- floor(ymin/10) * 10\r\n    range_tm <- seq(ymin, ymax, 10)\r\n  }\r\n  templabs <- paste0(range_tm)\r\n  templabs[range_tm > 50] <- \"\"\r\n  range_prec <- range_tm * 2\r\n  range_prec[range_tm > 50] <- range_tm[range_tm > 50] * 20 - 900\r\n  preclabs <- paste0(range_prec)\r\n  preclabs[range_tm < 0] <- \"\"\r\n  \r\n  wandlplot <- ggplot2::ggplot() + ggplot2::geom_line(data = dat_long_end, \r\n                                                      aes(x = .data$indrow, y = .data$pm_reesc), color = pcol) + \r\n    ggplot2::geom_line(data = dat_long_end, aes(x = .data$indrow, \r\n                                                y = .data$tm), color = tcol)\r\n  if (nrow(tm_max_line > 0)) {\r\n    wandlplot <- wandlplot + ggplot2::geom_segment(aes(x = .data$x, \r\n                                                       y = .data$ylim_res, xend = .data$x, yend = .data$y), \r\n                                                   data = tm_max_line, color = tcol, alpha = 0.2)\r\n  }\r\n  if (nrow(pm_max_line > 0)) {\r\n    wandlplot <- wandlplot + ggplot2::geom_segment(aes(x = .data$x, \r\n                                                       y = .data$ylim_res, xend = .data$x, yend = .data$y), \r\n                                                   data = pm_max_line, color = pcol, alpha = 0.2)\r\n  }\r\n  if (p3line) {\r\n    wandlplot <- wandlplot + ggplot2::geom_line(data = dat_long_end, \r\n                                                aes(x = .data$indrow, y = .data$p3line), color = pcol)\r\n  }\r\n  if (max(dat_long_end$pm_reesc) > 50) {\r\n    wandlplot <- wandlplot + ggplot2::geom_polygon(data = prep_max_poly, aes(x, y),\r\n                                                   fill = pcol)\r\n  }\r\n  if (min(dat_long_end[[ta_min]]) < 0) {\r\n    wandlplot <- wandlplot + ggplot2::geom_polygon(data = probfreeze, aes(x = x, y = y),\r\n                                                   fill = pfcol, colour = \"black\")\r\n  }\r\n  if (min(dat_long_end[[tm_min]]) < 0) {\r\n    wandlplot <- wandlplot + geom_polygon(data = surefreeze, aes(x = x, y = y),\r\n                                          fill = sfcol, colour = \"black\")\r\n  }\r\n  wandlplot <- wandlplot + geom_hline(yintercept = c(0, 50), \r\n                                      size = 0.5) +\r\n    geom_segment(data = ticks, aes(x = x, xend = x, y = ymin, yend = ymax)) +\r\n    scale_x_continuous(breaks = month_breaks, name = \"\", labels = month_labs, expand = c(0, 0)) + \r\n    scale_y_continuous(\"C\", limits = c(ymin, ymax), labels = templabs, \r\n                       breaks = range_tm, sec.axis = dup_axis(name = \"mm\", labels = preclabs))\r\n  wandlplot <- wandlplot +\r\n    ggplot2::labs(title = title, subtitle = sub, tag = tags) +\r\n    ggplot2::theme_classic() +\r\n    ggplot2::theme(plot.title = element_text(lineheight = 1, size = 14, face = \"bold\"),\r\n                   plot.subtitle = element_text(hjust = 1, vjust = 1, size = 14),\r\n                   plot.tag = element_text(size = 10), \r\n                   plot.tag.position = \"left\", axis.ticks.length.x.bottom = unit(0, \"pt\"), \r\n                   axis.line.x.bottom = element_blank(), \r\n                   axis.title.y.left = element_text(angle = 0, \r\n                                                    vjust = 0.9, size = 10, colour = tcol,\r\n                                                    margin = unit(rep(10, 4), \"pt\")),\r\n                   axis.text.x.bottom = element_text(size = 10), \r\n                   axis.text.y.left = element_text(colour = tcol, size = 10), \r\n                   axis.title.y.right = element_text(angle = 0, vjust = 0.9, \r\n                                                     size = 10, colour = pcol,\r\n                                                     margin = unit(rep(10, 4), \"pt\")),\r\n                   axis.text.y.right = element_text(colour = pcol, size = 10))\r\n  \r\n  if (!is.null(station)){\r\n    wandlplot <- wandlplot + facet_wrap(station)\r\n  }\r\n  \r\n  return(wandlplot)\r\n}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Equal(4, dctRStatements.Count);
        Assert.Equal("prepare_walter_lieth<-function(data,month,tm_min,ta_min){dat_long_int<-NULL;for(j in seq(nrow(data)-1)){intres<-NULL;for(i in seq_len(ncol(data))){if(is.character(data[j,i])){val<-as.data.frame(data[j,i])} else {interpol<-approx(x=data[c(j,j+1),\"indrow\"],y=data[c(j,j+1),i],n=50);val<-as.data.frame(interpol$y)};names(val)<-names(data)[i];intres<-dplyr::bind_cols(intres,val)};dat_long_int<-dplyr::bind_rows(dat_long_int,intres)};dat_long_int$interpolate<-TRUE;dat_long_int[[month]]<-\"\";data$interpolate<-FALSE;dat_long_int<-dat_long_int[!dat_long_int$indrow%in%data$indrow,];dat_long_end<-dplyr::bind_rows(data,dat_long_int);dat_long_end<-dat_long_end[order(dat_long_end$indrow),];dat_long_end<-dat_long_end[dat_long_end$indrow>=0&dat_long_end$indrow<=12,];dat_long_end<-tibble::as_tibble(dat_long_end);"+
                "getpolymax<-function(x,y,y_lim){initpoly<-FALSE;yres<-NULL;xres<-NULL;for(i in seq_len(length(y))){lastobs<-i==length(x);if(y[i]>y_lim[i]){if(isFALSE(initpoly)){xres<-c(xres,x[i]);yres<-c(yres,y_lim[i]);initpoly<-TRUE};xres<-c(xres,x[i]);yres<-c(yres,y[i]);if(lastobs){xres<-c(xres,x[i],NA);yres<-c(yres,y_lim[i],NA)}} else {if(initpoly){xres<-c(xres,x[i-1],NA);yres<-c(yres,y_lim[i-1],NA);initpoly<-FALSE}}};poly<-tibble::tibble(x=xres,y=yres);return(poly)};"+
                "getlines<-function(x,y,y_lim){yres<-NULL;xres<-NULL;ylim_res<-NULL;for(i in seq_len(length(y))){if(y[i]>y_lim[i]){xres<-c(xres,x[i]);yres<-c(yres,y[i]);ylim_res<-c(ylim_res,y_lim[i])}};line<-tibble::tibble(x=xres,y=yres,ylim_res=ylim_res);return(line)};prep_max_poly<-getpolymax(x=dat_long_end$indrow,y=pmax(dat_long_end$pm_reesc,50),y_lim=rep(50,length(dat_long_end$indrow)));tm_max_line<-getlines(x=dat_long_end$indrow,y=dat_long_end$tm,y_lim=dat_long_end$pm_reesc);pm_max_line<-getlines(x=dat_long_end$indrow,y=pmin(dat_long_end$pm_reesc,50),y_lim=dat_long_end$tm);dat_real<-dat_long_end[dat_long_end$interpolate==FALSE,c(\"indrow\",ta_min)];x<-NULL;y<-NULL;for(i in seq_len(nrow(dat_real))){if(dat_real[i,][[ta_min]]<0){x<-c(x,NA,rep(dat_real[i,]$indrow-0.5,2),rep(dat_real[i,]$indrow+0.5,2),NA);y<-c(y,NA,-3,0,0,-3,NA)} else {x<-c(x,NA);y<-c(y,NA)}};probfreeze<-tibble::tibble(x=x,y=y);rm(dat_real);dat_real<-dat_long_end[dat_long_end$interpolate==FALSE,c(\"indrow\",tm_min)];x<-NULL;y<-NULL;for(i in seq_len(nrow(dat_real))){if(dat_real[i,][[tm_min]]<0){x<-c(x,NA,rep(dat_real[i,]$indrow-0.5,2),rep(dat_real[i,]$indrow+0.5,2),NA);y<-c(y,NA,-3,0,0,-3,NA)} else {x<-c(x,NA);y<-c(y,NA)}};surefreeze<-tibble::tibble(x=x,y=y);return_list<-list(dat_long_end,tm_max_line,pm_max_line,prep_max_poly,probfreeze,surefreeze);names(return_list)<-c(\"dat_long_end\",\"tm_max_line\",\"pm_max_line\",\"prep_max_poly\",\"prob_freeze\",\"surefreeze\");return(return_list)}",
            (dctRStatements[2] as RStatement)?.TextNoFormatting);

        strInput = "if(a){b\nc\nif(d){e\nf\nif(g){h\nk\nl}m}\nn\no}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a){b;c;if(d){e;f;if(g){h;k;l}m};n;o}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(a){b\nc\nif(d){e\nf\nif(g){h\nk\nl}m}\nn\no}else{p\nq}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a){b;c;if(d){e;f;if(g){h;k;l}m};n;o} else {p;q}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(a){b<-c+d\ne<-f+g}else{h<-i+j\nk<-l+m}";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(a){b<-c+d;e<-f+g} else {h<-i+j;k<-l+m}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

        strInput = "if(i == 1) {\r\n    tmp_prev <- tmp_prev\r\n    tmp <- cnt[i]\r\n    \r\n  } else {\r\n    tmp_prev <- tmp_prev + cnt[i-1]  \r\n    tmp <- tmp + cnt[i]\r\n  }";
        strActual = new RScript(strInput).GetAsExecutableScript();
        Assert.Equal(strInput, strActual);
        dctRStatements = new RScript(strInput).statements;
        Assert.Single(dctRStatements);
        Assert.Equal("if(i==1){tmp_prev<-tmp_prev;tmp<-cnt[i]} else {tmp_prev<-tmp_prev+cnt[i-1];tmp<-tmp+cnt[i]}", (dctRStatements[0] as RStatement)?.TextNoFormatting);

    }
}