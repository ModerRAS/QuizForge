namespace QuizForge.Models;

/// <summary>
/// 模板章节模型
/// </summary>
public class TemplateSection
{
    /// <summary>
    /// 章节ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 章节标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 章节说明
    /// </summary>
    public string Instructions { get; set; } = string.Empty;
    
    /// <summary>
    /// 题目数量
    /// </summary>
    public int QuestionCount { get; set; }
    
    /// <summary>
    /// 总分值
    /// </summary>
    public decimal TotalPoints { get; set; }
    
    /// <summary>
    /// 题目ID列表
    /// </summary>
    public List<Guid> QuestionIds { get; set; } = new();
}