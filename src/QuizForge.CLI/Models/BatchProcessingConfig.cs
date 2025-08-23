namespace QuizForge.CLI.Models;

/// <summary>
/// 批处理配置
/// </summary>
public class BatchProcessingConfig
{
    /// <summary>
    /// 输入文件列表
    /// </summary>
    public List<string> InputFiles { get; set; } = new();

    /// <summary>
    /// 输出目录
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// 并发处理数量
    /// </summary>
    public int MaxConcurrency { get; set; } = 4;

    /// <summary>
    /// 是否继续处理失败文件
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// 输出文件名模式
    /// </summary>
    public string? OutputFileNamePattern { get; set; }
}