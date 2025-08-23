using QuizForge.Models;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI验证服务接口
/// </summary>
public interface ICliValidationService
{
    /// <summary>
    /// 验证文件路径
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileType">文件类型描述</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateFilePath(string filePath, string fileType = "文件");

    /// <summary>
    /// 验证输出目录
    /// </summary>
    /// <param name="outputPath">输出路径</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateOutputPath(string outputPath);

    /// <summary>
    /// 验证题库格式
    /// </summary>
    /// <param name="format">题库格式</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateQuestionBankFormat(string format);

    /// <summary>
    /// 验证模板名称
    /// </summary>
    /// <param name="templateName">模板名称</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateTemplateName(string templateName);

    /// <summary>
    /// 验证题库数据
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateQuestionBank(QuestionBank questionBank);

    /// <summary>
    /// 验证题目
    /// </summary>
    /// <param name="question">题目数据</param>
    /// <param name="questionNumber">题目编号</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateQuestion(Question question, int questionNumber = 0);

    /// <summary>
    /// 验证批处理配置
    /// </summary>
    /// <param name="batchConfig">批处理配置</param>
    /// <returns>验证结果</returns>
    SimpleValidationResult ValidateBatchConfig(BatchProcessingConfig batchConfig);

    /// <summary>
    /// 安全地执行操作并处理异常
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称</param>
    /// <param name="defaultValue">失败时的默认值</param>
    /// <returns>操作结果</returns>
    Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> operation, string operationName, T defaultValue = default!);

    /// <summary>
    /// 安全地执行操作并处理异常
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ExecuteSafelyAsync(Func<Task> operation, string operationName);
}