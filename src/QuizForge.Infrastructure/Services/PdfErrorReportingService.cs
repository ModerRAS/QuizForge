using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using QuizForge.Infrastructure.Exceptions;

namespace QuizForge.Infrastructure.Services;

/// <summary>
/// PDF错误报告服务
/// </summary>
public class PdfErrorReportingService
{
    private readonly ILogger<PdfErrorReportingService> _logger;
    private readonly List<PdfErrorReport> _errorReports = new();
    
    /// <summary>
    /// PDF错误报告服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public PdfErrorReportingService(ILogger<PdfErrorReportingService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 记录错误
    /// </summary>
    /// <param name="exception">异常</param>
    /// <param name="context">上下文信息</param>
    public void ReportError(PdfGenerationException exception, PdfErrorContext context)
    {
        var report = new PdfErrorReport
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            ErrorType = exception.ErrorType,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
            Context = context
        };
        
        _errorReports.Add(report);
        
        // 记录到日志
        _logger.LogError(exception, "PDF生成错误: {ErrorType} - {Message}", exception.ErrorType, exception.Message);
        
        // 如果是严重错误，可以发送通知或采取其他措施
        if (IsCriticalError(exception.ErrorType))
        {
            HandleCriticalError(report);
        }
    }
    
    /// <summary>
    /// 记录警告
    /// </summary>
    /// <param name="message">警告消息</param>
    /// <param name="context">上下文信息</param>
    public void ReportWarning(string message, PdfErrorContext context)
    {
        var report = new PdfErrorReport
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            ErrorType = PdfErrorType.Unknown,
            Message = message,
            IsWarning = true,
            Context = context
        };
        
        _errorReports.Add(report);
        
        // 记录到日志
        _logger.LogWarning("PDF生成警告: {Message}", message);
    }
    
    /// <summary>
    /// 获取所有错误报告
    /// </summary>
    /// <returns>错误报告列表</returns>
    public IReadOnlyList<PdfErrorReport> GetErrorReports()
    {
        return _errorReports.AsReadOnly();
    }
    
    /// <summary>
    /// 获取指定类型的错误报告
    /// </summary>
    /// <param name="errorType">错误类型</param>
    /// <returns>错误报告列表</returns>
    public IReadOnlyList<PdfErrorReport> GetErrorReportsByType(PdfErrorType errorType)
    {
        return _errorReports.Where(r => r.ErrorType == errorType).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// 获取指定时间范围内的错误报告
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>错误报告列表</returns>
    public IReadOnlyList<PdfErrorReport> GetErrorReportsByTimeRange(DateTime startTime, DateTime endTime)
    {
        return _errorReports.Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// 清除所有错误报告
    /// </summary>
    public void ClearErrorReports()
    {
        _errorReports.Clear();
    }
    
    /// <summary>
    /// 导出错误报告为文本
    /// </summary>
    /// <returns>错误报告文本</returns>
    public string ExportErrorReportsAsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("PDF生成错误报告");
        sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"错误总数: {_errorReports.Count}");
        sb.AppendLine(new string('-', 50));
        
        foreach (var report in _errorReports)
        {
            sb.AppendLine($"错误ID: {report.Id}");
            sb.AppendLine($"时间: {report.Timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"类型: {report.ErrorType}");
            sb.AppendLine($"级别: {(report.IsWarning ? "警告" : "错误")}");
            sb.AppendLine($"消息: {report.Message}");
            
            if (!string.IsNullOrEmpty(report.StackTrace))
            {
                sb.AppendLine($"堆栈跟踪: {report.StackTrace}");
            }
            
            if (!string.IsNullOrEmpty(report.InnerException))
            {
                sb.AppendLine($"内部异常: {report.InnerException}");
            }
            
            if (report.Context != null)
            {
                sb.AppendLine("上下文信息:");
                sb.AppendLine($"  操作: {report.Context.Operation}");
                sb.AppendLine($"  文件路径: {report.Context.FilePath}");
                sb.AppendLine($"  内容长度: {report.Context.ContentLength}");
                sb.AppendLine($"  引擎类型: {report.Context.EngineType}");
            }
            
            sb.AppendLine(new string('-', 50));
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// 导出错误报告为CSV
    /// </summary>
    /// <returns>错误报告CSV内容</returns>
    public string ExportErrorReportsAsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,时间,类型,级别,消息,操作,文件路径,内容长度,引擎类型");
        
        foreach (var report in _errorReports)
        {
            sb.AppendLine($"{report.Id},{report.Timestamp:yyyy-MM-dd HH:mm:ss},{report.ErrorType},{(report.IsWarning ? "警告" : "错误")}," +
                         $"\"{report.Message.Replace("\"", "\"\"")}\",\"{report.Context?.Operation}\"," +
                         $"\"{report.Context?.FilePath}\",{report.Context?.ContentLength},{report.Context?.EngineType}");
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// 判断是否为严重错误
    /// </summary>
    /// <param name="errorType">错误类型</param>
    /// <returns>是否为严重错误</returns>
    private bool IsCriticalError(PdfErrorType errorType)
    {
        return errorType switch
        {
            PdfErrorType.OutOfMemoryError => true,
            PdfErrorType.PermissionError => true,
            PdfErrorType.ConfigurationError => true,
            _ => false
        };
    }
    
    /// <summary>
    /// 处理严重错误
    /// </summary>
    /// <param name="report">错误报告</param>
    private void HandleCriticalError(PdfErrorReport report)
    {
        // 这里可以实现严重错误处理逻辑，如发送通知、记录到系统日志等
        _logger.LogCritical("检测到严重的PDF生成错误: {ErrorType} - {Message}", report.ErrorType, report.Message);
        
        // 可以添加更多处理逻辑，如发送邮件、写入系统事件日志等
    }
}

/// <summary>
/// PDF错误报告
/// </summary>
public class PdfErrorReport
{
    /// <summary>
    /// 错误ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 错误类型
    /// </summary>
    public PdfErrorType ErrorType { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// 内部异常消息
    /// </summary>
    public string? InnerException { get; set; }
    
    /// <summary>
    /// 是否为警告
    /// </summary>
    public bool IsWarning { get; set; }
    
    /// <summary>
    /// 上下文信息
    /// </summary>
    public PdfErrorContext? Context { get; set; }
}

/// <summary>
/// PDF错误上下文
/// </summary>
public class PdfErrorContext
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public string? Operation { get; set; }
    
    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// 内容长度
    /// </summary>
    public long ContentLength { get; set; }
    
    /// <summary>
    /// 引擎类型
    /// </summary>
    public string? EngineType { get; set; }
    
    /// <summary>
    /// 其他上下文数据
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}