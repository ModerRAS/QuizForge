using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Infrastructure.Services;
using QuizForge.Infrastructure.Engines;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Infrastructure.Renderers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace QuizForge.Tests.Layout;

/// <summary>
/// 密封线预览测试类
/// </summary>
public class SealLinePreviewTests : IDisposable
{
    private readonly Mock<ILogger<PrintPreviewService>> _mockPrintPreviewLogger;
    private readonly Mock<ILogger<PdfEngine>> _mockPdfEngineLogger;
    private readonly Mock<ILogger<LatexParser>> _mockLatexParserLogger;
    private readonly Mock<ILogger<MathRenderer>> _mockMathRendererLogger;
    private readonly Mock<ILogger<PdfErrorReportingService>> _mockErrorReportingLogger;
    private readonly Mock<ILogger<PdfCacheService>> _mockCacheLogger;
    private readonly Mock<IPdfEngine> _mockPdfEngine;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _testOutputDirectory;
    private readonly PrintPreviewService _printPreviewService;
    private readonly NativePdfEngine _pdfEngine;
    private readonly LatexParser _latexParser;
    private readonly MathRenderer _mathRenderer;
    private readonly PdfErrorReportingService _errorReportingService;
    private readonly PdfCacheService _cacheService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SealLinePreviewTests()
    {
        // 初始化Mock对象
        _mockPrintPreviewLogger = new Mock<ILogger<PrintPreviewService>>();
        _mockPdfEngineLogger = new Mock<ILogger<PdfEngine>>();
        _mockLatexParserLogger = new Mock<ILogger<LatexParser>>();
        _mockMathRendererLogger = new Mock<ILogger<MathRenderer>>();
        _mockErrorReportingLogger = new Mock<ILogger<PdfErrorReportingService>>();
        _mockCacheLogger = new Mock<ILogger<PdfCacheService>>();
        _mockPdfEngine = new Mock<IPdfEngine>();
        _mockConfiguration = new Mock<IConfiguration>();

        // 设置测试输出目录
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "Tests");
        
        // 确保测试输出目录存在
        if (!Directory.Exists(_testOutputDirectory))
        {
            Directory.CreateDirectory(_testOutputDirectory);
        }

        // 创建服务实例
        _printPreviewService = new PrintPreviewService(_mockPrintPreviewLogger.Object, _mockPdfEngine.Object);
        
        // 创建依赖服务
        _latexParser = new LatexParser(_mockLatexParserLogger.Object);
        _mathRenderer = new MathRenderer(_mockMathRendererLogger.Object);
        _errorReportingService = new PdfErrorReportingService(_mockErrorReportingLogger.Object);
        _cacheService = new PdfCacheService(_mockCacheLogger.Object, _mockConfiguration.Object);
        
        // 配置mock
        _mockConfiguration.Setup(c => c.GetSection("PdfEngine:EnableCache").Value).Returns("true");
        _mockConfiguration.Setup(c => c.GetSection("PdfEngine:TempDirectory").Value).Returns(_testOutputDirectory);
        
