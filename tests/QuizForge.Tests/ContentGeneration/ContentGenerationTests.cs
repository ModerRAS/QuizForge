using QuizForge.Models;
using QuizForge.Core.ContentGeneration;
using QuizForge.Core.Interfaces;
using QuizForge.Models.Interfaces;
using Moq;
using Xunit;

namespace QuizForge.Tests.ContentGeneration;

/// <summary>
/// 内容生成测试类
/// </summary>
public class ContentGenerationTests
{
    private readonly Mock<IQuestionProcessor> _mockQuestionProcessor;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ITemplateRepository> _mockTemplateRepository;
    private readonly ContentGenerator _contentGenerator;
    private readonly DynamicContentInserter _dynamicContentInserter;
    private readonly ExamPaperGenerator _examPaperGenerator;

    public ContentGenerationTests()
    {
        _mockQuestionProcessor = new Mock<IQuestionProcessor>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockTemplateRepository = new Mock<ITemplateRepository>();
        _contentGenerator = new ContentGenerator();
        _dynamicContentInserter = new DynamicContentInserter();
        _examPaperGenerator = new ExamPaperGenerator(
            _mockQuestionProcessor.Object,
            _mockQuestionRepository.Object,
            _mockTemplateRepository.Object,
            _contentGenerator,
            _dynamicContentInserter);
    }

    /// <summary>
    /// 测试生成选择题内容
    /// </summary>
    [Fact]
    public void GenerateMultipleChoiceQuestion_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var question = new Question
        {
            Type = "选择题",
            Content = "以下哪个是正确的？",
            Points = 5,
            Options = new List<QuestionOption>
            {
                new QuestionOption { Key = "A", Value = "选项A", IsCorrect = false },
                new QuestionOption { Key = "B", Value = "选项B", IsCorrect = true },
                new QuestionOption { Key = "C", Value = "选项C", IsCorrect = false },
                new QuestionOption { Key = "D", Value = "选项D", IsCorrect = false }
            }
        };

        // Act
        var result = _contentGenerator.GenerateMultipleChoiceQuestion(question, 1);

        // Assert
        Assert.Contains("\\textbf{题目 1} (5分)", result);
        Assert.Contains("以下哪个是正确的？", result);
        Assert.Contains("\\begin{enumerate}", result);
        Assert.Contains("\\item 选项A", result);
        Assert.Contains("\\item 选项B", result);
        Assert.Contains("\\item 选项C", result);
        Assert.Contains("\\item 选项D", result);
        Assert.Contains("\\end{enumerate}", result);
    }

    /// <summary>
    /// 测试生成填空题内容
    /// </summary>
    [Fact]
    public void GenerateFillInBlankQuestion_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var question = new Question
        {
            Type = "填空题",
            Content = "中国的首都是___。",
            Points = 3
        };

        // Act
        var result = _contentGenerator.GenerateFillInBlankQuestion(question, 1);

        // Assert
        Assert.Contains("\\textbf{题目 1} (3分)", result);
        Assert.Contains("中国的首都是\\underline{\\hspace{3cm}}。", result);
    }

    /// <summary>
    /// 测试生成简答题内容
    /// </summary>
    [Fact]
    public void GenerateEssayQuestion_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var question = new Question
        {
            Type = "简答题",
            Content = "请简述中国的历史。",
            Points = 10
        };

        // Act
        var result = _contentGenerator.GenerateEssayQuestion(question, 1);

        // Assert
        Assert.Contains("\\textbf{题目 1} (10分)", result);
        Assert.Contains("请简述中国的历史。", result);
        Assert.Contains("\\vspace{5cm}", result);
    }

    /// <summary>
    /// 测试生成判断题内容
    /// </summary>
    [Fact]
    public void GenerateTrueFalseQuestion_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var question = new Question
        {
            Type = "判断题",
            Content = "中国的首都是北京。",
            Points = 2
        };

        // Act
        var result = _contentGenerator.GenerateTrueFalseQuestion(question, 1);

        // Assert
        Assert.Contains("\\textbf{题目 1} (2分)", result);
        Assert.Contains("中国的首都是北京。", result);
        Assert.Contains("\\begin{enumerate}", result);
        Assert.Contains("\\item 正确", result);
        Assert.Contains("\\item 错误", result);
        Assert.Contains("\\end{enumerate}", result);
    }

    /// <summary>
    /// 测试生成答题卡内容
    /// </summary>
    [Fact]
    public void GenerateAnswerSheetContent_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var question = new Question
        {
            Type = "选择题",
            Content = "以下哪个是正确的？",
            Points = 5
        };

        // Act
        var result = _contentGenerator.GenerateAnswerSheetContent(question, 1);

        // Assert
        Assert.Contains("\\textbf{题目 1} \\underline{\\hspace{2cm}} (5分)", result);
    }

    /// <summary>
    /// 测试生成章节内容
    /// </summary>
    [Fact]
    public void GenerateSectionContent_ShouldReturnCorrectLaTeX()
    {
        // Arrange
        var section = new TemplateSection
        {
            Title = "选择题部分",
            Instructions = "请选择正确的答案。"
        };

        // Act
        var result = _contentGenerator.GenerateSectionContent(section);

        // Assert
        Assert.Contains("\\section*{选择题部分}", result);
        Assert.Contains("\\textbf{请选择正确的答案。}", result);
    }

    /// <summary>
    /// 测试插入动态内容
    /// </summary>
    [Fact]
    public void InsertDynamicContent_ShouldReplacePlaceholders()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试科目",
            HeaderContent = "测试页眉",
            FooterContent = "测试页脚"
        };

        var questions = new List<Question>
        {
            new Question
            {
                Type = "选择题",
                Content = "以下哪个是正确的？",
                Points = 5,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Key = "A", Value = "选项A", IsCorrect = false },
                    new QuestionOption { Key = "B", Value = "选项B", IsCorrect = true }
                }
            }
        };

        var templateContent = @"
