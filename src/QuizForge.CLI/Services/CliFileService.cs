using Microsoft.Extensions.Options;
using QuizForge.CLI.Models;
using QuizForge.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI文件服务接口
/// </summary>
public interface ICliFileService
{
    /// <summary>
    /// 验证文件是否存在且可访问
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateFileAsync(string filePath);

    /// <summary>
    /// 确保目录存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>是否成功</returns>
    Task<bool> EnsureDirectoryExistsAsync(string directoryPath);

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件信息</returns>
    Task<FileInfo?> GetFileInfoAsync(string filePath);

    /// <summary>
    /// 获取目录中的文件列表
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="pattern">文件模式</param>
    /// <returns>文件列表</returns>
    Task<List<FileInfo>> GetFilesAsync(string directoryPath, string pattern = "*.*");

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destinationPath">目标文件路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <returns>是否成功</returns>
    Task<bool> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="olderThan">清理早于此时间的文件</param>
    /// <returns>清理的文件数量</returns>
    Task<int> CleanupTempFilesAsync(string directoryPath, TimeSpan? olderThan = null);
}

/// <summary>
/// CLI文件服务实现
/// </summary>
public class CliFileService : ICliFileService
{
    private readonly ILogger<CliFileService> _logger;
    private readonly CliOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">CLI选项</param>
    public CliFileService(ILogger<CliFileService> logger, IOptions<CliOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateFileAsync(string filePath)
    {
        var result = new ValidationResult();

        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Errors.Add("文件路径不能为空");
                return result;
            }

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                result.Errors.Add($"文件不存在: {filePath}");
                return result;
            }

            // 获取文件信息
            var fileInfo = new FileInfo(filePath);
            result.FileInfo = fileInfo;

            // 检查文件大小
            if (fileInfo.Length == 0)
            {
                result.Errors.Add("文件为空");
                return result;
            }

            if (fileInfo.Length > 100 * 1024 * 1024) // 100MB
            {
                result.Warnings.Add("文件较大，处理可能需要较长时间");
            }

            // 检查文件访问权限
            try
            {
                using var stream = File.OpenRead(filePath);
                stream.Close();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"无法访问文件: {ex.Message}");
                return result;
            }

            // 检查文件扩展名
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var supportedExtensions = new[] { ".xlsx", ".xls", ".md", ".tex" };
            
            if (!supportedExtensions.Contains(extension))
            {
                result.Warnings.Add($"不支持的文件扩展名: {extension}");
            }

            result.IsValid = true;
            result.Information.Add($"文件验证通过: {fileInfo.Name} ({fileInfo.Length} bytes)");

            _logger.LogInformation("文件验证通过: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"文件验证失败: {ex.Message}");
            _logger.LogError(ex, "文件验证失败: {FilePath}", filePath);
        }

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                _logger.LogWarning("目录路径为空");
                return false;
            }

            if (!Directory.Exists(directoryPath))
            {
                if (_options.AutoCreateDirectories)
                {
                    Directory.CreateDirectory(directoryPath);
                    _logger.LogInformation("创建目录: {DirectoryPath}", directoryPath);
                }
                else
                {
                    _logger.LogWarning("目录不存在且自动创建已禁用: {DirectoryPath}", directoryPath);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建目录失败: {DirectoryPath}", directoryPath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<FileInfo?> GetFileInfoAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return new FileInfo(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件信息失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<List<FileInfo>> GetFilesAsync(string directoryPath, string pattern = "*.*")
    {
        var files = new List<FileInfo>();

        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                _logger.LogWarning("目录不存在: {DirectoryPath}", directoryPath);
                return files;
            }

            var fileInfos = Directory.GetFiles(directoryPath, pattern);
            foreach (var filePath in fileInfos)
            {
                files.Add(new FileInfo(filePath));
            }

            _logger.LogInformation("在目录 {DirectoryPath} 中找到 {Count} 个文件", directoryPath, files.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件列表失败: {DirectoryPath}", directoryPath);
        }

        return await Task.FromResult(files);
    }

    /// <inheritdoc/>
    public async Task<bool> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                _logger.LogError("源文件不存在: {SourcePath}", sourcePath);
                return false;
            }

            // 确保目标目录存在
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                await EnsureDirectoryExistsAsync(destinationDirectory);
            }

            File.Copy(sourcePath, destinationPath, overwrite);
            _logger.LogInformation("文件复制成功: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件复制失败: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("文件不存在: {FilePath}", filePath);
                return false;
            }

            File.Delete(filePath);
            _logger.LogInformation("文件删除成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupTempFilesAsync(string directoryPath, TimeSpan? olderThan = null)
    {
        var cleanedCount = 0;
        var cutoffTime = DateTime.Now - (olderThan ?? TimeSpan.FromHours(1));

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return 0;
            }

            var files = Directory.GetFiles(directoryPath);
            foreach (var filePath in files)
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.LastWriteTime < cutoffTime)
                {
                    try
                    {
                        File.Delete(filePath);
                        cleanedCount++;
                        _logger.LogDebug("清理临时文件: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "清理临时文件失败: {FilePath}", filePath);
                    }
                }
            }

            _logger.LogInformation("清理了 {Count} 个临时文件", cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理临时文件失败: {DirectoryPath}", directoryPath);
        }

        return await Task.FromResult(cleanedCount);
    }
}