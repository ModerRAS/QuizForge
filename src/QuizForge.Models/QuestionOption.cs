namespace QuizForge.Models;

/// <summary>
/// 题目选项模型
/// </summary>
public class QuestionOption
{
    /// <summary>
    /// 选项ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 选项键
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// 选项值
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否为正确答案
    /// </summary>
    public bool IsCorrect { get; set; }
}