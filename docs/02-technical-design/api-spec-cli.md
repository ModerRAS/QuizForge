# QuizForge CLI API和接口设计

## 接口概览

QuizForge CLI 提供命令行接口和内部服务接口，支持试卷生成的完整流程。接口设计遵循简洁性、一致性和可扩展性原则。

## 命令行接口设计

### 1. 主命令结构
```bash
quizforge [command] [options]

Commands:
  generate     生成单个试卷
  batch        批量生成试卷
  validate     验证题库文件
  template     模板管理
  config       配置管理
  help         显示帮助信息

Options:
  --version    显示版本信息
  --verbose    详细输出
  --quiet      静默模式
  --log-file   日志文件路径
```

### 2. generate 命令
```bash
quizforge generate [options]

Options:
  -i, --input <file>           输入Excel题库文件路径 (必需)
  -o, --output <file>          输出PDF文件路径 (必需)
  -t, --template <file>        LaTeX模板文件路径
  --title <text>               试卷标题
  --subject <text>             考试科目
  --question-count <number>    题目数量 (默认: 50)
  --difficulty <text>          难度级别 (简单/中等/困难)
  --category <text>            题目类别
  --random-questions           随机选择题目
  --include-answers            包含答案
  --exam-time <minutes>        考试时间 (分钟)
  --school-name <text>         学校名称
  --class <text>               班级名称
  --exam-date <date>           考试日期 (YYYY-MM-DD)
  --header-style <style>       页眉样式 (Standard/Compact/Minimal)
  --output-format <format>     输出格式 (PDF/LaTeX/Both)
  --timeout <seconds>          超时时间 (秒)

Examples:
  quizforge generate -i questions.xlsx -o exam.pdf
  quizforge generate -i math.xlsx -o math_exam.pdf --title "数学期末考试" --question-count 30
  quizforge generate -i physics.xlsx -o physics.pdf --random-questions --include-answers
```

### 3. batch 命令
```bash
quizforge batch [options]

Options:
  -i, --input <file>           输入Excel题库文件路径 (必需)
  -o, --output-dir <dir>       输出目录路径 (必需)
  -c, --count <number>         生成试卷数量 (必需)
  -t, --template <file>        LaTeX模板文件路径
  --prefix <text>              输出文件前缀 (默认: "exam_")
  --title <text>               试卷标题
  --question-count <number>    每份试卷题目数量
  --random-questions           每份试卷随机选择题目
  --include-answers            包含答案
  --parallel <number>          并行处理数量 (默认: 2)
  --overwrite                  覆盖已存在文件

Examples:
  quizforge batch -i questions.xlsx -o ./output -c 5
  quizforge batch -i math.xlsx -o ./exams -c 10 --prefix "math_exam_" --parallel 4
```

### 4. validate 命令
```bash
quizforge validate [options]

Options:
  -i, --input <file>           输入Excel题库文件路径 (必需)
  --detailed                   显示详细验证信息
  --format <format>            输出格式 (Text/JSON/XML)

Examples:
  quizforge validate -i questions.xlsx
  quizforge validate -i questions.xlsx --detailed --format JSON
```

### 5. template 命令
```bash
quizforge template [subcommand] [options]

Subcommands:
  list                          列出可用模板
  create <name>                 创建新模板
  validate <file>               验证模板文件
  install <file>                安装模板
  remove <name>                 移除模板

Examples:
  quizforge template list
  quizforge template create custom_template
  quizforge template validate my_template.tex
  quizforge template install custom_template.tex
```

### 6. config 命令
```bash
quizforge config [subcommand] [options]

Subcommands:
  show                          显示当前配置
  set <key> <value>             设置配置值
  get <key>                     获取配置值
  reset                         重置为默认配置
  init                          初始化配置文件

Examples:
  quizforge config show
  quizforge config set latex.timeout 300
  quizforge config get latex.executable_path
```

## 内部服务接口设计

### 1. CLI服务接口

#### ICliService
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// CLI服务接口
/// </summary>
public interface ICliService
{
    /// <summary>
    /// 生成试卷
    /// </summary>
    /// <param name="options">生成选项</param>
    /// <returns>生成结果</returns>
    Task<GenerationResult> GenerateExamPaperAsync(GenerationOptions options);
    
    /// <summary>
    /// 批量生成试卷
    /// </summary>
    /// <param name="options">批量生成选项</param>
    /// <returns>批量生成结果</returns>
    Task<BatchGenerationResult> BatchGenerateExamPapersAsync(BatchGenerationOptions options);
    
