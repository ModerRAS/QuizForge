using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.App.ViewModels;
using QuizForge.Infrastructure.Services;
using Xunit;

namespace QuizForge.Tests.ViewModels;

/// <summary>
/// PDF预览视图模型测试类
/// </summary>
public class PdfPreviewViewModelTests : IDisposable
{
    private readonly Mock<ILogger<PdfPreviewViewModel>> _mockLogger;
    private readonly Mock<ILogger<PrintPreviewService>> _mockPrintPreviewLogger;
    private readonly Mock<IPdfEngine> _mockPdfEngine;
    private readonly Mock<IPrintPreviewService> _mockPrintPreviewService;
    private readonly Mock<IExamPaperService> _mockExamPaperService;
    private readonly string _testOutputDirectory;
    private readonly PdfPreviewViewModel _viewModel;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfPreviewViewModelTests()
    {
        // 初始化Mock对象
        _mockLogger = new Mock<ILogger<PdfPreviewViewModel>>();
        _mockPrintPreviewLogger = new Mock<ILogger<PrintPreviewService>>();
        _mockPdfEngine = new Mock<IPdfEngine>();
        _mockPrintPreviewService = new Mock<IPrintPreviewService>();
        _mockExamPaperService = new Mock<IExamPaperService>();

        // 设置测试输出目录
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "Tests");
        
        // 确保测试输出目录存在
        if (!Directory.Exists(_testOutputDirectory))
        {
            Directory.CreateDirectory(_testOutputDirectory);
        }

