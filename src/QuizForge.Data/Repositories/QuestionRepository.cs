using Microsoft.EntityFrameworkCore;
using QuizForge.Data.Contexts;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Data.Repositories;

/// <summary>
/// 题库数据访问实现
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly QuizDbContext _context;

    /// <summary>
    /// 初始化题库仓库
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public QuestionRepository(QuizDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 根据ID获取题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>题库数据</returns>
    public async Task<QuestionBank?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.QuestionBanks
                .Include(b => b.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有题库
    /// </summary>
    /// <returns>题库列表</returns>
    public async Task<List<QuestionBank>> GetAllAsync()
    {
        try
        {
            return await _context.QuestionBanks
                .Include(b => b.Questions)
                .ThenInclude(q => q.Options)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取所有题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 添加题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>添加后的题库数据</returns>
    public async Task<QuestionBank> AddAsync(QuestionBank questionBank)
    {
        try
        {
            if (questionBank.Id == Guid.Empty)
            {
                questionBank.Id = Guid.NewGuid();
            }

            questionBank.CreatedAt = DateTime.UtcNow;
            questionBank.UpdatedAt = DateTime.UtcNow;

            _context.QuestionBanks.Add(questionBank);
            await _context.SaveChangesAsync();

            return questionBank;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"添加题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>更新后的题库数据</returns>
    public async Task<QuestionBank> UpdateAsync(QuestionBank questionBank)
    {
        try
        {
            var existingBank = await _context.QuestionBanks
                .Include(b => b.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(b => b.Id == questionBank.Id);

            if (existingBank == null)
            {
                throw new Exception($"未找到ID为 {questionBank.Id} 的题库");
            }

            // 更新基本信息
            existingBank.Name = questionBank.Name;
            existingBank.Description = questionBank.Description;
            existingBank.UpdatedAt = DateTime.UtcNow;

            // 处理题目更新
            // 先删除不存在的题目
            var existingQuestionIds = existingBank.Questions.Select(q => q.Id).ToList();
            var newQuestionIds = questionBank.Questions.Select(q => q.Id).ToList();
            var questionsToDelete = existingQuestionIds.Except(newQuestionIds).ToList();

            foreach (var questionId in questionsToDelete)
            {
                var questionToDelete = existingBank.Questions.FirstOrDefault(q => q.Id == questionId);
                if (questionToDelete != null)
                {
                    existingBank.Questions.Remove(questionToDelete);
                }
            }

            // 添加或更新题目
            foreach (var question in questionBank.Questions)
            {
                var existingQuestion = existingBank.Questions.FirstOrDefault(q => q.Id == question.Id);
                
                if (existingQuestion == null)
                {
                    // 新题目
                    if (question.Id == Guid.Empty)
                    {
                        question.Id = Guid.NewGuid();
                    }
                    existingBank.Questions.Add(question);
                }
                else
                {
                    // 更新现有题目
                    existingQuestion.Type = question.Type;
                    existingQuestion.Content = question.Content;
                    existingQuestion.Difficulty = question.Difficulty;
                    existingQuestion.Category = question.Category;
                    existingQuestion.CorrectAnswer = question.CorrectAnswer;
                    existingQuestion.Explanation = question.Explanation;
                    existingQuestion.Points = question.Points;

                    // 处理选项更新
                    var existingOptionIds = existingQuestion.Options.Select(o => o.Id).ToList();
                    var newOptionIds = question.Options.Select(o => o.Id).ToList();
                    var optionsToDelete = existingOptionIds.Except(newOptionIds).ToList();

                    foreach (var optionId in optionsToDelete)
                    {
                        var optionToDelete = existingQuestion.Options.FirstOrDefault(o => o.Id == optionId);
                        if (optionToDelete != null)
                        {
                            existingQuestion.Options.Remove(optionToDelete);
                        }
                    }

                    foreach (var option in question.Options)
                    {
                        var existingOption = existingQuestion.Options.FirstOrDefault(o => o.Id == option.Id);
                        
                        if (existingOption == null)
                        {
                            // 新选项
                            if (option.Id == Guid.Empty)
                            {
                                option.Id = Guid.NewGuid();
                            }
                            existingQuestion.Options.Add(option);
                        }
                        else
                        {
                            // 更新现有选项
                            existingOption.Key = option.Key;
                            existingOption.Value = option.Value;
                            existingOption.IsCorrect = option.IsCorrect;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return existingBank;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"更新题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>删除结果</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var questionBank = await _context.QuestionBanks
                .Include(b => b.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (questionBank == null)
            {
                return false;
            }

            _context.QuestionBanks.Remove(questionBank);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"删除题库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据题库ID获取题目
    /// </summary>
    /// <param name="bankId">题库ID</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByBankIdAsync(Guid bankId)
    {
        try
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => EF.Property<Guid>(q, "QuestionBankId") == bankId)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.Difficulty)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"根据题库ID获取题目失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据类别获取题目
    /// </summary>
    /// <param name="category">类别</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByCategoryAsync(string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("类别不能为空", nameof(category));
            }

            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.Category == category)
                .OrderBy(q => q.Difficulty)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"根据类别获取题目失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据难度获取题目
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetQuestionsByDifficultyAsync(string difficulty)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(difficulty))
            {
                throw new ArgumentException("难度不能为空", nameof(difficulty));
            }

            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.Difficulty == difficulty)
                .OrderBy(q => q.Category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"根据难度获取题目失败: {ex.Message}", ex);
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
            return await _context.Questions
                .Include(q => q.Options)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.Difficulty)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取所有题目失败: {ex.Message}", ex);
        }
    }
}