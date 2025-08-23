using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace QuizForge.CLI.Tests.E2E;

[TestClass]
public class ExcelToPdfWorkflowTests : TestBase
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
    public async Task CompleteExcelToPdfWorkflow_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("test_questions.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "generated_exam.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var parameters = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "期末考试",
            Subject = "计算机科学",
            ExamTime = "120分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue($"PDF文件应该被创建: {outputPdfFile}");
        
        // 验证PDF文件内容
        var pdfContent = await File.ReadAllTextAsync(outputPdfFile);
        pdfContent.Should().Contain("Mock PDF content", "PDF内容应该包含生成的信息");
        pdfContent.Should().Contain("test_questions.xlsx", "PDF内容应该引用源文件");
        
        // 验证文件大小
        var fileInfo = new FileInfo(outputPdfFile);
        fileInfo.Length.Should().BeGreaterThan(0, "PDF文件应该有内容");
        
        // 验证生成时间
        var generationTime = fileInfo.CreationTime;
        (DateTime.Now - generationTime).Should().BeLessThan(TimeSpan.FromMinutes(1), "文件应该是最近生成的");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_WithCustomTemplate_ReturnsSuccess()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("test_questions.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "custom_exam.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var parameters = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "期中考试",
            Subject = "数据结构",
            ExamTime = "90分钟",
            Template = "advanced",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0);
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证PDF文件内容包含自定义信息
        var pdfContent = await File.ReadAllTextAsync(outputPdfFile);
        pdfContent.Should().Contain("Mock PDF content");
        pdfContent.Should().Contain("期中考试");
        pdfContent.Should().Contain("数据结构");
        pdfContent.Should().Contain("90分钟");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_WithValidation_ReturnsSuccess()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("valid_questions.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "validated_exam.pdf");
        
        var validateCommand = _serviceProvider.GetRequiredService<ValidateCommand>();
        var generateCommand = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var validateParams = new CliCommandParameters
        {
            InputFile = testExcelFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };
        
        var generateParams = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "验证后的试卷",
            Subject = "软件工程",
            ExamTime = "150分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act - 先验证，再生成
        var validateExitCode = await validateCommand.ExecuteAsync(context, validateParams);
        var generateExitCode = await generateCommand.ExecuteAsync(context, generateParams);

        // Assert
        validateExitCode.Should().Be(0, "验证命令应该成功");
        generateExitCode.Should().Be(0, "生成命令应该成功");
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证PDF文件内容
        var pdfContent = await File.ReadAllTextAsync(outputPdfFile);
        pdfContent.Should().Contain("Mock PDF content");
        pdfContent.Should().Contain("验证后的试卷");
        pdfContent.Should().Contain("软件工程");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_WithErrorHandling_ReturnsFailure()
    {
        // Arrange
        var invalidExcelFile = Path.Combine(TestTempPath, "invalid_questions.xlsx");
        // 不创建文件，模拟不存在的情况
        
        var outputPdfFile = Path.Combine(TestTempPath, "error_exam.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var parameters = new CliCommandParameters
        {
            InputFile = invalidExcelFile,
            OutputFile = outputPdfFile,
            Title = "错误测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(1, "错误处理应该返回失败代码");
        
        // 验证PDF文件没有被创建
        File.Exists(outputPdfFile).Should().BeFalse("错误情况下不应该创建PDF文件");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_WithConfiguration_ReturnsSuccess()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("config_test.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "config_exam.pdf");
        
        var configCommand = _serviceProvider.GetRequiredService<ConfigSetCommand>();
        var generateCommand = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var configParams = new ConfigParameters
        {
            Key = "CLI:ShowProgress",
            Value = "false"
        };
        
        var generateParams = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "配置测试试卷",
            Subject = "网络工程",
            ExamTime = "180分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act - 先设置配置，再生成
        var configExitCode = await configCommand.ExecuteAsync(context, configParams);
        var generateExitCode = await generateCommand.ExecuteAsync(context, generateParams);

        // Assert
        configExitCode.Should().Be(0, "配置设置应该成功");
        generateExitCode.Should().Be(0, "生成命令应该成功");
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证配置被正确应用
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        var showProgress = configService.GetValue<string>("CLI:ShowProgress", "true");
        showProgress.Should().Be("false", "配置应该被正确设置");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_MultipleFiles_BatchProcessing_ReturnsSuccess()
    {
        // Arrange
        var inputDir = Path.Combine(TestTempPath, "batch_input");
        var outputDir = Path.Combine(TestTempPath, "batch_output");
        Directory.CreateDirectory(inputDir);
        
        // 创建多个Excel文件
        var excelFiles = new List<string>
        {
            CreateTestExcelFile("exam1.xlsx"),
            CreateTestExcelFile("exam2.xlsx"),
            CreateTestExcelFile("exam3.xlsx")
        };
        
        var batchCommand = _serviceProvider.GetRequiredService<BatchCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "batch");
        
        var parameters = new BatchParameters
        {
            InputDirectory = inputDir,
            OutputDirectory = outputDir,
            FilePattern = "*.xlsx",
            MaxParallel = 2,
            Template = "standard",
            Title = "批量生成试卷",
            Subject = "综合测试",
            ExamTime = "120分钟",
            Validate = true,
            Verbose = true,
            ContinueOnError = true
        };

        // Act
        var exitCode = await batchCommand.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0, "批量处理应该成功");
        
        // 验证输出目录被创建
        Directory.Exists(outputDir).Should().BeTrue();
        
        // 验证所有PDF文件被创建
        var outputFiles = Directory.GetFiles(outputDir, "*.pdf");
        outputFiles.Should().HaveCount(3, "应该生成3个PDF文件");
        
        // 验证每个PDF文件的内容
        foreach (var outputFile in outputFiles)
        {
            File.Exists(outputFile).Should().BeTrue();
            var pdfContent = await File.ReadAllTextAsync(outputFile);
            pdfContent.Should().Contain("Mock PDF content");
        }
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_WithTemplateManagement_ReturnsSuccess()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("template_test.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "template_exam.pdf");
        
        var templateListCommand = _serviceProvider.GetRequiredService<TemplateListCommand>();
        var generateCommand = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var templateParams = new TemplateParameters();
        
        var generateParams = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "模板测试试卷",
            Subject = "人工智能",
            ExamTime = "90分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act - 先列出模板，再生成
        var templateExitCode = await templateListCommand.ExecuteAsync(context, templateParams);
        var generateExitCode = await generateCommand.ExecuteAsync(context, generateParams);

        // Assert
        templateExitCode.Should().Be(0, "模板列表命令应该成功");
        generateExitCode.Should().Be(0, "生成命令应该成功");
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证PDF文件内容
        var pdfContent = await File.ReadAllTextAsync(outputPdfFile);
        pdfContent.Should().Contain("Mock PDF content");
        pdfContent.Should().Contain("模板测试试卷");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_Performance_MeasuresExecutionTime()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("performance_test.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "performance_exam.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var parameters = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "性能测试试卷",
            Subject = "性能测试",
            ExamTime = "60分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var startTime = DateTime.Now;
        var exitCode = await command.ExecuteAsync(context, parameters);
        var endTime = DateTime.Now;
        var executionTime = endTime - startTime;

        // Assert
        exitCode.Should().Be(0, "性能测试应该成功");
        
        // 验证执行时间在合理范围内
        executionTime.Should().BeLessThan(TimeSpan.FromSeconds(10), "执行时间应该少于10秒");
        executionTime.Should().BeGreaterThan(TimeSpan.Zero, "执行时间应该大于0");
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证文件大小合理
        var fileInfo = new FileInfo(outputPdfFile);
        fileInfo.Length.Should().BeGreaterThan(0, "文件大小应该大于0");
        fileInfo.Length.Should().BeLessThan(1024 * 1024, "文件大小应该小于1MB");
    }

    [TestMethod]
    public async Task CompleteExcelToPdfWorkflow_ResourceCleanup_WorksCorrectly()
    {
        // Arrange
        var testExcelFile = CreateTestExcelFile("cleanup_test.xlsx");
        var outputPdfFile = Path.Combine(TestTempPath, "cleanup_exam.pdf");
        
        var command = _serviceProvider.GetRequiredService<ExcelGenerateCommand>();
        var context = new CommandContext(_testConsole, Array.Empty<string>(), "generate excel");
        
        var parameters = new CliCommandParameters
        {
            InputFile = testExcelFile,
            OutputFile = outputPdfFile,
            Title = "清理测试试卷",
            Subject = "清理测试",
            ExamTime = "60分钟",
            Template = "standard",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var exitCode = await command.ExecuteAsync(context, parameters);

        // Assert
        exitCode.Should().Be(0, "清理测试应该成功");
        
        // 验证PDF文件被创建
        File.Exists(outputPdfFile).Should().BeTrue();
        
        // 验证临时文件被清理（如果有的话）
        var tempDir = Path.Combine(TestTempPath, "Temp");
        if (Directory.Exists(tempDir))
        {
            var tempFiles = Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories);
            tempFiles.Should().BeEmpty("临时文件应该被清理");
        }
    }
}