\documentclass{article}
\begin{document}
{EXAM_TITLE}
{SUBJECT}
{TOTAL_POINTS}
{HEADER_CONTENT}
{FOOTER_CONTENT}
{CONTENT}
{ANSWER_SHEET_CONTENT}
\end{document}";

        // Act
        var result = _dynamicContentInserter.InsertDynamicContent(templateContent, template, questions);

        // Assert
        Assert.Contains("测试试卷", result);
        Assert.Contains("测试科目", result);
        Assert.Contains("5", result);
        Assert.Contains("测试页眉", result);
        Assert.Contains("测试页脚", result);
        Assert.Contains("\\textbf{题目 1} (5分)", result);
        Assert.Contains("以下哪个是正确的？", result);
        Assert.Contains("\\textbf{题目 1} \\underline{\\hspace{2cm}} (5分)", result);
    }

    /// <summary>
    /// 测试生成试卷
    /// </summary>
    [Fact]
    public async Task GenerateExamPaperAsync_ShouldReturnValidExamPaper()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var questionBankId = Guid.NewGuid();
        var options = new ExamPaperOptions
        {
            Title = "测试试卷",
            QuestionCount = 2,
            RandomQuestions = false
        };

        var template = new ExamTemplate
        {
            Id = templateId,
            Name = "基础模板",
            Description = "基础模板描述",
            Style = TemplateStyle.Basic,
            Sections = new List<TemplateSection>
            {
                new TemplateSection
                {
                    Title = "第一部分",
                    QuestionCount = 2,
                    Instructions = "请回答以下问题。"
                }
            }
        };

        var questionBank = new QuestionBank
        {
            Id = questionBankId,
            Name = "测试题库",
            Questions = new List<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    Type = "选择题",
                    Content = "问题1",
                    Points = 5,
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { Key = "A", Value = "选项A", IsCorrect = false },
                        new QuestionOption { Key = "B", Value = "选项B", IsCorrect = true }
                    }
                },
                new Question
                {
                    Id = Guid.NewGuid(),
                    Type = "填空题",
                    Content = "问题2",
                    Points = 3
                }
            }
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionBankId)).ReturnsAsync(questionBank);
        _mockQuestionProcessor.Setup(p => p.ProcessQuestionBank(questionBank)).Returns(questionBank);
        _mockQuestionProcessor.Setup(p => p.GetRandomQuestions(2, questionBank, null, null))
            .Returns(questionBank.Questions);

        // Act
        var result = await _examPaperGenerator.GenerateExamPaperAsync(templateId, questionBankId, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("测试试卷", result.Title);
        Assert.Equal(templateId, result.TemplateId);
        Assert.Equal(questionBankId, result.QuestionBankId);
        Assert.Equal(2, result.Questions.Count);
        Assert.Equal(8, result.TotalPoints);
        Assert.Contains("\\textbf{题目 1} (5分)", result.Content);
        Assert.Contains("\\textbf{题目 2} (3分)", result.Content);
    }

    /// <summary>
    /// 测试生成多页试卷
    /// </summary>
    [Fact]
    public async Task GenerateMultiPageLaTeXContentAsync_ShouldIncludePageBreaks()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试科目",
            Style = TemplateStyle.Basic
        };

        var questions = new List<Question>();
        for (int i = 1; i <= 6; i++)
        {
            questions.Add(new Question
            {
                Id = Guid.NewGuid(),
                Type = "选择题",
                Content = $"问题{i}",
                Points = 5,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Key = "A", Value = "选项A", IsCorrect = false },
                    new QuestionOption { Key = "B", Value = "选项B", IsCorrect = true }
                }
            });
        }

        // Act
        var result = await _examPaperGenerator.GenerateMultiPageLaTeXContentAsync(template, questions, 3);

        // Assert
        Assert.Contains("\\newpage", result);
        Assert.Contains("\\textbf{题目 1} (5分)", result);
        Assert.Contains("\\textbf{题目 4} (5分)", result);
    }

    /// <summary>
    /// 测试LaTeX特殊字符转义
    /// </summary>
    [Fact]
    public void EscapeLaTeX_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var text = "这是一个包含特殊字符的文本：$ % & # _ { } ~ ^ \\ < >";

        // Act
        var result = _contentGenerator.GetType()
            .GetMethod("EscapeLaTeX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_contentGenerator, new object[] { text }) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\\textbackslash ", result);
        Assert.Contains("\\$", result);
        Assert.Contains("\\%", result);
        Assert.Contains("\\&", result);
        Assert.Contains("\\#", result);
        Assert.Contains("\\_", result);
        Assert.Contains("\\{", result);
        Assert.Contains("\\}", result);
        Assert.Contains("\\textasciitilde ", result);
        Assert.Contains("\\textasciicircum ", result);
        Assert.Contains("$<$", result);
        Assert.Contains("$>$", result);
    }
}