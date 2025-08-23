using Microsoft.Extensions.Options;
using QuizForge.CLI.Models;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Core.ContentGeneration;
using QuizForge.Infrastructure.Engines;
using Microsoft.Extensions.Logging;

// 使用类型别名解决命名冲突
using CliModels = QuizForge.CLI.Models;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI生成服务接口
/// </summary>
public interface ICliGenerationService
{
    /// <summary>
    /// 从Excel文件生成试卷PDF
    /// </summary>
    /// <param name="parameters">生成参数</param>
    /// <returns>生成结果</returns>
    Task<GenerationResult> GenerateFromExcelAsync(CliCommandParameters parameters);

    /// <summary>
    /// 从Markdown文件生成试卷PDF
    /// </summary>
    /// <param name="parameters">生成参数</param>
    /// <returns>生成结果</returns>
    Task<GenerationResult> GenerateFromMarkdownAsync(CliCommandParameters parameters);

    /// <summary>
    /// 批量生成试卷
    /// </summary>
    /// <param name="parameters">批量参数</param>
    /// <returns>批量生成结果</returns>
    Task<CliModels.BatchGenerationResult> BatchGenerateAsync(BatchParameters parameters);

    /// <summary>
    /// 验证Excel文件格式
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateExcelAsync(string filePath);

    /// <summary>
    /// 获取可用模板列表
    /// </summary>
    /// <returns>模板列表</returns>
    Task<List<TemplateInfo>> GetAvailableTemplatesAsync();
}

