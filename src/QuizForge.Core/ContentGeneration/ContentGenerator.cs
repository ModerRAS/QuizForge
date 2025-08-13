using QuizForge.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 内容生成器，负责将题库内容转换为LaTeX格式
/// </summary>
public class ContentGenerator
{
    private readonly LaTeXGenerationConfig _config;
    private readonly MathFormulaProcessor _mathFormulaProcessor;
    
    /// <summary>
    /// 内容生成器构造函数
    /// </summary>
    /// <param name="config">LaTeX生成配置</param>
    /// <param name="mathFormulaProcessor">数学公式处理器</param>
    public ContentGenerator(LaTeXGenerationConfig config, MathFormulaProcessor mathFormulaProcessor)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _mathFormulaProcessor = mathFormulaProcessor ?? throw new ArgumentNullException(nameof(mathFormulaProcessor));
    }
    
    /// <summary>
    /// 默认构造函数，使用默认配置
    /// </summary>
    public ContentGenerator() : this(new LaTeXGenerationConfig(), new MathFormulaProcessor())
    {
    }
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加题目
            var title = string.Format(_config.QuestionNumberFormat, questionNumber);
            contentBuilder.AppendLine($"\\textbf{{{title}}}");
            
            if (_config.ShowPoints)
            {
                contentBuilder.AppendLine($"({question.Points}分)");
            }
            
            if (_config.ShowDifficulty && !string.IsNullOrWhiteSpace(question.Difficulty))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Difficulty)}]");
            }
            
            if (_config.ShowCategory && !string.IsNullOrWhiteSpace(question.Category))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Category)}]");
            }
            
            // 处理题目内容，包括数学公式
            var content = _mathFormulaProcessor.ProcessMathFormulas(question.Content);
            contentBuilder.AppendLine(EscapeLaTeX(content));
            contentBuilder.AppendLine();

            // 添加选项
            if (question.Options != null && question.Options.Count > 0)
            {
                // 判断是否为多选题
                var isMultipleChoice = IsMultipleChoiceQuestion(question);
                
                if (isMultipleChoice)
                {
                    contentBuilder.AppendLine("\\textbf{（多选题）}");
                }
                
                contentBuilder.AppendLine("\\begin{enumerate}");
                foreach (var option in question.Options)
                {
                    // 处理选项内容，包括数学公式
                    var optionContent = _mathFormulaProcessor.ProcessMathFormulas(option.Value);
                    var optionText = string.Format(_config.OptionFormat, EscapeLaTeX(optionContent));
                    contentBuilder.AppendLine(optionText);
                }
                contentBuilder.AppendLine("\\end{enumerate}");
                contentBuilder.AppendLine();
            }

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成选择题LaTeX内容失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 判断是否为多选题
    /// </summary>
    /// <param name="question">题目</param>
    /// <returns>是否为多选题</returns>
    private bool IsMultipleChoiceQuestion(Question question)
    {
        if (question == null || string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            return false;
        }
        
        // 如果正确答案包含多个选项（如 "A,B" 或 "ABC"），则认为是多选题
        var correctAnswers = question.CorrectAnswer.Split(new[] { ',', ' ', ';', '、' }, StringSplitOptions.RemoveEmptyEntries);
        return correctAnswers.Length > 1;
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加题目
            var title = string.Format(_config.QuestionNumberFormat, questionNumber);
            contentBuilder.AppendLine($"\\textbf{{{title}}}");
            
            if (_config.ShowPoints)
            {
                contentBuilder.AppendLine($"({question.Points}分)");
            }
            
            if (_config.ShowDifficulty && !string.IsNullOrWhiteSpace(question.Difficulty))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Difficulty)}]");
            }
            
            if (_config.ShowCategory && !string.IsNullOrWhiteSpace(question.Category))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Category)}]");
            }
            
            // 处理填空题内容，包括数学公式和填空线
            var content = _mathFormulaProcessor.ProcessMathFormulas(question.Content);
            content = EscapeLaTeX(content);
            
            // 将下划线替换为LaTeX格式的填空线
            content = Regex.Replace(content, "_{3,}", $"\\underline{{\\hspace{{{_config.BlankLineLength}}}}}");
            
            // 处理其他可能的填空标记，如 [blank] 或 ( )
            content = Regex.Replace(content, @"\[blank\]", $"\\underline{{\\hspace{{{_config.BlankLineLength}}}}}");
            content = Regex.Replace(content, @"\(\s*\)", $"\\underline{{\\hspace{{{_config.BlankLineLength}}}}}");
            
            contentBuilder.AppendLine(content);
            contentBuilder.AppendLine();

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成填空题LaTeX内容失败: {ex.Message}", ex);
        }
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加题目
            var title = string.Format(_config.QuestionNumberFormat, questionNumber);
            contentBuilder.AppendLine($"\\textbf{{{title}}}");
            
            if (_config.ShowPoints)
            {
                contentBuilder.AppendLine($"({question.Points}分)");
            }
            
            if (_config.ShowDifficulty && !string.IsNullOrWhiteSpace(question.Difficulty))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Difficulty)}]");
            }
            
            if (_config.ShowCategory && !string.IsNullOrWhiteSpace(question.Category))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Category)}]");
            }
            
            // 处理题目内容，包括数学公式
            var content = _mathFormulaProcessor.ProcessMathFormulas(question.Content);
            contentBuilder.AppendLine(EscapeLaTeX(content));
            contentBuilder.AppendLine();
            
            // 添加答题区域
            contentBuilder.AppendLine($"\\vspace{{{_config.EssayAnswerHeight}}}");
            contentBuilder.AppendLine();

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成简答题LaTeX内容失败: {ex.Message}", ex);
        }
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加题目
            var title = string.Format(_config.QuestionNumberFormat, questionNumber);
            contentBuilder.AppendLine($"\\textbf{{{title}}}");
            
            if (_config.ShowPoints)
            {
                contentBuilder.AppendLine($"({question.Points}分)");
            }
            
            if (_config.ShowDifficulty && !string.IsNullOrWhiteSpace(question.Difficulty))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Difficulty)}]");
            }
            
            if (_config.ShowCategory && !string.IsNullOrWhiteSpace(question.Category))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Category)}]");
            }
            
            // 处理题目内容，包括数学公式
            var content = _mathFormulaProcessor.ProcessMathFormulas(question.Content);
            contentBuilder.AppendLine(EscapeLaTeX(content));
            contentBuilder.AppendLine();
            
            // 添加选项
            contentBuilder.AppendLine("\\begin{enumerate}");
            contentBuilder.AppendLine("\\item 正确");
            contentBuilder.AppendLine("\\item 错误");
            contentBuilder.AppendLine("\\end{enumerate}");
            contentBuilder.AppendLine();

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成判断题LaTeX内容失败: {ex.Message}", ex);
        }
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
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加题目
            var title = string.Format(_config.QuestionNumberFormat, questionNumber);
            contentBuilder.AppendLine($"\\textbf{{{title}}}");
            
            if (_config.ShowPoints)
            {
                contentBuilder.AppendLine($"({question.Points}分)");
            }
            
            if (_config.ShowDifficulty && !string.IsNullOrWhiteSpace(question.Difficulty))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Difficulty)}]");
            }
            
            if (_config.ShowCategory && !string.IsNullOrWhiteSpace(question.Category))
            {
                contentBuilder.AppendLine($"[{EscapeLaTeX(question.Category)}]");
            }
            
            // 处理题目内容，包括数学公式
            var content = _mathFormulaProcessor.ProcessMathFormulas(question.Content);
            contentBuilder.AppendLine(EscapeLaTeX(content));
            contentBuilder.AppendLine();

            // 添加选项（如果有）
            if (question.Options != null && question.Options.Count > 0)
            {
                contentBuilder.AppendLine("\\begin{enumerate}");
                foreach (var option in question.Options)
                {
                    var optionText = _mathFormulaProcessor.ProcessMathFormulas(option.Value);
                    contentBuilder.AppendLine($"\\item {EscapeLaTeX(optionText)}");
                }
                contentBuilder.AppendLine("\\end{enumerate}");
                contentBuilder.AppendLine();
            }

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成通用题目LaTeX内容失败: {ex.Message}", ex);
        }
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 根据题目类型生成不同的答题卡格式
            switch (question.Type)
            {
                case "选择题":
                case "判断题":
                    var mcTitle = string.Format(_config.QuestionNumberFormat, questionNumber);
                    contentBuilder.AppendLine($"\\textbf{{{mcTitle}}} \\underline{{\\hspace{{{_config.AnswerSheetChoiceWidth}}}}}}");
                    
                    if (_config.ShowPoints)
                    {
                        contentBuilder.AppendLine($"({question.Points}分)");
                    }
                    break;
                    
                case "填空题":
                    var fbTitle = string.Format(_config.QuestionNumberFormat, questionNumber);
                    contentBuilder.AppendLine($"\\textbf{{{fbTitle}}} \\underline{{\\hspace{{{_config.AnswerSheetBlankWidth}}}}}}");
                    
                    if (_config.ShowPoints)
                    {
                        contentBuilder.AppendLine($"({question.Points}分)");
                    }
                    break;
                    
                case "简答题":
                    var essayTitle = string.Format(_config.QuestionNumberFormat, questionNumber);
                    contentBuilder.AppendLine($"\\textbf{{{essayTitle}}}");
                    
                    if (_config.ShowPoints)
                    {
                        contentBuilder.AppendLine($"({question.Points}分)");
                    }
                    
                    contentBuilder.AppendLine($"\\vspace{{{_config.AnswerSheetEssayHeight}}}");
                    break;
                    
                default:
                    var genericTitle = string.Format(_config.QuestionNumberFormat, questionNumber);
                    contentBuilder.AppendLine($"\\textbf{{{genericTitle}}} \\underline{{\\hspace{{{_config.AnswerSheetBlankWidth}}}}}}");
                    
                    if (_config.ShowPoints)
                    {
                        contentBuilder.AppendLine($"({question.Points}分)");
                    }
                    break;
            }
            
            contentBuilder.AppendLine();

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成答题卡LaTeX内容失败: {ex.Message}", ex);
        }
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

        try
        {
            var contentBuilder = new StringBuilder();
            
            // 添加章节标题
            var sectionTitle = _mathFormulaProcessor.ProcessMathFormulas(section.Title);
            contentBuilder.AppendLine($"\\{_config.SectionCommand}{{{EscapeLaTeX(sectionTitle)}}}");
            
            if (!string.IsNullOrWhiteSpace(section.Instructions))
            {
                var instructions = _mathFormulaProcessor.ProcessMathFormulas(section.Instructions);
                contentBuilder.AppendLine($"\\textbf{{{EscapeLaTeX(instructions)}}}");
                contentBuilder.AppendLine();
            }

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"生成章节内容LaTeX失败: {ex.Message}", ex);
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