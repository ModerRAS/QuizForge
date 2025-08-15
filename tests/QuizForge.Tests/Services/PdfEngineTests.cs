using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using QuizForge.Infrastructure.Engines;
using QuizForge.Infrastructure.Exceptions;
using QuizForge.Infrastructure.Services;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Infrastructure.Renderers;
using QuizForge.Models.Interfaces;
using Xunit;

namespace QuizForge.Tests.Services;

/// <summary>
/// PDF引擎测试类
/// </summary>
public class PdfEngineTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _testTempDirectory;
    private readonly string _testCacheDirectory;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfEngineTests()
    {
        // 创建测试临时目录
        _testTempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge.Tests");
        _testCacheDirectory = Path.Combine(_testTempDirectory, "Cache");
        
        if (!Directory.Exists(_testTempDirectory))
        {
            Directory.CreateDirectory(_testTempDirectory);
        }
        
        if (!Directory.Exists(_testCacheDirectory))
        {
            Directory.CreateDirectory(_testCacheDirectory);
        }

        // 创建配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json")
            .Build();

        // 创建服务集合
        var services = new ServiceCollection();
        
        // 添加日志
        services.AddLogging(builder => builder.AddConsole());
        
        // 添加配置
        services.AddSingleton<IConfiguration>(configuration);
        
        // 注册服务
        services.AddScoped<IPdfEngine, PdfEngine>();
        services.AddScoped<PdfErrorReportingService>();
        services.AddScoped<PdfCacheService>();
        services.AddScoped<LatexParser>();
        services.AddScoped<MathRenderer>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 测试PDF引擎生成基本PDF
    /// </summary>
    [Fact]
    public async Task GeneratePdfAsync_ShouldGeneratePdfFile()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var content = "这是一个测试文档。\nThis is a test document.";
        var outputPath = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 执行
            var result = await pdfEngine.GeneratePdfAsync(content, outputPath);

            // 断言
            Assert.True(result);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            // 清理
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    /// <summary>
    /// 测试PDF引擎从LaTeX生成PDF
    /// </summary>
    [Fact]
    public async Task GenerateFromLatexAsync_ShouldGeneratePdfFromLatex()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\title{测试文档}
\author{测试作者}
\date{\today}
\begin{document}
\maketitle
\section{引言}
这是一个测试文档，用于验证PDF引擎是否能够正确处理LaTeX内容。

\section{数学公式}
这是一个数学公式示例：$E = mc^2$

\section{列表}
\begin{itemize}
\item 第一项
\item 第二项
\item 第三项
\end{itemize}
\end{document}";
        var outputPath = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 执行
            var result = await pdfEngine.GenerateFromLatexAsync(latexContent, outputPath);

            // 断言
            Assert.True(result);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            // 清理
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    /// <summary>
    /// 测试PDF引擎生成PDF预览
    /// </summary>
    [Fact]
    public async Task GeneratePreviewAsync_ShouldGeneratePreview()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var content = "这是一个测试文档。\nThis is a test document.";
        var pdfPath = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 先生成PDF文件
            var generateResult = await pdfEngine.GeneratePdfAsync(content, pdfPath);
            Assert.True(generateResult);
            Assert.True(File.Exists(pdfPath));

            // 执行
            var previewData = await pdfEngine.GeneratePreviewAsync(pdfPath);

            // 断言
            Assert.NotNull(previewData);
            Assert.True(previewData.Length > 0);
        }
        finally
        {
            // 清理
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }
        }
    }

    /// <summary>
    /// 测试PDF引擎缓存功能
    /// </summary>
    [Fact]
    public async Task GeneratePdfAsync_ShouldUseCache()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var cacheService = _serviceProvider.GetRequiredService<PdfCacheService>();
        var content = "这是一个测试文档，用于验证缓存功能。\nThis is a test document for cache verification.";
        var outputPath1 = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");
        var outputPath2 = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 第一次生成
            var result1 = await pdfEngine.GeneratePdfAsync(content, outputPath1);
            Assert.True(result1);
            Assert.True(File.Exists(outputPath1));

            // 第二次生成（应该使用缓存）
            var result2 = await pdfEngine.GeneratePdfAsync(content, outputPath2);
            Assert.True(result2);
            Assert.True(File.Exists(outputPath2));

            // 验证两个文件内容相同
            var file1Bytes = await File.ReadAllBytesAsync(outputPath1);
            var file2Bytes = await File.ReadAllBytesAsync(outputPath2);
            Assert.Equal(file1Bytes, file2Bytes);
        }
        finally
        {
            // 清理
            if (File.Exists(outputPath1))
            {
                File.Delete(outputPath1);
            }
            if (File.Exists(outputPath2))
            {
                File.Delete(outputPath2);
            }
        }
    }

    /// <summary>
    /// 测试PDF引擎错误处理
    /// </summary>
    [Fact]
    public async Task GeneratePdfAsync_ShouldHandleErrors()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var invalidOutputPath = Path.Combine("Invalid", "Path", $"{Guid.NewGuid()}.pdf");

        // 执行和断言
        await Assert.ThrowsAsync<PdfGenerationException>(() => pdfEngine.GeneratePdfAsync("测试内容", invalidOutputPath));
    }

    /// <summary>
    /// 测试PDF引擎中文支持
    /// </summary>
    [Fact]
    public async Task GeneratePdfAsync_ShouldSupportChinese()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var content = "这是一个包含中文的测试文档。\nThis is a test document with Chinese content.\n中文与English混合的内容。";
        var outputPath = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 执行
            var result = await pdfEngine.GeneratePdfAsync(content, outputPath);

            // 断言
            Assert.True(result);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            // 清理
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    /// <summary>
    /// 测试PDF引擎数学公式支持
    /// </summary>
    [Fact]
    public async Task GenerateFromLatexAsync_ShouldSupportMathFormulas()
    {
        // 准备
        var pdfEngine = _serviceProvider.GetRequiredService<IPdfEngine>();
        var latexContent = @"
\documentclass{article}
\usepackage{ctex}
\usepackage{amsmath}
\title{数学公式测试}
\author{测试作者}
\date{\today}
\begin{document}
\maketitle
\section{数学公式}
行内公式：$E = mc^2$

行间公式：
\[
\int_{a}^{b} f(x) dx = F(b) - F(a)
\]

复杂公式：
\[
\frac{\partial^2 u}{\partial t^2} = c^2 \nabla^2 u
\]
\end{document}";
        var outputPath = Path.Combine(_testTempDirectory, $"{Guid.NewGuid()}.pdf");

        try
        {
            // 执行
            var result = await pdfEngine.GenerateFromLatexAsync(latexContent, outputPath);

            // 断言
            Assert.True(result);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            // 清理
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
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
            if (Directory.Exists(_testTempDirectory))
            {
                Directory.Delete(_testTempDirectory, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }
}