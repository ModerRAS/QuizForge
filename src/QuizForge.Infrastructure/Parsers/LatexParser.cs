using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using QuizForge.Models.Interfaces;

namespace QuizForge.Infrastructure.Parsers;

/// <summary>
/// LaTeX解析器，用于将基本的LaTeX语法转换为可渲染的内容
/// </summary>
public class LatexParser
{
    private readonly ILogger<LatexParser> _logger;
    
    /// <summary>
    /// LaTeX解析器构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public LatexParser(ILogger<LatexParser> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 解析LaTeX内容并转换为结构化内容
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <returns>解析后的结构化内容</returns>
    public LatexDocument Parse(string latexContent)
    {
        try
        {
            var document = new LatexDocument();
            
            // 提取文档类和包
            ExtractDocumentClass(latexContent, document);
            ExtractPackages(latexContent, document);
            
            // 提取文档内容（去除document环境）
            var content = ExtractDocumentContent(latexContent);
            
            // 解析各个部分
            ParseSections(content, document);
            ParseMath(content, document);
            ParseTables(content, document);
            ParseLists(content, document);
            ParseTextFormatting(content, document);
            
            _logger.LogInformation("LaTeX内容解析成功");
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LaTeX内容解析失败");
            throw;
        }
    }
    
    /// <summary>
    /// 提取文档类
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="document">文档对象</param>
    private void ExtractDocumentClass(string latexContent, LatexDocument document)
    {
        var match = Regex.Match(latexContent, @"\\documentclass\[(.*?)\]\{(.*?)\}");
        if (match.Success)
        {
            document.DocumentClass = match.Groups[2].Value;
            document.Options = match.Groups[1].Value.Split(',').Select(o => o.Trim()).ToList();
        }
        else
        {
            match = Regex.Match(latexContent, @"\\documentclass\{(.*?)\}");
            if (match.Success)
            {
                document.DocumentClass = match.Groups[1].Value;
            }
        }
    }
    
    /// <summary>
    /// 提取使用的包
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <param name="document">文档对象</param>
    private void ExtractPackages(string latexContent, LatexDocument document)
    {
        var matches = Regex.Matches(latexContent, @"\\usepackage\[(.*?)\]\{(.*?)\}");
        foreach (Match match in matches)
        {
            document.Packages.Add(new LatexPackage
            {
                Name = match.Groups[2].Value,
                Options = match.Groups[1].Value.Split(',').Select(o => o.Trim()).ToList()
            });
        }
        
        matches = Regex.Matches(latexContent, @"\\usepackage\{(.*?)\}");
        foreach (Match match in matches)
        {
            document.Packages.Add(new LatexPackage
            {
                Name = match.Groups[1].Value,
                Options = new List<string>()
            });
        }
    }
    
