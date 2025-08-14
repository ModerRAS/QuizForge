using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuizForge.Infrastructure.Services;

/// <summary>
/// PDF缓存服务，用于提高PDF生成性能
/// </summary>
public class PdfCacheService
{
    private readonly ILogger<PdfCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _cacheDirectory;
    private readonly Dictionary<string, CacheEntry> _memoryCache = new();
    private readonly TimeSpan _defaultCacheDuration;
    private readonly int _maxMemoryCacheSize; // 最大内存缓存条目数
    private readonly int _maxDiskCacheSize; // 最大磁盘缓存条目数
    private readonly object _lock = new();

    /// <summary>
    /// 缓存条目
    /// </summary>
    private class CacheEntry
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }
        public long FileSize { get; set; }
    }

    /// <summary>
    /// PDF缓存服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="configuration">配置</param>
    public PdfCacheService(ILogger<PdfCacheService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // 从配置中获取缓存设置
        _cacheDirectory = configuration.GetSection("PdfEngine:CacheDirectory").Value ??
                          Path.Combine(Path.GetTempPath(), "QuizForge", "Cache");
        _maxMemoryCacheSize = int.Parse(configuration.GetSection("PdfEngine:MaxMemoryCacheSize").Value ?? "100");
        _maxDiskCacheSize = int.Parse(configuration.GetSection("PdfEngine:MaxDiskCacheSize").Value ?? "1000");
        var cacheExpirationDays = int.Parse(configuration.GetSection("PdfEngine:CacheExpirationDays").Value ?? "7");
        _defaultCacheDuration = TimeSpan.FromDays(cacheExpirationDays);
        
        // 确保缓存目录存在
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
        
        // 初始化缓存
        InitializeCache();
        
        _logger.LogInformation("PDF缓存服务初始化完成，缓存目录: {CacheDirectory}，内存缓存大小: {MemorySize}，磁盘缓存大小: {DiskSize}，过期时间: {ExpirationDays}天",
            _cacheDirectory, _maxMemoryCacheSize, _maxDiskCacheSize, cacheExpirationDays);
    }

    /// <summary>
    /// 初始化缓存
    /// </summary>
    private void InitializeCache()
    {
        try
        {
            // 清理过期的缓存文件
            CleanExpiredCacheFiles();
            
            // 加载现有的缓存文件到内存缓存
            var cacheFiles = Directory.GetFiles(_cacheDirectory, "*.pdf");
            foreach (var file in cacheFiles)
            {
                var fileInfo = new FileInfo(file);
                var key = Path.GetFileNameWithoutExtension(file);
                
                _memoryCache[key] = new CacheEntry
                {
                    FilePath = file,
                    CreatedAt = fileInfo.CreationTime,
                    LastAccessed = fileInfo.LastAccessTime,
                    AccessCount = 0,
                    FileSize = fileInfo.Length
                };
            }
            
            _logger.LogInformation("加载了 {Count} 个缓存文件", cacheFiles.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化缓存失败");
        }
    }

    /// <summary>
    /// 清理过期的缓存文件
    /// </summary>
    private void CleanExpiredCacheFiles()
    {
        try
        {
            var now = DateTime.Now;
            var cacheFiles = Directory.GetFiles(_cacheDirectory, "*.pdf");
            
            // 如果缓存文件数量超过最大磁盘缓存大小，删除最旧的文件
            if (cacheFiles.Length > _maxDiskCacheSize)
            {
                var filesToDelete = cacheFiles
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.LastAccessTime)
                    .Take(cacheFiles.Length - _maxDiskCacheSize);
                
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file.FullName);
                        _logger.LogDebug("删除过多缓存文件: {File}", file.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除缓存文件失败: {File}", file.FullName);
                    }
                }
            }
            
            // 删除过期的缓存文件
            foreach (var file in cacheFiles)
            {
                var fileInfo = new FileInfo(file);
                if (now - fileInfo.LastAccessTime > _defaultCacheDuration)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogDebug("删除过期缓存文件: {File}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除缓存文件失败: {File}", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期缓存文件失败");
        }
    }

    /// <summary>
    /// 生成内容的哈希键
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>哈希键</returns>
    private string GenerateHashKey(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// 获取缓存的PDF文件路径
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>缓存的PDF文件路径，如果不存在则返回null</returns>
    public string? GetCachedPdf(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        var key = GenerateHashKey(content);
        
        lock (_lock)
        {
            if (_memoryCache.TryGetValue(key, out var entry))
            {
                // 更新访问信息
                entry.LastAccessed = DateTime.Now;
                entry.AccessCount++;
                
                // 检查文件是否存在
                if (File.Exists(entry.FilePath))
                {
                    _logger.LogDebug("从缓存中获取PDF: {Key}", key);
                    return entry.FilePath;
                }
                else
                {
                    // 文件不存在，从内存缓存中移除
                    _memoryCache.Remove(key);
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// 缓存PDF文件
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="pdfData">PDF数据</param>
    /// <returns>缓存的PDF文件路径</returns>
    public async Task<string> CachePdfAsync(string content, byte[] pdfData)
    {
        if (string.IsNullOrEmpty(content) || pdfData == null || pdfData.Length == 0)
        {
            throw new ArgumentException("内容和PDF数据不能为空");
        }

        var key = GenerateHashKey(content);
        var filePath = Path.Combine(_cacheDirectory, $"{key}.pdf");
        
        lock (_lock)
        {
            // 检查是否已存在
            if (_memoryCache.TryGetValue(key, out var existingEntry))
            {
                if (File.Exists(existingEntry.FilePath))
                {
                    // 更新访问信息
                    existingEntry.LastAccessed = DateTime.Now;
                    existingEntry.AccessCount++;
                    return existingEntry.FilePath;
                }
                else
                {
                    // 文件不存在，从内存缓存中移除
                    _memoryCache.Remove(key);
                }
            }
            
            // 检查内存缓存大小
            if (_memoryCache.Count >= _maxMemoryCacheSize)
            {
                // 移除最少使用的缓存条目
                var lruKey = _memoryCache
                    .OrderBy(kv => kv.Value.LastAccessed)
                    .ThenBy(kv => kv.Value.AccessCount)
                    .First().Key;
                
                _memoryCache.Remove(lruKey);
            }
            
            // 添加到内存缓存
            _memoryCache[key] = new CacheEntry
            {
                FilePath = filePath,
                CreatedAt = DateTime.Now,
                LastAccessed = DateTime.Now,
                AccessCount = 1,
                FileSize = pdfData.Length
            };
        }
        
        // 写入文件
        try
        {
            await File.WriteAllBytesAsync(filePath, pdfData);
            _logger.LogDebug("缓存PDF文件: {Key}, 大小: {Size} bytes", key, pdfData.Length);
            return filePath;
        }
        catch (Exception ex)
        {
            // 写入失败，从内存缓存中移除
            lock (_lock)
            {
                _memoryCache.Remove(key);
            }
            
            _logger.LogError(ex, "缓存PDF文件失败: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearCache()
    {
        try
        {
            lock (_lock)
            {
                // 清除内存缓存
                _memoryCache.Clear();
                
                // 清除文件缓存
                var cacheFiles = Directory.GetFiles(_cacheDirectory, "*.pdf");
                foreach (var file in cacheFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除缓存文件失败: {File}", file);
                    }
                }
            }
            
            _logger.LogInformation("已清除所有缓存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除缓存失败");
            throw;
        }
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    /// <returns>缓存统计信息</returns>
    public (int MemoryCacheCount, long TotalCacheSize) GetCacheStatistics()
    {
        lock (_lock)
        {
            var totalSize = _memoryCache.Values.Sum(entry => entry.FileSize);
            return (_memoryCache.Count, totalSize);
        }
    }

    /// <summary>
    /// 清理过期的缓存
    /// </summary>
    public void CleanupExpiredCache()
    {
        CleanExpiredCacheFiles();
        
        lock (_lock)
        {
            var now = DateTime.Now;
            var expiredKeys = _memoryCache
                .Where(kv => now - kv.Value.LastAccessed > _defaultCacheDuration)
                .Select(kv => kv.Key)
                .ToList();
            
            foreach (var key in expiredKeys)
            {
                var entry = _memoryCache[key];
                
                try
                {
                    if (File.Exists(entry.FilePath))
                    {
                        File.Delete(entry.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除过期缓存文件失败: {File}", entry.FilePath);
                }
                
                _memoryCache.Remove(key);
            }
        }
        
        _logger.LogInformation("清理过期缓存完成");
    }
}