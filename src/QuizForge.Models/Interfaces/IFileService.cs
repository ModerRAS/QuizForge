using System.Collections.Generic;
namespace QuizForge.Models.Interfaces;

/// <summary>
/// 文件服务接口
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    bool FileExists(string filePath);
    
    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>目录是否存在</returns>
    bool DirectoryExists(string directoryPath);
    
    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    void CreateDirectory(string directoryPath);
    
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件内容</returns>
    Task<string> ReadFileAsync(string filePath);
    
    /// <summary>
    /// 写入文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">内容</param>
    Task WriteFileAsync(string filePath, string content);
    
    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destinationPath">目标文件路径</param>
    /// <param name="overwrite">是否覆盖</param>
    Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false);
    
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    Task DeleteFileAsync(string filePath);
    
    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件扩展名</returns>
    string GetFileExtension(string filePath);
    
    /// <summary>
    /// 获取文件名（不含扩展名）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件名</returns>
    string GetFileNameWithoutExtension(string filePath);
    
    /// <summary>
    /// 将PDF转换为Word
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="wordPath">Word文件路径</param>
    Task ConvertPdfToWordAsync(string pdfPath, string wordPath);
    
    /// <summary>
    /// 打印文档
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="settings">打印设置</param>
    Task<bool> PrintDocumentAsync(string filePath, PrintSettings settings);
    
    /// <summary>
    /// 获取可用打印机列表
    /// </summary>
    /// <returns>打印机名称列表</returns>
    Task<List<string>> GetAvailablePrintersAsync();
}