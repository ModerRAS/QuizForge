using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuizForge.Models.Interfaces;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Infrastructure.Renderers;
using QuizForge.Infrastructure.Exceptions;
using QuizForge.Infrastructure.Services;

namespace QuizForge.Infrastructure.Engines;

/// <summary>
/// 原生PDF引擎实现，不依赖外部LaTeX发行版
/// </summary>
public class NativePdfEngine : IPdfEngine
{
    private readonly ILogger<NativePdfEngine> _logger;
    private readonly LatexParser _latexParser;
    private readonly MathRenderer _mathRenderer;
    private readonly PdfErrorReportingService _errorReportingService;
    private readonly PdfCacheService _cacheService;
    private readonly string _tempDirectory;
    private readonly bool _enableCache;
    
    /// <summary>
    /// 原生PDF引擎构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="latexParser">LaTeX解析器</param>
    /// <param name="mathRenderer">数学公式渲染器</param>
    /// <param name="errorReportingService">错误报告服务</param>
    /// <param name="cacheService">PDF缓存服务</param>
    /// <param name="configuration">配置</param>
    public NativePdfEngine(ILogger<NativePdfEngine> logger, LatexParser latexParser, MathRenderer mathRenderer,
        PdfErrorReportingService errorReportingService, PdfCacheService cacheService, IConfiguration configuration)
    {
        _logger = logger;
        _latexParser = latexParser;
        _mathRenderer = mathRenderer;
        _errorReportingService = errorReportingService;
        _cacheService = cacheService;
        
        // 从配置中获取是否启用缓存
        _enableCache = configuration.GetValue<bool>("PdfEngine:EnableCache") ?? true;
        
        // 设置临时目录
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "PDF");
        
        // 确保临时目录存在
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
        
        // 设置QuestPDF许可证
        QuestPDF.Settings.License = LicenseType.Community;
        
        // 设置字体
        ConfigureFonts();
        