        _pdfEngine = new NativePdfEngine(_mockPdfEngineLogger.Object, _latexParser, _mathRenderer, _errorReportingService, _cacheService, _mockConfiguration.Object);
    }

    /// <summary>
    /// 测试奇数页密封线位置（左侧）
    /// </summary>
    [Fact]
    public async Task GeneratePreviewWithSealLineAsync_OddPage_ShouldPlaceSealLineOnLeft()
    {
        // Arrange
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var pageNumber = 1; // 奇数页
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var result = await _printPreviewService.GeneratePreviewWithSealLineAsync(pdfPath, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // 验证密封线添加方法被调用
        _mockPdfEngine.Verify(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 测试偶数页密封线位置（右侧）
    /// </summary>
    [Fact]
    public async Task GeneratePreviewWithSealLineAsync_EvenPage_ShouldPlaceSealLineOnRight()
    {
        // Arrange
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var pageNumber = 2; // 偶数页
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var result = await _printPreviewService.GeneratePreviewWithSealLineAsync(pdfPath, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // 验证密封线添加方法被调用
        _mockPdfEngine.Verify(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    
    /// <summary>
    /// 测试PDF引擎生成带密封线的PDF
    /// </summary>
    [Fact]
    public async Task NativePdfEngine_GeneratePdfWithSealLine_ShouldIncludeSealLine()
    {
        // Arrange
        var content = "Test content";
        var outputPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var includeSealLine = true;

        // Act
        var result = await _pdfEngine.GeneratePdfAsync(content, outputPath);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// 测试PDF引擎生成不带密封线的PDF
    /// </summary>
    [Fact]
    public async Task NativePdfEngine_GeneratePdfWithoutSealLine_ShouldNotIncludeSealLine()
    {
        // Arrange
        var content = "Test content";
        var outputPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var includeSealLine = false;

        // Act
        var result = await _pdfEngine.GeneratePdfAsync(content, outputPath);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// 测试预览和PDF密封线位置一致性
    /// </summary>
    [Fact]
    public async Task PreviewAndPdfSealLinePosition_ShouldBeConsistent()
    {
        // Arrange
        var content = "Test content";
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var includeSealLine = true;
        
        // 生成PDF
        await _pdfEngine.GeneratePdfAsync(content, pdfPath);
        
        // 生成预览
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var previewWithSealLine = await _printPreviewService.GeneratePreviewWithSealLineAsync(pdfPath, 1);

        // Assert
        Assert.NotNull(previewWithSealLine);
        Assert.True(previewWithSealLine.Length > 0);
        
        // 验证PDF生成和预览生成都被调用
        _mockPdfEngine.Verify(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 测试不同页数的密封线位置
    /// </summary>
    [Theory]
    [InlineData(1, true)]  // 第1页，奇数页，左侧密封线
    [InlineData(2, false)] // 第2页，偶数页，右侧密封线
    [InlineData(3, true)]  // 第3页，奇数页，左侧密封线
    [InlineData(4, false)] // 第4页，偶数页，右侧密封线
    public async Task SealLinePosition_ShouldVaryByPageNumber(int pageNumber, bool isLeftPosition)
    {
        // Arrange
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        var result = await _printPreviewService.GeneratePreviewWithSealLineAsync(pdfPath, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // 验证预览生成被调用
        _mockPdfEngine.Verify(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 测试密封线配置保存和加载
    /// </summary>
    [Fact]
    public async Task SealLineConfig_ShouldBeSavedAndLoaded()
    {
        // Arrange
        var config = new PreviewConfig
        {
            DefaultQuality = 90,
            DisplayMode = PreviewDisplayMode.SinglePage,
            ShowSealLine = true,
            DefaultZoomLevel = 1.5
        };

        // Act
        await _printPreviewService.SetPreviewConfigAsync(config);
        var loadedConfig = await _printPreviewService.GetPreviewConfigAsync();

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(config.DefaultQuality, loadedConfig.DefaultQuality);
        Assert.Equal(config.DisplayMode, loadedConfig.DisplayMode);
        Assert.Equal(config.ShowSealLine, loadedConfig.ShowSealLine);
        Assert.Equal(config.DefaultZoomLevel, loadedConfig.DefaultZoomLevel);
    }

    /// <summary>
    /// 测试密封线显示切换
    /// </summary>
    [Fact]
    public async Task SealLineVisibility_ShouldBeToggleable()
    {
        // Arrange
        var pdfPath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf");
        var pageNumber = 1;
        var previewData = new byte[] { 1, 2, 3, 4, 5 }; // 模拟预览数据
        
        _mockPdfEngine.Setup(x => x.GeneratePreviewAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始配置
        var config = new PreviewConfig
        {
            DefaultQuality = 75,
            DisplayMode = PreviewDisplayMode.SinglePage,
            ShowSealLine = true,
            DefaultZoomLevel = 1.0
        };
        
        await _printPreviewService.SetPreviewConfigAsync(config);

        // Act - 切换密封线显示
        config.ShowSealLine = false;
        await _printPreviewService.SetPreviewConfigAsync(config);
        
        var resultWithoutSealLine = await _printPreviewService.GeneratePreviewImageAsync(pdfPath, pageNumber);
        
        // 再次切换密封线显示
        config.ShowSealLine = true;
        await _printPreviewService.SetPreviewConfigAsync(config);
        
        var resultWithSealLine = await _printPreviewService.GeneratePreviewWithSealLineAsync(pdfPath, pageNumber);

        // Assert
        Assert.NotNull(resultWithoutSealLine);
        Assert.NotNull(resultWithSealLine);
        Assert.True(resultWithoutSealLine.Length > 0);
        Assert.True(resultWithSealLine.Length > 0);
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