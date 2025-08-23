using QuizForge.Models;

namespace QuizForge.Core.Interfaces;

/// <summary>
/// 题库处理器接口
/// </summary>
public interface IQuestionProcessor
{
    /// <summary>
    /// 处理题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>处理后的题库数据</returns>
    QuestionBank ProcessQuestionBank(QuestionBank questionBank);
    
    /// <summary>
    /// 验证题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>验证结果</returns>
    bool ValidateQuestionBank(QuestionBank questionBank);
    
    /// <summary>
    /// 验证并处理单个题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <returns>处理后的题目数据</returns>
    Question ValidateAndProcessQuestion(Question question);
    
    /// <summary>
    /// 根据类别获取题目
    /// </summary>
    /// <param name="category">类别</param>
    /// <param name="questionBank">题库</param>
    /// <returns>题目列表</returns>
    List<Question> GetQuestionsByCategory(string category, QuestionBank questionBank);
    
    /// <summary>
    /// 根据难度获取题目
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <param name="questionBank">题库</param>
    /// <returns>题目列表</returns>
    List<Question> GetQuestionsByDifficulty(string difficulty, QuestionBank questionBank);
    
    /// <summary>
    /// 随机获取题目
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="questionBank">题库</param>
    /// <param name="category">类别（可选）</param>
    /// <param name="difficulty">难度（可选）</param>
    /// <returns>题目列表</returns>
    List<Question> GetRandomQuestions(int count, QuestionBank questionBank, string? category = null, string? difficulty = null);
}