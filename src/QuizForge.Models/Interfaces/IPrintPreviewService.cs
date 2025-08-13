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

    /// <summary>
    /// 生成指定页面范围的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="startPage">起始页码</param>
    /// <param name="endPage">结束页码</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据列表</returns>
    Task<List<byte[]>> GeneratePreviewRangeAsync(string pdfPath, int startPage, int endPage, int width = 800, int height = 600);

    /// <summary>
    /// 生成高质量预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="quality">预览质量（1-100）</param>
    /// <returns>预览图像数据</returns>
    Task<byte[]> GenerateHighQualityPreviewAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600, int quality = 90);

    /// <summary>
    /// 生成带密封线标记的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="showSealLine">是否显示密封线标记</param>
    /// <returns>预览图像数据</returns>
    Task<byte[]> GeneratePreviewWithSealLineAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600, bool showSealLine = true);

    /// <summary>
    /// 生成缩略图预览
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="thumbnailSize">缩略图尺寸</param>
    /// <returns>缩略图图像数据</returns>
    Task<byte[]> GenerateThumbnailAsync(string pdfPath, int pageNumber = 1, int thumbnailSize = 200);

    /// <summary>
    /// 获取预览配置
    /// </summary>
    /// <returns>预览配置</returns>
    Task<PreviewConfig> GetPreviewConfigAsync();

    /// <summary>
    /// 设置预览配置
    /// </summary>
    /// <param name="config">预览配置</param>
    /// <returns>设置结果</returns>
    Task<bool> SetPreviewConfigAsync(PreviewConfig config);

    /// <summary>
    /// 裁剪预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="x">裁剪起始X坐标</param>
    /// <param name="y">裁剪起始Y坐标</param>
    /// <param name="width">裁剪宽度</param>
    /// <param name="height">裁剪高度</param>
    /// <returns>裁剪后的图像数据</returns>
    Task<byte[]> CropPreviewImageAsync(byte[] imageData, int x, int y, int width, int height);

    /// <summary>
    /// 调整预览图像亮度
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="brightness">亮度调整值（-100到100）</param>
    /// <returns>调整后的图像数据</returns>
    Task<byte[]> AdjustBrightnessAsync(byte[] imageData, int brightness);

    /// <summary>
    /// 调整预览图像对比度
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="contrast">对比度调整值（-100到100）</param>
    /// <returns>调整后的图像数据</returns>
    Task<byte[]> AdjustContrastAsync(byte[] imageData, int contrast);
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

/// <summary>
/// 预览配置
/// </summary>
public class PreviewConfig
{
    /// <summary>
    /// 默认预览宽度
    /// </summary>
    public int DefaultWidth { get; set; } = 800;

    /// <summary>
    /// 默认预览高度
    /// </summary>
    public int DefaultHeight { get; set; } = 600;

    /// <summary>
    /// 默认缩略图尺寸
    /// </summary>
    public int DefaultThumbnailSize { get; set; } = 200;

    /// <summary>
    /// 默认预览质量（1-100）
    /// </summary>
    public int DefaultQuality { get; set; } = 90;

    /// <summary>
    /// 是否显示密封线标记
    /// </summary>
    public bool ShowSealLine { get; set; } = true;

    /// <summary>
    /// 是否启用高质量预览
    /// </summary>
    public bool EnableHighQuality { get; set; } = false;

    /// <summary>
    /// 默认缩放级别
    /// </summary>
    public double DefaultZoomLevel { get; set; } = 100;

    /// <summary>
    /// 最小缩放级别
    /// </summary>
    public double MinZoomLevel { get; set; } = 10;

    /// <summary>
    /// 最大缩放级别
    /// </summary>
    public double MaxZoomLevel { get; set; } = 500;

    /// <summary>
    /// 缩放步长
    /// </summary>
    public double ZoomStep { get; set; } = 10;

    /// <summary>
    /// 是否启用鼠标滚轮缩放
    /// </summary>
    public bool EnableMouseWheelZoom { get; set; } = true;

    /// <summary>
    /// 是否启用拖动浏览
    /// </summary>
    public bool EnableDragNavigation { get; set; } = true;

    /// <summary>
    /// 是否启用适应宽度
    /// </summary>
    public bool EnableFitToWidth { get; set; } = true;

    /// <summary>
    /// 是否启用双页显示
    /// </summary>
    public bool EnableDualPageView { get; set; } = false;

    /// <summary>
    /// 是否启用连续滚动
    /// </summary>
    public bool EnableContinuousScroll { get; set; } = true;

    /// <summary>
    /// 预览显示模式
    /// </summary>
    public PreviewDisplayMode DisplayMode { get; set; } = PreviewDisplayMode.SinglePage;

    /// <summary>
    /// 默认亮度调整值（-100到100）
    /// </summary>
    public int DefaultBrightness { get; set; } = 0;

    /// <summary>
    /// 默认对比度调整值（-100到100）
    /// </summary>
    public int DefaultContrast { get; set; } = 0;
}

/// <summary>
/// 预览显示模式
/// </summary>
public enum PreviewDisplayMode
{
    /// <summary>
    /// 单页显示
    /// </summary>
    SinglePage,

    /// <summary>
    /// 双页显示
    /// </summary>
    DualPage,

    /// <summary>
    /// 连续滚动
    /// </summary>
    ContinuousScroll,

    /// <summary>
    /// 缩略图网格
    /// </summary>
    ThumbnailGrid
}