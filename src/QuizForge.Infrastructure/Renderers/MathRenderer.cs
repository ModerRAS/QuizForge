using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace QuizForge.Infrastructure.Renderers;

/// <summary>
/// 数学公式渲染器，用于将LaTeX数学公式转换为图像
/// </summary>
public class MathRenderer
{
    private readonly ILogger<MathRenderer> _logger;
    private readonly SKFont _defaultFont;
    private readonly SKFont _mathFont;
    
    /// <summary>
    /// 数学公式渲染器构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public MathRenderer(ILogger<MathRenderer> logger)
    {
        _logger = logger;
        
        // 加载默认字体
        try
        {
            // 尝试加载系统字体
            var defaultTypeface = SKTypeface.FromFamilyName("Arial");
            if (defaultTypeface == null)
            {
                defaultTypeface = SKTypeface.FromFamilyName("SimSun");
            }
            _defaultFont = new SKFont(defaultTypeface, 16);
            
            // 尝试加载数学字体
            var mathTypeface = SKTypeface.FromFamilyName("Cambria Math");
            if (mathTypeface == null)
            {
                mathTypeface = SKTypeface.FromFamilyName("Times New Roman");
            }
            _mathFont = new SKFont(mathTypeface, 16);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载字体失败");
            throw;
        }
    }
    
