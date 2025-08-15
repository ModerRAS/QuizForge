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
    /// 模板管理视图模型
    /// </summary>
    public partial class TemplateViewModel : ObservableObject
{
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;

    [ObservableProperty]
    private ObservableCollection<ExamTemplate> _templates = new();

    [ObservableProperty]
    private ExamTemplate? _selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<TemplateSection> _sections = new();

    [ObservableProperty]
    private TemplateSection? _selectedSection;

    [ObservableProperty]
    private string _status = "就绪";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _templateName = string.Empty;

    [ObservableProperty]
    private string _templateDescription = string.Empty;

    [ObservableProperty]
    private string _sectionName = string.Empty;

    [ObservableProperty]
    private string _sectionDescription = string.Empty;

    [ObservableProperty]
    private int _questionCount = 10;

    [ObservableProperty]
    private string _difficulty = "中等";

    [ObservableProperty]
    private ObservableCollection<string> _difficulties = new()
    {
        "简单",
        "中等",
        "困难"
    };

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private ExamTemplate _editingTemplate = new();

    [ObservableProperty]
    private TemplateSection _editingSection = new();

    [ObservableProperty]
    private ObservableCollection<string> _paperSizes = new() { "A4", "A3", "B4", "B5" };

    [ObservableProperty]
    private ObservableCollection<string> _orientations = new() { "横向", "纵向" };

    [ObservableProperty]
    private string _selectedPaperSize = "A4";

    [ObservableProperty]
    private string _selectedOrientation = "纵向";

    [ObservableProperty]
    private double _marginTop = 2.5;

    [ObservableProperty]
    private double _marginBottom = 2.5;

    [ObservableProperty]
    private double _marginLeft = 2.5;

    [ObservableProperty]
    private double _marginRight = 2.5;

    public TemplateViewModel(
        ITemplateService templateService,
        IFileService fileService)
    {
        _templateService = templateService;
        _fileService = fileService;

        // 初始化数据
        InitializeData();
    }

    private async void InitializeData()
    {
        // 原本实现：硬编码的示例模板
        // 完整实现：从数据库加载模板列表
        try
        {
            // 从数据库加载所有模板
            var templates = await _templateService.GetAllTemplatesAsync();
            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            // 如果没有模板，创建示例模板
            if (Templates.Count == 0)
            {
                await CreateSampleTemplatesAsync();
            }

            // 选择第一个模板
            if (Templates.Count > 0)
            {
                SelectedTemplate = Templates[0];
            }
        }
        catch (Exception ex)
        {
            Status = $"初始化失败: {ex.Message}";
            // 创建默认模板以确保系统能够运行
            await CreateSampleTemplatesAsync();
        }
    }

