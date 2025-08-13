using System;
using System.Linq;
using QuizForge.Infrastructure.Parsers;
using Xunit;

namespace QuizForge.Tests.Parsers;

/// <summary>
/// LaTeX解析器测试类
/// </summary>
public class LatexParserTests
{
    private readonly LatexParser _parser;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LatexParserTests()
    {
        _parser = new LatexParser();
    }

    /// <summary>
    /// 测试解析基本LaTeX文档
    /// </summary>
    [Fact]
    public void Parse_ShouldParseBasicLatexDocument()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\title{测试文档}
\author{测试作者}
\date{\today}
\begin{document}
\maketitle
\section{引言}
这是一个测试文档。

\section{结论}
这是文档的结论部分。
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Equal("article", document.DocumentClass);
        Assert.Contains("ctex", document.Packages);
        Assert.Equal(2, document.Sections.Count);
        Assert.Equal("引言", document.Sections[0].Title);
        Assert.Equal("结论", document.Sections[1].Title);
    }

    /// <summary>
    /// 测试解析LaTeX文档中的数学公式
    /// </summary>
    [Fact]
    public void Parse_ShouldParseMathFormulas()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\usepackage{amsmath}
\begin{document}
行内公式：$E = mc^2$

行间公式：
\[
\int_{a}^{b} f(x) dx = F(b) - F(a)
\]

复杂公式：
\[
\frac{\partial^2 u}{\partial t^2} = c^2 \nabla^2 u
\]
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Equal(3, document.MathElements.Count);
        Assert.Contains("$E = mc^2$", document.MathElements.Select(m => m.Content));
        Assert.Contains(@"\[
\int_{a}^{b} f(x) dx = F(b) - F(a)
\]", document.MathElements.Select(m => m.Content));
        Assert.Contains(@"\[
\frac{\partial^2 u}{\partial t^2} = c^2 \nabla^2 u
\]", document.MathElements.Select(m => m.Content));
    }

    /// <summary>
    /// 测试解析LaTeX文档中的表格
    /// </summary>
    [Fact]
    public void Parse_ShouldParseTables()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\begin{document}
\begin{table}[h]
\centering
\caption{测试表格}
\begin{tabular}{|c|c|c|}
\hline
姓名 & 年龄 & 职业 \\
\hline
张三 & 25 & 工程师 \\
\hline
李四 & 30 & 教师 \\
\hline
\end{tabular}
\end{table}
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Single(document.Tables);
        Assert.Equal("测试表格", document.Tables[0].Caption);
        Assert.Equal(3, document.Tables[0].Columns);
        Assert.Equal(2, document.Tables[0].Rows);
        Assert.Equal("姓名", document.Tables[0].Cells[0, 0]);
        Assert.Equal("教师", document.Tables[0].Cells[1, 2]);
    }

    /// <summary>
    /// 测试解析LaTeX文档中的列表
    /// </summary>
    [Fact]
    public void Parse_ShouldParseLists()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\begin{document}
无序列表：
\begin{itemize}
\item 第一项
\item 第二项
\item 第三项
\end{itemize}

有序列表：
\begin{enumerate}
\item 第一步
\item 第二步
\item 第三步
\end{enumerate}
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Equal(2, document.Lists.Count);
        
        var unorderedList = document.Lists.FirstOrDefault(l => l.Type == "itemize");
        Assert.NotNull(unorderedList);
        Assert.Equal(3, unorderedList.Items.Count);
        Assert.Equal("第一项", unorderedList.Items[0]);
        
        var orderedList = document.Lists.FirstOrDefault(l => l.Type == "enumerate");
        Assert.NotNull(orderedList);
        Assert.Equal(3, orderedList.Items.Count);
        Assert.Equal("第一步", orderedList.Items[0]);
    }

    /// <summary>
    /// 测试解析LaTeX文档中的文本格式
    /// </summary>
    [Fact]
    public void Parse_ShouldParseTextFormatting()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\begin{document}
这是\textbf{粗体文本}，这是\textit{斜体文本}，
这是\underline{下划线文本}，这是\texttt{等宽文本}。
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Equal(4, document.TextFormatting.Count);
        Assert.Contains("粗体文本", document.TextFormatting.Select(t => t.Text));
        Assert.Contains("斜体文本", document.TextFormatting.Select(t => t.Text));
        Assert.Contains("下划线文本", document.TextFormatting.Select(t => t.Text));
        Assert.Contains("等宽文本", document.TextFormatting.Select(t => t.Text));
    }

    /// <summary>
    /// 测试解析空LaTeX文档
    /// </summary>
    [Fact]
    public void Parse_ShouldHandleEmptyDocument()
    {
        // 准备
        var latexContent = "";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Empty(document.Sections);
        Assert.Empty(document.MathElements);
        Assert.Empty(document.Tables);
        Assert.Empty(document.Lists);
        Assert.Empty(document.TextFormatting);
    }

    /// <summary>
    /// 测试解析无效LaTeX文档
    /// </summary>
    [Fact]
    public void Parse_ShouldHandleInvalidDocument()
    {
        // 准备
        var latexContent = "这不是一个有效的LaTeX文档";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Empty(document.Sections);
        Assert.Empty(document.MathElements);
        Assert.Empty(document.Tables);
        Assert.Empty(document.Lists);
        Assert.Empty(document.TextFormatting);
    }

    /// <summary>
    /// 测试解析中文LaTeX文档
    /// </summary>
    [Fact]
    public void Parse_ShouldHandleChineseContent()
    {
        // 准备
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\begin{document}
\section{中文测试}
这是一个包含中文的测试文档。

\subsection{数学公式}
中文与数学公式混合：$E = mc^2$

\subsection{列表}
\begin{itemize}
\item 中文第一项
\item 中文第二项
\end{itemize}
\end{document}";

        // 执行
        var document = _parser.Parse(latexContent);

        // 断言
        Assert.NotNull(document);
        Assert.Equal(2, document.Sections.Count);
        Assert.Equal("中文测试", document.Sections[0].Title);
        Assert.Equal("数学公式", document.Sections[1].Title);
        Assert.Contains("这是一个包含中文的测试文档", document.Sections[0].Content);
        Assert.Contains("中文与数学公式混合", document.Sections[1].Content);
        Assert.Single(document.MathElements);
        Assert.Contains("$E = mc^2$", document.MathElements.Select(m => m.Content));
        Assert.Single(document.Lists);
        Assert.Equal(2, document.Lists[0].Items.Count);
        Assert.Contains("中文第一项", document.Lists[0].Items);
    }
}