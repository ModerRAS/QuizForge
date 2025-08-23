using Microsoft.Extensions.Options;
using QuizForge.CLI.Models;
using Spectre.Console;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI进度服务接口
/// </summary>
public interface ICliProgressService
{
    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="totalSteps">总步骤数</param>
    /// <param name="description">描述</param>
    /// <returns>进度上下文</returns>
    Task<IProgressContext> StartProgressAsync(int totalSteps, string description);

    /// <summary>
    /// 显示状态
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="description">描述</param>
    void ShowStatus(string status, string description = "");

    /// <summary>
    /// 显示错误
    /// </summary>
    /// <param name="error">错误消息</param>
    void ShowError(string error);

    /// <summary>
    /// 显示警告
    /// </summary>
    /// <param name="warning">警告消息</param>
    void ShowWarning(string warning);

    /// <summary>
    /// 显示信息
    /// </summary>
    /// <param name="message">消息</param>
    void ShowInfo(string message);

    /// <summary>
    /// 显示成功
    /// </summary>
    /// <param name="message">消息</param>
    void ShowSuccess(string message);

    /// <summary>
    /// 显示标题
    /// </summary>
    /// <param name="title">标题</param>
    void ShowTitle(string title);

    /// <summary>
    /// 显示表格
    /// </summary>
    /// <param name="title">表格标题</param>
    /// <param name="columns">列名</param>
    /// <param name="rows">数据行</param>
    void ShowTable(string title, string[] columns, List<string[]> rows);

    /// <summary>
    /// 显示验证结果
    /// </summary>
    /// <param name="result">验证结果</param>
    void ShowValidationResult(Models.ValidationResult result);
}

/// <summary>
/// 进度上下文接口
/// </summary>
public interface IProgressContext : IDisposable
{
    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="currentStep">当前步骤</param>
    /// <param name="description">描述</param>
    void Update(int currentStep, string description);

    /// <summary>
    /// 完成进度
    /// </summary>
    void Complete();

    /// <summary>
    /// 失败
    /// </summary>
    void Fail(string error);
}

/// <summary>
/// CLI进度服务实现
/// </summary>
public class CliProgressService : ICliProgressService
{
    private readonly ILogger<CliProgressService> _logger;
    private readonly CliOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">CLI选项</param>
    public CliProgressService(ILogger<CliProgressService> logger, IOptions<CliOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<IProgressContext> StartProgressAsync(int totalSteps, string description)
    {
        if (!_options.ShowProgress)
        {
            ShowInfo(description);
            return new NullProgressContext(this);
        }

        return await Task.FromResult<IProgressContext>(new SpectreProgressContext(totalSteps, description));
    }

    /// <inheritdoc/>
    public void ShowStatus(string status, string description = "")
    {
        var message = string.IsNullOrEmpty(description) ? status : $"{status}: {description}";
        
        if (_options.ColoredOutput)
        {
            AnsiConsole.MarkupLine($"[blue]{EscapeMarkup(message)}[/]");
        }
        else
        {
            Console.WriteLine(message);
        }

        _logger.LogInformation("状态: {Message}", message);
    }

    /// <inheritdoc/>
    public void ShowError(string error)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.MarkupLine($"[red]错误: {EscapeMarkup(error)}[/]");
        }
        else
        {
            Console.WriteLine($"错误: {error}");
        }

