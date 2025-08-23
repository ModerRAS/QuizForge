using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Integration.ServiceIntegration;

[TestClass]
public class ServiceIntegrationTests : TestBase
{
    private IHost _host = null!;
    private IServiceProvider _serviceProvider = null!;

    [TestInitialize]
    public void Setup()
    {
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
    public async Task CliGenerationService_IntegrationWithFileService_WorksCorrectly()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
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
        var result = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.QuestionCount.Should().BeGreaterThan(0);
        result.OutputPath.Should().Be(outputFile);
        result.FileSize.Should().BeGreaterThan(0);
        
        // 验证文件服务集成
        var fileInfo = await fileService.GetFileInfoAsync(outputFile);
        fileInfo.Should().NotBeNull();
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().Be(result.FileSize);
    }

    [TestMethod]
    public async Task CliGenerationService_IntegrationWithValidationService_WorksCorrectly()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var validationService = _serviceProvider.GetRequiredService<ICliValidationService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
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
        var validationResult = await validationService.ValidateFilePath(testFile, "Excel文件");
        var generationResult = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        generationResult.Success.Should().BeTrue();
        generationResult.QuestionCount.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task CliGenerationService_IntegrationWithConfigurationService_WorksCorrectly()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
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
        var templateDir = configService.GetValue<string>("Templates:Directory", "./templates");
        var result = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        templateDir.Should().NotBeNullOrEmpty();
        result.Success.Should().BeTrue();
        result.QuestionCount.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task CliFileService_IntegrationWithConfigurationService_WorksCorrectly()
    {
        // Arrange
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output", "test.pdf");
        
        // Act
        var validationResult = await fileService.ValidateFileAsync(testFile);
        var directoryCreated = await fileService.EnsureDirectoryExistsAsync(Path.GetDirectoryName(outputFile)!);
        var copyResult = await fileService.CopyFileAsync(testFile, outputFile);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        directoryCreated.Should().BeTrue();
        copyResult.Should().BeTrue();
        
        // 验证文件被正确复制
        File.Exists(outputFile).Should().BeTrue();
    }

    [TestMethod]
    public async Task CliValidationService_IntegrationWithFileService_WorksCorrectly()
    {
        // Arrange
        var validationService = _serviceProvider.GetRequiredService<ICliValidationService>();
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        // Act
        var filePathValidation = await validationService.ValidateFilePath(testFile, "Excel文件");
        var outputPathValidation = await validationService.ValidateOutputPath(outputFile);
        var fileValidation = await fileService.ValidateFileAsync(testFile);

        // Assert
        filePathValidation.IsValid.Should().BeTrue();
        outputPathValidation.IsValid.Should().BeTrue();
        fileValidation.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task BatchProcessing_IntegrationWithAllServices_WorksCorrectly()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var validationService = _serviceProvider.GetRequiredService<ICliValidationService>();
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        
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
        var directoryValidation = await validationService.ValidateOutputPath(outputDir);
        var files = await fileService.GetFilesAsync(inputDir, "*.*");
        var batchResult = await generationService.BatchGenerateAsync(parameters);

        // Assert
        directoryValidation.IsValid.Should().BeTrue();
        files.Should().HaveCount(3);
        batchResult.Should().NotBeNull();
        batchResult.TotalFiles.Should().Be(3);
        batchResult.SuccessCount.Should().Be(3);
        batchResult.FailureCount.Should().Be(0);
        
        // 验证输出文件被创建
        Directory.Exists(outputDir).Should().BeTrue();
        var outputFiles = Directory.GetFiles(outputDir, "*.pdf");
        outputFiles.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task ServiceIntegration_ErrorHandling_PropagatesCorrectly()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var validationService = _serviceProvider.GetRequiredService<ICliValidationService>();
        
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
        var parameters = new CliCommandParameters
        {
            InputFile = nonExistentFile,
            OutputFile = outputFile,
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };

        // Act
        var filePathValidation = await validationService.ValidateFilePath(nonExistentFile, "Excel文件");
        var fileValidation = await fileService.ValidateFileAsync(nonExistentFile);
        var generationResult = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        filePathValidation.IsValid.Should().BeFalse();
        fileValidation.IsValid.Should().BeFalse();
        generationResult.Success.Should().BeFalse();
        generationResult.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public async Task ServiceIntegration_ConfigurationChanges_AreReflected()
    {
        // Arrange
        var configService = _serviceProvider.GetRequiredService<ICliConfigurationService>();
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
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
        var originalTemplate = configService.GetValue<string>("Templates:DefaultTemplate", "standard.tex");
        var setResult = await configService.SetValueAsync("Templates:DefaultTemplate", "advanced.tex");
        var newTemplate = configService.GetValue<string>("Templates:DefaultTemplate", "standard.tex");
        var result = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        setResult.Should().BeTrue();
        newTemplate.Should().Be("advanced.tex");
        result.Success.Should().BeTrue();
    }

    [TestMethod]
    public async Task ServiceIntegration_MultipleOperations_MaintainState()
    {
        // Arrange
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var validationService = _serviceProvider.GetRequiredService<ICliValidationService>();
        
        var testFile = CreateTestExcelFile("test.xlsx");
        var outputFile = Path.Combine(TestTempPath, "output.pdf");
        
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

        // Act - 执行多个操作
        var validation1 = await validationService.ValidateFilePath(testFile, "Excel文件");
        var fileValidation = await fileService.ValidateFileAsync(testFile);
        var result1 = await generationService.GenerateFromExcelAsync(parameters);
        
        var validation2 = await validationService.ValidateOutputPath(outputFile);
        var fileInfo = await fileService.GetFileInfoAsync(outputFile);
        var result2 = await generationService.GenerateFromExcelAsync(parameters);

        // Assert
        validation1.IsValid.Should().BeTrue();
        fileValidation.IsValid.Should().BeTrue();
        result1.Success.Should().BeTrue();
        validation2.IsValid.Should().BeTrue();
        fileInfo.Should().NotBeNull();
        result2.Success.Should().BeTrue();
        
        // 验证状态保持一致
        result1.OutputPath.Should().Be(result2.OutputPath);
        result1.QuestionCount.Should().Be(result2.QuestionCount);
    }

    [TestMethod]
    public async Task ServiceIntegration_ResourceCleanup_WorksCorrectly()
    {
        // Arrange
        var fileService = _serviceProvider.GetRequiredService<ICliFileService>();
        var generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        
        var tempDir = Path.Combine(TestTempPath, "temp");
        Directory.CreateDirectory(tempDir);
        
        // 创建临时文件
        var tempFiles = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var tempFile = Path.Combine(tempDir, $"temp{i}.tmp");
            await File.WriteAllTextAsync(tempFile, $"Temporary content {i}");
            tempFiles.Add(tempFile);
        }
        
        // 修改文件时间为2小时前
        foreach (var tempFile in tempFiles)
        {
            var fileInfo = new FileInfo(tempFile);
            fileInfo.LastWriteTime = DateTime.Now.AddHours(-2);
        }

        // Act
        var cleanupCount = await fileService.CleanupTempFilesAsync(tempDir, TimeSpan.FromHours(1));
        var remainingFiles = Directory.GetFiles(tempDir, "*.tmp");

        // Assert
        cleanupCount.Should().Be(5);
        remainingFiles.Should().BeEmpty();
    }
}