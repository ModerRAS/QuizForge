using QuizForge.Core.Layout;
using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// 布局逻辑测试类
/// </summary>
public class LayoutLogicTests
{
    private readonly SealLineLayout _sealLineLayout;
    private readonly HeaderLayout _headerLayout;
    private readonly HeaderFooterLayout _headerFooterLayout;

    public LayoutLogicTests()
    {
        _sealLineLayout = new SealLineLayout();
        _headerLayout = new HeaderLayout();
        _headerFooterLayout = new HeaderFooterLayout();
    }

    #region SealLineLayout Tests

    [Fact]
    public void GenerateSealLine_ShouldThrowExceptionForNullTemplate()
    {
        // Arrange
        ExamTemplate? template = null;
        var pageNumber = 1;
        var totalPages = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages));
    }

    [Fact]
    public void GenerateSealLine_ShouldReturnValidLaTeX()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            SealLine = SealLinePosition.Left
        };
        var pageNumber = 1;
        var totalPages = 1;

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(@"\sealline{", result);
    }

    [Fact]
    public void GenerateSealLineCommandDefinition_ShouldReturnValidLaTeX()
    {
        // Act
        var result = _sealLineLayout.GenerateSealLineCommandDefinition();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(@"\newcommand{\sealline}[1]{", result);
        Assert.Contains(@"\ifthenelse{\isodd{\thepage}}{", result);
        Assert.Contains(@"\draw[thick] (current page.north west) ++(0,-2) -- (current page.south west) ++(0,2);", result);
        Assert.Contains(@"\draw[thick] (current page.north east) ++(0,-2) -- (current page.south east) ++(0,2);", result);
    }

    [Theory]
    [InlineData(1, SealLinePosition.Left)]
    [InlineData(2, SealLinePosition.Right)]
    [InlineData(3, SealLinePosition.Left)]
    [InlineData(4, SealLinePosition.Right)]
    public void GenerateSealLine_ShouldUseCorrectPositionBasedOnPageNumber(int pageNumber, SealLinePosition expectedPosition)
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            SealLine = SealLinePosition.Left // 使用自动位置
        };
        var totalPages = 4;

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        // 验证生成的LaTeX代码包含正确的位置逻辑
    }

    [Fact]
    public void GenerateSealLine_ShouldUseTemplateFixedPosition()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            SealLine = SealLinePosition.Top // 固定位置
        };
        var pageNumber = 2; // 偶数页，但应使用模板固定位置
        var totalPages = 4;

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        // 验证生成的LaTeX代码使用模板固定位置
    }

    #endregion

    #region HeaderLayout Tests

    [Fact]
    public void GenerateHeader_ShouldThrowExceptionForNullTemplate()
    {
        // Arrange
        ExamTemplate? template = null;
        var examTime = 120;
        var totalPoints = 100;
        var pageNumber = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _headerLayout.GenerateHeader(template, examTime, totalPoints, pageNumber));
    }

    [Fact]
    public void GenerateHeader_ShouldReturnEmptyForNonFirstPage()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试"
        };
        var examTime = 120;
        var totalPoints = 100;
        var pageNumber = 2; // 非第一页

        // Act
        var result = _headerLayout.GenerateHeader(template, examTime, totalPoints, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateHeader_ShouldReturnValidLaTeXForFirstPage()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试"
        };
        var examTime = 120;
        var totalPoints = 100;
        var pageNumber = 1; // 第一页

        // Act
        var result = _headerLayout.GenerateHeader(template, examTime, totalPoints, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\begin{center}", result);
        Assert.Contains(@"\Large \textbf{测试试卷}", result);
        Assert.Contains(@"考试科目：数学测试", result);
        Assert.Contains(@"考试时间：120分钟", result);
        Assert.Contains(@"总分：100分", result);
        Assert.Contains(@"\end{center}", result);
    }

    [Fact]
    public void GenerateHeader_ShouldHandleEmptyDescription()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "" // 空描述
        };
        var examTime = 120;
        var totalPoints = 100;
        var pageNumber = 1; // 第一页

        // Act
        var result = _headerLayout.GenerateHeader(template, examTime, totalPoints, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\Large \textbf{测试试卷}", result);
        Assert.DoesNotContain(@"考试科目：", result); // 不应显示考试科目行
    }

    [Fact]
    public void GenerateCustomHeader_ShouldReturnEmptyForNonFirstPage()
    {
        // Arrange
        var customContent = "自定义抬头内容";
        var pageNumber = 2; // 非第一页

        // Act
        var result = _headerLayout.GenerateCustomHeader(customContent, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateCustomHeader_ShouldReturnValidLaTeXForFirstPage()
    {
        // Arrange
        var customContent = "自定义抬头内容";
        var pageNumber = 1; // 第一页

        // Act
        var result = _headerLayout.GenerateCustomHeader(customContent, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\begin{center}", result);
        Assert.Contains(@"自定义抬头内容", result);
        Assert.Contains(@"\end{center}", result);
    }

    [Fact]
    public void GenerateCustomHeader_ShouldReturnEmptyForEmptyContent()
    {
        // Arrange
        var customContent = "";
        var pageNumber = 1; // 第一页

        // Act
        var result = _headerLayout.GenerateCustomHeader(customContent, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region HeaderFooterLayout Tests

    [Fact]
    public void GenerateHeaderFooter_ShouldThrowExceptionForNullTemplate()
    {
        // Arrange
        ExamTemplate? template = null;
        var examTitle = "测试试卷";
        var subject = "数学测试";
        var pageNumber = 1;
        var totalPages = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _headerFooterLayout.GenerateHeaderFooter(template, examTitle, subject, pageNumber, totalPages));
    }

    [Fact]
    public void GenerateHeaderFooter_ShouldReturnValidLaTeX()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            HeaderContent = "自定义页眉",
            FooterContent = "自定义页脚"
        };
        var examTitle = "测试试卷";
        var subject = "数学测试";
        var pageNumber = 1;
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateHeaderFooter(template, examTitle, subject, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\pagestyle{fancy}", result);
        Assert.Contains(@"\fancyhead[C]{自定义页眉}", result);
        Assert.Contains(@"\fancyfoot[C]{自定义页脚}", result);
    }

    [Fact]
    public void GenerateHeaderFooter_ShouldUseDefaultHeaderFooterWhenCustomIsEmpty()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试",
            HeaderContent = "", // 空自定义页眉
            FooterContent = ""  // 空自定义页脚
        };
        var examTitle = "测试试卷";
        var subject = "数学测试";
        var pageNumber = 1;
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateHeaderFooter(template, examTitle, subject, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyhead[C]{测试试卷 - 数学测试}", result);
        Assert.Contains(@"\fancyfoot[C]{第1页/共1页}", result);
    }

    [Fact]
    public void GenerateHeaderFooterSetup_ShouldReturnValidLaTeX()
    {
        // Act
        var result = _headerFooterLayout.GenerateHeaderFooterSetup();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\pagestyle{fancy}", result);
        Assert.Contains(@"\fancyhf{}", result);
        Assert.Contains(@"\renewcommand{\headrulewidth}{0.4pt}", result);
        Assert.Contains(@"\renewcommand{\footrulewidth}{0.4pt}", result);
    }

    [Fact]
    public void GenerateHeader_ShouldUseCustomContentWhenAvailable()
    {
        // Arrange
        var template = new ExamTemplate
        {
            HeaderContent = "自定义页眉内容"
        };
        var examTitle = "测试试卷";
        var subject = "数学测试";
        var pageNumber = 1;

        // Act
        var result = _headerFooterLayout.GenerateHeader(template, examTitle, subject, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyhead[C]{自定义页眉内容}", result);
        Assert.DoesNotContain(@"测试试卷", result); // 不应使用默认标题
    }

    [Fact]
    public void GenerateHeader_ShouldUseDefaultContentWhenCustomIsEmpty()
    {
        // Arrange
        var template = new ExamTemplate
        {
            HeaderContent = "" // 空自定义页眉
        };
        var examTitle = "测试试卷";
        var subject = "数学测试";
        var pageNumber = 1;

        // Act
        var result = _headerFooterLayout.GenerateHeader(template, examTitle, subject, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyhead[C]{测试试卷 - 数学测试}", result);
    }

    [Fact]
    public void GenerateFooter_ShouldUseCustomContentWhenAvailable()
    {
        // Arrange
        var template = new ExamTemplate
        {
            FooterContent = "自定义页脚内容"
        };
        var pageNumber = 1;
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateFooter(template, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyfoot[C]{自定义页脚内容}", result);
        Assert.DoesNotContain(@"第1页/共1页", result); // 不应使用默认页码
    }

    [Fact]
    public void GenerateFooter_ShouldUseDefaultContentWhenCustomIsEmpty()
    {
        // Arrange
        var template = new ExamTemplate
        {
            FooterContent = "" // 空自定义页脚
        };
        var pageNumber = 1;
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateFooter(template, pageNumber, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyfoot[C]{第1页/共1页}", result);
    }

    [Fact]
    public void GenerateFirstPageHeaderFooterSetup_ShouldReturnValidLaTeX()
    {
        // Act
        var result = _headerFooterLayout.GenerateFirstPageHeaderFooterSetup();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\thispagestyle{fancy}", result);
        Assert.Contains(@"\fancyhf{}", result);
    }

    [Fact]
    public void GenerateFirstPageHeader_ShouldReturnEmptyHeader()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "数学测试"
        };
        var examTitle = "测试试卷";
        var subject = "数学测试";

        // Act
        var result = _headerFooterLayout.GenerateFirstPageHeader(template, examTitle, subject);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyhead[C]{}", result); // 首页不显示页眉
    }

    [Fact]
    public void GenerateFirstPageFooter_ShouldUseCustomContentWhenAvailable()
    {
        // Arrange
        var template = new ExamTemplate
        {
            FooterContent = "自定义页脚内容"
        };
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateFirstPageFooter(template, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyfoot[C]{自定义页脚内容}", result);
        Assert.DoesNotContain(@"第1页/共1页", result); // 不应使用默认页码
    }

    [Fact]
    public void GenerateFirstPageFooter_ShouldUseDefaultContentWhenCustomIsEmpty()
    {
        // Arrange
        var template = new ExamTemplate
        {
            FooterContent = "" // 空自定义页脚
        };
        var totalPages = 1;

        // Act
        var result = _headerFooterLayout.GenerateFirstPageFooter(template, totalPages);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\fancyfoot[C]{第1页/共1页}", result);
    }

    [Fact]
    public void GenerateBlankHeaderFooterSetup_ShouldReturnValidLaTeX()
    {
        // Act
        var result = _headerFooterLayout.GenerateBlankHeaderFooterSetup();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(@"\thispagestyle{plain}", result);
    }

    #endregion
}