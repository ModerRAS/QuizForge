using QuizForge.Models;
using System.Text;

namespace QuizForge.Core.Layout;

/// <summary>
/// 页眉页脚布局逻辑类
/// </summary>
public class HeaderFooterLayout
{
    /// <summary>
    /// 根据页码位置获取LaTeX命令
    /// </summary>
    /// <param name="position">页码位置</param>
    /// <returns>LaTeX命令</returns>
    private string GetPageNumberPositionCommand(PageNumberPosition position)
    {
        return position switch
        {
            PageNumberPosition.Left => "L",
            PageNumberPosition.Center => "C",
            PageNumberPosition.Right => "R",
            PageNumberPosition.Outside => "O",
            PageNumberPosition.Inside => "I",
            _ => "C"
        };
    }

    /// <summary>
    /// 根据页码格式获取页码文本
    /// </summary>
    /// <param name="format">页码格式</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页码文本</returns>
    private string GetPageNumberText(PageNumberFormat format, int pageNumber, int totalPages)
    {
        return format switch
        {
            PageNumberFormat.Chinese => $"第{pageNumber}页/共{totalPages}页",
            PageNumberFormat.Numeric => $"{pageNumber}/{totalPages}",
            PageNumberFormat.English => $"Page {pageNumber} of {totalPages}",
            _ => $"第{pageNumber}页/共{totalPages}页"
        };
    }
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
    /// 生成页眉页脚LaTeX代码（使用HeaderConfig）
    /// </summary>
    /// <param name="headerConfig">抬头配置</param>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页眉页脚LaTeX代码</returns>
    public string GenerateHeaderFooter(HeaderConfig headerConfig, string examTitle, string subject, int pageNumber, int totalPages)
    {
        if (headerConfig == null)
        {
            throw new ArgumentNullException(nameof(headerConfig));
        }

        var builder = new StringBuilder();

        // 生成页眉页脚设置
        if (headerConfig.EnableOddEvenHeaderFooter)
        {
            builder.AppendLine(GenerateOddEvenHeaderFooterSetup());
        }
        else
        {
            builder.AppendLine(GenerateHeaderFooterSetup());
        }

        // 生成页眉内容
        builder.AppendLine(GenerateHeader(headerConfig, examTitle, subject, pageNumber));

        // 生成页脚内容
        builder.AppendLine(GenerateFooter(headerConfig, pageNumber, totalPages));

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
\renewcommand{\footrulewidth}{0.4pt}
% 设置页眉页脚字体大小
\newcommand{\headerfontsiz}{\small}
\newcommand{\footerfontsiz}{\footnotesize}";
    }

