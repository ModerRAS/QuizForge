namespace QuizForge.Models;

/// <summary>
/// 试卷页面模型
/// </summary>
public class ExamPage
{
    /// <summary>
    /// 页面ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 试卷ID
    /// </summary>
    public Guid ExamPaperId { get; set; }
    
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// 页面内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 页面标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 页面类型
    /// </summary>
    public ExamPageType PageType { get; set; }
    
    /// <summary>
    /// 页面顺序
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 试卷页面类型
/// </summary>
public enum ExamPageType
{
    /// <summary>
    /// 封面
    /// </summary>
    Cover,
    
    /// <summary>
    /// 说明页
    /// </summary>
    Instructions,
    
    /// <summary>
    /// 题目页
    /// </summary>
    Questions,
    
    /// <summary>
    /// 答题卡
    /// </summary>
    AnswerSheet,
    
    /// <summary>
    /// 封底
    /// </summary>
    BackCover
}