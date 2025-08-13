using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
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
using PdfSharpImage = SixLabors.ImageSharp.Image;

namespace QuizForge.Infrastructure.Engines;

/// <summary>
/// PDF 引擎实现
/// </summary>
public class PdfEngine : IPdfEngine
{
    private readonly ILogger<PdfEngine> _logger;
    private readonly string _tempDirectory;
    private readonly string _latexExecutablePath;

    /// <summary>
    /// PDF引擎构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public PdfEngine(ILogger<PdfEngine> logger)
    {
        _logger = logger;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "LaTeX");
        
        // 确保临时目录存在
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
        
        // 尝试查找LaTeX可执行文件路径
        _latexExecutablePath = FindLatexExecutable();
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
            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 创建PDF文档
            var document = new PdfDocument();
            var page = document.AddPage();
            
            // 使用PdfSharp添加内容
            var graphics = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharp.Drawing.XFont("SimSun", 12, PdfSharp.Drawing.XFontStyle.Regular);
            
            // 将内容写入PDF
            graphics.DrawString(content, font, PdfSharp.Drawing.XBrushes.Black,
                new PdfSharp.Drawing.XRect(10, 10, page.Width - 20, page.Height - 20),
                PdfSharp.Drawing.XStringFormats.TopLeft);
            
            // 保存PDF文档
            document.Save(outputPath);
            
            _logger.LogInformation("PDF生成成功: {OutputPath}", outputPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF生成失败: {OutputPath}", outputPath);
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
            if (string.IsNullOrEmpty(_latexExecutablePath))
            {
                _logger.LogError("未找到LaTeX可执行文件，无法生成PDF");
                return false;
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 创建临时LaTeX文件
            var tempLatexFile = Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.tex");
            await File.WriteAllTextAsync(tempLatexFile, latexContent);

            // 构建LaTeX编译命令
            var workingDir = Path.GetDirectoryName(tempLatexFile);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(tempLatexFile);
            var arguments = $"-interaction=nonstopmode -output-directory=\"{workingDir}\" \"{tempLatexFile}\"";

            // 执行LaTeX编译
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _latexExecutablePath,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("LaTeX编译失败: {Error}", error);
                return false;
            }

            // 检查生成的PDF文件
            var tempPdfFile = Path.Combine(workingDir, $"{fileNameWithoutExt}.pdf");
            if (!File.Exists(tempPdfFile))
            {
                _logger.LogError("LaTeX编译未生成PDF文件");
                return false;
            }

            // 将生成的PDF文件移动到目标位置
            File.Move(tempPdfFile, outputPath, true);

            // 清理临时文件
            CleanupTempFiles(workingDir, fileNameWithoutExt);

            _logger.LogInformation("LaTeX转PDF成功: {OutputPath}", outputPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LaTeX转PDF失败: {OutputPath}", outputPath);
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
            if (!File.Exists(pdfPath))
            {
                _logger.LogError("PDF文件不存在: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            // 使用PdfSharp读取PDF
            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
            
            if (document.PageCount == 0)
            {
                _logger.LogError("PDF文件没有页面: {PdfPath}", pdfPath);
                return Array.Empty<byte>();
            }

            // 获取第一页
            var page = document.Pages[0];
            
            // 使用ImageSharp创建预览图像
            using var image = new Image<Rgb24>(width, height);
            
            // 设置白色背景
            image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.White));
            
            // 在这里添加PDF页面渲染逻辑
            // 由于PdfSharp本身不提供直接渲染为图像的功能，
            // 这里我们创建一个占位图像，实际项目中可能需要使用其他库如PdfiumViewer或MuPDF
            
            // 添加预览文本
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 16);
            var textOptions = new RichTextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.PointF(10, 10),
                TabWidth = 4,
                WrappingLength = width - 20
            };
            
            image.Mutate(ctx =>
            {
                ctx.DrawText(
                    textOptions,
                    $"PDF预览: {Path.GetFileName(pdfPath)}\n页数: {document.PageCount}\n尺寸: {page.Width}x{page.Height}",
                    SixLabors.ImageSharp.Color.Black);
            });
            
            // 将图像转换为字节数组
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            
            _logger.LogInformation("PDF预览图像生成成功: {PdfPath}", pdfPath);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF预览图像生成失败: {PdfPath}", pdfPath);
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