    /// <summary>
    /// 验证题库文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="detailed">是否显示详细信息</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateQuestionBankAsync(string filePath, bool detailed = false);
    
    /// <summary>
    /// 获取应用版本信息
    /// </summary>
    /// <returns>版本信息</returns>
    VersionInfo GetVersionInfo();
}
```

#### IFileService
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// 文件服务接口
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 验证输入文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    Task<FileValidationResult> ValidateInputFileAsync(string filePath);
    
    /// <summary>
    /// 确保输出目录存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>是否成功</returns>
    Task<bool> EnsureOutputDirectoryAsync(string directoryPath);
    
    /// <summary>
    /// 查找模板文件
    /// </summary>
    /// <param name="templateName">模板名称</param>
    /// <returns>模板文件路径</returns>
    Task<string?> FindTemplateFileAsync(string templateName);
    
    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件信息</returns>
    Task<FileInfo> GetFileInfoAsync(string filePath);
    
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否存在</returns>
    Task<bool> FileExistsAsync(string filePath);
}
```

#### IProgressService
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// 进度服务接口
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// 显示进度
    /// </summary>
    /// <param name="current">当前进度</param>
    /// <param name="total">总进度</param>
    /// <param name="message">进度消息</param>
    void ShowProgress(int current, int total, string message);
    
    /// <summary>
    /// 显示成功消息
    /// </summary>
    /// <param name="message">消息内容</param>
    void ShowSuccess(string message);
    
    /// <summary>
    /// 显示错误消息
    /// </summary>
    /// <param name="message">消息内容</param>
    void ShowError(string message);
    
    /// <summary>
    /// 显示警告消息
    /// </summary>
    /// <param name="message">消息内容</param>
    void ShowWarning(string message);
    
    /// <summary>
    /// 显示信息消息
    /// </summary>
    /// <param name="message">消息内容</param>
    void ShowInfo(string message);
    
    /// <summary>
    /// 开始进度条
    /// </summary>
    /// <param name="total">总进度</param>
    void StartProgress(int total);
    
    /// <summary>
    /// 更新进度条
    /// </summary>
    /// <param name="current">当前进度</param>
    void UpdateProgress(int current);
    
    /// <summary>
    /// 完成进度条
    /// </summary>
    void CompleteProgress();
}
```

#### IConfigurationService
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <returns>配置值</returns>
    T? GetValue<T>(string key);
    
    /// <summary>
    /// 设置配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    /// <returns>是否成功</returns>
    Task<bool> SetValueAsync(string key, object value);
    
    /// <summary>
    /// 保存配置
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> SaveConfigAsync();
    
    /// <summary>
    /// 重置配置
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> ResetConfigAsync();
    
    /// <summary>
    /// 获取所有配置
    /// </summary>
    /// <returns>配置字典</returns>
    Task<Dictionary<string, object>> GetAllConfigAsync();
}
```

### 2. 数据模型接口

#### GenerationOptions
```csharp
namespace QuizForge.CLI.Models;

/// <summary>
/// 生成选项
/// </summary>
public class GenerationOptions
{
    /// <summary>
    /// 输入文件路径
    /// </summary>
    public string InputFile { get; set; } = string.Empty;
    
    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string OutputFile { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板文件路径
    /// </summary>
    public string? Template { get; set; }
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 考试科目
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// 题目数量
    /// </summary>
    public int QuestionCount { get; set; } = 50;
    
    /// <summary>
    /// 难度级别
    /// </summary>
    public string? Difficulty { get; set; }
    
    /// <summary>
    /// 题目类别
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 是否随机选择题目
    /// </summary>
    public bool RandomQuestions { get; set; }
    
    /// <summary>
    /// 是否包含答案
    /// </summary>
    public bool IncludeAnswers { get; set; }
    
    /// <summary>
    /// 考试时间（分钟）
    /// </summary>
    public int ExamTime { get; set; } = 120;
    
    /// <summary>
    /// 学校名称
    /// </summary>
    public string? SchoolName { get; set; }
    
    /// <summary>
    /// 班级名称
    /// </summary>
    public string? Class { get; set; }
    
    /// <summary>
    /// 考试日期
    /// </summary>
    public string? ExamDate { get; set; }
    
    /// <summary>
    /// 页眉样式
    /// </summary>
    public string? HeaderStyle { get; set; } = "Standard";
    
    /// <summary>
    /// 输出格式
    /// </summary>
    public string? OutputFormat { get; set; } = "PDF";
    
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int Timeout { get; set; } = 300;
}
```

