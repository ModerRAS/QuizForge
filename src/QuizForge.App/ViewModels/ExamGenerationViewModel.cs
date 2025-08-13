using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 试卷生成视图模型
/// </summary>
public partial class ExamGenerationViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IGenerationService _generationService;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private ObservableCollection<QuestionBank> _questionBanks = new();

    [ObservableProperty]
    private QuestionBank? _selectedQuestionBank;

    [ObservableProperty]
    private ObservableCollection<ExamTemplate> _templates = new();

    [ObservableProperty]
    private ExamTemplate? _selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<Question> _selectedQuestions = new();

    [ObservableProperty]
    private Question? _selectedQuestion;

    [ObservableProperty]
    private ObservableCollection<Question> _availableQuestions = new();

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private string _examTitle = "新试卷";

    [ObservableProperty]
    private string _examDescription = "试卷描述";

    [ObservableProperty]
    private int _totalQuestions = 20;

    [ObservableProperty]
    private string _difficultyFilter = "全部";

    [ObservableProperty]
    private string _categoryFilter = "全部";

    [ObservableProperty]
    private bool _isRandomSelection = true;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private double _generationProgress;

    [ObservableProperty]
    private ExamPaper? _generatedExamPaper;

    public ExamGenerationViewModel(
        IQuestionService questionService,
        ITemplateService templateService,
        IGenerationService generationService,
        IExportService exportService)
    {
        _questionService = questionService;
        _templateService = templateService;
        _generationService = generationService;
        _exportService = exportService;

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
        
        if (QuestionBanks.Count > 0)
        {
            SelectedQuestionBank = QuestionBanks[0];
        }
        
        if (Templates.Count > 0)
        {
            SelectedTemplate = Templates[0];
        }
    }

    partial void OnSelectedQuestionBankChanged(QuestionBank? value)
    {
        if (value != null)
        {
            LoadAvailableQuestions(value.Id);
        }
        else
        {
            AvailableQuestions.Clear();
        }
    }

    private void LoadAvailableQuestions(Guid questionBankId)
    {
        // TODO: 从数据库加载可用题目列表
        AvailableQuestions.Clear();
        
        // 模拟数据
        for (int i = 1; i <= 30; i++)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Content = $"这是第{i}道题目，内容示例...",
                Type = i % 3 == 0 ? "MultipleChoice" : "SingleChoice",
                Difficulty = (i % 3 + 1).ToString(),
                Category = i % 2 == 0 ? "代数" : "几何",
                CorrectAnswer = i % 4 == 0 ? "D" : ((char)('A' + i % 4)).ToString(),
                Explanation = $"这是第{i}道题的解析说明...",
                Points = 5,
                Options = new System.Collections.Generic.List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = $"选项A", IsCorrect = i % 4 == 1 },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = $"选项B", IsCorrect = i % 4 == 2 },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = $"选项C", IsCorrect = i % 4 == 3 },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = $"选项D", IsCorrect = i % 4 == 0 }
                }
            };
            AvailableQuestions.Add(question);
        }
        
        Status = $"已加载 {AvailableQuestions.Count} 道可用题目";
    }

    [RelayCommand]
    private void AddQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要添加的题目";
            return;
        }

        if (SelectedQuestions.Contains(SelectedQuestion))
        {
            Status = "该题目已在试卷中";
            return;
        }

        SelectedQuestions.Add(SelectedQuestion);
        Status = $"已添加题目，当前共 {SelectedQuestions.Count} 道题目";
    }

    [RelayCommand]
    private void RemoveQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要移除的题目";
            return;
        }

        if (!SelectedQuestions.Contains(SelectedQuestion))
        {
            Status = "该题目不在试卷中";
            return;
        }

        SelectedQuestions.Remove(SelectedQuestion);
        Status = $"已移除题目，当前共 {SelectedQuestions.Count} 道题目";
    }

    [RelayCommand]
    private void AddRandomQuestions()
    {
        if (AvailableQuestions.Count == 0)
        {
            Status = "没有可用的题目";
            return;
        }

        var questionsToAdd = Math.Min(TotalQuestions - SelectedQuestions.Count, AvailableQuestions.Count);
        if (questionsToAdd <= 0)
        {
            Status = "已达到题目数量上限";
            return;
        }

        var random = new Random();
        var availableQuestionsCopy = AvailableQuestions.ToList();
        
        for (int i = 0; i < questionsToAdd; i++)
        {
            var index = random.Next(availableQuestionsCopy.Count);
            var question = availableQuestionsCopy[index];
            
            if (!SelectedQuestions.Contains(question))
            {
                SelectedQuestions.Add(question);
            }
            
            availableQuestionsCopy.RemoveAt(index);
        }
        
        Status = $"已随机添加 {questionsToAdd} 道题目，当前共 {SelectedQuestions.Count} 道题目";
    }

    [RelayCommand]
    private void ClearAllQuestions()
    {
        SelectedQuestions.Clear();
        Status = "已清空所有题目";
    }

    [RelayCommand]
    private async Task GenerateExamAsync()
    {
        if (SelectedQuestions.Count == 0)
        {
            Status = "请先选择题目";
            return;
        }

        if (SelectedTemplate == null)
        {
            Status = "请先选择模板";
            return;
        }

        try
        {
            IsGenerating = true;
            GenerationProgress = 0;
            Status = "正在生成试卷...";

            // 模拟生成过程
            await Task.Delay(1000);
            GenerationProgress = 30;

            // TODO: 实际的生成逻辑
            // var examPaper = await _generationService.GenerateExamPaperAsync(
            //     SelectedTemplate.Id,
            //     SelectedQuestions.Select(q => q.Id).ToList(),
            //     ExamTitle,
            //     ExamDescription);

            await Task.Delay(1000);
            GenerationProgress = 70;

            // 创建模拟的试卷对象
            GeneratedExamPaper = new ExamPaper
            {
                Id = Guid.NewGuid(),
                Title = ExamTitle,
                TemplateId = SelectedTemplate.Id,
                QuestionBankId = SelectedQuestionBank?.Id ?? Guid.Empty,
                Content = ExamDescription,
                CreatedAt = DateTime.Now,
                TotalPoints = SelectedQuestions.Sum(q => q.Points),
                Questions = SelectedQuestions.ToList()
            };

            await Task.Delay(500);
            GenerationProgress = 100;

            Status = $"试卷生成完成，共 {SelectedQuestions.Count} 道题目";
        }
        catch (Exception ex)
        {
            Status = $"生成失败: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task ExportExamAsync()
    {
        if (GeneratedExamPaper == null)
        {
            Status = "请先生成试卷";
            return;
        }

        try
        {
            Status = "正在导出试卷...";
            
            // TODO: 实际的导出逻辑
            // await _exportService.ExportToPdfAsync(GeneratedExamPaper);
            
            // 模拟导出过程
            await Task.Delay(1000);
            
            Status = $"试卷已导出到: C:\\QuizForge\\Generated\\{GeneratedExamPaper.Title}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        }
        catch (Exception ex)
        {
            Status = $"导出失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void FilterQuestions()
    {
        // TODO: 实现题目过滤逻辑
        Status = $"正在过滤题目: 难度={DifficultyFilter}, 类别={CategoryFilter}";
    }

    [RelayCommand]
    private void PreviewExam()
    {
        if (GeneratedExamPaper == null)
        {
            Status = "请先生成试卷";
            return;
        }

        // TODO: 打开试卷预览窗口
        Status = $"正在预览试卷: {GeneratedExamPaper.Title}";
    }
}