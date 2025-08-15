namespace QuizForge.Models;

/// <summary>
/// 题目模型
/// </summary>
public class Question
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 题目类型
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 题目内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 题目难度
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;
    
    /// <summary>
    /// 题目类别
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 选项列表
    /// </summary>
    public List<QuestionOption> Options { get; set; } = new();
    
    /// <summary>
    /// 正确答案
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;
    
    /// <summary>
    /// 解析说明
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
    
    /// <summary>
    /// 分值
    /// </summary>
    public decimal Points { get; set; }
    
    /// <summary>
    /// 题库ID
    /// </summary>
    public Guid QuestionBankId { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}