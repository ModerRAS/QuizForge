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
    /// 抬头配置
    /// </summary>
    public HeaderConfig HeaderConfig { get; set; } = new();
    
    /// <summary>
    /// 密封线位置
    /// </summary>
    public SealLinePosition SealLine { get; set; } = SealLinePosition.Left;
    
    /// <summary>
    /// 密封线内容配置
    /// </summary>
    public SealLineContentConfig SealLineContent { get; set; } = new SealLineContentConfig();
    
    /// <summary>
    /// 是否启用动态密封线位置（根据页码自动调整）
    /// </summary>
    public bool EnableDynamicSealLinePosition { get; set; } = true;
    
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

/// <summary>
/// 密封线内容配置类
/// </summary>
public class SealLineContentConfig
{
    /// <summary>
    /// 是否显示姓名字段
    /// </summary>
    public bool ShowName { get; set; } = true;
    
    /// <summary>
    /// 姓名字段标签
    /// </summary>
    public string NameLabel { get; set; } = "姓名：";
    
    /// <summary>
    /// 是否显示考号字段
    /// </summary>
    public bool ShowStudentId { get; set; } = true;
    
    /// <summary>
    /// 考号字段标签
    /// </summary>
    public string StudentIdLabel { get; set; } = "考号：";
    
    /// <summary>
    /// 是否显示班级字段
    /// </summary>
    public bool ShowClass { get; set; } = true;
    
    /// <summary>
    /// 班级字段标签
    /// </summary>
    public string ClassLabel { get; set; } = "班级：";
    
    /// <summary>
    /// 是否显示日期字段
    /// </summary>
    public bool ShowDate { get; set; } = true;
    
    /// <summary>
    /// 日期字段标签
    /// </summary>
    public string DateLabel { get; set; } = "日期：";
    
    /// <summary>
    /// 是否显示学校字段
    /// </summary>
    public bool ShowSchool { get; set; } = false;
    
    /// <summary>
    /// 学校字段标签
    /// </summary>
    public string SchoolLabel { get; set; } = "学校：";
    
    /// <summary>
    /// 是否显示科目字段
    /// </summary>
    public bool ShowSubject { get; set; } = false;
    
    /// <summary>
    /// 科目字段标签
    /// </summary>
    public string SubjectLabel { get; set; } = "科目：";
    
    /// <summary>
    /// 自定义字段1
    /// </summary>
    public string CustomField1Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否显示自定义字段1
    /// </summary>
    public bool ShowCustomField1 { get; set; } = false;
    
    /// <summary>
    /// 自定义字段2
    /// </summary>
    public string CustomField2Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否显示自定义字段2
    /// </summary>
    public bool ShowCustomField2 { get; set; } = false;
    
    /// <summary>
    /// 下划线长度（厘米）
    /// </summary>
    public double UnderlineLength { get; set; } = 3.0;
    
    /// <summary>
    /// 密封线标题
    /// </summary>
    public string Title { get; set; } = "密封线";
}