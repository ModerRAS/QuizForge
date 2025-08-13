namespace QuizForge.Models.Exceptions;

/// <summary>
/// QuizForge 自定义异常基类
/// </summary>
public class QuizForgeException : Exception
{
    /// <summary>
    /// 初始化 QuizForgeException 类的新实例
    /// </summary>
    public QuizForgeException() : base()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 QuizForgeException 类的新实例
    /// </summary>
    /// <param name="message">描述错误的消息</param>
    public QuizForgeException(string message) : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和对作为此异常原因的内部异常的引用来初始化 QuizForgeException 类的新实例
    /// </summary>
    /// <param name="message">描述错误的消息</param>
    /// <param name="innerException">导致当前异常的异常</param>
    public QuizForgeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}