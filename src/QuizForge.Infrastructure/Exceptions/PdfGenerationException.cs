using QuizForge.Models.Exceptions;

namespace QuizForge.Infrastructure.Exceptions;

/// <summary>
/// PDF生成异常
/// </summary>
public class PdfGenerationException : QuizForgeException
{
    /// <summary>
    /// PDF生成异常构造函数
    /// </summary>
    public PdfGenerationException() : base()
    {
    }
    
    /// <summary>
    /// PDF生成异常构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public PdfGenerationException(string message) : base(message)
    {
    }
    
    /// <summary>
    /// PDF生成异常构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public PdfGenerationException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    /// <summary>
    /// PDF生成异常构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="errorType">错误类型</param>
    public PdfGenerationException(string message, PdfErrorType errorType) : base(message)
    {
        ErrorType = errorType;
    }
    
    /// <summary>
    /// PDF生成异常构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="errorType">错误类型</param>
    /// <param name="innerException">内部异常</param>
    public PdfGenerationException(string message, PdfErrorType errorType, Exception innerException) : base(message, innerException)
    {
        ErrorType = errorType;
    }
    
    /// <summary>
    /// 错误类型
    /// </summary>
    public PdfErrorType ErrorType { get; } = PdfErrorType.Unknown;
}

/// <summary>
/// PDF错误类型
/// </summary>
public enum PdfErrorType
{
    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown,
    
    /// <summary>
    /// LaTeX解析错误
    /// </summary>
    LatexParsingError,
    
    /// <summary>
    /// 字体加载错误
    /// </summary>
    FontLoadingError,
    
    /// <summary>
    /// 数学公式渲染错误
    /// </summary>
    MathRenderingError,
    
    /// <summary>
    /// 文件IO错误
    /// </summary>
    FileIoError,
    
    /// <summary>
    /// 内存不足错误
    /// </summary>
    OutOfMemoryError,
    
    /// <summary>
    /// 权限错误
    /// </summary>
    PermissionError,
    
    /// <summary>
    /// 配置错误
    /// </summary>
    ConfigurationError,
    
    /// <summary>
    /// 超时错误
    /// </summary>
    TimeoutError
}