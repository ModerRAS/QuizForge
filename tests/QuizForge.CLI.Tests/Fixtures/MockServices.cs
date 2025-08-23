using QuizForge.Models.Interfaces;
using QuizForge.Models;
using QuizForge.Infrastructure.Engines;
using QuizForge.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuizForge.CLI.Tests.Fixtures;

/// <summary>
/// Mock Excel解析器
/// </summary>
public class MockExcelParser : IExcelParser
{
    private readonly ILogger<MockExcelParser> _logger;

    public MockExcelParser(ILogger<MockExcelParser> logger)
    {
        _logger = logger;
    }

    public async Task<QuestionBank> ParseAsync(string filePath)
    {
        _logger.LogInformation("Mock Excel解析器开始解析文件: {FilePath}", filePath);
        
        // 模拟解析延迟
        await Task.Delay(50);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("测试文件不存在", filePath);
        }

        // 生成测试题库
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        questionBank.Name = Path.GetFileNameWithoutExtension(filePath);
        
        _logger.LogInformation("Mock Excel解析器解析完成，共 {Count} 道题目", questionBank.Questions.Count);
        return questionBank;
    }

    public async Task<bool> ValidateFormatAsync(string filePath)
    {
        _logger.LogInformation("Mock Excel解析器验证格式: {FilePath}", filePath);
        await Task.Delay(10);
        
        // 模拟格式验证
        return File.Exists(filePath) && Path.GetExtension(filePath).ToLowerInvariant() is ".xlsx" or ".xls";
    }

    public async Task<List<string>> GetSupportedFormatsAsync()
    {
        await Task.CompletedTask;
        return new List<string> { ".xlsx", ".xls" };
    }
}

/// <summary>
/// Mock Markdown解析器
/// </summary>
public class MockMarkdownParser : IMarkdownParser
{
    private readonly ILogger<MockMarkdownParser> _logger;

    public MockMarkdownParser(ILogger<MockMarkdownParser> logger)
    {
        _logger = logger;
    }

    public async Task<QuestionBank> ParseAsync(string filePath)
    {
        _logger.LogInformation("Mock Markdown解析器开始解析文件: {FilePath}", filePath);
        
        await Task.Delay(50);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("测试文件不存在", filePath);
        }

        // 生成测试题库
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(8);
        questionBank.Name = Path.GetFileNameWithoutExtension(filePath);
        
        _logger.LogInformation("Mock Markdown解析器解析完成，共 {Count} 道题目", questionBank.Questions.Count);
        return questionBank;
    }

    public async Task<bool> ValidateFormatAsync(string filePath)
    {
        _logger.LogInformation("Mock Markdown解析器验证格式: {FilePath}", filePath);
        await Task.Delay(10);
        
        return File.Exists(filePath) && Path.GetExtension(filePath).ToLowerInvariant() == ".md";
    }

    public async Task<List<string>> GetSupportedFormatsAsync()
    {
        await Task.CompletedTask;
        return new List<string> { ".md", ".markdown" };
    }
}

/// <summary>
/// Mock PDF引擎
/// </summary>
public class MockPdfEngine : IPdfEngine
{
    private readonly ILogger<MockPdfEngine> _logger;
    private readonly PdfOptions _options;