#### BatchGenerationOptions
```csharp
namespace QuizForge.CLI.Models;

/// <summary>
/// 批量生成选项
/// </summary>
public class BatchGenerationOptions
{
    /// <summary>
    /// 输入文件路径
    /// </summary>
    public string InputFile { get; set; } = string.Empty;
    
    /// <summary>
    /// 输出目录路径
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;
    
    /// <summary>
    /// 生成数量
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// 模板文件路径
    /// </summary>
    public string? Template { get; set; }
    
    /// <summary>
    /// 输出文件前缀
    /// </summary>
    public string Prefix { get; set; } = "exam_";
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 每份试卷题目数量
    /// </summary>
    public int QuestionCount { get; set; } = 50;
    
    /// <summary>
    /// 是否随机选择题目
    /// </summary>
    public bool RandomQuestions { get; set; }
    
    /// <summary>
    /// 是否包含答案
    /// </summary>
    public bool IncludeAnswers { get; set; }
    
    /// <summary>
    /// 并行处理数量
    /// </summary>
    public int ParallelCount { get; set; } = 2;
    
    /// <summary>
    /// 是否覆盖已存在文件
    /// </summary>
    public bool Overwrite { get; set; }
}
```

#### GenerationResult
```csharp
namespace QuizForge.CLI.Models;

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
    public string? OutputFile { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// 题目数量
    /// </summary>
    public int QuestionCount { get; set; }
    
    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// 额外信息
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
```

#### BatchGenerationResult
```csharp
namespace QuizForge.CLI.Models;

/// <summary>
/// 批量生成结果
/// </summary>
public class BatchGenerationResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 成功生成的文件列表
    /// </summary>
    public List<string> SuccessFiles { get; set; } = new();
    
    /// <summary>
    /// 失败的文件列表
    /// </summary>
    public List<string> FailedFiles { get; set; } = new();
    
    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();
    
    /// <summary>
    /// 总执行时间（毫秒）
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }
    
    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailureCount { get; set; }
}
```

### 3. 配置接口设计

#### LatexSettings
```csharp
namespace QuizForge.CLI.Configuration;

/// <summary>
/// LaTeX配置
/// </summary>
public class LatexSettings
{
    /// <summary>
    /// LaTeX可执行文件路径
    /// </summary>
    public string ExecutablePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 临时目录
    /// </summary>
    public string TempDirectory { get; set; } = "/tmp/quizforge";
    
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// 编译参数
    /// </summary>
    public string CompileArguments { get; set; } = "-interaction=nonstopmode";
    
    /// <summary>
    /// 是否启用PDF预览
    /// </summary>
    public bool EnablePreview { get; set; } = false;
}
```

#### FileSettings
```csharp
namespace QuizForge.CLI.Configuration;

/// <summary>
/// 文件配置
/// </summary>
public class FileSettings
{
    /// <summary>
    /// 最大文件大小（MB）
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 10;
    
    /// <summary>
    /// 支持的文件格式
    /// </summary>
    public List<string> SupportedFormats { get; set; } = new() { ".xlsx", ".xls" };
    
    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = "./output";
    
    /// <summary>
    /// 临时文件保留时间（小时）
    /// </summary>
    public int TempFileRetentionHours { get; set; } = 24;
}
```

#### LoggingSettings
```csharp
namespace QuizForge.CLI.Configuration;

/// <summary>
/// 日志配置
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel { get; set; } = "Information";
    
    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string? LogFilePath { get; set; }
    
    /// <summary>
    /// 最大日志文件大小（MB）
    /// </summary>
    public int MaxLogFileSizeMB { get; set; } = 10;
    
    /// <summary>
    /// 保留的日志文件数量
    /// </summary>
    public int RetainedLogFiles { get; set; } = 5;
    
    /// <summary>
    /// 是否启用控制台日志
    /// </summary>
    public bool EnableConsoleLog { get; set; } = true;
    
    /// <summary>
    /// 是否启用文件日志
    /// </summary>
    public bool EnableFileLog { get; set; } = false;
}
```

## 错误处理接口

### 1. 异常类型

#### CliException
```csharp
namespace QuizForge.CLI.Exceptions;

/// <summary>
/// CLI异常基类
/// </summary>
public class CliException : Exception
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// 错误级别
    /// </summary>
    public ErrorLevel ErrorLevel { get; }
    
    /// <summary>
    /// 用户友好消息
    /// </summary>
    public string UserMessage { get; }
    
    public CliException(string errorCode, string userMessage, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
        ErrorLevel = ErrorLevel.Error;
    }
    
    public CliException(string errorCode, string userMessage, ErrorLevel errorLevel, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
        ErrorLevel = errorLevel;
    }
}
```

