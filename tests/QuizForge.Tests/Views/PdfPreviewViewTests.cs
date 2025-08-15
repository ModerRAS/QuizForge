using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.App.ViewModels;
using QuizForge.App.Views;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Services;
using Xunit;

namespace QuizForge.Tests.Views;

/// <summary>
/// PDF预览视图测试类
/// </summary>
public class PdfPreviewViewTests : IDisposable
{
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IPrintPreviewService> _mockPrintPreviewService;
    private readonly string _testOutputDirectory;
    private readonly PdfPreviewViewModel _viewModel;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfPreviewViewTests()
    {
        // 初始化Mock对象
        _mockExportService = new Mock<IExportService>();
        _mockPrintPreviewService = new Mock<IPrintPreviewService>();

        // 设置测试输出目录
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "Tests");
        
        // 确保测试输出目录存在
        if (!Directory.Exists(_testOutputDirectory))
        {
            Directory.CreateDirectory(_testOutputDirectory);
        }

        // 创建视图模型实例
        _viewModel = new PdfPreviewViewModel(
            _mockExportService.Object,
            _mockPrintPreviewService.Object);
    }

    /// <summary>
    /// 测试视图构造函数
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeView()
    {
        // Arrange & Act
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };

        // Assert
        Assert.NotNull(view);
        Assert.Equal(_viewModel, view.DataContext);
    }

    /// <summary>
    /// 测试鼠标滚轮缩放功能
    /// </summary>
    [Fact]
    public void HandleMouseWheel_ShouldUpdateZoomLevel()
    {
        // Arrange
        var initialZoomLevel = _viewModel.ZoomLevel;
        
        // Act
        _viewModel.HandleMouseWheel(120); // 向上滚动

        // Assert
        Assert.True(_viewModel.ZoomLevel > initialZoomLevel);
    }

    /// <summary>
    /// 测试缩略图点击功能
    /// </summary>
    [Fact]
    public void HandleThumbnailClick_ShouldUpdateCurrentPage()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content"
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);
        
        _viewModel.SelectedExamPaper = examPaper;

        // Act
        _viewModel.HandleThumbnailClick(2); // 点击第3页（索引为2）

        // Assert
        Assert.Equal(3, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试工具栏按钮功能
    /// </summary>
    [Fact]
    public void ToolBarCommands_ShouldExecuteCorrectly()
    {
        // Arrange
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Test Exam Paper",
            Content = "Test Content"
        };
        
        var previewData = new byte[] { 1, 2, 3, 4, 5 };
        var pageCount = 5;
        
        _mockPrintPreviewService.Setup(x => x.GetPageCountAsync(It.IsAny<string>()))
            .ReturnsAsync(pageCount);
        
        _mockPrintPreviewService.Setup(x => x.GeneratePreviewImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(previewData);
        
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;
        
        var initialPage = _viewModel.CurrentPage;
        var initialZoom = _viewModel.ZoomLevel;

        // Act - 测试上一页按钮
        _viewModel.PreviousPageCommand.Execute(null);

        // Assert
        Assert.Equal(initialPage - 1, _viewModel.CurrentPage);

        // Act - 测试放大按钮
        _viewModel.ZoomInCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.ZoomLevel > initialZoom);
    }

    /// <summary>
    /// 测试显示模式切换
    /// </summary>
    [Fact]
    public void DisplayModeToggle_ShouldChangeDisplayMode()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialDisplayMode = _viewModel.DisplayMode;

        // Act
        _viewModel.ToggleDualPageViewCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialDisplayMode, _viewModel.DisplayMode);
    }

    /// <summary>
    /// 测试密封线显示切换
    /// </summary>
    [Fact]
    public void SealLineToggle_ShouldChangeSealLineVisibility()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialShowSealLine = _viewModel.ShowSealLine;

        // Act
        _viewModel.ToggleSealLineCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialShowSealLine, _viewModel.ShowSealLine);
    }

    /// <summary>
    /// 测试预览质量选择
    /// </summary>
    [Fact]
    public void PreviewQualitySelection_ShouldChangePreviewQuality()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialPreviewQuality = _viewModel.PreviewQuality;

        // Act
        _viewModel.PreviewQuality = 90;

        // Assert
        Assert.NotEqual(initialPreviewQuality, _viewModel.PreviewQuality);
        Assert.Equal(90, _viewModel.PreviewQuality);
    }

    /// <summary>
    /// 测试亮度调整
    /// </summary>
    [Fact]
    public void BrightnessAdjustment_ShouldChangeBrightness()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialBrightness = _viewModel.Brightness;
        var newBrightness = 20;

        // Act
        _viewModel.Brightness = newBrightness;

        // Assert
        Assert.NotEqual(initialBrightness, _viewModel.Brightness);
        Assert.Equal(newBrightness, _viewModel.Brightness);
    }

    /// <summary>
    /// 测试对比度调整
    /// </summary>
    [Fact]
    public void ContrastAdjustment_ShouldChangeContrast()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialContrast = _viewModel.Contrast;
        var newContrast = 20;

        // Act
        _viewModel.Contrast = newContrast;

        // Assert
        Assert.NotEqual(initialContrast, _viewModel.Contrast);
        Assert.Equal(newContrast, _viewModel.Contrast);
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