    /// <summary>
    /// 提取文档内容
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <returns>文档内容</returns>
    private string ExtractDocumentContent(string latexContent)
    {
        var match = Regex.Match(latexContent, @"\\begin\{document\}(.*?)\\end\{document\}", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : latexContent;
    }
    
    /// <summary>
    /// 解析章节
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="document">文档对象</param>
    private void ParseSections(string content, LatexDocument document)
    {
        // 解析section
        var sectionMatches = Regex.Matches(content, @"\\section\{(.*?)\}");
        foreach (Match match in sectionMatches)
        {
            document.Sections.Add(new LatexSection
            {
                Level = 1,
                Title = match.Groups[1].Value,
                Type = "section"
            });
        }
        
        // 解析subsection
        var subsectionMatches = Regex.Matches(content, @"\\subsection\{(.*?)\}");
        foreach (Match match in subsectionMatches)
        {
            document.Sections.Add(new LatexSection
            {
                Level = 2,
                Title = match.Groups[1].Value,
                Type = "subsection"
            });
        }
        
        // 解析subsubsection
        var subsubsectionMatches = Regex.Matches(content, @"\\subsubsection\{(.*?)\}");
        foreach (Match match in subsubsectionMatches)
        {
            document.Sections.Add(new LatexSection
            {
                Level = 3,
                Title = match.Groups[1].Value,
                Type = "subsubsection"
            });
        }
    }
    
    /// <summary>
    /// 解析数学公式
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="document">文档对象</param>
    private void ParseMath(string content, LatexDocument document)
    {
        // 解析行内数学公式 $...$
        var inlineMathMatches = Regex.Matches(content, @"\$(.*?)\$");
        foreach (Match match in inlineMathMatches)
        {
            document.MathElements.Add(new LatexMathElement
            {
                Type = "inline",
                Content = match.Groups[1].Value
            });
        }
        
        // 解析行间数学公式 \[...\]
        var displayMathMatches = Regex.Matches(content, @"\\\[(.*?)\\\]");
        foreach (Match match in displayMathMatches)
        {
            document.MathElements.Add(new LatexMathElement
            {
                Type = "display",
                Content = match.Groups[1].Value
            });
        }
        
        // 解析数学环境 equation, align, etc.
        var mathEnvMatches = Regex.Matches(content, @"\\begin\{(equation|align|gather|multline)\}(.*?)\\end\{\1\}", RegexOptions.Singleline);
        foreach (Match match in mathEnvMatches)
        {
            document.MathElements.Add(new LatexMathElement
            {
                Type = match.Groups[1].Value,
                Content = match.Groups[2].Value
            });
        }
    }
    
    /// <summary>
    /// 解析表格
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="document">文档对象</param>
    private void ParseTables(string content, LatexDocument document)
    {
        var tableMatches = Regex.Matches(content, @"\\begin\{tabular\}(.*?)\\end\{tabular\}", RegexOptions.Singleline);
        foreach (Match match in tableMatches)
        {
            var table = new LatexTable
            {
                Content = match.Groups[1].Value
            };
            
            // 解析表格列定义
            var columnMatch = Regex.Match(match.Groups[1].Value, @"\{(.*?)\}");
            if (columnMatch.Success)
            {
                table.ColumnDefinitions = columnMatch.Groups[1].Value.ToCharArray().Select(c => c.ToString()).ToList();
            }
            
            // 解析表格行
            var rows = Regex.Split(match.Groups[1].Value, @"\\\\").Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            foreach (var row in rows)
            {
                var cells = Regex.Split(row, @"\&").Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
                table.Rows.Add(cells);
            }
            
            document.Tables.Add(table);
        }
    }
    
    /// <summary>
    /// 解析列表
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="document">文档对象</param>
    private void ParseLists(string content, LatexDocument document)
    {
        // 解析itemize列表
        var itemizeMatches = Regex.Matches(content, @"\\begin\{itemize\}(.*?)\\end\{itemize\}", RegexOptions.Singleline);
        foreach (Match match in itemizeMatches)
        {
            var list = new LatexList
            {
                Type = "itemize"
            };
            
            var items = Regex.Matches(match.Groups[1].Value, @"\\item\s+(.*?)(?=\\item|$)", RegexOptions.Singleline);
            foreach (Match itemMatch in items)
            {
                list.Items.Add(itemMatch.Groups[1].Value.Trim());
            }
            
            document.Lists.Add(list);
        }
        
        // 解析enumerate列表
        var enumerateMatches = Regex.Matches(content, @"\\begin\{enumerate\}(.*?)\\end\{enumerate\}", RegexOptions.Singleline);
        foreach (Match match in enumerateMatches)
        {
            var list = new LatexList
            {
                Type = "enumerate"
            };
            
            var items = Regex.Matches(match.Groups[1].Value, @"\\item\s+(.*?)(?=\\item|$)", RegexOptions.Singleline);
            foreach (Match itemMatch in items)
            {
                list.Items.Add(itemMatch.Groups[1].Value.Trim());
            }
            
            document.Lists.Add(list);
        }
    }
    
    /// <summary>
    /// 解析文本格式
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="document">文档对象</param>
    private void ParseTextFormatting(string content, LatexDocument document)
    {
        // 解析粗体 \textbf{...}
        var boldMatches = Regex.Matches(content, @"\\textbf\{(.*?)\}");
        foreach (Match match in boldMatches)
        {
            document.TextElements.Add(new LatexTextElement
            {
                Type = "bold",
                Content = match.Groups[1].Value
            });
        }
        
        // 解析斜体 \textit{...}
        var italicMatches = Regex.Matches(content, @"\\textit\{(.*?)\}");
        foreach (Match match in italicMatches)
        {
            document.TextElements.Add(new LatexTextElement
            {
                Type = "italic",
                Content = match.Groups[1].Value
            });
        }
        
        // 解析下划线 \underline{...}
        var underlineMatches = Regex.Matches(content, @"\\underline\{(.*?)\}");
        foreach (Match match in underlineMatches)
        {
            document.TextElements.Add(new LatexTextElement
            {
                Type = "underline",
                Content = match.Groups[1].Value
            });
        }
    }
}

/// <summary>
/// LaTeX文档结构
/// </summary>
public class LatexDocument
{
    /// <summary>
    /// 文档类
    /// </summary>
    public string? DocumentClass { get; set; }
    
    /// <summary>
    /// 文档选项
    /// </summary>
    public List<string> Options { get; set; } = new();
    
    /// <summary>
    /// 使用的包
    /// </summary>
    public List<LatexPackage> Packages { get; set; } = new();
    
    /// <summary>
    /// 章节
    /// </summary>
    public List<LatexSection> Sections { get; set; } = new();
    
    /// <summary>
    /// 数学元素
    /// </summary>
    public List<LatexMathElement> MathElements { get; set; } = new();
    
    /// <summary>
    /// 表格
    /// </summary>
    public List<LatexTable> Tables { get; set; } = new();
    
    /// <summary>
    /// 列表
    /// </summary>
    public List<LatexList> Lists { get; set; } = new();
    
    /// <summary>
    /// 文本元素
    /// </summary>
    public List<LatexTextElement> TextElements { get; set; } = new();
}

/// <summary>
/// LaTeX包
/// </summary>
public class LatexPackage
{
    /// <summary>
    /// 包名
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 包选项
    /// </summary>
    public List<string> Options { get; set; } = new();
}

/// <summary>
/// LaTeX章节
/// </summary>
public class LatexSection
{
    /// <summary>
    /// 章节级别
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// 章节标题
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// 章节类型
    /// </summary>
    public string? Type { get; set; }
}

/// <summary>
/// LaTeX数学元素
/// </summary>
public class LatexMathElement
{
    /// <summary>
    /// 数学元素类型
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// 数学内容
    /// </summary>
    public string? Content { get; set; }
}

/// <summary>
/// LaTeX表格
/// </summary>
public class LatexTable
{
    /// <summary>
    /// 表格内容
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// 列定义
    /// </summary>
    public List<string> ColumnDefinitions { get; set; } = new();
    
    /// <summary>
    /// 表格行
    /// </summary>
    public List<List<string>> Rows { get; set; } = new();
}

/// <summary>
/// LaTeX列表
/// </summary>
public class LatexList
{
    /// <summary>
    /// 列表类型
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// 列表项
    /// </summary>
    public List<string> Items { get; set; } = new();
}

/// <summary>
/// LaTeX文本元素
/// </summary>
public class LatexTextElement
{
    /// <summary>
    /// 文本类型
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? Content { get; set; }
}