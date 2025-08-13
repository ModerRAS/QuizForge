using Microsoft.Extensions.Logging;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Services;

/// <summary>
/// 导出服务实现
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;
    private readonly IPdfEngine _pdfEngine;
    private readonly IPrintPreviewService _printPreviewService;
    private readonly IFileService _fileService;
    private readonly string _tempDirectory;

    /// <summary>
    /// 导出服务构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="pdfEngine">PDF引擎</param>
    /// <param name="printPreviewService">打印预览服务</param>
    /// <param name="fileService">文件服务</param>
    public ExportService(
        ILogger<ExportService> logger,
        IPdfEngine pdfEngine,
        IPrintPreviewService printPreviewService,
        IFileService fileService)
    {
        _logger = logger;
        _pdfEngine = pdfEngine;
        _printPreviewService = printPreviewService;
        _fileService = fileService;
        
        // 设置临时目录
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge", "Exports");
        
        // 确保临时目录存在
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    /// <summary>
    /// 导出为PDF
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>PDF文件路径</returns>
    public async Task<string> ExportToPdfAsync(string latexContent, ExportConfiguration configuration)
    {
        try
        {
            if (string.IsNullOrEmpty(latexContent))
            {
                throw new ArgumentException("LaTeX内容不能为空");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 确保输出目录存在
            if (!Directory.Exists(configuration.OutputPath))
            {
                Directory.CreateDirectory(configuration.OutputPath);
            }

            // 生成文件名
            var fileName = !string.IsNullOrEmpty(configuration.FileName) 
                ? configuration.FileName 
                : $"Export_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".pdf";
            }

            var outputPath = Path.Combine(configuration.OutputPath, fileName);

            // 添加水印和答案（如果需要）
            var processedContent = await ProcessLatexContentAsync(latexContent, configuration);

            // 使用PDF引擎生成PDF
            var success = await _pdfEngine.GenerateFromLatexAsync(processedContent, outputPath);

            if (!success)
            {
                throw new Exception("PDF生成失败");
            }

            // 如果需要多份，复制文件
            if (configuration.Copies > 1)
            {
                await CreateCopiesAsync(outputPath, configuration.Copies);
            }

            _logger.LogInformation("PDF导出成功: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF导出失败");
            throw;
        }
    }

    /// <summary>
    /// 导出为LaTeX
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>LaTeX文件路径</returns>
    public async Task<string> ExportToLaTeXAsync(string latexContent, ExportConfiguration configuration)
    {
        try
        {
            if (string.IsNullOrEmpty(latexContent))
            {
                throw new ArgumentException("LaTeX内容不能为空");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 确保输出目录存在
            if (!Directory.Exists(configuration.OutputPath))
            {
                Directory.CreateDirectory(configuration.OutputPath);
            }

            // 生成文件名
            var fileName = !string.IsNullOrEmpty(configuration.FileName) 
                ? configuration.FileName 
                : $"Export_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            if (!fileName.EndsWith(".tex", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".tex";
            }

            var outputPath = Path.Combine(configuration.OutputPath, fileName);

            // 添加水印和答案（如果需要）
            var processedContent = await ProcessLatexContentAsync(latexContent, configuration);

            // 保存LaTeX文件
            await File.WriteAllTextAsync(outputPath, processedContent);

            _logger.LogInformation("LaTeX导出成功: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LaTeX导出失败");
            throw;
        }
    }

    /// <summary>
    /// 导出为Word
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>Word文件路径</returns>
    public async Task<string> ExportToWordAsync(string latexContent, ExportConfiguration configuration)
    {
        try
        {
            if (string.IsNullOrEmpty(latexContent))
            {
                throw new ArgumentException("LaTeX内容不能为空");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 确保输出目录存在
            if (!Directory.Exists(configuration.OutputPath))
            {
                Directory.CreateDirectory(configuration.OutputPath);
            }

            // 生成文件名
            var fileName = !string.IsNullOrEmpty(configuration.FileName) 
                ? configuration.FileName 
                : $"Export_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".docx";
            }

            var outputPath = Path.Combine(configuration.OutputPath, fileName);

            // 添加水印和答案（如果需要）
            var processedContent = await ProcessLatexContentAsync(latexContent, configuration);

            // 先生成PDF，然后转换为Word
            var tempPdfPath = Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.pdf");
            var success = await _pdfEngine.GenerateFromLatexAsync(processedContent, tempPdfPath);

            if (!success)
            {
                throw new Exception("PDF生成失败");
            }

            // 使用文件服务转换为Word
            await _fileService.ConvertPdfToWordAsync(tempPdfPath, outputPath);

            // 清理临时文件
            if (File.Exists(tempPdfPath))
            {
                File.Delete(tempPdfPath);
            }

            // 如果需要多份，复制文件
            if (configuration.Copies > 1)
            {
                await CreateCopiesAsync(outputPath, configuration.Copies);
            }

            _logger.LogInformation("Word导出成功: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Word导出失败");
            throw;
        }
    }

    /// <summary>
    /// 生成预览图像
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    public async Task<byte[]> GeneratePreviewImageAsync(string latexContent, int width = 800, int height = 600)
    {
        try
        {
            if (string.IsNullOrEmpty(latexContent))
            {
                throw new ArgumentException("LaTeX内容不能为空");
            }

            // 创建临时PDF文件
            var tempPdfPath = Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.pdf");
            
            // 生成PDF
            var success = await _pdfEngine.GenerateFromLatexAsync(latexContent, tempPdfPath);

            if (!success)
            {
                throw new Exception("PDF生成失败");
            }

            // 生成预览图像
            var previewImage = await _printPreviewService.GeneratePreviewImageAsync(tempPdfPath, 1, width, height);

            // 清理临时文件
            if (File.Exists(tempPdfPath))
            {
                File.Delete(tempPdfPath);
            }

            return previewImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成预览图像失败");
            throw;
        }
    }

    /// <summary>
    /// 打印文档
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="settings">打印设置</param>
    /// <returns>打印结果</returns>
    public async Task<bool> PrintDocumentAsync(string filePath, PrintSettings settings)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("文件不存在", filePath);
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            // 使用文件服务打印文档
            var success = await _fileService.PrintDocumentAsync(filePath, settings);

            if (success)
            {
                _logger.LogInformation("文档打印成功: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("文档打印失败: {FilePath}", filePath);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文档打印失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 处理LaTeX内容（添加水印和答案）
    /// </summary>
    /// <param name="latexContent">原始LaTeX内容</param>
    /// <param name="configuration">导出配置</param>
    /// <returns>处理后的LaTeX内容</returns>
    private async Task<string> ProcessLatexContentAsync(string latexContent, ExportConfiguration configuration)
    {
        var processedContent = latexContent;

        // 添加水印
        if (configuration.IncludeWatermark && !string.IsNullOrEmpty(configuration.WatermarkText))
        {
            processedContent = await AddWatermarkAsync(processedContent, configuration.WatermarkText);
        }

        // 添加答案
        if (configuration.IncludeAnswerKey)
        {
            processedContent = await AddAnswerKeyAsync(processedContent);
        }

        return processedContent;
    }

    /// <summary>
    /// 添加水印
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="watermarkText">水印文本</param>
    /// <returns>添加水印后的LaTeX内容</returns>
    private Task<string> AddWatermarkAsync(string latexContent, string watermarkText)
    {
        try
        {
            // 在LaTeX内容中添加水印
            var watermarkPackage = @"
% 水印包
\usepackage{draftwatermark}
\SetWatermarkText{" + watermarkText + @"}
\SetWatermarkScale{0.5}
\SetWatermarkColor[gray]{0.9}
";

            // 在documentclass后插入水印包
            var documentClassIndex = latexContent.IndexOf("\\documentclass", StringComparison.OrdinalIgnoreCase);
            if (documentClassIndex >= 0)
            {
                var endOfLineIndex = latexContent.IndexOf('\n', documentClassIndex);
                if (endOfLineIndex > 0)
                {
                    var insertPosition = endOfLineIndex + 1;
                    latexContent = latexContent.Insert(insertPosition, watermarkPackage);
                }
            }

            return Task.FromResult(latexContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加水印失败");
            return Task.FromResult(latexContent);
        }
    }

    /// <summary>
    /// 添加答案
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <returns>添加答案后的LaTeX内容</returns>
    private Task<string> AddAnswerKeyAsync(string latexContent)
    {
        try
        {
            // 在LaTeX内容末尾添加答案部分
            var answerSection = @"

\newpage
\section*{参考答案}

% 这里应该包含答案内容
% 实际实现中需要根据LaTeX内容解析出答案并格式化

\noindent
\textbf{注意：} 此为参考答案，实际答案内容需要根据题目解析。

";

            // 在\end{document}之前插入答案部分
            var endDocumentIndex = latexContent.IndexOf("\\end{document}", StringComparison.OrdinalIgnoreCase);
            if (endDocumentIndex > 0)
            {
                latexContent = latexContent.Insert(endDocumentIndex, answerSection);
            }

            return Task.FromResult(latexContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加答案失败");
            return Task.FromResult(latexContent);
        }
    }

    /// <summary>
    /// 创建文件副本
    /// </summary>
    /// <param name="originalPath">原始文件路径</param>
    /// <param name="copies">副本数量</param>
    private async Task CreateCopiesAsync(string originalPath, int copies)
    {
        try
        {
            var directory = Path.GetDirectoryName(originalPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);

            for (int i = 1; i < copies; i++)
            {
                var copyFileName = $"{fileNameWithoutExt}_copy{i}{extension}";
                var copyPath = Path.Combine(directory!, copyFileName);
                await Task.Run(() => File.Copy(originalPath, copyPath, true));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建文件副本失败");
            throw;
        }
    }
}