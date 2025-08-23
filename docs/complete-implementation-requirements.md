# QuizForge CLI 完整功能实现要求

## 概述

本文档定义了QuizForge CLI项目的完整功能实现要求，目标是消除所有硬编码返回值，确保所有业务逻辑完整实现。

## 功能完整性目标

### 主要目标
- **零硬编码返回值**: 所有方法都有完整的业务逻辑实现
- **完整业务逻辑**: 所有服务方法都实现预期功能
- **完整错误处理**: 所有错误情况都有适当处理
- **完整配置管理**: 配置系统完整实现
- **完整日志记录**: 日志系统完整实现

### 次要目标
- **完整的验证**: 所有输入验证完整实现
- **完整的监控**: 系统监控功能完整实现
- **完整的测试**: 所有功能都有对应测试
- **完整的文档**: 所有功能都有详细文档

## 服务层完整性要求

### SRV-001: QuestionService 完整性

#### 必须完整实现的方法
```csharp
public interface IQuestionService
{
    // 基础CRUD操作
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<IEnumerable<Question>> GetQuestionsByBankIdAsync(int bankId);
    Task<Question> CreateQuestionAsync(Question question);
    Task<Question> UpdateQuestionAsync(Question question);
    Task<bool> DeleteQuestionAsync(int id);
    
    // 批量操作
    Task<IEnumerable<Question>> CreateQuestionsAsync(IEnumerable<Question> questions);
    Task<bool> DeleteQuestionsAsync(IEnumerable<int> questionIds);
    
    // 查询和筛选
    Task<IEnumerable<Question>> GetQuestionsByCategoryAsync(string category);
    Task<IEnumerable<Question>> GetQuestionsByDifficultyAsync(Difficulty difficulty);
    Task<IEnumerable<Question>> GetQuestionsByTypeAsync(QuestionType type);
    
    // 统计和分析
    Task<int> GetQuestionCountAsync(int bankId);
    Task<Dictionary<Difficulty, int>> GetQuestionDistributionAsync(int bankId);
    Task<bool> ValidateQuestionBankAsync(int bankId);
}
```

#### 实现标准
```csharp
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;
    private readonly IQuestionValidator _validator;
    private readonly ILogger<QuestionService> _logger;
    
    public QuestionService(
        IQuestionRepository repository,
        IQuestionValidator validator,
        ILogger<QuestionService> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }
    
    // 完整实现示例
    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Question ID must be positive", nameof(id));
        }
        
        try
        {
            var question = await _repository.GetByIdAsync(id);
            
            if (question == null)
            {
                _logger.LogWarning("Question with ID {Id} not found", id);
                return null;
            }
            
            _logger.LogDebug("Retrieved question with ID {Id}", id);
            return question;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving question with ID {Id}", id);
            throw new QuestionServiceException($"Failed to retrieve question {id}", ex);
        }
    }
    
    // 其他方法的完整实现...
}
```

### SRV-002: TemplateService 完整性

#### 必须完整实现的方法
```csharp
public interface ITemplateService
{
    // 模板CRUD操作
    Task<Template?> GetTemplateByIdAsync(int id);
    Task<IEnumerable<Template>> GetAllTemplatesAsync();
    Task<Template> CreateTemplateAsync(Template template);
    Task<Template> UpdateTemplateAsync(Template template);
    Task<bool> DeleteTemplateAsync(int id);
    
    // 模板操作
    Task<string> GetTemplateContentAsync(int templateId);
    Task<bool> ValidateTemplateAsync(Template template);
    Task<Template> CloneTemplateAsync(int templateId, string newName);
    
    // 模板应用
    Task<string> ApplyTemplateAsync(int templateId, QuestionBank questionBank);
    Task<Dictionary<string, object>> GetTemplateVariablesAsync(int templateId);
}
```

#### 实现标准
```csharp
public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _repository;
    private readonly ITemplateValidator _validator;
    private readonly ITemplateEngine _engine;
    private readonly ILogger<TemplateService> _logger;
    
    public TemplateService(
        ITemplateRepository repository,
        ITemplateValidator validator,
        ITemplateEngine engine,
        ILogger<TemplateService> logger)
    {
        _repository = repository;
        _validator = validator;
        _engine = engine;
        _logger = logger;
    }
    
    // 完整实现示例
    public async Task<string> ApplyTemplateAsync(int templateId, QuestionBank questionBank)
    {
        if (templateId <= 0)
        {
            throw new ArgumentException("Template ID must be positive", nameof(templateId));
        }
        
        if (questionBank == null)
        {
            throw new ArgumentNullException(nameof(questionBank));
        }
        
        try
        {
            var template = await _repository.GetByIdAsync(templateId);
            
            if (template == null)
            {
                throw new TemplateNotFoundException($"Template with ID {templateId} not found");
            }
            
            var variables = await GetTemplateVariablesAsync(templateId);
            var result = await _engine.RenderAsync(template.Content, questionBank, variables);
            
            _logger.LogInformation("Applied template {TemplateId} to question bank {BankId}", 
                templateId, questionBank.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying template {TemplateId} to question bank {BankId}", 
                templateId, questionBank.Id);
            throw new TemplateServiceException($"Failed to apply template {templateId}", ex);
        }
    }
    
    // 其他方法的完整实现...
}
```

