using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace QuizForge.CLI.Tests.Integration.CommandExecution;

[TestClass]
public class CommandExecutionIntegrationTests : TestBase
{
    private IHost _host = null!;
    private IServiceProvider _serviceProvider = null!;
    private TestConsole _testConsole = null!;

    [TestInitialize]
    public void Setup()
    {
        _testConsole = new TestConsole();
        
        // 创建测试主机
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // 使用测试配置
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.test.json", optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["LaTeX:TemplatePath"] = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "Templates"),
                        ["LaTeX:OutputPath"] = Path.Combine(TestTempPath, "Output"),
                        ["LaTeX:WorkingDirectory"] = Path.Combine(TestTempPath, "Temp"),
                        ["Templates:Directory"] = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "Templates"),
                        ["CLI:DefaultOutputDirectory"] = Path.Combine(TestTempPath, "Output"),
                        ["TestSettings:MockDataPath"] = Path.Combine(Directory.GetCurrentDirectory(), "MockData")
                    })
                    .Build();

                services.AddSingleton(configuration);

                // 注册CLI服务
                services.AddSingleton<ICliFileService, CliFileService>();
                services.AddSingleton<ICliProgressService, MockCliProgressService>();
                services.AddSingleton<ICliConfigurationService, CliConfigurationService>();
                services.AddSingleton<ICliGenerationService, CliGenerationService>();
                services.AddSingleton<ICliValidationService, CliValidationService>();

                // 注册基础设施服务
                services.AddSingleton<IExcelParser, MockExcelParser>();
                services.AddSingleton<IMarkdownParser, MockMarkdownParser>();
                services.AddSingleton<IPdfEngine, MockPdfEngine>();

                // 注册核心组件
                services.AddSingleton<LaTeXDocumentGenerator>();
                services.AddSingleton<ContentGenerator>();

                // 注册QuizForge服务
                services.AddSingleton<IQuestionService, MockQuestionService>();
                services.AddSingleton<ITemplateService, MockTemplateService>();
                services.AddSingleton<IGenerationService, MockGenerationService>();
                services.AddSingleton<IExportService, MockExportService>();

                // 配置选项
                services.Configure<LaTeXOptions>(configuration.GetSection("LaTeX"));
                services.Configure<ExcelOptions>(configuration.GetSection("Excel"));
                services.Configure<PdfOptions>(configuration.GetSection("PDF"));
                services.Configure<TemplateOptions>(configuration.GetSection("Templates"));
                services.Configure<CliOptions>(configuration.GetSection("CLI"));

                // 注册日志服务
                services.AddLogging(builder => 
                {
                    builder.AddDebug();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });

                // 注册命令
                services.AddScoped<ExcelGenerateCommand>();
                services.AddScoped<MarkdownGenerateCommand>();
                services.AddScoped<BatchCommand>();
                services.AddScoped<ValidateCommand>();
                services.AddScoped<TemplateListCommand>();
                services.AddScoped<TemplateCreateCommand>();
                services.AddScoped<TemplateDeleteCommand>();
                services.AddScoped<ConfigShowCommand>();
                services.AddScoped<ConfigSetCommand>();
                services.AddScoped<ConfigResetCommand>();
            })
            .Build();

        _serviceProvider = _host.Services;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _host?.Dispose();
        base.Cleanup();
    }

    [TestMethod]
    public async Task ExcelGenerateCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        var parameters = new CliCommandParameters
        {
            InputFile = testFile,
            OutputFile = outputFile,
            Title = "测试试卷",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证输出文件被创建
        File.Exists(outputFile).Should().BeTrue();
        
        // 验证输出文件内容
        var outputContent = await File.ReadAllTextAsync(outputFile);
        outputContent.Should().Contain("Mock PDF content");
        outputContent.Should().Contain("test.xlsx");
    }

    [TestMethod]
    public async Task MarkdownGenerateCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestMarkdownFile("test.md");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        var command = _serviceProvider.GetRequiredService<MarkdownGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate markdown");
        var parameters = new CliCommandParameters
        {
            InputFile = testFile,
            OutputFile = outputFile,
            Title = "测试试卷",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证输出文件被创建
        File.Exists(outputFile).Should().BeTrue();
        
        // 验证输出文件内容
        var outputContent = await File.ReadAllTextAsync(outputFile);
        outputContent.Should().Contain("Mock PDF content");
        outputContent.Should().Contain("test.md");
    }

    [TestMethod]
    public async Task BatchCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "input");
        var outputDir = Path.Combine(TestTempPath, "output");
        Directory.CreateDirectory(inputDir);
        
        // 创建多个测试文件
        var testFiles = new List<string>
        {
            CreateTestExcelFile("test1.xlsx"),
            CreateTestExcelFile("test2.xlsx"),
            CreateTestMarkdownFile("test3.md")
        };
        
        var command = _serviceProvider.GetRequiredService<BatchCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        var parameters = new BatchParameters
        {
            InputDirectory = inputDir,
            OutputDirectory = outputDir,
            FilePattern = "*.*",
            MaxParallel = 2,
            Template = "standard",
            Title = "批量测试试卷",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = true,
            Verbose = true,
            ContinueOnError = true
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证输出目录被创建
        Directory.Exists(outputDir).Should().BeTrue();
        
        // 验证输出文件被创建
        var outputFiles = Directory.GetFiles(outputDir, "*.pdf");
        outputFiles.Should().HaveCount(3);
        
        // 验证每个输出文件的内容
        foreach (var outputFile in outputFiles)
        {
            var outputContent = await File.ReadAllTextAsync(outputFile);
            outputContent.Should().Contain("Mock PDF content");
        }
    }

    [TestMethod]
    public async Task ValidateCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        
        var command = _serviceProvider.GetRequiredService<ValidateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "validate");
        var parameters = new CliCommandParameters
        {
            InputFile = testFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
    }

    [TestMethod]
    public async Task TemplateListCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<TemplateListCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template list");
        var parameters = new TemplateParameters();

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
    }

    [TestMethod]
    public async Task TemplateCreateCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var sourceFile = CreateTestExcelFile("template.tex");
        var command = _serviceProvider.GetRequiredService<TemplateCreateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "template create");
        var parameters = new TemplateParameters
        {
            Name = "test_template",
            FilePath = sourceFile,
            IsDefault = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
    }

    [TestMethod]
    public async Task ConfigShowCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<ConfigShowCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        var parameters = new ConfigParameters
        {
            ShowAll = false,
            Key = ""
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
    }

    [TestMethod]
    public async Task ConfigSetCommand_FullExecutionFlow_ReturnsSuccess()
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<ConfigSetCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config set");
        var parameters = new ConfigParameters
        {
            Key = "CLI:ShowProgress",
            Value = "true"
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
    }

    [TestMethod]
    public async Task CommandExecution_ErrorHandling_ReturnsFailure()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        var parameters = new CliCommandParameters
        {
            InputFile = nonExistentFile,
            OutputFile = Path.Combine(TestTempPath, "output.pdf"),
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1);
        
        // 验证输出文件没有被创建
        File.Exists(Path.Combine(TestTempPath, "output.pdf")).Should().BeFalse();
    }

    [TestMethod]
    public async Task CommandExecution_ServiceDependencyInjection_WorksCorrectly()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        var parameters = new CliCommandParameters
        {
            InputFile = testFile,
            OutputFile = outputFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证所有依赖服务都被正确注入和使用
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var progressService = _serviceProvider.GetRequiredService<ICliProgressService>();
        
        fileService.Should().NotBeNull();
        generationService.Should().NotBeNull();
        progressService.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CommandExecution_ConfigurationLoading_WorksCorrectly()
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<ConfigShowCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "config show");
        var parameters = new ConfigParameters
        {
            ShowAll = false,
            Key = "CLI:ShowProgress"
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证配置被正确加载
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        configService.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CommandExecution_MultipleCommands_SequentialExecution_WorksCorrectly()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        var excelCommand = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var validateCommand = _serviceProvider.GetRequiredService<ValidateCommand>();
        
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var generateParams = new CliCommandParameters
        {
            InputFile = testFile,
            OutputFile = outputFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };
        
        var validateParams = new CliCommandParameters
        {
            InputFile = testFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act - 先执行生成命令
        var generateExitCode = await excelCommand.ExecuteAsync(context, generateParams);
        
        // 然后执行验证命令
        var validateExitCode = await validateCommand.ExecuteAsync(context, validateParams);

        // Assert
        generateExitCode.Should().Be(0);
        validateExitCode.Should().Be(0);
        
        // 验证输出文件被创建
        File.Exists(outputFile).Should().BeTrue();
    }
}