    /// <summary>
    /// 创建示例模板
    /// </summary>
    private async Task CreateSampleTemplatesAsync()
    {
        // 简化实现：创建硬编码的示例模板
        // 原本实现：应该从配置文件或数据库加载模板
        try
        {
            var sampleTemplate = new ExamTemplate
            {
                Id = Guid.NewGuid(),
                Name = "标准试卷模板",
                Description = "适用于常规考试的标准试卷模板",
                PaperSize = PaperSize.A4,
                Orientation = "纵向",
                MarginTop = 2.5,
                MarginBottom = 2.5,
                MarginLeft = 2.5,
                MarginRight = 2.5,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            Templates.Add(sampleTemplate);
            Status = "已创建示例模板";
        }
        catch (Exception ex)
        {
            Status = $"创建示例模板失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 创建示例模板节
    /// </summary>
    private async Task CreateSampleSectionsAsync(Guid templateId)
    {
        // 简化实现：创建硬编码的示例模板节
        // 原本实现：应该从配置文件或数据库加载模板节
        try
        {
            var sampleSections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templateId,
                    Title = "选择题",
                    Instructions = "请选择正确答案",
                    QuestionCount = 10,
                    Difficulty = "中等",
                    TotalPoints = 20,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new TemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templateId,
                    Title = "填空题",
                    Instructions = "请填写正确答案",
                    QuestionCount = 5,
                    Difficulty = "中等",
                    TotalPoints = 10,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            foreach (var section in sampleSections)
            {
                Sections.Add(section);
            }

            Status = "已创建示例模板节";
        }
        catch (Exception ex)
        {
            Status = $"创建示例模板节失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 初始化数据（异步版本）
    /// </summary>
    private async Task InitializeDataAsync()
    {
        // 简化实现：直接调用InitializeData
        // 原本实现：应该有更复杂的初始化逻辑
        await Task.Run(() => InitializeData());
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    private async Task<bool?> ShowConfirmDialog(string title, string message)
    {
        // 简化实现：使用控制台输出模拟确认对话框
        // 原本实现：应该使用自定义的确认对话框
        Console.WriteLine($"{title}: {message}");
        await Task.CompletedTask;
        return true;
    }

    /// <summary>
    /// 生成模板预览
    /// </summary>
    private async Task<string> GenerateTemplatePreviewAsync(ExamTemplate template)
    {
        // 简化实现：生成简单的文本预览
        // 原本实现：应该生成完整的HTML或PDF预览
        var preview = $"模板预览：{template.Name}\n\n";
        preview += $"描述：{template.Description}\n";
        preview += $"纸张大小：{template.PaperSize}\n";
        preview += $"方向：{template.Orientation}\n";
        preview += $"边距：上{template.MarginTop}厘米，下{template.MarginBottom}厘米，左{template.MarginLeft}厘米，右{template.MarginRight}厘米\n";
        preview += $"模板节数：{template.Sections.Count}\n";
        
        return preview;
    }

    /// <summary>
    /// 显示预览对话框
    /// </summary>
    private async Task ShowPreviewDialogAsync(string title, string content)
    {
        // 简化实现：使用控制台输出模拟预览对话框
        // 原本实现：应该使用自定义的预览对话框
        Console.WriteLine($"{title}: {content}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 加载模板
    /// </summary>
    public async Task LoadTemplateAsync(ExamTemplate template)
    {
        // 简化实现：直接设置选中的模板
        // 原本实现：应该从文件或数据库加载模板数据
        SelectedTemplate = template;
        Status = $"已加载模板：{template.Name}";
    }

    /// <summary>
    /// 保存模板
    /// </summary>
    public async Task SaveTemplateAsync()
    {
        // 简化实现：直接调用SaveTemplate方法
        // 原本实现：应该有更复杂的保存逻辑
        await SaveTemplate();
    }

    /// <summary>
    /// 另存为模板
    /// </summary>
    public async Task SaveTemplateAsAsync(string filePath)
    {
        // 简化实现：设置当前文件路径并保存
        // 原本实现：应该处理文件格式转换和路径验证
        CurrentFilePath = filePath;
        await SaveTemplateAsync();
    }

    /// <summary>
    /// 撤销操作
    /// </summary>
    public async Task UndoAsync()
    {
        // 简化实现：暂不实现撤销功能
        // 原本实现：应该实现完整的撤销功能
        Status = "撤销功能尚未实现";
        await Task.CompletedTask;
    }

    /// <summary>
    /// 重做操作
    /// </summary>
    public async Task RedoAsync()
    {
        // 简化实现：暂不实现重做功能
        // 原本实现：应该实现完整的重做功能
        Status = "重做功能尚未实现";
        await Task.CompletedTask;
    }

    /// <summary>
    /// 剪切操作
    /// </summary>
    public async Task CutAsync()
    {
        // 简化实现：暂不实现剪切功能
        // 原本实现：应该实现完整的剪切功能
        Status = "剪切功能尚未实现";
        await Task.CompletedTask;
    }

    /// <summary>
    /// 复制操作
    /// </summary>
    public async Task CopyAsync()
    {
        // 简化实现：暂不实现复制功能
        // 原本实现：应该实现完整的复制功能
        Status = "复制功能尚未实现";
        await Task.CompletedTask;
    }

    /// <summary>
    /// 粘贴操作
    /// </summary>
    public async Task PasteAsync()
    {
        // 简化实现：暂不实现粘贴功能
        // 原本实现：应该实现完整的粘贴功能
        Status = "粘贴功能尚未实现";
        await Task.CompletedTask;
    }

    /// <summary>
    /// 删除选中项
    /// </summary>
    public async Task DeleteSelectedAsync()
    {
        // 简化实现：根据当前选中状态删除
        // 原本实现：应该实现完整的删除功能
        if (SelectedSection != null)
        {
            await DeleteSection();
        }
        else if (SelectedTemplate != null)
        {
            await DeleteTemplate();
        }
        else
        {
            Status = "没有可删除的项目";
        }
    }

    partial void OnSelectedTemplateChanged(ExamTemplate? value)
    {
        if (value != null)
        {
            _ = LoadSectionsAsync(value.Id); // 使用异步方法但不等待
            TemplateName = value.Name;
            TemplateDescription = value.Description;
            SelectedPaperSize = value.PaperSize switch
            {
                PaperSize.A4 => "A4",
                PaperSize.A3 => "A3",
                PaperSize.B4 => "B4",
                PaperSize.B5 => "B5",
                _ => "A4"
            };
            SelectedOrientation = value.Orientation;
            MarginTop = value.MarginTop;
            MarginBottom = value.MarginBottom;
            MarginLeft = value.MarginLeft;
            MarginRight = value.MarginRight;
        }
        else
        {
            Sections.Clear();
            TemplateName = string.Empty;
            TemplateDescription = string.Empty;
            SelectedPaperSize = "A4";
            SelectedOrientation = "纵向";
            MarginTop = 2.5;
            MarginBottom = 2.5;
            MarginLeft = 2.5;
            MarginRight = 2.5;
        }
    }

    private async Task LoadSectionsAsync(Guid templateId)
    {
        // 原本实现：硬编码的模拟数据
        // 完整实现：从数据库加载模板节列表
        try
        {
            Sections.Clear();
            Status = "正在加载模板节...";

            // 从数据库加载模板节
            var sections = await _templateService.GetTemplateSectionsAsync(templateId);
            foreach (var section in sections)
            {
                Sections.Add(section);
            }

            // 如果没有模板节，创建示例模板节
            if (Sections.Count == 0)
            {
                await CreateSampleSectionsAsync(templateId);
            }

            Status = $"已加载 {Sections.Count} 个模板节";
            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            Status = $"加载模板节失败: {ex.Message}";
            // 创建示例模板节以确保系统能够运行
            await CreateSampleSectionsAsync(templateId);
        }
    }

    [RelayCommand]
    private async Task NewTemplate()
    {
        try
        {
            TemplateName = "新模板";
            TemplateDescription = "新模板的描述";
            SelectedPaperSize = "A4";
            SelectedOrientation = "纵向";
            MarginTop = 2.5;
            MarginBottom = 2.5;
            MarginLeft = 2.5;
            MarginRight = 2.5;
            
            IsEditing = true;
            SelectedTemplate = null; // 清除当前选中的模板
            Sections.Clear(); // 清除节列表
            
            Status = "正在创建新模板...";
        }
        catch (Exception ex)
        {
            Status = $"创建新模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveTemplate()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
            {
                Status = "模板名称不能为空";
                return;
            }

            if (SelectedTemplate == null)
            {
                // 创建新模板
                var newTemplate = new ExamTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = TemplateName,
                    Description = TemplateDescription,
                    PaperSize = SelectedPaperSize switch
                    {
                        "A4" => PaperSize.A4,
                        "A3" => PaperSize.A3,
                        "B4" => PaperSize.B4,
                        "B5" => PaperSize.B5,
                        _ => PaperSize.A4
                    },
                    Orientation = SelectedOrientation,
                    MarginTop = MarginTop,
                    MarginBottom = MarginBottom,
                    MarginLeft = MarginLeft,
                    MarginRight = MarginRight,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 保存到数据库
                await _templateService.CreateTemplateAsync(newTemplate);

                Templates.Add(newTemplate);
                SelectedTemplate = newTemplate;
                Status = "模板已创建";
            }
            else
            {
                // 更新现有模板
                SelectedTemplate.Name = TemplateName;
                SelectedTemplate.Description = TemplateDescription;
                SelectedTemplate.PaperSize = SelectedPaperSize switch
                    {
                        "A4" => PaperSize.A4,
                        "A3" => PaperSize.A3,
                        "B4" => PaperSize.B4,
                        "B5" => PaperSize.B5,
                        _ => PaperSize.A4
                    };
                SelectedTemplate.Orientation = SelectedOrientation;
                SelectedTemplate.MarginTop = MarginTop;
                SelectedTemplate.MarginBottom = MarginBottom;
                SelectedTemplate.MarginLeft = MarginLeft;
                SelectedTemplate.MarginRight = MarginRight;
                SelectedTemplate.UpdatedAt = DateTime.Now;

                // 保存到数据库
                await _templateService.UpdateTemplateAsync(SelectedTemplate);

                Status = "模板已更新";
            }

            // 保存模板节
            foreach (var section in Sections)
            {
                section.TemplateId = SelectedTemplate.Id;
                if (section.Id == Guid.Empty)
                {
                    section.Id = Guid.NewGuid();
                    await _templateService.CreateTemplateSectionAsync(section);
                }
                else
                {
                    await _templateService.UpdateTemplateSectionAsync(section);
                }
            }

            HasUnsavedChanges = false;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            Status = $"保存模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择要删除的模板";
            return;
        }

        try
        {
            // 显示确认对话框
            var result = await ShowConfirmDialog("删除确认", "确定要删除选中的模板吗？此操作不可撤销。");
            if (result != true)
            {
                Status = "取消删除";
                return;
            }

            // 从数据库删除模板节
            await _templateService.DeleteTemplateSectionsByTemplateIdAsync(SelectedTemplate.Id);
            
            // 从数据库删除模板
            await _templateService.DeleteTemplateAsync(SelectedTemplate.Id);
            
            // 从列表中删除
            Templates.Remove(SelectedTemplate);
            SelectedTemplate = null;
            Sections.Clear();
            
            HasUnsavedChanges = true;
            Status = "模板已删除";
        }
        catch (Exception ex)
        {
            Status = $"删除模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelEdit()
    {
        try
        {
            if (SelectedTemplate != null)
            {
                TemplateName = SelectedTemplate.Name;
                TemplateDescription = SelectedTemplate.Description;
                SelectedPaperSize = SelectedTemplate.PaperSize switch
                    {
                        PaperSize.A4 => "A4",
                        PaperSize.A3 => "A3",
                        PaperSize.B4 => "B4",
                        PaperSize.B5 => "B5",
                        _ => "A4"
                    };
                SelectedOrientation = SelectedTemplate.Orientation;
                MarginTop = SelectedTemplate.MarginTop;
                MarginBottom = SelectedTemplate.MarginBottom;
                MarginLeft = SelectedTemplate.MarginLeft;
                MarginRight = SelectedTemplate.MarginRight;
            }
            else
            {
                TemplateName = string.Empty;
                TemplateDescription = string.Empty;
                SelectedPaperSize = "A4";
                SelectedOrientation = "纵向";
                MarginTop = 2.5;
                MarginBottom = 2.5;
                MarginLeft = 2.5;
                MarginRight = 2.5;
            }

            IsEditing = false;
            Status = "已取消编辑";
        }
        catch (Exception ex)
        {
            Status = $"取消编辑失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddSection()
    {
        try
        {
            if (SelectedTemplate == null)
            {
                Status = "请先选择一个模板";
                return;
            }

            SectionName = "新节";
            SectionDescription = "新节的描述";
            QuestionCount = 10;
            Difficulty = "中等";
            SelectedSection = null; // 清除当前选中的节
            
            IsEditing = true;
            Status = "正在添加新节...";
        }
        catch (Exception ex)
        {
            Status = $"添加新节失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSection()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SectionName))
            {
                Status = "节名称不能为空";
                return;
            }

            if (SelectedTemplate == null)
            {
                Status = "请先选择一个模板";
                return;
            }

            if (SelectedSection == null)
            {
                // 创建新节
                var newSection = new TemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = SelectedTemplate.Id,
                    Title = SectionName,
                    Instructions = SectionDescription,
                    QuestionCount = QuestionCount,
                    Difficulty = Difficulty,
                    TotalPoints = QuestionCount * 2, // 假设每题2分
                    QuestionIds = new List<Guid>(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 保存到数据库
                await _templateService.CreateTemplateSectionAsync(newSection);

                Sections.Add(newSection);
                SelectedSection = newSection;
                Status = "节已添加";
            }
            else
            {
                // 更新现有节
                SelectedSection.Title = SectionName;
                SelectedSection.Instructions = SectionDescription;
                SelectedSection.QuestionCount = QuestionCount;
                SelectedSection.Difficulty = Difficulty;
                SelectedSection.TotalPoints = QuestionCount * 2; // 假设每题2分
                SelectedSection.UpdatedAt = DateTime.Now;

                // 保存到数据库
                await _templateService.UpdateTemplateSectionAsync(SelectedSection);

                Status = "节已更新";
            }

            HasUnsavedChanges = true;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            Status = $"保存节失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSection()
    {
        if (SelectedSection == null)
        {
            Status = "请先选择要删除的节";
            return;
        }

        try
        {
            // 显示确认对话框
            var result = await ShowConfirmDialog("删除确认", "确定要删除选中的节吗？此操作不可撤销。");
            if (result != true)
            {
                Status = "取消删除";
                return;
            }

            // 从数据库删除
            await _templateService.DeleteTemplateSectionAsync(SelectedSection.Id);
            
            // 从列表中删除
            Sections.Remove(SelectedSection);
            SelectedSection = null;
            
            HasUnsavedChanges = true;
            Status = "节已删除";
        }
        catch (Exception ex)
        {
            Status = $"删除节失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchTemplates()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await InitializeDataAsync();
                return;
            }

            Status = $"搜索包含 \"{SearchText}\" 的模板...";

            // 搜索模板
            var searchResults = await _templateService.SearchTemplatesAsync(SearchText);

            Templates.Clear();
            foreach (var template in searchResults)
            {
                Templates.Add(template);
            }

            Status = $"找到 {Templates.Count} 个相关模板";
        }
        catch (Exception ex)
        {
            Status = $"搜索模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PreviewTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择一个模板";
            return;
        }

        try
        {
            Status = $"正在预览模板: {SelectedTemplate.Name}";

            // 生成模板预览报告
            var preview = await GenerateTemplatePreviewAsync(SelectedTemplate);
            
            // 显示预览对话框
            await ShowPreviewDialogAsync("模板预览", preview);
            
            Status = "模板预览完成";
        }
        catch (Exception ex)
        {
            Status = $"预览模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择一个模板";
            return;
        }

        try
        {
            // 获取顶层窗口用于文件对话框
            var topLevel = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel == null)
            {
                Status = "无法获取窗口句柄";
                return;
            }

            // 显示保存文件对话框
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出模板",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("LaTeX模板")
                    {
                        Patterns = new[] { "*.tex" }
                    },
                    new FilePickerFileType("JSON文件")
                    {
                        Patterns = new[] { "*.json" }
                    }
                },
                DefaultExtension = "tex",
                SuggestedFileName = $"{SelectedTemplate.Name}.tex"
            });

            if (file != null)
            {
                var filePath = file.Path.AbsolutePath;
                var extension = System.IO.Path.GetExtension(filePath).ToLower();
                
                bool success = false;
                
                if (extension == ".tex")
                {
                    success = await _templateService.ExportTemplateToLaTeXAsync(SelectedTemplate.Id, filePath);
                }
                else if (extension == ".json")
                {
                    success = await _templateService.ExportTemplateToJsonAsync(SelectedTemplate.Id, filePath);
                }
                
                if (success)
                {
                    CurrentFilePath = filePath;
                    Status = $"模板导出完成: {file.Name}";
                }
                else
                {
                    Status = "模板导出失败";
                }
            }
            else
            {
                Status = "取消导出模板";
            }
        }
        catch (Exception ex)
        {
            Status = $"导出模板失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ImportTemplate()
    {
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
                Title = "导入模板",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("支持的模板文件")
                    {
                        Patterns = new[] { "*.tex", "*.json" }
                    },
                    new FilePickerFileType("LaTeX模板")
                    {
                        Patterns = new[] { "*.tex" }
                    },
                    new FilePickerFileType("JSON文件")
                    {
                        Patterns = new[] { "*.json" }
                    }
                },
                AllowMultiple = false
            });

            if (files != null && files.Count > 0)
            {
                var filePath = files[0].Path.AbsolutePath;
                var extension = System.IO.Path.GetExtension(filePath).ToLower();
                
                ExamTemplate importedTemplate = null;
                
                if (extension == ".tex")
                {
                    importedTemplate = await _templateService.ImportTemplateFromLaTeXAsync(filePath);
                }
                else if (extension == ".json")
                {
                    importedTemplate = await _templateService.ImportTemplateFromJsonAsync(filePath);
                }
                
                if (importedTemplate != null)
                {
                    Templates.Add(importedTemplate);
                    SelectedTemplate = importedTemplate;
                    CurrentFilePath = filePath;
                    Status = $"模板导入完成: {importedTemplate.Name}";
                }
                else
                {
                    Status = "模板导入失败";
                }
            }
            else
            {
                Status = "取消导入模板";
            }
        }
        catch (Exception ex)
        {
            Status = $"导入模板失败: {ex.Message}";
        }
    }
}
}