### SRV-003: GenerationService 完整性

#### 必须完整实现的方法
```csharp
public interface IGenerationService
{
    // 试卷生成
    Task<ExamPaper> GenerateExamPaperAsync(GenerationRequest request);
    Task<ExamPaper> GenerateRandomExamPaperAsync(RandomGenerationRequest request);
    Task<IEnumerable<ExamPaper>> GenerateBatchExamPapersAsync(BatchGenerationRequest request);
    
    // 生成配置
    Task<GenerationConfiguration> GetConfigurationAsync();
    Task<GenerationConfiguration> UpdateConfigurationAsync(GenerationConfiguration config);
    
    // 生成历史
    Task<IEnumerable<GenerationHistory>> GetGenerationHistoryAsync(DateTime? startDate = null);
    Task<GenerationHistory?> GetGenerationHistoryByIdAsync(int historyId);
    
    // 生成统计
    Task<GenerationStatistics> GetGenerationStatisticsAsync();
    Task<Dictionary<string, int>> GetGenerationCountByDayAsync(int days);
}
```

#### 实现标准
```csharp
public class GenerationService : IGenerationService
{
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IExamPaperRepository _paperRepository;
    private readonly IGenerationValidator _validator;
    private readonly ILogger<GenerationService> _logger;
    
    public GenerationService(
        IQuestionService questionService,
        ITemplateService templateService,
        IExamPaperRepository paperRepository,
        IGenerationValidator validator,
        ILogger<GenerationService> logger)
    {
        _questionService = questionService;
        _templateService = templateService;
        _paperRepository = paperRepository;
        _validator = validator;
        _logger = logger;
    }
    
    // 完整实现示例
    public async Task<ExamPaper> GenerateExamPaperAsync(GenerationRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        await _validator.ValidateGenerationRequestAsync(request);
        
        try
        {
            _logger.LogInformation("Starting exam paper generation for bank {BankId}", 
                request.QuestionBankId);
            
            var questions = await SelectQuestionsAsync(request);
            var templateContent = await _templateService.GetTemplateContentAsync(request.TemplateId);
            var paperContent = await GeneratePaperContentAsync(questions, templateContent, request);
            
            var examPaper = new ExamPaper
            {
                Title = request.Title ?? $"Exam Paper {DateTime.Now:yyyyMMdd}",
                QuestionBankId = request.QuestionBankId,
                TemplateId = request.TemplateId,
                Content = paperContent,
                TotalPoints = questions.Sum(q => q.Points),
                QuestionCount = questions.Count,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = request.GeneratedBy ?? "System"
            };
            
            var savedPaper = await _paperRepository.AddAsync(examPaper);
            
            await RecordGenerationHistoryAsync(savedPaper, request);
            
            _logger.LogInformation("Successfully generated exam paper {PaperId} with {Count} questions", 
                savedPaper.Id, questions.Count);
            
            return savedPaper;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating exam paper for bank {BankId}", 
                request.QuestionBankId);
            throw new GenerationServiceException("Failed to generate exam paper", ex);
        }
    }
    
    private async Task<List<Question>> SelectQuestionsAsync(GenerationRequest request)
    {
        var allQuestions = await _questionService.GetQuestionsByBankIdAsync(request.QuestionBankId);
        var availableQuestions = allQuestions.Where(q => q.IsActive).ToList();
        
        if (availableQuestions.Count < request.QuestionCount)
        {
            throw new InsufficientQuestionsException(
                $"Insufficient questions. Required: {request.QuestionCount}, Available: {availableQuestions.Count}");
        }
        
        var selectedQuestions = new List<Question>();
        
        if (request.UseRandomSelection)
        {
            selectedQuestions = SelectRandomQuestions(availableQuestions, request);
        }
        else
        {
            selectedQuestions = SelectSequentialQuestions(availableQuestions, request);
        }
        
        return selectedQuestions;
    }
    
    private List<Question> SelectRandomQuestions(List<Question> questions, GenerationRequest request)
    {
        var random = new Random();
        var shuffled = questions.OrderBy(q => random.Next()).ToList();
        
        if (request.DifficultyDistribution != null && request.DifficultyDistribution.Any())
        {
            return SelectByDifficultyDistribution(shuffled, request.DifficultyDistribution);
        }
        
        return shuffled.Take(request.QuestionCount).ToList();
    }
    
    private List<Question> SelectSequentialQuestions(List<Question> questions, GenerationRequest request)
    {
        return questions.Take(request.QuestionCount).ToList();
    }
    
    private List<Question> SelectByDifficultyDistribution(
        List<Question> questions, 
        Dictionary<Difficulty, int> distribution)
    {
        var result = new List<Question>();
        
        foreach (var (difficulty, count) in distribution)
        {
            var difficultyQuestions = questions.Where(q => q.Difficulty == difficulty).ToList();
            
            if (difficultyQuestions.Count < count)
            {
                throw new InsufficientQuestionsException(
                    $"Insufficient questions for difficulty {difficulty}. Required: {count}, Available: {difficultyQuestions.Count}");
            }
            
            result.AddRange(difficultyQuestions.Take(count));
        }
        
        return result;
    }
    
    // 其他方法的完整实现...
}
```

