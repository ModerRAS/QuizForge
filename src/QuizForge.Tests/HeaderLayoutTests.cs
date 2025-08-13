using QuizForge.Core.Layout;
using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests;

/// <summary>
/// HeaderLayout类的单元测试
/// </summary>
public class HeaderLayoutTests
{
    private readonly HeaderLayout _headerLayout;

    public HeaderLayoutTests()
    {
        _headerLayout = new HeaderLayout();
    }

    [Fact]
    public void GenerateHeader_WithStandardStyle_ShouldGenerateCorrectLaTeX()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "期末考试",
            Subject = "数学",
            ExamTime = 120,
            TotalPoints = 100,
            ShowSealLine = true
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\textbf{期末考试}", result);
        Assert.Contains("考试科目：数学", result);
        Assert.Contains("考试时间：120分钟", result);
        Assert.Contains("总分：100分", result);
        Assert.Contains("\\begin{center}", result);
        Assert.Contains("\\end{center}", result);
    }

    [Fact]
    public void GenerateHeader_WithSimpleStyle_ShouldGenerateSimpleLaTeX()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Simple,
            ExamTitle = "单元测试",
            Subject = "物理",
            ExamTime = 60,
            TotalPoints = 50
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\textbf{单元测试}", result);
        Assert.Contains("物理", result);
        Assert.Contains("60分钟", result);
        Assert.Contains("50分", result);
    }

    [Fact]
    public void GenerateHeader_WithDetailedStyle_ShouldIncludeStudentInfo()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Detailed,
            ExamTitle = "期中考试",
            Subject = "化学",
            ExamTime = 90,
            TotalPoints = 80,
            ExamLocation = "教学楼A101",
            SchoolName = "测试大学"
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\textbf{期中考试}", result);
        Assert.Contains("考试地点：教学楼A101", result);
        Assert.Contains("测试大学", result);
        Assert.Contains("姓名", result);
        Assert.Contains("学号", result);
    }

    [Fact]
    public void GenerateHeader_WithCustomStyle_ShouldUseCustomTemplate()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Custom,
            CustomTemplate = @"
\begin{center}
\textbf{{EXAM_TITLE}}
\end{center}
科目：{SUBJECT}",
            ExamTitle = "自定义考试",
            Subject = "生物"
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\textbf{自定义考试}", result);
        Assert.Contains("科目：生物", result);
    }

    [Fact]
    public void GenerateHeader_WithStudentInfo_ShouldGenerateStudentInfoSection()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "测试考试",
            Subject = "英语",
            ExamTime = 90,
            TotalPoints = 100,
            StudentInfo = new StudentInfoConfig
            {
                ShowName = true,
                ShowStudentId = true,
                ShowClass = true,
                ShowDate = true,
                Layout = StudentInfoLayout.Horizontal
            }
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("姓名", result);
        Assert.Contains("学号", result);
        Assert.Contains("班级", result);
        Assert.Contains("日期", result);
    }

    [Fact]
    public void GenerateHeader_WithAlignmentLeft_ShouldAlignLeft()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "左对齐测试",
            Subject = "历史",
            Alignment = HeaderAlignment.Left
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\begin{flushleft}", result);
        Assert.Contains("\\end{flushleft}", result);
        Assert.DoesNotContain("\\begin{center}", result);
    }

    [Fact]
    public void GenerateHeader_WithAlignmentRight_ShouldAlignRight()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "右对齐测试",
            Subject = "地理",
            Alignment = HeaderAlignment.Right
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\begin{flushright}", result);
        Assert.Contains("\\end{flushright}", result);
        Assert.DoesNotContain("\\begin{center}", result);
    }

    [Fact]
    public void GenerateHeader_WithLargeFontSize_ShouldUseLargeFont()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "大字体测试",
            Subject = "政治",
            TitleFontSize = HeaderFontSize.Large
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 1);

        // Assert
        Assert.Contains("\\Large", result);
        Assert.DoesNotContain("\\huge", result);
        Assert.DoesNotContain("\\normalsize", result);
    }

    [Fact]
    public void GenerateHeader_WithPageNumberGreaterThan1_ShouldReturnEmpty()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "分页测试",
            Subject = "体育",
            ShowOnFirstPageOnly = true
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 2);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateHeader_WithShowOnFirstPageOnlyFalse_ShouldShowOnAllPages()
    {
        // Arrange
        var config = new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "全页显示测试",
            Subject = "音乐",
            ShowOnFirstPageOnly = false
        };

        // Act
        var result = _headerLayout.GenerateHeader(config, 2);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("\\textbf{全页显示测试}", result);
    }
}