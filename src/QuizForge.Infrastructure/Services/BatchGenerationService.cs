using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Infrastructure.Services;

/// <summary>
/// 批量生成服务实现
/// </summary>
public class BatchGenerationService : IBatchGenerationService
{
    private readonly ILogger<BatchGenerationService> _logger;
    private readonly IExportService _exportService;
    private readonly IQuestionService _questionService;
    private readonly ITemplateService _templateService;
    private readonly IGenerationService _generationService;

    // 用于存储批量生成任务的内存缓存
    private static readonly ConcurrentDictionary<Guid, BatchGenerationTask> _batchTasks = new();

    /// <summary>
    /// 批量生成服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="exportService">导出服务</param>
    /// <param name="questionService">题目服务</param>
    /// <param name="templateService">模板服务</param>
    /// <param name="generationService">生成服务</param>
    public BatchGenerationService(
        ILogger<BatchGenerationService> logger,
        IExportService exportService,
        IQuestionService questionService,
        ITemplateService templateService,
        IGenerationService generationService)
    {
        _logger = logger;
        _exportService = exportService;
        _questionService = questionService;
        _templateService = templateService;
        _generationService = generationService;
    }

    /// <summary>
    /// 批量生成试卷
    /// </summary>
    /// <param name="request">批量生成请求</param>
    /// <returns>批量生成结果</returns>
    public async Task<BatchGenerationResult> BatchGenerateExamPapersAsync(BatchGenerationRequest request)
    {
        try
        {
            // 验证请求参数
            if (request.QuestionBankIds == null || !request.QuestionBankIds.Any())
            {
                throw new ArgumentException("题库ID列表不能为空");
            }

            if (request.Count <= 0)
            {
                throw new ArgumentException("生成数量必须大于0");
            }

            if (string.IsNullOrEmpty(request.OutputDirectory))
            {
                throw new ArgumentException("输出目录不能为空");
            }

            // 确保输出目录存在
            if (!Directory.Exists(request.OutputDirectory))
            {
                Directory.CreateDirectory(request.OutputDirectory);
            }

            // 创建批量任务
            var batchId = Guid.NewGuid();
            var batchTask = new BatchGenerationTask
            {
                BatchId = batchId,
                Request = request,
                Status = BatchGenerationStatus.Created,
                StartTime = DateTime.UtcNow,
                Result = new BatchGenerationResult
                {
                    BatchId = batchId,
                    Status = BatchGenerationStatus.Created,
                    TotalCount = request.Count,
                    StartTime = DateTime.UtcNow
                }
            };

            // 将任务添加到缓存
            _batchTasks[batchId] = batchTask;

            // 启动后台任务进行批量生成
            _ = Task.Run(() => ProcessBatchGenerationAsync(batchId));

            _logger.LogInformation("批量生成任务已创建: {BatchId}, 生成数量: {Count}", batchId, request.Count);
            return batchTask.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建批量生成任务失败");
            throw;
        }
    }