        _logger.LogInformation("原生PDF引擎初始化完成，缓存: {Enabled}", _enableCache ? "启用" : "禁用");
    }
    
    /// <summary>
    /// 配置字体
    /// </summary>
    private void ConfigureFonts()
    {
        try
        {
            // 注册中文字体
            var fontPaths = new List<string>();
            
            // 尝试查找系统中文字体
            var possibleFontPaths = new[]
            {
                @"C:\Windows\Fonts\simhei.ttf",      // 黑体
                @"C:\Windows\Fonts\simkai.ttf",      // 楷体
                @"C:\Windows\Fonts\simfang.ttf",     // 仿宋
                @"C:\Windows\Fonts\simsun.ttc",      // 宋体
                @"C:\Windows\Fonts\msyh.ttc",        // 微软雅黑
                @"C:\Windows\Fonts\msyhbd.ttc",      // 微软雅黑粗体
                @"C:\Windows\Fonts\msyhl.ttc",       // 微软雅黑细体
                @"C:\Windows\Fonts\simli.ttf",        // 隶书
                @"C:\Windows\Fonts\simyou.ttf",       // 幼圆
            };
            
            foreach (var path in possibleFontPaths)
            {
                if (File.Exists(path))
                {
                    fontPaths.Add(path);
                }
            }
            
            // 如果没有找到任何中文字体，记录警告
            if (fontPaths.Count == 0)
            {
                var warning = "未找到任何中文字体，中文内容可能无法正确显示";
                _logger.LogWarning(warning);
                _errorReportingService.ReportWarning(warning, new PdfErrorContext
                {
                    Operation = "ConfigureFonts",
                    EngineType = "NativePdfEngine"
                });
            }
            
            // 注册字体
            foreach (var fontPath in fontPaths)
            {
                try
                {
                    QuestPDF.Helpers.FontManager.RegisterFont(fontPath);
                    _logger.LogDebug("成功注册字体: {FontPath}", fontPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "注册字体失败: {FontPath}", fontPath);
                    _errorReportingService.ReportWarning($"注册字体失败: {fontPath}", new PdfErrorContext
                    {
                        Operation = "RegisterFont",
                        FilePath = fontPath,
                        EngineType = "NativePdfEngine"
                    });
                }
            }
            
            // 设置默认字体
            if (fontPaths.Any(p => p.Contains("msyh.ttc")))
            {
                QuestPDF.Helpers.FontManager.RegisterFont("Microsoft YaHei", @"C:\Windows\Fonts\msyh.ttc");
                QuestPDF.Helpers.FontManager.RegisterFont("Microsoft YaHei Bold", @"C:\Windows\Fonts\msyhbd.ttc");
                QuestPDF.Helpers.FontManager.RegisterFont("Microsoft YaHei Light", @"C:\Windows\Fonts\msyhl.ttc");
            }
            else if (fontPaths.Any(p => p.Contains("simsun.ttc")))
            {
                QuestPDF.Helpers.FontManager.RegisterFont("SimSun", @"C:\Windows\Fonts\simsun.ttc");
            }
            else if (fontPaths.Any(p => p.Contains("simhei.ttf")))
            {
                QuestPDF.Helpers.FontManager.RegisterFont("SimHei", @"C:\Windows\Fonts\simhei.ttf");
            }
            
            // 设置字体家族
            QuestPDF.Helpers.FontManager.RegisterFontFamily("ChineseFont",
                fontPaths.Any(p => p.Contains("msyh.ttc")) ? "Microsoft YaHei" :
                fontPaths.Any(p => p.Contains("simsun.ttc")) ? "SimSun" : "SimHei");
            
            _logger.LogInformation("字体配置完成，共注册 {Count} 个字体", fontPaths.Count);
        }
        catch (Exception ex)
        {
            var pdfException = new PdfGenerationException("字体配置失败", PdfErrorType.FontLoadingError, ex);
            _errorReportingService.ReportError(pdfException, new PdfErrorContext
            {
                Operation = "ConfigureFonts",
                EngineType = "NativePdfEngine"
            });
            throw pdfException;
        }
    }
    
    /// <summary>
    /// 生成 PDF 文档
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    public async Task<bool> GeneratePdfAsync(string content, string outputPath)
    {
        try
        {
            // 检查参数
            if (string.IsNullOrEmpty(content))
            {
                throw new PdfGenerationException("内容不能为空", PdfErrorType.ConfigurationError);
            }
            
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new PdfGenerationException("输出路径不能为空", PdfErrorType.ConfigurationError);
            }
            
            // 检查缓存
            if (_enableCache)
            {
                var cachedPath = _cacheService.GetCachedPdf(content);
                if (cachedPath != null && File.Exists(cachedPath))
                {
                    try
                    {
                        // 从缓存复制文件
                        File.Copy(cachedPath, outputPath, true);
                        _logger.LogInformation("从缓存生成PDF成功: {OutputPath}", outputPath);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "从缓存复制文件失败，将重新生成");
                    }
                }
            }
            
            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    throw new PdfGenerationException($"无法创建输出目录: {outputDir}", PdfErrorType.PermissionError, ex);
                }
            }
            
            // 创建PDF文档
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("ChineseFont"));
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, content));
                    page.Footer().Element(ComposeFooter);
                });
            });
            
            // 生成PDF到内存流
            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            var pdfData = ms.ToArray();
            
            // 写入输出文件
            await File.WriteAllBytesAsync(outputPath, pdfData);
            
            // 缓存PDF
            if (_enableCache)
            {
                try
                {
                    await _cacheService.CachePdfAsync(content, pdfData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "缓存PDF失败");
                }
            }
            
            _logger.LogInformation("PDF生成成功: {OutputPath}", outputPath);
            return true;
        }
        catch (PdfGenerationException)
        {
            // 重新抛出PDF生成异常
            throw;
        }
        catch (Exception ex)
        {
            var pdfException = new PdfGenerationException($"PDF生成失败: {outputPath}", PdfErrorType.Unknown, ex);
            _errorReportingService.ReportError(pdfException, new PdfErrorContext
            {
                Operation = "GeneratePdfAsync",
                FilePath = outputPath,
                ContentLength = content?.Length ?? 0,
                EngineType = "NativePdfEngine"
            });
            throw pdfException;
        }
    }
    
    /// <summary>
    /// 从 LaTeX 内容生成 PDF
    /// </summary>
    /// <param name="latexContent">LaTeX 内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    public async Task<bool> GenerateFromLatexAsync(string latexContent, string outputPath)
    {
        try
        {
            // 检查参数
            if (string.IsNullOrEmpty(latexContent))
            {
                throw new PdfGenerationException("LaTeX内容不能为空", PdfErrorType.ConfigurationError);
            }
            
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new PdfGenerationException("输出路径不能为空", PdfErrorType.ConfigurationError);
            }
            
            // 检查缓存
            if (_enableCache)
            {
                var cachedPath = _cacheService.GetCachedPdf(latexContent);
                if (cachedPath != null && File.Exists(cachedPath))
                {
                    try
                    {
                        // 从缓存复制文件
                        File.Copy(cachedPath, outputPath, true);
                        _logger.LogInformation("从缓存生成LaTeX PDF成功: {OutputPath}", outputPath);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "从缓存复制文件失败，将重新生成");
                    }
                }
            }
            
            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    throw new PdfGenerationException($"无法创建输出目录: {outputDir}", PdfErrorType.PermissionError, ex);
                }
            }
            
            // 解析LaTeX内容
            LatexDocument document;
            try
            {
                document = _latexParser.Parse(latexContent);
            }
            catch (Exception ex)
            {
                throw new PdfGenerationException("LaTeX内容解析失败", PdfErrorType.LatexParsingError, ex);
            }
            
            // 创建PDF文档
            var pdfDocument = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("ChineseFont"));
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeLatexContent(c, document));
                    page.Footer().Element(ComposeFooter);
                });
            });
            
            // 生成PDF到内存流
            using var ms = new MemoryStream();
            pdfDocument.GeneratePdf(ms);
            var pdfData = ms.ToArray();
            
            // 写入输出文件
            await File.WriteAllBytesAsync(outputPath, pdfData);
            
            // 缓存PDF
            if (_enableCache)
            {
                try
                {
                    await _cacheService.CachePdfAsync(latexContent, pdfData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "缓存LaTeX PDF失败");
                }
            }
            
            _logger.LogInformation("LaTeX转PDF成功: {OutputPath}", outputPath);
            return true;
        }
        catch (PdfGenerationException)
        {
            // 重新抛出PDF生成异常
            throw;
        }
        catch (Exception ex)
        {
            var pdfException = new PdfGenerationException($"LaTeX转PDF失败: {outputPath}", PdfErrorType.Unknown, ex);
            _errorReportingService.ReportError(pdfException, new PdfErrorContext
            {
                Operation = "GenerateFromLatexAsync",
                FilePath = outputPath,
                ContentLength = latexContent?.Length ?? 0,
                EngineType = "NativePdfEngine"
            });
            throw pdfException;
        }
    }
    
    /// <summary>
    /// 生成 PDF 预览图像
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GeneratePreviewAsync(string pdfPath, int width = 800, int height = 600)
    {
        try
        {
            if (string.IsNullOrEmpty(pdfPath))
            {
                throw new PdfGenerationException("PDF文件路径不能为空", PdfErrorType.ConfigurationError);
            }
            
            if (!File.Exists(pdfPath))
            {
                throw new PdfGenerationException($"PDF文件不存在: {pdfPath}", PdfErrorType.FileIoError);
            }
            
            // 检查文件大小
            var fileInfo = new FileInfo(pdfPath);
            if (fileInfo.Length == 0)
            {
                throw new PdfGenerationException($"PDF文件为空: {pdfPath}", PdfErrorType.FileIoError);
            }
            
            // 检查尺寸参数
            if (width <= 0 || height <= 0)
            {
                throw new PdfGenerationException("图像宽度和高度必须大于0", PdfErrorType.ConfigurationError);
            }
            
            // 使用PdfiumViewer库渲染PDF页面
            // 这确保预览与实际PDF内容一致
            using var document = PdfiumViewer.PdfDocument.Load(pdfPath);
            
            // 获取第一页
            var page = document.PageSizes[0];
            
            // 计算缩放比例以保持宽高比
            var pageWidth = page.Width;
            var pageHeight = page.Height;
            var scale = Math.Min((double)width / pageWidth, (double)height / pageHeight);
            
            // 计算实际渲染尺寸
            var renderWidth = (int)(pageWidth * scale);
            var renderHeight = (int)(pageHeight * scale);
            
            // 渲染PDF页面为图像
            using var image = document.Render(0, renderWidth, renderHeight, 96, 96, PdfiumViewer.PdfRenderFlags.Annotations);
            
            // 转换为ImageSharp图像以便进一步处理
            using var inputStream = new MemoryStream();
            image.Save(inputStream, System.Drawing.Imaging.ImageFormat.Png);
            inputStream.Position = 0;
            
            using var sharpImage = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
            
            // 将图像转换为字节数组
            using var outputStream = new MemoryStream();
            sharpImage.SaveAsPngAsync(outputStream).Wait();
            
            _logger.LogInformation("PDF预览图像生成成功: {PdfPath}", pdfPath);
            return outputStream.ToArray();
        }
        catch (PdfGenerationException)
        {
            // 重新抛出PDF生成异常
            throw;
        }
        catch (Exception ex)
        {
            var pdfException = new PdfGenerationException($"PDF预览图像生成失败: {pdfPath}", PdfErrorType.Unknown, ex);
            _errorReportingService.ReportError(pdfException, new PdfErrorContext
            {
                Operation = "GeneratePreviewAsync",
                FilePath = pdfPath,
                EngineType = "NativePdfEngine",
                AdditionalData = new Dictionary<string, object>
                {
                    { "Width", width },
                    { "Height", height }
                }
            });
            throw pdfException;
        }
    }
    
    /// <summary>
    /// 组合页眉
    /// </summary>
    /// <param name="container">容器</param>
    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // 第一行：标题和日期
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("QuizForge").FontSize(16).Bold().FontFamily("ChineseFont").FontColor(Colors.Blue.Darken2);
                row.ConstantItem(100).Height(25).AlignRight().AlignMiddle().Text(DateTime.Now.ToString("yyyy-MM-dd")).FontSize(10).FontFamily("ChineseFont");
            });
            
            // 第二行：密封线
            column.Item().Row(row =>
            {
                // 使用静态文本作为密封线标记
                row.ConstantItem(80).Height(20).AlignLeft().AlignMiddle().Border(1).BorderColor(QuestPDF.Helpers.Colors.Red).Padding(2).Text("密封线 (左)").FontSize(10).FontFamily("ChineseFont").FontColor(QuestPDF.Helpers.Colors.Red);
                row.RelativeItem();
                row.ConstantItem(80).Height(20).AlignRight().AlignMiddle().Border(1).BorderColor(QuestPDF.Helpers.Colors.Red).Padding(2).Text("密封线 (右)").FontSize(10).FontFamily("ChineseFont").FontColor(QuestPDF.Helpers.Colors.Red);
            });
            
            // 分隔线
            column.Item().LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
        });
    }
    
    /// <summary>
    /// 组合页脚
    /// </summary>
    /// <param name="container">容器</param>
    private void ComposeFooter(IContainer container)
    {
        container.LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        
        container.Row(row =>
        {
            row.RelativeItem().Text(x =>
            {
                x.Span("第 ");
                x.CurrentPageNumber().Format("第 {0} 页");
                x.Span(" 页，共 ");
                x.TotalPages().Format("共 {0} 页");
            }).FontSize(10).FontFamily("ChineseFont");
            
            row.ConstantItem(100).Height(25).AlignRight().AlignMiddle().Text("QuizForge").FontSize(10).FontFamily("ChineseFont");
        });
    }
    
    /// <summary>
    /// 组合内容
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="content">内容</param>
    private void ComposeContent(IContainer container, string content)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // 处理段落
            var paragraphs = content.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var paragraph in paragraphs)
            {
                var trimmedParagraph = paragraph.Trim();
                if (!string.IsNullOrEmpty(trimmedParagraph))
                {
                    column.Item().Text(trimmedParagraph).FontSize(12).FontFamily("ChineseFont");
                    column.Item().PaddingVertical(5);
                }
            }
        });
    }
    
    /// <summary>
    /// 组合LaTeX内容
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="document">LaTeX文档</param>
    private void ComposeLatexContent(IContainer container, LatexDocument document)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // 处理章节
            foreach (var section in document.Sections)
            {
                column.Item().Element(c => ComposeSection(c, section));
            }
            
            // 处理表格
            foreach (var table in document.Tables)
            {
                column.Item().Element(c => ComposeTable(c, table));
            }
            
            // 处理列表
            foreach (var list in document.Lists)
            {
                column.Item().Element(c => ComposeList(c, list));
            }
            
            // 处理数学公式
            foreach (var math in document.MathElements)
            {
                column.Item().Element(c => ComposeMath(c, math));
            }
        });
    }
    
    /// <summary>
    /// 组合章节
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="section">章节</param>
    private void ComposeSection(IContainer container, LatexSection section)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(10);
            
            switch (section.Level)
            {
                case 1:
                    column.Item().Text(section.Title).FontSize(18).Bold().FontFamily("ChineseFont").FontColor(Colors.Blue.Darken1);
                    break;
                case 2:
                    column.Item().Text(section.Title).FontSize(16).Bold().FontFamily("ChineseFont");
                    break;
                case 3:
                    column.Item().Text(section.Title).FontSize(14).Bold().FontFamily("ChineseFont");
                    break;
            }
        });
    }
    
    /// <summary>
    /// 组合表格
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="table">表格</param>
    private void ComposeTable(IContainer container, LatexTable table)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(10);
            
            // 创建表格
            column.Item().Table(tableContainer =>
            {
                // 定义列
                var columnCount = table.ColumnDefinitions.Count;
                if (columnCount == 0)
                {
                    columnCount = table.Rows.Count > 0 ? table.Rows[0].Count : 1;
                }
                
                for (int i = 0; i < columnCount; i++)
                {
                    tableContainer.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                    });
                }
                
                // 添加表头
                if (table.Rows.Count > 0)
                {
                    tableContainer.Header(header =>
                    {
                        for (int i = 0; i < table.Rows[0].Count; i++)
                        {
                            header.Cell().Element(cell =>
                            {
                                cell.Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().AlignMiddle();
                                cell.Text(table.Rows[0][i]).FontSize(12).Bold();
                            });
                        }
                    });
                }
                
                // 添加数据行
                for (int i = 1; i < table.Rows.Count; i++)
                {
                    for (int j = 0; j < table.Rows[i].Count; j++)
                    {
                        tableContainer.Cell().Element(cell =>
                        {
                            cell.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5);
                            cell.Text(table.Rows[i][j]).FontSize(10);
                        });
                    }
                }
            });
        });
    }
    
    /// <summary>
    /// 组合列表
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="list">列表</param>
    private void ComposeList(IContainer container, LatexList list)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(5);
            
            for (int i = 0; i < list.Items.Count; i++)
            {
                var item = list.Items[i];
                var prefix = list.Type == "itemize" ? "•" : $"{i + 1}.";
                
                column.Item().Row(row =>
                {
                    row.ConstantItem(20).Text(prefix).FontSize(12);
                    row.RelativeItem().Text(item).FontSize(12);
                });
            }
        });
    }
    
    /// <summary>
    /// 组合数学公式
    /// </summary>
    /// <param name="container">容器</param>
    /// <param name="math">数学公式</param>
    private void ComposeMath(IContainer container, LatexMathElement math)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(5);
            
            if (math.Type == "inline")
            {
                // 行内公式
                column.Item().Text($"${math.Content}$").FontSize(12).FontColor(Colors.Purple.Darken1);
            }
            else
            {
                // 行间公式
                column.Item().AlignCenter().Element(c =>
                {
                    try
                    {
                        // 渲染数学公式为图像
                        var imageData = _mathRenderer.RenderDisplayMath(math.Content ?? string.Empty);
                        c.Image(imageData);
                    }
                    catch
                    {
                        // 如果渲染失败，显示原始LaTeX代码
                        c.Text($"\\[{math.Content}\\]").FontSize(12).FontColor(QuestPDF.Helpers.Colors.Purple.Darken1);
                    }
                });
            }
        });
    }
}