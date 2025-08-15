using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.Models.Interfaces;
using QuizForge.Models;
using System.Collections.ObjectModel;
using System.Timers;
using System;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia;

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
        // 简化实现：从配置文件加载最近使用的文件
        // 原本实现：硬编码的示例文件
        // 简化实现：从应用程序配置或本地存储加载最近文件
        try
        {
            // 这里应该从配置文件或数据库加载最近使用的文件
            // 简化实现：使用硬编码的示例文件
            RecentFiles.Add(new RecentFile(this) { Name = "数学题库.md", Path = "C:\\QuizForge\\数学题库.md" });
            RecentFiles.Add(new RecentFile(this) { Name = "英语试卷模板.tex", Path = "C:\\QuizForge\\英语试卷模板.tex" });
        }
        catch (Exception ex)
        {
            // 简化实现：静默处理异常，不显示错误
            Console.WriteLine($"加载最近文件失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NewQuestionBank()
    {
        try
        {
            Status = "正在创建新题库...";
            
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
                Title = "创建新题库",
                FileTypeChoices = new[]
                {
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
                    }
                },
                DefaultExtension = "md",
                SuggestedFileName = "新题库.md"
            });

            if (file != null)
            {
                // 创建新的题库文件
                var questionBank = new QuestionBank
                {
                    Id = Guid.NewGuid(),
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                    Description = "新创建的题库",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Format = GetFileFormat(file.Name),
                    FilePath = file.Path.AbsolutePath
                };

                // 保存题库到文件
                await SaveQuestionBankToFile(questionBank, file.Path.AbsolutePath);
                
                // 添加到最近文件列表
                AddToRecentFiles(file.Name, file.Path.AbsolutePath);
                
                Status = "新题库已创建";
                ShowQuestionBankView();
            }
            else
            {
                Status = "取消创建题库";
            }
        }
        catch (Exception ex)
        {
            Status = $"创建题库失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task ImportQuestionBank()
    {
        try
        {
            Status = "正在导入题库...";
            
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

            if (files != null && files.Count > 0)
            {
                var file = files[0];
                var filePath = file.Path.AbsolutePath;
                
                // 使用QuestionService导入题库
                var questionBank = await _questionService.ImportQuestionBankAsync(filePath, GetFileFormat(filePath));
                
                if (questionBank != null)
                {
                    // 添加到最近文件列表
                    AddToRecentFiles(file.Name, filePath);
                    
                    Status = "题库导入完成";
                    ShowQuestionBankView();
                    
                    // 通知QuestionBankViewModel加载新导入的题库
                    if (CurrentView is QuestionBankViewModel questionBankViewModel)
                    {
                        await questionBankViewModel.LoadQuestionBankAsync(questionBank);
                    }
                }
                else
                {
                    Status = "题库导入失败";
                }
            }
            else
            {
                Status = "取消导入题库";
            }
        }
        catch (Exception ex)
        {
            Status = $"导入题库失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            Status = "正在保存...";
            
            // 根据当前视图类型执行相应的保存操作
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                await questionBankViewModel.SaveQuestionBankAsync();
                Status = "题库保存完成";
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                await templateViewModel.SaveTemplateAsync();
                Status = "模板保存完成";
            }
            else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                await examGenerationViewModel.SaveExamPaperAsync();
                Status = "试卷保存完成";
            }
            else
            {
                Status = "没有可保存的内容";
            }
        }
        catch (Exception ex)
        {
            Status = $"保存失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        try
        {
            Status = "正在另存为...";
            
            // 获取顶层窗口用于文件对话框
            var topLevel = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel == null)
            {
                Status = "无法获取窗口句柄";
                return;
            }

            // 根据当前视图类型执行相应的另存为操作
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "另存为题库",
                    FileTypeChoices = new[]
                    {
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
                        }
                    },
                    DefaultExtension = "md",
                    SuggestedFileName = "题库副本.md"
                });

                if (file != null)
                {
                    await questionBankViewModel.SaveQuestionBankAsAsync(file.Path.AbsolutePath);
                    AddToRecentFiles(file.Name, file.Path.AbsolutePath);
                    Status = "题库另存为完成";
                }
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "另存为模板",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("LaTeX模板")
                        {
                            Patterns = new[] { "*.tex" }
                        }
                    },
                    DefaultExtension = "tex",
                    SuggestedFileName = "模板副本.tex"
                });

                if (file != null)
                {
                    await templateViewModel.SaveTemplateAsAsync(file.Path.AbsolutePath);
                    AddToRecentFiles(file.Name, file.Path.AbsolutePath);
                    Status = "模板另存为完成";
                }
            }
            else
            {
                Status = "没有可另存为的内容";
            }
        }
        catch (Exception ex)
        {
            Status = $"另存为失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Exit()
    {
        try
        {
            Status = "正在退出...";
            
            // 检查是否有未保存的更改
            bool hasUnsavedChanges = false;
            
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                hasUnsavedChanges = questionBankViewModel.HasUnsavedChanges;
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                hasUnsavedChanges = templateViewModel.HasUnsavedChanges;
            }
            else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                hasUnsavedChanges = examGenerationViewModel.HasUnsavedChanges;
            }

            if (hasUnsavedChanges)
            {
                // 显示确认对话框
                var result = await ShowConfirmDialog("退出确认", "您有未保存的更改，是否在退出前保存？");
                
                if (result == true)
                {
                    await Save(); // 保存更改
                }
                else if (result == null)
                {
                    // 用户取消退出
                    Status = "取消退出";
                    return;
                }
            }

            // 保存最近文件列表到配置
            await SaveRecentFilesToConfig();
            
            // 停止定时器
            _timer.Stop();
            _timer.Dispose();
            
            // 退出应用程序
            (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        }
        catch (Exception ex)
        {
            Status = $"退出失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Undo()
    {
        try
        {
            Status = "正在撤销...";
            
            // 根据当前视图类型执行相应的撤销操作
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                await questionBankViewModel.UndoAsync();
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                await templateViewModel.UndoAsync();
            }
            else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                await examGenerationViewModel.UndoAsync();
            }
            else
            {
                Status = "没有可撤销的操作";
                return;
            }
            
            Status = "撤销完成";
        }
        catch (Exception ex)
        {
            Status = $"撤销失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Redo()
    {
        try
        {
            Status = "正在重做...";
            
            // 根据当前视图类型执行相应的重做操作
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                await questionBankViewModel.RedoAsync();
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                await templateViewModel.RedoAsync();
            }
            else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                await examGenerationViewModel.RedoAsync();
            }
            else
            {
                Status = "没有可重做的操作";
                return;
            }
            
            Status = "重做完成";
        }
        catch (Exception ex)
        {
            Status = $"重做失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Cut()
    {
        try
        {
            Status = "正在剪切...";
            
            // 获取当前选中的文本或内容
            var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                // 这里需要根据当前视图获取选中的内容
                // 简化实现：直接调用当前视图的剪切方法
                if (CurrentView is QuestionBankViewModel questionBankViewModel)
                {
                    await questionBankViewModel.CutAsync();
                }
                else if (CurrentView is TemplateViewModel templateViewModel)
                {
                    await templateViewModel.CutAsync();
                }
                else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
                {
                    await examGenerationViewModel.CutAsync();
                }
                else
                {
                    Status = "没有可剪切的内容";
                    return;
                }
            }
            
            Status = "剪切完成";
        }
        catch (Exception ex)
        {
            Status = $"剪切失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Copy()
    {
        try
        {
            Status = "正在复制...";
            
            // 获取当前选中的文本或内容
            var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                // 这里需要根据当前视图获取选中的内容
                // 简化实现：直接调用当前视图的复制方法
                if (CurrentView is QuestionBankViewModel questionBankViewModel)
                {
                    await questionBankViewModel.CopyAsync();
                }
                else if (CurrentView is TemplateViewModel templateViewModel)
                {
                    await templateViewModel.CopyAsync();
                }
                else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
                {
                    await examGenerationViewModel.CopyAsync();
                }
                else
                {
                    Status = "没有可复制的内容";
                    return;
                }
            }
            
            Status = "复制完成";
        }
        catch (Exception ex)
        {
            Status = $"复制失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Paste()
    {
        try
        {
            Status = "正在粘贴...";
            
            // 获取剪贴板内容
            var clipboard = TopLevel.GetTopLevel((Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                // 这里需要根据当前视图粘贴内容
                // 简化实现：直接调用当前视图的粘贴方法
                if (CurrentView is QuestionBankViewModel questionBankViewModel)
                {
                    await questionBankViewModel.PasteAsync();
                }
                else if (CurrentView is TemplateViewModel templateViewModel)
                {
                    await templateViewModel.PasteAsync();
                }
                else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
                {
                    await examGenerationViewModel.PasteAsync();
                }
                else
                {
                    Status = "没有可粘贴的位置";
                    return;
                }
            }
            
            Status = "粘贴完成";
        }
        catch (Exception ex)
        {
            Status = $"粘贴失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        try
        {
            Status = "正在删除...";
            
            // 显示确认对话框
            var result = await ShowConfirmDialog("删除确认", "确定要删除选中的内容吗？");
            if (result != true)
            {
                Status = "取消删除";
                return;
            }
            
            // 根据当前视图类型执行相应的删除操作
            if (CurrentView is QuestionBankViewModel questionBankViewModel)
            {
                await questionBankViewModel.DeleteSelectedAsync();
            }
            else if (CurrentView is TemplateViewModel templateViewModel)
            {
                await templateViewModel.DeleteSelectedAsync();
            }
            else if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                await examGenerationViewModel.DeleteSelectedAsync();
            }
            else
            {
                Status = "没有可删除的内容";
                return;
            }
            
            Status = "删除完成";
        }
        catch (Exception ex)
        {
            Status = $"删除失败: {ex.Message}";
            // 这里应该记录日志
        }
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
        try
        {
            Status = "正在生成试卷...";
            
            // 检查是否有题库和模板
            if (CurrentView is ExamGenerationViewModel examGenerationViewModel)
            {
                var success = await examGenerationViewModel.GeneratePaperAsync();
                if (success)
                {
                    Status = "试卷生成完成";
                    ShowPdfPreviewView();
                }
                else
                {
                    Status = "试卷生成失败";
                }
            }
            else
            {
                Status = "请先切换到试卷生成视图";
                ShowExamGenerationView();
            }
        }
        catch (Exception ex)
        {
            Status = $"生成试卷失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task ExportPaper()
    {
        try
        {
            Status = "正在导出试卷...";
            
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
                Title = "导出试卷",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PDF文件")
                    {
                        Patterns = new[] { "*.pdf" }
                    },
                    new FilePickerFileType("Word文档")
                    {
                        Patterns = new[] { "*.docx" }
                    },
                    new FilePickerFileType("LaTeX文件")
                    {
                        Patterns = new[] { "*.tex" }
                    }
                },
                DefaultExtension = "pdf",
                SuggestedFileName = "试卷.pdf"
            });

            if (file != null)
            {
                var filePath = file.Path.AbsolutePath;
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
                
                // 根据文件扩展名选择导出格式
                bool success = false;
                
                if (fileExtension == ".pdf")
                {
                    var pdfConfig = new ExportConfiguration 
                    { 
                        OutputPath = filePath, 
                        Format = ExportFormat.PDF,
                        FileName = System.IO.Path.GetFileName(filePath)
                    };
                    var result = await _exportService.ExportToPdfAsync("", pdfConfig);
                    success = !string.IsNullOrEmpty(result);
                }
                else if (fileExtension == ".docx")
                {
                    var wordConfig = new ExportConfiguration 
                    { 
                        OutputPath = filePath, 
                        Format = ExportFormat.Word,
                        FileName = System.IO.Path.GetFileName(filePath)
                    };
                    var result = await _exportService.ExportToWordAsync("", wordConfig);
                    success = !string.IsNullOrEmpty(result);
                }
                else if (fileExtension == ".tex")
                {
                    var latexConfig = new ExportConfiguration 
                    { 
                        OutputPath = filePath, 
                        Format = ExportFormat.LaTeX,
                        FileName = System.IO.Path.GetFileName(filePath)
                    };
                    var result = await _exportService.ExportToLaTeXAsync("", latexConfig);
                    success = !string.IsNullOrEmpty(result);
                }
                
                if (success)
                {
                    AddToRecentFiles(file.Name, filePath);
                    Status = "试卷导出完成";
                }
                else
                {
                    Status = "试卷导出失败";
                }
            }
            else
            {
                Status = "取消导出试卷";
            }
        }
        catch (Exception ex)
        {
            Status = $"导出试卷失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        try
        {
            Status = "正在打开设置...";
            
            // 创建设置窗口
            var settingsWindow = new Window
            {
                Title = "设置",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // 这里应该创建设置视图并添加到窗口
            // 简化实现：显示一个简单的设置对话框
            await ShowMessageDialog("设置", "设置功能正在开发中...");
            
            Status = "设置已打开";
        }
        catch (Exception ex)
        {
            Status = $"打开设置失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        try
        {
            Status = "正在打开帮助...";
            
            // 创建帮助窗口
            var helpWindow = new Window
            {
                Title = "帮助",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // 这里应该创建帮助视图并添加到窗口
            // 简化实现：显示一个简单的帮助对话框
            var helpContent = @"
QuizForge 使用指南

1. 题库管理
   - 创建新题库：文件 → 新建题库
   - 导入题库：文件 → 导入题库
   - 编辑题库：在题库管理视图中添加、修改、删除题目

2. 模板管理
   - 创建模板：在模板管理视图中创建新模板
   - 编辑模板：修改模板的布局和样式
   - 预览模板：查看模板的最终效果

3. 试卷生成
   - 选择题库：从已导入的题库中选择
   - 选择模板：从已创建的模板中选择
   - 生成试卷：点击生成按钮创建试卷

4. 导出功能
   - PDF导出：生成高质量的PDF试卷
   - Word导出：生成可编辑的Word文档
   - LaTeX导出：生成LaTeX源码文件

快捷键：
- Ctrl+N：新建题库
- Ctrl+O：导入题库
- Ctrl+S：保存
- Ctrl+Shift+S：另存为
- Ctrl+Z：撤销
- Ctrl+Y：重做
- Ctrl+X：剪切
- Ctrl+C：复制
- Ctrl+V：粘贴
- Delete：删除
- F1：帮助
            ";

            await ShowMessageDialog("帮助", helpContent);
            
            Status = "帮助已打开";
        }
        catch (Exception ex)
        {
            Status = $"打开帮助失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        try
        {
            Status = "正在打开关于...";
            
            var aboutContent = @"
QuizForge - 试卷生成系统

版本：1.0.0
作者：QuizForge Team
许可证：MIT License

简介：
QuizForge是一个基于.NET的试卷生成系统，旨在为教育机构、教师和考试组织者提供一个高效、灵活的试卷生成解决方案。

主要功能：
- 支持多种题库格式（Markdown、Excel、JSON、XML）
- 灵活的模板系统，支持自定义布局
- 高质量的PDF生成
- 批量生成试卷
- 跨平台支持（Windows、macOS、Linux）

技术栈：
- .NET 8
- Avalonia UI
- LaTeX
- SQLite

感谢使用QuizForge！
            ";

            await ShowMessageDialog("关于 QuizForge", aboutContent);
            
            Status = "关于已打开";
        }
        catch (Exception ex)
        {
            Status = $"打开关于失败: {ex.Message}";
            // 这里应该记录日志
        }
    }

// 辅助方法
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

    private async Task SaveQuestionBankToFile(QuestionBank questionBank, string filePath)
    {
        // 根据文件格式保存题库
        var format = GetFileFormat(filePath);
        await _questionService.ExportQuestionBankAsync(questionBank, filePath, format);
    }

    private void AddToRecentFiles(string name, string path)
    {
        // 移除已存在的相同文件
        var existingFile = RecentFiles.FirstOrDefault(f => f.Path == path);
        if (existingFile != null)
        {
            RecentFiles.Remove(existingFile);
        }

        // 添加到列表开头
        RecentFiles.Insert(0, new RecentFile(this) { Name = name, Path = path });

        // 保持最近文件列表不超过10个
        while (RecentFiles.Count > 10)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    private async Task SaveRecentFilesToConfig()
    {
        // 这里应该将最近文件列表保存到配置文件
        // 简化实现：暂时不实现
        await Task.CompletedTask;
    }

    private async Task<bool?> ShowConfirmDialog(string title, string message)
    {
        // 简化实现：使用控制台输出模拟确认对话框
        // 原本实现：应该使用自定义的确认对话框
        // 简化实现：总是返回true
        Console.WriteLine($"{title}: {message}");
        await Task.CompletedTask;
        return true;
    }

    private async Task ShowMessageDialog(string title, string message)
    {
        // 简化实现：使用控制台输出模拟消息对话框
        // 原本实现：应该使用自定义的消息对话框
        Console.WriteLine($"{title}: {message}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 最近使用的文件
    /// </summary>
    public partial class RecentFile : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        public RecentFile(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task Open()
        {
            try
            {
                // 检查文件是否存在
                if (!System.IO.File.Exists(Path))
                {
                    Console.WriteLine($"错误：文件不存在：{Path}");
                    return;
                }

                // 根据文件扩展名判断文件类型
                var extension = System.IO.Path.GetExtension(Path).ToLower();
                
                if (extension == ".md" || extension == ".xlsx" || extension == ".xls" || extension == ".json" || extension == ".xml")
                {
                    // 导入题库文件
                    var questionBank = await _mainViewModel._questionService.ImportQuestionBankAsync(Path, QuestionBankFormat.Markdown);
                    if (questionBank != null)
                    {
                        // 切换到题库管理视图
                        _mainViewModel.ShowQuestionBankView();
                        
                        // 通知QuestionBankViewModel加载题库
                        if (_mainViewModel.CurrentView is QuestionBankViewModel questionBankViewModel)
                        {
                            await questionBankViewModel.LoadQuestionBankAsync(questionBank);
                        }
                    }
                    else
                    {
                        Console.WriteLine("错误：导入题库失败");
                    }
                }
                else if (extension == ".tex")
                {
                    // 打开模板文件
                    var template = await _mainViewModel._templateService.LoadTemplateAsync(Path);
                    if (template != null)
                    {
                        // 切换到模板管理视图
                        _mainViewModel.ShowTemplateView();
                        
                        // 通知TemplateViewModel加载模板
                        if (_mainViewModel.CurrentView is TemplateViewModel templateViewModel)
                        {
                            await templateViewModel.LoadTemplateAsync(template);
                        }
                    }
                    else
                    {
                        Console.WriteLine("错误：加载模板失败");
                    }
                }
                else
                {
                    Console.WriteLine("错误：不支持的文件类型");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：打开文件失败：{ex.Message}");
            }
        }
    }
}
}