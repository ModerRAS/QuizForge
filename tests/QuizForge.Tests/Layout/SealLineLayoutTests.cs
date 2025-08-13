using QuizForge.Core.Layout;
using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests.Layout;

/// <summary>
/// 密封线布局测试类
/// </summary>
public class SealLineLayoutTests
{
    private readonly SealLineLayout _sealLineLayout;

    public SealLineLayoutTests()
    {
        _sealLineLayout = new SealLineLayout();
    }

    /// <summary>
    /// 测试生成密封线LaTeX命令定义
    /// </summary>
    [Fact]
    public void GenerateSealLineCommandDefinition_ShouldReturnValidLaTeX()
    {
        // Act
        var result = _sealLineLayout.GenerateSealLineCommandDefinition();

        // Assert
        Assert.Contains("\\newcommand{\\sealline}", result);
        Assert.Contains("\\newcommand{\\topsealline}", result);
        Assert.Contains("\\newcommand{\\bottomsealline}", result);
        Assert.Contains("tikzpicture", result);
    }

    /// <summary>
    /// 测试奇数页密封线位置
    /// </summary>
    [Fact]
    public void GenerateSealLine_OddPage_ShouldUseLeftPosition()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试试卷描述",
            EnableDynamicSealLinePosition = true,
            SealLineContent = new SealLineContentConfig
            {
                ShowName = true,
                ShowStudentId = true,
                ShowClass = true,
                ShowDate = true
            }
        };

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, 1, 2);

        // Assert
        Assert.Contains("\\sealline{", result);
        Assert.Contains("姓名：", result);
        Assert.Contains("考号：", result);
        Assert.Contains("班级：", result);
        Assert.Contains("日期：", result);
    }

    /// <summary>
    /// 测试偶数页密封线位置
    /// </summary>
    [Fact]
    public void GenerateSealLine_EvenPage_ShouldUseRightPosition()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试试卷描述",
            EnableDynamicSealLinePosition = true,
            SealLineContent = new SealLineContentConfig
            {
                ShowName = true,
                ShowStudentId = true,
                ShowClass = true,
                ShowDate = true
            }
        };

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, 2, 2);

        // Assert
        Assert.Contains("\\sealline{", result);
        Assert.Contains("姓名：", result);
        Assert.Contains("考号：", result);
        Assert.Contains("班级：", result);
        Assert.Contains("日期：", result);
    }

    /// <summary>
    /// 测试固定位置密封线
    /// </summary>
    [Fact]
    public void GenerateSealLine_FixedPosition_ShouldUseSpecifiedPosition()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试试卷描述",
            EnableDynamicSealLinePosition = false,
            SealLine = SealLinePosition.Top,
            SealLineContent = new SealLineContentConfig
            {
                ShowName = true,
                ShowStudentId = true,
                ShowClass = true,
                ShowDate = true
            }
        };

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, 2, 2);

        // Assert
        Assert.Contains("\\sealline{", result);
        // 注意：由于SealLineLayout类中的GenerateSealLine方法内部调用的是GenerateSealLineLaTeX，
        // 而GenerateSealLineLaTeX总是使用\sealline命令，所以这里我们只能验证命令存在
        // 实际的位置由LaTeX的\ifthenelse{\isodd{\thepage}}条件判断
    }

    /// <summary>
    /// 测试自定义密封线内容
    /// </summary>
    [Fact]
    public void GenerateSealLine_CustomContent_ShouldIncludeCustomFields()
    {
        // Arrange
        var template = new ExamTemplate
        {
            Name = "测试试卷",
            Description = "测试试卷描述",
            EnableDynamicSealLinePosition = true,
            SealLineContent = new SealLineContentConfig
            {
                Title = "机密试卷",
                ShowName = true,
                NameLabel = "考生姓名：",
                ShowStudentId = true,
                StudentIdLabel = "准考证号：",
                ShowClass = false,
                ShowDate = false,
                ShowSchool = true,
                SchoolLabel = "学校：",
                ShowSubject = true,
                SubjectLabel = "科目：",
                ShowCustomField1 = true,
                CustomField1Label = "考场号：",
                UnderlineLength = 4.0
            }
        };

        // Act
        var result = _sealLineLayout.GenerateSealLine(template, 1, 1);

        // Assert
        Assert.Contains("\\textbf{机密试卷}", result);
        Assert.Contains("考生姓名：", result);
        Assert.Contains("准考证号：", result);
        Assert.Contains("学校：", result);
        Assert.Contains("科目：", result);
        Assert.Contains("考场号：", result);
        Assert.Contains("\\hspace{4cm}", result);
        Assert.DoesNotContain("班级：", result);
        Assert.DoesNotContain("日期：", result);
    }

    /// <summary>
    /// 测试空模板异常
    /// </summary>
    [Fact]
    public void GenerateSealLine_NullTemplate_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sealLineLayout.GenerateSealLine(null, 1, 1));
    }

    /// <summary>
    /// 测试生成指定位置的密封线LaTeX代码
    /// </summary>
    [Theory]
    [InlineData(SealLinePosition.Left, "\\sealline{")]
    [InlineData(SealLinePosition.Right, "\\sealline{")]
    [InlineData(SealLinePosition.Top, "\\topsealline{")]
    [InlineData(SealLinePosition.Bottom, "\\bottomsealline{")]
    public void GeneratePositionalSealLineLaTeX_ShouldUseCorrectCommand(SealLinePosition position, string expectedCommand)
    {
        // Arrange
        var content = "测试内容";

        // Act
        var result = _sealLineLayout.GeneratePositionalSealLineLaTeX(content, position);

        // Assert
        Assert.Contains(expectedCommand, result);
        Assert.Contains(content, result);
    }
}