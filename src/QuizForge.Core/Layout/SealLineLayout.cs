using QuizForge.Models;

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
        var sealLineContent = GenerateSealLineContent();

        // 根据页码和模板设置确定密封线位置
        var position = DetermineSealLinePosition(template, pageNumber);

        // 生成LaTeX代码
        return GenerateSealLineLaTeX(sealLineContent, position, pageNumber);
    }

    /// <summary>
    /// 生成密封线内容
    /// </summary>
    /// <returns>密封线内容</returns>
    private string GenerateSealLineContent()
    {
        return @"
          \begin{tabular}{ll}
            姓名：\underline{\hspace{3cm}} & 考号：\underline{\hspace{3cm}} \\
            班级：\underline{\hspace{3cm}} & 日期：\underline{\hspace{3cm}} \\
          \end{tabular}";
    }

    /// <summary>
    /// 确定密封线位置
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>密封线位置</returns>
    private SealLinePosition DetermineSealLinePosition(ExamTemplate template, int pageNumber)
    {
        // 如果模板设置了固定位置，使用模板设置
        if (template.SealLine != SealLinePosition.Left && template.SealLine != SealLinePosition.Right)
        {
            return template.SealLine;
        }

        // 否则根据页码自动确定：奇数页在左侧，偶数页在右侧
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
}";
    }
}