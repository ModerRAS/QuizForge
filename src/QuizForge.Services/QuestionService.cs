using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Data.Repositories;
using QuizForge.Core.Interfaces;
using QuizForge.Infrastructure.Parsers;

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
}