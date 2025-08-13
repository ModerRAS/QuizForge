using QuizForge.Core.Services;
using QuizForge.Core.Layout;
using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// 模板处理器测试类
/// </summary>
public class TemplateProcessorTests
{
    private readonly TemplateProcessor _templateProcessor;

    public TemplateProcessorTests()
    {
        _templateProcessor = new TemplateProcessor(
            new SealLineLayout(),
            new HeaderLayout(),
            new HeaderFooterLayout());
    }

    [Fact]
    public void ProcessTemplate_ShouldSetTimestamps()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Id = Guid.NewGuid(),
            Name = "测试模板",
            Description = "测试描述",
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Id = Guid.NewGuid(),
                    Title = "测试章节",
                    QuestionCount = 5,
                    TotalPoints = 50,
                    QuestionIds = new List<Guid>
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                }
            }
        };

        // Act
        var result = _templateProcessor.ProcessTemplate(template);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.UpdatedAt);
        Assert.Equal(template.Name, result.Name);
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnFalseForNullTemplate()
    {
        // Arrange
        ExamTemplate? template = null;

        // Act
        var result = _templateProcessor.ValidateTemplate(template);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnFalseForEmptyName()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "",
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "测试章节",
                    QuestionCount = 5
                }
            }
        };

        // Act
        var result = _templateProcessor.ValidateTemplate(template);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnFalseForNoSections()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试模板",
            Sections = new List<TemplateSection>()
        };

        // Act
        var result = _templateProcessor.ValidateTemplate(template);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnFalseForInvalidSection()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试模板",
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "",
                    QuestionCount = 0
                }
            }
        };

        // Act
        var result = _templateProcessor.ValidateTemplate(template);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnTrueForValidTemplate()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试模板",
            Description = "测试描述",
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "测试章节",
                    QuestionCount = 5,
                    TotalPoints = 50,
                    QuestionIds = new List<Guid>
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                }
            }
        };

        // Act
        var result = _templateProcessor.ValidateTemplate(template);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenerateTemplateContent_ShouldThrowExceptionForNullTemplate()
    {
        // Arrange
        ExamTemplate? template = null;
        var questions = new List<Question>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _templateProcessor.GenerateTemplateContent(template, questions));
    }

    [Fact]
    public void GenerateTemplateContent_ShouldThrowExceptionForNullQuestions()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试模板",
            Style = TemplateStyle.Basic,
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "测试章节",
                    QuestionCount = 2,
                    QuestionIds = new List<Guid>
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                }
            }
        };
        List<Question>? questions = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _templateProcessor.GenerateTemplateContent(template, questions));
    }

    [Fact]
    public void GenerateTemplateContent_ShouldGenerateValidContent()
    {
        // Arrange
        var questionId1 = Guid.NewGuid();
        var questionId2 = Guid.NewGuid();
        
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            Style = TemplateStyle.Basic,
            HeaderContent = "测试页眉",
            FooterContent = "测试页脚",
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "选择题",
                    Instructions = "请选择正确答案",
                    QuestionCount = 2,
                    QuestionIds = new List<Guid> { questionId1, questionId2 }
                }
            }
        };

        var questions = new List<Question>
        {
            new Question
            {
                Id = questionId1,
                Type = "选择题",
                Content = "1 + 1 = ?",
                Difficulty = "简单",
                Category = "数学",
                Points = 5,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Key = "A", Value = "1" },
                    new QuestionOption { Key = "B", Value = "2" },
                    new QuestionOption { Key = "C", Value = "3" },
                    new QuestionOption { Key = "D", Value = "4" }
                }
            },
            new Question
            {
                Id = questionId2,
                Type = "选择题",
                Content = "2 + 2 = ?",
                Difficulty = "简单",
                Category = "数学",
                Points = 5,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Key = "A", Value = "2" },
                    new QuestionOption { Key = "B", Value = "3" },
                    new QuestionOption { Key = "C", Value = "4" },
                    new QuestionOption { Key = "D", Value = "5" }
                }
            }
        };

        // Act
        var result = _templateProcessor.GenerateTemplateContent(template, questions);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("测试试卷", result);
        Assert.Contains("数学测试", result);
        Assert.Contains("选择题", result);
        Assert.Contains("1 + 1 = ?", result);
        Assert.Contains("2 + 2 = ?", result);
        Assert.Contains("A. 1", result);
        Assert.Contains("B. 2", result);
        Assert.Contains("C. 3", result);
        Assert.Contains("D. 4", result);
        Assert.Contains("测试页眉", result);
        Assert.Contains("测试页脚", result);
    }

    [Fact]
    public void EscapeLaTeX_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var text = "This is a test with #special $characters &symbols _here^";

        // Act
        var result = _templateProcessor.GetType()
            .GetMethod("EscapeLaTeX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(_templateProcessor, new object[] { text }) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\\#", result);
        Assert.Contains("\\$", result);
        Assert.Contains("\\&", result);
        Assert.Contains("\\_", result);
        Assert.Contains("\\textasciicircum ", result);
    }
}