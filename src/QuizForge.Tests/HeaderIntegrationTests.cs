using QuizForge.Core.ContentGeneration;
using QuizForge.Core.Layout;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using Xunit;
using Moq;

namespace QuizForge.Tests;

/// <summary>
/// 抬头功能与现有系统集成的测试
/// </summary>
public class HeaderIntegrationTests
{
    private readonly Mock<IQuestionProcessor> _mockQuestionProcessor;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ITemplateRepository> _mockTemplateRepository;
    private readonly ContentGenerator _contentGenerator;
    private readonly DynamicContentInserter _dynamicContentInserter;

    public HeaderIntegrationTests()
    {
        _mockQuestionProcessor = new Mock<IQuestionProcessor>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockTemplateRepository = new Mock<ITemplateRepository>();
        _contentGenerator = new ContentGenerator();
        _dynamicContentInserter = new DynamicContentInserter();
    }

    [Fact]
    public async Task GenerateExamPaperAsync_WithHeaderConfig_ShouldIncludeHeaderInOutput()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var questionBankId = Guid.NewGuid();
        var options = new ExamPaperOptions
        {
            Title = "集成测试试卷",
            QuestionCount = 5
        };

        var template = new ExamTemplate
        {
            Id = templateId,
            Name = "测试模板",
            Description = "测试科目",
            Style = TemplateStyle.Basic,
            HeaderConfig = new HeaderConfig
            {
                Style = HeaderStyle.Standard,
                ExamTitle = "集成测试试卷",
                Subject = "测试科目",
                ExamTime = 90,
                TotalPoints = 100,
                ShowStudentInfo = true,
                StudentInfo = new StudentInfoConfig
                {
                    ShowName = true,
                    ShowStudentId = true,
                    ShowClass = true,
                    ShowDate = true,
                    Layout = StudentInfoLayout.Horizontal
                }
            }
        };

