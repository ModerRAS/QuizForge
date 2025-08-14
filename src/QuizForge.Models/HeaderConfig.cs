using System;

namespace QuizForge.Models;

/// <summary>
/// 抬头配置类
/// </summary>
public class HeaderConfig
{
    /// <summary>
    /// 抬头样式
    /// </summary>
    public HeaderStyle Style { get; set; } = HeaderStyle.Standard;
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string ExamTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 考试科目
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// 考试时间（分钟）
    /// </summary>
    public int ExamTime { get; set; } = 120;
    
    /// <summary>
    /// 总分
    /// </summary>
    public decimal TotalPoints { get; set; } = 100;
    
    /// <summary>
    /// 考试日期
    /// </summary>
    public string ExamDate { get; set; } = string.Empty;
    
    /// <summary>
    /// 学校名称
    /// </summary>
    public string SchoolName { get; set; } = string.Empty;
    
    /// <summary>
    /// 考试地点
    /// </summary>
    public string ExamLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// 学院
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// 专业
    /// </summary>
    public string Major { get; set; } = string.Empty;
    
    /// <summary>
    /// 班级
    /// </summary>
    public string Class { get; set; } = string.Empty;
    
    /// <summary>
    /// 学期
    /// </summary>
    public string Semester { get; set; } = string.Empty;
    
    /// <summary>
    /// 考试类型
    /// </summary>
    public string ExamType { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否显示抬头
    /// </summary>
    public bool ShowHeader { get; set; } = true;
    
    /// <summary>
    /// 是否显示密封线
    /// </summary>
    public bool ShowSealLine { get; set; } = true;
    
    /// <summary>
    /// 是否显示考生信息填写区域
    /// </summary>
    public bool ShowStudentInfo { get; set; } = true;
    
    /// <summary>
    /// 考生信息配置
    /// </summary>
    public StudentInfoConfig StudentInfo { get; set; } = new();
    
    /// <summary>
    /// 自定义模板内容
    /// </summary>
    public string CustomTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// 自定义内容
    /// </summary>
    public string CustomContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 对齐方式
    /// </summary>
    public HeaderAlignment Alignment { get; set; } = HeaderAlignment.Center;
    
    /// <summary>
    /// 标题字体大小
    /// </summary>
    public HeaderFontSize TitleFontSize { get; set; } = HeaderFontSize.Large;
    
    /// <summary>
    /// 标题是否加粗
    /// </summary>
    public bool TitleBold { get; set; } = true;
    
    /// <summary>
    /// 抬头后间距（厘米）
    /// </summary>
    public double SpacingAfter { get; set; } = 1.0;
    
    /// <summary>
    /// 是否只在第一页显示抬头
    /// </summary>
    public bool ShowOnFirstPageOnly { get; set; } = true;
    
    /// <summary>
    /// 是否启用奇偶页不同的页眉页脚
    /// </summary>
    public bool EnableOddEvenHeaderFooter { get; set; } = false;
    
    /// <summary>
    /// 奇数页页眉内容
    /// </summary>
    public string OddPageHeaderContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 偶数页页眉内容
    /// </summary>
    public string EvenPageHeaderContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 奇数页页脚内容
    /// </summary>
    public string OddPageFooterContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 偶数页页脚内容
    /// </summary>
    public string EvenPageFooterContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 页码格式
    /// </summary>
    public PageNumberFormat PageNumberFormat { get; set; } = PageNumberFormat.Chinese;
    
    /// <summary>
    /// 是否在页脚显示页码
    /// </summary>
    public bool ShowPageNumberInFooter { get; set; } = true;
    
    /// <summary>
    /// 是否在页眉显示页码
    /// </summary>
    public bool ShowPageNumberInHeader { get; set; } = false;
    
    /// <summary>
    /// 是否显示页脚
    /// </summary>
    public bool ShowFooter { get; set; } = true;
    
    /// <summary>
    /// 页码位置
    /// </summary>
    public PageNumberPosition PageNumberPosition { get; set; } = PageNumberPosition.Center;
}

/// <summary>
/// 抬头样式枚举
/// </summary>
public enum HeaderStyle
{
    /// <summary>
    /// 标准样式
    /// </summary>
    Standard,
    
    /// <summary>
    /// 简洁样式
    /// </summary>
    Simple,
    
    /// <summary>
    /// 详细样式
    /// </summary>
    Detailed,
    
    /// <summary>
    /// 自定义样式
    /// </summary>
    Custom
}

/// <summary>
/// 抬头对齐方式枚举
/// </summary>
public enum HeaderAlignment
{
    /// <summary>
    /// 左对齐
    /// </summary>
    Left,
    
    /// <summary>
    /// 居中对齐
    /// </summary>
    Center,
    
    /// <summary>
    /// 右对齐
    /// </summary>
    Right
}

/// <summary>
/// 抬头字体大小枚举
/// </summary>
public enum HeaderFontSize
{
    /// <summary>
    /// 小号字体
    /// </summary>
    Small,
    
    /// <summary>
    /// 正常字体
    /// </summary>
    Normal,
    
    /// <summary>
    /// 中号字体
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大号字体
    /// </summary>
    Large,
    
    /// <summary>
    /// 特大号字体
    /// </summary>
    ExtraLarge
}

/// <summary>
/// 考生信息布局枚举
/// </summary>
public enum StudentInfoLayout
{
    /// <summary>
    /// 单列布局
    /// </summary>
    SingleColumn,
    
    /// <summary>
    /// 两列布局
    /// </summary>
    TwoColumn,
    
    /// <summary>
    /// 水平布局
    /// </summary>
    Horizontal,
    
    /// <summary>
    /// 垂直布局
    /// </summary>
    Vertical,
    
    /// <summary>
    /// 网格布局
    /// </summary>
    Grid
}

/// <summary>
/// 考生信息配置类
/// </summary>
public class StudentInfoConfig
{
    /// <summary>
    /// 布局方式
    /// </summary>
    public StudentInfoLayout Layout { get; set; } = StudentInfoLayout.SingleColumn;
    
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
    /// 自定义字段1标签
    /// </summary>
    public string CustomField1Label { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否显示自定义字段1
    /// </summary>
    public bool ShowCustomField1 { get; set; } = false;
    
    /// <summary>
    /// 自定义字段2标签
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
}

/// <summary>
/// 页码格式枚举
/// </summary>
public enum PageNumberFormat
{
    /// <summary>
    /// 中文格式（第X页/共Y页）
    /// </summary>
    Chinese,
    
    /// <summary>
    /// 数字格式（X/Y）
    /// </summary>
    Numeric,
    
    /// <summary>
    /// 英文格式（Page X of Y）
    /// </summary>
    English,
    
    /// <summary>
    /// 自定义格式
    /// </summary>
    Custom
}

/// <summary>
/// 页码位置枚举
/// </summary>
public enum PageNumberPosition
{
    /// <summary>
    /// 左侧
    /// </summary>
    Left,
    
    /// <summary>
    /// 居中
    /// </summary>
    Center,
    
    /// <summary>
    /// 右侧
    /// </summary>
    Right,
    
    /// <summary>
    /// 外侧（奇数页右侧，偶数页左侧）
    /// </summary>
    Outside,
    
    /// <summary>
    /// 内侧（奇数页左侧，偶数页右侧）
    /// </summary>
    Inside
}