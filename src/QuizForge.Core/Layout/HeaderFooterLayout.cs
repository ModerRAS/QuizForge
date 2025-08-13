using QuizForge.Models;
using System.Text;

namespace QuizForge.Core.Layout;

/// <summary>
/// 页眉页脚布局逻辑类
/// </summary>
public class HeaderFooterLayout
{
    /// <summary>
    /// 生成页眉页脚LaTeX代码
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页眉页脚LaTeX代码</returns>
    public string GenerateHeaderFooter(ExamTemplate template, string examTitle, string subject, int pageNumber, int totalPages)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        var builder = new StringBuilder();

        // 生成页眉页脚设置
        builder.AppendLine(GenerateHeaderFooterSetup());

        // 生成页眉内容
        builder.AppendLine(GenerateHeader(template, examTitle, subject, pageNumber));

        // 生成页脚内容
        builder.AppendLine(GenerateFooter(template, pageNumber, totalPages));

        return builder.ToString();
    }

    /// <summary>
    /// 生成页眉页脚设置
    /// </summary>
    /// <returns>页眉页脚设置LaTeX代码</returns>
    public string GenerateHeaderFooterSetup()
    {
        return @"
% 页眉页脚设置
\pagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0.4pt}
\renewcommand{\footrulewidth}{0.4pt}";
    }

    /// <summary>
    /// 生成页眉内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>页眉LaTeX代码</returns>
    public string GenerateHeader(ExamTemplate template, string examTitle, string subject, int pageNumber)
    {
        var headerContent = new StringBuilder();

        // 如果模板有自定义页眉内容，优先使用
        if (!string.IsNullOrWhiteSpace(template.HeaderContent))
        {
            headerContent.AppendLine($@"\fancyhead[C]{{{EscapeLaTeX(template.HeaderContent)}}}");
        }
        else
        {
            // 否则使用默认页眉格式
            var defaultHeader = new StringBuilder();

            // 添加试卷标题
            if (!string.IsNullOrWhiteSpace(examTitle))
            {
                defaultHeader.Append(EscapeLaTeX(examTitle));
            }

            // 添加考试科目
            if (!string.IsNullOrWhiteSpace(subject))
            {
                if (defaultHeader.Length > 0)
                {
                    defaultHeader.Append(" - ");
                }
                defaultHeader.Append(EscapeLaTeX(subject));
            }

            headerContent.AppendLine($@"\fancyhead[C]{{{defaultHeader.ToString()}}}");
        }

        return headerContent.ToString();
    }

    /// <summary>
    /// 生成页脚内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页脚LaTeX代码</returns>
    public string GenerateFooter(ExamTemplate template, int pageNumber, int totalPages)
    {
        var footerContent = new StringBuilder();

        // 如果模板有自定义页脚内容，优先使用
        if (!string.IsNullOrWhiteSpace(template.FooterContent))
        {
            footerContent.AppendLine($@"\fancyfoot[C]{{{EscapeLaTeX(template.FooterContent)}}}");
        }
        else
        {
            // 否则使用默认页脚格式（页码）
            footerContent.AppendLine($@"\fancyfoot[C]{{第{pageNumber}页/共{totalPages}页}}");
        }

        return footerContent.ToString();
    }

    /// <summary>
    /// 生成首页页眉页脚设置（首页可能需要不同的样式）
    /// </summary>
    /// <returns>首页页眉页脚设置LaTeX代码</returns>
    public string GenerateFirstPageHeaderFooterSetup()
    {
        return @"
% 首页页眉页脚设置
\thispagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0.4pt}
\renewcommand{\footrulewidth}{0.4pt}";
    }

    /// <summary>
    /// 生成首页页眉内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <returns>首页页眉LaTeX代码</returns>
    public string GenerateFirstPageHeader(ExamTemplate template, string examTitle, string subject)
    {
        // 首页可能不需要页眉，或者需要特殊的页眉
        return @"
% 首页不显示页眉
\fancyhead[C]{}";
    }

    /// <summary>
    /// 生成首页页脚内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>首页页脚LaTeX代码</returns>
    public string GenerateFirstPageFooter(ExamTemplate template, int totalPages)
    {
        var footerContent = new StringBuilder();

        // 如果模板有自定义页脚内容，优先使用
        if (!string.IsNullOrWhiteSpace(template.FooterContent))
        {
            footerContent.AppendLine($@"\fancyfoot[C]{{{EscapeLaTeX(template.FooterContent)}}}");
        }
        else
        {
            // 首页页脚格式
            footerContent.AppendLine($@"\fancyfoot[C]{{第1页/共{totalPages}页}}");
        }

        return footerContent.ToString();
    }

    /// <summary>
    /// 生成空白页眉页脚设置（用于特殊页面，如答题卡）
    /// </summary>
    /// <returns>空白页眉页脚设置LaTeX代码</returns>
    public string GenerateBlankHeaderFooterSetup()
    {
        return @"
% 空白页眉页脚设置
\thispagestyle{plain}";
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