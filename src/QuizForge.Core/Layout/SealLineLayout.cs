using QuizForge.Models;
using System.Text;

namespace QuizForge.Core.Layout;

/// <summary>
/// 密封线布局逻辑类
/// </summary>
public class SealLineLayout
{
    /// <summary>
    /// 生成密封线LaTeX代码
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>密封线LaTeX代码</returns>
    public string GenerateSealLine(ExamTemplate template, int pageNumber, int totalPages)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // 生成密封线内容
        var sealLineContent = GenerateSealLineContent(template);

        // 根据页码和模板设置确定密封线位置
        var position = DetermineSealLinePosition(template, pageNumber);

        // 生成LaTeX代码
        return GenerateSealLineLaTeX(sealLineContent, position, pageNumber);
    }

    /// <summary>
    /// 生成密封线内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <returns>密封线内容</returns>
    private string GenerateSealLineContent(ExamTemplate template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        var config = template.SealLineContent;
        var contentBuilder = new StringBuilder();
        
        // 添加标题
        if (!string.IsNullOrWhiteSpace(config.Title))
        {
            contentBuilder.AppendLine($"\\textbf{{{EscapeLaTeX(config.Title)}}}\\\\");
            contentBuilder.AppendLine("\\vspace{0.3cm}");
        }
        
        // 开始表格
        contentBuilder.AppendLine("\\begin{tabular}{ll}");
        
        // 第一行：姓名和考号
        if (config.ShowName || config.ShowStudentId)
        {
            var nameField = config.ShowName ? $"{EscapeLaTeX(config.NameLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            var idField = config.ShowStudentId ? $"{EscapeLaTeX(config.StudentIdLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            
            if (!string.IsNullOrEmpty(nameField) && !string.IsNullOrEmpty(idField))
            {
                contentBuilder.AppendLine($"  {nameField} & {idField} \\\\");
            }
            else if (!string.IsNullOrEmpty(nameField))
            {
                contentBuilder.AppendLine($"  {nameField} & \\\\");
            }
            else if (!string.IsNullOrEmpty(idField))
            {
                contentBuilder.AppendLine($"  & {idField} \\\\");
            }
        }
        
        // 第二行：班级和日期
        if (config.ShowClass || config.ShowDate)
        {
            var classField = config.ShowClass ? $"{EscapeLaTeX(config.ClassLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            var dateField = config.ShowDate ? $"{EscapeLaTeX(config.DateLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            
            if (!string.IsNullOrEmpty(classField) && !string.IsNullOrEmpty(dateField))
            {
                contentBuilder.AppendLine($"  {classField} & {dateField} \\\\");
            }
            else if (!string.IsNullOrEmpty(classField))
            {
                contentBuilder.AppendLine($"  {classField} & \\\\");
            }
            else if (!string.IsNullOrEmpty(dateField))
            {
                contentBuilder.AppendLine($"  & {dateField} \\\\");
            }
        }
        
        // 第三行：学校和科目
        if (config.ShowSchool || config.ShowSubject)
        {
            var schoolField = config.ShowSchool ? $"{EscapeLaTeX(config.SchoolLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            var subjectField = config.ShowSubject ? $"{EscapeLaTeX(config.SubjectLabel)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            
            if (!string.IsNullOrEmpty(schoolField) && !string.IsNullOrEmpty(subjectField))
            {
                contentBuilder.AppendLine($"  {schoolField} & {subjectField} \\\\");
            }
            else if (!string.IsNullOrEmpty(schoolField))
            {
                contentBuilder.AppendLine($"  {schoolField} & \\\\");
            }
            else if (!string.IsNullOrEmpty(subjectField))
            {
                contentBuilder.AppendLine($"  & {subjectField} \\\\");
            }
        }
        
        // 第四行：自定义字段
        if (config.ShowCustomField1 || config.ShowCustomField2)
        {
            var customField1 = config.ShowCustomField1 ? $"{EscapeLaTeX(config.CustomField1Label)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            var customField2 = config.ShowCustomField2 ? $"{EscapeLaTeX(config.CustomField2Label)}\\underline{{\\hspace{{{config.UnderlineLength}cm}}}}" : "";
            
            if (!string.IsNullOrEmpty(customField1) && !string.IsNullOrEmpty(customField2))
            {
                contentBuilder.AppendLine($"  {customField1} & {customField2} \\\\");
            }
            else if (!string.IsNullOrEmpty(customField1))
            {
                contentBuilder.AppendLine($"  {customField1} & \\\\");
            }
            else if (!string.IsNullOrEmpty(customField2))
            {
                contentBuilder.AppendLine($"  & {customField2} \\\\");
            }
        }
        
        // 结束表格
        contentBuilder.AppendLine("\\end{tabular}");
        
        return contentBuilder.ToString();
    }

    /// <summary>
    /// 确定密封线位置
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>密封线位置</returns>
    private SealLinePosition DetermineSealLinePosition(ExamTemplate template, int pageNumber)
    {
        // 如果模板设置了固定位置且未启用动态调整，使用模板设置
        if (!template.EnableDynamicSealLinePosition &&
            (template.SealLine == SealLinePosition.Top || template.SealLine == SealLinePosition.Bottom))
        {
            return template.SealLine;
        }
        
        // 如果模板设置了固定位置（左或右）且未启用动态调整，使用模板设置
        if (!template.EnableDynamicSealLinePosition)
        {
            return template.SealLine;
        }

        // 启用动态调整时，根据页码自动确定：奇数页在左侧，偶数页在右侧
        return pageNumber % 2 == 1 ? SealLinePosition.Left : SealLinePosition.Right;
    }

    /// <summary>
    /// 生成密封线LaTeX代码
    /// </summary>
    /// <param name="content">密封线内容</param>
    /// <param name="position">密封线位置</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>密封线LaTeX代码</returns>
    private string GenerateSealLineLaTeX(string content, SealLinePosition position, int pageNumber)
    {
        return $@"
\sealline{{{content}}}";
    }

    /// <summary>
    /// 生成密封线LaTeX命令定义
    /// </summary>
    /// <returns>密封线LaTeX命令定义</returns>
    public string GenerateSealLineCommandDefinition()
    {
        return @"
% 密封线命令
\newcommand{\sealline}[1]{
  \begin{tikzpicture}[remember picture,overlay]
    \ifthenelse{\isodd{\thepage}}{
      % 奇数页密封线在左侧
      \draw[thick] (current page.north west) ++(0,-2) -- (current page.south west) ++(0,2);
      \node[rotate=90,anchor=center] at (current page.west) {#1};
    }{
      % 偶数页密封线在右侧
      \draw[thick] (current page.north east) ++(0,-2) -- (current page.south east) ++(0,2);
      \node[rotate=-90,anchor=center] at (current page.east) {#1};
    }
  \end{tikzpicture}
}

% 顶部密封线命令
\newcommand{\topsealline}[1]{
  \begin{tikzpicture}[remember picture,overlay]
    \draw[thick] (current page.north west) ++(2,0) -- (current page.north east) ++(-2,0);
    \node[anchor=center] at (current page.north) {#1};
  \end{tikzpicture}
}

% 底部密封线命令
\newcommand{\bottomsealline}[1]{
  \begin{tikzpicture}[remember picture,overlay]
    \draw[thick] (current page.south west) ++(2,0) -- (current page.south east) ++(-2,0);
    \node[anchor=center] at (current page.south) {#1};
  \end{tikzpicture}
}";
    }
    
    /// <summary>
    /// 生成指定位置的密封线LaTeX代码
    /// </summary>
    /// <param name="content">密封线内容</param>
    /// <param name="position">密封线位置</param>
    /// <returns>密封线LaTeX代码</returns>
    public string GeneratePositionalSealLineLaTeX(string content, SealLinePosition position)
    {
        return position switch
        {
            SealLinePosition.Top => $@"
\topsealline{{{content}}}",
            SealLinePosition.Bottom => $@"
\bottomsealline{{{content}}}",
            SealLinePosition.Left => $@"
\sealline{{{content}}}",
            SealLinePosition.Right => $@"
\sealline{{{content}}}",
            _ => $@"
\sealline{{{content}}}"
        };
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