    /// <summary>
    /// 获取批量生成进度
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>批量生成进度</returns>
    public Task<BatchGenerationProgress> GetBatchGenerationProgressAsync(Guid batchId)
    {
        try
        {
            if (!_batchTasks.TryGetValue(batchId, out var batchTask))
            {
                throw new KeyNotFoundException($"未找到批量任务: {batchId}");
            }

            var progress = new BatchGenerationProgress
            {
                BatchId = batchId,
                Status = batchTask.Status,
                TotalCount = batchTask.Result.TotalCount,
                CompletedCount = batchTask.Result.CompletedCount,
                FailedCount = batchTask.Result.FailedCount,
                StartTime = batchTask.StartTime,
                EstimatedCompletionTime = batchTask.EstimatedCompletionTime,
                ElapsedSeconds = (DateTime.UtcNow - batchTask.StartTime).TotalSeconds,
                CurrentFile = batchTask.CurrentFile,
                ErrorMessage = batchTask.ErrorMessage
            };

            // 计算进度百分比
            if (batchTask.Result.TotalCount > 0)
            {
                progress.ProgressPercentage = (double)batchTask.Result.CompletedCount / batchTask.Result.TotalCount * 100;
            }

            // 计算剩余时间
            if (progress.ProgressPercentage > 0 && progress.ProgressPercentage < 100)
            {
                var elapsedSeconds = progress.ElapsedSeconds;
                var totalEstimatedSeconds = elapsedSeconds / (progress.ProgressPercentage / 100);
                progress.RemainingSeconds = totalEstimatedSeconds - elapsedSeconds;
            }

            return Task.FromResult(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取批量生成进度失败: {BatchId}", batchId);
            throw;
        }
    }

    /// <summary>
    /// 取消批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>取消结果</returns>
    public Task<bool> CancelBatchGenerationAsync(Guid batchId)
    {
        try
        {
            if (!_batchTasks.TryGetValue(batchId, out var batchTask))
            {
                _logger.LogWarning("尝试取消不存在的批量任务: {BatchId}", batchId);
                return Task.FromResult(false);
            }

            if (batchTask.Status == BatchGenerationStatus.Completed ||
                batchTask.Status == BatchGenerationStatus.Cancelled ||
                batchTask.Status == BatchGenerationStatus.Failed)
            {
                _logger.LogWarning("尝试取消已完成的批量任务: {BatchId}, 状态: {Status}", batchId, batchTask.Status);
                return Task.FromResult(false);
            }

            // 标记任务为已取消
            batchTask.Status = BatchGenerationStatus.Cancelled;
            batchTask.Result.Status = BatchGenerationStatus.Cancelled;
            batchTask.Result.CompletionTime = DateTime.UtcNow;

            _logger.LogInformation("批量生成任务已取消: {BatchId}", batchId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消批量生成任务失败: {BatchId}", batchId);
            throw;
        }
    }

    /// <summary>
    /// 获取批量生成历史
    /// </summary>
    /// <param name="pageSize">页面大小</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>批量生成历史列表</returns>
    public Task<PagedResult<BatchGenerationHistory>> GetBatchGenerationHistoryAsync(int pageSize = 10, int pageNumber = 1)
    {
        try
        {
            // 验证分页参数
            if (pageSize <= 0)
            {
                throw new ArgumentException("页面大小必须大于0");
            }

            if (pageNumber <= 0)
            {
                throw new ArgumentException("页码必须大于0");
            }

            // 获取所有已完成的任务
            var completedTasks = _batchTasks.Values
                .Where(t => t.Status == BatchGenerationStatus.Completed ||
                           t.Status == BatchGenerationStatus.Cancelled ||
                           t.Status == BatchGenerationStatus.Failed)
                .OrderByDescending(t => t.StartTime)
                .ToList();

            var totalCount = completedTasks.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 确保页码在有效范围内
            pageNumber = Math.Min(pageNumber, Math.Max(1, totalPages));

            // 获取当前页的数据
            var items = completedTasks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new BatchGenerationHistory
                {
                    BatchId = t.BatchId,
                    TaskName = t.Request.FileNamePrefix,
                    Status = t.Status,
                    TotalCount = t.Result.TotalCount,
                    CompletedCount = t.Result.CompletedCount,
                    FailedCount = t.Result.FailedCount,
                    StartTime = t.StartTime,
                    CompletionTime = t.Result.CompletionTime,
                    OutputDirectory = t.Request.OutputDirectory,
                    CreatedBy = "System" // 实际项目中可以从用户上下文获取
                })
                .ToList();

            var result = new PagedResult<BatchGenerationHistory>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取批量生成历史失败");
            throw;
        }
    }

    /// <summary>
    /// 清理完成的批量生成任务
    /// </summary>
    /// <param name="olderThanDays">早于指定天数的任务</param>
    /// <returns>清理的任务数量</returns>
    public Task<int> CleanupCompletedBatchGenerationsAsync(int olderThanDays = 30)
    {
        try
        {
            if (olderThanDays <= 0)
            {
                throw new ArgumentException("天数必须大于0");
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            var tasksToRemove = _batchTasks.Values
                .Where(t => (t.Status == BatchGenerationStatus.Completed ||
                           t.Status == BatchGenerationStatus.Cancelled ||
                           t.Status == BatchGenerationStatus.Failed) &&
                          t.StartTime < cutoffDate)
                .ToList();

            var removedCount = 0;
            foreach (var task in tasksToRemove)
            {
                if (_batchTasks.TryRemove(task.BatchId, out _))
                {
                    removedCount++;
                }
            }

            _logger.LogInformation("清理了 {Count} 个完成的批量生成任务", removedCount);
            return Task.FromResult(removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理完成的批量生成任务失败");
            throw;
        }
    }

    /// <summary>
    /// 处理批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    private async Task ProcessBatchGenerationAsync(Guid batchId)
    {
        try
        {
            if (!_batchTasks.TryGetValue(batchId, out var batchTask))
            {
                _logger.LogError("批量任务不存在: {BatchId}", batchId);
                return;
            }

            // 更新任务状态为进行中
            batchTask.Status = BatchGenerationStatus.InProgress;
            batchTask.Result.Status = BatchGenerationStatus.InProgress;

            // 计算预计完成时间（基于每个文件的平均处理时间）
            var averageTimePerFile = 5.0; // 假设每个文件平均需要5秒
            var totalEstimatedSeconds = batchTask.Request.Count * averageTimePerFile / batchTask.Request.ParallelCount;
            batchTask.EstimatedCompletionTime = batchTask.StartTime.AddSeconds(totalEstimatedSeconds);

            _logger.LogInformation("开始处理批量生成任务: {BatchId}, 总数量: {Count}", batchId, batchTask.Request.Count);

            // 获取题库和模板
            var questionBanks = new List<QuestionBank>();
            foreach (var questionBankId in batchTask.Request.QuestionBankIds)
            {
                var questionBank = await _questionService.GetQuestionBankAsync(questionBankId);
                if (questionBank != null)
                {
                    questionBanks.Add(questionBank);
                }
            }

            if (!questionBanks.Any())
            {
                throw new Exception("未找到任何有效的题库");
            }

            var template = await _templateService.GetTemplateAsync(batchTask.Request.TemplateId);
            if (template == null)
            {
                throw new Exception($"未找到模板: {batchTask.Request.TemplateId}");
            }

            // 创建导出配置
            var exportConfig = new ExportConfiguration
            {
                OutputPath = batchTask.Request.OutputDirectory,
                Format = batchTask.Request.Format,
                IncludeAnswerKey = batchTask.Request.IncludeAnswerKey,
                FileName = batchTask.Request.FileNamePrefix
            };

            // 使用并行处理生成试卷
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = batchTask.Request.ParallelCount
            };

            await Parallel.ForEachAsync(Enumerable.Range(1, batchTask.Request.Count), options, async (i, cancellationToken) =>
            {
                try
                {
                    // 检查任务是否已取消
                    if (batchTask.Status == BatchGenerationStatus.Cancelled)
                    {
                        return;
                    }

                    var fileName = $"{batchTask.Request.FileNamePrefix}_{i:D3}.{GetFileExtension(batchTask.Request.Format)}";
                    batchTask.CurrentFile = fileName;

                    // 创建试卷
                    var examPaper = await CreateExamPaperAsync(questionBanks, template, batchTask.Request.Options, i);

                    // 生成LaTeX内容
                    var latexContent = await _generationService.GenerateLaTeXContentAsync(examPaper);

                    // 导出文件
                    var filePath = await _exportService.ExportToPdfAsync(latexContent, exportConfig);

                    // 更新结果
                    lock (batchTask.Result)
                    {
                        batchTask.Result.CompletedCount++;
                        batchTask.Result.GeneratedFiles.Add(new GeneratedFileInfo
                        {
                            FileName = fileName,
                            FilePath = filePath,
                            FileSize = new FileInfo(filePath).Length,
                            CreatedAt = DateTime.UtcNow,
                            ExamPaperId = examPaper.Id
                        });
                    }

                    _logger.LogInformation("生成试卷成功: {FileName}", fileName);
                }
                catch (Exception ex)
                {
                    lock (batchTask.Result)
                    {
                        batchTask.Result.FailedCount++;
                        batchTask.Result.Errors.Add($"生成试卷 {i} 失败: {ex.Message}");
                    }

                    _logger.LogError(ex, "生成试卷失败: {Index}", i);
                }
            });

            // 更新任务状态
            if (batchTask.Status != BatchGenerationStatus.Cancelled)
            {
                batchTask.Status = batchTask.Result.FailedCount > 0 
                    ? BatchGenerationStatus.Failed 
                    : BatchGenerationStatus.Completed;
                
                batchTask.Result.Status = batchTask.Status;
                batchTask.Result.CompletionTime = DateTime.UtcNow;
            }

            _logger.LogInformation("批量生成任务完成: {BatchId}, 完成: {Completed}, 失败: {Failed}", 
                batchId, batchTask.Result.CompletedCount, batchTask.Result.FailedCount);
        }
        catch (Exception ex)
        {
            if (_batchTasks.TryGetValue(batchId, out var batchTask))
            {
                batchTask.Status = BatchGenerationStatus.Failed;
                batchTask.Result.Status = BatchGenerationStatus.Failed;
                batchTask.Result.CompletionTime = DateTime.UtcNow;
                batchTask.ErrorMessage = ex.Message;
                batchTask.Result.Errors.Add($"批量生成失败: {ex.Message}");
            }

            _logger.LogError(ex, "批量生成任务失败: {BatchId}", batchId);
        }
    }

    /// <summary>
    /// 创建试卷
    /// </summary>
    /// <param name="questionBanks">题库列表</param>
    /// <param name="template">模板</param>
    /// <param name="options">生成选项</param>
    /// <param name="index">索引</param>
    /// <returns>试卷</returns>
    private async Task<ExamPaper> CreateExamPaperAsync(
        List<QuestionBank> questionBanks,
        ExamTemplate template,
        BatchGenerationOptions options,
        int index)
    {
        // 从所有题库中随机选择题目
        var allQuestions = questionBanks
            .SelectMany(qb => qb.Questions)
            .Where(q => options.QuestionTypeFilter.Count == 0 || options.QuestionTypeFilter.Contains(q.Type))
            .Where(q =>
            {
                // 尝试解析难度字符串为数值
                if (decimal.TryParse(q.Difficulty, out var difficulty))
                {
                    return difficulty >= options.DifficultyRange.Min && difficulty <= options.DifficultyRange.Max;
                }
                
                // 如果无法解析为数值，则使用字符串比较
                var difficultyLevels = new[] { "简单", "中等", "困难" };
                var minLevelIndex = (int)Math.Clamp(options.DifficultyRange.Min, 0, difficultyLevels.Length - 1);
                var maxLevelIndex = (int)Math.Clamp(options.DifficultyRange.Max, 0, difficultyLevels.Length - 1);
                var currentLevelIndex = Array.IndexOf(difficultyLevels, q.Difficulty);
                
                return currentLevelIndex >= minLevelIndex && currentLevelIndex <= maxLevelIndex;
            })
            .ToList();

        // 应用随机排序
        if (options.RandomizeQuestions)
        {
            allQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).ToList();
        }

        // 限制题目数量
        int questionCount = allQuestions.Count;
        if (options.MaxQuestionCount > 0)
        {
            questionCount = Math.Min(questionCount, options.MaxQuestionCount);
        }

        if (options.MinQuestionCount > 0)
        {
            questionCount = Math.Max(questionCount, options.MinQuestionCount);
        }

        var selectedQuestions = allQuestions.Take(questionCount).ToList();

        // 应用选项随机排序
        if (options.RandomizeOptions)
        {
            foreach (var question in selectedQuestions)
            {
                if (question.Options != null && question.Options.Any())
                {
                    question.Options = question.Options.OrderBy(o => Guid.NewGuid()).ToList();
                }
            }
        }

        // 创建试卷
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = $"{template.Name} - 第{index}份",
            TemplateId = template.Id,
            QuestionBankId = questionBanks.First().Id,
            CreatedAt = DateTime.UtcNow,
            Questions = selectedQuestions,
            TotalPoints = selectedQuestions.Sum(q => q.Points)
        };

        return await Task.FromResult(examPaper);
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="format">导出格式</param>
    /// <returns>文件扩展名</returns>
    private static string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.PDF => "pdf",
            ExportFormat.LaTeX => "tex",
            ExportFormat.Word => "docx",
            _ => "pdf"
        };
    }

    /// <summary>
    /// 批量生成任务
    /// </summary>
    private class BatchGenerationTask
    {
        /// <summary>
        /// 批量任务ID
        /// </summary>
        public Guid BatchId { get; set; }

        /// <summary>
        /// 生成请求
        /// </summary>
        public BatchGenerationRequest Request { get; set; } = new();

        /// <summary>
        /// 任务状态
        /// </summary>
        public BatchGenerationStatus Status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 预计完成时间
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; set; }

        /// <summary>
        /// 当前正在处理的文件
        /// </summary>
        public string? CurrentFile { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 生成结果
        /// </summary>
        public BatchGenerationResult Result { get; set; } = new();
    }
}