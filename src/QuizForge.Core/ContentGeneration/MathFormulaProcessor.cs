using System.Text.RegularExpressions;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 数学公式处理器，用于处理LaTeX中的数学公式和特殊符号
/// </summary>
public class MathFormulaProcessor
{
    /// <summary>
    /// 处理文本中的数学公式和特殊符号
    /// </summary>
    /// <param name="text">包含数学公式的文本</param>
    /// <returns>处理后的LaTeX兼容文本</returns>
    public string ProcessMathFormulas(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var result = text;

        // 处理行内数学公式 $...$
        result = ProcessInlineMath(result);

        // 处理行间数学公式 \[...\]
        result = ProcessDisplayMath(result);

        // 处理数学环境
        result = ProcessMathEnvironments(result);

        // 处理常见数学符号
        result = ProcessMathSymbols(result);

        // 处理希腊字母
        result = ProcessGreekLetters(result);

        // 处理上标和下标
        result = ProcessSuperscriptsAndSubscripts(result);

        // 处理分数
        result = ProcessFractions(result);

        // 处理根号
        result = ProcessSquareRoots(result);

        // 处理积分和求和
        result = ProcessIntegralsAndSums(result);

        return result;
    }

    /// <summary>
    /// 处理行内数学公式
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessInlineMath(string text)
    {
        // 确保 $...$ 格式的数学公式正确
        var result = text;
        
        // 修复不匹配的数学公式标记
        var dollarCount = result.Count(c => c == '$');
        if (dollarCount % 2 != 0)
        {
            // 如果有奇数个$符号，移除最后一个
            var lastIndex = result.LastIndexOf('$');
            if (lastIndex >= 0)
            {
                result = result.Remove(lastIndex, 1);
            }
        }

        return result;
    }

    /// <summary>
    /// 处理行间数学公式
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessDisplayMath(string text)
    {
        var result = text;

        // 将 \[...\] 转换为 $$...$$ 格式（更常见的行间数学公式格式）
        result = Regex.Replace(result, @"\\\[(.*?)\\\]", "$$$1$$");

        return result;
    }

