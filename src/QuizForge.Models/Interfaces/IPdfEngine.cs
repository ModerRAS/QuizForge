namespace QuizForge.Models.Interfaces;

/// <summary>
/// PDF引擎接口
/// </summary>
public interface IPdfEngine
{
    /// <summary>
    /// 生成PDF文档
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    Task<bool> GeneratePdfAsync(string content, string outputPath);
    
    /// <summary>
    /// 从LaTeX内容生成PDF
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>生成结果</returns>
    Task<bool> GenerateFromLatexAsync(string latexContent, string outputPath);
    
    /// <summary>
    /// 生成PDF预览图像
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <returns>预览图像数据</returns>
    Task<byte[]> GeneratePreviewAsync(string pdfPath, int width = 800, int height = 600);
}