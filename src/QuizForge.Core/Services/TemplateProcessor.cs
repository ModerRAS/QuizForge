using QuizForge.Models;
using QuizForge.Core.Layout;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.Services;

/// <summary>
/// 模板处理器，负责模板的核心业务逻辑
/// </summary>
public class TemplateProcessor
{
    private readonly Dictionary<TemplateStyle, string> _templatePaths = new()
    {
        { TemplateStyle.Basic, "QuizForge.Infrastructure.Templates.BasicExamTemplate.tex" },
        { TemplateStyle.Advanced, "QuizForge.Infrastructure.Templates.AdvancedExamTemplate.tex" }
    };

    private readonly SealLineLayout _sealLineLayout;
    private readonly HeaderLayout _headerLayout;
    private readonly HeaderFooterLayout _headerFooterLayout;

    public TemplateProcessor(
        SealLineLayout sealLineLayout,
        HeaderLayout headerLayout,
        HeaderFooterLayout headerFooterLayout)
    {
        _sealLineLayout = sealLineLayout ?? throw new ArgumentNullException(nameof(sealLineLayout));
        _headerLayout = headerLayout ?? throw new ArgumentNullException(nameof(headerLayout));
        _headerFooterLayout = headerFooterLayout ?? throw new ArgumentNullException(nameof(headerFooterLayout));
    }

    /// <summary>
    /// 处理模板数据
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>处理后的模板数据</returns>
    public ExamTemplate ProcessTemplate(ExamTemplate template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // 设置创建时间和更新时间
        if (template.CreatedAt == default)
        {
            template.CreatedAt = DateTime.UtcNow;
        }
        template.UpdatedAt = DateTime.UtcNow;

        // 处理模板章节
        foreach (var section in template.Sections)
        {
            if (section.Id == default)
            {
                section.Id = Guid.NewGuid();
            }
        }

        return template;
    }
    
    /// <summary>
    /// 验证模板数据
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>验证结果</returns>
    public bool ValidateTemplate(ExamTemplate template)
    {
        if (template == null)
        {
            return false;
        }

        // 验证基本信息
        if (string.IsNullOrWhiteSpace(template.Name))
        {
            return false;
        }

        // 验证章节
        if (template.Sections == null || template.Sections.Count == 0)
        {
            return false;
        }

        // 验证每个章节
        foreach (var section in template.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Title) || section.QuestionCount <= 0)
            {
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// 生成模板内容
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <param name="questions">题目列表</param>
    /// <returns>模板内容</returns>
    public string GenerateTemplateContent(ExamTemplate template, List<Question> questions)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (questions == null)
        {
            throw new ArgumentNullException(nameof(questions));
        }

        // 获取模板文件路径
        if (!_templatePaths.TryGetValue(template.Style, out var templatePath))
        {
            throw new NotSupportedException($"不支持的模板样式: {template.Style}");
        }

        // 读取模板文件内容
        var templateContent = ReadTemplateFile(templatePath);
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new InvalidOperationException("无法读取模板文件内容");
        }

        // 生成题目内容
        var contentBuilder = new StringBuilder();
        var answerSheetBuilder = new StringBuilder();
        var totalPoints = 0m;

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
            for (int i = 0; i < sectionQuestions.Count; i++)
            {
                var question = sectionQuestions[i];
                totalPoints += question.Points;
                
                // 添加题目
                contentBuilder.AppendLine($"\\textbf{{题目 {i + 1}}} ({question.Points}分)");
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

                // 添加答题卡内容
                answerSheetBuilder.AppendLine($"\\textbf{{题目 {i + 1}}} \\underline{{\\hspace{{5cm}}}} ({question.Points}分)");
                answerSheetBuilder.AppendLine();
            }

