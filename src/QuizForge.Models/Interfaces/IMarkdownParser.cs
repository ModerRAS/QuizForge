using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// Markdown解析器接口
/// </summary>
public interface IMarkdownParser
{
    /// <summary>
    /// 解析Markdown文件内容为题库
    /// </summary>
    /// <param name="filePath">Markdown文件路径</param>
    /// <returns>解析后的题库</returns>
    Task<QuestionBank> ParseAsync(string filePath);
    
    /// <summary>
    /// 验证Markdown文件格式
    /// </summary>
    /// <param name="filePath">Markdown文件路径</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateFormatAsync(string filePath);
    
    /// <summary>
    /// 导出题库到Markdown文件
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <param name="filePath">输出文件路径</param>
    /// <returns>导出结果</returns>
    Task<bool> ExportAsync(QuestionBank questionBank, string filePath);
}