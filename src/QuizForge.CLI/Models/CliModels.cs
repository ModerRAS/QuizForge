using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;

namespace QuizForge.CLI.Models;

/// <summary>
/// LaTeX配置选项
/// </summary>
public class LaTeXOptions
{
    /// <summary>
    /// 默认模板名称
    /// </summary>
    public string DefaultTemplate { get; set; } = "standard";

    /// <summary>
    /// 临时目录路径
    /// </summary>
    public string TempDirectory { get; set; } = "/tmp/quizforge";

    /// <summary>
    /// 是否启用中文支持
    /// </summary>
    public bool EnableChineseSupport { get; set; } = true;

    /// <summary>
    /// 文档类
    /// </summary>
    public string DocumentClass { get; set; } = "article";

    /// <summary>
    /// 字体大小
    /// </summary>
    public string FontSize { get; set; } = "12pt";

    /// <summary>
    /// 页面边距
    /// </summary>
    public string PageMargin { get; set; } = "2.5cm";
}

/// <summary>
/// Excel配置选项
/// </summary>
public class ExcelOptions
{
    /// <summary>
    /// 默认工作表索引
    /// </summary>
    public int DefaultSheetIndex { get; set; } = 1;

    /// <summary>
    /// 标题行关键词
    /// </summary>
    public List<string> HeaderRowKeywords { get; set; } = new() { "题型", "题目", "答案" };

    /// <summary>
    /// 最大行数
    /// </summary>
    public int MaxRows { get; set; } = 1000;

    /// <summary>
    /// 文件编码
    /// </summary>
    public string Encoding { get; set; } = "UTF-8";
}

/// <summary>
/// PDF配置选项
/// </summary>
public class PdfOptions
{
    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = "./output";

    /// <summary>
    /// 默认DPI
    /// </summary>
    public int DefaultDPI { get; set; } = 300;

    /// <summary>
    /// 是否启用预览
    /// </summary>
    public bool EnablePreview { get; set; } = true;

    /// <summary>
    /// 是否自动清理临时文件
    /// </summary>
    public bool AutoCleanup { get; set; } = true;
}

/// <summary>
/// 模板配置选项
/// </summary>
public class TemplateOptions
{
    /// <summary>
    /// 模板目录
    /// </summary>
    public string Directory { get; set; } = "./templates";

    /// <summary>
    /// 默认模板文件
    /// </summary>
    public string DefaultTemplate { get; set; } = "standard.tex";

    /// <summary>
    /// 自定义模板列表
    /// </summary>
    public List<string> CustomTemplates { get; set; } = new();
}

/// <summary>
/// CLI配置选项
/// </summary>
public class CliOptions
{
    /// <summary>
    /// 是否显示进度
    /// </summary>
    public bool ShowProgress { get; set; } = true;

    /// <summary>
    /// 是否使用彩色输出
    /// </summary>
    public bool ColoredOutput { get; set; } = true;

    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// 是否自动创建目录
    /// </summary>
    public bool AutoCreateDirectories { get; set; } = true;
}

/// <summary>
/// CLI命令参数
/// </summary>
public class CliCommandParameters : CommandSettings
{
    /// <summary>
    /// 输入文件路径
    /// </summary>
    [CommandOption("-i|--input")]
        public string InputFile { get; set; } = string.Empty;

    /// <summary>
    /// 输出文件路径
    /// </summary>
    [CommandOption("-o|--output")]
        public string OutputFile { get; set; } = string.Empty;

    /// <summary>
    /// 模板名称
    /// </summary>
    [CommandOption("-t|--template")]
        public string Template { get; set; } = "standard";

    /// <summary>
    /// 标题
    /// </summary>
    [CommandOption("--title")]
        public string Title { get; set; } = "试卷";

    /// <summary>
    /// 科目
    /// </summary>
    [CommandOption("--subject")]
        public string Subject { get; set; } = "通用";

    /// <summary>
    /// 考试时间（分钟）
    /// </summary>
    [CommandOption("--time")]
        public int ExamTime { get; set; } = 120;

    /// <summary>
    /// 是否验证输入文件
    /// </summary>
    [CommandOption("--validate")]
        public new bool Validate { get; set; } = true;

    /// <summary>
    /// 是否显示详细输出
    /// </summary>
    [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; } = false;

    /// <summary>
    /// 是否显示进度条
    /// </summary>
    [CommandOption("--progress")]
        public bool ShowProgress { get; set; } = true;

    /// <summary>
    /// 配置文件路径
    /// </summary>
    [CommandOption("-c|--config")]
        public string ConfigFile { get; set; } = string.Empty;
}

/// <summary>
/// 批量处理参数
/// </summary>
public class BatchParameters : CliCommandParameters
{
    /// <summary>
    /// 输入目录
    /// </summary>
    [CommandOption("--input-dir")]
        public string InputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 输出目录
    /// </summary>
    [CommandOption("--output-dir")]
        public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 文件过滤模式
    /// </summary>
    [CommandOption("--pattern")]
        public string FilePattern { get; set; } = "*.xlsx";

    /// <summary>
    /// 并行处理数量
    /// </summary>
    [CommandOption("--parallel")]
        public int MaxParallel { get; set; } = 4;

    /// <summary>
    /// 失败时是否继续
    /// </summary>
    [CommandOption("--continue-on-error")]
        public bool ContinueOnError { get; set; } = false;
}

/// <summary>
/// 模板参数
/// </summary>
public class TemplateParameters : CommandSettings
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [CommandOption("-n|--name")]
        public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板文件路径
    /// </summary>
    [CommandOption("-f|--file")]
        public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    [CommandOption("-d|--description")]
        public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认模板
    /// </summary>
    [CommandOption("--default")]
        public bool IsDefault { get; set; } = false;
}

/// <summary>
/// 配置参数
/// </summary>
public class ConfigParameters : CommandSettings
{
    /// <summary>
    /// 配置键
    /// </summary>
    [CommandOption("-k|--key")]
        public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 配置值
    /// </summary>
    [CommandOption("-v|--value")]
        public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 配置文件路径
    /// </summary>
    [CommandOption("-c|--config")]
        public string ConfigFile { get; set; } = string.Empty;

    /// <summary>
    /// 是否显示所有配置
    /// </summary>
    [CommandOption("-a|--all")]
        public bool ShowAll { get; set; } = false;
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告消息列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 信息消息列表
    /// </summary>
    public List<string> Information { get; set; } = new();

    /// <summary>
    /// 文件信息
    /// </summary>
    public FileInfo? FileInfo { get; set; }

    /// <summary>
    /// 错误消息（用于向后兼容）
    /// </summary>
    public string? ErrorMessage => Errors.Count > 0 ? string.Join("; ", Errors) : null;
}

/// <summary>
/// 生成结果
/// </summary>
public class GenerationResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// 题目数量
    /// </summary>
    public int QuestionCount { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 预览图像数据
    /// </summary>
    public byte[]? PreviewData { get; set; }
}

/// <summary>
/// 批量生成结果
/// </summary>
public class BatchGenerationResult
{
    /// <summary>
    /// 总文件数
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 成功结果列表
    /// </summary>
    public List<GenerationResult> SuccessResults { get; set; } = new();

    /// <summary>
    /// 失败结果列表
    /// </summary>
    public List<GenerationResult> FailureResults { get; set; } = new();

    /// <summary>
    /// 总处理时间
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }
}

/// <summary>
/// 模板信息
/// </summary>
public class TemplateInfo
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 模板文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认模板
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}