            contentBuilder.AppendLine();
        }

        // 生成布局元素
        var layoutElements = GenerateLayoutElements(template, totalPoints);

        // 替换模板占位符
        var result = templateContent
            .Replace("{EXAM_TITLE}", EscapeLaTeX(template.Name))
            .Replace("{SUBJECT}", EscapeLaTeX(template.Description))
            .Replace("{EXAM_TIME}", "120") // 默认120分钟
            .Replace("{TOTAL_POINTS}", totalPoints.ToString())
            .Replace("{HEADER_CONTENT}", EscapeLaTeX(template.HeaderContent))
            .Replace("{FOOTER_CONTENT}", EscapeLaTeX(template.FooterContent))
            .Replace("{CONTENT}", contentBuilder.ToString())
            .Replace("{ANSWER_SHEET_CONTENT}", answerSheetBuilder.ToString())
            .Replace("{LAYOUT_ELEMENTS}", layoutElements);

        return result;
    }

    /// <summary>
    /// 生成布局元素
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="totalPoints">总分</param>
    /// <returns>布局元素LaTeX代码</returns>
    private string GenerateLayoutElements(ExamTemplate template, decimal totalPoints)
    {
        var layoutBuilder = new StringBuilder();

        // 添加密封线命令定义
        layoutBuilder.AppendLine(_sealLineLayout.GenerateSealLineCommandDefinition());

        // 添加页眉页脚设置
        layoutBuilder.AppendLine(_headerFooterLayout.GenerateHeaderFooterSetup());

        // 添加抬头（第一页）
        layoutBuilder.AppendLine(_headerLayout.GenerateHeader(template, 120, totalPoints, 1));

        // 添加密封线（第一页）
        layoutBuilder.AppendLine(_sealLineLayout.GenerateSealLine(template, 1, 1));

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 生成多页试卷内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <param name="questionsPerPage">每页题目数量</param>
    /// <returns>多页试卷内容</returns>
    public string GenerateMultiPageTemplateContent(ExamTemplate template, List<Question> questions, int questionsPerPage = 5)
    {
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

        // 获取模板文件路径
        if (!_templatePaths.TryGetValue(template.Style, out var templatePath))
        {
            throw new NotSupportedException($"不支持的模板样式: {template.Style}");
        }

        // 读取模板文件内容
        var templateContent = ReadTemplateFile(templatePath);
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new InvalidOperationException("无法读取模板文件内容");
        }

        // 生成布局元素
        var layoutElements = GenerateMultiPageLayoutElements(template, totalPages);

        // 分页生成题目内容
        var contentBuilder = new StringBuilder();
        var answerSheetBuilder = new StringBuilder();
        var totalPoints = 0m;

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
            foreach (var question in pageQuestions)
            {
                totalPoints += question.Points;
                
                // 添加题目
                contentBuilder.AppendLine($"\\textbf{{题目 {startIndex + 1}}} ({question.Points}分)");
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

                // 添加答题卡内容
                answerSheetBuilder.AppendLine($"\\textbf{{题目 {startIndex + 1}}} \\underline{{\\hspace{{5cm}}}} ({question.Points}分)");
                answerSheetBuilder.AppendLine();

                startIndex++;
            }
        }

        // 替换模板占位符
        var result = templateContent
            .Replace("{EXAM_TITLE}", EscapeLaTeX(template.Name))
            .Replace("{SUBJECT}", EscapeLaTeX(template.Description))
            .Replace("{EXAM_TIME}", "120") // 默认120分钟
            .Replace("{TOTAL_POINTS}", totalPoints.ToString())
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
    /// <param name="totalPages">总页数</param>
    /// <returns>多页布局元素LaTeX代码</returns>
    private string GenerateMultiPageLayoutElements(ExamTemplate template, int totalPages)
    {
        var layoutBuilder = new StringBuilder();
        var totalPoints = template.Sections.Sum(s => s.TotalPoints);

        // 添加密封线命令定义
        layoutBuilder.AppendLine(_sealLineLayout.GenerateSealLineCommandDefinition());

        // 添加页眉页脚设置
        layoutBuilder.AppendLine(_headerFooterLayout.GenerateHeaderFooterSetup());

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
                layoutBuilder.AppendLine(_headerLayout.GenerateHeader(template, 120, totalPoints, page));
            }

            // 添加密封线
            layoutBuilder.AppendLine(_sealLineLayout.GenerateSealLine(template, page, totalPages));

            // 添加页眉页脚
            layoutBuilder.AppendLine(_headerFooterLayout.GenerateHeaderFooter(template, template.Name, template.Description, page, totalPages));
        }

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 读取模板文件内容
    /// </summary>
    /// <param name="templatePath">模板文件路径</param>
    /// <returns>模板文件内容</returns>
    private string ReadTemplateFile(string templatePath)
    {
        try
        {
            // 在实际应用中，这里应该从文件系统读取模板文件
            // 这里为了演示，我们使用硬编码的模板内容
            if (templatePath.Contains("BasicExamTemplate"))
            {
                return System.IO.File.ReadAllText("src/QuizForge.Infrastructure/Templates/BasicExamTemplate.tex");
            }
            else if (templatePath.Contains("AdvancedExamTemplate"))
            {
                return System.IO.File.ReadAllText("src/QuizForge.Infrastructure/Templates/AdvancedExamTemplate.tex");
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
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