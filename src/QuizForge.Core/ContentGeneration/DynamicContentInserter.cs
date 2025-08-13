using QuizForge.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 动态内容插入器，处理动态内容插入到LaTeX模板
/// </summary>
public class DynamicContentInserter
{
    private readonly Dictionary<string, Func<ExamTemplate, List<Question>, string>> _placeholderHandlers;

    public DynamicContentInserter()
    {
        _placeholderHandlers = new Dictionary<string, Func<ExamTemplate, List<Question>, string>>
        {
            { "{EXAM_TITLE}", (template, questions) => EscapeLaTeX(template.Name) },
            { "{SUBJECT}", (template, questions) => EscapeLaTeX(template.Description) },
            { "{EXAM_TIME}", (template, questions) => "120" }, // 默认120分钟
            { "{TOTAL_POINTS}", (template, questions) => questions.Sum(q => q.Points).ToString() },
            { "{HEADER_CONTENT}", (template, questions) => EscapeLaTeX(template.HeaderContent) },
            { "{FOOTER_CONTENT}", (template, questions) => EscapeLaTeX(template.FooterContent) },
            { "{CONTENT}", (template, questions) => GenerateContent(template, questions) },
            { "{ANSWER_SHEET_CONTENT}", (template, questions) => GenerateAnswerSheetContent(questions) },
            { "{LAYOUT_ELEMENTS}", (template, questions) => GenerateLayoutElements(template, questions) }
        };
    }

    /// <summary>
    /// 插入动态内容到模板
    /// </summary>
    /// <param name="templateContent">模板内容</param>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>插入动态内容后的完整LaTeX内容</returns>
    public string InsertDynamicContent(string templateContent, ExamTemplate template, List<Question> questions)
    {
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new ArgumentException("模板内容不能为空", nameof(templateContent));
        }

        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (questions == null)
        {
            throw new ArgumentNullException(nameof(questions));
        }

        var result = templateContent;

        // 处理所有占位符
        foreach (var placeholder in _placeholderHandlers.Keys)
        {
            if (result.Contains(placeholder))
            {
                var handler = _placeholderHandlers[placeholder];
                var replacement = handler(template, questions);
                result = result.Replace(placeholder, replacement);
            }
        }

