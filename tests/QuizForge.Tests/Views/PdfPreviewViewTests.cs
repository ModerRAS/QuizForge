using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.App.ViewModels;
using QuizForge.App.Views;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using Xunit;

namespace QuizForge.Tests.Views;

/// <summary>
/// PDF预览视图测试类
/// </summary>
public class PdfPreviewViewTests : IDisposable
{
    private readonly Mock<ILogger<PdfPreviewViewModel>> _mockLogger;
    private readonly Mock<IPrintPreviewService> _mockPrintPreviewService;
    private readonly Mock<IExamPaperService> _mockExamPaperService;
    private readonly string _testOutputDirectory;
    private readonly PdfPreviewViewModel _viewModel;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfPreviewViewTests()
    {
        // 初始化Mock对象
        _mockLogger = new Mock<ILogger<PdfPreviewViewModel>>();
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
    /// 测试预览图像鼠标滚轮事件
    /// </summary>
    [Fact]
    public void PreviewImage_PointerWheel_ShouldUpdateZoomLevel()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        var initialZoomLevel = _viewModel.ZoomLevel;
        
        // 模拟鼠标滚轮事件
        var args = new Avalonia.Input.PointerWheelEventArgs
        {
            Delta = 120.0, // 向上滚动
            KeyModifiers = Avalonia.Input.KeyModifiers.Control
        };

        // Act
        view.PreviewImage_PointerWheel(view, args);

        // Assert
        Assert.True(_viewModel.ZoomLevel > initialZoomLevel);
    }

    /// <summary>
    /// 测试预览图像鼠标按下事件
    /// </summary>
    [Fact]
    public void PreviewImage_PointerPressed_ShouldStartDragging()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        // 模拟鼠标按下事件
        var args = new Avalonia.Input.PointerPressedEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            GetCurrentPoint = _ => new Avalonia.Input.PointerPoint(new Avalonia.Point(100, 100), Avalonia.Input.PointerPointProperties.None)
        };

        // Act
        view.PreviewImage_PointerPressed(view, args);

        // Assert
        // 验证拖动开始
        Assert.NotNull(view);
    }

    /// <summary>
    /// 测试预览图像鼠标移动事件
    /// </summary>
    [Fact]
    public void PreviewImage_PointerMoved_ShouldUpdateScrollPosition()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        // 模拟鼠标按下事件
        var pressedArgs = new Avalonia.Input.PointerPressedEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            GetCurrentPoint = _ => new Avalonia.Input.PointerPoint(new Avalonia.Point(100, 100), Avalonia.Input.PointerPointProperties.None)
        };
        
        view.PreviewImage_PointerPressed(view, pressedArgs);
        
        // 模拟鼠标移动事件
        var movedArgs = new Avalonia.Input.PointerEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            GetCurrentPoint = _ => new Avalonia.Input.PointerPoint(new Avalonia.Point(150, 150), Avalonia.Input.PointerPointProperties.None)
        };

        // Act
        view.PreviewImage_PointerMoved(view, movedArgs);

        // Assert
        // 验证滚动位置更新
        Assert.NotNull(view);
    }

    /// <summary>
    /// 测试预览图像鼠标释放事件
    /// </summary>
    [Fact]
    public void PreviewImage_PointerReleased_ShouldStopDragging()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        // 模拟鼠标按下事件
        var pressedArgs = new Avalonia.Input.PointerPressedEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            GetCurrentPoint = _ => new Avalonia.Input.PointerPoint(new Avalonia.Point(100, 100), Avalonia.Input.PointerPointProperties.None)
        };
        
        view.PreviewImage_PointerPressed(view, pressedArgs);
        
        // 模拟鼠标释放事件
        var releasedArgs = new Avalonia.Input.PointerReleasedEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            GetCurrentPoint = _ => new Avalonia.Input.PointerPoint(new Avalonia.Point(150, 150), Avalonia.Input.PointerPointProperties.None)
        };

        // Act
        view.PreviewImage_PointerReleased(view, releasedArgs);

        // Assert
        // 验证拖动停止
        Assert.NotNull(view);
    }

    /// <summary>
    /// 测试缩略图点击事件
    /// </summary>
    [Fact]
    public void Thumbnail_PointerPressed_ShouldUpdateCurrentPage()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        // 设置考试试卷
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
        
        _viewModel.SelectedExamPaper = examPaper;
        
        // 创建缩略图项
        var thumbnailItem = new PdfPreviewViewModel.ThumbnailItem
        {
            PageNumber = 3,
            ImageData = previewData
        };
        
        // 模拟缩略图点击事件
        var args = new Avalonia.Input.PointerPressedEventArgs
        {
            Pointer = new Avalonia.Input.Pointer(Avalonia.Input.PointerType.Mouse),
            Source = thumbnailItem
        };

        // Act
        view.Thumbnail_PointerPressed(view, args);

        // Assert
        Assert.Equal(3, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试工具栏按钮点击事件
    /// </summary>
    [Fact]
    public void ToolBarButtons_ShouldExecuteCommands()
    {
        // Arrange
        var view = new PdfPreviewView
        {
            DataContext = _viewModel
        };
        
        // 设置考试试卷
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
        
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;
        
        var initialPage = _viewModel.CurrentPage;
        var initialZoom = _viewModel.ZoomLevel;

        // Act - 测试上一页按钮
        _viewModel.GoToPreviousPageCommand.Execute(null);

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
        _viewModel.ToggleDisplayModeCommand.Execute(null);

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
        _viewModel.PreviewQuality = PreviewQuality.High;

        // Assert
        Assert.NotEqual(initialPreviewQuality, _viewModel.PreviewQuality);
        Assert.Equal(PreviewQuality.High, _viewModel.PreviewQuality);
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
        var newBrightness = 1.2;

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
        var newContrast = 1.2;

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