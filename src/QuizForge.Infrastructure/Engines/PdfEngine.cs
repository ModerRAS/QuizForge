using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Infrastructure.Renderers;
using QuizForge.Infrastructure.Services;
using QuizForge.Infrastructure.Exceptions;
using PdfSharpImage = SixLabors.ImageSharp.Image;

namespace QuizForge.Infrastructure.Engines;

/// <summary>
/// PDF 引擎实现，支持原生PDF生成和LaTeX PDF生成
/// </summary>
public class PdfEngine : IPdfEngine
{
    private readonly ILogger<PdfEngine> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _tempDirectory;
    private readonly string _latexExecutablePath;
    private readonly IPdfEngine _nativePdfEngine;
    private readonly IPdfEngine _latexPdfEngine;
    private readonly PdfErrorReportingService _errorReportingService;
    private readonly bool _useNativeEngine;

    /// <summary>
    /// PDF引擎构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="configuration">配置</param>
    /// <param name="latexParser">LaTeX解析器</param>
    /// <param name="mathRenderer">数学公式渲染器</param>
    /// <param name="errorReportingService">错误报告服务</param>
    public PdfEngine(ILogger<PdfEngine> logger, IConfiguration configuration, LatexParser latexParser, MathRenderer mathRenderer, PdfErrorReportingService errorReportingService, PdfCacheService cacheService)
    {
        _logger = logger;
        _configuration = configuration;
        _errorReportingService = errorReportingService;
        
        // 从配置中获取是否使用原生引擎
        _useNativeEngine = configuration.GetValue<bool>("PdfEngine:UseNativeEngine") ?? true;
        
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "LaTeX");
        
        // 确保临时目录存在
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
        
        // 尝试查找LaTeX可执行文件路径
        _latexExecutablePath = FindLatexExecutable();
        
        // 创建PDF引擎
        _nativePdfEngine = new NativePdfEngine(logger, latexParser, mathRenderer, errorReportingService, cacheService, configuration);
        _latexPdfEngine = new LatexPdfEngine(logger);
        
        _logger.LogInformation("PDF引擎初始化完成，使用{EngineType}引擎", _useNativeEngine ? "原生" : "LaTeX");
    }

    /// <summary>
    /// 生成 PDF 文档
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    public async Task<bool> GeneratePdfAsync(string content, string outputPath)
    {
        try
        {
            _logger.LogInformation("开始生成PDF: {OutputPath}", outputPath);
            
            // 根据配置选择引擎
            if (_useNativeEngine)
            {
                return await _nativePdfEngine.GeneratePdfAsync(content, outputPath);
            }
            else
            {
                return await _latexPdfEngine.GeneratePdfAsync(content, outputPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF生成失败: {OutputPath}", outputPath);
            
            // 报告错误
            if (ex is PdfGenerationException pdfEx)
            {
                _errorReportingService.ReportError(pdfEx, new PdfErrorContext
                {
                    Operation = "GeneratePdfAsync",
                    FilePath = outputPath,
                    ContentLength = content?.Length ?? 0,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine"
                });
            }
            else
            {
                _errorReportingService.ReportWarning($"PDF生成失败: {ex.Message}", new PdfErrorContext
                {
                    Operation = "GeneratePdfAsync",
                    FilePath = outputPath,
                    ContentLength = content?.Length ?? 0,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine"
                });
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// 从 LaTeX 内容生成 PDF
    /// </summary>
    /// <param name="latexContent">LaTeX 内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    public async Task<bool> GenerateFromLatexAsync(string latexContent, string outputPath)
    {
        try
        {
            _logger.LogInformation("开始从LaTeX生成PDF: {OutputPath}", outputPath);
            
            // 根据配置选择引擎
            if (_useNativeEngine)
            {
                return await _nativePdfEngine.GenerateFromLatexAsync(latexContent, outputPath);
            }
            else
            {
                return await _latexPdfEngine.GenerateFromLatexAsync(latexContent, outputPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LaTeX转PDF失败: {OutputPath}", outputPath);
            
            // 报告错误
            if (ex is PdfGenerationException pdfEx)
            {
                _errorReportingService.ReportError(pdfEx, new PdfErrorContext
                {
                    Operation = "GenerateFromLatexAsync",
                    FilePath = outputPath,
                    ContentLength = latexContent?.Length ?? 0,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine"
                });
            }
            else
            {
                _errorReportingService.ReportWarning($"LaTeX转PDF失败: {ex.Message}", new PdfErrorContext
                {
                    Operation = "GenerateFromLatexAsync",
                    FilePath = outputPath,
                    ContentLength = latexContent?.Length ?? 0,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine"
                });
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// 生成 PDF 预览图像
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GeneratePreviewAsync(string pdfPath, int width = 800, int height = 600)
    {
        try
        {
            _logger.LogInformation("开始生成PDF预览: {PdfPath}", pdfPath);
            
            // 根据配置选择引擎
            if (_useNativeEngine)
            {
                return await _nativePdfEngine.GeneratePreviewAsync(pdfPath, width, height);
            }
            else
            {
                return await _latexPdfEngine.GeneratePreviewAsync(pdfPath, width, height);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF预览生成失败: {PdfPath}", pdfPath);
            
            // 报告错误
            if (ex is PdfGenerationException pdfEx)
            {
                _errorReportingService.ReportError(pdfEx, new PdfErrorContext
                {
                    Operation = "GeneratePreviewAsync",
                    FilePath = pdfPath,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine",
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "Width", width },
                        { "Height", height }
                    }
                });
            }
            else
            {
                _errorReportingService.ReportWarning($"PDF预览生成失败: {ex.Message}", new PdfErrorContext
                {
                    Operation = "GeneratePreviewAsync",
                    FilePath = pdfPath,
                    EngineType = _useNativeEngine ? "NativePdfEngine" : "LatexPdfEngine",
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "Width", width },
                        { "Height", height }
                    }
                });
            }
            
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 查找LaTeX可执行文件
    /// </summary>
    /// <returns>LaTeX可执行文件路径</returns>
    private string FindLatexExecutable()
    {
        // 常见的LaTeX发行版路径
        var possiblePaths = new[]
        {
            // MiKTeX
            @"C:\Program Files\MiKTeX\miktex\bin\x64\pdflatex.exe",
            @"C:\Program Files (x86)\MiKTeX\miktex\bin\pdflatex.exe",
            // TeX Live
            @"C:\texlive\2023\bin\win32\pdflatex.exe",
            @"C:\texlive\2022\bin\win32\pdflatex.exe",
            // 其他可能的路径
            @"C:\Program Files\TeX Live\bin\win32\pdflatex.exe",
            @"C:\Program Files (x86)\TeX Live\bin\win32\pdflatex.exe"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // 尝试从PATH环境变量查找
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "pdflatex",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                var firstLine = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (File.Exists(firstLine))
                {
                    return firstLine;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从PATH环境变量查找pdflatex失败");
        }

        _logger.LogWarning("未找到pdflatex可执行文件");
        return string.Empty;
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="workingDir">工作目录</param>
    /// <param name="fileNameWithoutExt">不带扩展名的文件名</param>
    private void CleanupTempFiles(string workingDir, string fileNameWithoutExt)
    {
        try
        {
            var extensions = new[] { ".tex", ".aux", ".log", ".out", ".toc", ".lof", ".lot" };
            
            foreach (var ext in extensions)
            {
                var file = Path.Combine(workingDir, $"{fileNameWithoutExt}{ext}");
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理临时文件失败");
        }
    }
}