## CLI层完整性要求

### CLI-001: 命令处理完整性

#### 必须完整实现的命令
```csharp
// GenerateCommand.cs
public class GenerateCommand : AsyncCommand<GenerateOptions>
{
    private readonly IGenerationService _generationService;
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;
    private readonly IProgressService _progressService;
    private readonly ILogger<GenerateCommand> _logger;
    
    public GenerateCommand(
        IGenerationService generationService,
        IQuestionService questionService,
        ITemplateService templateService,
        IFileService fileService,
        IProgressService progressService,
        ILogger<GenerateCommand> logger)
    {
        _generationService = generationService;
        _questionService = questionService;
        _templateService = templateService;
        _fileService = fileService;
        _progressService = progressService;
        _logger = logger;
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateOptions options)
    {
        try
        {
            await ValidateOptionsAsync(options);
            
            _progressService.StartProgress("Validating input file...");
            var questionBank = await LoadQuestionBankAsync(options.InputFile);
            
            _progressService.UpdateProgress("Preparing generation request...");
            var request = await PrepareGenerationRequestAsync(options, questionBank);
            
            _progressService.UpdateProgress("Generating exam paper...");
            var examPaper = await _generationService.GenerateExamPaperAsync(request);
            
            _progressService.UpdateProgress("Generating PDF...");
            var pdfPath = await GeneratePdfAsync(examPaper, options.OutputFile);
            
            _progressService.CompleteProgress($"Successfully generated exam paper: {pdfPath}");
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating exam paper");
            _progressService.ShowError($"Error: {ex.Message}");
            return 1;
        }
    }
    
    private async Task ValidateOptionsAsync(GenerateOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.InputFile))
        {
            throw new ArgumentException("Input file is required", nameof(options.InputFile));
        }
        
        if (string.IsNullOrWhiteSpace(options.OutputFile))
        {
            throw new ArgumentException("Output file is required", nameof(options.OutputFile));
        }
        
        if (!File.Exists(options.InputFile))
        {
            throw new FileNotFoundException("Input file not found", options.InputFile);
        }
        
        await _fileService.ValidateFileAsync(options.InputFile);
    }
    
    private async Task<QuestionBank> LoadQuestionBankAsync(string inputFile)
    {
        var extension = Path.GetExtension(inputFile).ToLowerInvariant();
        
        return extension switch
        {
            ".xlsx" or ".xls" => await _questionService.LoadFromExcelAsync(inputFile),
            ".csv" => await _questionService.LoadFromCsvAsync(inputFile),
            _ => throw new NotSupportedException($"File format {extension} is not supported")
        };
    }
    
    private async Task<GenerationRequest> PrepareGenerationRequestAsync(
        GenerateOptions options, 
        QuestionBank questionBank)
    {
        var templateId = await GetTemplateIdAsync(options.Template);
        
        return new GenerationRequest
        {
            QuestionBankId = questionBank.Id,
            TemplateId = templateId,
            Title = options.Title,
            QuestionCount = options.QuestionCount ?? 50,
            UseRandomSelection = options.Random,
            DifficultyDistribution = await ParseDifficultyDistributionAsync(options.Difficulty),
            GeneratedBy = Environment.UserName,
            AdditionalParameters = await ParseAdditionalParametersAsync(options)
        };
    }
    
    private async Task<int> GetTemplateIdAsync(string? templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            var defaultTemplate = await _templateService.GetDefaultTemplateAsync();
            return defaultTemplate.Id;
        }
        
        var template = await _templateService.GetTemplateByNameAsync(templateName);
        return template?.Id ?? throw new TemplateNotFoundException($"Template '{templateName}' not found");
    }
    
    private async Task<string> GeneratePdfAsync(ExamPaper examPaper, string outputFile)
    {
        var pdfEngine = new LatexPdfEngine();
        await pdfEngine.GenerateFromLatexAsync(examPaper.Content, outputFile);
        return outputFile;
    }
    
    // 其他辅助方法的完整实现...
}
```

