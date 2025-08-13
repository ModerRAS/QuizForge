using QuizForge.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 内容生成器，负责将题库内容转换为LaTeX格式
/// </summary>
public class ContentGenerator
{
    /// <summary>
    /// 生成选择题的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>选择题的LaTeX内容</returns>
    public string GenerateMultipleChoiceQuestion(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        var contentBuilder = new StringBuilder();
        
        // 添加题目
        contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
        contentBuilder.AppendLine(EscapeLaTeX(question.Content));
        contentBuilder.AppendLine();

        // 添加选项
        if (question.Options != null && question.Options.Count > 0)
        {
            contentBuilder.AppendLine("\\begin{enumerate}");
            foreach (var option in question.Options)
            {
                contentBuilder.AppendLine($"\\item {EscapeLaTeX(option.Value)}");
            }
            contentBuilder.AppendLine("\\end{enumerate}");
            contentBuilder.AppendLine();
        }

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成填空题的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>填空题的LaTeX内容</returns>
    public string GenerateFillInBlankQuestion(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        var contentBuilder = new StringBuilder();
        
        // 添加题目
        contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
        
        // 处理填空题内容，将下划线替换为LaTeX格式的填空线
        var content = EscapeLaTeX(question.Content);
        content = Regex.Replace(content, "_{3,}", "\\underline{\\hspace{3cm}}");
        contentBuilder.AppendLine(content);
        contentBuilder.AppendLine();

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成简答题的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>简答题的LaTeX内容</returns>
    public string GenerateEssayQuestion(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        var contentBuilder = new StringBuilder();
        
        // 添加题目
        contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
        contentBuilder.AppendLine(EscapeLaTeX(question.Content));
        contentBuilder.AppendLine();
        
        // 添加答题区域
        contentBuilder.AppendLine("\\vspace{5cm}");
        contentBuilder.AppendLine();

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成判断题的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>判断题的LaTeX内容</returns>
    public string GenerateTrueFalseQuestion(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        var contentBuilder = new StringBuilder();
        
        // 添加题目
        contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
        contentBuilder.AppendLine(EscapeLaTeX(question.Content));
        contentBuilder.AppendLine();
        
        // 添加选项
        contentBuilder.AppendLine("\\begin{enumerate}");
        contentBuilder.AppendLine("\\item 正确");
        contentBuilder.AppendLine("\\item 错误");
        contentBuilder.AppendLine("\\end{enumerate}");
        contentBuilder.AppendLine();

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 根据题目类型生成对应的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>题目的LaTeX内容</returns>
    public string GenerateQuestionContent(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        return question.Type switch
        {
            "选择题" => GenerateMultipleChoiceQuestion(question, questionNumber),
            "填空题" => GenerateFillInBlankQuestion(question, questionNumber),
            "简答题" => GenerateEssayQuestion(question, questionNumber),
            "判断题" => GenerateTrueFalseQuestion(question, questionNumber),
            _ => GenerateGenericQuestion(question, questionNumber)
        };
    }

    /// <summary>
    /// 生成通用题目的LaTeX内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>通用题目的LaTeX内容</returns>
    private string GenerateGenericQuestion(Question question, int questionNumber)
    {
        var contentBuilder = new StringBuilder();
        
        // 添加题目
        contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
        contentBuilder.AppendLine(EscapeLaTeX(question.Content));
        contentBuilder.AppendLine();

        // 添加选项（如果有）
        if (question.Options != null && question.Options.Count > 0)
        {
            contentBuilder.AppendLine("\\begin{enumerate}");
            foreach (var option in question.Options)
            {
                contentBuilder.AppendLine($"\\item {EscapeLaTeX(option.Value)}");
            }
            contentBuilder.AppendLine("\\end{enumerate}");
            contentBuilder.AppendLine();
        }

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成答题卡内容
    /// </summary>
    /// <param name="question">题目</param>
    /// <param name="questionNumber">题号</param>
    /// <returns>答题卡的LaTeX内容</returns>
    public string GenerateAnswerSheetContent(Question question, int questionNumber)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        var contentBuilder = new StringBuilder();
        
        // 根据题目类型生成不同的答题卡格式
        switch (question.Type)
        {
            case "选择题":
            case "判断题":
                contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} \\underline{{\\hspace{{2cm}}}} ({question.Points}分)");
                break;
            case "填空题":
                contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} \\underline{{\\hspace{{5cm}}}} ({question.Points}分)");
                break;
            case "简答题":
                contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} ({question.Points}分)");
                contentBuilder.AppendLine("\\vspace{5cm}");
                break;
            default:
                contentBuilder.AppendLine($"\\textbf{{题目 {questionNumber}}} \\underline{{\\hspace{{5cm}}}} ({question.Points}分)");
                break;
        }
        
        contentBuilder.AppendLine();

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成章节标题和说明
    /// </summary>
    /// <param name="section">模板章节</param>
    /// <returns>章节标题和说明的LaTeX内容</returns>
    public string GenerateSectionContent(TemplateSection section)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        var contentBuilder = new StringBuilder();
        
        // 添加章节标题
        contentBuilder.AppendLine($"\\section*{{{EscapeLaTeX(section.Title)}}}");
        
        if (!string.IsNullOrWhiteSpace(section.Instructions))
        {
            contentBuilder.AppendLine($"\\textbf{{{EscapeLaTeX(section.Instructions)}}}");
            contentBuilder.AppendLine();
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