        _logger.LogError("错误: {Error}", error);
    }

    /// <inheritdoc/>
    public void ShowWarning(string warning)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.MarkupLine($"[yellow]警告: {EscapeMarkup(warning)}[/]");
        }
        else
        {
            Console.WriteLine($"警告: {warning}");
        }

        _logger.LogWarning("警告: {Warning}", warning);
    }

    /// <inheritdoc/>
    public void ShowInfo(string message)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.MarkupLine($"[grey]{EscapeMarkup(message)}[/]");
        }
        else
        {
            Console.WriteLine(message);
        }

        _logger.LogInformation("信息: {Message}", message);
    }

    /// <inheritdoc/>
    public void ShowSuccess(string message)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.MarkupLine($"[green]✓ {EscapeMarkup(message)}[/]");
        }
        else
        {
            Console.WriteLine($"✓ {message}");
        }

        _logger.LogInformation("成功: {Message}", message);
    }

    /// <inheritdoc/>
    public void ShowTitle(string title)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold]{EscapeMarkup(title)}[/]").DoubleBorder().HeavyBorder());
            AnsiConsole.WriteLine();
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();
        }
    }

    /// <inheritdoc/>
    public void ShowTable(string title, string[] columns, List<string[]> rows)
    {
        if (_options.ColoredOutput)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold]{EscapeMarkup(title)}[/]").DoubleBorder());
            AnsiConsole.WriteLine();

            var table = new Table();
            foreach (var column in columns)
            {
                table.AddColumn(new TableColumn(EscapeMarkup(column)).LeftAligned());
            }

            foreach (var row in rows)
            {
                var escapedRow = row.Select(EscapeMarkup).ToArray();
                table.AddRow(escapedRow);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();

            // 简单的表格格式
            var columnWidths = columns.Select(c => Math.Max(c.Length, 10)).ToArray();
            
            // 计算每列的最大宽度
            foreach (var row in rows)
            {
                for (int i = 0; i < Math.Min(row.Length, columnWidths.Length); i++)
                {
                    columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
                }
            }

            // 打印表头
            var headerLine = string.Join(" | ", columns.Select((c, i) => c.PadRight(columnWidths[i])));
            Console.WriteLine(headerLine);
            Console.WriteLine(new string('-', headerLine.Length));

            // 打印数据行
            foreach (var row in rows)
            {
                var rowLine = string.Join(" | ", row.Select((c, i) => c.PadRight(columnWidths[i])));
                Console.WriteLine(rowLine);
            }

            Console.WriteLine();
        }
    }

    /// <inheritdoc/>
    public void ShowValidationResult(Models.ValidationResult result)
    {
        ShowTitle("文件验证结果");

        if (result.IsValid)
        {
            ShowSuccess("文件验证通过");
        }
        else
        {
            ShowError("文件验证失败");
        }

        foreach (var info in result.Information)
        {
            ShowInfo(info);
        }

        foreach (var warning in result.Warnings)
        {
            ShowWarning(warning);
        }

        foreach (var error in result.Errors)
        {
            ShowError(error);
        }

        if (result.FileInfo != null)
        {
            ShowInfo($"文件大小: {result.FileInfo.Length} bytes");
            ShowInfo($"修改时间: {result.FileInfo.LastWriteTime}");
        }
    }

    /// <summary>
    /// 转义标记字符
    /// </summary>
    /// <param name="text">要转义的文本</param>
    /// <returns>转义后的文本</returns>
    private string EscapeMarkup(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text
            .Replace("[", "[[")
            .Replace("]", "]]")
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}

/// <summary>
/// Spectre进度上下文
/// </summary>
public class SpectreProgressContext : IProgressContext
{
    private ProgressTask? _task;
    private bool _completed = false;

    public SpectreProgressContext(int totalSteps, string description)
    {
        _task = AnsiConsole.Progress()
            .AutoClear(true)
            .AutoRefresh(true)
            .HideCompleted(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
            })
            .Start(ctx =>
            {
                return ctx.AddTask(description, new ProgressTaskSettings
                {
                    MaxValue = totalSteps,
                    AutoStart = true
                });
            });
    }

    public void Update(int currentStep, string description)
    {
        if (!_completed && _task != null)
        {
            _task.Value = Math.Min(currentStep, _task.MaxValue);
            _task.Description = description;
        }
    }

    public void Complete()
    {
        if (!_completed && _task != null)
        {
            _task.Value = _task.MaxValue;
            _task.Description = "完成";
            _completed = true;
        }
    }

    public void Fail(string error)
    {
        if (!_completed && _task != null)
        {
            _task.Description = $"失败: {error}";
            _completed = true;
        }
    }

    public void Dispose()
    {
        Complete();
    }
}

/// <summary>
/// 空进度上下文（当进度显示禁用时使用）
/// </summary>
public class NullProgressContext : IProgressContext
{
    private readonly CliProgressService _progressService;

    public NullProgressContext(CliProgressService progressService)
    {
        _progressService = progressService;
    }

    public void Update(int currentStep, string description)
    {
        _progressService.ShowInfo($"步骤 {currentStep}: {description}");
    }

    public void Complete()
    {
        _progressService.ShowSuccess("完成");
    }

    public void Fail(string error)
    {
        _progressService.ShowError(error);
    }

    public void Dispose()
    {
        Complete();
    }
}