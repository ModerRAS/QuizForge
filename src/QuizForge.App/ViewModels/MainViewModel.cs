using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System.Timers;
using System;
using System.Threading.Tasks;

namespace QuizForge.App.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IGenerationService _generationService;
    private readonly IExportService _exportService;
    private readonly System.Timers.Timer _timer;

    [ObservableProperty]
    private string _title = "QuizForge - 试卷生成系统";

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private bool _isQuestionBankViewVisible;

    [ObservableProperty]
    private bool _isTemplateViewVisible;

    [ObservableProperty]
    private bool _isExamGenerationViewVisible;

    [ObservableProperty]
    private bool _isPdfPreviewViewVisible;

    [ObservableProperty]
    private ObservableCollection<RecentFile> _recentFiles = new();

    [ObservableProperty]
    private DateTime _currentTime = DateTime.Now;

    public MainViewModel(
        IServiceProvider serviceProvider,
        IQuestionService questionService,
        ITemplateService templateService,
        IGenerationService generationService,
        IExportService exportService)
    {
        _serviceProvider = serviceProvider;
        _questionService = questionService;
        _templateService = templateService;
        _generationService = generationService;
        _exportService = exportService;

        // 初始化定时器，用于更新时间显示
        _timer = new System.Timers.Timer(60000); // 每分钟更新一次
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();

        // 初始化最近文件列表
        InitializeRecentFiles();

        // 默认显示题库管理视图
        ShowQuestionBankView();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        CurrentTime = DateTime.Now;
    }

    private void InitializeRecentFiles()
    {
        // TODO: 从配置文件加载最近使用的文件
        RecentFiles.Add(new RecentFile { Name = "数学题库.md", Path = "C:\\QuizForge\\数学题库.md" });
        RecentFiles.Add(new RecentFile { Name = "英语试卷模板.tex", Path = "C:\\QuizForge\\英语试卷模板.tex" });
    }

    [RelayCommand]
    private void NewQuestionBank()
    {
        Status = "正在创建新题库...";
        // TODO: 实现新建题库的逻辑
        Task.Delay(500).Wait();
        Status = "新题库已创建";
        ShowQuestionBankView();
    }

    [RelayCommand]
    private async Task ImportQuestionBank()
    {
        Status = "正在导入题库...";
        // TODO: 实现导入题库的逻辑
        await Task.Delay(1000);
        Status = "题库导入完成";
        ShowQuestionBankView();
    }

    [RelayCommand]
    private void Save()
    {
        Status = "正在保存...";
        // TODO: 实现保存的逻辑
        Task.Delay(500).Wait();
        Status = "保存完成";
    }

    [RelayCommand]
    private void SaveAs()
    {
        Status = "正在另存为...";
        // TODO: 实现另存为的逻辑
        Task.Delay(500).Wait();
        Status = "另存为完成";
    }

    [RelayCommand]
    private void Exit()
    {
        // TODO: 实现退出前的保存确认
        Status = "正在退出...";
        Task.Delay(500).Wait();
        // 在实际应用中，这里应该关闭应用程序
    }

    [RelayCommand]
    private void Undo()
    {
        Status = "正在撤销...";
        // TODO: 实现撤销的逻辑
        Task.Delay(300).Wait();
        Status = "撤销完成";
    }

    [RelayCommand]
    private void Redo()
    {
        Status = "正在重做...";
        // TODO: 实现重做的逻辑
        Task.Delay(300).Wait();
        Status = "重做完成";
    }

    [RelayCommand]
    private void Cut()
    {
        Status = "正在剪切...";
        // TODO: 实现剪切的逻辑
        Task.Delay(300).Wait();
        Status = "剪切完成";
    }

    [RelayCommand]
    private void Copy()
    {
        Status = "正在复制...";
        // TODO: 实现复制的逻辑
        Task.Delay(300).Wait();
        Status = "复制完成";
    }

    [RelayCommand]
    private void Paste()
    {
        Status = "正在粘贴...";
        // TODO: 实现粘贴的逻辑
        Task.Delay(300).Wait();
        Status = "粘贴完成";
    }

    [RelayCommand]
    private void Delete()
    {
        Status = "正在删除...";
        // TODO: 实现删除的逻辑
        Task.Delay(300).Wait();
        Status = "删除完成";
    }

    [RelayCommand]
    private void ShowQuestionBankView()
    {
        IsQuestionBankViewVisible = true;
        IsTemplateViewVisible = false;
        IsExamGenerationViewVisible = false;
        IsPdfPreviewViewVisible = false;
        
        if (CurrentView is not QuestionBankViewModel)
        {
            CurrentView = _serviceProvider.GetRequiredService<QuestionBankViewModel>();
        }
        
        Status = "题库管理";
    }

    [RelayCommand]
    private void ShowTemplateView()
    {
        IsQuestionBankViewVisible = false;
        IsTemplateViewVisible = true;
        IsExamGenerationViewVisible = false;
        IsPdfPreviewViewVisible = false;
        
        if (CurrentView is not TemplateViewModel)
        {
            CurrentView = _serviceProvider.GetRequiredService<TemplateViewModel>();
        }
        
        Status = "模板管理";
    }

    [RelayCommand]
    private void ShowExamGenerationView()
    {
        IsQuestionBankViewVisible = false;
        IsTemplateViewVisible = false;
        IsExamGenerationViewVisible = true;
        IsPdfPreviewViewVisible = false;
        
        if (CurrentView is not ExamGenerationViewModel)
        {
            CurrentView = _serviceProvider.GetRequiredService<ExamGenerationViewModel>();
        }
        
        Status = "试卷生成";
    }

    [RelayCommand]
    private void ShowPdfPreviewView()
    {
        IsQuestionBankViewVisible = false;
        IsTemplateViewVisible = false;
        IsExamGenerationViewVisible = false;
        IsPdfPreviewViewVisible = true;
        
        if (CurrentView is not PdfPreviewViewModel)
        {
            CurrentView = _serviceProvider.GetRequiredService<PdfPreviewViewModel>();
        }
        
        Status = "PDF预览";
    }

    [RelayCommand]
    private async Task GeneratePaper()
    {
        Status = "正在生成试卷...";
        // TODO: 实现生成试卷的逻辑
        await Task.Delay(1000);
        Status = "试卷生成完成";
        ShowPdfPreviewView();
    }

    [RelayCommand]
    private async Task ExportPaper()
    {
        Status = "正在导出试卷...";
        // TODO: 实现导出试卷的逻辑
        await Task.Delay(1000);
        Status = "试卷导出完成";
    }

    [RelayCommand]
    private void ShowSettings()
    {
        Status = "正在打开设置...";
        // TODO: 实现显示设置的逻辑
        Task.Delay(500).Wait();
        Status = "设置已打开";
    }

    [RelayCommand]
    private void ShowHelp()
    {
        Status = "正在打开帮助...";
        // TODO: 实现显示帮助的逻辑
        Task.Delay(500).Wait();
        Status = "帮助已打开";
    }

    [RelayCommand]
    private void ShowAbout()
    {
        Status = "正在打开关于...";
        // TODO: 实现显示关于的逻辑
        Task.Delay(500).Wait();
        Status = "关于已打开";
    }
}
}

/// <summary>
/// 最近使用的文件
/// </summary>
public partial class RecentFile : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    [RelayCommand]
    private void Open()
    {
        // TODO: 实现打开文件的逻辑
        Task.Delay(500).Wait();
    }
}