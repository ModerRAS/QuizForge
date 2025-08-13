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