    /// <summary>
    /// 渲染行内数学公式
    /// </summary>
    /// <param name="mathContent">数学公式内容</param>
    /// <returns>渲染后的图像数据</returns>
    public byte[] RenderInlineMath(string mathContent)
    {
        try
        {
            // 简化处理：将LaTeX数学公式转换为普通文本
            var simplifiedText = SimplifyLatexMath(mathContent);
            
            // 测量文本大小
            var textPaint = new SKPaint
            {
                Typeface = _mathFont.Typeface,
                TextSize = _mathFont.Size,
                Color = SKColors.Black,
                IsAntialias = true
            };
            
            var textBounds = new SKRect();
            textPaint.MeasureText(simplifiedText, ref textBounds);
            
            // 创建位图
            var width = (int)Math.Ceiling(textBounds.Width) + 10;
            var height = (int)Math.Ceiling(textBounds.Height) + 10;
            
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            
            // 清除背景
            canvas.Clear(SKColors.White);
            
            // 绘制文本
            canvas.DrawText(simplifiedText, 5, height - 5, textPaint);
            
            // 转换为字节数组
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "渲染行内数学公式失败: {MathContent}", mathContent);
            throw;
        }
    }
    
    /// <summary>
    /// 渲染行间数学公式
    /// </summary>
    /// <param name="mathContent">数学公式内容</param>
    /// <returns>渲染后的图像数据</returns>
    public byte[] RenderDisplayMath(string mathContent)
    {
        try
        {
            // 简化处理：将LaTeX数学公式转换为普通文本
            var simplifiedText = SimplifyLatexMath(mathContent);
            
            // 测量文本大小
            var textPaint = new SKPaint
            {
                Typeface = _mathFont.Typeface,
                TextSize = _mathFont.Size * 1.2f, // 行间公式稍大
                Color = SKColors.Black,
                IsAntialias = true
            };
            
            var textBounds = new SKRect();
            textPaint.MeasureText(simplifiedText, ref textBounds);
            
            // 创建位图
            var width = (int)Math.Ceiling(textBounds.Width) + 20;
            var height = (int)Math.Ceiling(textBounds.Height) + 20;
            
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            
            // 清除背景
            canvas.Clear(SKColors.White);
            
            // 绘制文本（居中）
            var x = (width - textBounds.Width) / 2;
            var y = (height + textBounds.Height) / 2;
            canvas.DrawText(simplifiedText, x, y, textPaint);
            
            // 转换为字节数组
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "渲染行间数学公式失败: {MathContent}", mathContent);
            throw;
        }
    }
    
    /// <summary>
    /// 简化LaTeX数学公式
    /// </summary>
    /// <param name="latexMath">LaTeX数学公式</param>
    /// <returns>简化后的文本</returns>
    private string SimplifyLatexMath(string latexMath)
    {
        // 这是一个简化的实现，实际项目中可能需要使用专门的数学公式渲染库
        // 如MathJax、KaTeX等
        
        var result = latexMath;
        
        // 处理一些常见的LaTeX数学符号
        result = Regex.Replace(result, @"\\alpha", "α");
        result = Regex.Replace(result, @"\\beta", "β");
        result = Regex.Replace(result, @"\\gamma", "γ");
        result = Regex.Replace(result, @"\\delta", "δ");
        result = Regex.Replace(result, @"\\epsilon", "ε");
        result = Regex.Replace(result, @"\\zeta", "ζ");
        result = Regex.Replace(result, @"\\eta", "η");
        result = Regex.Replace(result, @"\\theta", "θ");
        result = Regex.Replace(result, @"\\iota", "ι");
        result = Regex.Replace(result, @"\\kappa", "κ");
        result = Regex.Replace(result, @"\\lambda", "λ");
        result = Regex.Replace(result, @"\\mu", "μ");
        result = Regex.Replace(result, @"\\nu", "ν");
        result = Regex.Replace(result, @"\\xi", "ξ");
        result = Regex.Replace(result, @"\\omicron", "ο");
        result = Regex.Replace(result, @"\\pi", "π");
        result = Regex.Replace(result, @"\\rho", "ρ");
        result = Regex.Replace(result, @"\\sigma", "σ");
        result = Regex.Replace(result, @"\\tau", "τ");
        result = Regex.Replace(result, @"\\upsilon", "υ");
        result = Regex.Replace(result, @"\\phi", "φ");
        result = Regex.Replace(result, @"\\chi", "χ");
        result = Regex.Replace(result, @"\\psi", "ψ");
        result = Regex.Replace(result, @"\\omega", "ω");
        
        result = Regex.Replace(result, @"\\Alpha", "Α");
        result = Regex.Replace(result, @"\\Beta", "Β");
        result = Regex.Replace(result, @"\\Gamma", "Γ");
        result = Regex.Replace(result, @"\\Delta", "Δ");
        result = Regex.Replace(result, @"\\Epsilon", "Ε");
        result = Regex.Replace(result, @"\\Zeta", "Ζ");
        result = Regex.Replace(result, @"\\Eta", "Η");
        result = Regex.Replace(result, @"\\Theta", "Θ");
        result = Regex.Replace(result, @"\\Iota", "Ι");
        result = Regex.Replace(result, @"\\Kappa", "Κ");
        result = Regex.Replace(result, @"\\Lambda", "Λ");
        result = Regex.Replace(result, @"\\Mu", "Μ");
        result = Regex.Replace(result, @"\\Nu", "Ν");
        result = Regex.Replace(result, @"\\Xi", "Ξ");
        result = Regex.Replace(result, @"\\Omicron", "Ο");
        result = Regex.Replace(result, @"\\Pi", "Π");
        result = Regex.Replace(result, @"\\Rho", "Ρ");
        result = Regex.Replace(result, @"\\Sigma", "Σ");
        result = Regex.Replace(result, @"\\Tau", "Τ");
        result = Regex.Replace(result, @"\\Upsilon", "Υ");
        result = Regex.Replace(result, @"\\Phi", "Φ");
        result = Regex.Replace(result, @"\\Chi", "Χ");
        result = Regex.Replace(result, @"\\Psi", "Ψ");
        result = Regex.Replace(result, @"\\Omega", "Ω");
        
        // 处理数学符号
        result = Regex.Replace(result, @"\\sum", "∑");
        result = Regex.Replace(result, @"\\prod", "∏");
        result = Regex.Replace(result, @"\\int", "∫");
        result = Regex.Replace(result, @"\\infty", "∞");
        result = Regex.Replace(result, @"\\pm", "±");
        result = Regex.Replace(result, @"\\times", "×");
        result = Regex.Replace(result, @"\\div", "÷");
        result = Regex.Replace(result, @"\\leq", "≤");
        result = Regex.Replace(result, @"\\geq", "≥");
        result = Regex.Replace(result, @"\\neq", "≠");
        result = Regex.Replace(result, @"\\approx", "≈");
        result = Regex.Replace(result, @"\\sqrt\{(.*?)\}", "√($1)");
        
        // 处理上标和下标
        result = Regex.Replace(result, @"\^\{(.*?)\}", "^($1)");
        result = Regex.Replace(result, @"_\{(.*?)\}", "_($1)");
        
        // 处理分数
        result = Regex.Replace(result, @"\\frac\{(.*?)\}\{(.*?)\}", "($1)/($2)");
        
        // 移除其他LaTeX命令
        result = Regex.Replace(result, @"\\[a-zA-Z]+\s*", "");
        
        return result;
    }
}