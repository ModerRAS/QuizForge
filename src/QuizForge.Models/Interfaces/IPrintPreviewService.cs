using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 打印预览服务接口
/// </summary>
public interface IPrintPreviewService
{
    /// <summary>
    /// 生成PDF预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    Task<byte[]> GeneratePreviewImageAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600);

    /// <summary>
    /// 生成PDF所有页面的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据列表</returns>
    Task<List<byte[]>> GenerateAllPreviewImagesAsync(string pdfPath, int width = 800, int height = 600);

    /// <summary>
    /// 获取PDF页面数量
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>页面数量</returns>
    Task<int> GetPageCountAsync(string pdfPath);

    /// <summary>
    /// 获取PDF信息
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>PDF信息</returns>
    Task<PdfInfo> GetPdfInfoAsync(string pdfPath);

    /// <summary>
    /// 缩放预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="scale">缩放比例</param>
    /// <returns>缩放后的图像数据</returns>
    Task<byte[]> ScalePreviewImageAsync(byte[] imageData, double scale);

    /// <summary>
    /// 旋转预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="angle">旋转角度</param>
    /// <returns>旋转后的图像数据</returns>
    Task<byte[]> RotatePreviewImageAsync(byte[] imageData, int angle);
}

/// <summary>
/// PDF信息
/// </summary>
public class PdfInfo
{
    /// <summary>
    /// 页面数量
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// PDF标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 页面尺寸列表
    /// </summary>
    public List<PageSize> PageSizes { get; set; } = new();
}

/// <summary>
/// 页面尺寸
/// </summary>
public class PageSize
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 宽度（点）
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// 高度（点）
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// 宽度（毫米）
    /// </summary>
    public double WidthInMm => Width * 0.3527777778;

    /// <summary>
    /// 高度（毫米）
    /// </summary>
    public double HeightInMm => Height * 0.3527777778;
}