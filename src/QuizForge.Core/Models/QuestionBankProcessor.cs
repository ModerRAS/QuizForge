using QuizForge.Models;
using QuizForge.Core.Interfaces;
using QuizForge.Models.Interfaces;

namespace QuizForge.Core.Models;

/// <summary>
/// 题库处理器，负责题库的核心业务逻辑
/// </summary>
public class QuestionBankProcessor : IQuestionProcessor
{
    /// <summary>
    /// 处理题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>处理后的题库数据</returns>
    public QuestionBank ProcessQuestionBank(QuestionBank questionBank)
    {
        if (questionBank == null)
        {
            throw new ArgumentNullException(nameof(questionBank));
        }

        // 设置更新时间
        questionBank.UpdatedAt = DateTime.Now;

        // 确保所有题目都有ID
        foreach (var question in questionBank.Questions)
        {
            if (question.Id == Guid.Empty)
            {
                question.Id = Guid.NewGuid();
            }

            // 确保所有选项都有ID
            foreach (var option in question.Options)
            {
                if (option.Id == Guid.Empty)
                {
                    option.Id = Guid.NewGuid();
                }
            }
        }

        return questionBank;
    }
    
    /// <summary>
    /// 验证题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>验证结果</returns>
    public bool ValidateQuestionBank(QuestionBank questionBank)
    {
        if (questionBank == null)
        {
            return false;
        }

        // 验证基本信息
        if (string.IsNullOrWhiteSpace(questionBank.Name))
        {
            return false;
        }

        // 验证题目列表
        if (questionBank.Questions == null || questionBank.Questions.Count == 0)
        {
            return false;
        }

        // 验证每个题目
        foreach (var question in questionBank.Questions)
        {
            if (!ValidateQuestion(question))
            {
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// 验证并处理单个题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <returns>处理后的题目数据</returns>
    public Question ValidateAndProcessQuestion(Question question)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        // 验证题目
        if (!ValidateQuestion(question))
        {
            throw new ArgumentException("题目数据验证失败", nameof(question));
        }

        // 处理题目数据
        if (question.Id == Guid.Empty)
        {
            question.Id = Guid.NewGuid();
        }

        if (question.CreatedAt == default)
        {
            question.CreatedAt = DateTime.UtcNow;
        }

        question.UpdatedAt = DateTime.UtcNow;

        // 确保所有选项都有ID
        if (question.Options != null)
        {
            foreach (var option in question.Options)
            {
                if (option.Id == Guid.Empty)
                {
                    option.Id = Guid.NewGuid();
                }
            }
        }

        return question;
    }

    /// <summary>
    /// 根据类别获取题目
    /// </summary>
    /// <param name="category">类别</param>
    /// <param name="questionBank">题库</param>
    /// <returns>题目列表</returns>
    public List<Question> GetQuestionsByCategory(string category, QuestionBank questionBank)
    {
        if (questionBank == null || questionBank.Questions == null)
        {
            return new List<Question>();
        }

        return questionBank.Questions
            .Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 根据难度获取题目
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <param name="questionBank">题库</param>
    /// <returns>题目列表</returns>
    public List<Question> GetQuestionsByDifficulty(string difficulty, QuestionBank questionBank)
    {
        if (questionBank == null || questionBank.Questions == null)
        {
            return new List<Question>();
        }

        return questionBank.Questions
            .Where(q => q.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 随机获取题目
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="questionBank">题库</param>
    /// <param name="category">类别（可选）</param>
    /// <param name="difficulty">难度（可选）</param>
    /// <returns>题目列表</returns>
    public List<Question> GetRandomQuestions(int count, QuestionBank questionBank, string? category = null, string? difficulty = null)
    {
        if (questionBank == null || questionBank.Questions == null || count <= 0)
        {
            return new List<Question>();
        }

        var questions = questionBank.Questions.AsEnumerable();

        // 按类别筛选
        if (!string.IsNullOrWhiteSpace(category))
        {
            questions = questions.Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        // 按难度筛选
        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            questions = questions.Where(q => q.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
        }

        // 随机排序并取指定数量
        return questions
            .OrderBy(q => Guid.NewGuid())
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// 验证单个题目
    /// </summary>
    /// <param name="question">题目</param>
    /// <returns>验证结果</returns>
    private bool ValidateQuestion(Question question)
    {
        if (question == null)
        {
            return false;
        }

        // 验证基本信息
        if (string.IsNullOrWhiteSpace(question.Type) ||
            string.IsNullOrWhiteSpace(question.Content) ||
            string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            return false;
        }

        // 如果是选择题，验证选项
        if (question.Type == "选择题")
        {
            if (question.Options == null || question.Options.Count < 2)
            {
                return false;
            }

            // 验证是否有正确答案
            var hasCorrectAnswer = question.Options.Any(o => o.IsCorrect) ||
                                  question.Options.Any(o => o.Key.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
            
            if (!hasCorrectAnswer)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 从文件加载题库
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">题库格式</param>
    /// <param name="markdownParser">Markdown解析器</param>
    /// <param name="excelParser">Excel解析器</param>
    /// <returns>处理后的题库数据</returns>
    public async Task<QuestionBank> LoadQuestionBankFromFileAsync(
        string filePath,
        QuestionBankFormat format,
        IMarkdownParser markdownParser,
        IExcelParser excelParser)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        QuestionBank questionBank;

        switch (format)
        {
            case QuestionBankFormat.Markdown:
                questionBank = await markdownParser.ParseAsync(filePath);
                break;
            case QuestionBankFormat.Excel:
                questionBank = await excelParser.ParseAsync(filePath);
                break;
            default:
                throw new NotSupportedException($"不支持的题库格式: {format}");
        }

        // 处理题库数据
        return ProcessQuestionBank(questionBank);
    }

    /// <summary>
    /// 验证题库文件格式
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">题库格式</param>
    /// <param name="markdownParser">Markdown解析器</param>
    /// <param name="excelParser">Excel解析器</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateQuestionBankFileAsync(
        string filePath,
        QuestionBankFormat format,
        IMarkdownParser markdownParser,
        IExcelParser excelParser)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            switch (format)
            {
                case QuestionBankFormat.Markdown:
                    return await markdownParser.ValidateFormatAsync(filePath);
                case QuestionBankFormat.Excel:
                    return await excelParser.ValidateFormatAsync(filePath);
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
}