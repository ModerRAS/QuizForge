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

namespace QuizForge.App.ViewModels;

/// <summary>
/// 打印预览视图模型
/// </summary>
public partial class PrintPreviewViewModel : ObservableObject
{
    private readonly IExportService _exportService;
    private readonly IPrintPreviewService _printPreviewService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private ObservableCollection<ExamPaper> _examPapers = new();

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private double _zoomLevel = 100;

    [ObservableProperty]
    private Bitmap? _currentPreviewImage;

    [ObservableProperty]
    private ObservableCollection<ThumbnailItem> _thumbnailImages = new();

    [ObservableProperty]
    private string _selectedPrinter = string.Empty;

    [ObservableProperty]
    private PrintSettings _printSettings = new();

    [ObservableProperty]
    private int _copies = 1;

    [ObservableProperty]
    private bool _isPrinting;

    [ObservableProperty]
    private double _printProgress;

    [ObservableProperty]
    private ObservableCollection<string> _availablePrinters = new();

    [ObservableProperty]
    private bool _showPageSetupDialog;

    [ObservableProperty]
    private bool _showPrintPreviewDialog;

    [ObservableProperty]
    private int _firstPageToPrint = 1;

    [ObservableProperty]
    private int _lastPageToPrint = 1;

    [ObservableProperty]
    private PrintDuplexMode _duplexMode = PrintDuplexMode.Simplex;

    [ObservableProperty]
    private PrintOrientation _orientation = PrintOrientation.Portrait;

    [ObservableProperty]
    private PrintQuality _quality = PrintQuality.Normal;

    [ObservableProperty]
    private bool _printToPdf = false;

    [ObservableProperty]
    private string _pdfOutputPath = string.Empty;

    [ObservableProperty]
    private bool _showPrintOptions = true;

    [ObservableProperty]
    private bool _showAdvancedOptions = false;

    [ObservableProperty]
    private double _minZoomLevel = 10;

    [ObservableProperty]
    private double _maxZoomLevel = 200;

    [ObservableProperty]
    private double _zoomStep = 10;

    [ObservableProperty]
    private bool _isThumbnailViewVisible = true;

    [ObservableProperty]
    private int _thumbnailSize = 150;

    public PrintPreviewViewModel(
        IExportService exportService,
        IPrintPreviewService printPreviewService,
        IFileService fileService)
    {
        _exportService = exportService;
        _printPreviewService = printPreviewService;
        _fileService = fileService;

        // 初始化数据
        InitializeData();
    }

    private void InitializeData()
    {
        // 初始化打印机列表
        LoadAvailablePrinters();

        // TODO: 从数据库加载试卷列表
        ExamPapers.Add(new ExamPaper { Id = Guid.NewGuid(), Title = "数学期末考试", CreatedAt = DateTime.Now, TotalPoints = 100 });
        ExamPapers.Add(new ExamPaper { Id = Guid.NewGuid(), Title = "英语单元测试", CreatedAt = DateTime.Now, TotalPoints = 50 });
        ExamPapers.Add(new ExamPaper { Id = Guid.NewGuid(), Title = "物理期中考试", CreatedAt = DateTime.Now, TotalPoints = 80 });

        // 初始化打印设置
        PrintSettings = new PrintSettings
        {
            PrinterName = SelectedPrinter,
            Copies = Copies,
            DuplexMode = DuplexMode,
            Orientation = Orientation,
            Quality = Quality,
            PaperSize = PaperSize.A4,
            Margins = new PrintMargins { Left = 10, Top = 10, Right = 10, Bottom = 10 }
        };

        // 设置默认PDF输出路径
        PdfOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuizForge_Prints");
    }

    private async void LoadAvailablePrinters()
    {
        try
        {
            var printers = await _fileService.GetAvailablePrintersAsync();
            foreach (var printer in printers)
            {
                AvailablePrinters.Add(printer);
            }

            if (AvailablePrinters.Count > 0)
            {
                SelectedPrinter = AvailablePrinters[0];
            }
        }
        catch (Exception ex)
        {
            Status = $"加载打印机列表失败: {ex.Message}";
        }
    }

