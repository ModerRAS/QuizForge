using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.Infrastructure.Services;
using Xunit;

namespace QuizForge.Tests.Services;

/// <summary>
/// PDF缓存服务测试类
/// </summary>
public class PdfCacheServiceTests : IDisposable
{
    private readonly PdfCacheService _cacheService;
    private readonly string _testCacheDirectory;
    private readonly Mock<ILogger<PdfCacheService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfCacheServiceTests()
    {
        // 创建测试缓存目录
        _testCacheDirectory = Path.Combine(Path.GetTempPath(), "QuizForge.Tests.Cache");
        
        if (!Directory.Exists(_testCacheDirectory))
        {
            Directory.CreateDirectory(_testCacheDirectory);
        }

        // 创建模拟对象
        _loggerMock = new Mock<ILogger<PdfCacheService>>();
        _configurationMock = new Mock<IConfiguration>();

        // 设置配置模拟
        _configurationMock.Setup(c => c.GetValue<string>("PdfEngine:CacheDirectory", It.IsAny<string>()))
            .Returns(_testCacheDirectory);
        _configurationMock.Setup(c => c.GetValue<int>("PdfEngine:MaxMemoryCacheSize", It.IsAny<int>()))
            .Returns(10);
        _configurationMock.Setup(c => c.GetValue<int>("PdfEngine:MaxDiskCacheSize", It.IsAny<int>()))
            .Returns(20);
        _configurationMock.Setup(c => c.GetValue<int>("PdfEngine:CacheExpirationDays", It.IsAny<int>()))
            .Returns(1);

        // 创建缓存服务
        _cacheService = new PdfCacheService(_loggerMock.Object, _configurationMock.Object);
    }

    /// <summary>
    /// 测试缓存PDF文件
    /// </summary>
    [Fact]
    public async Task CachePdfAsync_ShouldCachePdfFile()
    {
        // 准备
        var content = "这是一个测试文档。\nThis is a test document.";
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        try
        {
            // 执行
            var cachedPath = await _cacheService.CachePdfAsync(content, pdfData);

            // 断言
            Assert.NotNull(cachedPath);
            Assert.True(File.Exists(cachedPath));
            Assert.Equal(pdfData.Length, new FileInfo(cachedPath).Length);
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 测试获取缓存的PDF文件
    /// </summary>
    [Fact]
    public async Task GetCachedPdf_ShouldReturnCachedFilePath()
    {
        // 准备
        var content = "这是一个测试文档。\nThis is a test document.";
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        try
        {
            // 先缓存PDF文件
            var cachedPath = await _cacheService.CachePdfAsync(content, pdfData);
            Assert.NotNull(cachedPath);

            // 执行
            var retrievedPath = _cacheService.GetCachedPdf(content);

            // 断言
            Assert.NotNull(retrievedPath);
            Assert.Equal(cachedPath, retrievedPath);
            Assert.True(File.Exists(retrievedPath));
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 测试获取不存在的缓存PDF文件
    /// </summary>
    [Fact]
    public void GetCachedPdf_ShouldReturnNullForNonExistentContent()
    {
        // 准备
        var content = "这是一个不存在的测试文档。";

        // 执行
        var cachedPath = _cacheService.GetCachedPdf(content);

        // 断言
        Assert.Null(cachedPath);
    }

    /// <summary>
    /// 测试清除所有缓存
    /// </summary>
    [Fact]
    public async Task ClearCache_ShouldRemoveAllCachedFiles()
    {
        // 准备
        var content1 = "测试文档1。\nTest document 1.";
        var content2 = "测试文档2。\nTest document 2.";
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        try
        {
            // 缓存多个PDF文件
            await _cacheService.CachePdfAsync(content1, pdfData);
            await _cacheService.CachePdfAsync(content2, pdfData);

            // 验证文件已缓存
            Assert.NotNull(_cacheService.GetCachedPdf(content1));
            Assert.NotNull(_cacheService.GetCachedPdf(content2));

            // 执行
            _cacheService.ClearCache();

            // 断言
            Assert.Null(_cacheService.GetCachedPdf(content1));
            Assert.Null(_cacheService.GetCachedPdf(content2));
            Assert.Empty(Directory.GetFiles(_testCacheDirectory, "*.pdf"));
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 测试获取缓存统计信息
    /// </summary>
    [Fact]
    public async Task GetCacheStatistics_ShouldReturnCorrectStatistics()
    {
        // 准备
        var content1 = "测试文档1。\nTest document 1.";
        var content2 = "测试文档2。\nTest document 2.";
        var pdfData1 = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 5字节
        var pdfData2 = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 }; // 6字节

        try
        {
            // 缓存多个PDF文件
            await _cacheService.CachePdfAsync(content1, pdfData1);
            await _cacheService.CachePdfAsync(content2, pdfData2);

            // 执行
            var statistics = _cacheService.GetCacheStatistics();

            // 断言
            Assert.Equal(2, statistics.MemoryCacheCount);
            Assert.Equal(11, statistics.TotalCacheSize); // 5 + 6
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 测试清理过期缓存
    /// </summary>
    [Fact]
    public async Task CleanupExpiredCache_ShouldRemoveExpiredCacheFiles()
    {
        // 准备
        var content = "这是一个测试文档。\nThis is a test document.";
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        try
        {
            // 缓存PDF文件
            var cachedPath = await _cacheService.CachePdfAsync(content, pdfData);
            Assert.NotNull(cachedPath);

            // 验证文件已缓存
            Assert.NotNull(_cacheService.GetCachedPdf(content));

            // 执行
            _cacheService.CleanupExpiredCache();

            // 断言
            // 由于缓存过期时间设置为1天，所以缓存文件应该仍然存在
            Assert.NotNull(_cacheService.GetCachedPdf(content));
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 测试缓存空内容
    /// </summary>
    [Fact]
    public async Task CachePdfAsync_ShouldThrowExceptionForNullOrEmptyContent()
    {
        // 准备
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        // 执行和断言
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.CachePdfAsync(null, pdfData));
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.CachePdfAsync("", pdfData));
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.CachePdfAsync("test", null));
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.CachePdfAsync("test", new byte[0]));
    }

    /// <summary>
    /// 测试内存缓存限制
    /// </summary>
    [Fact]
    public async Task CachePdfAsync_ShouldRespectMemoryCacheLimit()
    {
        // 准备
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // 模拟PDF文件头

        try
        {
            // 缓存超过最大内存缓存数量的文件
            for (int i = 0; i < 15; i++) // 最大内存缓存数量为10
            {
                var content = $"测试文档{i}。\nTest document {i}.";
                await _cacheService.CachePdfAsync(content, pdfData);
            }

            // 验证最早的缓存已被移除
            Assert.Null(_cacheService.GetCachedPdf("测试文档0。\nTest document 0."));
            Assert.NotNull(_cacheService.GetCachedPdf("测试文档14。\nTest document 14."));
        }
        finally
        {
            // 清理
            _cacheService.ClearCache();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        try
        {
            // 清理测试目录
            if (Directory.Exists(_testCacheDirectory))
            {
                Directory.Delete(_testCacheDirectory, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }
}