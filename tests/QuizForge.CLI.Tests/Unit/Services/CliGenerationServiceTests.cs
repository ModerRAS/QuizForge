using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using QuizForge.CLI.Models;
using QuizForge.CLI.Services;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Core.ContentGeneration;
using QuizForge.Infrastructure.Engines;
using QuizForge.CLI.Tests.Fixtures;

namespace QuizForge.CLI.Tests.Unit.Services;

[TestClass]
public class CliGenerationServiceTests : TestBase
{
    private ICliGenerationService _generationService = null!;
    private Mock<IExcelParser> _mockExcelParser = null!;
    private Mock<IMarkdownParser> _mockMarkdownParser = null!;
    private Mock<IPdfEngine> _mockPdfEngine = null!;
    private Mock<LaTeXDocumentGenerator> _mockLatexGenerator = null!;
    private Mock<ICliFileService> _mockFileService = null!;
    private Mock<ICliProgressService> _mockProgressService = null!;
    private Mock<ICliConfigurationService> _mockConfigService = null!;
    private Mock<ILogger<CliGenerationService>> _mockLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        // 创建Mock对象
        _mockExcelParser = new Mock<IExcelParser>();
        _mockMarkdownParser = new Mock<IMarkdownParser>();
        _mockPdfEngine = new Mock<IPdfEngine>();
        _mockLatexGenerator = new Mock<LaTeXDocumentGenerator>();
        _mockFileService = new Mock<ICliFileService>();
        _mockProgressService = new Mock<ICliProgressService>();
        _mockConfigService = new Mock<ICliConfigurationService>();
        _mockLogger = new Mock<ILogger<CliGenerationService>>();

