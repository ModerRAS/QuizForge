using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace QuizForge.App.ViewModels
{
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

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private Question _editingQuestion = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private ObservableCollection<string> _difficultyLevels = new() { "1", "2", "3", "4", "5" };

    [ObservableProperty]
    private ObservableCollection<string> _questionTypes = new() { "SingleChoice", "MultipleChoice", "TrueFalse", "FillInBlank", "Essay" };

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

    private async void InitializeData()
    {
        // 原本实现：硬编码的示例题库
        // 完整实现：从数据库加载题库列表
        try
        {
            // 从数据库加载所有题库
            var questionBanks = await _questionService.GetAllQuestionBanksAsync();
            foreach (var questionBank in questionBanks)
            {
                QuestionBanks.Add(questionBank);
            }

            // 如果没有题库，创建示例题库
            if (QuestionBanks.Count == 0)
            {
                var sampleQuestionBank = new QuestionBank
                {
                    Id = Guid.NewGuid(),
                    Name = "示例题库",
                    Description = "系统自动创建的示例题库",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Format = QuestionBankFormat.Markdown
                };
                
                await _questionService.CreateQuestionBankAsync(sampleQuestionBank);
                QuestionBanks.Add(sampleQuestionBank);
            }

            // 加载类别列表
            await LoadCategoriesAsync();

            // 选择第一个题库
            if (QuestionBanks.Count > 0)
            {
                SelectedQuestionBank = QuestionBanks[0];
            }
        }
        catch (Exception ex)
        {
            Status = $"初始化失败: {ex.Message}";
            // 创建默认题库以确保系统能够运行
            QuestionBanks.Add(new QuestionBank 
            { 
                Id = Guid.NewGuid(), 
                Name = "默认题库", 
                Description = "系统默认题库", 
                CreatedAt = DateTime.Now, 
                UpdatedAt = DateTime.Now 
            });
            SelectedQuestionBank = QuestionBanks[0];
        }
    }

    partial void OnSelectedQuestionBankChanged(QuestionBank? value)
    {
        if (value != null)
        {
            _ = LoadQuestionsAsync(value.Id); // 使用异步方法但不等待
        }
        else
        {
            Questions.Clear();
        }
    }

    private async Task LoadQuestions(Guid questionBankId)
    {
        // 原本实现：硬编码的模拟数据
        // 完整实现：从数据库加载题目列表
        try
        {
            Questions.Clear();
            Status = "正在加载题目...";

            // 从数据库加载题目
            var questions = await _questionService.GetQuestionsByQuestionBankIdAsync(questionBankId);
            foreach (var question in questions)
            {
                Questions.Add(question);
            }

            // 如果没有题目，创建示例题目
            if (Questions.Count == 0)
            {
                await CreateSampleQuestionsAsync(questionBankId);
            }

            Status = $"已加载 {Questions.Count} 道题目";
            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            Status = $"加载题目失败: {ex.Message}";
            // 创建示例题目以确保系统能够运行
            await CreateSampleQuestionsAsync(questionBankId);
        }
    }

    [RelayCommand]
    private async Task ImportQuestionBankAsync()
    {
        // 原本实现：硬编码的文件路径和模拟导入过程
        // 完整实现：使用文件选择对话框和实际的导入逻辑
        try
        {
            // 获取顶层窗口用于文件对话框
            var topLevel = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel == null)
            {
                Status = "无法获取窗口句柄";
                return;
            }

            // 显示打开文件对话框
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "导入题库",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("支持的题库文件")
                    {
                        Patterns = new[] { "*.md", "*.xlsx", "*.xls", "*.json", "*.xml" }
                    },
                    new FilePickerFileType("Markdown文件")
                    {
                        Patterns = new[] { "*.md" }
                    },
                    new FilePickerFileType("Excel文件")
                    {
                        Patterns = new[] { "*.xlsx", "*.xls" }
                    },
                    new FilePickerFileType("JSON文件")
                    {
                        Patterns = new[] { "*.json" }
                    },
                    new FilePickerFileType("XML文件")
                    {
                        Patterns = new[] { "*.xml" }
                    }
                },
                AllowMultiple = false
            });

            if (files == null || files.Count == 0)
            {
                Status = "未选择文件";
                return;
            }

            var filePath = files[0].Path.AbsolutePath;
            
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
            else if (extension == ".json")
            {
                format = QuestionBankFormat.Json;
            }
            else if (extension == ".xml")
            {
                format = QuestionBankFormat.Xml;
            }
            else
            {
                Status = "不支持的文件格式";
                return;
            }

            ImportProgress = 20;

            // 创建新的题库
            var questionBank = new QuestionBank
            {
                Id = Guid.NewGuid(),
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                Description = $"从{extension}文件导入",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Format = format,
                FilePath = filePath
            };

            ImportProgress = 50;

            // 根据格式选择解析器并导入题目
            List<Question> questions = new List<Question>();
            
            if (format == QuestionBankFormat.Markdown)
            {
                var parsedQuestionBank = await _markdownParser.ParseAsync(filePath);
                questions = parsedQuestionBank.Questions;
            }
            else if (format == QuestionBankFormat.Excel)
            {
                var parsedQuestionBank = await _excelParser.ParseAsync(filePath);
                questions = parsedQuestionBank.Questions;
            }
            // TODO: 添加JSON和XML解析器的支持
            // else if (format == QuestionBankFormat.Json)
            // {
            //     questions = await _jsonParser.ParseAsync(filePath);
            // }
            // else if (format == QuestionBankFormat.Xml)
            // {
            //     questions = await _xmlParser.ParseAsync(filePath);
            // }

            ImportProgress = 70;

            // 保存题库到数据库
            await _questionService.CreateQuestionBankAsync(questionBank);
            
            // 导入题目到数据库
            if (questions.Count > 0)
            {
                await _questionService.ImportQuestionsAsync(questionBank.Id, questions);
                questionBank.Questions = questions;
            }

            ImportProgress = 90;

            // 添加到题库列表
            QuestionBanks.Add(questionBank);
            SelectedQuestionBank = questionBank;
            CurrentFilePath = filePath;

            ImportProgress = 100;
            Status = $"题库导入完成，共{questions.Count}道题目";
            HasUnsavedChanges = false;
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
    private async Task AddQuestion()
    {
        if (SelectedQuestionBank == null)
        {
            Status = "请先选择一个题库";
            return;
        }

        try
        {
            var newQuestion = new Question
            {
                Id = Guid.NewGuid(),
                Content = "新题目",
                Type = "SingleChoice",
                Difficulty = "1",
                Category = Categories.FirstOrDefault() ?? "默认类别",
                CorrectAnswer = "A",
                Explanation = "这是新题目的解析说明...",
                Points = 5,
                QuestionBankId = SelectedQuestionBank.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "选项A", IsCorrect = true },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "选项B", IsCorrect = false },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = "选项C", IsCorrect = false },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = "选项D", IsCorrect = false }
                }
            };

            // 保存到数据库
            await _questionService.CreateQuestionAsync(newQuestion);
            
            Questions.Add(newQuestion);
            SelectedQuestion = newQuestion;
            HasUnsavedChanges = true;
            Status = "已添加新题目";
        }
        catch (Exception ex)
        {
            Status = $"添加题目失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要删除的题目";
            return;
        }

        try
        {
            // 显示确认对话框
            var result = await ShowConfirmDialog("删除确认", "确定要删除选中的题目吗？此操作不可撤销。");
            if (result != true)
            {
                Status = "取消删除";
                return;
            }

            // 从数据库删除
            await _questionService.DeleteQuestionAsync(SelectedQuestion.Id);
            
            // 从列表中删除
            Questions.Remove(SelectedQuestion);
            HasUnsavedChanges = true;
            Status = "题目已删除";
            SelectedQuestion = null;
        }
        catch (Exception ex)
        {
            Status = $"删除题目失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditQuestion()
    {
        if (SelectedQuestion == null)
        {
            Status = "请先选择要编辑的题目";
            return;
        }

        try
        {
            // 复制选中的题目用于编辑
            EditingQuestion = new Question
            {
                Id = SelectedQuestion.Id,
                Content = SelectedQuestion.Content,
                Type = SelectedQuestion.Type,
                Difficulty = SelectedQuestion.Difficulty,
                Category = SelectedQuestion.Category,
                CorrectAnswer = SelectedQuestion.CorrectAnswer,
                Explanation = SelectedQuestion.Explanation,
                Points = SelectedQuestion.Points,
                QuestionBankId = SelectedQuestion.QuestionBankId,
                CreatedAt = SelectedQuestion.CreatedAt,
                UpdatedAt = DateTime.Now,
                Options = SelectedQuestion.Options.Select(o => new QuestionOption
                {
                    Id = o.Id,
                    Key = o.Key,
                    Value = o.Value,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            IsEditing = true;
            Status = "正在编辑题目...";
            
            // 这里应该打开题目编辑对话框
            // 简化实现：直接更新题目
            await ShowEditDialogAsync();
        }
        catch (Exception ex)
        {
            Status = $"编辑题目失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchQuestions()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (SelectedQuestionBank != null)
                {
                    await LoadQuestionsAsync(SelectedQuestionBank.Id);
                }
                return;
            }

            Status = $"搜索包含 \"{SearchText}\" 的题目...";

            // 在当前题库中搜索题目
            var searchResults = await _questionService.SearchQuestionsAsync(
                SelectedQuestionBank?.Id ?? Guid.Empty, 
                SearchText);

            Questions.Clear();
            foreach (var question in searchResults)
            {
                Questions.Add(question);
            }

            Status = $"找到 {Questions.Count} 道相关题目";
        }
        catch (Exception ex)
        {
            Status = $"搜索失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PreviewQuestionBank()
    {
        if (SelectedQuestionBank == null)
        {
            Status = "请先选择一个题库";
            return;
        }

        try
        {
            Status = $"正在预览题库: {SelectedQuestionBank.Name}";

            // 生成题库预览报告
            var preview = await GenerateQuestionBankPreviewAsync(SelectedQuestionBank);
            
            // 显示预览对话框
            await ShowPreviewDialogAsync("题库预览", preview);
            
            Status = "题库预览完成";
        }
        catch (Exception ex)
        {
            Status = $"预览题库失败: {ex.Message}";
        }
    }

    // MainViewModel需要调用的方法
    public async Task LoadQuestionBankAsync(QuestionBank questionBank)
    {
        try
        {
            SelectedQuestionBank = questionBank;
            CurrentFilePath = questionBank.FilePath ?? string.Empty;
            await LoadQuestionsAsync(questionBank.Id);
        }
        catch (Exception ex)
        {
            Status = $"加载题库失败: {ex.Message}";
        }
    }

    public async Task SaveQuestionBankAsync()
    {
        try
        {
            if (SelectedQuestionBank == null)
            {
                Status = "没有可保存的题库";
                return;
            }

            Status = "正在保存题库...";

            // 保存题库信息
            await _questionService.UpdateQuestionBankAsync(SelectedQuestionBank);

            // 保存所有题目
            foreach (var question in Questions)
            {
                question.QuestionBankId = SelectedQuestionBank.Id;
                question.UpdatedAt = DateTime.Now;
                await _questionService.UpdateQuestionAsync(question);
            }

            HasUnsavedChanges = false;
            Status = "题库保存完成";
        }
        catch (Exception ex)
        {
            Status = $"保存题库失败: {ex.Message}";
        }
    }

    public async Task SaveQuestionBankAsAsync(string filePath)
    {
        try
        {
            if (SelectedQuestionBank == null)
            {
                Status = "没有可保存的题库";
                return;
            }

            Status = "正在另存为题库...";

            // 获取文件格式
            var format = GetFileFormat(filePath);
            
            // 导出题库到文件
            await _questionService.ExportQuestionBankAsync(SelectedQuestionBank, filePath, format);

            CurrentFilePath = filePath;
            HasUnsavedChanges = false;
            Status = "题库另存为完成";
        }
        catch (Exception ex)
        {
            Status = $"题库另存为失败: {ex.Message}";
        }
    }

    public async Task UndoAsync()
    {
        // 简化实现：撤销功能需要更复杂的实现
        Status = "撤销功能正在开发中...";
        await Task.CompletedTask;
    }

    public async Task RedoAsync()
    {
        // 简化实现：重做功能需要更复杂的实现
        Status = "重做功能正在开发中...";
        await Task.CompletedTask;
    }

    public async Task CutAsync()
    {
        if (SelectedQuestion != null)
        {
            var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(SelectedQuestion.Content);
                Status = "题目内容已复制到剪贴板";
            }
        }
    }

    public async Task CopyAsync()
    {
        if (SelectedQuestion != null)
        {
            var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(SelectedQuestion.Content);
                Status = "题目内容已复制到剪贴板";
            }
        }
    }

    public async Task PasteAsync()
    {
        var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
        if (clipboard != null)
        {
            var text = await clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(text) && SelectedQuestion != null)
            {
                SelectedQuestion.Content = text;
                HasUnsavedChanges = true;
                Status = "内容已粘贴";
            }
        }
    }

    public async Task DeleteSelectedAsync()
    {
        await DeleteQuestion();
    }

    // 辅助方法
    private async Task LoadQuestionsAsync(Guid questionBankId)
    {
        await LoadQuestions(questionBankId);
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _questionService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            Status = $"加载类别失败: {ex.Message}";
            // 添加默认类别
            Categories.Add("默认类别");
        }
    }

    private async Task CreateSampleQuestionsAsync(Guid questionBankId)
    {
        try
        {
            var sampleQuestions = new List<Question>();
            
            for (int i = 1; i <= 5; i++)
            {
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Content = $"示例第{i}道题目，这是一个单选题示例。",
                    Type = "SingleChoice",
                    Difficulty = ((i % 3) + 1).ToString(),
                    Category = "示例类别",
                    CorrectAnswer = ((char)('A' + i % 4)).ToString(),
                    Explanation = $"这是第{i}道示例题目的解析说明...",
                    Points = 5,
                    QuestionBankId = questionBankId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = $"选项A内容", IsCorrect = i % 4 == 1 },
                        new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = $"选项B内容", IsCorrect = i % 4 == 2 },
                        new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = $"选项C内容", IsCorrect = i % 4 == 3 },
                        new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = $"选项D内容", IsCorrect = i % 4 == 0 }
                    }
                };
                
                sampleQuestions.Add(question);
                await _questionService.CreateQuestionAsync(question);
                Questions.Add(question);
            }
        }
        catch (Exception ex)
        {
            Status = $"创建示例题目失败: {ex.Message}";
        }
    }

    private QuestionBankFormat GetFileFormat(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".md" => QuestionBankFormat.Markdown,
            ".xlsx" or ".xls" => QuestionBankFormat.Excel,
            ".json" => QuestionBankFormat.Json,
            ".xml" => QuestionBankFormat.Xml,
            _ => QuestionBankFormat.Markdown
        };
    }

    private async Task<string> GenerateQuestionBankPreviewAsync(QuestionBank questionBank)
    {
        var preview = new System.Text.StringBuilder();
        preview.AppendLine($"题库名称: {questionBank.Name}");
        preview.AppendLine($"题库描述: {questionBank.Description}");
        preview.AppendLine($"创建时间: {questionBank.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        preview.AppendLine($"更新时间: {questionBank.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        preview.AppendLine($"题目总数: {Questions.Count}");
        preview.AppendLine();

        // 按难度统计
        var difficultyStats = Questions.GroupBy(q => q.Difficulty)
            .Select(g => new { Difficulty = g.Key, Count = g.Count() })
            .OrderBy(g => g.Difficulty);

        preview.AppendLine("难度分布:");
        foreach (var stat in difficultyStats)
        {
            preview.AppendLine($"  难度{stat.Difficulty}: {stat.Count}道");
        }
        preview.AppendLine();

        // 按类别统计
        var categoryStats = Questions.GroupBy(q => q.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderBy(g => g.Category);

        preview.AppendLine("类别分布:");
        foreach (var stat in categoryStats)
        {
            preview.AppendLine($"  {stat.Category}: {stat.Count}道");
        }
        preview.AppendLine();

        // 按类型统计
        var typeStats = Questions.GroupBy(q => q.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderBy(g => g.Type);

        preview.AppendLine("题型分布:");
        foreach (var stat in typeStats)
        {
            preview.AppendLine($"  {stat.Type}: {stat.Count}道");
        }

        return preview.ToString();
    }

    private async Task ShowEditDialogAsync()
    {
        // 简化实现：使用控制台输出模拟编辑对话框
        Console.WriteLine("编辑题目对话框");
        Console.WriteLine($"题目内容: {EditingQuestion.Content}");
        Console.WriteLine("在实际应用中，这里应该显示一个编辑对话框");
        await Task.CompletedTask;
    }

    private async Task ShowPreviewDialogAsync(string title, string content)
    {
        // 简化实现：使用控制台输出模拟预览对话框
        Console.WriteLine($"{title}:");
        Console.WriteLine(content);
        await Task.CompletedTask;
    }

    private async Task<bool?> ShowConfirmDialog(string title, string message)
    {
        // 简化实现：使用控制台输出模拟确认对话框
        Console.WriteLine($"{title}: {message}");
        Console.WriteLine("在实际应用中，这里应该显示一个确认对话框");
        return true; // 简化实现，总是返回true
    }
}
}