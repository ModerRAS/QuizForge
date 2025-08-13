namespace QuizForge.Models;

/// <summary>
/// 试卷模型
/// </summary>
public class ExamPaper
{
    /// <summary>
    /// 试卷ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid TemplateId { get; set; }
    
    /// <summary>
    /// 题库ID
    /// </summary>
    public Guid QuestionBankId { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 试卷内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 总分值
    /// </summary>
    public decimal TotalPoints { get; set; }
    
    /// <summary>
    /// 题目列表
    /// </summary>
    public List<Question> Questions { get; set; } = new();
}