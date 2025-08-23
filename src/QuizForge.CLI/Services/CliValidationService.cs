using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.CLI.Models;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI验证服务，负责输入验证和错误处理
/// </summary>
public class CliValidationService : ICliValidationService
{
    private readonly ILogger<CliValidationService> _logger;

    /// <summary>
    /// 初始化CLI验证服务
    /// </summary>
    /// <param name="logger">日志服务</param>
    public CliValidationService(ILogger<CliValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 验证文件路径
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileType">文件类型描述</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateFilePath(string filePath, string fileType = "文件")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return SimpleValidationResult.Failure($"{fileType}路径不能为空");
            }

            if (!File.Exists(filePath))
            {
                return SimpleValidationResult.Failure($"{fileType}不存在: {filePath}");
            }

            if (filePath.Length > 260)
            {
                return SimpleValidationResult.Failure($"{fileType}路径过长，请使用较短的路径");
            }

            // 检查文件扩展名
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
            {
                return SimpleValidationResult.Failure($"{fileType}必须包含扩展名");
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证文件路径时发生错误: {FilePath}", filePath);
            return SimpleValidationResult.Failure($"验证{fileType}路径时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证输出目录
    /// </summary>
    /// <param name="outputPath">输出路径</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateOutputPath(string outputPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return SimpleValidationResult.Failure("输出路径不能为空");
            }

            // 如果是文件路径，检查目录是否存在
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    return SimpleValidationResult.Failure($"无法创建输出目录: {ex.Message}");
                }
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证输出路径时发生错误: {OutputPath}", outputPath);
            return SimpleValidationResult.Failure($"验证输出路径时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证题库格式
    /// </summary>
    /// <param name="format">题库格式</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateQuestionBankFormat(string format)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return SimpleValidationResult.Failure("题库格式不能为空");
            }

            var validFormats = new[] { "excel", "markdown", "xlsx", "md", "tex" };
            var normalizedFormat = format.ToLowerInvariant();

