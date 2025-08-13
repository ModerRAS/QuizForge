using QuizForge.Models;
using QuizForge.Core.Layout;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// LaTeX文档生成器，用于将题库内容转换为完整的LaTeX文档
/// </summary>
public class LaTeXDocumentGenerator
{
    private readonly LaTeXGenerationConfig _config;
    private readonly ContentGenerator _contentGenerator;
    private readonly HeaderLayout _headerLayout;

    /// <summary>
    /// LaTeX文档生成器构造函数
    /// </summary>
    /// <param name="config">LaTeX生成配置</param>
    /// <param name="contentGenerator">内容生成器</param>
    public LaTeXDocumentGenerator(LaTeXGenerationConfig config, ContentGenerator contentGenerator)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _contentGenerator = contentGenerator ?? throw new ArgumentNullException(nameof(contentGenerator));
        _headerLayout = new HeaderLayout();
    }

    /// <summary>
    /// 生成完整的LaTeX文档（使用新的HeaderConfig）
    /// </summary>
    /// <param name="questionBank">题库</param>
    /// <param name="headerConfig">抬头配置</param>
    /// <returns>完整的LaTeX文档内容</returns>
    public string GenerateLaTeXDocument(QuestionBank questionBank, HeaderConfig headerConfig)
    {
        if (questionBank == null)
        {
            throw new ArgumentNullException(nameof(questionBank));
        }

        if (headerConfig == null)
        {
            throw new ArgumentNullException(nameof(headerConfig));
        }

        try
        {
            var documentBuilder = new StringBuilder();

            // 生成文档头部
            GenerateDocumentHeader(documentBuilder, headerConfig);

            // 生成文档内容
            GenerateDocumentContent(documentBuilder, questionBank);

            // 生成文档尾部
            GenerateDocumentFooter(documentBuilder);

            return documentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("生成LaTeX文档失败", ex);
        }
    }

    /// <summary>
    /// 生成完整的LaTeX文档
    /// </summary>
    /// <param name="questionBank">题库</param>
    /// <param name="title">文档标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="examTime">考试时间（分钟）</param>
    /// <returns>完整的LaTeX文档内容</returns>
    public string GenerateLaTeXDocument(QuestionBank questionBank, string title, string subject, int examTime)
    {
        if (questionBank == null)
        {
            throw new ArgumentNullException(nameof(questionBank));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("标题不能为空", nameof(title));
        }

        try
        {
            var documentBuilder = new StringBuilder();

            // 生成文档头部
            GenerateDocumentHeader(documentBuilder, title, subject, examTime);

            // 生成文档内容
            GenerateDocumentContent(documentBuilder, questionBank);

            // 生成文档尾部
            GenerateDocumentFooter(documentBuilder);

            return documentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("生成LaTeX文档失败", ex);
        }
    }

    /// <summary>
    /// 生成文档头部（使用新的HeaderConfig）
    /// </summary>
    /// <param name="builder">字符串构建器</param>
    /// <param name="headerConfig">抬头配置</param>
    private void GenerateDocumentHeader(StringBuilder builder, HeaderConfig headerConfig)
    {
        // 文档类
        builder.AppendLine($"\\documentclass[{_config.FontSize}]{{{_config.DocumentClass}}}");
        builder.AppendLine();

        // 基础包
        builder.AppendLine("\\usepackage{geometry}");
        builder.AppendLine($"\\geometry{{a4paper, margin={_config.PageMargin}}}");
        builder.AppendLine("\\usepackage{fancyhdr}");
        builder.AppendLine("\\usepackage{lastpage}");
        builder.AppendLine("\\usepackage{tikz}");
        builder.AppendLine("\\usepackage{ifthen}");

        // 中文支持
        if (_config.IncludeCtex)
        {
            builder.AppendLine("\\usepackage{ctex}");
            builder.AppendLine("\\usepackage{xeCJK}");
            builder.AppendLine("\\setCJKmainfont{SimSun}");
        }

        // 数学包
        if (_config.IncludeMathPackages)
        {
            builder.AppendLine("\\usepackage{amsmath}");
            builder.AppendLine("\\usepackage{amssymb}");
            builder.AppendLine("\\usepackage{amsfonts}");
        }

        // 图形包
        if (_config.IncludeGraphicPackages)
        {
            builder.AppendLine("\\usepackage{graphicx}");
            builder.AppendLine("\\usepackage{float}");
        }

        // 表格包
        if (_config.IncludeTablePackages)
        {
            builder.AppendLine("\\usepackage{array}");
            builder.AppendLine("\\usepackage{booktabs}");
            builder.AppendLine("\\usepackage{multirow}");
            builder.AppendLine("\\usepackage{multicol}");
        }

        // 自定义包
        foreach (var package in _config.CustomPackages)
        {
            builder.AppendLine($"\\usepackage{{{package}}}");
        }

        builder.AppendLine();

        // 自定义命令
        foreach (var command in _config.CustomCommands)
        {
            builder.AppendLine(command);
        }

        builder.AppendLine();

        // 页眉页脚设置
        builder.AppendLine("\\pagestyle{fancy}");
        builder.AppendLine("\\fancyhf{}");
        builder.AppendLine("\\fancyhead[C]{}");
        builder.AppendLine("\\fancyfoot[C]{第\\thepage 页/共\\pageref{LastPage}页}");
        builder.AppendLine();

        // 文档开始
        builder.AppendLine("\\begin{document}");
        builder.AppendLine();

        // 使用HeaderLayout生成抬头部分
        var headerContent = _headerLayout.GenerateHeader(headerConfig, 1);
        builder.Append(headerContent);
        builder.AppendLine();

        // 密封线
        builder.AppendLine("\\sealline{");
        builder.AppendLine("  \\begin{tabular}{ll}");
        builder.AppendLine("    姓名：\\underline{\\hspace{3cm}} & 考号：\\underline{\\hspace{3cm}} \\\\");
        builder.AppendLine("    班级：\\underline{\\hspace{3cm}} & 日期：\\underline{\\hspace{3cm}} \\\\");
        builder.AppendLine("  \\end{tabular}");
        builder.AppendLine("}");
        builder.AppendLine();

        builder.AppendLine("\\vspace{1cm}");
        builder.AppendLine();
    }

    /// <summary>
    /// 生成文档头部
    /// </summary>
    /// <param name="builder">字符串构建器</param>
    /// <param name="title">文档标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="examTime">考试时间（分钟）</param>
    private void GenerateDocumentHeader(StringBuilder builder, string title, string subject, int examTime)
    {
        // 创建默认的HeaderConfig
        var headerConfig = new HeaderConfig
        {
            ExamTitle = title,
            Subject = subject,
            ExamTime = examTime,
            Style = HeaderStyle.Standard,
            ShowStudentInfo = true
        };

        // 使用新的方法生成文档头部
        GenerateDocumentHeader(builder, headerConfig);
    }

    /// <summary>
    /// 生成文档内容
    /// </summary>
    /// <param name="builder">字符串构建器</param>
    /// <param name="questionBank">题库</param>
    private void GenerateDocumentContent(StringBuilder builder, QuestionBank questionBank)
    {
        if (questionBank?.Questions == null || questionBank.Questions.Count == 0)
        {
            builder.AppendLine("\\textbf{无题目内容}");
            return;
        }

        // 按题型分组
        var questionsByType = questionBank.Questions.GroupBy(q => q.Type).ToList();

        foreach (var group in questionsByType)
        {
            // 添加题型标题
            builder.AppendLine($"\\subsection*{{{EscapeLaTeX(group.Key)}}}");
            builder.AppendLine();

            // 生成该题型下的所有题目
            var questionNumber = 1;
            foreach (var question in group)
            {
                var content = _contentGenerator.GenerateQuestionContent(question, questionNumber);
                builder.AppendLine(content);
                questionNumber++;
            }
        }
    }

    /// <summary>
    /// 生成文档尾部
    /// </summary>
    /// <param name="builder">字符串构建器</param>
    private void GenerateDocumentFooter(StringBuilder builder)
    {
        builder.AppendLine("\\end{document}");
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