using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Infrastructure.Engines;
using QuizForge.Core.ContentGeneration;
using QuizForge.Services;
using System.IO.Abstractions;

namespace QuizForge.CLI.Tests.Fixtures;

/// <summary>
/// 测试夹具基类，提供共享的测试服务和配置
/// </summary>
public class TestBase
{
    protected IServiceProvider ServiceProvider { get; }
    protected IConfiguration Configuration { get; }
    protected IFileSystem FileSystem { get; }
    protected string TestTempPath { get; }

    public TestBase()
    {
        // 创建测试临时目录
        TestTempPath = Path.Combine(Path.GetTempPath(), $"QuizForge_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TestTempPath);

        // 设置文件系统（使用测试文件系统）
        FileSystem = new FileSystem();

        // 创建配置
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: true)
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

        // 创建服务容器
        var services = new ServiceCollection();
        
        // 注册配置
        services.AddSingleton(Configuration);

        // 注册文件系统
        services.AddSingleton(FileSystem);

        // 注册基础设施服务（使用Mock）
        services.AddSingleton<IExcelParser, MockExcelParser>();
        services.AddSingleton<IMarkdownParser, MockMarkdownParser>();
        services.AddSingleton<IPdfEngine, MockPdfEngine>();

        // 注册CLI服务
        services.AddSingleton<ICliFileService, CliFileService>();
        services.AddSingleton<ICliProgressService, MockCliProgressService>();
        services.AddSingleton<ICliConfigurationService, CliConfigurationService>();
        services.AddSingleton<ICliGenerationService, CliGenerationService>();
        services.AddSingleton<ICliValidationService, CliValidationService>();

        // 注册核心组件
        services.AddSingleton<LaTeXDocumentGenerator>();
        services.AddSingleton<ContentGenerator>();

        // 注册QuizForge服务
        services.AddSingleton<IQuestionService, MockQuestionService>();
        services.AddSingleton<ITemplateService, MockTemplateService>();
        services.AddSingleton<IGenerationService, MockGenerationService>();
        services.AddSingleton<IExportService, MockExportService>();

        // 配置选项
        services.Configure<LaTeXOptions>(Configuration.GetSection("LaTeX"));
        services.Configure<ExcelOptions>(Configuration.GetSection("Excel"));
        services.Configure<PdfOptions>(Configuration.GetSection("PDF"));
        services.Configure<TemplateOptions>(Configuration.GetSection("Templates"));
        services.Configure<CliOptions>(Configuration.GetSection("CLI"));

        // 注册日志服务
        services.AddLogging(builder => 
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 清理测试资源
    /// </summary>
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(TestTempPath))
            {
                Directory.Delete(TestTempPath, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

    /// <summary>
    /// 获取测试服务
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// 创建测试用的Excel文件
    /// </summary>
    public string CreateTestExcelFile(string fileName, int questionCount = 10)
    {
        var filePath = Path.Combine(TestTempPath, fileName);
        // 创建模拟Excel文件
        File.WriteAllText(filePath, "Mock Excel Content");
        return filePath;
    }

    /// <summary>
    /// 创建测试用的Markdown文件
    /// </summary>
    public string CreateTestMarkdownFile(string fileName, int questionCount = 10)
    {
        var filePath = Path.Combine(TestTempPath, fileName);
        // 创建模拟Markdown文件
        var content = @"# 测试题库

## 选择题
1. 这是一个测试题目
   - A. 选项A
   - B. 选项B
   - C. 选项C
   - D. 选项D
   
   答案：B

## 填空题
2. 这是一个填空题，答案是：_______

   答案：测试答案
";
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// 获取测试参数
    /// </summary>
    public CliCommandParameters GetTestParameters(string inputFile, string? outputFile = null)
    {
        return new CliCommandParameters
        {
            InputFile = inputFile,
            OutputFile = outputFile ?? Path.Combine(TestTempPath, "output.pdf"),
            Template = "standard",
            Title = "测试试卷",
            Subject = "测试科目",
            ExamTime = "60分钟",
            Validate = true,
            Verbose = true,
            ShowProgress = false
        };
    }

    /// <summary>
    /// 获取批量测试参数
    /// </summary>
    public BatchParameters GetBatchTestParameters(string inputDir, string outputDir)
    {
        return new BatchParameters
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
    }
}

/// <summary>
/// 测试数据生成器
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// 生成测试题目
    /// </summary>
    public static Question GenerateTestQuestion(int index, QuestionType type = QuestionType.SingleChoice)
    {
        return new Question
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = $"测试题目 {index}",
            Options = type switch
            {
                QuestionType.SingleChoice => new List<string> { "选项A", "选项B", "选项C", "选项D" },
                QuestionType.MultipleChoice => new List<string> { "选项A", "选项B", "选项C", "选项D" },
                QuestionType.TrueFalse => new List<string> { "正确", "错误" },
                _ => new List<string>()
            },
            CorrectAnswer = type switch
            {
                QuestionType.SingleChoice => "选项B",
                QuestionType.MultipleChoice => "选项A,选项C",
                QuestionType.TrueFalse => "正确",
                QuestionType.FillInBlank => "测试答案",
                _ => "答案"
            },
            Difficulty = QuestionDifficulty.Medium,
            Category = "测试分类",
            Points = 5,
            Explanation = $"这是第{index}题的解析",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// 生成测试题库
    /// </summary>
    public static QuestionBank GenerateTestQuestionBank(int questionCount = 10)
    {
        var questions = new List<Question>();
        for (int i = 1; i <= questionCount; i++)
        {
            questions.Add(GenerateTestQuestion(i));
        }

        return new QuestionBank
        {
            Id = Guid.NewGuid(),
            Name = "测试题库",
            Description = "用于测试的题库",
            Subject = "测试科目",
            Difficulty = QuestionDifficulty.Medium,
            QuestionCount = questionCount,
            TotalPoints = questionCount * 5,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Questions = questions
        };
    }

    /// <summary>
    /// 生成测试结果
    /// </summary>
    public static GenerationResult GenerateTestResult(bool success = true)
    {
        return new GenerationResult
        {
            Success = success,
            OutputPath = success ? Path.Combine(Path.GetTempPath(), "test.pdf") : null,
            QuestionCount = success ? 10 : 0,
            FileSize = success ? 1024 : 0,
            ProcessingTime = TimeSpan.FromSeconds(2),
            ErrorMessage = success ? null : "测试错误消息"
        };
    }
}