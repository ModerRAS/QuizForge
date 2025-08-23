using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// Excel解析器接口
/// </summary>
public interface IExcelParser
{
    /// <summary>
    /// 解析Excel文件内容为题库
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <returns>解析后的题库</returns>
    Task<QuestionBank> ParseAsync(string filePath);
    
    /// <summary>
    /// 验证Excel文件格式
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateFormatAsync(string filePath);
    
    /// <summary>
    /// 导出题库到Excel文件
    /// </summary>
    /// <param name="questionBank">题库数据</param>
    /// <param name="filePath">输出文件路径</param>
    /// <returns>导出结果</returns>
    Task<bool> ExportAsync(QuestionBank questionBank, string filePath);
}