### CLI-002: 选项验证完整性

#### 完整的选项验证
```csharp
public class GenerateOptions
{
    [CommandOption("-i|--input", Description = "Input Excel file path")]
    public string InputFile { get; set; } = string.Empty;
    
    [CommandOption("-o|--output", Description = "Output PDF file path")]
    public string OutputFile { get; set; } = string.Empty;
    
    [CommandOption("-t|--template", Description = "Template name")]
    public string? Template { get; set; }
    
    [CommandOption("--title", Description = "Exam paper title")]
    public string? Title { get; set; }
    
    [CommandOption("--question-count", Description = "Number of questions")]
    public int? QuestionCount { get; set; }
    
    [CommandOption("--random", Description = "Use random selection")]
    public bool Random { get; set; }
    
    [CommandOption("--difficulty", Description = "Difficulty distribution (e.g., easy:20,medium:20,hard:10)")]
    public string? Difficulty { get; set; }
    
    [CommandOption("--verbose", Description = "Verbose output")]
    public bool Verbose { get; set; }
    
    [CommandOption("--quiet", Description = "Quiet mode")]
    public bool Quiet { get; set; }
    
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(InputFile))
        {
            result.Errors.Add("Input file is required");
        }
        
        if (string.IsNullOrWhiteSpace(OutputFile))
        {
            result.Errors.Add("Output file is required");
        }
        
        if (QuestionCount.HasValue && QuestionCount.Value <= 0)
        {
            result.Errors.Add("Question count must be positive");
        }
        
        if (!string.IsNullOrWhiteSpace(Difficulty))
        {
            var distributionResult = ValidateDifficultyDistribution(Difficulty);
            if (!distributionResult.IsValid)
            {
                result.Errors.AddRange(distributionResult.Errors);
            }
        }
        
        return result;
    }
    
    private ValidationResult ValidateDifficultyDistribution(string difficulty)
    {
        var result = new ValidationResult();
        
        try
        {
            var parts = difficulty.Split(',');
            foreach (var part in parts)
            {
                var keyValue = part.Split(':');
                if (keyValue.Length != 2)
                {
                    result.Errors.Add($"Invalid difficulty format: {part}");
                    continue;
                }
                
                if (!Enum.TryParse<Difficulty>(keyValue[0], true, out var difficultyType))
                {
                    result.Errors.Add($"Invalid difficulty type: {keyValue[0]}");
                    continue;
                }
                
                if (!int.TryParse(keyValue[1], out var count) || count <= 0)
                {
                    result.Errors.Add($"Invalid count for {difficultyType}: {keyValue[1]}");
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error parsing difficulty distribution: {ex.Message}");
        }
        
        return result;
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public bool IsValid => !Errors.Any();
}
```

## 基础设施层完整性要求

### INF-001: ExcelParser 完整性

