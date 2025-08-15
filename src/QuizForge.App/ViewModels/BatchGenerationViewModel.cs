using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizForge.App.ViewModels;

/// <summary>
/// 批量生成试卷视图模型
/// </summary>
public partial class BatchGenerationViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IGenerationService _generationService;
    private readonly IBatchGenerationService _batchGenerationService;

    [ObservableProperty]
    private ObservableCollection<QuestionBank> _questionBanks = new();

    [ObservableProperty]
    private ObservableCollection<ExamTemplate> _templates = new();

    [ObservableProperty]
    private ObservableCollection<Guid> _selectedQuestionBanks = new();

    [ObservableProperty]
    private ExamTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    [ObservableProperty]
    private string _fileNamePrefix = "试卷";

    [ObservableProperty]
    private int _generationCount = 10;

    [ObservableProperty]
    private int _parallelCount = 2;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private double _generationProgress;

    [ObservableProperty]
    private BatchGenerationProgress? _currentProgress;

    [ObservableProperty]
    private ObservableCollection<BatchGenerationHistory> _generationHistory = new();

    [ObservableProperty]
    private Guid? _currentBatchId;

    public BatchGenerationViewModel(
        IQuestionService questionService,
        ITemplateService templateService,
        IGenerationService generationService,
        IBatchGenerationService batchGenerationService)
    {
        _questionService = questionService;
        _templateService = templateService;
        _generationService = generationService;
        _batchGenerationService = batchGenerationService;

        // 初始化数据
        InitializeData();
    }

    private void InitializeData()
    {
        // TODO: 从数据库加载题库列表
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "数学题库", Description = "高中数学题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "英语题库", Description = "高中英语题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "物理题库", Description = "高中物理题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });

        // TODO: 从数据库加载模板列表
        Templates.Add(new ExamTemplate { Id = Guid.NewGuid(), Name = "标准试卷模板", Description = "适用于常规考试的标准试卷模板", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        Templates.Add(new ExamTemplate { Id = Guid.NewGuid(), Name = "竞赛试卷模板", Description = "适用于学科竞赛的试卷模板", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        Templates.Add(new ExamTemplate { Id = Guid.NewGuid(), Name = "单元测试模板", Description = "适用于单元测试的简化试卷模板", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });

        if (Templates.Count > 0)
        {
            SelectedTemplate = Templates[0];
        }

        // 加载生成历史
        LoadGenerationHistory();
    }

    private async void LoadGenerationHistory()
    {
        try
        {
            var history = await _batchGenerationService.GetBatchGenerationHistoryAsync(10, 1);
            foreach (var item in history.Items)
            {
                GenerationHistory.Add(item);
            }
        }
        catch (Exception ex)
        {
            Status = $"加载历史记录失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddQuestionBank()
    {
        if (SelectedQuestionBanks.Count >= QuestionBanks.Count)
        {
            Status = "已选择所有可用题库";
            return;
        }

        // 添加未选择的题库
        var availableBanks = QuestionBanks.Where(qb => !SelectedQuestionBanks.Contains(qb.Id)).ToList();
        if (availableBanks.Count > 0)
        {
            SelectedQuestionBanks.Add(availableBanks[0].Id);
            Status = $"已选择 {SelectedQuestionBanks.Count} 个题库";
        }
    }

    [RelayCommand]
    private void RemoveQuestionBank(Guid questionBankId)
    {
        if (SelectedQuestionBanks.Contains(questionBankId))
        {
            SelectedQuestionBanks.Remove(questionBankId);
            Status = $"已选择 {SelectedQuestionBanks.Count} 个题库";
        }
    }

    [RelayCommand]
    private void SelectOutputDirectory()
    {
        // TODO: 实现文件夹选择对话框
        Status = "请选择输出目录";
    }

    [RelayCommand]
    private async Task StartBatchGenerationAsync()
    {
        if (SelectedQuestionBanks.Count == 0)
        {
            Status = "请选择至少一个题库";
            return;
        }

        if (SelectedTemplate == null)
        {
            Status = "请选择一个模板";
            return;
        }

        if (GenerationCount <= 0)
        {
            Status = "生成数量必须大于0";
            return;
        }

        try
        {
            IsGenerating = true;
            GenerationProgress = 0;
            Status = "正在启动批量生成...";

            // 创建批量生成请求
            var request = new BatchGenerationRequest
            {
                QuestionBankIds = SelectedQuestionBanks.ToList(),
                TemplateId = SelectedTemplate.Id,
                Count = GenerationCount,
                OutputDirectory = OutputDirectory,
                FileNamePrefix = FileNamePrefix,
                Format = ExportFormat.PDF,
                ParallelCount = ParallelCount,
                Options = new BatchGenerationOptions
                {
                    MaxQuestionCount = 20,
                    MinQuestionCount = 15,
                    RandomizeQuestions = true,
                    RandomizeOptions = true,
                    QuestionTypeFilter = new List<string> { "选择题", "填空题", "简答题" },
                    DifficultyRange = new DifficultyRange { Min = 1, Max = 3 }
                }
            };

            // 启动批量生成
            var result = await _batchGenerationService.BatchGenerateExamPapersAsync(request);
            CurrentBatchId = result.BatchId;

            Status = $"批量生成任务已启动: {result.BatchId}";
            
            // 开始监控进度
            await MonitorProgressAsync(result.BatchId);
        }
        catch (Exception ex)
        {
            Status = $"启动批量生成失败: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task CancelBatchGenerationAsync()
    {
        if (!CurrentBatchId.HasValue)
        {
            Status = "没有正在进行的批量生成任务";
            return;
        }

        try
        {
            var success = await _batchGenerationService.CancelBatchGenerationAsync(CurrentBatchId.Value);
            if (success)
            {
                Status = $"批量生成任务已取消: {CurrentBatchId}";
                CurrentBatchId = null;
                CurrentProgress = null;
            }
            else
            {
                Status = "取消批量生成任务失败";
            }
        }
        catch (Exception ex)
        {
            Status = $"取消批量生成失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshProgressAsync()
    {
        if (!CurrentBatchId.HasValue)
        {
            Status = "没有正在进行的批量生成任务";
            return;
        }

        try
        {
            var progress = await _batchGenerationService.GetBatchGenerationProgressAsync(CurrentBatchId.Value);
            CurrentProgress = progress;
            GenerationProgress = progress.ProgressPercentage;

            Status = $"进度: {progress.CompletedCount}/{progress.TotalCount} ({progress.ProgressPercentage:F1}%)";
        }
        catch (Exception ex)
        {
            Status = $"刷新进度失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        try
        {
            GenerationHistory.Clear();
            var history = await _batchGenerationService.GetBatchGenerationHistoryAsync(10, 1);
            foreach (var item in history.Items)
            {
                GenerationHistory.Add(item);
            }
            Status = "历史记录已刷新";
        }
        catch (Exception ex)
        {
            Status = $"刷新历史记录失败: {ex.Message}";
        }
    }

    private async Task MonitorProgressAsync(Guid batchId)
    {
        try
        {
            while (IsGenerating && CurrentBatchId == batchId)
            {
                var progress = await _batchGenerationService.GetBatchGenerationProgressAsync(batchId);
                CurrentProgress = progress;
                GenerationProgress = progress.ProgressPercentage;

                Status = $"批量生成中... {progress.CompletedCount}/{progress.TotalCount} ({progress.ProgressPercentage:F1}%)";

                // 检查是否完成
                if (progress.Status == BatchGenerationStatus.Completed ||
                    progress.Status == BatchGenerationStatus.Failed ||
                    progress.Status == BatchGenerationStatus.Cancelled)
                {
                    IsGenerating = false;
                    
                    if (progress.Status == BatchGenerationStatus.Completed)
                    {
                        Status = $"批量生成完成: {progress.CompletedCount} 份试卷成功生成";
                    }
                    else if (progress.Status == BatchGenerationStatus.Failed)
                    {
                        Status = $"批量生成失败: {progress.ErrorMessage}";
                    }
                    else
                    {
                        Status = "批量生成已取消";
                    }

                    // 刷新历史记录
                    await RefreshHistoryAsync();
                    break;
                }

                // 等待1秒后再次检查
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Status = $"监控进度失败: {ex.Message}";
            IsGenerating = false;
        }
    }

    // OnSelectedQuestionBanksChanged 和 OnSelectedTemplateChanged 方法由 CommunityToolkit.Mvvm.SourceGenerators 自动生成
}