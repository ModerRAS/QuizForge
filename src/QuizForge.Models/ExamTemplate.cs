namespace QuizForge.Models;

/// <summary>
/// 试卷模板模型
/// </summary>
public class ExamTemplate
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 纸张大小
    /// </summary>
    public PaperSize PaperSize { get; set; } = PaperSize.A4;
    
    /// <summary>
    /// 模板样式
    /// </summary>
    public TemplateStyle Style { get; set; } = TemplateStyle.Basic;
    
    /// <summary>
    /// 页眉内容
    /// </summary>
    public string HeaderContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 页脚内容
    /// </summary>
    public string FooterContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 密封线位置
    /// </summary>
    public SealLinePosition SealLine { get; set; } = SealLinePosition.Left;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 模板章节列表
    /// </summary>
    public List<TemplateSection> Sections { get; set; } = new();
}

/// <summary>
/// 纸张大小枚举
/// </summary>
public enum PaperSize
{
    /// <summary>
    /// A4纸张
    /// </summary>
    A4,
    
    /// <summary>
    /// A3纸张
    /// </summary>
    A3
}

/// <summary>
/// 模板样式枚举
/// </summary>
public enum TemplateStyle
{
    /// <summary>
    /// 基础样式
    /// </summary>
    Basic,
    
    /// <summary>
    /// 高级样式
    /// </summary>
    Advanced,
    
    /// <summary>
    /// 自定义样式
    /// </summary>
    Custom
}

/// <summary>
/// 密封线位置枚举
/// </summary>
public enum SealLinePosition
{
    /// <summary>
    /// 左侧
    /// </summary>
    Left,
    
    /// <summary>
    /// 右侧
    /// </summary>
    Right,
    
    /// <summary>
    /// 顶部
    /// </summary>
    Top,
    
    /// <summary>
    /// 底部
    /// </summary>
    Bottom
}