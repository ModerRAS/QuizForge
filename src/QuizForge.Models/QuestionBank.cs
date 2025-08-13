namespace QuizForge.Models;

/// <summary>
/// 题库模型
/// </summary>
public class QuestionBank
{
    /// <summary>
    /// 题库ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 题库名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 题库描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 题目列表
    /// </summary>
    public List<Question> Questions { get; set; } = new();
}