    /// <summary>
    /// 生成支持奇偶页不同的页眉页脚设置
    /// </summary>
    /// <returns>页眉页脚设置LaTeX代码</returns>
    public string GenerateOddEvenHeaderFooterSetup()
    {
        return @"
% 页眉页脚设置（支持奇偶页不同）
\pagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0.4pt}
\renewcommand{\footrulewidth}{0.4pt}
% 设置页眉页脚字体大小
\newcommand{\headerfontsiz}{\small}
\newcommand{\footerfontsiz}{\footnotesize}";
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
            headerContent.AppendLine($@"\fancyhead[C]{{\headerfontsiz {EscapeLaTeX(template.HeaderContent)}}}");
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
                    defaultHeader.Append(" \\quad ");
                }
                defaultHeader.Append(EscapeLaTeX(subject));
            }

            headerContent.AppendLine($@"\fancyhead[C]{{\headerfontsiz {defaultHeader.ToString()}}}");
        }

        return headerContent.ToString();
    }

    /// <summary>
    /// 生成页眉内容（使用HeaderConfig）
    /// </summary>
    /// <param name="headerConfig">抬头配置</param>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>页眉LaTeX代码</returns>
    public string GenerateHeader(HeaderConfig headerConfig, string examTitle, string subject, int pageNumber)
    {
        var headerContent = new StringBuilder();

        // 检查是否启用奇偶页不同的页眉页脚
        if (headerConfig.EnableOddEvenHeaderFooter)
        {
            // 奇数页页眉
            if (!string.IsNullOrWhiteSpace(headerConfig.OddPageHeaderContent))
            {
                headerContent.AppendLine($@"\fancyhead[O]{{\headerfontsiz {EscapeLaTeX(headerConfig.OddPageHeaderContent)}}}");
            }
            else
            {
                var defaultHeader = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(examTitle))
                {
                    defaultHeader.Append(EscapeLaTeX(examTitle));
                }
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    if (defaultHeader.Length > 0)
                    {
                        defaultHeader.Append(" \\quad ");
                    }
                    defaultHeader.Append(EscapeLaTeX(subject));
                }
                headerContent.AppendLine($@"\fancyhead[O]{{\headerfontsiz {defaultHeader.ToString()}}}");
            }

            // 偶数页页眉
            if (!string.IsNullOrWhiteSpace(headerConfig.EvenPageHeaderContent))
            {
                headerContent.AppendLine($@"\fancyhead[E]{{\headerfontsiz {EscapeLaTeX(headerConfig.EvenPageHeaderContent)}}}");
            }
            else
            {
                var defaultHeader = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    defaultHeader.Append(EscapeLaTeX(subject));
                }
                if (!string.IsNullOrWhiteSpace(examTitle))
                {
                    if (defaultHeader.Length > 0)
                    {
                        defaultHeader.Append(" \\quad ");
                    }
                    defaultHeader.Append(EscapeLaTeX(examTitle));
                }
                headerContent.AppendLine($@"\fancyhead[E]{{\headerfontsiz {defaultHeader.ToString()}}}");
            }
        }
        else
        {
            // 如果有自定义页眉内容，优先使用
            if (!string.IsNullOrWhiteSpace(headerConfig.OddPageHeaderContent))
            {
                headerContent.AppendLine($@"\fancyhead[C]{{\headerfontsiz {EscapeLaTeX(headerConfig.OddPageHeaderContent)}}}");
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
                        defaultHeader.Append(" \\quad ");
                    }
                    defaultHeader.Append(EscapeLaTeX(subject));
                }

                headerContent.AppendLine($@"\fancyhead[C]{{\headerfontsiz {defaultHeader.ToString()}}}");
            }
        }

        // 如果需要在页眉显示页码
        if (headerConfig.ShowPageNumberInHeader)
        {
            string positionCommand = GetPageNumberPositionCommand(headerConfig.PageNumberPosition);
            headerContent.AppendLine($@"\fancyhead[{positionCommand}]{{\headerfontsiz \thepage}}");
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
            footerContent.AppendLine($@"\fancyfoot[C]{{\footerfontsiz {EscapeLaTeX(template.FooterContent)}}}");
        }
        else
        {
            // 否则使用默认页脚格式（页码）
            footerContent.AppendLine($@"\fancyfoot[C]{{\footerfontsiz 第{pageNumber}页/共{totalPages}页}}");
        }

        return footerContent.ToString();
    }

    /// <summary>
    /// 生成页脚内容（使用HeaderConfig）
    /// </summary>
    /// <param name="headerConfig">抬头配置</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页脚LaTeX代码</returns>
    public string GenerateFooter(HeaderConfig headerConfig, int pageNumber, int totalPages)
    {
        var footerContent = new StringBuilder();

        // 检查是否启用奇偶页不同的页眉页脚
        if (headerConfig.EnableOddEvenHeaderFooter)
        {
            // 奇数页页脚
            if (!string.IsNullOrWhiteSpace(headerConfig.OddPageFooterContent))
            {
                footerContent.AppendLine($@"\fancyfoot[O]{{\footerfontsiz {EscapeLaTeX(headerConfig.OddPageFooterContent)}}}");
            }

            // 偶数页页脚
            if (!string.IsNullOrWhiteSpace(headerConfig.EvenPageFooterContent))
            {
                footerContent.AppendLine($@"\fancyfoot[E]{{\footerfontsiz {EscapeLaTeX(headerConfig.EvenPageFooterContent)}}}");
            }
        }
        else
        {
            // 如果有自定义页脚内容，优先使用
            if (!string.IsNullOrWhiteSpace(headerConfig.OddPageFooterContent))
            {
                footerContent.AppendLine($@"\fancyfoot[C]{{\footerfontsiz {EscapeLaTeX(headerConfig.OddPageFooterContent)}}}");
            }
        }

        // 如果需要在页脚显示页码
        if (headerConfig.ShowPageNumberInFooter)
        {
            string positionCommand = GetPageNumberPositionCommand(headerConfig.PageNumberPosition);
            string pageNumberText = GetPageNumberText(headerConfig.PageNumberFormat, pageNumber, totalPages);
            
            // 如果启用了奇偶页不同的页眉页脚，需要分别设置奇偶页的页码
            if (headerConfig.EnableOddEvenHeaderFooter)
            {
                footerContent.AppendLine($@"\fancyfoot[{positionCommand}]{{\footerfontsiz {pageNumberText}}}");
            }
            else
            {
                footerContent.AppendLine($@"\fancyfoot[{positionCommand}]{{\footerfontsiz {pageNumberText}}}");
            }
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
\renewcommand{\footrulewidth}{0.4pt}
% 设置页眉页脚字体大小
\newcommand{\headerfontsiz}{\small}
\newcommand{\footerfontsiz}{\footnotesize}";
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
            footerContent.AppendLine($@"\fancyfoot[C]{{\footerfontsiz {EscapeLaTeX(template.FooterContent)}}}");
        }
        else
        {
            // 首页页脚格式
            footerContent.AppendLine($@"\fancyfoot[C]{{\footerfontsiz 第1页/共{totalPages}页}}");
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