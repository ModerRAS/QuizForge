using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 批量生成服务接口
/// </summary>
public interface IBatchGenerationService
{
    /// <summary>
    /// 批量生成试卷
    /// </summary>
    /// <param name="request">批量生成请求</param>
    /// <returns>批量生成结果</returns>
    Task<BatchGenerationResult> BatchGenerateExamPapersAsync(BatchGenerationRequest request);

    /// <summary>
    /// 获取批量生成进度
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>批量生成进度</returns>
    Task<BatchGenerationProgress> GetBatchGenerationProgressAsync(Guid batchId);

    /// <summary>
    /// 取消批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>取消结果</returns>
    Task<bool> CancelBatchGenerationAsync(Guid batchId);

    /// <summary>
    /// 获取批量生成历史
    /// </summary>
    /// <param name="pageSize">页面大小</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>批量生成历史列表</returns>
    Task<PagedResult<BatchGenerationHistory>> GetBatchGenerationHistoryAsync(int pageSize = 10, int pageNumber = 1);

    /// <summary>
    /// 清理完成的批量生成任务
    /// </summary>
    /// <param name="olderThanDays">早于指定天数的任务</param>
    /// <returns>清理的任务数量</returns>
    Task<int> CleanupCompletedBatchGenerationsAsync(int olderThanDays = 30);

    /// <summary>
    /// 高级批量生成试卷
    /// </summary>
    /// <param name="request">高级批量生成请求</param>
    /// <returns>批量生成结果</returns>
    Task<AdvancedBatchGenerationResult> AdvancedBatchGenerateExamPapersAsync(AdvancedBatchGenerationRequest request);

    /// <summary>
    /// 获取批量生成的详细报告
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>批量生成详细报告</returns>
    Task<BatchGenerationReport> GetBatchGenerationReportAsync(Guid batchId);

    /// <summary>
    /// 暂停批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>暂停结果</returns>
    Task<bool> PauseBatchGenerationAsync(Guid batchId);

    /// <summary>
    /// 恢复批量生成任务
    /// </summary>
    /// <param name="batchId">批量任务ID</param>
    /// <returns>恢复结果</returns>
    Task<bool> ResumeBatchGenerationAsync(Guid batchId);
}

/// <summary>
/// 批量生成请求
/// </summary>
public class BatchGenerationRequest
{
    /// <summary>
    /// 题库ID列表
    /// </summary>
    public List<Guid> QuestionBankIds { get; set; } = new();

    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 生成数量
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 文件名前缀
    /// </summary>
    public string FileNamePrefix { get; set; } = "ExamPaper";

    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.PDF;

    /// <summary>
    /// 包含答案
    /// </summary>
    public bool IncludeAnswerKey { get; set; } = false;

    /// <summary>
    /// 并行生成数量
    /// </summary>
    public int ParallelCount { get; set; } = 1;

    /// <summary>
    /// 生成选项
    /// </summary>
    public BatchGenerationOptions Options { get; set; } = new();
}

/// <summary>
/// 批量生成选项
/// </summary>
public class BatchGenerationOptions
{
    /// <summary>
    /// 是否随机排序题目
    /// </summary>
    public bool RandomizeQuestions { get; set; } = false;

    /// <summary>
    /// 是否随机排序选项
    /// </summary>
    public bool RandomizeOptions { get; set; } = false;

    /// <summary>
    /// 每份试卷的最小题目数量
    /// </summary>
    public int MinQuestionCount { get; set; } = 0;

    /// <summary>
    /// 每份试卷的最大题目数量
    /// </summary>
    public int MaxQuestionCount { get; set; } = 0;

    /// <summary>
    /// 试卷难度范围
    /// </summary>
    public DifficultyRange DifficultyRange { get; set; } = new();

    /// <summary>
    /// 题目类型过滤
    /// </summary>
    public List<string> QuestionTypeFilter { get; set; } = new();
}

/// <summary>
/// 难度范围
/// </summary>
public class DifficultyRange
{
    /// <summary>
    /// 最小难度
    /// </summary>
    public decimal Min { get; set; } = 0;

    /// <summary>
    /// 最大难度
    /// </summary>
    public decimal Max { get; set; } = 10;
}

/// <summary>
/// 批量生成结果
/// </summary>
public class BatchGenerationResult
{
    /// <summary>
    /// 批量任务ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public BatchGenerationStatus Status { get; set; } = BatchGenerationStatus.Created;

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 预计完成时间
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletionTime { get; set; }

    /// <summary>
    /// 生成的文件列表
    /// </summary>
    public List<GeneratedFileInfo> GeneratedFiles { get; set; } = new();

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 批量生成状态
/// </summary>
public enum BatchGenerationStatus
{
    /// <summary>
    /// 已创建
    /// </summary>
    Created,

    /// <summary>
    /// 进行中
    /// </summary>
    InProgress,

    /// <summary>
    /// 已暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 失败
    /// </summary>
    Failed
}

/// <summary>
/// 生成的文件信息
/// </summary>
public class GeneratedFileInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 试卷ID
    /// </summary>
    public Guid ExamPaperId { get; set; }
}

