using QuizForge.Models;
using QuizForge.Core.Layout;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// LaTeX模板处理器，负责将模板与动态内容结合
/// </summary>
public class LaTeXTemplateProcessor
{
    private readonly SealLineLayout _sealLineLayout;
    private readonly HeaderLayout _headerLayout;
    private readonly HeaderFooterLayout _headerFooterLayout;

    /// <summary>
    /// LaTeX模板处理器构造函数
    /// </summary>
    public LaTeXTemplateProcessor()
    {
        _sealLineLayout = new SealLineLayout();
        _headerLayout = new HeaderLayout();
        _headerFooterLayout = new HeaderFooterLayout();
    }

    /// <summary>
    /// 处理LaTeX模板，替换所有占位符
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questionBank">题库</param>
    /// <param name="headerConfig">抬头配置</param>
    /// <param name="pageNumber">当前页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>处理后的LaTeX内容</returns>
    public string ProcessTemplate(ExamTemplate template, QuestionBank questionBank, HeaderConfig headerConfig, int pageNumber = 1, int totalPages = 1)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        if (questionBank == null)
            throw new ArgumentNullException(nameof(questionBank));
        if (headerConfig == null)
            throw new ArgumentNullException(nameof(headerConfig));

        // 获取模板内容
        var templateContent = GetTemplateContent(template);
        
        // 生成密封线命令定义
        var sealLineCommands = _sealLineLayout.GenerateSealLineCommandDefinition();
        
