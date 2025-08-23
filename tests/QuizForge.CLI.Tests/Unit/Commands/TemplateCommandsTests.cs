using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Unit.Commands;

[TestClass]
public class TemplateCommandsTests : TestBase
{
    private Mock<ILogger<TemplateListCommand>> _mockListLogger = null!;
    private Mock<ILogger<TemplateCreateCommand>> _mockCreateLogger = null!;
    private Mock<ILogger<TemplateDeleteCommand>> _mockDeleteLogger = null!;
    private Mock<ICliGenerationService> _mockGenerationService = null!;
    private Mock<ICliProgressService> _mockProgressService = null!;
    private Mock<ICliFileService> _mockFileService = null!;
    private Mock<ICliConfigurationService> _mockConfigService = null!;
    private TestConsole _testConsole = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockListLogger = new Mock<ILogger<TemplateListCommand>>();
        _mockCreateLogger = new Mock<ILogger<TemplateCreateCommand>>();
        _mockDeleteLogger = new Mock<ILogger<TemplateDeleteCommand>>();
        _mockGenerationService = new Mock<ICliGenerationService>();
        _mockProgressService = new Mock<ICliProgressService>();
        _mockFileService = new Mock<ICliFileService>();
        _mockConfigService = new Mock<ICliConfigurationService>();
        _testConsole = new TestConsole();
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public async Task TemplateListCommand_TemplatesExist_ReturnsSuccess()
    {
        // Arrange
        var templates = new List<TemplateInfo>
        {
            new TemplateInfo
            {
                Name = "standard",
                Type = "LaTeX",
                IsDefault = true,
                Description = "Standard template"
            },
            new TemplateInfo
            {
                Name = "advanced",
                Type = "LaTeX",
                IsDefault = false,
                Description = "Advanced template"
            }
        };
        
        var command = new TemplateListCommand(
            _mockListLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template list");
        var parameters = new TemplateParameters();
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(templates);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("可用模板列表"), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable("可用模板", It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Once);
        _mockProgressService.Verify(x => x.ShowInfo($"共找到 {templates.Count} 个模板"), Times.Once);
    }

    [TestMethod]
    public async Task TemplateListCommand_NoTemplates_ReturnsSuccess()
    {
        // Arrange
        var command = new TemplateListCommand(
            _mockListLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template list");
        var parameters = new TemplateParameters();
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(new List<TemplateInfo>());

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("可用模板列表"), Times.Once);
        _mockProgressService.Verify(x => x.ShowWarning("没有找到可用模板"), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_ValidParameters_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("template.tex");
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = testFile,
            IsDefault = true
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockConfigService.Setup(x => x.GetValue<string>("Templates:Directory", "./templates"))
            .Returns("./templates");
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync("./templates"))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.CopyFileAsync(testFile, It.Is<string>(s => s.EndsWith("test_template.tex")), true))
            .ReturnsAsync(true);
        
        _mockConfigService.Setup(x => x.SetValueAsync("Templates:DefaultTemplate", "test_template.tex"))
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("创建模板"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess(It.Is<string>(s => s.Contains("模板创建成功"))), Times.Once);
        _mockFileService.Verify(x => x.CopyFileAsync(testFile, It.Is<string>(s => s.EndsWith("test_template.tex")), true), Times.Once);
        _mockConfigService.Verify(x => x.SetValueAsync("Templates:DefaultTemplate", "test_template.tex"), Times.Once);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_EmptyName_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("template.tex");
        var parameters = new TemplateParameters
        {
            Name = "",
            FilePath = testFile
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("模板名称不能为空"), Times.Once);
        _mockFileService.Verify(x => x.ValidateFileAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_EmptyFilePath_ReturnsFailure()
    {
        // Arrange
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = ""
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("模板文件路径不能为空"), Times.Once);
        _mockFileService.Verify(x => x.ValidateFileAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_InvalidFile_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("invalid.tex");
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = testFile
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = false, Errors = { "Invalid file" } });

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowValidationResult(It.Is<ValidationResult>(r => !r.IsValid)), Times.Once);
        _mockFileService.Verify(x => x.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_CopyFileFails_ReturnsFailure()
    {
        // Arrange
        var testFile = CreateTestExcelFile("template.tex");
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = testFile
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockConfigService.Setup(x => x.GetValue<string>("Templates:Directory", "./templates"))
            .Returns("./templates");
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync("./templates"))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.CopyFileAsync(testFile, It.Is<string>(s => s.EndsWith("test_template.tex")), true))
            .ReturnsAsync(false);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("模板文件创建失败"), Times.Once);
        _mockConfigService.Verify(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_NotDefault_DoesNotUpdateConfig()
    {
        // Arrange
        var testFile = CreateTestExcelFile("template.tex");
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = testFile,
            IsDefault = false
        };
        
        var command = new TemplateCreateCommand(
            _mockCreateLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");
        
        _mockFileService.Setup(x => x.ValidateFileAsync(testFile))
            .ReturnsAsyncAsync(new ValidationResult { IsValid = true });
        
        _mockConfigService.Setup(x => x.GetValue<string>("Templates:Directory", "./templates"))
            .Returns("./templates");
        
        _mockFileService.Setup(x => x.EnsureDirectoryExistsAsync("./templates"))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.CopyFileAsync(testFile, It.Is<string>(s => s.EndsWith("test_template.tex")), true))
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockConfigService.Verify(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockProgressService.Verify(x => x.ShowInfo(It.Is<string>(s => s.Contains("是否默认: 否"))), Times.Once);
    }

    [TestMethod]
    public async Task TemplateDeleteCommand_ExistingTemplate_ReturnsSuccess()
    {
        // Arrange
        var templateName = "test_template";
        var parameters = new TemplateParameters { Name = templateName };
        
        var templates = new List<TemplateInfo>
        {
            new TemplateInfo
            {
                Name = templateName,
                FilePath = "/path/to/template.tex",
                IsDefault = false
            }
        };
        
        var command = new TemplateDeleteCommand(
            _mockDeleteLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template delete");
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(templates);
        
        _mockFileService.Setup(x => x.DeleteFileAsync("/path/to/template.tex"))
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("删除模板"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess($"模板删除成功: {templateName}"), Times.Once);
        _mockFileService.Verify(x => x.DeleteFileAsync("/path/to/template.tex"), Times.Once);
    }

    [TestMethod]
    public async Task TemplateDeleteCommand_EmptyName_ReturnsFailure()
    {
        // Arrange
        var parameters = new TemplateParameters { Name = "" };
        
        var command = new TemplateDeleteCommand(
            _mockDeleteLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template delete");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("模板名称不能为空"), Times.Once);
        _mockGenerationService.Verify(x => x.GetAvailableTemplatesAsync(), Times.Never);
    }

    [TestMethod]
    public async Task TemplateDeleteCommand_NonExistingTemplate_ReturnsFailure()
    {
        // Arrange
        var templateName = "nonexistent_template";
        var parameters = new TemplateParameters { Name = templateName };
        
        var templates = new List<TemplateInfo>
        {
            new TemplateInfo { Name = "existing_template" }
        };
        
        var command = new TemplateDeleteCommand(
            _mockDeleteLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template delete");
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(templates);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError($"模板不存在: {templateName}"), Times.Once);
        _mockFileService.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task TemplateDeleteCommand_DeleteFails_ReturnsFailure()
    {
        // Arrange
        var templateName = "test_template";
        var parameters = new TemplateParameters { Name = templateName };
        
        var templates = new List<TemplateInfo>
        {
            new TemplateInfo
            {
                Name = templateName,
                FilePath = "/path/to/template.tex",
                IsDefault = false
            }
        };
        
        var command = new TemplateDeleteCommand(
            _mockDeleteLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template delete");
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(templates);
        
        _mockFileService.Setup(x => x.DeleteFileAsync("/path/to/template.tex"))
            .ReturnsAsync(false);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("模板文件删除失败"), Times.Once);
    }

    [TestMethod]
    public async Task TemplateDeleteCommand_DefaultTemplate_ResetsConfig()
    {
        // Arrange
        var templateName = "test_template";
        var parameters = new TemplateParameters { Name = templateName };
        
        var templates = new List<TemplateInfo>
        {
            new TemplateInfo
            {
                Name = templateName,
                FilePath = "/path/to/template.tex",
                IsDefault = true
            }
        };
        
        var command = new TemplateDeleteCommand(
            _mockDeleteLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object,
            _mockFileService.Object,
            _mockConfigService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template delete");
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ReturnsAsync(templates);
        
        _mockFileService.Setup(x => x.DeleteFileAsync("/path/to/template.tex"))
            .ReturnsAsync(true);
        
        _mockConfigService.Setup(x => x.SetValueAsync("Templates:DefaultTemplate", "standard.tex"))
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockConfigService.Verify(x => x.SetValueAsync("Templates:DefaultTemplate", "standard.tex"), Times.Once);
    }

    [TestMethod]
    public async Task TemplateCommands_ExceptionHandling_ReturnsFailure()
    {
        // Arrange
        var command = new TemplateListCommand(
            _mockListLogger.Object,
            _mockGenerationService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template list");
        var parameters = new TemplateParameters();
        
        _mockGenerationService.Setup(x => x.GetAvailableTemplatesAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("执行失败: Test exception"), Times.Once);
    }
}