#### 必须完整实现的功能
```csharp
public interface IExcelParser
{
    // 基础解析功能
    Task<QuestionBank> ParseAsync(string filePath);
    Task<QuestionBank> ParseAsync(Stream stream);
    
    // 验证功能
    Task<ValidationResult> ValidateFormatAsync(string filePath);
    Task<ValidationResult> ValidateFormatAsync(Stream stream);
    
    // 配置功能
    Task<ParserConfiguration> GetConfigurationAsync();
    Task<ParserConfiguration> UpdateConfigurationAsync(ParserConfiguration config);
    
    // 统计功能
    Task<ParserStatistics> GetStatisticsAsync(string filePath);
    Task<ParserStatistics> GetStatisticsAsync(Stream stream);
}

public class ExcelParser : IExcelParser
{
    private readonly IParserConfiguration _config;
    private readonly IQuestionValidator _validator;
    private readonly ILogger<ExcelParser> _logger;
    
    public ExcelParser(
        IParserConfiguration config,
        IQuestionValidator validator,
        ILogger<ExcelParser> logger)
    {
        _config = config;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<QuestionBank> ParseAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required", nameof(filePath));
        }
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found", filePath);
        }
        
        try
        {
            _logger.LogInformation("Starting Excel parsing for file: {FilePath}", filePath);
            
            using var package = new ExcelPackage(new FileInfo(filePath));
            var workbook = package.Workbook;
            
            if (workbook.Worksheets.Count == 0)
            {
                throw new InvalidExcelFormatException("Excel file contains no worksheets");
            }
            
            var worksheet = workbook.Worksheets[0];
            var questionBank = await ParseWorksheetAsync(worksheet);
            
            _logger.LogInformation("Successfully parsed {Count} questions from Excel file", 
                questionBank.Questions.Count);
            
            return questionBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel file: {FilePath}", filePath);
            throw new ExcelParserException($"Failed to parse Excel file: {filePath}", ex);
        }
    }
    
    private async Task<QuestionBank> ParseWorksheetAsync(ExcelWorksheet worksheet)
    {
        var dimension = worksheet.Dimension;
        if (dimension == null)
        {
            throw new InvalidExcelFormatException("Worksheet has no dimensions");
        }
        
        var headerRow = await ParseHeaderRowAsync(worksheet);
        var questions = new List<Question>();
        
        for (int row = dimension.Start.Row + 1; row <= dimension.End.Row; row++)
        {
            try
            {
                var question = await ParseQuestionAsync(worksheet, row, headerRow);
                if (question != null)
                {
                    questions.Add(question);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing question at row {Row}", row);
                // 继续处理其他行，不要因为单个错误而停止
            }
        }
        
        return new QuestionBank
        {
            Name = GetQuestionBankName(worksheet),
            Questions = questions,
            Format = "Excel",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private async Task<Dictionary<string, int>> ParseHeaderRowAsync(ExcelWorksheet worksheet)
    {
        var headerRow = new Dictionary<string, int>();
        var firstRow = worksheet.Dimension!.Start.Row;
        
        for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
        {
            var headerValue = worksheet.Cells[firstRow, col].Text?.Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                var normalizedHeader = NormalizeHeader(headerValue);
                headerRow[normalizedHeader] = col;
            }
        }
        
        // 验证必需的列
        var requiredColumns = new[] { "题型", "题目", "答案" };
        foreach (var requiredColumn in requiredColumns)
        {
            if (!headerRow.ContainsKey(requiredColumn))
            {
                throw new InvalidExcelFormatException($"Required column '{requiredColumn}' not found");
            }
        }
        
        return headerRow;
    }
    
    private async Task<Question?> ParseQuestionAsync(
        ExcelWorksheet worksheet, 
        int row, 
        Dictionary<string, int> headerRow)
    {
        try
        {
            var questionTypeText = worksheet.Cells[row, headerRow["题型"]].Text?.Trim();
            if (string.IsNullOrWhiteSpace(questionTypeText))
            {
                return null; // 跳过空行
            }
            
            if (!Enum.TryParse<QuestionType>(questionTypeText, true, out var questionType))
            {
                throw new InvalidQuestionFormatException($"Invalid question type: {questionTypeText}");
            }
            
            var content = worksheet.Cells[row, headerRow["题目"]].Text?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidQuestionFormatException("Question content is required");
            }
            
            var question = new Question
            {
                Type = questionType,
                Content = content,
                CorrectAnswer = worksheet.Cells[row, headerRow["答案"]].Text?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            
            // 解析可选字段
            if (headerRow.TryGetValue("选项A", out var optionACol))
            {
                question.Options = new List<string>();
                for (char c = 'A'; c <= 'D'; c++)
                {
                    var optionKey = $"选项{c}";
                    if (headerRow.TryGetValue(optionKey, out var optionCol))
                    {
                        var optionValue = worksheet.Cells[row, optionCol].Text?.Trim();
                        if (!string.IsNullOrWhiteSpace(optionValue))
                        {
                            question.Options.Add(optionValue);
                        }
                    }
                }
            }
            
            if (headerRow.TryGetValue("难度", out var difficultyCol))
            {
                var difficultyText = worksheet.Cells[row, difficultyCol].Text?.Trim();
                if (Enum.TryParse<Difficulty>(difficultyText, true, out var difficulty))
                {
                    question.Difficulty = difficulty;
                }
            }
            
            if (headerRow.TryGetValue("分值", out var pointsCol))
            {
                var pointsText = worksheet.Cells[row, pointsCol].Text?.Trim();
                if (int.TryParse(pointsText, out var points))
                {
                    question.Points = points;
                }
            }
            
            if (headerRow.TryGetValue("分类", out var categoryCol))
            {
                question.Category = worksheet.Cells[row, categoryCol].Text?.Trim();
            }
            
            if (headerRow.TryGetValue("解析", out var explanationCol))
            {
                question.Explanation = worksheet.Cells[row, explanationCol].Text?.Trim();
            }
            
            // 验证题目
            await _validator.ValidateQuestionAsync(question);
            
            return question;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing question at row {Row}", row);
            throw;
        }
    }
    
    private string NormalizeHeader(string header)
    {
        return header.Trim()
            .Replace(" ", "")
            .Replace("（", "(")
            .Replace("）", ")")
            .ToLowerInvariant();
    }
    
    private string GetQuestionBankName(ExcelWorksheet worksheet)
    {
        // 尝试从工作表名称获取题库名称
        var sheetName = worksheet.Name?.Trim();
        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            return sheetName;
        }
        
        // 尝试从文件名获取题库名称
        var fileName = Path.GetFileNameWithoutExtension(worksheet.Workbook.File?.Name);
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }
        
        return "Unnamed Question Bank";
    }
    
    // 其他方法的完整实现...
}
```

