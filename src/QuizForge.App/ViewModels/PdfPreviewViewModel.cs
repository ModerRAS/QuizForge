using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia;

namespace QuizForge.App.ViewModels
{
    /// <summary>
    /// PDF预览视图模型
    /// </summary>
    public partial class PdfPreviewViewModel : ObservableObject
{
    private readonly IExportService _exportService;
    private readonly IPrintPreviewService _printPreviewService;

    [ObservableProperty]
    private ObservableCollection<ExamPaper> _examPapers = new();

    [ObservableProperty]
    private ExamPaper? _selectedExamPaper;

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private double _zoomLevel = 100;

    [ObservableProperty]
    private bool _isFitToWidth = true;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private double _exportProgress;

    [ObservableProperty]
    private Bitmap? _currentPreviewImage;

    [ObservableProperty]
    private ObservableCollection<ThumbnailItem> _thumbnailImages = new();

    [ObservableProperty]
    private PreviewConfig _previewConfig = new();

    [ObservableProperty]
    private bool _showSealLine = true;

    [ObservableProperty]
    private int _previewQuality = 90;

    [ObservableProperty]
    private int _brightness = 0;

    [ObservableProperty]
    private int _contrast = 0;

    [ObservableProperty]
    private PreviewDisplayMode _displayMode = PreviewDisplayMode.SinglePage;

    [ObservableProperty]
    private bool _isHighQualityMode = false;

    [ObservableProperty]
    private bool _isMouseWheelZoomEnabled = true;

    [ObservableProperty]
    private bool _isDragNavigationEnabled = true;

    [ObservableProperty]
    private double _minZoomLevel = 10;

    [ObservableProperty]
    private double _maxZoomLevel = 500;

    [ObservableProperty]
    private double _zoomStep = 10;

    [ObservableProperty]
    private bool _isContinuousScrollEnabled = true;

    [ObservableProperty]
    private bool _isDualPageViewEnabled = false;

    [ObservableProperty]
    private int _thumbnailSize = 200;

    [ObservableProperty]
    private bool _isThumbnailViewVisible = false;

    [ObservableProperty]
    private Rect _cropArea = new Rect(0, 0, 800, 1130);

    /// <summary>
    /// 获取显示模式列表
    /// </summary>
    public Array DisplayModes => Enum.GetValues(typeof(PreviewDisplayMode));

    public PdfPreviewViewModel(
        IExportService exportService,
        IPrintPreviewService printPreviewService)
    {
        _exportService = exportService;
        _printPreviewService = printPreviewService;

        // 初始化数据
        InitializeData();
        InitializePreview();
    }

    /// <summary>
    /// 缩略图项
    /// </summary>
    public class ThumbnailItem
    {
        /// <summary>
        /// 获取或设置缩略图图像
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// 获取或设置页码
        /// </summary>
        public int PageNumber { get; set; }
    }