        return result;
    }

    /// <summary>
    /// 生成题目内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>题目内容的LaTeX代码</returns>
    private string GenerateContent(ExamTemplate template, List<Question> questions)
    {
        var contentBuilder = new StringBuilder();
        var questionNumber = 1;

        // 按章节组织题目
        foreach (var section in template.Sections)
        {
            // 添加章节标题
            contentBuilder.AppendLine($"\\section*{{{EscapeLaTeX(section.Title)}}}");
            
            if (!string.IsNullOrWhiteSpace(section.Instructions))
            {
                contentBuilder.AppendLine($"\\textbf{{{EscapeLaTeX(section.Instructions)}}}");
                contentBuilder.AppendLine();
            }

            // 获取章节题目
            var sectionQuestions = questions.Where(q => section.QuestionIds.Contains(q.Id)).ToList();
            if (sectionQuestions.Count == 0)
            {
                continue;
            }

            // 生成题目内容
            var contentGenerator = new ContentGenerator();
            foreach (var question in sectionQuestions)
            {
                var questionContent = contentGenerator.GenerateQuestionContent(question, questionNumber);
                contentBuilder.Append(questionContent);
                questionNumber++;
            }

            contentBuilder.AppendLine();
        }

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成答题卡内容
    /// </summary>
    /// <param name="questions">题目列表</param>
    /// <returns>答题卡内容的LaTeX代码</returns>
    private string GenerateAnswerSheetContent(List<Question> questions)
    {
        var contentBuilder = new StringBuilder();
        var contentGenerator = new ContentGenerator();

        for (int i = 0; i < questions.Count; i++)
        {
            var answerSheetContent = contentGenerator.GenerateAnswerSheetContent(questions[i], i + 1);
            contentBuilder.Append(answerSheetContent);
        }

        return contentBuilder.ToString();
    }

    /// <summary>
    /// 生成布局元素
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>布局元素的LaTeX代码</returns>
    private string GenerateLayoutElements(ExamTemplate template, List<Question> questions)
    {
        var layoutBuilder = new StringBuilder();
        var totalPoints = questions.Sum(q => q.Points);

        // 添加密封线命令定义
        layoutBuilder.AppendLine(GenerateSealLineCommandDefinition());

        // 添加页眉页脚设置
        layoutBuilder.AppendLine(GenerateHeaderFooterSetup());

        // 添加抬头（第一页）
        layoutBuilder.AppendLine(GenerateHeader(template, 120, totalPoints, 1));

        // 添加密封线（第一页）
        layoutBuilder.AppendLine(GenerateSealLine(template, 1, 1));

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 生成多页试卷内容
    /// </summary>
    /// <param name="templateContent">模板内容</param>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <param name="questionsPerPage">每页题目数量</param>
    /// <returns>多页试卷的LaTeX内容</returns>
    public string InsertMultiPageDynamicContent(string templateContent, ExamTemplate template, List<Question> questions, int questionsPerPage = 5)
    {
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new ArgumentException("模板内容不能为空", nameof(templateContent));
        }

        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (questions == null)
        {
            throw new ArgumentNullException(nameof(questions));
        }

        // 计算总页数
        var totalPages = (int)Math.Ceiling((double)questions.Count / questionsPerPage);
        if (totalPages == 0)
        {
            totalPages = 1;
        }

        // 生成多页布局元素
        var layoutElements = GenerateMultiPageLayoutElements(template, questions, totalPages);

        // 分页生成题目内容
        var contentBuilder = new StringBuilder();
        var answerSheetBuilder = new StringBuilder();
        var questionNumber = 1;

        for (int page = 1; page <= totalPages; page++)
        {
            // 获取当前页的题目
            var startIndex = (page - 1) * questionsPerPage;
            var endIndex = Math.Min(startIndex + questionsPerPage, questions.Count);
            var pageQuestions = questions.Skip(startIndex).Take(endIndex - startIndex).ToList();

            // 如果不是第一页，添加分页符
            if (page > 1)
            {
                contentBuilder.AppendLine("\\newpage");
            }

            // 生成当前页的题目内容
            var contentGenerator = new ContentGenerator();
            foreach (var question in pageQuestions)
            {
                var questionContent = contentGenerator.GenerateQuestionContent(question, questionNumber);
                contentBuilder.Append(questionContent);
                
                var answerSheetContent = contentGenerator.GenerateAnswerSheetContent(question, questionNumber);
                answerSheetBuilder.Append(answerSheetContent);
                
                questionNumber++;
            }
        }

        // 替换模板占位符
        var result = templateContent
            .Replace("{EXAM_TITLE}", EscapeLaTeX(template.Name))
            .Replace("{SUBJECT}", EscapeLaTeX(template.Description))
            .Replace("{EXAM_TIME}", "120") // 默认120分钟
            .Replace("{TOTAL_POINTS}", questions.Sum(q => q.Points).ToString())
            .Replace("{HEADER_CONTENT}", EscapeLaTeX(template.HeaderContent))
            .Replace("{FOOTER_CONTENT}", EscapeLaTeX(template.FooterContent))
            .Replace("{CONTENT}", contentBuilder.ToString())
            .Replace("{ANSWER_SHEET_CONTENT}", answerSheetBuilder.ToString())
            .Replace("{LAYOUT_ELEMENTS}", layoutElements);

        return result;
    }

    /// <summary>
    /// 生成多页布局元素
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>多页布局元素的LaTeX代码</returns>
    private string GenerateMultiPageLayoutElements(ExamTemplate template, List<Question> questions, int totalPages)
    {
        var layoutBuilder = new StringBuilder();
        var totalPoints = questions.Sum(q => q.Points);

        // 添加密封线命令定义
        layoutBuilder.AppendLine(GenerateSealLineCommandDefinition());

        // 添加页眉页脚设置
        layoutBuilder.AppendLine(GenerateHeaderFooterSetup());

        // 为每一页生成布局元素
        for (int page = 1; page <= totalPages; page++)
        {
            // 如果不是第一页，添加分页符
            if (page > 1)
            {
                layoutBuilder.AppendLine("\\newpage");
            }

            // 添加抬头（只在第一页）
            if (page == 1)
            {
                layoutBuilder.AppendLine(GenerateHeader(template, 120, totalPoints, page));
            }

            // 添加密封线
            layoutBuilder.AppendLine(GenerateSealLine(template, page, totalPages));

            // 添加页眉页脚
            layoutBuilder.AppendLine(GenerateHeaderFooter(template, template.Name, template.Description, page, totalPages));
        }

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 生成密封线命令定义
    /// </summary>
    /// <returns>密封线命令定义的LaTeX代码</returns>
    private string GenerateSealLineCommandDefinition()
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

    /// <summary>
    /// 生成页眉页脚设置
    /// </summary>
    /// <returns>页眉页脚设置的LaTeX代码</returns>
    private string GenerateHeaderFooterSetup()
    {
        return @"
% 页眉页脚设置
\pagestyle{fancy}
\fancyhf{}
\fancyhead[C]{" + "{HEADER_CONTENT}" + @"}
\fancyfoot[C]{第\thepage 页/共\pageref{LastPage}页}";
    }

    /// <summary>
    /// 生成抬头
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTime">考试时间</param>
    /// <param name="totalPoints">总分</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头的LaTeX代码</returns>
    private string GenerateHeader(ExamTemplate template, int examTime, decimal totalPoints, int pageNumber)
    {
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        return $@"
% 抬头部分
\\begin{{center}}
{{\\Large \\textbf{{{EscapeLaTeX(template.Name)}}}}}\\\\
\\vspace{{0.5cm}}
考试科目：{EscapeLaTeX(template.Description)}\\\\
考试时间：{examTime}分钟\\\\
总分：{totalPoints}分
\\end{{center}}";
    }

    /// <summary>
    /// 生成密封线
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>密封线的LaTeX代码</returns>
    private string GenerateSealLine(ExamTemplate template, int pageNumber, int totalPages)
    {
        return $@"
% 密封线
\\sealline{{
  \\begin{{tabular}}{{ll}}
    姓名：\\underline{{\\hspace{{3cm}}}} & 考号：\\underline{{\\hspace{{3cm}}}} \\\\
    班级：\\underline{{\\hspace{{3cm}}}} & 日期：\\underline{{\\hspace{{3cm}}}} \\\\
  \\end{{tabular}}
}}";
    }

    /// <summary>
    /// 生成页眉页脚
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="title">标题</param>
    /// <param name="subject">科目</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="totalPages">总页数</param>
    /// <returns>页眉页脚的LaTeX代码</returns>
    private string GenerateHeaderFooter(ExamTemplate template, string title, string subject, int pageNumber, int totalPages)
    {
        return $@"
% 页眉页脚
\\fancyhead[C]{{{EscapeLaTeX(template.HeaderContent)}}}
\\fancyfoot[C]{{第{pageNumber} 页/共{totalPages}页}}";
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