## 配置管理完整性要求

### CONF-001: 配置系统完整性

#### 完整的配置管理
```csharp
public interface IConfigurationService
{
    // 基础配置
    Task<T> GetConfigurationAsync<T>(string sectionName) where T : new();
    Task<T> GetConfigurationAsync<T>(string sectionName, string key) where T : new();
    Task SetConfigurationAsync<T>(string sectionName, T configuration);
    Task SetConfigurationAsync<T>(string sectionName, string key, T configuration);
    
    // 环境配置
    Task<T> GetEnvironmentConfigurationAsync<T>() where T : new();
    Task<bool> IsEnvironmentAsync(string environment);
    
    // 功能开关
    Task<bool> IsFeatureEnabledAsync(string featureName);
    Task SetFeatureEnabledAsync(string featureName, bool enabled);
    
    // 配置验证
    Task<ValidationResult> ValidateConfigurationAsync<T>(T configuration);
    Task<Dictionary<string, string>> GetConfigurationErrorsAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IConfigurationValidator _validator;
    private readonly ILogger<ConfigurationService> _logger;
    
    public ConfigurationService(
        IConfiguration configuration,
        IFeatureFlagService featureFlagService,
        IConfigurationValidator validator,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _featureFlagService = featureFlagService;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<T> GetConfigurationAsync<T>(string sectionName) where T : new()
    {
        try
        {
            var section = _configuration.GetSection(sectionName);
            var config = section.Get<T>() ?? new T();
            
            var validationResult = await _validator.ValidateConfigurationAsync(config);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Configuration validation failed for section {Section}: {Errors}", 
                    sectionName, string.Join(", ", validationResult.Errors));
            }
            
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration for section {Section}", sectionName);
            throw new ConfigurationException($"Failed to get configuration for section {sectionName}", ex);
        }
    }
    
    public async Task SetConfigurationAsync<T>(string sectionName, T configuration)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentException("Section name is required", nameof(sectionName));
        }
        
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        
        try
        {
            var validationResult = await _validator.ValidateConfigurationAsync(configuration);
            if (!validationResult.IsValid)
            {
                throw new ConfigurationValidationException(
                    $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
            }
            
            // 这里应该实现实际的配置保存逻辑
            // 例如：保存到文件、数据库或配置服务
            _logger.LogInformation("Configuration updated for section {Section}", sectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting configuration for section {Section}", sectionName);
            throw new ConfigurationException($"Failed to set configuration for section {sectionName}", ex);
        }
    }
    
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name is required", nameof(featureName));
        }
        
        try
        {
            return await _featureFlagService.IsEnabledAsync(featureName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag {FeatureName}", featureName);
            return false; // 默认返回false以确保安全
        }
    }
    
    // 其他方法的完整实现...
}
```

## 错误处理完整性要求

### ERR-001: 异常体系完整性

#### 完整的异常体系
```csharp
// 基础异常类
public abstract class QuizForgeException : Exception
{
    public QuizForgeException(string message) : base(message)
    {
    }
    
    public QuizForgeException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

// 业务逻辑异常
public class QuestionServiceException : QuizForgeException
{
    public QuestionServiceException(string message) : base(message)
    {
    }
    
    public QuestionServiceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class TemplateServiceException : QuizForgeException
{
    public TemplateServiceException(string message) : base(message)
    {
    }
    
    public TemplateServiceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class GenerationServiceException : QuizForgeException
{
    public GenerationServiceException(string message) : base(message)
    {
    }
    
    public GenerationServiceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

// 验证异常
public class ValidationException : QuizForgeException
{
    public IEnumerable<string> ValidationErrors { get; }
    
    public ValidationException(string message, IEnumerable<string> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
    
    public ValidationException(string message, IEnumerable<string> validationErrors, Exception innerException) 
        : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}

// 配置异常
public class ConfigurationException : QuizForgeException
{
    public ConfigurationException(string message) : base(message)
    {
    }
    
    public ConfigurationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class ConfigurationValidationException : ConfigurationException
{
    public IEnumerable<string> ValidationErrors { get; }
    
    public ConfigurationValidationException(string message, IEnumerable<string> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}

// 文件处理异常
public class FileProcessingException : QuizForgeException
{
    public string FilePath { get; }
    
    public FileProcessingException(string message, string filePath) : base(message)
    {
        FilePath = filePath;
    }
    
    public FileProcessingException(string message, string filePath, Exception innerException) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}

// 特定异常类型
public class QuestionNotFoundException : QuestionServiceException
{
    public int QuestionId { get; }
    
    public QuestionNotFoundException(string message, int questionId) : base(message)
    {
        QuestionId = questionId;
    }
}

public class TemplateNotFoundException : TemplateServiceException
{
    public int TemplateId { get; }
    
    public TemplateNotFoundException(string message, int templateId) : base(message)
    {
        TemplateId = templateId;
    }
}

public class InsufficientQuestionsException : GenerationServiceException
{
    public int RequiredCount { get; }
    public int AvailableCount { get; }
    
    public InsufficientQuestionsException(string message, int requiredCount, int availableCount) 
        : base(message)
    {
        RequiredCount = requiredCount;
        AvailableCount = availableCount;
    }
}
```

