using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;
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
    private readonly IPdfEngine _pdfEngine;
    private readonly PdfCacheService _pdfCacheService;

    // 用于存储批量生成任务的内存缓存
    private static readonly ConcurrentDictionary<Guid, BatchGenerationTask> _batchTasks = new();
    
    // 用于存储高级批量生成任务的内存缓存
    private static readonly ConcurrentDictionary<Guid, AdvancedBatchGenerationTask> _advancedBatchTasks = new();
    
    // 用于缓存题库和模板，避免重复查询
    private static readonly ConcurrentDictionary<Guid, QuestionBank> _questionBankCache = new();
    private static readonly ConcurrentDictionary<Guid, ExamTemplate> _templateCache = new();
    private static readonly DateTimeOffset _cacheExpiry = DateTimeOffset.Now.AddHours(1);
    
    // 用于生成随机字符串
    private static readonly Random _random = new Random();
    
    // 用于暂停和恢复的信号量
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokenSources = new();

    /// <summary>
    /// 批量生成服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="exportService">导出服务</param>
    /// <param name="questionService">题目服务</param>
    /// <param name="templateService">模板服务</param>
    /// <param name="generationService">生成服务</param>
    /// <param name="pdfEngine">PDF引擎</param>
    /// <param name="pdfCacheService">PDF缓存服务</param>
    public BatchGenerationService(
        ILogger<BatchGenerationService> logger,
        IExportService exportService,
        IQuestionService questionService,
        ITemplateService templateService,
        IGenerationService generationService,
        IPdfEngine pdfEngine,
        PdfCacheService pdfCacheService)
    {
        _logger = logger;
        _exportService = exportService;
        _questionService = questionService;
        _templateService = templateService;
        _generationService = generationService;
        _pdfEngine = pdfEngine;
        _pdfCacheService = pdfCacheService;
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

            // 获取题库和模板（使用缓存优化）
            var questionBanks = new List<QuestionBank>();
            foreach (var questionBankId in batchTask.Request.QuestionBankIds)
            {
                QuestionBank questionBank;
                if (_questionBankCache.TryGetValue(questionBankId, out var cachedBank) && DateTimeOffset.Now < _cacheExpiry)
                {
                    questionBank = cachedBank;
                    _logger.LogDebug("从缓存中获取题库: {QuestionBankId}", questionBankId);
                }
                else
                {
                    questionBank = await _questionService.GetQuestionBankAsync(questionBankId);
                    if (questionBank != null)
                    {
                        _questionBankCache[questionBankId] = questionBank;
                    }
                }
                
                if (questionBank != null)
                {
                    questionBanks.Add(questionBank);
                }
            }

            if (!questionBanks.Any())
            {
                throw new Exception("未找到任何有效的题库");
            }

            ExamTemplate template;
            if (_templateCache.TryGetValue(batchTask.Request.TemplateId, out var cachedTemplate) && DateTimeOffset.Now < _cacheExpiry)
            {
                template = cachedTemplate;
                _logger.LogDebug("从缓存中获取模板: {TemplateId}", batchTask.Request.TemplateId);
            }
            else
            {
                template = await _templateService.GetTemplateAsync(batchTask.Request.TemplateId);
                if (template != null)
                {
                    _templateCache[batchTask.Request.TemplateId] = template;
                }
            }
            
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

                    // 检查缓存中是否已有该PDF
                    var cachedPdfPath = _pdfCacheService.GetCachedPdf(latexContent);
                    string filePath;
                    
                    if (cachedPdfPath != null && File.Exists(cachedPdfPath))
                    {
                        // 从缓存复制文件
                        filePath = Path.Combine(exportConfig.OutputPath, fileName);
                        File.Copy(cachedPdfPath, filePath, true);
                        _logger.LogDebug("从缓存复制PDF文件: {FileName}", fileName);
                    }
                    else
                    {
                        // 生成新文件
                        filePath = await _exportService.ExportToPdfAsync(latexContent, exportConfig);
                        
                        // 缓存生成的PDF
                        try
                        {
                            var pdfData = await File.ReadAllBytesAsync(filePath);
                            await _pdfCacheService.CachePdfAsync(latexContent, pdfData);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "缓存PDF文件失败: {FileName}", fileName);
                        }
                    }

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

    /// <summary>
    /// 高级批量生成任务
    /// </summary>
    private class AdvancedBatchGenerationTask
    {
        /// <summary>
        /// 批量任务ID
        /// </summary>
        public Guid BatchId { get; set; }

        /// <summary>
        /// 生成请求
        /// </summary>
        public AdvancedBatchGenerationRequest Request { get; set; } = new();

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
        public AdvancedBatchGenerationResult Result { get; set; } = new();

        /// <summary>
        /// 暂停信号
        /// </summary>
        public ManualResetEventSlim PauseEvent { get; set; } = new ManualResetEventSlim(true);

        /// <summary>
        /// 生成统计信息
        /// </summary>
        public GenerationStatistics Statistics { get; set; } = new();

        /// <summary>
        /// 上次进度更新时间
        /// </summary>
        public DateTime LastProgressUpdateTime { get; set; } = DateTime.UtcNow;
    }

    #region 高级批量生成方法

    /// <summary>
    /// 高级批量生成试卷
    /// </summary>
    /// <param name="request">高级批量生成请求</param>
    /// <returns>批量生成结果</returns>
    public async Task<AdvancedBatchGenerationResult> AdvancedBatchGenerateExamPapersAsync(AdvancedBatchGenerationRequest request)
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
            var cancellationTokenSource = new CancellationTokenSource();
            var batchTask = new AdvancedBatchGenerationTask
            {
                BatchId = batchId,
                Request = request,
                Status = BatchGenerationStatus.Created,
                StartTime = DateTime.UtcNow,
                Result = new AdvancedBatchGenerationResult
                {
                    BatchId = batchId,
                    Status = BatchGenerationStatus.Created,
                    TotalCount = request.Count,
                    StartTime = DateTime.UtcNow
                }
            };

            // 将任务添加到缓存
            _advancedBatchTasks[batchId] = batchTask;
            _cancellationTokenSources[batchId] = cancellationTokenSource;

            // 启动后台任务进行批量生成
            _ = Task.Run(() => ProcessAdvancedBatchGenerationAsync(batchId, cancellationTokenSource.Token));

            _logger.LogInformation("高级批量生成任务已创建: {BatchId}, 生成数量: {Count}", batchId, request.Count);
            return batchTask.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建高级批量生成任务失败");
            throw;
        }
    }

    /// <summary>
    /// 获取批量生成的详细报告
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>批量生成详细报告</returns>
    public Task<BatchGenerationReport> GetBatchGenerationReportAsync(Guid batchId)
    {
        try
        {
            if (!_advancedBatchTasks.TryGetValue(batchId, out var batchTask))
            {
                throw new KeyNotFoundException($"未找到批量任务: {batchId}");
            }

            var report = new BatchGenerationReport
            {
                BatchId = batchId,
                TaskName = batchTask.Request.FileNamePrefix,
                Status = batchTask.Status,
                StartTime = batchTask.StartTime,
                CompletionTime = batchTask.Result.CompletionTime,
                TotalGenerationTime = batchTask.Result.CompletionTime.HasValue
                    ? (batchTask.Result.CompletionTime.Value - batchTask.StartTime).TotalSeconds
                    : (DateTime.UtcNow - batchTask.StartTime).TotalSeconds,
                Statistics = batchTask.Statistics,
                GeneratedFiles = batchTask.Result.GeneratedFiles,
                AnswerPaperFiles = batchTask.Result.AnswerPaperFiles,
                Errors = batchTask.Result.Errors,
                Warnings = new List<string>(),
                OutputDirectory = batchTask.Result.ActualOutputDirectory,
                Configuration = batchTask.Request
            };

            return Task.FromResult(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取批量生成报告失败: {BatchId}", batchId);
            throw;
        }
    }

    /// <summary>
    /// 暂停批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>暂停结果</returns>
    public Task<bool> PauseBatchGenerationAsync(Guid batchId)
    {
        try
        {
            if (!_advancedBatchTasks.TryGetValue(batchId, out var batchTask))
            {
                _logger.LogWarning("尝试暂停不存在的批量任务: {BatchId}", batchId);
                return Task.FromResult(false);
            }

            if (batchTask.Status == BatchGenerationStatus.Completed ||
                batchTask.Status == BatchGenerationStatus.Cancelled ||
                batchTask.Status == BatchGenerationStatus.Failed ||
                batchTask.Status == BatchGenerationStatus.Paused)
            {
                _logger.LogWarning("尝试暂停已完成的批量任务: {BatchId}, 状态: {Status}", batchId, batchTask.Status);
                return Task.FromResult(false);
            }

            // 标记任务为已暂停
            batchTask.Status = BatchGenerationStatus.Paused;
            batchTask.Result.Status = BatchGenerationStatus.Paused;
            batchTask.PauseEvent.Reset();

            _logger.LogInformation("批量生成任务已暂停: {BatchId}", batchId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停批量生成任务失败: {BatchId}", batchId);
            throw;
        }
    }

    /// <summary>
    /// 恢复批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>恢复结果</returns>
    public Task<bool> ResumeBatchGenerationAsync(Guid batchId)
    {
        try
        {
            if (!_advancedBatchTasks.TryGetValue(batchId, out var batchTask))
            {
                _logger.LogWarning("尝试恢复不存在的批量任务: {BatchId}", batchId);
                return Task.FromResult(false);
            }

            if (batchTask.Status != BatchGenerationStatus.Paused)
            {
                _logger.LogWarning("尝试恢复未暂停的批量任务: {BatchId}, 状态: {Status}", batchId, batchTask.Status);
                return Task.FromResult(false);
            }

            // 标记任务为进行中
            batchTask.Status = BatchGenerationStatus.InProgress;
            batchTask.Result.Status = BatchGenerationStatus.InProgress;
            batchTask.PauseEvent.Set();

            _logger.LogInformation("批量生成任务已恢复: {BatchId}", batchId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复批量生成任务失败: {BatchId}", batchId);
            throw;
        }
    }

    /// <summary>
    /// 处理高级批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task ProcessAdvancedBatchGenerationAsync(Guid batchId, CancellationToken cancellationToken)
    {
        try
        {
            if (!_advancedBatchTasks.TryGetValue(batchId, out var batchTask))
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

            _logger.LogInformation("开始处理高级批量生成任务: {BatchId}, 总数量: {Count}", batchId, batchTask.Request.Count);

            // 准备输出目录
            var outputDirectory = PrepareOutputDirectory(batchTask.Request);
            batchTask.Result.ActualOutputDirectory = outputDirectory;

            // 获取题库和模板（使用缓存优化）
            var questionBanks = await GetQuestionBanksAsync(batchTask.Request.QuestionBankIds);
            var template = await GetTemplateAsync(batchTask.Request.TemplateId);

            // 初始化统计信息
            batchTask.Statistics = new GenerationStatistics
            {
                TotalGenerationTime = 0,
                FastestGenerationTime = double.MaxValue,
                SlowestGenerationTime = double.MinValue
            };

            // 使用并行处理生成试卷
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = batchTask.Request.ParallelCount,
                CancellationToken = cancellationToken
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Parallel.ForEachAsync(Enumerable.Range(1, batchTask.Request.Count), options, async (i, cancellationToken) =>
            {
                try
                {
                    // 检查任务是否已取消
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // 检查任务是否已暂停
                    batchTask.PauseEvent.Wait(cancellationToken);

                    var fileStopwatch = Stopwatch.StartNew();
                    
                    // 生成文件名
                    var fileName = GenerateFileName(batchTask.Request.FileNameRule, i, batchTask.Request.FileNamePrefix);
                    batchTask.CurrentFile = fileName;

                    // 创建试卷
                    var examPaper = await CreateAdvancedExamPaperAsync(questionBanks, template, batchTask.Request.Options, i);

                    // 生成LaTeX内容
                    var latexContent = await _generationService.GenerateLaTeXContentAsync(examPaper);

                    // 创建导出配置
                    var exportConfig = new ExportConfiguration
                    {
                        OutputPath = outputDirectory,
                        Format = batchTask.Request.Format,
                        IncludeAnswerKey = batchTask.Request.IncludeAnswerKey,
                        FileName = Path.GetFileNameWithoutExtension(fileName)
                    };

                    // 检查缓存中是否已有该PDF
                    var cachedPdfPath = _pdfCacheService.GetCachedPdf(latexContent);
                    string filePath;
                    
                    if (cachedPdfPath != null && File.Exists(cachedPdfPath))
                    {
                        // 从缓存复制文件
                        filePath = Path.Combine(exportConfig.OutputPath, fileName);
                        File.Copy(cachedPdfPath, filePath, true);
                        _logger.LogDebug("从缓存复制PDF文件: {FileName}", fileName);
                    }
                    else
                    {
                        // 生成新文件
                        filePath = await _exportService.ExportToPdfAsync(latexContent, exportConfig);
                        
                        // 缓存生成的PDF
                        try
                        {
                            var pdfData = await File.ReadAllBytesAsync(filePath, cancellationToken);
                            await _pdfCacheService.CachePdfAsync(latexContent, pdfData);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "缓存PDF文件失败: {FileName}", fileName);
                        }
                    }

                    fileStopwatch.Stop();

                    // 更新统计信息
                    UpdateStatistics(batchTask.Statistics, fileStopwatch.Elapsed.TotalSeconds, examPaper);

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

                    // 如果需要生成答案试卷
                    if (batchTask.Request.Options.GenerateAnswerPapers)
                    {
                        await GenerateAnswerPaperAsync(batchTask, examPaper, latexContent, exportConfig, i);
                    }

                    // 定期发送进度更新
                    SendProgressUpdateIfNeeded(batchTask);

                    _logger.LogInformation("生成试卷成功: {FileName}, 耗时: {ElapsedMs}ms", fileName, fileStopwatch.ElapsedMilliseconds);
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

            stopwatch.Stop();

            // 更新总生成时间
            batchTask.Statistics.TotalGenerationTime = stopwatch.Elapsed.TotalSeconds;

            // 生成统计报告
            if (batchTask.Request.Options.GenerateStatisticsReport)
            {
                await GenerateStatisticsReportAsync(batchTask, outputDirectory);
            }

            // 生成索引文件
            if (batchTask.Request.OutputOptions.CreateIndexFile)
            {
                await GenerateIndexFileAsync(batchTask, outputDirectory);
            }

            // 更新任务状态
            if (batchTask.Status != BatchGenerationStatus.Cancelled)
            {
                batchTask.Status = batchTask.Result.FailedCount > 0
                    ? BatchGenerationStatus.Failed
                    : BatchGenerationStatus.Completed;
                
                batchTask.Result.Status = batchTask.Status;
                batchTask.Result.CompletionTime = DateTime.UtcNow;
            }

            // 发送完成通知
            if (batchTask.Request.NotificationOptions.NotifyOnCompletion)
            {
                await SendCompletionNotificationAsync(batchTask);
            }

            // 清理资源
            if (_cancellationTokenSources.TryRemove(batchId, out var cts))
            {
                cts.Dispose();
            }

            _logger.LogInformation("高级批量生成任务完成: {BatchId}, 完成: {Completed}, 失败: {Failed}, 总耗时: {TotalSeconds}s",
                batchId, batchTask.Result.CompletedCount, batchTask.Result.FailedCount, stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            if (_advancedBatchTasks.TryGetValue(batchId, out var batchTask))
            {
                batchTask.Status = BatchGenerationStatus.Failed;
                batchTask.Result.Status = BatchGenerationStatus.Failed;
                batchTask.Result.CompletionTime = DateTime.UtcNow;
                batchTask.ErrorMessage = ex.Message;
                batchTask.Result.Errors.Add($"批量生成失败: {ex.Message}");

                // 发送错误通知
                if (batchTask.Request.NotificationOptions.NotifyOnError)
                {
                    await SendErrorNotificationAsync(batchTask, ex);
                }
            }

            _logger.LogError(ex, "高级批量生成任务失败: {BatchId}", batchId);
        }
    }

    /// <summary>
    /// 准备输出目录
    /// </summary>
    /// <param name="request">高级批量生成请求</param>
    /// <returns>输出目录路径</returns>
    private string PrepareOutputDirectory(AdvancedBatchGenerationRequest request)
    {
        var outputDirectory = request.OutputDirectory;

        // 根据组织方式创建子目录
        switch (request.OutputOptions.OrganizationMethod)
        {
            case OutputOrganizationMethod.ByDate:
                if (request.OutputOptions.CreateDateSubdirectories)
                {
                    var dateSubDir = DateTime.Now.ToString(request.OutputOptions.DateSubdirectoryFormat);
                    outputDirectory = Path.Combine(outputDirectory, dateSubDir);
                }
                break;
                
            case OutputOrganizationMethod.ByBatch:
                if (request.OutputOptions.CreateBatchSubdirectories)
                {
                    var batchSubDir = $"{request.OutputOptions.BatchSubdirectoryPrefix}{DateTime.Now:yyyyMMdd_HHmmss}";
                    outputDirectory = Path.Combine(outputDirectory, batchSubDir);
                }
                break;
                
            case OutputOrganizationMethod.ByType:
                outputDirectory = Path.Combine(outputDirectory, request.Format.ToString());
                break;
        }

        // 确保输出目录存在
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        return outputDirectory;
    }

    /// <summary>
    /// 获取题库列表
    /// </summary>
    /// <param name="questionBankIds">题库ID列表</param>
    /// <returns>题库列表</returns>
    private async Task<List<QuestionBank>> GetQuestionBanksAsync(List<Guid> questionBankIds)
    {
        var questionBanks = new List<QuestionBank>();
        
        foreach (var questionBankId in questionBankIds)
        {
            QuestionBank questionBank;
            if (_questionBankCache.TryGetValue(questionBankId, out var cachedBank) && DateTimeOffset.Now < _cacheExpiry)
            {
                questionBank = cachedBank;
                _logger.LogDebug("从缓存中获取题库: {QuestionBankId}", questionBankId);
            }
            else
            {
                questionBank = await _questionService.GetQuestionBankAsync(questionBankId);
                if (questionBank != null)
                {
                    _questionBankCache[questionBankId] = questionBank;
                }
            }
            
            if (questionBank != null)
            {
                questionBanks.Add(questionBank);
            }
        }

        if (!questionBanks.Any())
        {
            throw new Exception("未找到任何有效的题库");
        }

        return questionBanks;
    }

    /// <summary>
    /// 获取模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>模板</returns>
    private async Task<ExamTemplate> GetTemplateAsync(Guid templateId)
    {
        ExamTemplate template;
        if (_templateCache.TryGetValue(templateId, out var cachedTemplate) && DateTimeOffset.Now < _cacheExpiry)
        {
            template = cachedTemplate;
            _logger.LogDebug("从缓存中获取模板: {TemplateId}", templateId);
        }
        else
        {
            template = await _templateService.GetTemplateAsync(templateId);
            if (template != null)
            {
                _templateCache[templateId] = template;
            }
        }
        
        if (template == null)
        {
            throw new Exception($"未找到模板: {templateId}");
        }

        return template;
    }

    /// <summary>
    /// 生成文件名
    /// </summary>
    /// <param name="fileNameRule">文件名规则</param>
    /// <param name="index">索引</param>
    /// <param name="prefix">前缀</param>
    /// <returns>文件名</returns>
    private string GenerateFileName(FileNameRule fileNameRule, int index, string prefix)
    {
        var fileName = fileNameRule.Template;
        
        // 替换占位符
        fileName = fileName.Replace("{prefix}", prefix);
        fileName = fileName.Replace("{index}", index.ToString("D3"));
        
        if (fileNameRule.IncludeDate)
        {
            fileName = fileName.Replace("{date}", DateTime.Now.ToString(fileNameRule.DateFormat));
        }
        
        if (fileNameRule.IncludeTime)
        {
            fileName = fileName.Replace("{time}", DateTime.Now.ToString(fileNameRule.TimeFormat));
        }
        
        if (fileNameRule.RandomLength > 0)
        {
            var randomString = GenerateRandomString(fileNameRule.RandomLength);
            fileName = fileName.Replace("{random}", randomString);
        }
        
        // 确保文件名合法
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        
        return fileName;
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="length">长度</param>
    /// <returns>随机字符串</returns>
    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[_random.Next(chars.Length)]);
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// 创建高级试卷
    /// </summary>
    /// <param name="questionBanks">题库列表</param>
    /// <param name="template">模板</param>
    /// <param name="options">生成选项</param>
    /// <param name="index">索引</param>
    /// <returns>试卷</returns>
    private async Task<ExamPaper> CreateAdvancedExamPaperAsync(
        List<QuestionBank> questionBanks,
        ExamTemplate template,
        AdvancedBatchGenerationOptions options,
        int index)
    {
        // 从所有题库中获取题目
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

        // 如果需要变化题目组合
        if (options.VaryQuestionCombination)
        {
            // 使用索引作为随机种子，确保每份试卷的题目组合不同但可重现
            var random = new Random(index);
            var variationQuestionCount = (int)(allQuestions.Count * (1 - options.QuestionVariationRate));
            
            // 随机选择题目
            var variationSelectedQuestions = new List<Question>();
            var availableQuestions = new List<Question>(allQuestions);
            
            for (int i = 0; i < variationQuestionCount && availableQuestions.Count > 0; i++)
            {
                var randomIndex = random.Next(availableQuestions.Count);
                variationSelectedQuestions.Add(availableQuestions[randomIndex]);
                availableQuestions.RemoveAt(randomIndex);
            }
            
            allQuestions = variationSelectedQuestions;
        }

        // 应用随机排序
        if (options.VaryQuestionOrder)
        {
            // 使用索引作为随机种子，确保每份试卷的题目顺序不同但可重现
            var random = new Random(index);
            allQuestions = allQuestions.OrderBy(q => random.Next()).ToList();
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
        if (options.VaryOptionOrder)
        {
            // 使用索引作为随机种子，确保每份试卷的选项顺序不同但可重现
            var random = new Random(index);
            
            foreach (var question in selectedQuestions)
            {
                if (question.Options != null && question.Options.Any())
                {
                    // 将选项转换为列表以便随机排序
                    var optionsList = question.Options.ToList();
                    var shuffledOptions = optionsList.OrderBy(o => random.Next()).ToList();
                    
                    // 更新正确答案的键
                    if (!string.IsNullOrEmpty(question.CorrectAnswer))
                    {
                        var correctOption = optionsList.FirstOrDefault(o => o.Key == question.CorrectAnswer);
                        if (correctOption != null)
                        {
                            var newIndex = shuffledOptions.IndexOf(correctOption);
                            question.CorrectAnswer = shuffledOptions[newIndex].Key;
                        }
                    }
                    
                    question.Options = shuffledOptions;
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
    /// 更新统计信息
    /// </summary>
    /// <param name="statistics">统计信息</param>
    /// <param name="generationTime">生成时间</param>
    /// <param name="examPaper">试卷</param>
    private void UpdateStatistics(GenerationStatistics statistics, double generationTime, ExamPaper examPaper)
    {
        // 更新生成时间统计
        statistics.FastestGenerationTime = Math.Min(statistics.FastestGenerationTime, generationTime);
        statistics.SlowestGenerationTime = Math.Max(statistics.SlowestGenerationTime, generationTime);
        
        // 更新题目统计
        statistics.TotalQuestions += examPaper.Questions.Count;
        statistics.AverageQuestionsPerPaper = statistics.TotalQuestions / (statistics.TotalQuestions > 0 ? 1 : 1);
        
        // 更新分值统计
        statistics.TotalPoints += examPaper.TotalPoints;
        statistics.AveragePointsPerPaper = (double)(statistics.TotalPoints / (statistics.TotalPoints > 0 ? 1 : 1));
        
        // 更新题目类型分布
        foreach (var question in examPaper.Questions)
        {
            if (!statistics.QuestionTypeDistribution.ContainsKey(question.Type))
            {
                statistics.QuestionTypeDistribution[question.Type] = 0;
            }
            statistics.QuestionTypeDistribution[question.Type]++;
        }
        
        // 更新难度分布
        foreach (var question in examPaper.Questions)
        {
            var difficulty = question.Difficulty ?? "未知";
            if (!statistics.DifficultyDistribution.ContainsKey(difficulty))
            {
                statistics.DifficultyDistribution[difficulty] = 0;
            }
            statistics.DifficultyDistribution[difficulty]++;
        }
    }

    /// <summary>
    /// 生成答案试卷
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    /// <param name="examPaper">试卷</param>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="exportConfig">导出配置</param>
    /// <param name="index">索引</param>
    private async Task GenerateAnswerPaperAsync(
        AdvancedBatchGenerationTask batchTask,
        ExamPaper examPaper,
        string latexContent,
        ExportConfiguration exportConfig,
        int index)
    {
        try
        {
            // 创建答案试卷文件名
            var answerFileName = Path.GetFileNameWithoutExtension(exportConfig.FileName) +
                                batchTask.Request.Options.AnswerPaperSuffix +
                                Path.GetExtension(exportConfig.FileName);
            
            // 创建包含答案的导出配置
            var answerExportConfig = new ExportConfiguration
            {
                OutputPath = exportConfig.OutputPath,
                Format = exportConfig.Format,
                IncludeAnswerKey = true,
                FileName = Path.GetFileNameWithoutExtension(answerFileName)
            };
            
            // 生成答案试卷
            var answerFilePath = await _exportService.ExportToPdfAsync(latexContent, answerExportConfig);
            
            // 更新结果
            lock (batchTask.Result)
            {
                batchTask.Result.AnswerPaperFiles.Add(new GeneratedFileInfo
                {
                    FileName = answerFileName,
                    FilePath = answerFilePath,
                    FileSize = new FileInfo(answerFilePath).Length,
                    CreatedAt = DateTime.UtcNow,
                    ExamPaperId = examPaper.Id
                });
            }
            
            _logger.LogDebug("生成答案试卷成功: {AnswerFileName}", answerFileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "生成答案试卷失败: {Index}", index);
        }
    }

    /// <summary>
    /// 发送进度更新（如果需要）
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    private void SendProgressUpdateIfNeeded(AdvancedBatchGenerationTask batchTask)
    {
        var now = DateTime.UtcNow;
        var interval = TimeSpan.FromSeconds(batchTask.Request.NotificationOptions.ProgressUpdateInterval);
        
        if (now - batchTask.LastProgressUpdateTime >= interval)
        {
            batchTask.LastProgressUpdateTime = now;
            
            // 这里可以添加通知逻辑，例如通过事件、回调或消息队列发送进度更新
            _logger.LogInformation("批量生成进度更新: {BatchId}, 完成: {Completed}/{Total}",
                batchTask.BatchId, batchTask.Result.CompletedCount, batchTask.Result.TotalCount);
        }
    }

    /// <summary>
    /// 生成统计报告
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    /// <param name="outputDirectory">输出目录</param>
    private async Task GenerateStatisticsReportAsync(AdvancedBatchGenerationTask batchTask, string outputDirectory)
    {
        try
        {
            var reportPath = Path.Combine(outputDirectory, batchTask.Request.Options.StatisticsReportFileName);
            
            // 将统计信息序列化为JSON
            var json = System.Text.Json.JsonSerializer.Serialize(batchTask.Statistics, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(reportPath, json);
            batchTask.Result.StatisticsReportFilePath = reportPath;
            
            _logger.LogInformation("生成统计报告成功: {ReportPath}", reportPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "生成统计报告失败");
        }
    }

    /// <summary>
    /// 生成索引文件
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    /// <param name="outputDirectory">输出目录</param>
    private async Task GenerateIndexFileAsync(AdvancedBatchGenerationTask batchTask, string outputDirectory)
    {
        try
        {
            var indexPath = Path.Combine(outputDirectory, batchTask.Request.OutputOptions.IndexFileName);
            
            // 创建索引数据
            var indexData = new
            {
                BatchId = batchTask.BatchId,
                BatchName = batchTask.Request.FileNamePrefix,
                CreatedAt = batchTask.StartTime,
                CompletedAt = batchTask.Result.CompletionTime,
                Status = batchTask.Status.ToString(),
                TotalCount = batchTask.Result.TotalCount,
                CompletedCount = batchTask.Result.CompletedCount,
                FailedCount = batchTask.Result.FailedCount,
                OutputDirectory = outputDirectory,
                GeneratedFiles = batchTask.Result.GeneratedFiles.Select(f => new
                {
                    f.FileName,
                    f.FileSize,
                    f.CreatedAt
                }),
                AnswerPaperFiles = batchTask.Result.AnswerPaperFiles.Select(f => new
                {
                    f.FileName,
                    f.FileSize,
                    f.CreatedAt
                }),
                Statistics = batchTask.Statistics
            };
            
            // 将索引数据序列化为JSON
            var json = System.Text.Json.JsonSerializer.Serialize(indexData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(indexPath, json);
            batchTask.Result.IndexFilePath = indexPath;
            
            _logger.LogInformation("生成索引文件成功: {IndexPath}", indexPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "生成索引文件失败");
        }
    }

    /// <summary>
    /// 发送完成通知
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    private async Task SendCompletionNotificationAsync(AdvancedBatchGenerationTask batchTask)
    {
        try
        {
            var message = $"批量生成任务 '{batchTask.Request.FileNamePrefix}' 已完成。";
            message += $"\n总数量: {batchTask.Result.TotalCount}";
            message += $"\n完成数量: {batchTask.Result.CompletedCount}";
            message += $"\n失败数量: {batchTask.Result.FailedCount}";
            message += $"\n输出目录: {batchTask.Result.ActualOutputDirectory}";
            
            switch (batchTask.Request.NotificationOptions.NotificationMethod)
            {
                case NotificationMethod.Log:
                    _logger.LogInformation("批量生成完成通知: {Message}", message);
                    break;
                    
                case NotificationMethod.Console:
                    Console.WriteLine(message);
                    break;
                    
                case NotificationMethod.Email:
                    // 这里可以添加邮件发送逻辑
                    _logger.LogInformation("发送邮件通知: {Message}", message);
                    break;
                    
                case NotificationMethod.Webhook:
                    // 这里可以添加Webhook调用逻辑
                    _logger.LogInformation("调用Webhook通知: {Message}", message);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送完成通知失败");
        }
    }

    /// <summary>
    /// 发送错误通知
    /// </summary>
    /// <param name="batchTask">批量任务</param>
    /// <param name="exception">异常</param>
    private async Task SendErrorNotificationAsync(AdvancedBatchGenerationTask batchTask, Exception exception)
    {
        try
        {
            var message = $"批量生成任务 '{batchTask.Request.FileNamePrefix}' 发生错误。";
            message += $"\n错误信息: {exception.Message}";
            message += $"\n输出目录: {batchTask.Result.ActualOutputDirectory}";
            
            switch (batchTask.Request.NotificationOptions.NotificationMethod)
            {
                case NotificationMethod.Log:
                    _logger.LogError("批量生成错误通知: {Message}", message);
                    break;
                    
                case NotificationMethod.Console:
                    Console.WriteLine($"错误: {message}");
                    break;
                    
                case NotificationMethod.Email:
                    // 这里可以添加邮件发送逻辑
                    _logger.LogError("发送邮件错误通知: {Message}", message);
                    break;
                    
                case NotificationMethod.Webhook:
                    // 这里可以添加Webhook调用逻辑
                    _logger.LogError("调用Webhook错误通知: {Message}", message);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送错误通知失败");
        }
    }

    #endregion
}