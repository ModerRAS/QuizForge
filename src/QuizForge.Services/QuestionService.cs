using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Data.Repositories;
using QuizForge.Core.Interfaces;
using QuizForge.Infrastructure.Parsers;
using System.Diagnostics.CodeAnalysis;

namespace QuizForge.Services;

/// <summary>
/// 题库服务实现
/// </summary>
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuestionProcessor _questionProcessor;
    private readonly IMarkdownParser _markdownParser;
    private readonly IExcelParser _excelParser;

    public QuestionService(
        IQuestionRepository questionRepository,
        IQuestionProcessor questionProcessor,
        IMarkdownParser markdownParser,
        IExcelParser excelParser)
    {
        _questionRepository = questionRepository;
        _questionProcessor = questionProcessor;
        _markdownParser = markdownParser;
        _excelParser = excelParser;
    }

    /// <summary>
    /// 导入题库
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">题库格式</param>
    /// <returns>导入的题库</returns>
    public async Task<QuestionBank> ImportQuestionBankAsync(string filePath, QuestionBankFormat format)
    {
        QuestionBank questionBank;
        
        if (format == QuestionBankFormat.Markdown)
        {
            questionBank = await _markdownParser.ParseAsync(filePath);
        }
        else if (format == QuestionBankFormat.Excel)
        {
            questionBank = await _excelParser.ParseAsync(filePath);
        }
        else
        {
            throw new NotSupportedException($"不支持的题库格式: {format}");
        }
        
        // 处理题库数据
        questionBank = _questionProcessor.ProcessQuestionBank(questionBank);
        
        // 保存到数据存储
        return await _questionRepository.AddAsync(questionBank);
    }

    /// <summary>
    /// 获取题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>题库数据</returns>
    public async Task<QuestionBank?> GetQuestionBankAsync(Guid id)
    {
        return await _questionRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// 获取所有题库
    /// </summary>
    /// <returns>题库列表</returns>
    public async Task<List<QuestionBank>> GetAllQuestionBanksAsync()
    {
        return await _questionRepository.GetAllAsync();
    }

    /// <summary>
    /// 更新题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>更新后的题库数据</returns>
    public async Task<QuestionBank> UpdateQuestionBankAsync(QuestionBank questionBank)
    {
        // 验证题库数据
        if (!_questionProcessor.ValidateQuestionBank(questionBank))
        {
            throw new InvalidOperationException("题库数据验证失败");
        }
        
        // 处理题库数据
        questionBank = _questionProcessor.ProcessQuestionBank(questionBank);
        
        // 更新到数据存储
        return await _questionRepository.UpdateAsync(questionBank);
    }

    /// <summary>
    /// 删除题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>删除结果</returns>
    public async Task<bool> DeleteQuestionBankAsync(Guid id)
    {
        return await _questionRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 根据类别获取题目
    /// </summary>
    /// <param name="category">类别</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByCategoryAsync(string category)
    {
        return await _questionRepository.GetQuestionsByCategoryAsync(category);
    }

    /// <summary>
    /// 根据难度获取题目
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByDifficultyAsync(string difficulty)
    {
        return await _questionRepository.GetQuestionsByDifficultyAsync(difficulty);
    }

    /// <summary>
    /// 随机获取题目
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="category">类别（可选）</param>
    /// <param name="difficulty">难度（可选）</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetRandomQuestionsAsync(int count, string? category = null, string? difficulty = null)
    {
        try
        {
            // 验证参数
            if (count <= 0)
            {
                throw new ArgumentException("题目数量必须大于0", nameof(count));
            }

            // 获取所有题目
            var allQuestions = await _questionRepository.GetAllQuestionsAsync();
            
            if (allQuestions.Count == 0)
            {
                return new List<Question>();
            }

            // 创建临时题库用于随机选择
            // 简化实现：直接使用所有题目创建一个临时题库
            var tempQuestionBank = new QuestionBank
            {
                Id = Guid.NewGuid(),
                Name = "临时题库",
                Description = "用于随机选择的临时题库",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Questions = allQuestions
            };

            // 使用QuestionBankProcessor进行随机选择
            var randomQuestions = _questionProcessor.GetRandomQuestions(count, tempQuestionBank, category, difficulty);
            
            return randomQuestions;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"随机获取题目失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 导出题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">题库格式</param>
    /// <returns>导出结果</returns>
    public async Task<bool> ExportQuestionBankAsync(QuestionBank questionBank, string filePath, QuestionBankFormat format)
    {
        try
        {
            // 验证参数
            if (questionBank == null)
            {
                throw new ArgumentNullException(nameof(questionBank));
            }
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("文件路径不能为空", nameof(filePath));
            }
            
            if (questionBank.Questions.Count == 0)
            {
                throw new ArgumentException("题库中没有题目", nameof(questionBank));
            }
            
            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // 根据格式导出
            if (format == QuestionBankFormat.Markdown)
            {
                // 调用Markdown解析器的导出方法
                return await _markdownParser.ExportAsync(questionBank, filePath);
            }
            else if (format == QuestionBankFormat.Excel)
            {
                // 调用Excel解析器的导出方法
                return await _excelParser.ExportAsync(questionBank, filePath);
            }
            else
            {
                throw new NotSupportedException($"不支持的导出格式: {format}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"导出题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有类别
    /// </summary>
    /// <returns>类别列表</returns>
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        try
        {
            // 从数据库获取所有类别
            var categories = await _questionRepository.GetAllCategoriesAsync();
            return categories.Distinct().OrderBy(c => c).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"获取所有类别失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 创建题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <returns>创建后的题目数据</returns>
    public async Task<Question> CreateQuestionAsync(Question question)
    {
        try
        {
            // 验证题目数据
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }
            
            if (string.IsNullOrWhiteSpace(question.Content))
            {
                throw new ArgumentException("题目内容不能为空", nameof(question));
            }
            
            if (question.Type == "MultipleChoice" && 
                (question.Options == null || question.Options.Count == 0))
            {
                throw new ArgumentException("选择题必须包含选项", nameof(question));
            }
            
            // 验证并处理题目
            question = _questionProcessor.ValidateAndProcessQuestion(question);
            
            // 保存到数据库
            return await _questionRepository.AddQuestionAsync(question);
        }
        catch (Exception ex)
        {
            throw new Exception($"创建题目失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <returns>更新后的题目数据</returns>
    public async Task<Question> UpdateQuestionAsync(Question question)
    {
        try
        {
            // 验证题目数据
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }
            
            if (question.Id == Guid.Empty)
            {
                throw new ArgumentException("题目ID不能为空", nameof(question));
            }
            
            if (string.IsNullOrWhiteSpace(question.Content))
            {
                throw new ArgumentException("题目内容不能为空", nameof(question));
            }
            
            if (question.Type == "MultipleChoice" && 
                (question.Options == null || question.Options.Count == 0))
            {
                throw new ArgumentException("选择题必须包含选项", nameof(question));
            }
            
            // 验证并处理题目
            question = _questionProcessor.ValidateAndProcessQuestion(question);
            
            // 更新到数据库
            return await _questionRepository.UpdateQuestionAsync(question);
        }
        catch (Exception ex)
        {
            throw new Exception($"更新题目失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>删除结果</returns>
    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        try
        {
            // 验证参数
            if (id == Guid.Empty)
            {
                throw new ArgumentException("题目ID不能为空", nameof(id));
            }
            
            // 从数据库删除题目
            return await _questionRepository.DeleteQuestionAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"删除题目失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 创建题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>创建后的题库数据</returns>
    public async Task<QuestionBank> CreateQuestionBankAsync(QuestionBank questionBank)
    {
        try
        {
            // 验证题库数据
            if (questionBank == null)
            {
                throw new ArgumentNullException(nameof(questionBank));
            }
            
            if (string.IsNullOrWhiteSpace(questionBank.Name))
            {
                throw new ArgumentException("题库名称不能为空", nameof(questionBank));
            }
            
            // 验证并处理题库
            if (!_questionProcessor.ValidateQuestionBank(questionBank))
            {
                throw new InvalidOperationException("题库数据验证失败");
            }
            
            questionBank = _questionProcessor.ProcessQuestionBank(questionBank);
            
            // 保存到数据库
            return await _questionRepository.AddAsync(questionBank);
        }
        catch (Exception ex)
        {
            throw new Exception($"创建题库失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 根据题库ID获取题目
    /// </summary>
    /// <param name="questionBankId">题库ID</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByQuestionBankIdAsync(Guid questionBankId)
    {
        try
        {
            // 验证参数
            if (questionBankId == Guid.Empty)
            {
                throw new ArgumentException("题库ID不能为空", nameof(questionBankId));
            }
            
            // 从数据库获取题目
            return await _questionRepository.GetQuestionsByBankIdAsync(questionBankId);
        }
        catch (Exception ex)
        {
            throw new Exception($"根据题库ID获取题目失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 导入题目到题库
    /// </summary>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="questions">题目列表</param>
    /// <returns>导入结果</returns>
    public async Task<bool> ImportQuestionsAsync(Guid questionBankId, List<Question> questions)
    {
        try
        {
            // 验证参数
            if (questionBankId == Guid.Empty)
            {
                throw new ArgumentException("题库ID不能为空", nameof(questionBankId));
            }
            
            if (questions == null || questions.Count == 0)
            {
                throw new ArgumentException("题目列表不能为空", nameof(questions));
            }
            
            // 验证题库是否存在
            var questionBank = await _questionRepository.GetByIdAsync(questionBankId);
            if (questionBank == null)
            {
                throw new Exception($"未找到ID为 {questionBankId} 的题库");
            }
            
            // 处理每个题目
            var processedQuestions = new List<Question>();
            foreach (var question in questions)
            {
                // 验证并处理题目
                var processedQuestion = _questionProcessor.ValidateAndProcessQuestion(question);
                processedQuestions.Add(processedQuestion);
            }
            
            // 批量添加到题库
            return await _questionRepository.AddQuestionsToBankAsync(questionBankId, processedQuestions);
        }
        catch (Exception ex)
        {
            throw new Exception($"导入题目到题库失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 搜索题目
    /// </summary>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="searchText">搜索文本</param>
    /// <returns>匹配的题目列表</returns>
    public async Task<List<Question>> SearchQuestionsAsync(Guid questionBankId, string searchText)
    {
        try
        {
            // 验证参数
            if (questionBankId == Guid.Empty)
            {
                throw new ArgumentException("题库ID不能为空", nameof(questionBankId));
            }
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("搜索文本不能为空", nameof(searchText));
            }
            
            // 在数据库中搜索题目
            return await _questionRepository.SearchQuestionsAsync(questionBankId, searchText);
        }
        catch (Exception ex)
        {
            throw new Exception($"搜索题目失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        try
        {
            // 从数据库获取所有题目
            return await _questionRepository.GetAllQuestionsAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"获取所有题目失败: {ex.Message}", ex);
        }
    }
}