## 验证完整性要求

### VAL-001: 验证体系完整性

#### 完整的验证体系
```csharp
public interface IQuestionValidator
{
    Task<ValidationResult> ValidateQuestionAsync(Question question);
    Task<ValidationResult> ValidateQuestionBankAsync(QuestionBank questionBank);
    Task<ValidationResult> ValidateQuestionsAsync(IEnumerable<Question> questions);
}

public class QuestionValidator : IQuestionValidator
{
    private readonly ILogger<QuestionValidator> _logger;
    
    public QuestionValidator(ILogger<QuestionValidator> logger)
    {
        _logger = logger;
    }
    
    public async Task<ValidationResult> ValidateQuestionAsync(Question question)
    {
        if (question == null)
        {
            return new ValidationResult { Errors = { "Question cannot be null" } };
        }
        
        var errors = new List<string>();
        
        // 验证基本信息
        if (string.IsNullOrWhiteSpace(question.Content))
        {
            errors.Add("Question content is required");
        }
        
        if (question.Content.Length > 1000)
        {
            errors.Add("Question content cannot exceed 1000 characters");
        }
        
        // 验证题目类型
        if (!Enum.IsDefined(typeof(QuestionType), question.Type))
        {
            errors.Add("Invalid question type");
        }
        
        // 验证答案
        if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            errors.Add("Correct answer is required");
        }
        
        // 验证选项（选择题）
        if (question.Type == QuestionType.SingleChoice || question.Type == QuestionType.MultipleChoice)
        {
            if (question.Options == null || question.Options.Count < 2)
            {
                errors.Add("Choice questions must have at least 2 options");
            }
            
            if (question.Options != null && question.Options.Count > 10)
            {
                errors.Add("Choice questions cannot have more than 10 options");
            }
            
            if (question.Options != null)
            {
                var duplicateOptions = question.Options
                    .GroupBy(o => o)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                
                if (duplicateOptions.Any())
                {
                    errors.Add($"Duplicate options found: {string.Join(", ", duplicateOptions)}");
                }
                
                // 验证答案是否在选项中
                if (question.Type == QuestionType.SingleChoice)
                {
                    if (!question.Options.Contains(question.CorrectAnswer))
                    {
                        errors.Add("Correct answer must be one of the options");
                    }
                }
            }
        }
        
        // 验证分值
        if (question.Points <= 0)
        {
            errors.Add("Points must be positive");
        }
        
        if (question.Points > 100)
        {
            errors.Add("Points cannot exceed 100");
        }
        
        // 验证难度
        if (!Enum.IsDefined(typeof(Difficulty), question.Difficulty))
        {
            errors.Add("Invalid difficulty level");
        }
        
        // 验证分类
        if (!string.IsNullOrWhiteSpace(question.Category) && question.Category.Length > 50)
        {
            errors.Add("Category cannot exceed 50 characters");
        }
        
        // 验证解析
        if (!string.IsNullOrWhiteSpace(question.Explanation) && question.Explanation.Length > 2000)
        {
            errors.Add("Explanation cannot exceed 2000 characters");
        }
        
        return new ValidationResult { Errors = errors };
    }
    
    public async Task<ValidationResult> ValidateQuestionBankAsync(QuestionBank questionBank)
    {
        if (questionBank == null)
        {
            return new ValidationResult { Errors = { "Question bank cannot be null" } };
        }
        
        var errors = new List<string>();
        
        // 验证基本信息
        if (string.IsNullOrWhiteSpace(questionBank.Name))
        {
            errors.Add("Question bank name is required");
        }
        
        if (questionBank.Name.Length > 100)
        {
            errors.Add("Question bank name cannot exceed 100 characters");
        }
        
        // 验证题目列表
        if (questionBank.Questions == null)
        {
            errors.Add("Questions list cannot be null");
        }
        else
        {
            if (questionBank.Questions.Count == 0)
            {
                errors.Add("Question bank must contain at least one question");
            }
            
            if (questionBank.Questions.Count > 1000)
            {
                errors.Add("Question bank cannot contain more than 1000 questions");
            }
            
            // 验证每个题目
            var questionValidationTasks = questionBank.Questions.Select(q => ValidateQuestionAsync(q));
            var questionValidationResults = await Task.WhenAll(questionValidationTasks);
            
            foreach (var (question, validationResult) in questionBank.Questions.Zip(questionValidationResults))
            {
                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Errors.Select(e => $"Question '{question.Content}': {e}"));
                }
            }
        }
        
        return new ValidationResult { Errors = errors };
    }
    
    public async Task<ValidationResult> ValidateQuestionsAsync(IEnumerable<Question> questions)
    {
        if (questions == null)
        {
            return new ValidationResult { Errors = { "Questions collection cannot be null" } };
        }
        
        var errors = new List<string>();
        var questionList = questions.ToList();
        
        if (questionList.Count == 0)
        {
            errors.Add("Questions collection cannot be empty");
        }
        
        var validationTasks = questionList.Select(q => ValidateQuestionAsync(q));
        var validationResults = await Task.WhenAll(validationTasks);
        
        foreach (var (question, validationResult) in questionList.Zip(validationResults))
        {
            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => $"Question '{question.Content}': {e}"));
            }
        }
        
        return new ValidationResult { Errors = errors };
    }
}
```

