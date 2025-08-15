using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Services;
using QuizForge.Core.Interfaces;
using QuizForge.Data.Repositories;
using QuizForge.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// QuestionService 单元测试
/// </summary>
public class QuestionServiceTests
{
    private Mock<IQuestionRepository> _mockQuestionRepository;
    private Mock<IQuestionProcessor> _mockQuestionProcessor;
    private Mock<IMarkdownParser> _mockMarkdownParser;
    private Mock<IExcelParser> _mockExcelParser;
    private QuestionService _questionService;
    private List<Question> _testQuestions;

    public QuestionServiceTests()
    {
        Setup();
    }

    private void Setup()
    {
        // 创建模拟对象
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockQuestionProcessor = new Mock<IQuestionProcessor>();
        _mockMarkdownParser = new Mock<IMarkdownParser>();
        _mockExcelParser = new Mock<IExcelParser>();

        // 创建测试数据
        _testQuestions = new List<Question>
        {
            new Question
            {
                Id = Guid.NewGuid(),
                Type = "选择题",
                Content = "测试题目1",
                Category = "数学",
                Difficulty = "简单",
                CorrectAnswer = "A",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "选项A", IsCorrect = true },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "选项B", IsCorrect = false }
                }
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Type = "选择题",
                Content = "测试题目2",
                Category = "数学",
                Difficulty = "中等",
                CorrectAnswer = "B",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "选项A", IsCorrect = false },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "选项B", IsCorrect = true }
                }
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Type = "选择题",
                Content = "测试题目3",
                Category = "物理",
                Difficulty = "简单",
                CorrectAnswer = "A",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "选项A", IsCorrect = true },
                    new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "选项B", IsCorrect = false }
                }
            }
        };

        // 创建服务实例
        _questionService = new QuestionService(
            _mockQuestionRepository.Object,
            _mockQuestionProcessor.Object,
            _mockMarkdownParser.Object,
            _mockExcelParser.Object);
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WithValidCount_ReturnsQuestions()
    {
        // Arrange
        int count = 2;
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ReturnsAsync(_testQuestions);
        
        _mockQuestionProcessor.Setup(processor => processor.GetRandomQuestions(
                count, It.IsAny<QuestionBank>(), null, null))
            .Returns(_testQuestions.Take(count).ToList());

        // Act
        var result = await _questionService.GetRandomQuestionsAsync(count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
        _mockQuestionRepository.Verify(repo => repo.GetAllQuestionsAsync(), Times.Once);
        _mockQuestionProcessor.Verify(processor => processor.GetRandomQuestions(
            count, It.IsAny<QuestionBank>(), null, null), Times.Once);
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WithCategoryFilter_ReturnsFilteredQuestions()
    {
        // Arrange
        int count = 1;
        string category = "数学";
        var filteredQuestions = _testQuestions.Where(q => q.Category == category).ToList();
        
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ReturnsAsync(_testQuestions);
        
        _mockQuestionProcessor.Setup(processor => processor.GetRandomQuestions(
                count, It.IsAny<QuestionBank>(), category, null))
            .Returns(filteredQuestions.Take(count).ToList());

        // Act
        var result = await _questionService.GetRandomQuestionsAsync(count, category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
        _mockQuestionProcessor.Verify(processor => processor.GetRandomQuestions(
            count, It.IsAny<QuestionBank>(), category, null), Times.Once);
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WithDifficultyFilter_ReturnsFilteredQuestions()
    {
        // Arrange
        int count = 1;
        string difficulty = "简单";
        var filteredQuestions = _testQuestions.Where(q => q.Difficulty == difficulty).ToList();
        
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ReturnsAsync(_testQuestions);
        
        _mockQuestionProcessor.Setup(processor => processor.GetRandomQuestions(
                count, It.IsAny<QuestionBank>(), null, difficulty))
            .Returns(filteredQuestions.Take(count).ToList());

        // Act
        var result = await _questionService.GetRandomQuestionsAsync(count, null, difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
        _mockQuestionProcessor.Verify(processor => processor.GetRandomQuestions(
            count, It.IsAny<QuestionBank>(), null, difficulty), Times.Once);
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WithBothFilters_ReturnsFilteredQuestions()
    {
        // Arrange
        int count = 1;
        string category = "数学";
        string difficulty = "简单";
        var filteredQuestions = _testQuestions
            .Where(q => q.Category == category && q.Difficulty == difficulty)
            .ToList();
        
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ReturnsAsync(_testQuestions);
        
        _mockQuestionProcessor.Setup(processor => processor.GetRandomQuestions(
                count, It.IsAny<QuestionBank>(), category, difficulty))
            .Returns(filteredQuestions.Take(count).ToList());

        // Act
        var result = await _questionService.GetRandomQuestionsAsync(count, category, difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
        _mockQuestionProcessor.Verify(processor => processor.GetRandomQuestions(
            count, It.IsAny<QuestionBank>(), category, difficulty), Times.Once);
    }

    [Fact]
    public void GetRandomQuestionsAsync_WithInvalidCount_ThrowsException()
    {
        // Arrange
        int count = 0;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            _questionService.GetRandomQuestionsAsync(count));
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WithNoQuestions_ReturnsEmptyList()
    {
        // Arrange
        int count = 1;
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ReturnsAsync(new List<Question>());

        // Act
        var result = await _questionService.GetRandomQuestionsAsync(count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
        _mockQuestionRepository.Verify(repo => repo.GetAllQuestionsAsync(), Times.Once);
        _mockQuestionProcessor.Verify(processor => processor.GetRandomQuestions(
            It.IsAny<int>(), It.IsAny<QuestionBank>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetRandomQuestionsAsync_WhenRepositoryThrows_ThrowsException()
    {
        // Arrange
        int count = 1;
        _mockQuestionRepository.Setup(repo => repo.GetAllQuestionsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => 
            _questionService.GetRandomQuestionsAsync(count));
        Assert.Contains("随机获取题目失败", ex.Message);
    }

    [Fact]
    public async Task ImportQuestionBankAsync_WithMarkdownFormat_CallsMarkdownParser()
    {
        // Arrange
        string filePath = "test.md";
        var format = QuestionBankFormat.Markdown;
        var questionBank = new QuestionBank { Id = Guid.NewGuid(), Name = "Test Bank" };
        
        _mockMarkdownParser.Setup(parser => parser.ParseAsync(filePath))
            .ReturnsAsync(questionBank);
        _mockQuestionProcessor.Setup(processor => processor.ProcessQuestionBank(questionBank))
            .Returns(questionBank);
        _mockQuestionRepository.Setup(repo => repo.AddAsync(questionBank))
            .ReturnsAsync(questionBank);

        // Act
        var result = await _questionService.ImportQuestionBankAsync(filePath, format);

        // Assert
        Assert.NotNull(result);
        _mockMarkdownParser.Verify(parser => parser.ParseAsync(filePath), Times.Once);
        _mockQuestionProcessor.Verify(processor => processor.ProcessQuestionBank(questionBank), Times.Once);
        _mockQuestionRepository.Verify(repo => repo.AddAsync(questionBank), Times.Once);
    }

    [Fact]
    public async Task ImportQuestionBankAsync_WithExcelFormat_CallsExcelParser()
    {
        // Arrange
        string filePath = "test.xlsx";
        var format = QuestionBankFormat.Excel;
        var questionBank = new QuestionBank { Id = Guid.NewGuid(), Name = "Test Bank" };
        
        _mockExcelParser.Setup(parser => parser.ParseAsync(filePath))
            .ReturnsAsync(questionBank);
        _mockQuestionProcessor.Setup(processor => processor.ProcessQuestionBank(questionBank))
            .Returns(questionBank);
        _mockQuestionRepository.Setup(repo => repo.AddAsync(questionBank))
            .ReturnsAsync(questionBank);

        // Act
        var result = await _questionService.ImportQuestionBankAsync(filePath, format);

        // Assert
        Assert.NotNull(result);
        _mockExcelParser.Verify(parser => parser.ParseAsync(filePath), Times.Once);
        _mockQuestionProcessor.Verify(processor => processor.ProcessQuestionBank(questionBank), Times.Once);
        _mockQuestionRepository.Verify(repo => repo.AddAsync(questionBank), Times.Once);
    }

    [Fact]
    public void ImportQuestionBankAsync_WithUnsupportedFormat_ThrowsException()
    {
        // Arrange
        string filePath = "test.json";
        var format = (QuestionBankFormat)99; // 不支持的格式

        // Act & Assert
        Assert.ThrowsAsync<NotSupportedException>(() => 
            _questionService.ImportQuestionBankAsync(filePath, format));
    }
}