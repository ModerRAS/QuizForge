using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Unit.Commands;

[TestClass]
public class GenerationCommandsTests : TestBase
{
    private Mock<ILogger<ExcelGenerateCommand>> _mockExcelLogger = null!;
    private Mock<ILogger<MarkdownGenerateCommand>> _mockMarkdownLogger = null!;
    private Mock<ILogger<BatchCommand>> _mockBatchLogger = null!;
    private Mock<ILogger<ValidateCommand>> _mockValidateLogger = null!;
    private Mock<ICliGenerationService> _mockGenerationService = null!;
    private Mock<ICliProgressService> _mockProgressService = null!;
    private Mock<ICliFileService> _mockFileService = null!;
    private TestConsole _testConsole = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockExcelLogger = new Mock<ILogger<ExcelGenerateCommand>>();
        _mockMarkdownLogger = new Mock<ILogger<MarkdownGenerateCommand>>();
        _mockBatchLogger = new Mock<ILogger<BatchCommand>>();
        _mockValidateLogger = new Mock<ILogger<ValidateCommand>>();
        _mockGenerationService = new Mock<ICliGenerationService>();
        _mockProgressService = new Mock<ICliProgressService>();
        _mockFileService = new Mock<ICliFileService>();
        _testConsole = new TestConsole();
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public async Task ExcelGenerateCommand_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var expectedParams = GetTestParameters(testFile, outputFile);
        
