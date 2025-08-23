using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;
using QuizForge.Models;

namespace QuizForge.CLI.Tests.Unit.Services;

[TestClass]
public class CliValidationServiceTests : TestBase
{
    private ICliValidationService _validationService = null!;
    private Mock<ILogger<CliValidationService>> _mockLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CliValidationService>>();
        _validationService = new CliValidationService(_mockLogger.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public void ValidateFilePath_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("valid.xlsx");

        // Act
        var result = _validationService.ValidateFilePath(testFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateFilePath_EmptyPath_ReturnsFailure()
    {
        // Arrange
        var emptyPath = "";

        // Act
        var result = _validationService.ValidateFilePath(emptyPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("文件路径不能为空");
    }

    [TestMethod]
    public void ValidateFilePath_NonExistingFile_ReturnsFailure()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");

        // Act
        var result = _validationService.ValidateFilePath(nonExistentFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("不存在");
    }

    [TestMethod]
    public void ValidateFilePath_PathTooLong_ReturnsFailure()
    {
        // Arrange
        var longPath = Path.Combine(TestTempPath, new string('a', 300) + ".xlsx");

        // Act
        var result = _validationService.ValidateFilePath(longPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("路径过长");
    }

    [TestMethod]
    public void ValidateFilePath_NoExtension_ReturnsFailure()
    {
        // Arrange
        var noExtensionFile = CreateTestExcelFile("noextension");
        var noExtensionPath = Path.Combine(Path.GetDirectoryName(noExtensionFile)!, Path.GetFileNameWithoutExtension(noExtensionFile));

        // Act
        var result = _validationService.ValidateFilePath(noExtensionPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("必须包含扩展名");
    }

    [TestMethod]
    public void ValidateOutputPath_ValidPath_ReturnsSuccess()
    {
        // Arrange
        var outputPath = Path.Combine(TestTempPath, "output.pdf");

        // Act
        var result = _validationService.ValidateOutputPath(outputPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateOutputPath_EmptyPath_ReturnsFailure()
    {
        // Arrange
        var emptyPath = "";

        // Act
        var result = _validationService.ValidateOutputPath(emptyPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("输出路径不能为空");
    }

    [TestMethod]
    public void ValidateOutputPath_CreatesDirectoryIfNeeded_ReturnsSuccess()
    {
        // Arrange
        var outputPath = Path.Combine(TestTempPath, "newdir", "output.pdf");
        var dir = Path.GetDirectoryName(outputPath);
        Directory.Exists(dir).Should().BeFalse();

        // Act
        var result = _validationService.ValidateOutputPath(outputPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        Directory.Exists(dir).Should().BeTrue();
    }

    [TestMethod]
    public void ValidateQuestionBankFormat_ValidFormat_ReturnsSuccess()
    {
        // Arrange
        var validFormats = new[] { "excel", "markdown", "xlsx", "md", "tex" };

        foreach (var format in validFormats)
        {
            // Act
            var result = _validationService.ValidateQuestionBankFormat(format);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }
    }

    [TestMethod]
    public void ValidateQuestionBankFormat_InvalidFormat_ReturnsFailure()
    {
        // Arrange
        var invalidFormat = "invalid";

        // Act
        var result = _validationService.ValidateQuestionBankFormat(invalidFormat);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("不支持的题库格式");
    }

    [TestMethod]
    public void ValidateQuestionBankFormat_EmptyFormat_ReturnsFailure()
    {
        // Arrange
        var emptyFormat = "";

        // Act
        var result = _validationService.ValidateQuestionBankFormat(emptyFormat);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题库格式不能为空");
    }

    [TestMethod]
    public void ValidateTemplateName_ValidName_ReturnsSuccess()
    {
        // Arrange
        var validName = "standard_template";

        // Act
        var result = _validationService.ValidateTemplateName(validName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateTemplateName_EmptyName_ReturnsFailure()
    {
        // Arrange
        var emptyName = "";

        // Act
        var result = _validationService.ValidateTemplateName(emptyName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("模板名称不能为空");
    }

    [TestMethod]
    public void Validate_NameTooLong_ReturnsFailure()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var result = _validationService.ValidateTemplateName(longName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("模板名称不能超过100个字符");
    }

    [TestMethod]
    public void ValidateTemplateName_InvalidCharacters_ReturnsFailure()
    {
        // Arrange
        var invalidName = "template<>name";

        // Act
        var result = _validationService.ValidateTemplateName(invalidName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("模板名称包含非法字符");
    }

    [TestMethod]
    public void ValidateQuestionBank_ValidQuestionBank_ReturnsSuccess()
    {
        // Arrange
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(5);

        // Act
        var result = _validationService.ValidateQuestionBank(questionBank);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateQuestionBank_NullQuestionBank_ReturnsFailure()
    {
        // Arrange
        QuestionBank? nullQuestionBank = null;

        // Act
        var result = _validationService.ValidateQuestionBank(nullQuestionBank);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题库数据不能为空");
    }

    [TestMethod]
    public void ValidateQuestionBank_EmptyName_ReturnsFailure()
    {
        // Arrange
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(5);
        questionBank.Name = "";

        // Act
        var result = _validationService.ValidateQuestionBank(questionBank);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题库名称不能为空");
    }

    [TestMethod]
    public void ValidateQuestionBank_NoQuestions_ReturnsFailure()
    {
        // Arrange
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(0);

        // Act
        var result = _validationService.ValidateQuestionBank(questionBank);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题库中必须包含至少一个题目");
    }

    [TestMethod]
    public void ValidateQuestionBank_InvalidQuestion_ReturnsFailure()
    {
        // Arrange
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(2);
        questionBank.Questions[1].Content = ""; // 无效题目

        // Act
        var result = _validationService.ValidateQuestionBank(questionBank);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("题目 #2 内容不能为空");
    }

    [TestMethod]
    public void ValidateQuestion_ValidQuestion_ReturnsSuccess()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1);

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateQuestion_NullQuestion_ReturnsFailure()
    {
        // Arrange
        Question? nullQuestion = null;

        // Act
        var result = _validationService.ValidateQuestion(nullQuestion);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题目数据不能为空");
    }

    [TestMethod]
    public void ValidateQuestion_EmptyType_ReturnsFailure()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1);
        question.Type = "";

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题目类型不能为空");
    }

    [TestMethod]
    public void ValidateQuestion_EmptyContent_ReturnsFailure()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1);
        question.Content = "";

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题目内容不能为空");
    }

    [TestMethod]
    public void ValidateQuestion_EmptyAnswer_ReturnsFailure()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1);
        question.CorrectAnswer = "";

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题目答案不能为空");
    }

    [TestMethod]
    public void ValidateQuestion_MultipleChoiceNoOptions_ReturnsFailure()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1, QuestionType.MultipleChoice);
        question.Options = new List<string>();

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("选择题必须包含选项");
    }

    [TestMethod]
    public void ValidateQuestion_MultipleChoiceInsufficientOptions_ReturnsFailure()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1, QuestionType.MultipleChoice);
        question.Options = new List<string> { "Only one option" };

        // Act
        var result = _validationService.ValidateQuestion(question);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("选择题必须包含至少2个选项");
    }

    [TestMethod]
    public void ValidateQuestion_WithQuestionNumber_IncludesNumberInErrorMessage()
    {
        // Arrange
        var question = TestDataGenerator.GenerateTestQuestion(1);
        question.Content = "";
        var questionNumber = 5;

        // Act
        var result = _validationService.ValidateQuestion(question, questionNumber);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("题目 #5 内容不能为空");
    }

    [TestMethod]
    public void ValidateBatchConfig_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            CreateTestExcelFile("test2.xlsx")
        };
        
        var batchConfig = new BatchProcessingConfig
        {
            InputFiles = testFiles,
            OutputDirectory = TestTempPath
        };

        // Act
        var result = _validationService.ValidateBatchConfig(batchConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [TestMethod]
    public void ValidateBatchConfig_NullConfig_ReturnsFailure()
    {
        // Arrange
        BatchProcessingConfig? nullConfig = null;

        // Act
        var result = _validationService.ValidateBatchConfig(nullConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("批处理配置不能为空");
    }

    [TestMethod]
    public void ValidateBatchConfig_NoInputFiles_ReturnsFailure()
    {
        // Arrange
        var batchConfig = new BatchProcessingConfig
        {
            InputFiles = new List<string>(),
            OutputDirectory = TestTempPath
        };

        // Act
        var result = _validationService.ValidateBatchConfig(batchConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("输入文件列表不能为空");
    }

    [TestMethod]
    public void ValidateBatchConfig_TooManyFiles_ReturnsFailure()
    {
        // Arrange
        var testFiles = new List<string>();
        for (int i = 0; i < 101; i++)
        {
            testFiles.Add(CreateTestExcelFile($"test{i}.xlsx"));
        }
        
        var batchConfig = new BatchProcessingConfig
        {
            InputFiles = testFiles,
            OutputDirectory = TestTempPath
        };

        // Act
        var result = _validationService.ValidateBatchConfig(batchConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("批处理文件数量不能超过100个");
    }

    [TestMethod]
    public void ValidateBatchConfig_InvalidInputFile_ReturnsFailure()
    {
        // Arrange
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            Path.Combine(TestTempPath, "nonexistent.xlsx")
        };
        
        var batchConfig = new BatchProcessingConfig
        {
            InputFiles = testFiles,
            OutputDirectory = TestTempPath
        };

        // Act
        var result = _validationService.ValidateBatchConfig(batchConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("输入文件 #2 不存在");
    }

    [TestMethod]
    public void ValidateBatchConfig_InvalidOutputDirectory_ReturnsFailure()
    {
        // Arrange
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx")
        };
        
        var batchConfig = new BatchProcessingConfig
        {
            InputFiles = testFiles,
            OutputDirectory = "invalid:path" // 无效路径
        };

        // Act
        var result = _validationService.ValidateBatchConfig(batchConfig);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("无法创建输出目录");
    }

    [TestMethod]
    public async Task ExecuteSafelyAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedResult = "test result";
        var operationName = "Test Operation";

        // Act
        var result = await _validationService.ExecuteSafelyAsync(() => Task.FromResult(expectedResult), operationName);

        // Assert
        result.Should().Be(expectedResult);
    }

    [TestMethod]
    public async Task ExecuteSafelyAsync_FailedOperation_ReturnsDefaultValue()
    {
        // Arrange
        var operationName = "Test Operation";
        var defaultValue = "default value";

        // Act
        var result = await _validationService.ExecuteSafelyAsync(() => throw new Exception("Test exception"), operationName, defaultValue);

        // Assert
        result.Should().Be(defaultValue);
    }

    [TestMethod]
    public async Task ExecuteSafelyAsync_VoidOperation_Successful_ReturnsTrue()
    {
        // Arrange
        var operationName = "Test Operation";
        var operationExecuted = false;

        // Act
        var result = await _validationService.ExecuteSafelyAsync(async () => 
        {
            operationExecuted = true;
            await Task.Delay(10);
        }, operationName);

        // Assert
        result.Should().BeTrue();
        operationExecuted.Should().BeTrue();
    }

    [TestMethod]
    public async Task ExecuteSafelyAsync_VoidOperation_Failed_ReturnsFalse()
    {
        // Arrange
        var operationName = "Test Operation";

        // Act
        var result = await _validationService.ExecuteSafelyAsync(() => throw new Exception("Test exception"), operationName);

        // Assert
        result.Should().BeFalse();
    }
}