    private async void InitializeData()
    {
        // TODO: 从数据库加载试卷列表
        ExamPapers.Add(new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "数学期末考试",
            TemplateId = Guid.NewGuid(),
            QuestionBankId = Guid.NewGuid(),
            Content = "这是数学期末考试的试卷内容...",
            CreatedAt = DateTime.Now.AddDays(-5),
            TotalPoints = 100
        });
        ExamPapers.Add(new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "英语单元测试",
            TemplateId = Guid.NewGuid(),
            QuestionBankId = Guid.NewGuid(),
            Content = "这是英语单元测试的试卷内容...",
            CreatedAt = DateTime.Now.AddDays(-3),
            TotalPoints = 50
        });
        ExamPapers.Add(new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "物理竞赛试卷",
            TemplateId = Guid.NewGuid(),
            QuestionBankId = Guid.NewGuid(),
            Content = "这是物理竞赛的试卷内容...",
            CreatedAt = DateTime.Now.AddDays(-1),
            TotalPoints = 150
        });
        
        if (ExamPapers.Count > 0)
        {
            SelectedExamPaper = ExamPapers[0];
        }
    }

    private async void InitializePreview()
    {
        // 加载预览配置
        PreviewConfig = await _printPreviewService.GetPreviewConfigAsync();
        
        // 应用配置
        ShowSealLine = PreviewConfig.ShowSealLine;
        PreviewQuality = PreviewConfig.DefaultQuality;
        IsMouseWheelZoomEnabled = PreviewConfig.EnableMouseWheelZoom;
        IsDragNavigationEnabled = PreviewConfig.EnableDragNavigation;
        MinZoomLevel = PreviewConfig.MinZoomLevel;
        MaxZoomLevel = PreviewConfig.MaxZoomLevel;
        ZoomStep = PreviewConfig.ZoomStep;
        IsContinuousScrollEnabled = PreviewConfig.EnableContinuousScroll;
        IsDualPageViewEnabled = PreviewConfig.EnableDualPageView;
        ThumbnailSize = PreviewConfig.DefaultThumbnailSize;
        DisplayMode = PreviewConfig.DisplayMode;
        Brightness = PreviewConfig.DefaultBrightness;
        Contrast = PreviewConfig.DefaultContrast;
    }

    partial void OnSelectedExamPaperChanged(ExamPaper? value)
    {
        if (value != null)
        {
            // 加载试卷预览
            CurrentPage = 1;
            TotalPages = 5; // 假设每份试卷有5页
            Status = $"正在预览试卷: {value.Title}";
            _ = LoadPreviewAsync();
        }
        else
        {
            Status = "未选择试卷";
            CurrentPreviewImage = null;
            ThumbnailImages.Clear();
        }
    }

    partial void OnCurrentPageChanged(int value)
    {
        if (SelectedExamPaper != null && value > 0 && value <= TotalPages)
        {
            _ = LoadPreviewAsync();
            Status = $"当前第 {CurrentPage} 页，共 {TotalPages} 页";
        }
    }

    partial void OnZoomLevelChanged(double value)
    {
        if (SelectedExamPaper != null)
        {
            Status = $"缩放级别: {ZoomLevel}%";
            _ = LoadPreviewAsync();
        }
    }

    partial void OnShowSealLineChanged(bool value)
    {
        if (SelectedExamPaper != null)
        {
            Status = value ? "已启用密封线显示" : "已禁用密封线显示";
            _ = LoadPreviewAsync();
        }
    }

    partial void OnPreviewQualityChanged(int value)
    {
        if (SelectedExamPaper != null)
        {
            Status = $"预览质量: {PreviewQuality}%";
            _ = LoadPreviewAsync();
        }
    }

    partial void OnBrightnessChanged(int value)
    {
        if (SelectedExamPaper != null && CurrentPreviewImage != null)
        {
            Status = $"亮度: {Brightness}";
            _ = AdjustImageBrightnessAsync();
        }
    }

    partial void OnContrastChanged(int value)
    {
        if (SelectedExamPaper != null && CurrentPreviewImage != null)
        {
            Status = $"对比度: {Contrast}";
            _ = AdjustImageContrastAsync();
        }
    }

    partial void OnDisplayModeChanged(PreviewDisplayMode value)
    {
        Status = $"显示模式: {GetDisplayModeName(value)}";
        UpdateDisplayMode();
    }

    partial void OnIsThumbnailViewVisibleChanged(bool value)
    {
        if (value && SelectedExamPaper != null)
        {
            _ = LoadThumbnailsAsync();
        }
    }

    [RelayCommand]
    private void FirstPage()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        CurrentPage = 1;
        Status = $"已跳转到第 {CurrentPage} 页";
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        if (CurrentPage > 1)
        {
            CurrentPage--;
            Status = $"当前第 {CurrentPage} 页，共 {TotalPages} 页";
        }
        else
        {
            Status = "已经是第一页";
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            Status = $"当前第 {CurrentPage} 页，共 {TotalPages} 页";
        }
        else
        {
            Status = "已经是最后一页";
        }
    }

    [RelayCommand]
    private void LastPage()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        CurrentPage = TotalPages;
        Status = $"已跳转到第 {CurrentPage} 页";
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (ZoomLevel < MaxZoomLevel)
        {
            ZoomLevel += ZoomStep;
            Status = $"缩放级别: {ZoomLevel}%";
        }
        else
        {
            Status = "已达到最大缩放级别";
        }
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (ZoomLevel > MinZoomLevel)
        {
            ZoomLevel -= ZoomStep;
            Status = $"缩放级别: {ZoomLevel}%";
        }
        else
        {
            Status = "已达到最小缩放级别";
        }
    }

    [RelayCommand]
    private void FitToWidth()
    {
        IsFitToWidth = !IsFitToWidth;
        Status = IsFitToWidth ? "已启用适应宽度" : "已禁用适应宽度";
    }

    [RelayCommand]
    private void ToggleSealLine()
    {
        ShowSealLine = !ShowSealLine;
    }

    [RelayCommand]
    private void ToggleHighQuality()
    {
        IsHighQualityMode = !IsHighQualityMode;
        Status = IsHighQualityMode ? "已启用高质量模式" : "已禁用高质量模式";
        _ = LoadPreviewAsync();
    }

    [RelayCommand]
    private void ToggleThumbnailView()
    {
        IsThumbnailViewVisible = !IsThumbnailViewVisible;
        Status = IsThumbnailViewVisible ? "已显示缩略图" : "已隐藏缩略图";
    }

    [RelayCommand]
    private void ToggleMouseWheelZoom()
    {
        IsMouseWheelZoomEnabled = !IsMouseWheelZoomEnabled;
        Status = IsMouseWheelZoomEnabled ? "已启用鼠标滚轮缩放" : "已禁用鼠标滚轮缩放";
    }

    [RelayCommand]
    private void ToggleDragNavigation()
    {
        IsDragNavigationEnabled = !IsDragNavigationEnabled;
        Status = IsDragNavigationEnabled ? "已启用拖动浏览" : "已禁用拖动浏览";
    }

    [RelayCommand]
    private void ToggleContinuousScroll()
    {
        IsContinuousScrollEnabled = !IsContinuousScrollEnabled;
        Status = IsContinuousScrollEnabled ? "已启用连续滚动" : "已禁用连续滚动";
    }

    [RelayCommand]
    private void ToggleDualPageView()
    {
        IsDualPageViewEnabled = !IsDualPageViewEnabled;
        Status = IsDualPageViewEnabled ? "已启用双页显示" : "已禁用双页显示";
        UpdateDisplayMode();
    }

    [RelayCommand]
    private void ResetImageAdjustments()
    {
        Brightness = 0;
        Contrast = 0;
        Status = "已重置图像调整";
        _ = LoadPreviewAsync();
    }

    [RelayCommand]
    private void IncreaseBrightness()
    {
        if (Brightness < 100)
        {
            Brightness += 5;
        }
    }

    [RelayCommand]
    private void DecreaseBrightness()
    {
        if (Brightness > -100)
        {
            Brightness -= 5;
        }
    }

    [RelayCommand]
    private void IncreaseContrast()
    {
        if (Contrast < 100)
        {
            Contrast += 5;
        }
    }

    [RelayCommand]
    private void DecreaseContrast()
    {
        if (Contrast > -100)
        {
            Contrast -= 5;
        }
    }

    [RelayCommand]
    private void SetDisplayMode(PreviewDisplayMode mode)
    {
        DisplayMode = mode;
    }

    [RelayCommand]
    private async Task SavePreviewConfigAsync()
    {
        try
        {
            // 更新配置
            PreviewConfig.ShowSealLine = ShowSealLine;
            PreviewConfig.DefaultQuality = PreviewQuality;
            PreviewConfig.EnableMouseWheelZoom = IsMouseWheelZoomEnabled;
            PreviewConfig.EnableDragNavigation = IsDragNavigationEnabled;
            PreviewConfig.MinZoomLevel = MinZoomLevel;
            PreviewConfig.MaxZoomLevel = MaxZoomLevel;
            PreviewConfig.ZoomStep = ZoomStep;
            PreviewConfig.EnableContinuousScroll = IsContinuousScrollEnabled;
            PreviewConfig.EnableDualPageView = IsDualPageViewEnabled;
            PreviewConfig.DefaultThumbnailSize = ThumbnailSize;
            PreviewConfig.DisplayMode = DisplayMode;
            PreviewConfig.DefaultBrightness = Brightness;
            PreviewConfig.DefaultContrast = Contrast;

            // 保存配置
            var result = await _printPreviewService.SetPreviewConfigAsync(PreviewConfig);
            Status = result ? "预览配置已保存" : "保存预览配置失败";
        }
        catch (Exception ex)
        {
            Status = $"保存预览配置失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadPreviewConfigAsync()
    {
        try
        {
            PreviewConfig = await _printPreviewService.GetPreviewConfigAsync();
            Status = "预览配置已加载";
            InitializePreview();
        }
        catch (Exception ex)
        {
            Status = $"加载预览配置失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        try
        {
            IsExporting = true;
            ExportProgress = 0;
            Status = "正在导出PDF...";

            // 模拟导出过程
            await Task.Delay(1000);
            ExportProgress = 50;

            // TODO: 实际的导出逻辑
            // await _exportService.ExportToPdfAsync(SelectedExamPaper);

            await Task.Delay(1000);
            ExportProgress = 100;

            Status = $"PDF已导出: {SelectedExamPaper.Title}.pdf";
        }
        catch (Exception ex)
        {
            Status = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private void Print()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        // TODO: 实现打印逻辑
        Status = $"正在打印试卷: {SelectedExamPaper.Title}";
    }

    [RelayCommand]
    private void Refresh()
    {
        if (SelectedExamPaper != null)
        {
            // TODO: 刷新预览
            Status = $"正在刷新预览: {SelectedExamPaper.Title}";
        }
        else
        {
            Status = "请先选择一份试卷";
        }
    }

    [RelayCommand]
    private void ShowProperties()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请先选择一份试卷";
            return;
        }

        // TODO: 显示试卷属性对话框
        Status = $"显示试卷属性: {SelectedExamPaper.Title}";
    }

    /// <summary>
    /// 加载预览图像
    /// </summary>
    private async Task LoadPreviewAsync()
    {
        if (SelectedExamPaper == null || CurrentPage <= 0)
        {
            CurrentPreviewImage = null;
            return;
        }

        try
        {
            Status = "正在加载预览...";
            
            if (IsHighQualityMode)
            {
                CurrentPreviewImage = await _printPreviewService.GenerateHighQualityPreviewAsync(
                    SelectedExamPaper,
                    CurrentPage,
                    PreviewQuality);
            }
            else if (ShowSealLine)
            {
                CurrentPreviewImage = await _printPreviewService.GeneratePreviewWithSealLineAsync(
                    SelectedExamPaper,
                    CurrentPage,
                    PreviewQuality);
            }
            else
            {
                CurrentPreviewImage = await _printPreviewService.GeneratePreviewRangeAsync(
                    SelectedExamPaper,
                    CurrentPage,
                    1,
                    PreviewQuality);
            }
            
            Status = $"预览加载完成 - 第 {CurrentPage} 页";
        }
        catch (Exception ex)
        {
            Status = $"加载预览失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 调整图像亮度
    /// </summary>
    private async Task AdjustImageBrightnessAsync()
    {
        if (CurrentPreviewImage == null)
        {
            return;
        }

        try
        {
            CurrentPreviewImage = await _printPreviewService.AdjustBrightnessContrastAsync(
                CurrentPreviewImage,
                Brightness,
                Contrast);
        }
        catch (Exception ex)
        {
            Status = $"调整亮度失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 调整图像对比度
    /// </summary>
    private async Task AdjustImageContrastAsync()
    {
        if (CurrentPreviewImage == null)
        {
            return;
        }

        try
        {
            CurrentPreviewImage = await _printPreviewService.AdjustBrightnessContrastAsync(
                CurrentPreviewImage,
                Brightness,
                Contrast);
        }
        catch (Exception ex)
        {
            Status = $"调整对比度失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取显示模式名称
    /// </summary>
    private string GetDisplayModeName(PreviewDisplayMode mode)
    {
        return mode switch
        {
            PreviewDisplayMode.SinglePage => "单页模式",
            PreviewDisplayMode.DualPage => "双页模式",
            PreviewDisplayMode.Thumbnail => "缩略图模式",
            PreviewDisplayMode.FullScreen => "全屏模式",
            PreviewDisplayMode.Presentation => "演示模式",
            _ => "未知模式"
        };
    }

    /// <summary>
    /// 更新显示模式
    /// </summary>
    private void UpdateDisplayMode()
    {
        if (IsDualPageViewEnabled)
        {
            DisplayMode = PreviewDisplayMode.DualPage;
        }
        else
        {
            DisplayMode = PreviewDisplayMode.SinglePage;
        }
    }

    /// <summary>
    /// 加载缩略图
    /// </summary>
    private async Task LoadThumbnailsAsync()
    {
        if (SelectedExamPaper == null || SelectedExamPaper.Pages == null || SelectedExamPaper.Pages.Count == 0)
        {
            ThumbnailImages.Clear();
            return;
        }

        try
        {
            Status = "正在加载缩略图...";

            var thumbnails = await _printPreviewService.GenerateThumbnailPreviewAsync(
                SelectedExamPaper,
                ThumbnailSize,
                PreviewQuality);

            ThumbnailImages.Clear();
            for (int i = 0; i < thumbnails.Count; i++)
            {
                ThumbnailImages.Add(new ThumbnailItem
                {
                    Image = thumbnails[i],
                    PageNumber = i + 1
                });
            }

            Status = "缩略图加载完成";
        }
        catch (Exception ex)
        {
            Status = $"加载缩略图失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 处理鼠标滚轮事件
    /// </summary>
    public void HandleMouseWheel(int delta)
    {
        if (!IsMouseWheelZoomEnabled || SelectedExamPaper == null)
        {
            return;
        }

        if (delta > 0)
        {
            ZoomInCommand.Execute(null);
        }
        else
        {
            ZoomOutCommand.Execute(null);
        }
    }

    /// <summary>
    /// 处理鼠标拖动事件
    /// </summary>
    public void HandleMouseDrag(double deltaX, double deltaY)
    {
        if (!IsDragNavigationEnabled || SelectedExamPaper == null)
        {
            return;
        }

        // 这里可以添加拖动浏览的逻辑
        // 例如：更新滚动位置或视图偏移
    }

    /// <summary>
    /// 处理缩略图点击事件
    /// </summary>
    public void HandleThumbnailClick(int pageIndex)
    {
        if (SelectedExamPaper == null || pageIndex < 0)
        {
            return;
        }

        // 确保页码在有效范围内
        if (pageIndex >= TotalPages)
        {
            CurrentPage = TotalPages;
        }
        else
        {
            CurrentPage = pageIndex + 1;
        }
    }
}
}