        var expectedResult = TestDataGenerator.GenerateTestResult(true);
        
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        _mockGenerationService.Setup(x => x.GenerateFromExcelAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == outputFile)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, expectedParams);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.GenerateFromExcelAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == outputFile)), Times.Once);
    }

    [TestMethod]
    public async Task ExcelGenerateCommand_EmptyInputFile_ReturnsFailure()
    {
        // Arrange
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        var parameters = new CliCommandParameters { InputFile = "" };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("输入文件路径不能为空"), Times.Once);
        _mockGenerationService.Verify(x => x.GenerateFromExcelAsync(It.IsAny<CliCommandParameters>()), Times.Never);
    }

    [TestMethod]
    public async Task ExcelGenerateCommand_GenerationFailure_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var failedResult = TestDataGenerator.GenerateTestResult(false);
        
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        _mockGenerationService.Setup(x => x.GenerateFromExcelAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == outputFile)))
            .ReturnsAsync(failedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError($"生成失败: {failedResult.ErrorMessage}"), Times.Once);
    }

    [TestMethod]
    public async Task ExcelGenerateCommand_AutoGeneratesOutputFile_UsesInputFileName()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var expectedOutputFile = Path.Combine(Path.GetDirectoryName(testFile)!, "test.pdf");
        var parameters = GetTestParameters(testFile, ""); // 空输出文件
        
        var expectedResult = TestDataGenerator.GenerateTestResult(true);
        
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        _mockGenerationService.Setup(x => x.GenerateFromExcelAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == expectedOutputFile)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.GenerateFromExcelAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == expectedOutputFile)), Times.Once);
    }

    [TestMethod]
    public async Task MarkdownGenerateCommand_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestMarkdownFile("test.md");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var expectedResult = TestDataGenerator.GenerateTestResult(true);
        
        var command = new MarkdownGenerateCommand(
            _mockMarkdownLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate markdown");
        
        _mockGenerationService.Setup(x => x.GenerateFromMarkdownAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == outputFile)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.GenerateFromMarkdownAsync(It.Is<CliCommandParameters>(p => 
            p.InputFile == testFile && p.OutputFile == outputFile)), Times.Once);
    }

    [TestMethod]
    public async Task MarkdownGenerateCommand_EmptyInputFile_ReturnsFailure()
    {
        // Arrange
        var command = new MarkdownGenerateCommand(
            _mockMarkdownLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate markdown");
        var parameters = new CliCommandParameters { InputFile = "" };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("输入文件路径不能为空"), Times.Once);
        _mockGenerationService.Verify(x => x.GenerateFromMarkdownAsync(It.IsAny<CliCommandParameters>()), Times.Never);
    }

    [TestMethod]
    public async Task BatchCommand_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        // 创建测试文件
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            CreateTestExcelFile("test2.xlsx")
        };
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        
        var expectedResult = new QuizForge.CLI.Models.BatchGenerationResult
        {
            TotalFiles = 2,
            SuccessCount = 2,
            FailureCount = 0,
            SuccessResults = new List<GenerationResult>
            {
                TestDataGenerator.GenerateTestResult(true),
                TestDataGenerator.GenerateTestResult(true)
            }
        };
        
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(testFiles.Select(f => new FileInfo(f)).ToList());
        
        _mockGenerationService.Setup(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == outputDir)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == outputDir)), Times.Once);
    }

    [TestMethod]
    public async Task BatchCommand_EmptyInputDirectory_ReturnsFailure()
    {
        // Arrange
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        var parameters = new BatchParameters { InputDirectory = "" };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("输入目录不能为空"), Times.Once);
        _mockGenerationService.Verify(x => x.BatchGenerateAsync(It.IsAny<BatchParameters>()), Times.Never);
    }

    [TestMethod]
    public async Task BatchCommand_NonExistingInputDirectory_ReturnsFailure()
    {
        // Arrange
        var nonExistentDir = Path.Combine(TestTempPath, "nonexistent");
        var outputDir = Path.Combine(TestTempPath, "output");
        var parameters = GetBatchTestParameters(nonExistentDir, outputDir);
        
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError($"输入目录不存在: {nonExistentDir}"), Times.Once);
        _mockGenerationService.Verify(x => x.BatchGenerateAsync(It.IsAny<BatchParameters>()), Times.Never);
    }

    [TestMethod]
    public async Task BatchCommand_AutoGeneratesOutputDirectory_UsesDefault()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        Directory.CreateDirectory(inputDir);
        
        var expectedOutputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var parameters = GetBatchTestParameters(inputDir, ""); // 空输出目录
        
        var expectedResult = new QuizForge.CLI.Models.BatchGenerationResult
        {
            TotalFiles = 0,
            SuccessCount = 0,
            FailureCount = 0
        };
        
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(new List<FileInfo>());
        
        _mockGenerationService.Setup(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == expectedOutputDir)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == expectedOutputDir)), Times.Once);
    }

    [TestMethod]
    public async Task BatchCommand_PartialFailures_ContinueOnError_ReturnsSuccess()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            CreateTestExcelFile("test2.xlsx")
        };
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        parameters.ContinueOnError = true;
        
        var expectedResult = new QuizForge.CLI.Models.BatchGenerationResult
        {
            TotalFiles = 2,
            SuccessCount = 1,
            FailureCount = 1,
            SuccessResults = new List<GenerationResult>
            {
                TestDataGenerator.GenerateTestResult(true)
            },
            FailureResults = new List<GenerationResult>
            {
                TestDataGenerator.GenerateTestResult(false)
            }
        };
        
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(testFiles.Select(f => new FileInfo(f)).ToList());
        
        _mockGenerationService.Setup(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == outputDir)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0); // ContinueOnError = true, so partial failure is still success
        _mockProgressService.Verify(x => x.ShowWarning("1 个文件生成失败"), Times.Once);
    }

    [TestMethod]
    public async Task BatchCommand_PartialFailures_StopOnError_ReturnsFailure()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            CreateTestExcelFile("test2.xlsx")
        };
        
        var parameters = GetBatchTestParameters(inputDir, outputDir);
        parameters.ContinueOnError = false;
        
        var expectedResult = new QuizForge.CLI.Models.BatchGenerationResult
        {
            TotalFiles = 2,
            SuccessCount = 1,
            FailureCount = 1,
            SuccessResults = new List<GenerationResult>
            {
                TestDataGenerator.GenerateTestResult(true)
            },
            FailureResults = new List<GenerationResult>
            {
                TestDataGenerator.GenerateTestResult(false)
            }
        };
        
        var command = new BatchCommand(
            _mockBatchLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        
        _mockFileService.Setup(x => x.GetFilesAsync(inputDir, "*.*"))
            .ReturnsAsyncAsync(testFiles.Select(f => new FileInfo(f)).ToList());
        
        _mockGenerationService.Setup(x => x.BatchGenerateAsync(It.Is<BatchParameters>(p => 
            p.InputDirectory == inputDir && p.OutputDirectory == outputDir)))
            .ReturnsAsync(expectedResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1); // ContinueOnError = false, so partial failure is failure
        _mockProgressService.Verify(x => x.ShowWarning("1 个文件生成失败"), Times.Once);
    }

    [TestMethod]
    public async Task ValidateCommand_ValidExcelFile_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var parameters = GetTestParameters(testFile);
        
        var validationResult = new ValidationResult { IsValid = true };
        
        var command = new ValidateCommand(
            _mockValidateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "validate");
        
        _mockGenerationService.Setup(x => x.ValidateExcelAsync(testFile))
            .ReturnsAsync(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockGenerationService.Verify(x => x.ValidateExcelAsync(testFile), Times.Once);
        _mockProgressService.Verify(x => x.ShowValidationResult(validationResult), Times.Once);
    }

    [TestMethod]
    public async Task ValidateCommand_EmptyInputFile_ReturnsFailure()
    {
        // Arrange
        var command = new ValidateCommand(
            _mockValidateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "validate");
        var parameters = new CliCommandParameters { InputFile = "" };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("输入文件路径不能为空"), Times.Once);
        _mockGenerationService.Verify(x => x.ValidateExcelAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateCommand_InvalidFile_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var parameters = GetTestParameters(testFile);
        
        var validationResult = new ValidationResult { IsValid = false, Errors = { "Invalid format" } };
        
        var command = new ValidateCommand(
            _mockValidateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "validate");
        
        _mockGenerationService.Setup(x => x.ValidateExcelAsync(testFile))
            .ReturnsAsync(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockGenerationService.Verify(x => x.ValidateExcelAsync(testFile), Times.Once);
        _mockProgressService.Verify(x => x.ShowValidationResult(validationResult), Times.Once);
    }

    [TestMethod]
    public async Task ValidateCommand_MarkdownFile_UsesFileValidation()
    {
        // Arrange
        var testFile = CreateTestMarkdownFile("test.md");
        var parameters = GetTestParameters(testFile);
        
        var validationResult = new ValidationResult { IsValid = true };
        
        var command = new ValidateCommand(
            _mockValidateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "validate");
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsync(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockFileService.Verify(x => x.ValidateFileAsync(testFile), Times.Once);
        _mockGenerationService.Verify(x => x.ValidateExcelAsync(It.IsAny<string>()), Times.Never);
        _mockProgressService.Verify(x => x.ShowValidationResult(validationResult), Times.Once);
    }

    [TestMethod]
    public async Task Commands_ExceptionHandling_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        var parameters = GetTestParameters(testFile, outputFile);
        
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        _mockGenerationService.Setup(x => x.GenerateFromExcelAsync(It.IsAny<CliCommandParameters>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("执行失败: Test exception"), Times.Once);
    }

    [TestMethod]
    public async Task Commands_NullParameters_ThrowsException()
    {
        // Arrange
        var command = new ExcelGenerateCommand(
            _mockExcelLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");

        // Act & Assert
        await command.Invoking(async c => await c.ExecuteAsync(context, null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }
}