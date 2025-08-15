using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Services;
using QuizForge.App.ViewModels;
using Xunit;

namespace QuizForge.Tests.ViewModels;

/// <summary>
/// PDF预览视图模型测试类
/// </summary>
public class PdfPreviewViewModelTests : IDisposable
{
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IPrintPreviewService> _mockPrintPreviewService;
    private readonly string _testOutputDirectory;
    private readonly PdfPreviewViewModel _viewModel;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfPreviewViewModelTests()
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
    /// 测试初始化属性
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.NotNull(_viewModel);
        Assert.Null(_viewModel.SelectedExamPaper);
        Assert.Equal(1, _viewModel.CurrentPage);
        Assert.Equal(100, _viewModel.ZoomLevel);
        Assert.Equal(1, _viewModel.TotalPages);
        Assert.Null(_viewModel.CurrentPreviewImage);
        Assert.NotNull(_viewModel.ThumbnailImages);
        Assert.Empty(_viewModel.ThumbnailImages);
        Assert.NotNull(_viewModel.PreviewConfig);
        Assert.True(_viewModel.ShowSealLine);
        Assert.Equal(90, _viewModel.PreviewQuality);
        Assert.Equal(0, _viewModel.Brightness);
        Assert.Equal(0, _viewModel.Contrast);
        Assert.Equal(PreviewDisplayMode.SinglePage, _viewModel.DisplayMode);
        Assert.NotNull(_viewModel.FirstPageCommand);
        Assert.NotNull(_viewModel.PreviousPageCommand);
        Assert.NotNull(_viewModel.NextPageCommand);
        Assert.NotNull(_viewModel.LastPageCommand);
        Assert.NotNull(_viewModel.ZoomInCommand);
        Assert.NotNull(_viewModel.ZoomOutCommand);
        Assert.NotNull(_viewModel.FitToWidthCommand);
        Assert.NotNull(_viewModel.ToggleSealLineCommand);
        Assert.NotNull(_viewModel.ToggleHighQualityCommand);
        Assert.NotNull(_viewModel.ToggleThumbnailViewCommand);
        Assert.NotNull(_viewModel.ToggleMouseWheelZoomCommand);
        Assert.NotNull(_viewModel.ToggleDragNavigationCommand);
        Assert.NotNull(_viewModel.ToggleContinuousScrollCommand);
        Assert.NotNull(_viewModel.ToggleDualPageViewCommand);
        Assert.NotNull(_viewModel.ResetImageAdjustmentsCommand);
        Assert.NotNull(_viewModel.IncreaseBrightnessCommand);
        Assert.NotNull(_viewModel.DecreaseBrightnessCommand);
        Assert.NotNull(_viewModel.IncreaseContrastCommand);
        Assert.NotNull(_viewModel.DecreaseContrastCommand);
        Assert.NotNull(_viewModel.SetDisplayModeCommand);
        Assert.NotNull(_viewModel.SavePreviewConfigCommand);
        Assert.NotNull(_viewModel.LoadPreviewConfigCommand);
        Assert.NotNull(_viewModel.ExportToPdfCommand);
        Assert.NotNull(_viewModel.PrintCommand);
        Assert.NotNull(_viewModel.RefreshCommand);
        Assert.NotNull(_viewModel.ShowPropertiesCommand);
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
            Content = "Test Content"
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
        Assert.Equal(pageCount, _viewModel.TotalPages);
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
            Content = "Test Content"
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
        var newZoomLevel = 150.0;

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
        var newDisplayMode = PreviewDisplayMode.DualPage;

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
        var newPreviewQuality = 90;

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
        var newBrightness = 20;

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
        var newContrast = 20;

        // Act
        _viewModel.Contrast = newContrast;

        // Assert
        Assert.Equal(newContrast, _viewModel.Contrast);
    }

    /// <summary>
    /// 测试转到首页命令
    /// </summary>
    [Fact]
    public void FirstPageCommand_ShouldSetCurrentPageToOne()
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

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.FirstPageCommand.Execute(null);

        // Assert
        Assert.Equal(1, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到上一页命令
    /// </summary>
    [Fact]
    public void PreviousPageCommand_ShouldDecrementCurrentPage()
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

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.PreviousPageCommand.Execute(null);

        // Assert
        Assert.Equal(2, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到下一页命令
    /// </summary>
    [Fact]
    public void NextPageCommand_ShouldIncrementCurrentPage()
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

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.NextPageCommand.Execute(null);

        // Assert
        Assert.Equal(4, _viewModel.CurrentPage);
    }

    /// <summary>
    /// 测试转到末页命令
    /// </summary>
    [Fact]
    public void LastPageCommand_ShouldSetCurrentPageToPageCount()
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

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;
        _viewModel.CurrentPage = 3;

        // Act
        _viewModel.LastPageCommand.Execute(null);

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
        var initialZoomLevel = 100.0;

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
        var initialZoomLevel = 100.0;

        // Act
        _viewModel.ZoomOutCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.ZoomLevel < initialZoomLevel);
    }

    /// <summary>
    /// 测试适应窗口命令
    /// </summary>
    [Fact]
    public void FitToWidthCommand_ShouldToggleFitToWidth()
    {
        // Arrange
        var initialFitToWidth = _viewModel.IsFitToWidth;

        // Act
        _viewModel.FitToWidthCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialFitToWidth, _viewModel.IsFitToWidth);
    }

    /// <summary>
    /// 测试高质量模式切换命令
    /// </summary>
    [Fact]
    public void ToggleHighQualityCommand_ShouldToggleHighQualityMode()
    {
        // Arrange
        var initialHighQuality = _viewModel.IsHighQualityMode;

        // Act
        _viewModel.ToggleHighQualityCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialHighQuality, _viewModel.IsHighQualityMode);
    }

    /// <summary>
    /// 测试刷新命令
    /// </summary>
    [Fact]
    public void RefreshCommand_ShouldRefreshPreview()
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

        // 设置初始状态
        _viewModel.SelectedExamPaper = examPaper;

        // Act
        _viewModel.RefreshCommand.Execute(null);

        // Assert
        // 验证状态更新
        Assert.Contains("正在刷新", _viewModel.Status);
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
    /// 测试缩略图切换命令
    /// </summary>
    [Fact]
    public void ToggleThumbnailViewCommand_ShouldToggleThumbnailView()
    {
        // Arrange
        var initialThumbnailView = _viewModel.IsThumbnailViewVisible;

        // Act
        _viewModel.ToggleThumbnailViewCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialThumbnailView, _viewModel.IsThumbnailViewVisible);
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