    /// <summary>
    /// 处理数学环境
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessMathEnvironments(string text)
    {
        var result = text;

        // 确保常见的数学环境正确闭合
        var mathEnvironments = new[] { "equation", "align", "gather", "multline", "flalign" };
        
        foreach (var env in mathEnvironments)
        {
            var beginPattern = $@"\\begin\{{{env}}}";
            var endPattern = $@"\\end\{{{env}}}";
            
            var beginCount = Regex.Matches(result, beginPattern).Count;
            var endCount = Regex.Matches(result, endPattern).Count;
            
            // 如果开始和结束标记数量不匹配，添加缺失的结束标记
            if (beginCount > endCount)
            {
                for (int i = 0; i < beginCount - endCount; i++)
                {
                    result += $"\n\\end{{{env}}}";
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 处理常见数学符号
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessMathSymbols(string text)
    {
        var result = text;

        // 处理常见数学符号的简写形式
        var symbolMappings = new Dictionary<string, string>
        {
            { @"(?<!\\)\<=", "\\leq" },
            { @"(?<!\\)\>=", "\\geq" },
            { @"(?<!\\)\!=", "\\neq" },
            { @"(?<!\\)~=", "\\approx" },
            { @"(?<!\\)\+-", "\\pm" },
            { @"(?<!\\)\x/", "\\times" },
            { @"(?<!\\)\div", "\\div" },
            { @"(?<!\\)\infty", "\\infty" },
            { @"(?<!\\)\sum", "\\sum" },
            { @"(?<!\\)\prod", "\\prod" },
            { @"(?<!\\)\int", "\\int" },
            { @"(?<!\\)\partial", "\\partial" },
            { @"(?<!\\)\nabla", "\\nabla" },
            { @"(?<!\\)\pm", "\\pm" },
            { @"(?<!\\)\mp", "\\mp" }
        };

        foreach (var mapping in symbolMappings)
        {
            result = Regex.Replace(result, mapping.Key, mapping.Value);
        }

        return result;
    }

    /// <summary>
    /// 处理希腊字母
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessGreekLetters(string text)
    {
        var result = text;

        // 处理希腊字母的简写形式
        var greekMappings = new Dictionary<string, string>
        {
            // 小写希腊字母
            { @"(?<!\\)alpha", "\\alpha" },
            { @"(?<!\\)beta", "\\beta" },
            { @"(?<!\\)gamma", "\\gamma" },
            { @"(?<!\\)delta", "\\delta" },
            { @"(?<!\\)epsilon", "\\epsilon" },
            { @"(?<!\\)zeta", "\\zeta" },
            { @"(?<!\\)eta", "\\eta" },
            { @"(?<!\\)theta", "\\theta" },
            { @"(?<!\\)iota", "\\iota" },
            { @"(?<!\\)kappa", "\\kappa" },
            { @"(?<!\\)lambda", "\\lambda" },
            { @"(?<!\\)mu", "\\mu" },
            { @"(?<!\\)nu", "\\nu" },
            { @"(?<!\\)xi", "\\xi" },
            { @"(?<!\\)omicron", "\\omicron" },
            { @"(?<!\\)pi", "\\pi" },
            { @"(?<!\\)rho", "\\rho" },
            { @"(?<!\\)sigma", "\\sigma" },
            { @"(?<!\\)tau", "\\tau" },
            { @"(?<!\\)upsilon", "\\upsilon" },
            { @"(?<!\\)phi", "\\phi" },
            { @"(?<!\\)chi", "\\chi" },
            { @"(?<!\\)psi", "\\psi" },
            { @"(?<!\\)omega", "\\omega" },
            
            // 大写希腊字母
            { @"(?<!\\)Alpha", "\\Alpha" },
            { @"(?<!\\)Beta", "\\Beta" },
            { @"(?<!\\)Gamma", "\\Gamma" },
            { @"(?<!\\)Delta", "\\Delta" },
            { @"(?<!\\)Epsilon", "\\Epsilon" },
            { @"(?<!\\)Zeta", "\\Zeta" },
            { @"(?<!\\)Eta", "\\Eta" },
            { @"(?<!\\)Theta", "\\Theta" },
            { @"(?<!\\)Iota", "\\Iota" },
            { @"(?<!\\)Kappa", "\\Kappa" },
            { @"(?<!\\)Lambda", "\\Lambda" },
            { @"(?<!\\)Mu", "\\Mu" },
            { @"(?<!\\)Nu", "\\Nu" },
            { @"(?<!\\)Xi", "\\Xi" },
            { @"(?<!\\)Omicron", "\\Omicron" },
            { @"(?<!\\)Pi", "\\Pi" },
            { @"(?<!\\)Rho", "\\Rho" },
            { @"(?<!\\)Sigma", "\\Sigma" },
            { @"(?<!\\)Tau", "\\Tau" },
            { @"(?<!\\)Upsilon", "\\Upsilon" },
            { @"(?<!\\)Phi", "\\Phi" },
            { @"(?<!\\)Chi", "\\Chi" },
            { @"(?<!\\)Psi", "\\Psi" },
            { @"(?<!\\)Omega", "\\Omega" }
        };

        foreach (var mapping in greekMappings)
        {
            result = Regex.Replace(result, mapping.Key, mapping.Value);
        }

        return result;
    }

    /// <summary>
    /// 处理上标和下标
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessSuperscriptsAndSubscripts(string text)
    {
        var result = text;

        // 处理上标：将 x^2 转换为 x^{2}
        result = Regex.Replace(result, @"(\w)\^(\w)", "$1^{$2}");
        
        // 处理下标：将 x_2 转换为 x_{2}
        result = Regex.Replace(result, @"(\w)_(\w)", "$1_{$2}");

        return result;
    }

    /// <summary>
    /// 处理分数
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessFractions(string text)
    {
        var result = text;

        // 处理分数：将 1/2 转换为 \frac{1}{2}
        result = Regex.Replace(result, @"(\d+)/(\d+)", "\\frac{$1}{$2}");

        return result;
    }

    /// <summary>
    /// 处理根号
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessSquareRoots(string text)
    {
        var result = text;

        // 处理平方根：将 sqrt(x) 转换为 \sqrt{x}
        result = Regex.Replace(result, @"sqrt\(([^)]+)\)", "\\sqrt{$1}");

        return result;
    }

    /// <summary>
    /// 处理积分和求和
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>处理后的文本</returns>
    private string ProcessIntegralsAndSums(string text)
    {
        var result = text;

        // 处理积分：将 int_{a}^{b} f(x) dx 转换为 \int_{a}^{b} f(x) dx
        result = Regex.Replace(result, @"int_\{([^}]+)\}^\{([^}]+)\}", "\\int_{$1}^{$2}");
        
        // 处理求和：将 sum_{i=1}^{n} 转换为 \sum_{i=1}^{n}
        result = Regex.Replace(result, @"sum_\{([^}]+)\}^\{([^}]+)\}", "\\sum_{$1}^{$2}");

        return result;
    }
}