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
using DrawingImage = System.Drawing.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace QuizForge.Infrastructure.Services;

/// <summary>
/// 打印预览服务实现
/// </summary>
public class PrintPreviewService : IPrintPreviewService
{
    private readonly ILogger<PrintPreviewService> _logger;
    private readonly IPdfEngine _pdfEngine;

    /// <summary>
    /// 打印预览服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="pdfEngine">PDF引擎</param>
    public PrintPreviewService(ILogger<PrintPreviewService> logger, IPdfEngine pdfEngine)
    {
        _logger = logger;
        _pdfEngine = pdfEngine;
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
            var textOptions = new RichTextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.PointF(10, 10),
                TabWidth = 4,
                WrappingLength = width - 20
            };
            
            image.Mutate(ctx =>
            {
                ctx.DrawText(
                    textOptions,
                    $"PDF预览: {Path.GetFileName(pdfPath)}\n" +
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
}