        var questionBank = new QuestionBank
        {
            Id = questionBankId,
            Name = "测试题库",
            Questions = new List<Question>
            {
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "测试题目1", Points = 20 },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "测试题目2", Points = 20 },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "测试题目3", Points = 20 },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "测试题目4", Points = 20 },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "测试题目5", Points = 20 }
            }
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionBankId)).ReturnsAsync(questionBank);
        _mockQuestionProcessor.Setup(p => p.ProcessQuestionBank(questionBank)).Returns(questionBank);
        _mockQuestionProcessor.Setup(p => p.GetRandomQuestions(5, questionBank)).Returns(questionBank.Questions);

        var examPaperGenerator = new ExamPaperGenerator(
            _mockQuestionProcessor.Object,
            _mockQuestionRepository.Object,
            _mockTemplateRepository.Object,
            _contentGenerator,
            _dynamicContentInserter
        );

        // Act
        var examPaper = await examPaperGenerator.GenerateExamPaperAsync(templateId, questionBankId, options);

        // Assert
        Assert.NotNull(examPaper);
        Assert.NotEmpty(examPaper.Content);
        Assert.Contains("\\textbf{集成测试试卷}", examPaper.Content);
        Assert.Contains("考试科目：测试科目", examPaper.Content);
        Assert.Contains("考试时间：90分钟", examPaper.Content);
        Assert.Contains("总分：100分", examPaper.Content);
        Assert.Contains("姓名", examPaper.Content);
        Assert.Contains("学号", examPaper.Content);
    }

    [Fact]
    public async Task GenerateExamPaperAsync_WithoutHeaderConfig_ShouldUseDefaultHeader()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var questionBankId = Guid.NewGuid();
        var options = new ExamPaperOptions
        {
            Title = "默认抬头测试",
            QuestionCount = 3
        };

        var template = new ExamTemplate
        {
            Id = templateId,
            Name = "默认抬头模板",
            Description = "默认科目",
            Style = TemplateStyle.Basic,
            HeaderConfig = null // 不设置HeaderConfig，测试默认行为
        };

        var questionBank = new QuestionBank
        {
            Id = questionBankId,
            Name = "测试题库",
            Questions = new List<Question>
            {
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "题目1", Points = 33.33m },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "题目2", Points = 33.33m },
                new Question { Id = Guid.NewGuid(), Type = QuestionType.SingleChoice, Content = "题目3", Points = 33.34m }
            }
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionBankId)).ReturnsAsync(questionBank);
        _mockQuestionProcessor.Setup(p => p.ProcessQuestionBank(questionBank)).Returns(questionBank);
        _mockQuestionProcessor.Setup(p => p.GetRandomQuestions(3, questionBank)).Returns(questionBank.Questions);

        var examPaperGenerator = new ExamPaperGenerator(
            _mockQuestionProcessor.Object,
            _mockQuestionRepository.Object,
            _mockTemplateRepository.Object,
            _contentGenerator,
            _dynamicContentInserter
        );

        // Act
        var examPaper = await examPaperGenerator.GenerateExamPaperAsync(templateId, questionBankId, options);

        // Assert
        Assert.NotNull(examPaper);
        Assert.NotEmpty(examPaper.Content);
        // 即使没有设置HeaderConfig，也应该生成默认的抬头
        Assert.Contains("\\textbf{默认抬头测试}", examPaper.Content);
        Assert.Contains("考试科目：默认科目", examPaper.Content);
    }

    [Fact]
    public async Task GenerateLaTeXContentAsync_WithDetailedHeader_ShouldIncludeDetailedInfo()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Id = Guid.NewGuid(),
            Name = "详细抬头测试",
            Description = "详细科目",
            Style = TemplateStyle.Advanced,
            HeaderConfig = new HeaderConfig
            {
                Style = HeaderStyle.Detailed,
                ExamTitle = "详细抬头测试",
                Subject = "详细科目",
                ExamTime = 120,
                TotalPoints = 150,
                ExamLocation = "测试教学楼B201",
                SchoolName = "测试大学",
                ExamDate = DateTime.Now.ToString("yyyy-MM-dd"),
                ShowStudentInfo = true,
                StudentInfo = new StudentInfoConfig
                {
                    ShowName = true,
                    ShowStudentId = true,
                    ShowClass = true,
                    ShowDate = true,
                    ShowSchool = true,
                    ShowSubject = true,
                    Layout = StudentInfoLayout.Vertical
                }
            },
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Title = "第一部分",
                    QuestionCount = 2,
                    QuestionIds = new List<Guid>
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                }
            }
        };

        var questions = new List<Question>
        {
            new Question { Id = template.Sections[0].QuestionIds[0], Type = QuestionType.SingleChoice, Content = "详细题目1", Points = 75 },
            new Question { Id = template.Sections[0].QuestionIds[1], Type = QuestionType.SingleChoice, Content = "详细题目2", Points = 75 }
        };

        // Act
        var latexContent = await _dynamicContentInserter.InsertDynamicContent(
            await File.ReadAllTextAsync("src/QuizForge.Infrastructure/Templates/AdvancedExamTemplate.tex"),
            template,
            questions
        );

        // Assert
        Assert.NotNull(latexContent);
        Assert.Contains("\\textbf{详细抬头测试}", latexContent);
        Assert.Contains("考试地点：测试教学楼B201", latexContent);
        Assert.Contains("测试大学", latexContent);
        Assert.Contains("姓名", latexContent);
        Assert.Contains("学号", latexContent);
        Assert.Contains("班级", latexContent);
        Assert.Contains("日期", latexContent);
    }

    [Fact]
    public async Task GenerateMultiPageLaTeXContentAsync_WithHeaderConfig_ShouldShowHeaderOnlyOnFirstPage()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Id = Guid.NewGuid(),
            Name = "多页抬头测试",
            Description = "多页科目",
            Style = TemplateStyle.Basic,
            HeaderConfig = new HeaderConfig
            {
                Style = HeaderStyle.Standard,
                ExamTitle = "多页抬头测试",
                Subject = "多页科目",
                ExamTime = 180,
                TotalPoints = 200,
                ShowOnFirstPageOnly = true
            },
            Sections = new List<ExamSection>
            {
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Title = "第一部分",
                    QuestionCount = 5,
                    QuestionIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList()
                },
                new ExamSection
                {
                    Id = Guid.NewGuid(),
                    Title = "第二部分",
                    QuestionCount = 5,
                    QuestionIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList()
                }
            }
        };

        var questions = template.Sections
            .SelectMany(section => section.QuestionIds)
            .Select((id, index) => new Question
            {
                Id = id,
                Type = QuestionType.SingleChoice,
                Content = $"多页题目{index + 1}",
                Points = 20
            })
            .ToList();

        // Act
        var latexContent = await _dynamicContentInserter.InsertMultiPageDynamicContent(
            await File.ReadAllTextAsync("src/QuizForge.Infrastructure/Templates/BasicExamTemplate.tex"),
            template,
            questions,
            5 // 每页5个题目，总共2页
        );

        // Assert
        Assert.NotNull(latexContent);
        // 检查抬头内容只在第一页出现
        var headerCount = latexContent.Split("\\textbf{多页抬头测试}").Length - 1;
        Assert.Equal(1, headerCount);
        
        // 检查是否有分页符
        Assert.Contains("\\newpage", latexContent);
    }
}