    // OnSelectedExamPaperChanged 方法由 CommunityToolkit.Mvvm.SourceGenerators 自动生成

    [RelayCommand]
    private async Task GeneratePreviewAsync()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请选择一份试卷";
            return;
        }

        try
        {
            Status = "正在生成预览...";

            // 生成LaTeX内容
            var latexContent = await _exportService.ExportToLaTeXAsync(SelectedExamPaper.Content, new ExportConfiguration
            {
                OutputPath = Path.GetTempPath(),
                Format = ExportFormat.LaTeX,
                IncludeAnswerKey = false
            });

            // 生成预览图像
            var previewBytes = await _printPreviewService.GeneratePreviewImageAsync(latexContent, 1, 800, 1130);
            
            // 转换为Bitmap
            using var stream = new MemoryStream(previewBytes);
            CurrentPreviewImage = await Task.Run(() => Bitmap.DecodeToWidth(stream, 800));

            // 生成缩略图
            await GenerateThumbnailsAsync();

            Status = "预览生成完成";
        }
        catch (Exception ex)
        {
            Status = $"生成预览失败: {ex.Message}";
        }
    }

    private async Task GenerateThumbnailsAsync()
    {
        try
        {
            ThumbnailImages.Clear();
            
            // 为每一页生成缩略图
            for (int i = 1; i <= TotalPages; i++)
            {
                var thumbnailBytes = await _printPreviewService.GeneratePreviewImageAsync(
                    SelectedExamPaper!.Content, i, ThumbnailSize, (int)(ThumbnailSize * 1.414));
                
                using var stream = new MemoryStream(thumbnailBytes);
                var thumbnail = await Task.Run(() => Bitmap.DecodeToWidth(stream, ThumbnailSize));
                
                ThumbnailImages.Add(new ThumbnailItem
                {
                    PageNumber = i,
                    Image = thumbnail
                });
            }
        }
        catch (Exception ex)
        {
            Status = $"生成缩略图失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (SelectedExamPaper == null)
        {
            Status = "请选择一份试卷";
            return;
        }

        try
        {
            IsPrinting = true;
            Status = "正在准备打印...";

            // 验证打印设置
            if (!PrintToPdf && string.IsNullOrEmpty(SelectedPrinter))
            {
                Status = "请选择打印机";
                return;
            }

            // 更新打印设置
            PrintSettings.PrinterName = SelectedPrinter;
            PrintSettings.Copies = Copies;
            PrintSettings.DuplexMode = DuplexMode;
            PrintSettings.Orientation = Orientation;
            PrintSettings.Quality = Quality;

            // 设置打印范围
            PrintSettings.FirstPage = FirstPageToPrint;
            PrintSettings.LastPage = LastPageToPrint;

            if (PrintToPdf)
            {
                // 打印到PDF
                await PrintToPdfAsync();
            }
            else
            {
                // 打印到打印机
                await PrintToPrinterAsync();
            }
        }
        catch (Exception ex)
        {
            Status = $"打印失败: {ex.Message}";
        }
        finally
        {
            IsPrinting = false;
        }
    }

    private async Task PrintToPdfAsync()
    {
        try
        {
            Status = "正在生成PDF...";

            // 确保输出目录存在
            if (!Directory.Exists(PdfOutputPath))
            {
                Directory.CreateDirectory(PdfOutputPath);
            }

            // 生成PDF文件名
            var fileName = $"{SelectedExamPaper!.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(PdfOutputPath, fileName);

            // 导出PDF
            var exportConfig = new ExportConfiguration
            {
                OutputPath = PdfOutputPath,
                FileName = Path.GetFileNameWithoutExtension(fileName),
                Format = ExportFormat.PDF,
                IncludeAnswerKey = false,
                Copies = Copies
            };

            var actualFilePath = await _exportService.ExportToPdfAsync(SelectedExamPaper.Content, exportConfig);

            Status = $"PDF已生成: {actualFilePath}";

            // 可选：打开PDF文件
            // System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(actualFilePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Status = $"生成PDF失败: {ex.Message}";
        }
    }

    private async Task PrintToPrinterAsync()
    {
        try
        {
            Status = "正在发送到打印机...";

            // 生成临时PDF文件
            var tempPdfPath = Path.Combine(Path.GetTempPath(), $"QuizForge_Print_{Guid.NewGuid()}.pdf");
            
            var exportConfig = new ExportConfiguration
            {
                OutputPath = Path.GetTempPath(),
                FileName = Path.GetFileNameWithoutExtension(tempPdfPath),
                Format = ExportFormat.PDF,
                IncludeAnswerKey = false,
                Copies = 1
            };

            await _exportService.ExportToPdfAsync(SelectedExamPaper!.Content, exportConfig);

            // 打印文件
            var success = await _fileService.PrintDocumentAsync(tempPdfPath, PrintSettings);

            if (success)
            {
                Status = $"已发送到打印机: {SelectedPrinter}";
            }
            else
            {
                Status = "打印失败";
            }

            // 清理临时文件
            if (File.Exists(tempPdfPath))
            {
                File.Delete(tempPdfPath);
            }
        }
        catch (Exception ex)
        {
            Status = $"打印失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowPageSetup()
    {
        ShowPageSetupDialog = true;
    }

    [RelayCommand]
    private void HidePageSetup()
    {
        ShowPageSetupDialog = false;
    }

    [RelayCommand]
    private void ApplyPageSetup()
    {
        // 应用页面设置
        HidePageSetup();
        Status = "页面设置已应用";
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (ZoomLevel < MaxZoomLevel)
        {
            ZoomLevel = Math.Min(ZoomLevel + ZoomStep, MaxZoomLevel);
        }
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (ZoomLevel > MinZoomLevel)
        {
            ZoomLevel = Math.Max(ZoomLevel - ZoomStep, MinZoomLevel);
        }
    }

    [RelayCommand]
    private void FitToWidth()
    {
        ZoomLevel = 100;
    }

    [RelayCommand]
    private void FirstPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage = 1;
            GeneratePreviewAsync();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            GeneratePreviewAsync();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            GeneratePreviewAsync();
        }
    }

    [RelayCommand]
    private void LastPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage = TotalPages;
            GeneratePreviewAsync();
        }
    }

    [RelayCommand]
    private void TogglePrintOptions()
    {
        ShowPrintOptions = !ShowPrintOptions;
    }

    [RelayCommand]
    private void ToggleAdvancedOptions()
    {
        ShowAdvancedOptions = !ShowAdvancedOptions;
    }

    [RelayCommand]
    private void SelectPdfOutputPath()
    {
        // TODO: 实现文件夹选择对话框
        Status = "请选择PDF输出路径";
    }

    // OnSelectedPrinterChanged、OnCopiesChanged、OnDuplexModeChanged、OnOrientationChanged、OnQualityChanged 方法由 CommunityToolkit.Mvvm.SourceGenerators 自动生成
}

/// <summary>
/// 缩略图项
/// </summary>
public class ThumbnailItem
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 缩略图图像
    /// </summary>
    public Bitmap? Image { get; set; }
}

/// <summary>
/// 打印双面模式枚举
/// </summary>
public enum PrintDuplexMode
{
    /// <summary>
    /// 单面打印
    /// </summary>
    Simplex,
    
    /// <summary>
    /// 水平双面打印
    /// </summary>
    Horizontal,
    
    /// <summary>
    /// 垂直双面打印
    /// </summary>
    Vertical
}

/// <summary>
/// 打印方向枚举
/// </summary>
public enum PrintOrientation
{
    /// <summary>
    /// 纵向
    /// </summary>
    Portrait,
    
    /// <summary>
    /// 横向
    /// </summary>
    Landscape
}

/// <summary>
/// 打印质量枚举
/// </summary>
public enum PrintQuality
{
    /// <summary>
    /// 草稿质量
    /// </summary>
    Draft,
    
    /// <summary>
    /// 普通质量
    /// </summary>
    Normal,
    
    /// <summary>
    /// 高质量
    /// </summary>
    High
}