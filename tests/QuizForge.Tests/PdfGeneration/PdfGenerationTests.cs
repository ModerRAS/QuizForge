using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Infrastructure.Engines;
using QuizForge.Infrastructure.Services;
using SixLabors.ImageSharp;

namespace QuizForge.Tests.PdfGeneration;

/// <summary>
/// PDF生成测试类
/// </summary>
public class PdfGenerationTests : IDisposable
{
    private readonly Mock<ILogger<PdfEngine>> _mockPdfEngineLogger;
    private readonly Mock<ILogger<PrintPreviewService>> _mockPrintPreviewLogger;
    private readonly Mock<ILogger<ExportService>> _mockExportServiceLogger;
    private readonly Mock<ILogger<FileService>> _mockFileServiceLogger;
    private readonly Mock<IPdfEngine> _mockPdfEngine;
    private readonly Mock<IPrintPreviewService> _mockPrintPreviewService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IQuestionService> _mockQuestionService;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IGenerationService> _mockGenerationService;
    private readonly string _testOutputDirectory;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfGenerationTests()
    {
        // 初始化Mock对象
        _mockPdfEngineLogger = new Mock<ILogger<PdfEngine>>();
        _mockPrintPreviewLogger = new Mock<ILogger<PrintPreviewService>>();
        _mockExportServiceLogger = new Mock<ILogger<ExportService>>();
        _mockFileServiceLogger = new Mock<ILogger<FileService>>();
        _mockPdfEngine = new Mock<IPdfEngine>();
        _mockPrintPreviewService = new Mock<IPrintPreviewService>();
        _mockFileService = new Mock<IFileService>();
        _mockExportService = new Mock<IExportService>();
        _mockQuestionService = new Mock<IQuestionService>();
        _mockTemplateService = new Mock<ITemplateService>();
        _mockGenerationService = new Mock<IGenerationService>();

        // 设置测试输出目录
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "Tests");
        