        // 创建测试服务
        _generationService = new CliGenerationService(
            _mockLogger.Object,
            _mockExcelParser.Object,
            _mockMarkdownParser.Object,
            _mockPdfEngine.Object,
            _mockLatexGenerator.Object,
            _mockFileService.Object,
            _mockProgressService.Object,
            _mockConfigService.Object,
            Options.Create(new LaTeXOptions()),
            Options.Create(new PdfOptions()),
            Options.Create(new TemplateOptions())
        );
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public async Task GenerateFromExcelAsync_ValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var expectedQuestionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        
        // Mock设置
        _mockExcelParser.Setup(x => x.ParseAsync(testFile))
            .ReturnsAsync(expectedQuestionBank);
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), outputFile))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _mockFileService.Setup(x => x.GetFileInfoAsync(outputFile))
            .ReturnsAsyncAsync(new FileInfo(outputFile));
        
        var mockProgress = new Mock<IDisposable>();
        _mockProgressService.Setup(x => x.StartProgressAsync(5, It.IsAny<string>()))
            .ReturnsAsync(mockProgress.Object);

        // Act
        var result = await _generationService.GenerateFromExcelAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.QuestionCount.Should().Be(10);
        result.OutputPath.Should().Be(outputFile);
        result.FileSize.Should().BeGreaterThan(0);
        result.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);

        // 验证Mock调用
        _mockExcelParser.Verify(x => x.ParseAsync(testFile), Times.Once);
        _mockPdfEngine.Verify(x => x.GenerateFromLatexAsync(It.IsAny<string>(), outputFile), Times.Once);
        _mockFileService.Verify(x => x.EnsureDirectoryExistsAsync(Path.GetDirectoryName(outputFile)), Times.Once);
    }

    [TestMethod]
    public async Task GenerateFromExcelAsync_FileNotFound_ReturnsFailureResult()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(nonExistentFile, outputFile);

        // Mock设置
        _mockExcelParser.Setup(x => x.ParseAsync(nonExistentFile))
            .ThrowsAsync(new FileNotFoundException("File not found"));

        // Act
        var result = await _generationService.GenerateFromExcelAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("File not found");
        result.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [TestMethod]
    public async Task GenerateFromExcelAsync_EmptyQuestionBank_ReturnsFailureResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("empty.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var emptyQuestionBank = new QuestionBank { Questions = new List<Question>() };
        
        // Mock设置
        _mockExcelParser.Setup(x => x.ParseAsync(testFile))
            .ReturnsAsync(emptyQuestionBank);

        // Act
        var result = await _generationService.GenerateFromExcelAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Excel文件中没有找到题目");
        result.QuestionCount.Should().Be(0);
    }

    [TestMethod]
    public async Task GenerateFromExcelAsync_PdfGenerationFails_ReturnsFailureResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var expectedQuestionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        
        // Mock设置
        _mockExcelParser.Setup(x => x.ParseAsync(testFile))
            .ReturnsAsync(expectedQuestionBank);
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), outputFile))
            .ReturnsAsync(false);

        // Act
        var result = await _generationService.GenerateFromExcelAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("PDF生成失败");
    }

    [TestMethod]
    public async Task GenerateFromMarkdownAsync_ValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var testFile = CreateTestMarkdownFile("test.md");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var expectedQuestionBank = TestDataGenerator.GenerateTestQuestionBank(8);
        
        // Mock设置
        _mockMarkdownParser.Setup(x => x.ParseAsync(testFile))
            .ReturnsAsync(expectedQuestionBank);
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), outputFile))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _mockFileService.Setup(x => x.GetFileInfoAsync(outputFile))
            .ReturnsAsyncAsync(new FileInfo(outputFile));

        // Act
        var result = await _generationService.GenerateFromMarkdownAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.QuestionCount.Should().Be(8);
        result.OutputPath.Should().Be(outputFile);
        result.FileSize.Should().BeGreaterThan(0);

        // 验证Mock调用
        _mockMarkdownParser.Verify(x => x.ParseAsync(testFile), Times.Once);
        _mockPdfEngine.Verify(x => x.GenerateFromLatexAsync(It.IsAny<string>(), outputFile), Times.Once);
    }

    [TestMethod]
    public async Task GenerateFromMarkdownAsync_InvalidFile_ReturnsFailureResult()
    {
        // Arrange
        var testFile = CreateTestMarkdownFile("test.md");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        // Mock设置
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = false, Errors = { "Invalid file" } });

        // Act
        var result = await _generationService.GenerateFromMarkdownAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("文件验证失败");
    }

    [TestMethod]
    public async Task BatchGenerateAsync_ValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        // 创建测试文件
        var testFiles = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            testFiles.Add(CreateTestExcelFile($"test{i}.xlsx"));
        }
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        
        var expectedQuestionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        
        // Mock设置
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(testFiles.Select(f => new FileInfo(f)).ToList());
        
        _mockExcelParser.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedQuestionBank);
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _mockFileService.Setup(x => x.GetFileInfoAsync(It.IsAny<string>()))
            .ReturnsAsyncAsync(new FileInfo("test.pdf"));

        // Act
        var result = await _generationService.BatchGenerateAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.TotalFiles.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        result.SuccessResults.Should().HaveCount(3);
        result.FailureResults.Should().BeEmpty();
        result.TotalProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [TestMethod]
    public async Task BatchGenerateAsync_NoFilesFound_ReturnsEmptyResult()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        
        // Mock设置
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(new List<FileInfo>());

        // Act
        var result = await _generationService.BatchGenerateAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.TotalFiles.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [TestMethod]
    public async Task BatchGenerateAsync_PartialFailures_ReturnsMixedResult()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        // 创建测试文件
        var testFiles = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            testFiles.Add(CreateTestExcelFile($"test{i}.xlsx"));
        }
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        
        var expectedQuestionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        
        // Mock设置
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(testFiles.Select(f => new FileInfo(f)).ToList());
        
        // 第一个文件成功，第二个失败，第三个成功
        _mockExcelParser.Setup(x => x.ParseAsync(testFiles[0]))
            .ReturnsAsync(expectedQuestionBank);
        _mockExcelParser.Setup(x => x.ParseAsync(testFiles[1]))
            .ThrowsAsync(new Exception("Parse error"));
        _mockExcelParser.Setup(x => x.ParseAsync(testFiles[2]))
            .ReturnsAsync(expectedQuestionBank);
        
        _mockPdfEngine.Setup(x => x.GenerateFromLatexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        _mockFileService.Setup(x => x.GetFileInfoAsync(It.IsAny<string>()))
            .ReturnsAsyncAsync(new FileInfo("test.pdf"));

        // Act
        var result = await _generationService.BatchGenerateAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.TotalFiles.Should().Be(3);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(1);
        result.SuccessResults.Should().HaveCount(2);
        result.FailureResults.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task ValidateExcelAsync_ValidFile_ReturnsValidResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("valid.xlsx");
        
        // Mock设置
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockExcelParser.Setup(x => x.ValidateFormatAsync(testFile))
            .ReturnsAsync(true);

        // Act
        var result = await _generationService.ValidateExcelAsync(testFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Information.Should().Contain("Excel文件格式验证通过");
    }

    [TestMethod]
    public async Task ValidateExcelAsync_InvalidFormat_ReturnsInvalidResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("invalid.xlsx");
        
        // Mock设置
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockExcelParser.Setup(x => x.ValidateFormatAsync(testFile))
            .ReturnsAsync(false);

        // Act
        var result = await _generationService.ValidateExcelAsync(testFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Excel文件格式不正确，请确保包含必要的列（题型、题目、答案）");
    }

    [TestMethod]
    public async Task GetAvailableTemplatesAsync_TemplatesExist_ReturnsTemplateList()
    {
        // Arrange
        var templateDir = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "Templates");
        Directory.CreateDirectory(templateDir);
        
        // 创建测试模板文件
        var templateFile = Path.Combine(templateDir, "test.tex");
        File.WriteAllText(templateFile, "Test template content");
        
        var options = Options.Create(new TemplateOptions { Directory = templateDir });
        var service = new CliGenerationService(
            _mockLogger.Object,
            _mockExcelParser.Object,
            _mockMarkdownParser.Object,
            _mockPdfEngine.Object,
            _mockLatexGenerator.Object,
            _mockFileService.Object,
            _mockProgressService.Object,
            _mockConfigService.Object,
            Options.Create(new LaTeXOptions()),
            Options.Create(new PdfOptions()),
            options
        );

        // Act
        var result = await service.GetAvailableTemplatesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("test");
        result[0].FilePath.Should().Be(templateFile);
        result[0].Type.Should().Be("LaTeX");
        
        // 清理
        File.Delete(templateFile);
    }

    [TestMethod]
    public async Task GetAvailableTemplatesAsync_NoTemplates_ReturnsEmptyList()
    {
        // Arrange
        var templateDir = Path.Combine(TestTempPath, "templates");
        Directory.CreateDirectory(templateDir);
        
        var options = Options.Create(new TemplateOptions { Directory = templateDir });
        var service = new CliGenerationService(
            _mockLogger.Object,
            _mockExcelParser.Object,
            _mockMarkdownParser.Object,
            _mockPdfEngine.Object,
            _mockLatexGenerator.Object,
            _mockFileService.Object,
            _mockProgressService.Object,
            _mockConfigService.Object,
            Options.Create(new LaTeXOptions()),
            Options.Create(new PdfOptions()),
            options
        );

        // Act
        var result = await service.GetAvailableTemplatesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}