        // 创建视图模型实例
        _viewModel = new PdfPreviewViewModel(
            _mockLogger.Object,
            _mockPrintPreviewService.Object,
            _mockExamPaperService.Object);
    }

    /// <summary>
    /// 测试初始化属性
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.NotNull(_viewModel);
        Assert.Null(_viewModel.SelectedExamPaper);
        Assert.Equal(1, _viewModel.CurrentPage);
        Assert.Equal(1.0, _viewModel.ZoomLevel);
        Assert.Equal(0, _viewModel.PageCount);
        Assert.Null(_viewModel.CurrentPreviewImage);
        Assert.NotNull(_viewModel.ThumbnailImages);
        Assert.Empty(_viewModel.ThumbnailImages);
        Assert.NotNull(_viewModel.PreviewConfig);
        Assert.True(_viewModel.ShowSealLine);
        Assert.Equal(PreviewQuality.Medium, _viewModel.PreviewQuality);
        Assert.Equal(1.0, _viewModel.Brightness);
        Assert.Equal(1.0, _viewModel.Contrast);
        Assert.Equal(PreviewDisplayMode.SinglePage, _viewModel.DisplayMode);
        Assert.NotNull(_viewModel.GoToFirstPageCommand);
        Assert.NotNull(_viewModel.GoToPreviousPageCommand);
        Assert.NotNull(_viewModel.GoToNextPageCommand);
        Assert.NotNull(_viewModel.GoToLastPageCommand);
        Assert.NotNull(_viewModel.ZoomInCommand);
        Assert.NotNull(_viewModel.ZoomOutCommand);
        Assert.NotNull(_viewModel.ZoomToFitCommand);
        Assert.NotNull(_viewModel.RotateLeftCommand);
        Assert.NotNull(_viewModel.RotateRightCommand);
        Assert.NotNull(_viewModel.RefreshPreviewCommand);
        Assert.NotNull(_viewModel.ToggleSealLineCommand);
        Assert.NotNull(_viewModel.ToggleDisplayModeCommand);
    }

    /// <summary>
    /// 测试选择考试试卷
    /// </summary>
    [Fact]
    public async Task SelectedExamPaperChanged_ShouldLoadPreview()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // Act
        _viewModel.SelectedExamPaper = examPaper;

        // Assert
        Assert.Equal(examPaper, _viewModel.SelectedExamPaper);
        Assert.Equal(pageCount, _viewModel.PageCount);
        Assert.Equal(1, _viewModel.CurrentPage);
        Assert.NotNull(_viewModel.CurrentPreviewImage);
    }

    /// <summary>
    /// 测试当前页面变化
    /// </summary>
    [Fact]
    public async Task CurrentPageChanged_ShouldUpdatePreview()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        
        // Act
        _viewModel.CurrentPage = 3;

        // Assert
        Assert.Equal(3, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试缩放级别变化
    /// </summary>
    [Fact]
    public void ZoomLevelChanged_ShouldUpdateZoomLevel()
    {
        // Arrange
        var newZoomLevel = 1.5;

        // Act
        _viewModel.ZoomLevel = newZoomLevel;

        // Assert
        Assert.Equal(newZoomLevel, _viewModel.ZoomLevel);
    }

    /// <summary>
    /// 测试显示模式变化
    /// </summary>
    [Fact]
    public void DisplayModeChanged_ShouldUpdateDisplayMode()
    {
        // Arrange
        var newDisplayMode = PreviewDisplayMode.TwoPage;

        // Act
        _viewModel.DisplayMode = newDisplayMode;

        // Assert
        Assert.Equal(newDisplayMode, _viewModel.DisplayMode);
    }

    /// <summary>
    /// 测试密封线显示变化
    /// </summary>
    [Fact]
    public void ShowSealLineChanged_ShouldUpdateShowSealLine()
    {
        // Arrange
        var newShowSealLine = false;

        // Act
        _viewModel.ShowSealLine = newShowSealLine;

        // Assert
        Assert.Equal(newShowSealLine, _viewModel.ShowSealLine);
    }

    /// <summary>
    /// 测试预览质量变化
    /// </summary>
    [Fact]
    public void PreviewQualityChanged_ShouldUpdatePreviewQuality()
    {
        // Arrange
        var newPreviewQuality = PreviewQuality.High;

        // Act
        _viewModel.PreviewQuality = newPreviewQuality;

        // Assert
        Assert.Equal(newPreviewQuality, _viewModel.PreviewQuality);
    }

    /// <summary>
    /// 测试亮度变化
    /// </summary>
    [Fact]
    public void BrightnessChanged_ShouldUpdateBrightness()
    {
        // Arrange
        var newBrightness = 1.2;

        // Act
        _viewModel.Brightness = newBrightness;

        // Assert
        Assert.Equal(newBrightness, _viewModel.Brightness);
    }

    /// <summary>
    /// 测试对比度变化
    /// </summary>
    [Fact]
    public void ContrastChanged_ShouldUpdateContrast()
    {
        // Arrange
        var newContrast = 1.2;

        // Act
        _viewModel.Contrast = newContrast;

        // Assert
        Assert.Equal(newContrast, _viewModel.Contrast);
    }

    /// <summary>
    /// 测试转到首页命令
    /// </summary>
    [Fact]
    public void GoToFirstPageCommand_ShouldSetCurrentPageToOne()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.GoToFirstPageCommand.Execute(null);

        // Assert
        Assert.Equal(1, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到上一页命令
    /// </summary>
    [Fact]
    public void GoToPreviousPageCommand_ShouldDecrementCurrentPage()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.GoToPreviousPageCommand.Execute(null);

        // Assert
        Assert.Equal(2, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到下一页命令
    /// </summary>
    [Fact]
    public void GoToNextPageCommand_ShouldIncrementCurrentPage()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.GoToNextPageCommand.Execute(null);

        // Assert
        Assert.Equal(4, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到末页命令
    /// </summary>
    [Fact]
    public void GoToLastPageCommand_ShouldSetCurrentPageToPageCount()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.GoToLastPageCommand.Execute(null);

        // Assert
        Assert.Equal(pageCount, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试放大命令
    /// </summary>
    [Fact]
    public void ZoomInCommand_ShouldIncreaseZoomLevel()
    {
        // Arrange
        var initialZoomLevel = 1.0;

        // Act
        _viewModel.ZoomInCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.ZoomLevel > initialZoomLevel);
    }

    /// <summary>
    /// 测试缩小命令
    /// </summary>
    [Fact]
    public void ZoomOutCommand_ShouldDecreaseZoomLevel()
    {
        // Arrange
        var initialZoomLevel = 1.0;

        // Act
        _viewModel.ZoomOutCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.ZoomLevel < initialZoomLevel);
    }

    /// <summary>
    /// 测试适应窗口命令
    /// </summary>
    [Fact]
    public void ZoomToFitCommand_ShouldSetZoomLevelToOne()
    {
        // Arrange
        _viewModel.ZoomLevel = 1.5;

        // Act
        _viewModel.ZoomToFitCommand.Execute(null);

        // Assert
        Assert.Equal(1.0, _viewModel.ZoomLevel);
    }

    /// <summary>
    /// 测试左旋转命令
    /// </summary>
    [Fact]
    public void RotateLeftCommand_ShouldRotateImageLeft()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);
        
        _mockPrintPreviewService.Setup(x => x.RotatePreviewImageAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;

        // Act
        _viewModel.RotateLeftCommand.Execute(null);

        // Assert
        // 验证旋转方法被调用
        _mockPrintPreviewService.Verify(x => x.RotatePreviewImageAsync(It.IsAny<byte[]>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 测试右旋转命令
    /// </summary>
    [Fact]
    public void RotateRightCommand_ShouldRotateImageRight()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);
        
        _mockPrintPreviewService.Setup(x => x.RotatePreviewImageAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;

        // Act
        _viewModel.RotateRightCommand.Execute(null);

        // Assert
        // 验证旋转方法被调用
        _mockPrintPreviewService.Verify(x => x.RotatePreviewImageAsync(It.IsAny<byte[]>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 测试刷新预览命令
    /// </summary>
    [Fact]
    public void RefreshPreviewCommand_ShouldReloadPreview()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content",
            FilePath = Path.Combine(_testOutputDirectory, $"{Guid.NewGuid()}.pdf")
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;

        // Act
        _viewModel.RefreshPreviewCommand.Execute(null);

        // Assert
        // 验证预览生成方法被调用
        _mockPrintPreviewService.Verify(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// 测试切换密封线命令
    /// </summary>
    [Fact]
    public void ToggleSealLineCommand_ShouldToggleSealLineVisibility()
    {
        // Arrange
        var initialShowSealLine = _viewModel.ShowSealLine;

        // Act
        _viewModel.ToggleSealLineCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialShowSealLine, _viewModel.ShowSealLine);
    }

    /// <summary>
    /// 测试切换显示模式命令
    /// </summary>
    [Fact]
    public void ToggleDisplayModeCommand_ShouldToggleDisplayMode()
    {
        // Arrange
        var initialDisplayMode = _viewModel.DisplayMode;

        // Act
        _viewModel.ToggleDisplayModeCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialDisplayMode, _viewModel.DisplayMode);
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