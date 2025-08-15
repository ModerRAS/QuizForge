using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats.Png;
using System.Text.Json;
using DrawingImage = System.Drawing.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using IOPath = System.IO.Path;

namespace QuizForge.Infrastructure.Services;

/// <summary>
/// 打印预览服务实现
/// </summary>
public class PrintPreviewService : IPrintPreviewService
{
    private readonly ILogger<PrintPreviewService> _logger;
    private readonly IPdfEngine _pdfEngine;
    private PreviewConfig _previewConfig = new PreviewConfig();
    private readonly string _configFilePath;

    /// <summary>
    /// 打印预览服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="pdfEngine">PDF引擎</param>
    public PrintPreviewService(ILogger<PrintPreviewService> logger, IPdfEngine pdfEngine)
    {
        _logger = logger;
        _pdfEngine = pdfEngine;
        
        // 初始化配置文件路径
        var appDataPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuizForge");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        _configFilePath = IOPath.Combine(appDataPath, "preview-config.json");
        
        // 加载预览配置
        _ = LoadPreviewConfigAsync();
    }

    /// <summary>
    /// 生成PDF预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GeneratePreviewImageAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            if (pageNumber < 1)
            {
                _logger.LogError("页码必须大于0: {PageNumber}", pageNumber);
                return Array.Empty<byte>();
            }

            // 使用PDF引擎生成预览图像
            var previewImage = await _pdfEngine.GeneratePreviewAsync(pdfPath, width, height);
            
            if (previewImage.Length == 0)
            {
                // 如果PDF引擎无法生成预览，使用备用方法
                return await GeneratePreviewImageFallbackAsync(pdfPath, pageNumber, width, height);
            }

