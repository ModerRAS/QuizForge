using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 题库仓库接口
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// 根据ID获取题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>题库数据</returns>
    Task<QuestionBank?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 获取所有题库
    /// </summary>
    /// <returns>题库列表</returns>
    Task<List<QuestionBank>> GetAllAsync();
    
    /// <summary>
    /// 添加题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>添加后的题库数据</returns>
    Task<QuestionBank> AddAsync(QuestionBank questionBank);
    
    /// <summary>
    /// 更新题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>更新后的题库数据</returns>
    Task<QuestionBank> UpdateAsync(QuestionBank questionBank);
    
    /// <summary>
    /// 删除题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 根据题库ID获取题目
    /// </summary>
    /// <param name="bankId">题库ID</param>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetQuestionsByBankIdAsync(Guid bankId);
    
    /// <summary>
    /// 根据类别获取题目
    /// </summary>
    /// <param name="category">类别</param>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetQuestionsByCategoryAsync(string category);
    
    /// <summary>
    /// 根据难度获取题目
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetQuestionsByDifficultyAsync(string difficulty);
    
    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetAllQuestionsAsync();
}