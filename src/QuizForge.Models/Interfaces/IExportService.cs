using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 导出服务接口
/// </summary>
public interface IExportService
{
    /// <summary>
    /// 导出为PDF
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>PDF文件路径</returns>
    Task<string> ExportToPdfAsync(string latexContent, ExportConfiguration configuration);
    
    /// <summary>
    /// 导出为LaTeX
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>LaTeX文件路径</returns>
    Task<string> ExportToLaTeXAsync(string latexContent, ExportConfiguration configuration);
    
    /// <summary>
    /// 导出为Word
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>Word文件路径</returns>
    Task<string> ExportToWordAsync(string latexContent, ExportConfiguration configuration);
    
    /// <summary>
    /// 生成预览图像
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    Task<byte[]> GeneratePreviewImageAsync(string latexContent, int width = 800, int height = 600);
    
    /// <summary>
    /// 打印文档
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="settings">打印设置</param>
    /// <returns>打印结果</returns>
    Task<bool> PrintDocumentAsync(string filePath, PrintSettings settings);
}

/// <summary>
/// 导出配置
/// </summary>
public class ExportConfiguration
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 输出路径
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.PDF;
    
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 包含答案
    /// </summary>
    public bool IncludeAnswerKey { get; set; } = false;
    
    /// <summary>
    /// 包含水印
    /// </summary>
    public bool IncludeWatermark { get; set; } = false;
    
    /// <summary>
    /// 水印文本
    /// </summary>
    public string WatermarkText { get; set; } = string.Empty;
    
    /// <summary>
    /// 份数
    /// </summary>
    public int Copies { get; set; } = 1;
    
    /// <summary>
    /// PDF设置
    /// </summary>
    public PdfSettings PdfSettings { get; set; } = new();
}

/// <summary>
/// PDF设置
/// </summary>
public class PdfSettings
{
    /// <summary>
    /// 设置ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 页面方向
    /// </summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    
    /// <summary>
    /// 上边距
    /// </summary>
    public decimal MarginTop { get; set; } = 2.5m;
    
    /// <summary>
    /// 下边距
    /// </summary>
    public decimal MarginBottom { get; set; } = 2.5m;
    
    /// <summary>
    /// 左边距
    /// </summary>
    public decimal MarginLeft { get; set; } = 2.5m;
    
    /// <summary>
    /// 右边距
    /// </summary>
    public decimal MarginRight { get; set; } = 2.5m;
    
    /// <summary>
    /// 字体
    /// </summary>
    public string FontFamily { get; set; } = "SimSun";
    
    /// <summary>
    /// 字号
    /// </summary>
    public decimal FontSize { get; set; } = 12m;
}

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// PDF格式
    /// </summary>
    PDF,
    
    /// <summary>
    /// LaTeX格式
    /// </summary>
    LaTeX,
    
    /// <summary>
    /// Word格式
    /// </summary>
    Word
}

/// <summary>
/// 页面方向枚举
/// </summary>
public enum PageOrientation
{
    /// <summary>
    /// 纵向
    /// </summary>
    Portrait,
    
    /// <summary>
    /// 横向
    /// </summary>
    Landscape
}

/// <summary>
/// 打印方向枚举
/// </summary>
public enum PrintOrientation
{
    /// <summary>
    /// 纵向
    /// </summary>
    Portrait,
    
    /// <summary>
    /// 横向
    /// </summary>
    Landscape
}

/// <summary>
/// 双面打印模式枚举
/// </summary>
public enum PrintDuplexMode
{
    /// <summary>
    /// 单面打印
    /// </summary>
    Simplex,
    
    /// <summary>
    /// 水平双面打印
    /// </summary>
    Horizontal,
    
    /// <summary>
    /// 垂直双面打印
    /// </summary>
    Vertical
}

/// <summary>
/// 打印质量枚举
/// </summary>
public enum PrintQuality
{
    /// <summary>
    /// 草稿质量
    /// </summary>
    Draft,
    
    /// <summary>
    /// 普通质量
    /// </summary>
    Normal,
    
    /// <summary>
    /// 高质量
    /// </summary>
    High
}

/// <summary>
/// 打印边距
/// </summary>
public class PrintMargins
{
    /// <summary>
    /// 左边距
    /// </summary>
    public int Left { get; set; } = 10;
    
    /// <summary>
    /// 上边距
    /// </summary>
    public int Top { get; set; } = 10;
    
    /// <summary>
    /// 右边距
    /// </summary>
    public int Right { get; set; } = 10;
    
    /// <summary>
    /// 下边距
    /// </summary>
    public int Bottom { get; set; } = 10;
}

/// <summary>
/// 打印设置
/// </summary>
public class PrintSettings
{
    /// <summary>
    /// 打印机名称
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;
    
    /// <summary>
    /// 份数
    /// </summary>
    public int Copies { get; set; } = 1;
    
    /// <summary>
    /// 双面打印
    /// </summary>
    public bool Duplex { get; set; } = false;
    
    /// <summary>
    /// 打印范围
    /// </summary>
    public string PrintRange { get; set; } = "All";
    
    /// <summary>
    /// 双面打印模式
    /// </summary>
    public PrintDuplexMode DuplexMode { get; set; } = PrintDuplexMode.Simplex;
    
    /// <summary>
    /// 页面方向
    /// </summary>
    public PrintOrientation Orientation { get; set; } = PrintOrientation.Portrait;
    
    /// <summary>
    /// 打印质量
    /// </summary>
    public PrintQuality Quality { get; set; } = PrintQuality.Normal;
    
    /// <summary>
    /// 起始页码
    /// </summary>
    public int FirstPage { get; set; } = 1;
    
    /// <summary>
    /// 结束页码
    /// </summary>
    public int LastPage { get; set; } = 1;
    
    /// <summary>
    /// 纸张大小
    /// </summary>
    public PaperSize PaperSize { get; set; } = PaperSize.A4;
    
    /// <summary>
    /// 页边距
    /// </summary>
    public PrintMargins Margins { get; set; } = new();
}