/// <summary>
/// CLI生成服务实现
/// </summary>
public class CliGenerationService : ICliGenerationService
{
    private readonly ILogger<CliGenerationService> _logger;
    private readonly IExcelParser _excelParser;
    private readonly IMarkdownParser _markdownParser;
    private readonly IPdfEngine _pdfEngine;
    private readonly LaTeXDocumentGenerator _latexDocumentGenerator;
    private readonly ICliFileService _fileService;
    private readonly ICliProgressService _progressService;
    private readonly ICliConfigurationService _configService;
    private readonly LaTeXOptions _latexOptions;
    private readonly PdfOptions _pdfOptions;
    private readonly TemplateOptions _templateOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    public CliGenerationService(
        ILogger<CliGenerationService> logger,
        IExcelParser excelParser,
        IMarkdownParser markdownParser,
        IPdfEngine pdfEngine,
        LaTeXDocumentGenerator latexDocumentGenerator,
        ICliFileService fileService,
        ICliProgressService progressService,
        ICliConfigurationService configService,
        IOptions<LaTeXOptions> latexOptions,
        IOptions<PdfOptions> pdfOptions,
        IOptions<TemplateOptions> templateOptions)
    {
        _logger = logger;
        _excelParser = excelParser;
        _markdownParser = markdownParser;
        _pdfEngine = pdfEngine;
        _latexDocumentGenerator = latexDocumentGenerator;
        _fileService = fileService;
        _progressService = progressService;
        _configService = configService;
        _latexOptions = latexOptions.Value;
        _pdfOptions = pdfOptions.Value;
        _templateOptions = templateOptions.Value;
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateFromExcelAsync(CliCommandParameters parameters)
    {
        var startTime = DateTime.Now;
        var result = new GenerationResult();

        try
        {
            _progressService.ShowTitle("从Excel生成试卷");

            // 验证输入文件
            if (parameters.Validate)
            {
                var validationResult = await ValidateExcelAsync(parameters.InputFile);
                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = $"文件验证失败: {string.Join(", ", validationResult.Errors)}";
                    return result;
                }
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(parameters.OutputFile);
            if (!string.IsNullOrEmpty(outputDir))
            {
                await _fileService.EnsureDirectoryExistsAsync(outputDir);
            }

            // 使用进度条
            using var progress = await _progressService.StartProgressAsync(5, "生成试卷");

            // 步骤1: 解析Excel文件
            progress.Update(1, "解析Excel文件");
            var questionBank = await _excelParser.ParseAsync(parameters.InputFile);
            result.QuestionCount = questionBank.Questions.Count;

            if (result.QuestionCount == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Excel文件中没有找到题目";
                return result;
            }

            // 步骤2: 生成LaTeX文档
            progress.Update(2, "生成LaTeX文档");
            var headerConfig = new HeaderConfig
            {
                ExamTitle = parameters.Title,
                Subject = parameters.Subject,
                ExamTime = parameters.ExamTime,
                Style = HeaderStyle.Standard,
                ShowStudentInfo = true
            };

            var latexContent = _latexDocumentGenerator.GenerateLaTeXDocument(questionBank, headerConfig);

            // 步骤3: 生成PDF
            progress.Update(3, "生成PDF文档");
            var pdfSuccess = await _pdfEngine.GenerateFromLatexAsync(latexContent, parameters.OutputFile);

            if (!pdfSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "PDF生成失败";
                return result;
            }

            // 步骤4: 生成预览
            progress.Update(4, "生成预览图像");
            if (_pdfOptions.EnablePreview)
            {
                result.PreviewData = await _pdfEngine.GeneratePreviewAsync(parameters.OutputFile);
            }

            // 步骤5: 验证输出文件
            progress.Update(5, "验证输出文件");
            var outputFileInfo = await _fileService.GetFileInfoAsync(parameters.OutputFile);
            if (outputFileInfo != null)
            {
                result.FileSize = outputFileInfo.Length;
                result.OutputPath = outputFileInfo.FullName;
            }

            result.Success = true;
            result.ProcessingTime = DateTime.Now - startTime;

            progress.Complete();
            _progressService.ShowSuccess($"试卷生成成功: {result.QuestionCount} 道题目");

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ProcessingTime = DateTime.Now - startTime;

            _logger.LogError(ex, "从Excel生成试卷失败: {InputFile}", parameters.InputFile);
            _progressService.ShowError($"生成失败: {ex.Message}");

            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<GenerationResult> GenerateFromMarkdownAsync(CliCommandParameters parameters)
    {
        var startTime = DateTime.Now;
        var result = new GenerationResult();

        try
        {
            _progressService.ShowTitle("从Markdown生成试卷");

            // 验证输入文件
            var validationResult = await _fileService.ValidateFileAsync(parameters.InputFile);
            if (!validationResult.IsValid)
            {
                result.Success = false;
                result.ErrorMessage = $"文件验证失败: {string.Join(", ", validationResult.Errors)}";
                return result;
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(parameters.OutputFile);
            if (!string.IsNullOrEmpty(outputDir))
            {
                await _fileService.EnsureDirectoryExistsAsync(outputDir);
            }

            // 使用进度条
            using var progress = await _progressService.StartProgressAsync(5, "生成试卷");

            // 步骤1: 解析Markdown文件
            progress.Update(1, "解析Markdown文件");
            var questionBank = await _markdownParser.ParseAsync(parameters.InputFile);
            result.QuestionCount = questionBank.Questions.Count;

            if (result.QuestionCount == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Markdown文件中没有找到题目";
                return result;
            }

            // 步骤2: 生成LaTeX文档
            progress.Update(2, "生成LaTeX文档");
            var headerConfig = new HeaderConfig
            {
                ExamTitle = parameters.Title,
                Subject = parameters.Subject,
                ExamTime = parameters.ExamTime,
                Style = HeaderStyle.Standard,
                ShowStudentInfo = true
            };

            var latexContent = _latexDocumentGenerator.GenerateLaTeXDocument(questionBank, headerConfig);

            // 步骤3: 生成PDF
            progress.Update(3, "生成PDF文档");
            var pdfSuccess = await _pdfEngine.GenerateFromLatexAsync(latexContent, parameters.OutputFile);

            if (!pdfSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "PDF生成失败";
                return result;
            }

            // 步骤4: 生成预览
            progress.Update(4, "生成预览图像");
            if (_pdfOptions.EnablePreview)
            {
                result.PreviewData = await _pdfEngine.GeneratePreviewAsync(parameters.OutputFile);
            }

            // 步骤5: 验证输出文件
            progress.Update(5, "验证输出文件");
            var outputFileInfo = await _fileService.GetFileInfoAsync(parameters.OutputFile);
            if (outputFileInfo != null)
            {
                result.FileSize = outputFileInfo.Length;
                result.OutputPath = outputFileInfo.FullName;
            }

            result.Success = true;
            result.ProcessingTime = DateTime.Now - startTime;

            progress.Complete();
            _progressService.ShowSuccess($"试卷生成成功: {result.QuestionCount} 道题目");

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ProcessingTime = DateTime.Now - startTime;

            _logger.LogError(ex, "从Markdown生成试卷失败: {InputFile}", parameters.InputFile);
            _progressService.ShowError($"生成失败: {ex.Message}");

            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<CliModels.BatchGenerationResult> BatchGenerateAsync(BatchParameters parameters)
    {
        var startTime = DateTime.Now;
        var result = new CliModels.BatchGenerationResult();

        try
        {
            _progressService.ShowTitle("批量生成试卷");

            // 获取输入文件列表
            var inputFiles = await _fileService.GetFilesAsync(parameters.InputDirectory, parameters.FilePattern);
            result.TotalFiles = inputFiles.Count;

            if (result.TotalFiles == 0)
            {
                _progressService.ShowWarning("没有找到匹配的文件");
                return result;
            }

            // 确保输出目录存在
            await _fileService.EnsureDirectoryExistsAsync(parameters.OutputDirectory);

            // 使用进度条
            using var progress = await _progressService.StartProgressAsync(result.TotalFiles, "批量处理文件");

            var semaphore = new SemaphoreSlim(parameters.MaxParallel);
            var tasks = new List<Task<GenerationResult>>();

            foreach (var file in inputFiles)
            {
                await semaphore.WaitAsync();
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var outputFileName = Path.ChangeExtension(file.Name, ".pdf");
                        var outputPath = Path.Combine(parameters.OutputDirectory, outputFileName);

                        var fileParams = new CliCommandParameters
                        {
                            InputFile = file.FullName,
                            OutputFile = outputPath,
                            Template = parameters.Template,
                            Title = parameters.Title,
                            Subject = parameters.Subject,
                            ExamTime = parameters.ExamTime,
                            Validate = parameters.Validate,
                            Verbose = parameters.Verbose,
                            ShowProgress = false // 批量处理时禁用子进度条
                        };

                        // 根据文件扩展名选择生成方法
                        var extension = Path.GetExtension(file.FullName).ToLowerInvariant();
                        if (extension == ".xlsx" || extension == ".xls")
                        {
                            return await GenerateFromExcelAsync(fileParams);
                        }
                        else if (extension == ".md")
                        {
                            return await GenerateFromMarkdownAsync(fileParams);
                        }
                        else
                        {
                            return new GenerationResult
                            {
                                Success = false,
                                ErrorMessage = $"不支持的文件类型: {extension}"
                            };
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            // 等待所有任务完成
            var generationResults = await Task.WhenAll(tasks);

            // 处理结果
            foreach (var generationResult in generationResults)
            {
                if (generationResult.Success)
                {
                    result.SuccessResults.Add(generationResult);
                    result.SuccessCount++;
                }
                else
                {
                    result.FailureResults.Add(generationResult);
                    result.FailureCount++;
                }
            }

            result.TotalProcessingTime = DateTime.Now - startTime;

            progress.Complete();

            // 显示汇总信息
            _progressService.ShowSuccess($"批量生成完成: {result.SuccessCount}/{result.TotalFiles} 成功");
            if (result.FailureCount > 0)
            {
                _progressService.ShowWarning($"{result.FailureCount} 个文件生成失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量生成失败");
            _progressService.ShowError($"批量生成失败: {ex.Message}");
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateExcelAsync(string filePath)
    {
        try
        {
            // 基本文件验证
            var validationResult = await _fileService.ValidateFileAsync(filePath);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            // Excel格式验证
            var formatValid = await _excelParser.ValidateFormatAsync(filePath);
            if (!formatValid)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Excel文件格式不正确，请确保包含必要的列（题型、题目、答案）");
            }
            else
            {
                validationResult.Information.Add("Excel文件格式验证通过");
            }

            return validationResult;
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = { $"验证失败: {ex.Message}" }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<List<TemplateInfo>> GetAvailableTemplatesAsync()
    {
        var templates = new List<TemplateInfo>();

        try
        {
            var templateDir = _templateOptions.Directory;
            if (!Directory.Exists(templateDir))
            {
                await _fileService.EnsureDirectoryExistsAsync(templateDir);
                return templates;
            }

            var templateFiles = Directory.GetFiles(templateDir, "*.tex");
            foreach (var templateFile in templateFiles)
            {
                var fileInfo = new FileInfo(templateFile);
                templates.Add(new TemplateInfo
                {
                    Name = Path.GetFileNameWithoutExtension(templateFile),
                    FilePath = templateFile,
                    Description = $"模板文件: {fileInfo.Name}",
                    IsDefault = fileInfo.Name.Equals(_templateOptions.DefaultTemplate, StringComparison.OrdinalIgnoreCase),
                    Type = "LaTeX"
                });
            }

            _logger.LogInformation("找到 {Count} 个模板文件", templates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模板列表失败");
        }

        return await Task.FromResult(templates);
    }
}