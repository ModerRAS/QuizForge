using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Collections.ObjectModel;
using System;

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

    public TemplateViewModel(
        ITemplateService templateService,
        IFileService fileService)
    {
        _templateService = templateService;
        _fileService = fileService;

        // 初始化数据
        InitializeData();
    }

    private void InitializeData()
    {
        // TODO: 从数据库加载模板列表
        Templates.Add(new ExamTemplate 
        { 
            Id = Guid.NewGuid(), 
            Name = "标准试卷模板", 
            Description = "适用于常规考试的标准试卷模板",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        Templates.Add(new ExamTemplate 
        { 
            Id = Guid.NewGuid(), 
            Name = "竞赛试卷模板", 
            Description = "适用于学科竞赛的试卷模板",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        Templates.Add(new ExamTemplate 
        { 
            Id = Guid.NewGuid(), 
            Name = "单元测试模板", 
            Description = "适用于单元测试的简化试卷模板",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        
        if (Templates.Count > 0)
        {
            SelectedTemplate = Templates[0];
        }
    }

    partial void OnSelectedTemplateChanged(ExamTemplate? value)
    {
        if (value != null)
        {
            LoadSections(value.Id);
            TemplateName = value.Name;
            TemplateDescription = value.Description;
        }
        else
        {
            Sections.Clear();
            TemplateName = string.Empty;
            TemplateDescription = string.Empty;
        }
    }

    private void LoadSections(Guid templateId)
    {
        // TODO: 从数据库加载模板节列表
        Sections.Clear();
        
        // 模拟数据
        Sections.Add(new TemplateSection
        {
            Id = Guid.NewGuid(),
            Title = "选择题",
            Instructions = "单选题和多选题",
            QuestionCount = 20,
            TotalPoints = 40,
            QuestionIds = new System.Collections.Generic.List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        });
        Sections.Add(new TemplateSection
        {
            Id = Guid.NewGuid(),
            Title = "填空题",
            Instructions = "填空题部分",
            QuestionCount = 10,
            TotalPoints = 20,
            QuestionIds = new System.Collections.Generic.List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        });
        Sections.Add(new TemplateSection
        {
            Id = Guid.NewGuid(),
            Title = "解答题",
            Instructions = "解答题部分",
            QuestionCount = 5,
            TotalPoints = 40,
            QuestionIds = new System.Collections.Generic.List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        });
        
        Status = $"已加载 {Sections.Count} 个模板节";
    }

    [RelayCommand]
    private void NewTemplate()
    {
        TemplateName = "新模板";
        TemplateDescription = "新模板的描述";
        IsEditing = true;
        Status = "正在创建新模板...";
    }

    [RelayCommand]
    private void SaveTemplate()
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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            Templates.Add(newTemplate);
            SelectedTemplate = newTemplate;
            Status = "模板已创建";
        }
        else
        {
            // 更新现有模板
            SelectedTemplate.Name = TemplateName;
            SelectedTemplate.Description = TemplateDescription;
            SelectedTemplate.UpdatedAt = DateTime.Now;
            Status = "模板已更新";
        }

        IsEditing = false;
    }

    [RelayCommand]
    private void DeleteTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择要删除的模板";
            return;
        }

        Templates.Remove(SelectedTemplate);
        SelectedTemplate = null;
        Status = "模板已删除";
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (SelectedTemplate != null)
        {
            TemplateName = SelectedTemplate.Name;
            TemplateDescription = SelectedTemplate.Description;
        }
        else
        {
            TemplateName = string.Empty;
            TemplateDescription = string.Empty;
        }

        IsEditing = false;
        Status = "已取消编辑";
    }

    [RelayCommand]
    private void AddSection()
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
        IsEditing = true;
        Status = "正在添加新节...";
    }

    [RelayCommand]
    private void SaveSection()
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
                Title = SectionName,
                Instructions = SectionDescription,
                QuestionCount = QuestionCount,
                TotalPoints = QuestionCount * 2, // 假设每题2分
                QuestionIds = new System.Collections.Generic.List<Guid>()
            };

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
            SelectedSection.TotalPoints = QuestionCount * 2; // 假设每题2分
            Status = "节已更新";
        }

        IsEditing = false;
    }

    [RelayCommand]
    private void DeleteSection()
    {
        if (SelectedSection == null)
        {
            Status = "请先选择要删除的节";
            return;
        }

        Sections.Remove(SelectedSection);
        SelectedSection = null;
        Status = "节已删除";
    }

    [RelayCommand]
    private void SearchTemplates()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            InitializeData();
            return;
        }

        // TODO: 实现搜索逻辑
        Status = $"搜索包含 \"{SearchText}\" 的模板...";
    }

    [RelayCommand]
    private void PreviewTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择一个模板";
            return;
        }

        // TODO: 打开模板预览窗口
        Status = $"正在预览模板: {SelectedTemplate.Name}";
    }

    [RelayCommand]
    private void ExportTemplate()
    {
        if (SelectedTemplate == null)
        {
            Status = "请先选择一个模板";
            return;
        }

        // TODO: 实现导出逻辑
        Status = $"正在导出模板: {SelectedTemplate.Name}";
    }

    [RelayCommand]
    private void ImportTemplate()
    {
        // TODO: 实现导入逻辑
        Status = "正在导入模板...";
    }
}
}