    public MockPdfEngine(ILogger<MockPdfEngine> logger, IOptions<PdfOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> GenerateFromLatexAsync(string latexContent, string outputPath)
    {
        _logger.LogInformation("Mock PDF引擎生成PDF: {OutputPath}", outputPath);
        
        await Task.Delay(100);

        try
        {
            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 创建模拟PDF文件
            await File.WriteAllTextAsync(outputPath, "Mock PDF Content: " + latexContent.Length + " characters");
            
            _logger.LogInformation("Mock PDF生成成功: {OutputPath}", outputPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mock PDF生成失败: {OutputPath}", outputPath);
            return false;
        }
    }

    public async Task<byte[]?> GeneratePreviewAsync(string pdfPath)
    {
        _logger.LogInformation("Mock PDF引擎生成预览: {PdfPath}", pdfPath);
        
        await Task.Delay(50);

        if (!File.Exists(pdfPath))
        {
            return null;
        }

        // 生成模拟预览数据
        return new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
    }

    public async Task<bool> ValidateLatexSyntaxAsync(string latexContent)
    {
        _logger.LogInformation("Mock PDF引擎验证LaTeX语法");
        await Task.Delay(10);
        
        // 简单的语法验证
        return !string.IsNullOrWhiteSpace(latexContent) && 
               latexContent.Contains("\\documentclass") && 
               latexContent.Contains("\\begin{document}") && 
               latexContent.Contains("\\end{document}");
    }

    public async Task<List<string>> GetSupportedFormatsAsync()
    {
        await Task.CompletedTask;
        return new List<string> { ".pdf" };
    }
}

/// <summary>
/// Mock 题库服务
/// </summary>
public class MockQuestionService : IQuestionService
{
    private readonly ILogger<MockQuestionService> _logger;

    public MockQuestionService(ILogger<MockQuestionService> logger)
    {
        _logger = logger;
    }

    public async Task<Question?> GetQuestionAsync(Guid id)
    {
        await Task.Delay(10);
        return TestDataGenerator.GenerateTestQuestion(1);
    }

    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        await Task.Delay(20);
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(10);
        return questionBank.Questions;
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        await Task.Delay(10);
        question.Id = Guid.NewGuid();
        question.CreatedAt = DateTime.Now;
        return question;
    }

    public async Task<Question> UpdateQuestionAsync(Question question)
    {
        await Task.Delay(10);
        question.UpdatedAt = DateTime.Now;
        return question;
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        await Task.Delay(10);
        return true;
    }

    public async Task<List<Question>> GetQuestionsByCategoryAsync(string category)
    {
        await Task.Delay(15);
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(5);
        return questionBank.Questions;
    }

    public async Task<List<Question>> GetQuestionsByDifficultyAsync(QuestionDifficulty difficulty)
    {
        await Task.Delay(15);
        var questionBank = TestDataGenerator.GenerateTestQuestionBank(5);
        return questionBank.Questions;
    }

    public async Task<QuestionBank> ImportFromExcelAsync(string filePath)
    {
        _logger.LogInformation("Mock QuestionService从Excel导入: {FilePath}", filePath);
        await Task.Delay(50);
        return TestDataGenerator.GenerateTestQuestionBank(10);
    }

    public async Task<QuestionBank> ImportFromMarkdownAsync(string filePath)
    {
        _logger.LogInformation("Mock QuestionService从Markdown导入: {FilePath}", filePath);
        await Task.Delay(50);
        return TestDataGenerator.GenerateTestQuestionBank(8);
    }
}

/// <summary>
/// Mock 模板服务
/// </summary>
public class MockTemplateService : ITemplateService
{
    private readonly ILogger<MockTemplateService> _logger;

    public MockTemplateService(ILogger<MockTemplateService> logger)
    {
        _logger = logger;
    }

    public async Task<ExamTemplate?> GetTemplateAsync(Guid id)
    {
        await Task.Delay(10);
        return new ExamTemplate
        {
            Id = id,
            Name = "Mock Template",
            FilePath = "/mock/template.tex",
            Description = "Mock template for testing"
        };
    }

    public async Task<List<ExamTemplate>> GetAllTemplatesAsync()
    {
        await Task.Delay(20);
        return new List<ExamTemplate>
        {
            new ExamTemplate { Id = Guid.NewGuid(), Name = "Standard", FilePath = "/standard.tex" },
            new ExamTemplate { Id = Guid.NewGuid(), Name = "Advanced", FilePath = "/advanced.tex" }
        };
    }

    public async Task<ExamTemplate> CreateTemplateAsync(ExamTemplate template)
    {
        await Task.Delay(10);
        template.Id = Guid.NewGuid();
        return template;
    }

    public async Task<ExamTemplate> UpdateTemplateAsync(ExamTemplate template)
    {
        await Task.Delay(10);
        return template;
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        await Task.Delay(10);
        return true;
    }

    public async Task<string> GetTemplateContentAsync(Guid id)
    {
        await Task.Delay(10);
        return "\\documentclass{article}\n\\begin{document}\nMock template content\n\\end{document}";
    }

    public async Task<bool> ValidateTemplateAsync(string content)
    {
        await Task.Delay(10);
        return !string.IsNullOrWhiteSpace(content);
    }
}

/// <summary>
/// Mock 生成服务
/// </summary>
public class MockGenerationService : IGenerationService
{
    private readonly ILogger<MockGenerationService> _logger;

    public MockGenerationService(ILogger<MockGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<ExamPaper> GenerateExamPaperAsync(ExamPaperRequest request)
    {
        _logger.LogInformation("Mock GenerationService生成试卷");
        await Task.Delay(100);
        
        return new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = request.Title ?? "Mock Exam Paper",
            Subject = request.Subject ?? "Mock Subject",
            QuestionCount = request.QuestionCount,
            TotalPoints = request.QuestionCount * 5,
            CreatedAt = DateTime.Now
        };
    }

    public async Task<ExamPaper> GenerateRandomExamPaperAsync(RandomExamPaperRequest request)
    {
        await Task.Delay(100);
        return new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = "Random Mock Exam Paper",
            QuestionCount = request.QuestionCount,
            TotalPoints = request.QuestionCount * 5,
            CreatedAt = DateTime.Now
        };
    }

