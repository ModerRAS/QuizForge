using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// 题库管理视图模型
/// </summary>
public partial class QuestionBankViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly IFileService _fileService;
    private readonly IMarkdownParser _markdownParser;
    private readonly IExcelParser _excelParser;

    [ObservableProperty]
    private ObservableCollection<QuestionBank> _questionBanks = new();

    [ObservableProperty]
    private QuestionBank? _selectedQuestionBank;

    [ObservableProperty]
    private ObservableCollection<Question> _questions = new();

    [ObservableProperty]
    private Question? _selectedQuestion;

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isImporting;

    [ObservableProperty]
    private double _importProgress;

    public QuestionBankViewModel(
        IQuestionService questionService,
        IFileService fileService,
        IMarkdownParser markdownParser,
        IExcelParser excelParser)
    {
        _questionService = questionService;
        _fileService = fileService;
        _markdownParser = markdownParser;
        _excelParser = excelParser;

        // 初始化数据
        InitializeData();
    }

    private void InitializeData()
    {
        // TODO: 从数据库加载题库列表
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "数学题库", Description = "高中数学题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "英语题库", Description = "高中英语题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        QuestionBanks.Add(new QuestionBank { Id = Guid.NewGuid(), Name = "物理题库", Description = "高中物理题目", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        
        if (QuestionBanks.Count > 0)
        {
            SelectedQuestionBank = QuestionBanks[0];
        }
    }

    partial void OnSelectedQuestionBankChanged(QuestionBank? value)
    {
        if (value != null)
        {
            LoadQuestions(value.Id);
        }
        else
        {
            Questions.Clear();
        }
    }

    private void LoadQuestions(Guid questionBankId)
    {
        // TODO: 从数据库加载题目列表
        Questions.Clear();
        
        // 模拟数据
        for (int i = 1; i <= 10; i++)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Content = $"这是第{i}道题目，内容示例...",
                Type = i % 3 == 0 ? "MultipleChoice" : "SingleChoice",
                Difficulty = (i % 3 + 1).ToString(),
                Category = "示例类别",
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
            Questions.Add(question);
        }
        
        Status = $"已加载 {Questions.Count} 道题目";
    }

    [RelayCommand]
    private async Task ImportQuestionBankAsync()
    {
        // TODO: 实现文件选择对话框
        var filePath = "C:\\QuizForge\\示例题库.md"; // 模拟选择的文件路径
        
        if (string.IsNullOrEmpty(filePath))
        {
            Status = "未选择文件";
            return;
        }

        try
        {
            IsImporting = true;
            ImportProgress = 0;
            Status = "正在导入题库...";

            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            QuestionBankFormat format;

            if (extension == ".md" || extension == ".markdown")
            {
                format = QuestionBankFormat.Markdown;
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                format = QuestionBankFormat.Excel;
            }
            else
            {
                Status = "不支持的文件格式";
                return;
            }

            ImportProgress = 20;

            // 模拟导入过程
            await Task.Delay(1000);
            ImportProgress = 50;

            // 创建新的题库
            var questionBank = new QuestionBank
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                Description = $"从{extension}文件导入",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            ImportProgress = 70;

            // TODO: 实际的导入逻辑
            // 根据格式选择解析器
            // if (format == QuestionBankFormat.Markdown)
            // {
            //     var questions = await _markdownParser.ParseAsync(filePath);
            //     questionBank.Questions = questions;
            //     await _questionService.ImportQuestionsAsync(questionBank.Id, questions);
            // }
            // else if (format == QuestionBankFormat.Excel)
            // {
            //     var questions = await _excelParser.ParseAsync(filePath);
            //     questionBank.Questions = questions;
            //     await _questionService.ImportQuestionsAsync(questionBank.Id, questions);
            // }

            ImportProgress = 90;

            // 添加到题库列表
            QuestionBanks.Add(questionBank);
            SelectedQuestionBank = questionBank;

            ImportProgress = 100;
            Status = $"题库导入完成，共{questionBank.Questions.Count}道题目";
        }
        catch (Exception ex)
        {
            Status = $"导入失败: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void AddQuestion()
    {
        if (SelectedQuestionBank == null)
        {
            Status = "请先选择一个题库";
            return;
        }

        var newQuestion = new Question
        {
            Id = Guid.NewGuid(),
            Content = "新题目",
            Type = "SingleChoice",
            Difficulty = "1",
            Category = "新类别",
            CorrectAnswer = "A",
            Explanation = "这是新题目的解析说明...",
            Points = 5,
            Options = new System.Collections.Generic.List<QuestionOption>
            {
                new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "选项A", IsCorrect = true },
                new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "选项B", IsCorrect = false },
                new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = "选项C", IsCorrect = false },
                new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = "选项D", IsCorrect = false }
            }
        };

        Questions.Add(newQuestion);
        SelectedQuestion = newQuestion;
        Status = "已添加新题目";
    }

    [RelayCommand]
    private void DeleteQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要删除的题目";
            return;
        }

        Questions.Remove(SelectedQuestion);
        Status = "题目已删除";
        SelectedQuestion = null;
    }

    [RelayCommand]
    private void EditQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要编辑的题目";
            return;
        }

        // TODO: 打开题目编辑对话框
        Status = "正在编辑题目...";
    }

    [RelayCommand]
    private void SearchQuestions()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            if (SelectedQuestionBank != null)
            {
                LoadQuestions(SelectedQuestionBank.Id);
            }
            return;
        }

        // TODO: 实现搜索逻辑
        Status = $"搜索包含 \"{SearchText}\" 的题目...";
    }

    [RelayCommand]
    private void PreviewQuestionBank()
    {
        if (SelectedQuestionBank == null)
        {
            Status = "请先选择一个题库";
            return;
        }

        // TODO: 打开题库预览窗口
        Status = $"正在预览题库: {SelectedQuestionBank.Name}";
    }
}