using QuizForge.Models;
using System.Text;

namespace QuizForge.Core.Layout;

/// <summary>
/// 抬头布局逻辑类
/// </summary>
public class HeaderLayout
{
    /// <summary>
    /// 生成抬头LaTeX代码
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTime">考试时间（分钟）</param>
    /// <param name="totalPoints">总分</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    public string GenerateHeader(ExamTemplate template, int examTime, decimal totalPoints, int pageNumber)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加试卷标题
        headerBuilder.AppendLine($@"{{\Large \textbf{{{EscapeLaTeX(template.Name)}}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加考试科目
        if (!string.IsNullOrWhiteSpace(template.Description))
        {
            headerBuilder.AppendLine($@"考试科目：{EscapeLaTeX(template.Description)}\\");
        }

        // 添加考试时间
        headerBuilder.AppendLine($@"考试时间：{examTime}分钟\\");

        // 添加总分
        headerBuilder.AppendLine($@"总分：{totalPoints}分");

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成抬头LaTeX代码（简化版）
    /// </summary>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="examTime">考试时间（分钟）</param>
    /// <param name="totalPoints">总分</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    public string GenerateHeader(string examTitle, string subject, int examTime, decimal totalPoints, int pageNumber)
    {
        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加试卷标题
        headerBuilder.AppendLine($@"{{\Large \textbf{{{EscapeLaTeX(examTitle)}}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加考试科目
        if (!string.IsNullOrWhiteSpace(subject))
        {
            headerBuilder.AppendLine($@"考试科目：{EscapeLaTeX(subject)}\\");
        }

        // 添加考试时间
        headerBuilder.AppendLine($@"考试时间：{examTime}分钟\\");

        // 添加总分
        headerBuilder.AppendLine($@"总分：{totalPoints}分");

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成自定义抬头LaTeX代码
    /// </summary>
    /// <param name="customContent">自定义抬头内容</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>自定义抬头LaTeX代码</returns>
    public string GenerateCustomHeader(string customContent, int pageNumber)
    {
        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(customContent))
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 自定义抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加自定义内容
        headerBuilder.AppendLine(EscapeLaTeX(customContent));

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
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