using Spectre.Console.Cli;
using QuizForge.CLI.Models;
using QuizForge.CLI.Services;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Commands;

/// <summary>
/// 生成命令基类，提供生成试卷的通用功能
/// </summary>
/// <remarks>
/// 这个基类为具体的生成命令（如Excel生成、Markdown生成）提供通用功能，
/// 包括参数验证、进度显示、错误处理等。
/// </remarks>
public abstract class GenerateCommandBase : AsyncCommand<CliCommandParameters>
{
    /// <summary>
    /// 日志服务
    /// </summary>
    protected readonly ILogger<GenerateCommandBase> Logger;

    /// <summary>
    /// 生成服务
    /// </summary>
    protected readonly ICliGenerationService GenerationService;

    /// <summary>
    /// 进度服务
    /// </summary>
    protected readonly ICliProgressService ProgressService;

    /// <summary>
    /// 文件服务
    /// </summary>
    protected readonly ICliFileService FileService;

    /// <summary>
    /// 初始化生成命令基类
    /// </summary>
    /// <param name="logger">日志服务</param>
    /// <param name="generationService">生成服务</param>
    /// <param name="progressService">进度服务</param>
    /// <param name="fileService">文件服务</param>
    /// <exception cref="ArgumentNullException">当任何服务为null时抛出</exception>
    protected GenerateCommandBase(
        ILogger<GenerateCommandBase> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        GenerationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    /// <summary>
    /// 执行生成命令
    /// </summary>
    /// <param name="context">命令上下文</param>
    /// <param name="parameters">命令参数</param>
    /// <returns>退出代码（0表示成功，非0表示失败）</returns>
    /// <exception cref="ArgumentNullException">当参数为null时抛出</exception>
    public override async Task<int> ExecuteAsync(CommandContext context, CliCommandParameters parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        try
        {
            // 验证必要参数
            if (string.IsNullOrWhiteSpace(parameters.InputFile))
            {
                ProgressService.ShowError("输入文件路径不能为空");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(parameters.OutputFile))
            {
                // 自动生成输出文件名
                var inputFileName = Path.GetFileNameWithoutExtension(parameters.InputFile);
                var outputDir = Path.GetDirectoryName(parameters.InputFile) ?? Directory.GetCurrentDirectory();
                parameters.OutputFile = Path.Combine(outputDir, $"{inputFileName}.pdf");
            }

            ProgressService.ShowInfo($"输入文件: {parameters.InputFile}");
            ProgressService.ShowInfo($"输出文件: {parameters.OutputFile}");

            // 执行生成
            var result = await ExecuteGenerationAsync(parameters);

            if (result.Success)
            {
                ProgressService.ShowSuccess($"生成完成: {result.OutputPath}");
                ProgressService.ShowInfo($"题目数量: {result.QuestionCount}");
                ProgressService.ShowInfo($"文件大小: {result.FileSize} bytes");
                ProgressService.ShowInfo($"处理时间: {result.ProcessingTime.TotalSeconds:F2} 秒");
                return 0;
            }
            else
            {
                ProgressService.ShowError($"生成失败: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "执行生成命令失败");
            ProgressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 执行生成逻辑
    /// </summary>
    /// <param name="parameters">参数</param>
    /// <returns>生成结果</returns>
    protected abstract Task<GenerationResult> ExecuteGenerationAsync(CliCommandParameters parameters);
}

/// <summary>
/// Excel生成命令
/// </summary>
public class ExcelGenerateCommand : GenerateCommandBase
{
    public ExcelGenerateCommand(
        ILogger<ExcelGenerateCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService) : base(logger, generationService, progressService, fileService)
    {
    }

    /// <inheritdoc/>
    protected override async Task<GenerationResult> ExecuteGenerationAsync(CliCommandParameters parameters)
    {
        return await GenerationService.GenerateFromExcelAsync(parameters);
    }
}

/// <summary>
/// Markdown生成命令
/// </summary>
public class MarkdownGenerateCommand : GenerateCommandBase
{
    public MarkdownGenerateCommand(
        ILogger<MarkdownGenerateCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService) : base(logger, generationService, progressService, fileService)
    {
    }

    /// <inheritdoc/>
    protected override async Task<GenerationResult> ExecuteGenerationAsync(CliCommandParameters parameters)
    {
        return await GenerationService.GenerateFromMarkdownAsync(parameters);
    }
}

/// <summary>
/// 批量生成命令
/// </summary>
public class BatchCommand : AsyncCommand<BatchParameters>
{
    private readonly ILogger<BatchCommand> _logger;
    private readonly ICliGenerationService _generationService;
    private readonly ICliProgressService _progressService;
    private readonly ICliFileService _fileService;

    public BatchCommand(
        ILogger<BatchCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService)
    {
        _logger = logger;
        _generationService = generationService;
        _progressService = progressService;
        _fileService = fileService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, BatchParameters parameters)
    {
        try
        {
            // 验证必要参数
            if (string.IsNullOrWhiteSpace(parameters.InputDirectory))
            {
                _progressService.ShowError("输入目录不能为空");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(parameters.OutputDirectory))
            {
                parameters.OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");
            }

            // 验证目录存在
            if (!Directory.Exists(parameters.InputDirectory))
            {
                _progressService.ShowError($"输入目录不存在: {parameters.InputDirectory}");
                return 1;
            }

            _progressService.ShowInfo($"输入目录: {parameters.InputDirectory}");
            _progressService.ShowInfo($"输出目录: {parameters.OutputDirectory}");
            _progressService.ShowInfo($"文件模式: {parameters.FilePattern}");
            _progressService.ShowInfo($"并行数量: {parameters.MaxParallel}");

            // 执行批量生成
            var result = await _generationService.BatchGenerateAsync(parameters);

            if (result.SuccessCount > 0)
            {
                _progressService.ShowSuccess($"批量生成完成: {result.SuccessCount}/{result.TotalFiles} 成功");
                
                // 显示成功文件列表
                if (parameters.Verbose && result.SuccessResults.Count > 0)
                {
                    _progressService.ShowTitle("成功生成的文件");
                    foreach (var successResult in result.SuccessResults)
                    {
                        _progressService.ShowInfo($"✓ {Path.GetFileName(successResult.OutputPath)} ({successResult.QuestionCount} 题目)");
                    }
                }
            }

            if (result.FailureCount > 0)
            {
                _progressService.ShowWarning($"{result.FailureCount} 个文件生成失败");
                
                // 显示失败文件列表
                if (parameters.Verbose && result.FailureResults.Count > 0)
                {
                    _progressService.ShowTitle("生成失败的文件");
                    foreach (var failureResult in result.FailureResults)
                    {
                        _progressService.ShowError($"✗ {Path.GetFileName(failureResult.OutputPath ?? "未知")}: {failureResult.ErrorMessage}");
                    }
                }

                return parameters.ContinueOnError ? 0 : 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行批量生成命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// 验证命令
/// </summary>
public class ValidateCommand : AsyncCommand<CliCommandParameters>
{
    private readonly ILogger<ValidateCommand> _logger;
    private readonly ICliGenerationService _generationService;
    private readonly ICliProgressService _progressService;
    private readonly ICliFileService _fileService;

    public ValidateCommand(
        ILogger<ValidateCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService)
    {
        _logger = logger;
        _generationService = generationService;
        _progressService = progressService;
        _fileService = fileService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, CliCommandParameters parameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameters.InputFile))
            {
                _progressService.ShowError("输入文件路径不能为空");
                return 1;
            }

            _progressService.ShowTitle("验证文件格式");

            // 根据文件扩展名选择验证方法
            var extension = Path.GetExtension(parameters.InputFile).ToLowerInvariant();
            ValidationResult validationResult;

            if (extension == ".xlsx" || extension == ".xls")
            {
                validationResult = await _generationService.ValidateExcelAsync(parameters.InputFile);
            }
            else
            {
                validationResult = await _fileService.ValidateFileAsync(parameters.InputFile);
            }

            // 显示验证结果
            _progressService.ShowValidationResult(validationResult);

            return validationResult.IsValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行验证命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}