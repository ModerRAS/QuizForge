using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Performance;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
[HtmlExporter]
[CsvExporter]
[JsonExporter]
public class BatchProcessingPerformanceTests : TestBase
{
    private IServiceProvider _serviceProvider = null!;
    private ICliGenerationService _generationService = null!;
    private ICliFileService _fileService = null!;
    private string _testInputDir = null!;
    private string _testOutputDir = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // 创建测试主机
        var host = Host.CreateDefaultBuilder()
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

        _serviceProvider = host.Services;
        _generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();
        _fileService = _serviceProvider.GetRequiredService<ICliFileService>();

        // 创建测试目录
        _testInputDir = Path.Combine(TestTempPath, "Performance_Input");
        _testOutputDir = Path.Combine(TestTempPath, "Performance_Output");
        Directory.CreateDirectory(_testInputDir);
        Directory.CreateDirectory(_testOutputDir);

        // 创建测试文件
        CreateTestFiles();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // 清理测试文件
        if (Directory.Exists(_testInputDir))
        {
            Directory.Delete(_testInputDir, true);
        }
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }

    private void CreateTestFiles()
    {
        // 创建小文件测试集
        for (int i = 0; i < 10; i++)
        {
            CreateTestExcelFile(Path.Combine(_testInputDir, $"small_{i}.xlsx"), 5);
        }

        // 创建中等文件测试集
        for (int i = 0; i < 5; i++)
        {
            CreateTestExcelFile(Path.Combine(_testInputDir, $"medium_{i}.xlsx"), 20);
        }

        // 创建大文件测试集
        for (int i = 0; i < 2; i++)
        {
            CreateTestExcelFile(Path.Combine(_testInputDir, $"large_{i}.xlsx"), 50);
        }
    }

    [Benchmark]
    public async Task BatchProcess_SmallFiles_Parallel2()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "small_*.xlsx",
            MaxParallel = 2,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_SmallFiles_Parallel4()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "small_*.xlsx",
            MaxParallel = 4,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_SmallFiles_Parallel8()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "small_*.xlsx",
            MaxParallel = 8,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_MediumFiles_Parallel2()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "medium_*.xlsx",
            MaxParallel = 2,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_MediumFiles_Parallel4()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "medium_*.xlsx",
            MaxParallel = 4,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_LargeFiles_Parallel2()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "large_*.xlsx",
            MaxParallel = 2,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task BatchProcess_AllFiles_Parallel4()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "*.xlsx",
            MaxParallel = 4,
            Template = "standard",
            Title = "性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task SingleFile_Generation_Excel()
    {
        var testFile = Path.Combine(_testInputDir, "small_0.xlsx");
        var outputFile = Path.Combine(_testOutputDir, $"single_{Guid.NewGuid()}.pdf");
        
        var parameters = new CliCommandParameters
        {
            InputFile = testFile,
            OutputFile = outputFile,
            Template = "standard",
            Title = "单文件性能测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ShowProgress = false
        };

        await _generationService.GenerateFromExcelAsync(parameters);
    }

    [Benchmark]
    public async Task FileService_GetFiles_Performance()
    {
        await _fileService.GetFilesAsync(_testInputDir, "*.xlsx");
    }

    [Benchmark]
    public async Task FileService_ValidateFiles_Performance()
    {
        var files = await _fileService.GetFilesAsync(_testInputDir, "*.xlsx");
        foreach (var file in files)
        {
            await _fileService.ValidateFileAsync(file.FullName);
        }
    }

    [Benchmark]
    public async Task GenerationService_ValidateExcel_Performance()
    {
        var files = await _fileService.GetFilesAsync(_testInputDir, "*.xlsx");
        foreach (var file in files)
        {
            await _generationService.ValidateExcelAsync(file.FullName);
        }
    }
}

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
public class MemoryUsagePerformanceTests : TestBase
{
    private IServiceProvider _serviceProvider = null!;
    private ICliGenerationService _generationService = null!;
    private string _testInputDir = null!;
    private string _testOutputDir = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // 创建测试主机
        var host = Host.CreateDefaultBuilder()
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

        _serviceProvider = host.Services;
        _generationService = _serviceProvider.GetRequiredService<ICliGenerationService>();

        // 创建测试目录
        _testInputDir = Path.Combine(TestTempPath, "Memory_Input");
        _testOutputDir = Path.Combine(TestTempPath, "Memory_Output");
        Directory.CreateDirectory(_testInputDir);
        Directory.CreateDirectory(_testOutputDir);

        // 创建测试文件
        for (int i = 0; i < 50; i++)
        {
            CreateTestExcelFile(Path.Combine(_testInputDir, $"memory_test_{i}.xlsx"), 10);
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // 清理测试文件
        if (Directory.Exists(_testInputDir))
        {
            Directory.Delete(_testInputDir, true);
        }
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }

    [Benchmark]
    public async Task MemoryUsage_BatchProcess_50Files()
    {
        var parameters = new BatchParameters
        {
            InputDirectory = _testInputDir,
            OutputDirectory = _testOutputDir,
            FilePattern = "*.xlsx",
            MaxParallel = 4,
            Template = "standard",
            Title = "内存测试",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = false,
            Verbose = false,
            ContinueOnError = true
        };

        await _generationService.BatchGenerateAsync(parameters);
    }

    [Benchmark]
    public async Task MemoryUsage_SequentialProcessing_50Files()
    {
        var files = Directory.GetFiles(_testInputDir, "*.xlsx");
        
        foreach (var file in files)
        {
            var outputFile = Path.Combine(_testOutputDir, Path.ChangeExtension(Path.GetFileName(file), ".pdf"));
            
            var parameters = new CliCommandParameters
            {
                InputFile = file,
                OutputFile = outputFile,
                Template = "standard",
                Title = "内存测试",
                Subject = "测试科目",
                ExamTime = "60分钟",
                Validate = false,
                Verbose = false,
                ShowProgress = false
            };

            await _generationService.GenerateFromExcelAsync(parameters);
        }
    }
}

public class PerformanceTestRunner
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<BatchProcessingPerformanceTests>();
        var memorySummary = BenchmarkRunner.Run<MemoryUsagePerformanceTests>();
        
        Console.WriteLine("Batch Processing Performance Results:");
        Console.WriteLine(summary);
        
        Console.WriteLine("Memory Usage Performance Results:");
        Console.WriteLine(memorySummary);
    }
}