            return previewImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成PDF预览图像失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成PDF所有页面的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据列表</returns>
    public async Task<List<byte[]>> GenerateAllPreviewImagesAsync(string pdfPath, int width = 800, int height = 600)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return new List<byte[]>();
            }

            var pageCount = await GetPageCountAsync(pdfPath);
            var previewImages = new List<byte[]>();

            for (int i = 1; i <= pageCount; i++)
            {
                var previewImage = await GeneratePreviewImageAsync(pdfPath, i, width, height);
                if (previewImage.Length > 0)
                {
                    previewImages.Add(previewImage);
                }
            }

            return previewImages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成PDF所有页面预览图像失败: {PdfPath}", pdfPath);
            return new List<byte[]>();
        }
    }

    /// <summary>
    /// 获取PDF页面数量
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>页面数量</returns>
    public async Task<int> GetPageCountAsync(string pdfPath)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return 0;
            }

            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
            return document.PageCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取PDF页面数量失败: {PdfPath}", pdfPath);
            return 0;
        }
    }

    /// <summary>
    /// 获取PDF信息
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>PDF信息</returns>
    public async Task<PdfInfo> GetPdfInfoAsync(string pdfPath)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return new PdfInfo();
            }

            var fileInfo = new FileInfo(pdfPath);
            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);

            var pdfInfo = new PdfInfo
            {
                PageCount = document.PageCount,
                Title = document.Info.Title ?? string.Empty,
                Author = document.Info.Author ?? string.Empty,
                CreationDate = document.Info.CreationDate,
                ModificationDate = document.Info.ModificationDate,
                FileSize = fileInfo.Length
            };

            // 获取每页的尺寸
            for (int i = 0; i < document.PageCount; i++)
            {
                var page = document.Pages[i];
                pdfInfo.PageSizes.Add(new PageSize
                {
                    PageNumber = i + 1,
                    Width = page.Width,
                    Height = page.Height
                });
            }

            return pdfInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取PDF信息失败: {PdfPath}", pdfPath);
            return new PdfInfo();
        }
    }

    /// <summary>
    /// 缩放预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="scale">缩放比例</param>
    /// <returns>缩放后的图像数据</returns>
    public async Task<byte[]> ScalePreviewImageAsync(byte[] imageData, double scale)
    {
        try
        {
            if (imageData.Length == 0 || scale <= 0)
            {
                return Array.Empty<byte>();
            }

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            var newWidth = (int)(image.Width * scale);
            var newHeight = (int)(image.Height * scale);
            
            // 确保最小尺寸
            newWidth = Math.Max(newWidth, 10);
            newHeight = Math.Max(newHeight, 10);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "缩放预览图像失败");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 旋转预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="angle">旋转角度</param>
    /// <returns>旋转后的图像数据</returns>
    public async Task<byte[]> RotatePreviewImageAsync(byte[] imageData, int angle)
    {
        try
        {
            if (imageData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            // 标准化角度到0-360度
            angle = angle % 360;
            if (angle < 0)
            {
                angle += 360;
            }

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            var rotateMode = angle switch
            {
                90 => RotateMode.Rotate90,
                180 => RotateMode.Rotate180,
                270 => RotateMode.Rotate270,
                _ => (RotateMode)angle
            };

            image.Mutate(x => x.Rotate(rotateMode));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "旋转预览图像失败");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成预览图像的备用方法
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>预览图像数据</returns>
    private async Task<byte[]> GeneratePreviewImageFallbackAsync(string pdfPath, int pageNumber, int width, int height)
    {
        try
        {
            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
            
            if (pageNumber > document.PageCount)
            {
                _logger.LogError("页码超出范围: {PageNumber}, 总页数: {PageCount}", pageNumber, document.PageCount);
                return Array.Empty<byte>();
            }

            var page = document.Pages[pageNumber - 1];
            
            // 创建预览图像
            using var image = new Image<Rgb24>(width, height);
            image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.White));
            
            // 添加预览文本
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 16);
            var textOptions = new SixLabors.ImageSharp.Drawing.Processing.RichTextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.PointF(10, 10),
                TabWidth = 4,
                WrappingLength = width - 20
            };
            
            image.Mutate(ctx =>
            {
                ctx.DrawText(
                    textOptions,
                    $"PDF预览: {IOPath.GetFileName(pdfPath)}\n" +
                    $"页码: {pageNumber}/{document.PageCount}\n" +
                    $"尺寸: {page.Width}x{page.Height}",
                    SixLabors.ImageSharp.Color.Black);
            });
            
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成预览图像备用方法失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成指定页面范围的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="startPage">起始页码</param>
    /// <param name="endPage">结束页码</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据列表</returns>
    public async Task<List<byte[]>> GeneratePreviewRangeAsync(string pdfPath, int startPage, int endPage, int width = 800, int height = 600)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return new List<byte[]>();
            }

            if (startPage < 1 || endPage < startPage)
            {
                _logger.LogError("无效的页码范围: 起始页 {StartPage}, 结束页 {EndPage}", startPage, endPage);
                return new List<byte[]>();
            }

            var pageCount = await GetPageCountAsync(pdfPath);
            if (endPage > pageCount)
            {
                endPage = pageCount;
                _logger.LogWarning("结束页码超出总页数，已调整为: {EndPage}", endPage);
            }

            var previewImages = new List<byte[]>();

            for (int i = startPage; i <= endPage; i++)
            {
                var previewImage = await GeneratePreviewImageAsync(pdfPath, i, width, height);
                if (previewImage.Length > 0)
                {
                    previewImages.Add(previewImage);
                }
            }

            return previewImages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成PDF页面范围预览图像失败: {PdfPath}, 页码范围: {StartPage}-{EndPage}", pdfPath, startPage, endPage);
            return new List<byte[]>();
        }
    }

    /// <summary>
    /// 生成高质量预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="quality">预览质量（1-100）</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GenerateHighQualityPreviewAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600, int quality = 90)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            if (pageNumber < 1)
            {
                _logger.LogError("页码必须大于0: {PageNumber}", pageNumber);
                return Array.Empty<byte>();
            }

            if (quality < 1 || quality > 100)
            {
                quality = Math.Clamp(quality, 1, 100);
                _logger.LogWarning("质量值超出范围，已调整为: {Quality}", quality);
            }

            // 使用更高的分辨率生成预览
            var highResWidth = (int)(width * 1.5);
            var highResHeight = (int)(height * 1.5);

            // 使用PDF引擎生成预览图像
            var previewImage = await _pdfEngine.GeneratePreviewAsync(pdfPath, highResWidth, highResHeight);
            
            if (previewImage.Length == 0)
            {
                // 如果PDF引擎无法生成预览，使用备用方法
                return await GeneratePreviewImageFallbackAsync(pdfPath, pageNumber, highResWidth, highResHeight);
            }

            // 应用质量设置
            return await ApplyQualitySettingsAsync(previewImage, quality);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成高质量PDF预览图像失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成带密封线标记的预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="showSealLine">是否显示密封线标记</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GeneratePreviewWithSealLineAsync(string pdfPath, int pageNumber = 1, int width = 800, int height = 600, bool showSealLine = true)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            if (pageNumber < 1)
            {
                _logger.LogError("页码必须大于0: {PageNumber}", pageNumber);
                return Array.Empty<byte>();
            }

            // 生成基础预览图像
            var previewImage = await GeneratePreviewImageAsync(pdfPath, pageNumber, width, height);
            
            if (previewImage.Length == 0 || !showSealLine)
            {
                return previewImage;
            }

            // 添加密封线标记
            return await AddSealLineMarkerAsync(previewImage, pageNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成带密封线标记的PDF预览图像失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成缩略图预览
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="thumbnailSize">缩略图尺寸</param>
    /// <returns>缩略图图像数据</returns>
    public async Task<byte[]> GenerateThumbnailAsync(string pdfPath, int pageNumber = 1, int thumbnailSize = 200)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            if (pageNumber < 1)
            {
                _logger.LogError("页码必须大于0: {PageNumber}", pageNumber);
                return Array.Empty<byte>();
            }

            if (thumbnailSize < 50 || thumbnailSize > 500)
            {
                thumbnailSize = Math.Clamp(thumbnailSize, 50, 500);
                _logger.LogWarning("缩略图尺寸超出范围，已调整为: {ThumbnailSize}", thumbnailSize);
            }

            // 生成预览图像
            var previewImage = await GeneratePreviewImageAsync(pdfPath, pageNumber, thumbnailSize, thumbnailSize);
            
            if (previewImage.Length == 0)
            {
                return Array.Empty<byte>();
            }

            return previewImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成PDF缩略图失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 生成缩略图预览（重载方法）
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="width">缩略图宽度</param>
    /// <param name="height">缩略图高度</param>
    /// <returns>缩略图图像数据</returns>
    public async Task<byte[]> GenerateThumbnailPreviewAsync(string pdfPath, int pageNumber = 1, int width = 200, int height = 200)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            if (pageNumber < 1)
            {
                _logger.LogError("页码必须大于0: {PageNumber}", pageNumber);
                return Array.Empty<byte>();
            }

            if (width < 50 || width > 500)
            {
                width = Math.Clamp(width, 50, 500);
                _logger.LogWarning("缩略图宽度超出范围，已调整为: {Width}", width);
            }

            if (height < 50 || height > 500)
            {
                height = Math.Clamp(height, 50, 500);
                _logger.LogWarning("缩略图高度超出范围，已调整为: {Height}", height);
            }

            // 生成预览图像
            var previewImage = await GeneratePreviewImageAsync(pdfPath, pageNumber, width, height);
            
            if (previewImage.Length == 0)
            {
                return Array.Empty<byte>();
            }

            return previewImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成PDF缩略图预览失败: {PdfPath}, 页码: {PageNumber}", pdfPath, pageNumber);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 获取预览配置
    /// </summary>
    /// <returns>预览配置</returns>
    public async Task<PreviewConfig> GetPreviewConfigAsync()
    {
        if (_previewConfig == null)
        {
            await LoadPreviewConfigAsync();
        }
        return _previewConfig ?? new PreviewConfig();
    }

    /// <summary>
    /// 设置预览配置
    /// </summary>
    /// <param name="config">预览配置</param>
    /// <returns>设置结果</returns>
    public async Task<bool> SetPreviewConfigAsync(PreviewConfig config)
    {
        try
        {
            if (config == null)
            {
                _logger.LogError("预览配置不能为null");
                return false;
            }

            _previewConfig = config;

            // 保存配置到文件
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, json);

            _logger.LogInformation("预览配置已保存");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存预览配置失败");
            return false;
        }
    }

    /// <summary>
    /// 裁剪预览图像
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="x">裁剪起始X坐标</param>
    /// <param name="y">裁剪起始Y坐标</param>
    /// <param name="width">裁剪宽度</param>
    /// <param name="height">裁剪高度</param>
    /// <returns>裁剪后的图像数据</returns>
    public async Task<byte[]> CropPreviewImageAsync(byte[] imageData, int x, int y, int width, int height)
    {
        try
        {
            if (imageData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            if (width <= 0 || height <= 0)
            {
                _logger.LogError("裁剪尺寸必须大于0: 宽度 {Width}, 高度 {Height}", width, height);
                return Array.Empty<byte>();
            }

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            // 确保裁剪区域在图像范围内
            x = Math.Clamp(x, 0, image.Width - 1);
            y = Math.Clamp(y, 0, image.Height - 1);
            width = Math.Min(width, image.Width - x);
            height = Math.Min(height, image.Height - y);
            
            var cropRectangle = new SixLabors.ImageSharp.Rectangle(x, y, width, height);
            image.Mutate(ctx => ctx.Crop(cropRectangle));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "裁剪预览图像失败");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 调整预览图像亮度
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="brightness">亮度调整值（-100到100）</param>
    /// <returns>调整后的图像数据</returns>
    public async Task<byte[]> AdjustBrightnessAsync(byte[] imageData, int brightness)
    {
        try
        {
            if (imageData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            brightness = Math.Clamp(brightness, -100, 100);

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            // 将亮度值从-100到100映射到ImageSharp的亮度参数
            var brightnessParameter = (brightness + 100) / 100f;
            
            image.Mutate(ctx => ctx.Brightness(brightnessParameter));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调整预览图像亮度失败");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 调整预览图像对比度
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="contrast">对比度调整值（-100到100）</param>
    /// <returns>调整后的图像数据</returns>
    public async Task<byte[]> AdjustContrastAsync(byte[] imageData, int contrast)
    {
        try
        {
            if (imageData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            contrast = Math.Clamp(contrast, -100, 100);

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            // 将对比度值从-100到100映射到ImageSharp的对比度参数
            var contrastParameter = (contrast + 100) / 100f;
            
            image.Mutate(ctx => ctx.Contrast(contrastParameter));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调整预览图像对比度失败");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 加载预览配置
    /// </summary>
    /// <returns>加载结果</returns>
    private async Task<bool> LoadPreviewConfigAsync()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                _previewConfig = JsonSerializer.Deserialize<PreviewConfig>(json) ?? new PreviewConfig();
                _logger.LogInformation("预览配置已加载");
            }
            else
            {
                _previewConfig = new PreviewConfig();
                _logger.LogInformation("使用默认预览配置");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载预览配置失败，使用默认配置");
            _previewConfig = new PreviewConfig();
            return false;
        }
    }

    /// <summary>
    /// 应用质量设置
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="quality">质量值（1-100）</param>
    /// <returns>处理后的图像数据</returns>
    private async Task<byte[]> ApplyQualitySettingsAsync(byte[] imageData, int quality)
    {
        try
        {
            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            // 应用锐化以提高清晰度
            if (quality > 70)
            {
                var sharpenLevel = (quality - 70) / 30f;
                image.Mutate(ctx => ctx.GaussianSharpen(sharpenLevel));
            }
            
            using var outputStream = new MemoryStream();
            
            // 使用PNG编码器设置质量
            var pngEncoder = new PngEncoder
            {
                ColorType = PngColorType.RgbWithAlpha,
                CompressionLevel = PngCompressionLevel.DefaultCompression,
                BitDepth = PngBitDepth.Bit8,
                Gamma = 2.2f
            };
            
            await image.SaveAsPngAsync(outputStream, pngEncoder);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用质量设置失败");
            return imageData;
        }
    }

    /// <summary>
    /// 添加密封线标记
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>带密封线标记的图像数据</returns>
    private async Task<byte[]> AddSealLineMarkerAsync(byte[] imageData, int pageNumber)
    {
        try
        {
            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 12);
            var isOddPage = pageNumber % 2 == 1;
            
            // 确定密封线位置（奇数页在左侧，偶数页在右侧）
            var x = isOddPage ? 20 : image.Width - 120;
            var y = 20;
            
            var textOptions = new SixLabors.ImageSharp.Drawing.Processing.RichTextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.PointF(x, y),
                HorizontalAlignment = isOddPage ? HorizontalAlignment.Left : HorizontalAlignment.Right
            };
            
            image.Mutate(ctx =>
            {
                // 绘制密封线框
                var boxX = isOddPage ? 15 : image.Width - 95;
                var boxY = 15;
                var boxWidth = 80;
                var boxHeight = 20;
                
                // 绘制矩形框
                ctx.DrawPolygon(
                    new SolidPen(SixLabors.ImageSharp.Color.Red, 2),
                    new SixLabors.ImageSharp.PointF(boxX, boxY),
                    new SixLabors.ImageSharp.PointF(boxX + boxWidth, boxY),
                    new SixLabors.ImageSharp.PointF(boxX + boxWidth, boxY + boxHeight),
                    new SixLabors.ImageSharp.PointF(boxX, boxY + boxHeight));
                
                // 添加密封线文本
                var textX = isOddPage ? boxX + 10 : boxX + boxWidth - 70;
                var textY = boxY + 3;
                
                var sealTextOptions = new SixLabors.ImageSharp.Drawing.Processing.RichTextOptions(font)
                {
                    Origin = new SixLabors.ImageSharp.PointF(textX, textY)
                };
                
                ctx.DrawText(
                    sealTextOptions,
                    $"密封线 ({(isOddPage ? "左" : "右")})",
                    SixLabors.ImageSharp.Color.Red);
            });
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加密封线标记失败");
            return imageData;
        }
    }
    
    /// <summary>
    /// 同时调整预览图像亮度和对比度
    /// </summary>
    /// <param name="imageData">原始图像数据</param>
    /// <param name="brightness">亮度调整值（-100到100）</param>
    /// <param name="contrast">对比度调整值（-100到100）</param>
    /// <returns>调整后的图像数据</returns>
    public async Task<byte[]> AdjustBrightnessContrastAsync(byte[] imageData, int brightness, int contrast)
    {
        try
        {
            if (imageData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            brightness = Math.Clamp(brightness, -100, 100);
            contrast = Math.Clamp(contrast, -100, 100);

            using var inputStream = new MemoryStream(imageData);
            using var image = await ImageSharpImage.LoadAsync(inputStream);
            
            // 将亮度值从-100到100映射到ImageSharp的亮度参数
            var brightnessParameter = (brightness + 100) / 100f;
            
            // 将对比度值从-100到100映射到ImageSharp的对比度参数
            var contrastParameter = (contrast + 100) / 100f;
            
            image.Mutate(ctx => ctx.Brightness(brightnessParameter).Contrast(contrastParameter));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调整预览图像亮度和对比度失败");
            return imageData;
        }
    }
}