#### FileValidationException
```csharp
namespace QuizForge.CLI.Exceptions;

/// <summary>
/// 文件验证异常
/// </summary>
public class FileValidationException : CliException
{
    public string FilePath { get; }
    
    public FileValidationException(string filePath, string message)
        : base("FILE_VALIDATION_ERROR", $"文件验证失败: {filePath} - {message}")
    {
        FilePath = filePath;
    }
}
```

#### LatexGenerationException
```csharp
namespace QuizForge.CLI.Exceptions;

/// <summary>
/// LaTeX生成异常
/// </summary>
public class LatexGenerationException : CliException
{
    public string? LatexContent { get; }
    
    public LatexGenerationException(string message, string? latexContent = null, Exception? innerException = null)
        : base("LATEX_GENERATION_ERROR", message, ErrorLevel.Error, innerException)
    {
        LatexContent = latexContent;
    }
}
```

### 2. 错误代码定义

#### ErrorCodes
```csharp
namespace QuizForge.CLI.Errors;

/// <summary>
/// 错误代码定义
/// </summary>
public static class ErrorCodes
{
    // 文件相关错误 (1000-1999)
    public const string FILE_NOT_FOUND = "1001";
    public const string FILE_INVALID_FORMAT = "1002";
    public const string FILE_TOO_LARGE = "1003";
    public const string FILE_ACCESS_DENIED = "1004";
    public const string FILE_ALREADY_EXISTS = "1005";
    
    // 参数相关错误 (2000-2999)
    public const string INVALID_ARGUMENT = "2001";
    public const string MISSING_REQUIRED_ARGUMENT = "2002";
    public const string INVALID_ARGUMENT_VALUE = "2003";
    public const string ARGUMENT_CONFLICT = "2004";
    
    // LaTeX相关错误 (3000-3999)
    public const string LATEX_COMPILATION_FAILED = "3001";
    public const string LATEX_NOT_FOUND = "3002";
    public const string LATEX_TIMEOUT = "3003";
    public const string LATEX_INVALID_CONTENT = "3004";
    
    // 模板相关错误 (4000-4999)
    public const string TEMPLATE_NOT_FOUND = "4001";
    public const string TEMPLATE_INVALID = "4002";
    public const string TEMPLATE_COMPILATION_FAILED = "4003";
    
    // 系统相关错误 (5000-5999)
    public const string SYSTEM_ERROR = "5001";
    public const string CONFIGURATION_ERROR = "5002";
    public const string PERMISSION_DENIED = "5003";
    public const string OUT_OF_MEMORY = "5004";
}
```

## 事件接口设计

### 1. 事件类型

#### GenerationProgressEvent
```csharp
namespace QuizForge.CLI.Events;

/// <summary>
/// 生成进度事件
/// </summary>
public class GenerationProgressEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; } = "GenerationProgress";
    
    /// <summary>
    /// 当前步骤
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;
    
    /// <summary>
    /// 进度百分比
    /// </summary>
    public double ProgressPercentage { get; set; }
    
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

#### GenerationCompletedEvent
```csharp
namespace QuizForge.CLI.Events;

/// <summary>
/// 生成完成事件
/// </summary>
public class GenerationCompletedEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; } = "GenerationCompleted";
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string? OutputFile { get; set; }
    
    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 2. 事件处理器接口

#### IEventHandler
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// 事件处理器接口
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="event">事件对象</param>
    Task HandleEventAsync(object @event);
    
    /// <summary>
    /// 是否支持指定事件类型
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>是否支持</returns>
    bool CanHandle(string eventType);
}
```

#### IEventPublisher
```csharp
namespace QuizForge.CLI.Interfaces;

/// <summary>
/// 事件发布器接口
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="event">事件对象</param>
    Task PublishAsync(object @event);
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="handler">事件处理器</param>
    void Subscribe(IEventHandler handler);
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="handler">事件处理器</param>
    void Unsubscribe(IEventHandler handler);
}
```

## 总结

QuizForge CLI 的接口设计遵循了以下原则：

1. **简洁性**: 接口设计简单明了，易于理解和使用
2. **一致性**: 接口命名和参数风格保持一致
3. **可扩展性**: 支持未来功能扩展和新的命令类型
4. **错误处理**: 完善的错误处理机制和用户友好的错误消息
5. **异步支持**: 所有I/O操作都支持异步处理
6. **配置驱动**: 支持灵活的配置管理和运行时配置

这些接口设计为QuizForge CLI提供了完整的功能支持，从命令行解析到内部业务逻辑处理，再到错误处理和事件通知，形成了完整的接口体系。