        // 生成密封线内容
        var sealLineContent = _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages);
        
        // 生成抬头部分
        var headerSection = _headerLayout.GenerateHeader(headerConfig, pageNumber);
        
        // 生成页眉页脚设置
        var headerFooterSetup = _headerFooterLayout.GenerateHeaderFooterSetup(template, headerConfig);
        
        // 生成页眉内容
        var headerContent = _headerFooterLayout.GenerateHeaderContent(template, headerConfig);
        
        // 生成页脚内容
        var footerContent = _headerFooterLayout.GenerateFooterContent(template, headerConfig);
        
        // 生成题目内容
        var content = GenerateQuestionsContent(questionBank);
        
        // 生成答题卡命令和内容
        var answerSheetCommands = GenerateAnswerSheetCommands();
        var answerSheetContent = GenerateAnswerSheetContent(questionBank);
        
        // 替换所有占位符
        var processedContent = templateContent
            .Replace("{SEAL_LINE_COMMANDS}", sealLineCommands)
            .Replace("{ANSWER_SHEET_COMMANDS}", answerSheetCommands)
            .Replace("{HEADER_FOOTER_SETUP}", headerFooterSetup)
            .Replace("{HEADER_CONTENT}", headerContent)
            .Replace("{FOOTER_CONTENT}", footerContent)
            .Replace("{HEADER_SECTION}", headerSection)
            .Replace("{SEAL_LINE_CONTENT}", sealLineContent)
            .Replace("{CONTENT}", content)
            .Replace("{ANSWER_SHEET_CONTENT}", answerSheetContent);
        
        return processedContent;
    }

    /// <summary>
    /// 获取模板内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <returns>模板内容</returns>
    private string GetTemplateContent(ExamTemplate template)
    {
        // 根据模板样式获取不同的模板内容
        switch (template.Style)
        {
            case TemplateStyle.Basic:
                return GetBasicTemplate();
            case TemplateStyle.Advanced:
                return GetAdvancedTemplate();
            case TemplateStyle.Custom:
                return template.HeaderContent ?? GetAdvancedTemplate();
            default:
                return GetAdvancedTemplate();
        }
    }

    /// <summary>
    /// 获取基础模板内容
    /// </summary>
    /// <returns>基础模板内容</returns>
    private string GetBasicTemplate()
    {
        return @"\documentclass{article}
\usepackage{geometry}
\usepackage{fancyhdr}
\usepackage{ctex}
\usepackage{lastpage}
\usepackage{tikz}
\usepackage{ifthen}
\usepackage{multicol}
\usepackage{array}
\usepackage{booktabs}

% 页面设置
\geometry{a4paper, margin=1in}

% 密封线命令（由SealLineLayout类动态生成）
{SEAL_LINE_COMMANDS}

% 答题卡命令（由SealLineLayout类动态生成）
{ANSWER_SHEET_COMMANDS}

% 页眉页脚设置（由HeaderFooterLayout类动态生成）
{HEADER_FOOTER_SETUP}

% 页眉内容（由HeaderFooterLayout类动态生成）
{HEADER_CONTENT}

% 页脚内容（由HeaderFooterLayout类动态生成）
{FOOTER_CONTENT}

\begin{document}

% 抬头部分（由HeaderLayout类动态生成）
{HEADER_SECTION}

% 密封线（由SealLineLayout类动态生成）
{SEAL_LINE_CONTENT}

\vspace{1cm}

% 题目内容
{CONTENT}

\end{document}";
    }

    /// <summary>
    /// 获取高级模板内容
    /// </summary>
    /// <returns>高级模板内容</returns>
    private string GetAdvancedTemplate()
    {
        return @"\documentclass{article}
\usepackage{geometry}
\usepackage{fancyhdr}
\usepackage{ctex}
\usepackage{lastpage}
\usepackage{tikz}
\usepackage{ifthen}
\usepackage{multicol}
\usepackage{array}
\usepackage{booktabs}
\usepackage{amsmath}
\usepackage{amssymb}
\usepackage{graphicx}
\usepackage{float}

% 页面设置
\geometry{a4paper, margin=1in}

% 密封线命令（由SealLineLayout类动态生成）
{SEAL_LINE_COMMANDS}

% 答题卡命令（由SealLineLayout类动态生成）
{ANSWER_SHEET_COMMANDS}

% 页眉页脚设置（由HeaderFooterLayout类动态生成）
{HEADER_FOOTER_SETUP}

% 页眉内容（由HeaderFooterLayout类动态生成）
{HEADER_CONTENT}

% 页脚内容（由HeaderFooterLayout类动态生成）
{FOOTER_CONTENT}

\begin{document}

% 抬头部分（由HeaderLayout类动态生成）
{HEADER_SECTION}

% 密封线（由SealLineLayout类动态生成）
{SEAL_LINE_CONTENT}

\vspace{1cm}

% 题目内容
{CONTENT}

% 新页面用于答题卡
\newpage

% 答题卡
\answersheet{
  {ANSWER_SHEET_CONTENT}
}

\end{document}";
    }

    /// <summary>
    /// 生成题目内容
    /// </summary>
    /// <param name="questionBank">题库</param>
    /// <returns>题目内容的LaTeX代码</returns>
    private string GenerateQuestionsContent(QuestionBank questionBank)
    {
        if (questionBank?.Questions == null || questionBank.Questions.Count == 0)
        {
            return "\\textbf{无题目内容}";
        }

        var contentBuilder = new StringBuilder();
        
        // 按题型分组
        var questionsByType = questionBank.Questions.GroupBy(q => q.Type).ToList();
        
        foreach (var group in questionsByType)
        {
            // 添加题型标题
            contentBuilder.AppendLine($"\\subsection*{{{EscapeLaTeX(group.Key)}}}");
            contentBuilder.AppendLine();
            
            // 生成该题型下的所有题目
            var questionNumber = 1;
            foreach (var question in group)
            {
                var questionContent = GenerateQuestionContent(question, questionNumber);
                contentBuilder.AppendLine(questionContent);
                questionNumber++;
            }
        }
        
        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成单个题目内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>题目内容的LaTeX代码</returns>
    private string GenerateQuestionContent(Question question, int questionNumber)
    {
        var contentBuilder = new StringBuilder();
        
        // 题目编号和内容
        contentBuilder.AppendLine($"{questionNumber}. {EscapeLaTeX(question.Content)}");
        
        // 如果是选择题，添加选项
        if (question.Type == "选择题" || question.Type == "MultipleChoice" || question.Type == "SingleChoice")
        {
            contentBuilder.AppendLine("\\begin{enumerate}[A.]");
            
            if (question.Options != null && question.Options.Count > 0)
            {
                foreach (var option in question.Options)
                {
                    contentBuilder.AppendLine($"\\item {EscapeLaTeX(option.Value)}");
                }
            }
            
            contentBuilder.AppendLine("\\end{enumerate}");
        }
        
        // 添加空行用于答题
        contentBuilder.AppendLine("\\vspace{0.5cm}");
        
        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成答题卡命令
    /// </summary>
    /// <returns>答题卡命令的LaTeX代码</returns>
    private string GenerateAnswerSheetCommands()
    {
        return @"% 答题卡命令
\newcommand{\answersheet}[1]{
  \begin{center}
    \textbf{答题卡}
  \end{center}
  \vspace{0.5cm}
  #1
}";
    }

    /// <summary>
    /// 生成答题卡内容
    /// </summary>
    /// <param name="questionBank">题库</param>
    /// <returns>答题卡内容的LaTeX代码</returns>
    private string GenerateAnswerSheetContent(QuestionBank questionBank)
    {
        if (questionBank?.Questions == null || questionBank.Questions.Count == 0)
        {
            return "\\textbf{无题目内容}";
        }

        var contentBuilder = new StringBuilder();
        
        // 按题型分组
        var questionsByType = questionBank.Questions.GroupBy(q => q.Type).ToList();
        
        foreach (var group in questionsByType)
        {
            // 添加题型标题
            contentBuilder.AppendLine($"\\textbf{{{EscapeLaTeX(group.Key)}}}");
            contentBuilder.AppendLine();
            
            // 生成该题型下的所有题目答题区
            var questionNumber = 1;
            foreach (var question in group)
            {
                contentBuilder.AppendLine($"{questionNumber}. \\underline{{\\hspace{{8cm}}}}");
                contentBuilder.AppendLine("\\vspace{0.3cm}");
                questionNumber++;
            }
        }
        
        return contentBuilder.ToString();
    }

    /// <summary>
    /// 转义LaTeX特殊字符
    /// </summary>
    /// <param name="text">要转义的文本</param>
    /// <returns>转义后的文本</returns>
    private string EscapeLaTeX(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // LaTeX特殊字符转义
        var escapedText = text
            .Replace("\\", "\\textbackslash ")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("#", "\\#")
            .Replace("$", "\\$")
            .Replace("%", "\\%")
            .Replace("&", "\\&")
            .Replace("_", "\\_")
            .Replace("^", "\\textasciicircum ")
            .Replace("~", "\\textasciitilde ")
            .Replace("<", "$<$")
            .Replace(">", "$>$");

        return escapedText;
    }
}