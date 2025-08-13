using QuizForge.Models;
using QuizForge.Core.Layout;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 动态内容插入器，处理动态内容插入到LaTeX模板
/// </summary>
public class DynamicContentInserter
{
    private readonly Dictionary<string, Func<ExamTemplate, List<Question>, string>> _placeholderHandlers;
    private readonly SealLineLayout _sealLineLayout;
    private readonly HeaderLayout _headerLayout;
    private readonly HeaderFooterLayout _headerFooterLayout;

    public DynamicContentInserter()
    {
        _placeholderHandlers = new Dictionary<string, Func<ExamTemplate, List<Question>, string>>
        {
            { "{EXAM_TITLE}", (template, questions) => EscapeLaTeX(template.Name) },
            { "{SUBJECT}", (template, questions) => EscapeLaTeX(template.Description) },
            { "{EXAM_TIME}", (template, questions) => "120" }, // 默认120分钟
            { "{TOTAL_POINTS}", (template, questions) => questions.Sum(q => q.Points).ToString() },
            { "{HEADER_CONTENT}", (template, questions) => GenerateHeaderContent(template, questions) },
            { "{FOOTER_CONTENT}", (template, questions) => GenerateFooterContent(template, questions) },
            { "{HEADER_FOOTER_SETUP}", (template, questions) => GenerateHeaderFooterSetup(template, questions) },
            { "{CONTENT}", (template, questions) => GenerateContent(template, questions) },
            { "{ANSWER_SHEET_CONTENT}", (template, questions) => GenerateAnswerSheetContent(questions) },
            { "{LAYOUT_ELEMENTS}", (template, questions) => GenerateLayoutElements(template, questions) },
            { "{HEADER_SECTION}", (template, questions) => GenerateHeaderSection(template, questions) },
            { "{SEAL_LINE_COMMANDS}", (template, questions) => GenerateSealLineCommandDefinition() },
            { "{SEAL_LINE_CONTENT}", (template, questions) => GenerateSealLine(template, 1, 1) },
            { "{ANSWER_SHEET_COMMANDS}", (template, questions) => GenerateAnswerSheetCommandDefinition() }
        };
        
        _sealLineLayout = new SealLineLayout();
        _headerLayout = new HeaderLayout();
        _headerFooterLayout = new HeaderFooterLayout();
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

        // 添加页眉页脚设置
        layoutBuilder.AppendLine(GenerateHeaderFooterSetup(template, questions));

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 生成页眉内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>页眉内容的LaTeX代码</returns>
    private string GenerateHeaderContent(ExamTemplate template, List<Question> questions)
    {
        // 如果有HeaderConfig，使用新的方法
        if (template.HeaderConfig != null)
        {
            var totalPoints = questions.Sum(q => q.Points);
            
            // 确保HeaderConfig中的基本信息已设置
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.ExamTitle))
            {
                template.HeaderConfig.ExamTitle = template.Name;
            }
            
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.Subject))
            {
                template.HeaderConfig.Subject = template.Description;
            }
            
            if (template.HeaderConfig.ExamTime == 0)
            {
                template.HeaderConfig.ExamTime = 120;
            }
            
            if (template.HeaderConfig.TotalPoints == 0)
            {
                template.HeaderConfig.TotalPoints = totalPoints;
            }
            
            return _headerFooterLayout.GenerateHeader(template.HeaderConfig, template.HeaderConfig.ExamTitle, template.HeaderConfig.Subject, 1);
        }
        else
        {
            // 使用向后兼容的方法
            return EscapeLaTeX(template.HeaderContent);
        }
    }

    /// <summary>
    /// 生成页脚内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>页脚内容的LaTeX代码</returns>
    private string GenerateFooterContent(ExamTemplate template, List<Question> questions)
    {
        // 如果有HeaderConfig，使用新的方法
        if (template.HeaderConfig != null)
        {
            var totalPoints = questions.Sum(q => q.Points);
            
            // 确保HeaderConfig中的基本信息已设置
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.ExamTitle))
            {
                template.HeaderConfig.ExamTitle = template.Name;
            }
            
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.Subject))
            {
                template.HeaderConfig.Subject = template.Description;
            }
            
            if (template.HeaderConfig.ExamTime == 0)
            {
                template.HeaderConfig.ExamTime = 120;
            }
            
            if (template.HeaderConfig.TotalPoints == 0)
            {
                template.HeaderConfig.TotalPoints = totalPoints;
            }
            
            return _headerFooterLayout.GenerateFooter(template.HeaderConfig, 1, 1);
        }
        else
        {
            // 使用向后兼容的方法
            return EscapeLaTeX(template.FooterContent);
        }
    }

    /// <summary>
    /// 生成页眉页脚设置
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>页眉页脚设置的LaTeX代码</returns>
    private string GenerateHeaderFooterSetup(ExamTemplate template, List<Question> questions)
    {
        // 如果有HeaderConfig，使用新的方法
        if (template.HeaderConfig != null)
        {
            var totalPoints = questions.Sum(q => q.Points);
            
            // 确保HeaderConfig中的基本信息已设置
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.ExamTitle))
            {
                template.HeaderConfig.ExamTitle = template.Name;
            }
            
            if (string.IsNullOrWhiteSpace(template.HeaderConfig.Subject))
            {
                template.HeaderConfig.Subject = template.Description;
            }
            
            if (template.HeaderConfig.ExamTime == 0)
            {
                template.HeaderConfig.ExamTime = 120;
            }
            
            if (template.HeaderConfig.TotalPoints == 0)
            {
                template.HeaderConfig.TotalPoints = totalPoints;
            }
            
            return _headerFooterLayout.GenerateHeaderFooterSetup();
        }
        else
        {
            // 使用向后兼容的方法
            return @"
% 页眉页脚设置
\pagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0.4pt}
\renewcommand{\footrulewidth}{0.4pt}";
        }
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
            .Replace("{HEADER_CONTENT}", GenerateHeaderContent(template, questions))
            .Replace("{FOOTER_CONTENT}", GenerateFooterContent(template, questions))
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
        layoutBuilder.AppendLine(GenerateHeaderFooterSetup(template, questions));

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
                // 使用HeaderConfig生成抬头
                if (template.HeaderConfig != null)
                {
                    // 确保HeaderConfig中的基本信息已设置
                    if (string.IsNullOrWhiteSpace(template.HeaderConfig.ExamTitle))
                    {
                        template.HeaderConfig.ExamTitle = template.Name;
                    }
                    
                    if (string.IsNullOrWhiteSpace(template.HeaderConfig.Subject))
                    {
                        template.HeaderConfig.Subject = template.Description;
                    }
                    
                    if (template.HeaderConfig.ExamTime == 0)
                    {
                        template.HeaderConfig.ExamTime = 120;
                    }
                    
                    if (template.HeaderConfig.TotalPoints == 0)
                    {
                        template.HeaderConfig.TotalPoints = totalPoints;
                    }
                    
                    layoutBuilder.AppendLine(_headerLayout.GenerateHeader(template.HeaderConfig, page));
                }
                else
                {
                    // 使用向后兼容的方法生成抬头
                    layoutBuilder.AppendLine(GenerateHeader(template, 120, totalPoints, page));
                }
            }

            // 添加密封线
            layoutBuilder.AppendLine(GenerateSealLine(template, page, totalPages));

            // 添加页眉页脚
            if (template.HeaderConfig != null)
            {
                layoutBuilder.AppendLine(_headerFooterLayout.GenerateHeaderFooter(template.HeaderConfig, template.HeaderConfig.ExamTitle, template.HeaderConfig.Subject, page, totalPages));
            }
            else
            {
                layoutBuilder.AppendLine(GenerateHeaderFooter(template, template.Name, template.Description, page, totalPages));
            }
        }

        return layoutBuilder.ToString();
    }

    /// <summary>
    /// 生成密封线命令定义
    /// </summary>
    /// <returns>密封线命令定义的LaTeX代码</returns>
    private string GenerateSealLineCommandDefinition()
    {
        return _sealLineLayout.GenerateSealLineCommandDefinition();
    }

    /// <summary>
    /// 生成页眉页脚设置
    /// </summary>
    /// <returns>页眉页脚设置的LaTeX代码</returns>
    private string GenerateHeaderFooterSetup()
    {
        return @”
% 页眉页脚设置
\pagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0.4pt}
\renewcommand{\footrulewidth}{0.4pt}";
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
        return _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages);
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
\\fancyfoot[C]{{第{pageNumber}页/共{totalPages}页}}";
    }

    /// <summary>
    /// 生成答题卡命令定义
    /// </summary>
    /// <returns>答题卡命令定义的LaTeX代码</returns>
    private string GenerateAnswerSheetCommandDefinition()
    {
        return @"
% 答题卡命令
\newcommand{\answersheet}[1]{
  \begin{center}
    \textbf{答题卡}
  \end{center}
  
  \vspace{0.5cm}
  
  \begin{tabular}{|ll|}
    \hline
    姓名：\underline{\hspace{3cm}} & 考号：\underline{\hspace{3cm}} \\
    \hline
    班级：\underline{\hspace{3cm}} & 日期：\underline{\hspace{3cm}} \\
    \hline
  \end{tabular}
  
  \vspace{1cm}
  
  #1
}";
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