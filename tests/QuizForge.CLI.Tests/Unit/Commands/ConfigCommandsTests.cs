using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Unit.Commands;

[TestClass]
public class ConfigCommandsTests : TestBase
{
    private Mock<ILogger<ConfigShowCommand>> _mockShowLogger = null!;
    private Mock<ILogger<ConfigSetCommand>> _mockSetLogger = null!;
    private Mock<ILogger<ConfigResetCommand>> _mockResetLogger = null!;
    private Mock<ICliConfigurationService> _mockConfigService = null!;
    private Mock<ICliProgressService> _mockProgressService = null!;
    private TestConsole _testConsole = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockShowLogger = new Mock<ILogger<ConfigShowCommand>>();
        _mockSetLogger = new Mock<ILogger<ConfigSetCommand>>();
        _mockResetLogger = new Mock<ILogger<ConfigResetCommand>>();
        _mockConfigService = new Mock<ICliConfigurationService>();
        _mockProgressService = new Mock<ICliProgressService>();
        _testConsole = new TestConsole();
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public async Task ConfigShowCommand_ShowMainConfig_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { ShowAll = false, Key = "" };
        
        var command = new ConfigShowCommand(
            _mockShowLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("standard.tex");
        
        _mockConfigService.Setup(x => x.GetValue<string>("PDF:OutputDirectory"))
            .Returns("./output");
        
        _mockConfigService.Setup(x => x.GetValue<string>("Templates:Directory"))
            .Returns("./templates");
        
        _mockConfigService.Setup(x => x.GetValue<string>("CLI:ShowProgress"))
            .Returns("true");
        
        _mockConfigService.Setup(x => x.GetValue<string>("CLI:ColoredOutput"))
            .Returns("true");
        
        var validationResult = new ValidationResult { IsValid = true };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("当前配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable("主要配置", It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess("配置验证通过"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigShowCommand_ShowSpecificKey_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { ShowAll = false, Key = "LaTeX:DefaultTemplate" };
        
        var command = new ConfigShowCommand(
            _mockShowLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("standard.tex");
        
        var validationResult = new ValidationResult { IsValid = true };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("当前配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowInfo("LaTeX:DefaultTemplate: standard.tex"), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfigShowCommand_ShowAllConfig_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { ShowAll = true, Key = "" };
        
        var command = new ConfigShowCommand(
            _mockShowLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        
        var allConfig = new Dictionary<string, object>
        {
            ["LaTeX"] = new Dictionary<string, object>
            {
                ["DefaultTemplate"] = "standard.tex",
                ["OutputPath"] = "./output"
            },
            ["PDF"] = new Dictionary<string, object>
            {
                ["OutputDirectory"] = "./output",
                ["EnablePreview"] = "true"
            }
        };
        
        _mockConfigService.Setup(x => x.GetAllValues())
            .Returns(allConfig);
        
        var validationResult = new ValidationResult { IsValid = true };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("当前配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable("LaTeX 配置", It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Once);
        _mockProgressService.Verify(x => x.ShowTable("PDF 配置", It.IsAny<string[]>(), It.IsAny<List<string[]>>()), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess("配置验证通过"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigShowCommand_ConfigValidationFails_ShowsErrors()
    {
        // Arrange
        var parameters = new ConfigParameters { ShowAll = false, Key = "" };
        
        var command = new ConfigShowCommand(
            _mockShowLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        
        _mockConfigService.Setup(x => x.GetValue<string>(It.IsAny<string>()))
            .Returns("test_value");
        
        var validationResult = new ValidationResult 
        { 
            IsValid = false, 
            Errors = { "Configuration error 1" },
            Warnings = { "Configuration warning 1" }
        };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("配置验证结果"), Times.Once);
        _mockProgressService.Verify(x => x.ShowError("Configuration error 1"), Times.Once);
        _mockProgressService.Verify(x => x.ShowWarning("Configuration warning 1"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfigSetCommand_ValidParameters_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters 
        { 
            Key = "LaTeX:DefaultTemplate", 
            Value = "advanced.tex" 
        };
        
        var command = new ConfigSetCommand(
            _mockSetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("standard.tex");
        
        _mockConfigService.Setup(x => x.SetValueAsync("LaTeX:DefaultTemplate", "advanced.tex"))
            .ReturnsAsync(true);
        
        var validationResult = new ValidationResult { IsValid = true };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("设置配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowInfo("当前值: standard.tex"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess("配置设置成功: LaTeX:DefaultTemplate = advanced.tex"), Times.Once);
        _mockConfigService.Verify(x => x.SetValueAsync("LaTeX:DefaultTemplate", "advanced.tex"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigSetCommand_EmptyKey_ReturnsFailure()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "", Value = "test_value" };
        
        var command = new ConfigSetCommand(
            _mockSetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("配置键不能为空"), Times.Once);
        _mockConfigService.Verify(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfigSetCommand_EmptyValue_ReturnsFailure()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "LaTeX:DefaultTemplate", Value = "" };
        
        var command = new ConfigSetCommand(
            _mockSetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("配置值不能为空"), Times.Once);
        _mockConfigService.Verify(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfigSetCommand_SetValueFails_ReturnsFailure()
    {
        // Arrange
        var parameters = new ConfigParameters 
        { 
            Key = "LaTeX:DefaultTemplate", 
            Value = "advanced.tex" 
        };
        
        var command = new ConfigSetCommand(
            _mockSetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("standard.tex");
        
        _mockConfigService.Setup(x => x.SetValueAsync("LaTeX:DefaultTemplate", "advanced.tex"))
            .ReturnsAsync(false);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("配置设置失败"), Times.Once);
        _mockConfigService.Verify(x => x.ValidateConfiguration(), Times.Never);
    }

    [TestMethod]
    public async Task ConfigSetCommand_NoCurrentValue_DoesNotShowCurrentValue()
    {
        // Arrange
        var parameters = new ConfigParameters 
        { 
            Key = "LaTeX:DefaultTemplate", 
            Value = "advanced.tex" 
        };
        
        var command = new ConfigSetCommand(
            _mockSetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns((string?)null); // 没有当前值
        
        _mockConfigService.Setup(x => x.SetValueAsync("LaTeX:DefaultTemplate", "advanced.tex"))
            .ReturnsAsync(true);
        
        var validationResult = new ValidationResult { IsValid = true };
        _mockConfigService.Setup(x => x.ValidateConfiguration())
            .Returns(validationResult);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowInfo(It.Is<string>(s => s.Contains("当前值"))), Times.Never);
        _mockProgressService.Verify(x => x.ShowSuccess("配置设置成功: LaTeX:DefaultTemplate = advanced.tex"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigResetCommand_ResetAll_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "" }; // 空键表示重置所有
        
        var command = new ConfigResetCommand(
            _mockResetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config reset");
        
        _mockConfigService.Setup(x => x.ResetValueAsync())
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("重置配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess("所有配置已重置为默认值"), Times.Once);
        _mockConfigService.Verify(x => x.ResetValueAsync(), Times.Once);
    }

    [TestMethod]
    public async Task ConfigResetCommand_ResetSpecificKey_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "LaTeX:DefaultTemplate" };
        
        var command = new ConfigResetCommand(
            _mockResetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config reset");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("custom.tex");
        
        _mockConfigService.Setup(x => x.ResetValueAsync("LaTeX:DefaultTemplate"))
            .ReturnsAsync(true);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("重置配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowSuccess("配置已重置: LaTeX:DefaultTemplate"), Times.Once);
        _mockConfigService.Verify(x => x.ResetValueAsync("LaTeX:DefaultTemplate"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigResetCommand_AlreadyDefaultValue_ReturnsSuccess()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "LaTeX:DefaultTemplate" };
        
        var command = new ConfigResetCommand(
            _mockResetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config reset");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns((string?)null); // 已经是默认值

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        _mockProgressService.Verify(x => x.ShowTitle("重置配置"), Times.Once);
        _mockProgressService.Verify(x => x.ShowInfo("配置 LaTeX:DefaultTemplate 已经是默认值"), Times.Once);
        _mockConfigService.Verify(x => x.ResetValueAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfigResetCommand_ResetFails_ReturnsFailure()
    {
        // Arrange
        var parameters = new ConfigParameters { Key = "LaTeX:DefaultTemplate" };
        
        var command = new ConfigResetCommand(
            _mockResetLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config reset");
        
        _mockConfigService.Setup(x => x.GetValue<string>("LaTeX:DefaultTemplate"))
            .Returns("custom.tex");
        
        _mockConfigService.Setup(x => x.ResetValueAsync("LaTeX:DefaultTemplate"))
            .ReturnsAsync(false);

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("配置重置失败: LaTeX:DefaultTemplate"), Times.Once);
    }

    [TestMethod]
    public async Task ConfigCommands_ExceptionHandling_ReturnsFailure()
    {
        // Arrange
        var parameters = new ConfigParameters { ShowAll = false, Key = "" };
        
        var command = new ConfigShowCommand(
            _mockShowLogger.Object,
            _mockConfigService.Object,
            _mockProgressService.Object
        );

        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        
        _mockConfigService.Setup(x => x.GetValue<string>(It.IsAny<string>()))
            .Throws(new Exception("Test exception"));

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        _mockProgressService.Verify(x => x.ShowError("执行失败: Test exception"), Times.Once);
    }
}