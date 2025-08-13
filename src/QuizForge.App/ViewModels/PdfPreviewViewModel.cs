using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;

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

    public PdfPreviewViewModel(
        IExportService exportService,
        IPrintPreviewService printPreviewService)
    {
        _exportService = exportService;
        _printPreviewService = printPreviewService;

        // 初始化数据
        InitializeData();
    }

    private void InitializeData()
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

    partial void OnSelectedExamPaperChanged(ExamPaper? value)
    {
        if (value != null)
        {
            // TODO: 加载试卷预览
            CurrentPage = 1;
            TotalPages = 5; // 假设每份试卷有5页
            Status = $"正在预览试卷: {value.Title}";
        }
        else
        {
            Status = "未选择试卷";
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
        if (ZoomLevel < 200)
        {
            ZoomLevel += 10;
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
        if (ZoomLevel > 50)
        {
            ZoomLevel -= 10;
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
}