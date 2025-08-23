namespace QuizForge.CLI.Models;

/// <summary>
/// 简化的验证结果
/// </summary>
public class SimpleValidationResult
{
    /// <summary>
    /// 验证是否成功
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private SimpleValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 创建成功的验证结果
    /// </summary>
    /// <returns>验证结果</returns>
    public static SimpleValidationResult Success()
    {
        return new SimpleValidationResult(true);
    }

    /// <summary>
    /// 创建失败的验证结果
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>验证结果</returns>
    public static SimpleValidationResult Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("错误消息不能为空", nameof(errorMessage));
        }
        return new SimpleValidationResult(false, errorMessage);
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns>字符串表示</returns>
    public override string ToString()
    {
        return IsValid ? "验证成功" : $"验证失败: {ErrorMessage}";
    }
}