    public async Task<byte[]> GeneratePdfAsync(ExamPaper examPaper, string templatePath)
    {
        _logger.LogInformation("Mock GenerationService生成PDF");
        await Task.Delay(150);
        
        return System.Text.Encoding.UTF8.GetBytes("Mock PDF content for " + examPaper.Title);
    }

    public async Task<ExamPaper> GetExamPaperAsync(Guid id)
    {
        await Task.Delay(10);
        return new ExamPaper
        {
            Id = id,
            Title = "Mock Exam Paper",
            CreatedAt = DateTime.Now
        };
    }

    public async Task<List<ExamPaper>> GetAllExamPapersAsync()
    {
        await Task.Delay(20);
        return new List<ExamPaper>
        {
            new ExamPaper { Id = Guid.NewGuid(), Title = "Paper 1", CreatedAt = DateTime.Now },
            new ExamPaper { Id = Guid.NewGuid(), Title = "Paper 2", CreatedAt = DateTime.Now }
        };
    }

    public async Task<bool> DeleteExamPaperAsync(Guid id)
    {
        await Task.Delay(10);
        return true;
    }
}

/// <summary>
/// Mock 导出服务
/// </summary>
public class MockExportService : IExportService
{
    private readonly ILogger<MockExportService> _logger;

    public MockExportService(ILogger<MockExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportToPdfAsync(ExamPaper examPaper)
    {
        _logger.LogInformation("Mock ExportService导出PDF");
        await Task.Delay(50);
        
        return System.Text.Encoding.UTF8.GetBytes("Mock PDF content for " + examPaper.Title);
    }

    public async Task<byte[]> ExportToWordAsync(ExamPaper examPaper)
    {
        await Task.Delay(50);
        return System.Text.Encoding.UTF8.GetBytes("Mock Word content for " + examPaper.Title);
    }

    public async Task<byte[]> ExportToExcelAsync(List<Question> questions)
    {
        await Task.Delay(50);
        return System.Text.Encoding.UTF8.GetBytes("Mock Excel content");
    }

    public async Task<byte[]> ExportToJsonAsync(ExamPaper examPaper)
    {
        await Task.Delay(20);
        return System.Text.Encoding.UTF8.GetBytes("{\"title\": \"" + examPaper.Title + "\"}");
    }

    public async Task<string> ExportToLatexAsync(ExamPaper examPaper)
    {
        await Task.Delay(30);
        return "\\documentclass{article}\n\\title{" + examPaper.Title + "}\n\\begin{document}\nMock LaTeX content\n\\end{document}";
    }
}

/// <summary>
/// Mock CLI进度服务
/// </summary>
public class MockCliProgressService : ICliProgressService
{
    private readonly ILogger<MockCliProgressService> _logger;

    public MockCliProgressService(ILogger<MockCliProgressService> logger)
    {
        _logger = logger;
    }

    public Task ShowTitle(string title)
    {
        _logger.LogInformation("Mock Progress: {Title}", title);
        return Task.CompletedTask;
    }

    public Task ShowInfo(string message)
    {
        _logger.LogInformation("Mock Info: {Message}", message);
        return Task.CompletedTask;
    }

    public Task ShowSuccess(string message)
    {
        _logger.LogInformation("Mock Success: {Message}", message);
        return Task.CompletedTask;
    }

    public Task ShowWarning(string message)
    {
        _logger.LogWarning("Mock Warning: {Message}", message);
        return Task.CompletedTask;
    }

    public Task ShowError(string message)
    {
        _logger.LogError("Mock Error: {Message}", message);
        return Task.CompletedTask;
    }

    public Task ShowValidationResult(ValidationResult result)
    {
        _logger.LogInformation("Mock Validation: {IsValid} - {Errors}", result.IsValid, string.Join(", ", result.Errors));
        return Task.CompletedTask;
    }

    public async Task<IDisposable> StartProgressAsync(int totalSteps, string description)
    {
        _logger.LogInformation("Mock Progress Start: {Steps} - {Description}", totalSteps, description);
        await Task.Delay(10);
        return new MockProgressDisposable(this);
    }

    public void UpdateProgress(int currentStep, string description)
    {
        _logger.LogInformation("Mock Progress Update: {Step} - {Description}", currentStep, description);
    }

    public Task CompleteProgressAsync()
    {
        _logger.LogInformation("Mock Progress Complete");
        return Task.CompletedTask;
    }

    private class MockProgressDisposable : IDisposable
    {
        private readonly MockCliProgressService _service;

        public MockProgressDisposable(MockCliProgressService service)
        {
            _service = service;
        }

        public void Dispose()
        {
            _service._logger.LogInformation("Mock Progress Disposed");
        }
    }
}