        // 确保测试输出目录存在
        if (!Directory.Exists(_testOutputDirectory))
        {
            Directory.CreateDirectory(_testOutputDirectory);
        }
    }

    /// <summary>
    /// 测试PDF引擎生成PDF
    /// </summary>
    [Fact]
    public async Task PdfEngine_GeneratePdfAsync_ShouldReturnTrue()
    {
        // Arrange
        var pdfEngine = new PdfEngine(_mockPdfEngineLogger.Object);
        var content = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var outputPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");

        // Act
        var result = await pdfEngine.GeneratePdfAsync(content, outputPath);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// 测试PDF引擎从LaTeX生成PDF
    /// </summary>
    [Fact]
    public async Task PdfEngine_GenerateFromLatexAsync_ShouldReturnTrue()
    {
        // Arrange
        var pdfEngine = new PdfEngine(_mockPdfEngineLogger.Object);
        var latexContent = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var outputPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");

        // Act
        var result = await pdfEngine.GenerateFromLatexAsync(latexContent, outputPath);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// 测试PDF引擎生成预览图像
    /// </summary>
    [Fact]
    public async Task PdfEngine_GeneratePreviewAsync_ShouldReturnImageData()
    {
        // Arrange
        var pdfEngine = new PdfEngine(_mockPdfEngineLogger.Object);
        var content = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        
        // 先生成PDF
        await pdfEngine.GenerateFromLatexAsync(content, pdfPath);

        // Act
        var previewData = await pdfEngine.GeneratePreviewAsync(pdfPath);

        // Assert
        Assert.NotNull(previewData);
        Assert.True(previewData.Length > 0);
    }

    /// <summary>
    /// 测试打印预览服务生成预览图像
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_GeneratePreviewImageAsync_ShouldReturnImageData()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var result = await printPreviewService.GeneratePreviewImageAsync(pdfPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(previewData, result);
    }

    /// <summary>
    /// 测试打印预览服务生成所有页面预览图像
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_GenerateAllPreviewImagesAsync_ShouldReturnImageDataList()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        var pageCount = 3;
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);
        
        _mockPdfEngine.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);

        // Act
        var result = await printPreviewService.GenerateAllPreviewImagesAsync(pdfPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pageCount, result.Count);
        Assert.All(result, data => Assert.Equal(previewData, data));
    }

    /// <summary>
    /// 测试打印预览服务获取PDF页面数量
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_GetPageCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var expectedCount = 5;
        
        _mockPdfEngine.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await printPreviewService.GetPageCountAsync(pdfPath);

        // Assert
        Assert.Equal(expectedCount, result);
    }

    /// <summary>
    /// 测试打印预览服务获取PDF信息
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_GetPdfInfoAsync_ShouldReturnPdfInfo()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var expectedPdfInfo = new PdfInfo
        {
            PageCount = 3,
            Title = "Test PDF",
            Author = "Test Author",
            FileSize = 1024
        };
        
        _mockPdfEngine.Setup(x => x.GetPdfInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedPdfInfo);

        // Act
        var result = await printPreviewService.GetPdfInfoAsync(pdfPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPdfInfo.PageCount, result.PageCount);
        Assert.Equal(expectedPdfInfo.Title, result.Title);
        Assert.Equal(expectedPdfInfo.Author, result.Author);
        Assert.Equal(expectedPdfInfo.FileSize, result.FileSize);
    }

    /// <summary>
    /// 测试打印预览服务缩放预览图像
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_ScalePreviewImageAsync_ShouldReturnScaledImageData()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var originalImageData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟图像数据
        var scale = 2.0;

        // Act
        var result = await printPreviewService.ScalePreviewImageAsync(originalImageData, scale);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// 测试打印预览服务旋转预览图像
    /// </summary>
    [Fact]
    public async Task PrintPreviewService_RotatePreviewImageAsync_ShouldReturnRotatedImageData()
    {
        // Arrange
        var printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        var originalImageData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟图像数据
        var angle = 90;

        // Act
        var result = await printPreviewService.RotatePreviewImageAsync(originalImageData, angle);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// 测试导出服务导出为PDF
    /// </summary>
    [Fact]
    public async Task ExportService_ExportToPdfAsync_ShouldReturnPdfPath()
    {
        // Arrange
        var exportService = new ExportService(
            _mockExportServiceLogger.Object,
            _mockPdfEngine.Object,
            _mockPrintPreviewService.Object,
            _mockFileService.Object);
        
        var latexContent = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var configuration = new ExportConfiguration
        {
            OutputPath = _testOutputDirectory,
            FileName = "TestExport",
            Format = ExportFormat.PDF
        };
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await exportService.ExportToPdfAsync(latexContent, configuration);

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".pdf", result);
        Assert.True(File.Exists(result));
    }

    /// <summary>
    /// 测试导出服务导出为LaTeX
    /// </summary>
    [Fact]
    public async Task ExportService_ExportToLaTeXAsync_ShouldReturnLatexPath()
    {
        // Arrange
        var exportService = new ExportService(
            _mockExportServiceLogger.Object,
            _mockPdfEngine.Object,
            _mockPrintPreviewService.Object,
            _mockFileService.Object);
        
        var latexContent = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var configuration = new ExportConfiguration
        {
            OutputPath = _testOutputDirectory,
            FileName = "TestExport",
            Format = ExportFormat.LaTeX
        };

        // Act
        var result = await exportService.ExportToLaTeXAsync(latexContent, configuration);

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".tex", result);
        Assert.True(File.Exists(result));
    }

    /// <summary>
    /// 测试导出服务导出为Word
    /// </summary>
    [Fact]
    public async Task ExportService_ExportToWordAsync_ShouldReturnWordPath()
    {
        // Arrange
        var exportService = new ExportService(
            _mockExportServiceLogger.Object,
            _mockPdfEngine.Object,
            _mockPrintPreviewService.Object,
            _mockFileService.Object);
        
        var latexContent = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var configuration = new ExportConfiguration
        {
            OutputPath = _testOutputDirectory,
            FileName = "TestExport",
            Format = ExportFormat.Word
        };
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.ConvertPdfToWordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await exportService.ExportToWordAsync(latexContent, configuration);

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".docx", result);
        Assert.True(File.Exists(result));
    }

    /// <summary>
    /// 测试导出服务生成预览图像
    /// </summary>
    [Fact]
    public async Task ExportService_GeneratePreviewImageAsync_ShouldReturnImageData()
    {
        // Arrange
        var exportService = new ExportService(
            _mockExportServiceLogger.Object,
            _mockPdfEngine.Object,
            _mockPrintPreviewService.Object,
            _mockFileService.Object);
        
        var latexContent = "\\documentclass{article}\\begin{document}Hello World\\end{document}";
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var result = await exportService.GeneratePreviewImageAsync(latexContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(previewData, result);
    }

    /// <summary>
    /// 测试文件服务复制文件
    /// </summary>
    [Fact]
    public async Task FileService_CopyFileAsync_ShouldCopyFile()
    {
        // Arrange
        var fileService = new FileService(_mockFileServiceLogger.Object);
        var sourcePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.txt");
        var destinationPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.txt");
        var content = "Test content";
        
        // 创建源文件
        await File.WriteAllTextAsync(sourcePath, content);

        // Act
        await fileService.CopyFileAsync(sourcePath, destinationPath);

        // Assert
        Assert.True(File.Exists(destinationPath));
        var destinationContent = await File.ReadAllTextAsync(destinationPath);
        Assert.Equal(content, destinationContent);
    }

    /// <summary>
    /// 测试文件服务将PDF转换为Word
    /// </summary>
    [Fact]
    public async Task FileService_ConvertPdfToWordAsync_ShouldCreateWordFile()
    {
        // Arrange
        var fileService = new FileService(_mockFileServiceLogger.Object);
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var wordPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.docx");
        
        // 创建PDF文件
        await File.WriteAllTextAsync(pdfPath, "PDF content");

        // Act
        await fileService.ConvertPdfToWordAsync(pdfPath, wordPath);

        // Assert
        Assert.True(File.Exists(wordPath));
    }

    /// <summary>
    /// 测试文件服务打印文档
    /// </summary>
    [Fact]
    public async Task FileService_PrintDocumentAsync_ShouldReturnTrue()
    {
        // Arrange
        var fileService = new FileService(_mockFileServiceLogger.Object);
        var filePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.txt");
        var settings = new PrintSettings
        {
            PrinterName = "Test Printer",
            Copies = 1
        };
        
        // 创建文件
        await File.WriteAllTextAsync(filePath, "Test content");

        // Act
        var result = await fileService.PrintDocumentAsync(filePath, settings);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 清理测试资源
    /// </summary>
    public void Dispose()
    {
        // 清理测试文件
        if (Directory.Exists(_testOutputDirectory))
        {
            try
            {
                Directory.Delete(_testOutputDirectory, true);
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
}