## 实现检查清单

### CHECK-001: 服务层检查清单
- [ ] IQuestionService 所有方法完整实现
- [ ] ITemplateService 所有方法完整实现
- [ ] IGenerationService 所有方法完整实现
- [ ] IConfigurationService 所有方法完整实现
- [ ] 所有服务都有完整的错误处理
- [ ] 所有服务都有完整的日志记录
- [ ] 所有服务都有完整的参数验证

### CHECK-002: CLI层检查清单
- [ ] GenerateCommand 完整实现
- [ ] ValidateCommand 完整实现
- [ ] BatchCommand 完整实现
- [ ] TemplateCommand 完整实现
- [ ] ConfigCommand 完整实现
- [ ] 所有命令都有完整的选项验证
- [ ] 所有命令都有完整的错误处理
- [ ] 所有命令都有完整的进度显示

### CHECK-003: 基础设施层检查清单
- [ ] ExcelParser 完整实现
- [ ] MarkdownParser 完整实现
- [ ] LatexPdfEngine 完整实现
- [ ] FileService 完整实现
- [ ] 所有解析器都有完整的验证功能
- [ ] 所有引擎都有完整的错误处理
- [ ] 所有服务都有完整的日志记录

### CHECK-004: 数据层检查清单
- [ ] 所有Repository接口完整实现
- [ ] 所有数据模型完整定义
- [ ] 所有DbContext配置完整
- [ ] 所有数据库迁移完整
- [ ] 所有数据操作都有完整的错误处理

### CHECK-005: 测试检查清单
- [ ] 所有服务方法都有单元测试
- [ ] 所有命令都有集成测试
- [ ] 所有异常情况都有测试覆盖
- [ ] 测试覆盖率 > 90%
- [ ] 所有测试都通过

## 实施计划

### PLAN-001: 实施阶段

#### 第一阶段：核心服务实现（2周）
- 实现QuestionService的完整功能
- 实现TemplateService的完整功能
- 实现GenerationService的完整功能
- 实现ConfigurationService的完整功能

#### 第二阶段：CLI层实现（1周）
- 实现GenerateCommand的完整功能
- 实现其他命令的完整功能
- 实现选项验证和错误处理
- 实现进度显示和用户界面

#### 第三阶段：基础设施层实现（1周）
- 实现ExcelParser的完整功能
- 实现其他解析器的完整功能
- 实现LatexPdfEngine的完整功能
- 实现FileService的完整功能

#### 第四阶段：测试和验证（1周）
- 编写单元测试
- 编写集成测试
- 验证功能完整性
- 修复发现的问题

### PLAN-002: 质量保证

#### 代码质量
- 确保零编译警告
- 确保零硬编码返回值
- 确保代码风格一致
- 确保文档注释完整

#### 功能质量
- 确保所有功能完整实现
- 确保所有错误情况处理
- 确保所有边界情况处理
- 确保性能满足要求

#### 测试质量
- 确保测试覆盖率 > 90%
- 确保测试质量良好
- 确保所有测试通过
- 确保测试覆盖所有场景

## 总结

本完整功能实现要求文档为QuizForge CLI项目提供了全面的功能实现指南。通过消除硬编码返回值、实现完整的业务逻辑、完善的错误处理和验证体系，我们将确保系统的功能完整性和可靠性。

关键成功因素：
- 严格的功能实现标准
- 完善的异常处理体系
- 全面的验证和测试
- 持续的质量检查

通过实施这些要求，我们将建立一个功能完整、高质量的QuizForge CLI系统。