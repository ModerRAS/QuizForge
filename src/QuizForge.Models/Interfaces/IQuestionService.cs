using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 题库服务接口
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// 导入题库
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">题库格式</param>
    /// <returns>导入的题库</returns>
    Task<QuestionBank> ImportQuestionBankAsync(string filePath, QuestionBankFormat format);
    
    /// <summary>
    /// 获取题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>题库数据</returns>
    Task<QuestionBank?> GetQuestionBankAsync(Guid id);
    
    /// <summary>
    /// 获取所有题库
    /// </summary>
    /// <returns>题库列表</returns>
    Task<List<QuestionBank>> GetAllQuestionBanksAsync();
    
    /// <summary>
    /// 更新题库
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>更新后的题库数据</returns>
    Task<QuestionBank> UpdateQuestionBankAsync(QuestionBank questionBank);
    
    /// <summary>
    /// 删除题库
    /// </summary>
    /// <param name="id">题库ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteQuestionBankAsync(Guid id);
    
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
    /// 随机获取题目
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="category">类别（可选）</param>
    /// <param name="difficulty">难度（可选）</param>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetRandomQuestionsAsync(int count, string? category = null, string? difficulty = null);
}