            if (!validFormats.Contains(normalizedFormat))
            {
                return SimpleValidationResult.Failure($"不支持的题库格式: {format}。支持的格式: {string.Join(", ", validFormats)}");
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证题库格式时发生错误: {Format}", format);
            return SimpleValidationResult.Failure($"验证题库格式时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证模板名称
    /// </summary>
    /// <param name="templateName">模板名称</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateTemplateName(string templateName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return SimpleValidationResult.Failure("模板名称不能为空");
            }

            if (templateName.Length > 100)
            {
                return SimpleValidationResult.Failure("模板名称不能超过100个字符");
            }

            // 检查是否包含非法字符
            var invalidChars = Path.GetInvalidFileNameChars();
            if (templateName.Any(c => invalidChars.Contains(c)))
            {
                return SimpleValidationResult.Failure("模板名称包含非法字符");
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证模板名称时发生错误: {TemplateName}", templateName);
            return SimpleValidationResult.Failure($"验证模板名称时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateQuestionBank(QuestionBank questionBank)
    {
        try
        {
            if (questionBank == null)
            {
                return SimpleValidationResult.Failure("题库数据不能为空");
            }

            if (string.IsNullOrWhiteSpace(questionBank.Name))
            {
                return SimpleValidationResult.Failure("题库名称不能为空");
            }

            if (questionBank.Questions == null || questionBank.Questions.Count == 0)
            {
                return SimpleValidationResult.Failure("题库中必须包含至少一个题目");
            }

            // 验证每个题目
            for (int i = 0; i < questionBank.Questions.Count; i++)
            {
                var question = questionBank.Questions[i];
                var questionValidation = ValidateQuestion(question, i + 1);
                if (!questionValidation.IsValid)
                {
                    return questionValidation;
                }
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证题库数据时发生错误");
            return SimpleValidationResult.Failure($"验证题库数据时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <param name="questionNumber">题目编号</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateQuestion(Question question, int questionNumber = 0)
    {
        try
        {
            if (question == null)
            {
                return SimpleValidationResult.Failure($"题目{(questionNumber > 0 ? $" #{questionNumber}" : "")}数据不能为空");
            }

            if (string.IsNullOrWhiteSpace(question.Type))
            {
                return SimpleValidationResult.Failure($"题目{(questionNumber > 0 ? $" #{questionNumber}" : "")}类型不能为空");
            }

            if (string.IsNullOrWhiteSpace(question.Content))
            {
                return SimpleValidationResult.Failure($"题目{(questionNumber > 0 ? $" #{questionNumber}" : "")}内容不能为空");
            }

            if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
            {
                return SimpleValidationResult.Failure($"题目{(questionNumber > 0 ? $" #{questionNumber}" : "")}答案不能为空");
            }

            // 验证选择题选项
            if (question.Type.Contains("选择", StringComparison.OrdinalIgnoreCase))
            {
                if (question.Options == null || question.Options.Count == 0)
                {
                    return SimpleValidationResult.Failure($"选择题{(questionNumber > 0 ? $" #{questionNumber}" : "")}必须包含选项");
                }

                if (question.Options.Count < 2)
                {
                    return SimpleValidationResult.Failure($"选择题{(questionNumber > 0 ? $" #{questionNumber}" : "")}必须包含至少2个选项");
                }

                // 验证是否有正确答案
                var hasCorrectAnswer = question.Options.Any(o => o.IsCorrect) ||
                                    question.Options.Any(o => o.Key.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
                
                if (!hasCorrectAnswer)
                {
                    return SimpleValidationResult.Failure($"选择题{(questionNumber > 0 ? $" #{questionNumber}" : "")}必须指定正确答案");
                }
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证题目时发生错误: {QuestionNumber}", questionNumber);
            return SimpleValidationResult.Failure($"验证题目{(questionNumber > 0 ? $" #{questionNumber}" : "")}时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证批处理配置
    /// </summary>
    /// <param name="batchConfig">批处理配置</param>
    /// <returns>验证结果</returns>
    public SimpleValidationResult ValidateBatchConfig(BatchProcessingConfig batchConfig)
    {
        try
        {
            if (batchConfig == null)
            {
                return SimpleValidationResult.Failure("批处理配置不能为空");
            }

            if (batchConfig.InputFiles == null || batchConfig.InputFiles.Count == 0)
            {
                return SimpleValidationResult.Failure("输入文件列表不能为空");
            }

            if (batchConfig.InputFiles.Count > 100)
            {
                return SimpleValidationResult.Failure("批处理文件数量不能超过100个");
            }

            // 验证每个输入文件
            for (int i = 0; i < batchConfig.InputFiles.Count; i++)
            {
                var fileValidation = ValidateFilePath(batchConfig.InputFiles[i], $"输入文件 #{i + 1}");
                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            // 验证输出目录
            if (!string.IsNullOrWhiteSpace(batchConfig.OutputDirectory))
            {
                var outputValidation = ValidateOutputPath(batchConfig.OutputDirectory);
                if (!outputValidation.IsValid)
                {
                    return outputValidation;
                }
            }

            return SimpleValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证批处理配置时发生错误");
            return SimpleValidationResult.Failure($"验证批处理配置时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 安全地执行操作并处理异常
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称</param>
    /// <param name="defaultValue">失败时的默认值</param>
    /// <returns>操作结果</returns>
    public async Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> operation, string operationName, T defaultValue = default!)
    {
        try
        {
            _logger.LogInformation("开始执行操作: {OperationName}", operationName);
            var result = await operation();
            _logger.LogInformation("操作执行成功: {OperationName}", operationName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作执行失败: {OperationName}", operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// 安全地执行操作并处理异常
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> ExecuteSafelyAsync(Func<Task> operation, string operationName)
    {
        try
        {
            _logger.LogInformation("开始执行操作: {OperationName}", operationName);
            await operation();
            _logger.LogInformation("操作执行成功: {OperationName}", operationName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作执行失败: {OperationName}", operationName);
            return false;
        }
    }
}