/// <summary>
/// 批量生成进度
/// </summary>
public class BatchGenerationProgress
{
    /// <summary>
    /// 批量任务ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public BatchGenerationStatus Status { get; set; }

    /// <summary>
    /// 进度百分比
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 预计完成时间
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// 已运行时间（秒）
    /// </summary>
    public double ElapsedSeconds { get; set; }

    /// <summary>
    /// 剩余时间（秒）
    /// </summary>
    public double? RemainingSeconds { get; set; }

    /// <summary>
    /// 当前正在处理的文件
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 批量生成历史
/// </summary>
public class BatchGenerationHistory
{
    /// <summary>
    /// 批量任务ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务状态
    /// </summary>
    public BatchGenerationStatus Status { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletionTime { get; set; }

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 创建用户
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// 高级批量生成请求
/// </summary>
public class AdvancedBatchGenerationRequest
{
    /// <summary>
    /// 题库ID列表
    /// </summary>
    public List<Guid> QuestionBankIds { get; set; } = new();

    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 生成数量
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 文件名前缀
    /// </summary>
    public string FileNamePrefix { get; set; } = "ExamPaper";

    /// <summary>
    /// 文件名规则
    /// </summary>
    public FileNameRule FileNameRule { get; set; } = new();

    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.PDF;

    /// <summary>
    /// 包含答案
    /// </summary>
    public bool IncludeAnswerKey { get; set; } = false;

    /// <summary>
    /// 并行生成数量
    /// </summary>
    public int ParallelCount { get; set; } = 1;

    /// <summary>
    /// 生成选项
    /// </summary>
    public AdvancedBatchGenerationOptions Options { get; set; } = new();

    /// <summary>
    /// 输出组织选项
    /// </summary>
    public OutputOrganizationOptions OutputOptions { get; set; } = new();

    /// <summary>
    /// 通知选项
    /// </summary>
    public NotificationOptions NotificationOptions { get; set; } = new();
}

/// <summary>
/// 文件名规则
/// </summary>
public class FileNameRule
{
    /// <summary>
    /// 文件名模板，支持以下占位符：
    /// {prefix} - 文件名前缀
    /// {index} - 索引
    /// {date} - 日期
    /// {time} - 时间
    /// {random} - 随机字符串
    /// </summary>
    public string Template { get; set; } = "{prefix}_{index:D3}";

    /// <summary>
    /// 是否包含日期
    /// </summary>
    public bool IncludeDate { get; set; } = false;

    /// <summary>
    /// 日期格式
    /// </summary>
    public string DateFormat { get; set; } = "yyyyMMdd";

    /// <summary>
    /// 是否包含时间
    /// </summary>
    public bool IncludeTime { get; set; } = false;

    /// <summary>
    /// 时间格式
    /// </summary>
    public string TimeFormat { get; set; } = "HHmmss";

    /// <summary>
    /// 随机字符串长度
    /// </summary>
    public int RandomLength { get; set; } = 0;
}

/// <summary>
/// 高级批量生成选项
/// </summary>
public class AdvancedBatchGenerationOptions : BatchGenerationOptions
{
    /// <summary>
    /// 是否为每份试卷生成不同的题目组合
    /// </summary>
    public bool VaryQuestionCombination { get; set; } = false;

    /// <summary>
    /// 题目组合变化率（0-1）
    /// </summary>
    public double QuestionVariationRate { get; set; } = 0.3;

    /// <summary>
    /// 是否确保每份试卷的难度分布一致
    /// </summary>
    public bool EnsureConsistentDifficulty { get; set; } = true;

    /// <summary>
    /// 是否为每份试卷生成不同的题目顺序
    /// </summary>
    public bool VaryQuestionOrder { get; set; } = true;

    /// <summary>
    /// 是否为每份试卷生成不同的选项顺序
    /// </summary>
    public bool VaryOptionOrder { get; set; } = true;

    /// <summary>
    /// 是否生成答案试卷
    /// </summary>
    public bool GenerateAnswerPapers { get; set; } = false;

    /// <summary>
    /// 答案试卷文件名后缀
    /// </summary>
    public string AnswerPaperSuffix { get; set; } = "_Answer";

    /// <summary>
    /// 是否生成统计报告
    /// </summary>
    public bool GenerateStatisticsReport { get; set; } = false;

    /// <summary>
    /// 统计报告文件名
    /// </summary>
    public string StatisticsReportFileName { get; set; } = "StatisticsReport.json";
}

/// <summary>
/// 输出组织选项
/// </summary>
public class OutputOrganizationOptions
{
    /// <summary>
    /// 输出组织方式
    /// </summary>
    public OutputOrganizationMethod OrganizationMethod { get; set; } = OutputOrganizationMethod.Flat;

    /// <summary>
    /// 是否按日期创建子目录
    /// </summary>
    public bool CreateDateSubdirectories { get; set; } = false;

    /// <summary>
    /// 日期子目录格式
    /// </summary>
    public string DateSubdirectoryFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// 是否按批次创建子目录
    /// </summary>
    public bool CreateBatchSubdirectories { get; set; } = false;

    /// <summary>
    /// 批次子目录前缀
    /// </summary>
    public string BatchSubdirectoryPrefix { get; set; } = "Batch_";

    /// <summary>
    /// 最大文件数每目录
    /// </summary>
    public int MaxFilesPerDirectory { get; set; } = 1000;

    /// <summary>
    /// 是否创建索引文件
    /// </summary>
    public bool CreateIndexFile { get; set; } = false;

    /// <summary>
    /// 索引文件名
    /// </summary>
    public string IndexFileName { get; set; } = "index.json";
}

/// <summary>
/// 输出组织方式枚举
/// </summary>
public enum OutputOrganizationMethod
{
    /// <summary>
    /// 平铺结构
    /// </summary>
    Flat,

    /// <summary>
    /// 按日期组织
    /// </summary>
    ByDate,

    /// <summary>
    /// 按批次组织
    /// </summary>
    ByBatch,

    /// <summary>
    /// 按类型组织
    /// </summary>
    ByType
}

/// <summary>
/// 通知选项
/// </summary>
public class NotificationOptions
{
    /// <summary>
    /// 是否在完成时通知
    /// </summary>
    public bool NotifyOnCompletion { get; set; } = false;

    /// <summary>
    /// 是否在错误时通知
    /// </summary>
    public bool NotifyOnError { get; set; } = true;

    /// <summary>
    /// 通知方式
    /// </summary>
    public NotificationMethod NotificationMethod { get; set; } = NotificationMethod.Log;

    /// <summary>
    /// 通知邮箱地址
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// 通知Webhook URL
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// 进度更新间隔（秒）
    /// </summary>
    public int ProgressUpdateInterval { get; set; } = 5;
}

/// <summary>
/// 通知方式枚举
/// </summary>
public enum NotificationMethod
{
    /// <summary>
    /// 日志记录
    /// </summary>
    Log,

    /// <summary>
    /// 邮件通知
    /// </summary>
    Email,

    /// <summary>
    /// Webhook通知
    /// </summary>
    Webhook,

    /// <summary>
    /// 控制台输出
    /// </summary>
    Console
}

/// <summary>
/// 高级批量生成结果
/// </summary>
public class AdvancedBatchGenerationResult : BatchGenerationResult
{
    /// <summary>
    /// 答案试卷文件列表
    /// </summary>
    public List<GeneratedFileInfo> AnswerPaperFiles { get; set; } = new();

    /// <summary>
    /// 统计报告文件路径
    /// </summary>
    public string StatisticsReportFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 索引文件路径
    /// </summary>
    public string IndexFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 使用的输出目录
    /// </summary>
    public string ActualOutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 生成统计信息
    /// </summary>
    public GenerationStatistics Statistics { get; set; } = new();
}

/// <summary>
/// 生成统计信息
/// </summary>
public class GenerationStatistics
{
    /// <summary>
    /// 总生成时间（秒）
    /// </summary>
    public double TotalGenerationTime { get; set; }

    /// <summary>
    /// 平均每份试卷生成时间（秒）
    /// </summary>
    public double AverageGenerationTime { get; set; }

    /// <summary>
    /// 最快生成时间（秒）
    /// </summary>
    public double FastestGenerationTime { get; set; }

    /// <summary>
    /// 最慢生成时间（秒）
    /// </summary>
    public double SlowestGenerationTime { get; set; }

    /// <summary>
    /// 总题目数
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// 平均每份试卷题目数
    /// </summary>
    public double AverageQuestionsPerPaper { get; set; }

    /// <summary>
    /// 总分值
    /// </summary>
    public decimal TotalPoints { get; set; }

    /// <summary>
    /// 平均每份试卷分值
    /// </summary>
    public double AveragePointsPerPaper { get; set; }

    /// <summary>
    /// 题目类型分布
    /// </summary>
    public Dictionary<string, int> QuestionTypeDistribution { get; set; } = new();

    /// <summary>
    /// 难度分布
    /// </summary>
    public Dictionary<string, int> DifficultyDistribution { get; set; } = new();
}

/// <summary>
/// 批量生成报告
/// </summary>
public class BatchGenerationReport
{
    /// <summary>
    /// 批量任务ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务状态
    /// </summary>
    public BatchGenerationStatus Status { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletionTime { get; set; }

    /// <summary>
    /// 总生成时间（秒）
    /// </summary>
    public double TotalGenerationTime { get; set; }

    /// <summary>
    /// 生成统计信息
    /// </summary>
    public GenerationStatistics Statistics { get; set; } = new();

    /// <summary>
    /// 生成的文件列表
    /// </summary>
    public List<GeneratedFileInfo> GeneratedFiles { get; set; } = new();

    /// <summary>
    /// 答案试卷文件列表
    /// </summary>
    public List<GeneratedFileInfo> AnswerPaperFiles { get; set; } = new();

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告信息列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 配置信息
    /// </summary>
    public AdvancedBatchGenerationRequest Configuration { get; set; } = new();
}