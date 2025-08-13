using QuizForge.Infrastructure.Parsers;
using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// Markdown解析器单元测试
/// </summary>
public class MarkdownParserTests
{
    private readonly IMarkdownParser _markdownParser;

    public MarkdownParserTests()
    {
        _markdownParser = new MarkdownParser();
    }

    [Fact]
    public async Task ParseAsync_WithValidMarkdownFile_ShouldReturnQuestionBank()
    {
        // Arrange
        var markdownContent = @"# 试卷标题
## 考试科目：数学
## 考试时间：120分钟

### 选择题
1. 1+1=？
   - A. 1
   - B. 2
   - C. 3
   - D. 4
   - 答案：B

### 填空题
2. 2+2=____
   - 答案：4

### 简答题
3. 请简述勾股定理。
   - 答案：直角三角形的两条直角边的平方和等于斜边的平方。";

        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, markdownContent);

        try
        {
            // Act
            var result = await _markdownParser.ParseAsync(tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("试卷标题", result.Name);
            Assert.Contains("考试科目：数学", result.Description);
            Assert.Contains("考试时间：120分钟", result.Description);
            Assert.Equal(3, result.Questions.Count);

            // 验证选择题
            var multipleChoiceQuestion = result.Questions.FirstOrDefault(q => q.Type == "选择题");
            Assert.NotNull(multipleChoiceQuestion);
            Assert.Equal("1+1=？", multipleChoiceQuestion.Content);
            Assert.Equal(4, multipleChoiceQuestion.Options.Count);
            Assert.Equal("B", multipleChoiceQuestion.CorrectAnswer);
            Assert.True(multipleChoiceQuestion.Options.FirstOrDefault(o => o.Key == "B")?.IsCorrect);

            // 验证填空题
            var fillInBlankQuestion = result.Questions.FirstOrDefault(q => q.Type == "填空题");
            Assert.NotNull(fillInBlankQuestion);
            Assert.Equal("2+2=____", fillInBlankQuestion.Content);
            Assert.Equal("4", fillInBlankQuestion.CorrectAnswer);

            // 验证简答题
            var essayQuestion = result.Questions.FirstOrDefault(q => q.Type == "简答题");
            Assert.NotNull(essayQuestion);
            Assert.Equal("请简述勾股定理。", essayQuestion.Content);
            Assert.Equal("直角三角形的两条直角边的平方和等于斜边的平方。", essayQuestion.CorrectAnswer);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ParseAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentFilePath = "non_existent_file.md";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => _markdownParser.ParseAsync(nonExistentFilePath));
        
        Assert.Contains(nonExistentFilePath, exception.Message);
    }

    [Fact]
    public async Task ValidateFormatAsync_WithValidMarkdownFile_ShouldReturnTrue()
    {
        // Arrange
        var markdownContent = @"# 试卷标题
## 考试科目：数学
## 考试时间：120分钟

### 选择题
1. 1+1=？
   - A. 1
   - B. 2
   - C. 3
   - D. 4
   - 答案：B";

        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, markdownContent);

        try
        {
            // Act
            var result = await _markdownParser.ValidateFormatAsync(tempFilePath);

            // Assert
            Assert.True(result);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ValidateFormatAsync_WithInvalidMarkdownFile_ShouldReturnFalse()
    {
        // Arrange
        var invalidMarkdownContent = @"这是一个无效的Markdown文件
没有标题
没有题目类型
没有题目";

        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, invalidMarkdownContent);

        try
        {
            // Act
            var result = await _markdownParser.ValidateFormatAsync(tempFilePath);

            // Assert
            Assert.False(result);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ValidateFormatAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentFilePath = "non_existent_file.md";

        // Act
        var result = await _markdownParser.ValidateFormatAsync(nonExistentFilePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ParseAsync_WithMultipleChoiceQuestions_ShouldParseCorrectly()
    {
        // Arrange
        var markdownContent = @"# 数学测试

### 选择题
1. 什么是勾股定理？
   - A. 直角三角形的两条直角边的和等于斜边
   - B. 直角三角形的两条直角边的平方和等于斜边的平方
   - C. 直角三角形的两条直角边的积等于斜边
   - D. 直角三角形的两条直角边的差等于斜边
   - 答案：B

2. 下列哪个是质数？
   - A. 4
   - B. 6
   - C. 7
   - D. 9
   - 答案：C";

        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, markdownContent);

        try
        {
            // Act
            var result = await _markdownParser.ParseAsync(tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Questions.Count);

            // 验证第一题
            var firstQuestion = result.Questions[0];
            Assert.Equal("什么是勾股定理？", firstQuestion.Content);
            Assert.Equal(4, firstQuestion.Options.Count);
            Assert.Equal("B", firstQuestion.CorrectAnswer);
            Assert.True(firstQuestion.Options.FirstOrDefault(o => o.Key == "B")?.IsCorrect);
            Assert.False(firstQuestion.Options.FirstOrDefault(o => o.Key == "A")?.IsCorrect);

            // 验证第二题
            var secondQuestion = result.Questions[1];
            Assert.Equal("下列哪个是质数？", secondQuestion.Content);
            Assert.Equal(4, secondQuestion.Options.Count);
            Assert.Equal("C", secondQuestion.CorrectAnswer);
            Assert.True(secondQuestion.Options.FirstOrDefault(o => o.Key == "C")?.IsCorrect);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ParseAsync_WithMixedQuestionTypes_ShouldParseCorrectly()
    {
        // Arrange
        var markdownContent = @"# 综合测试

### 选择题
1. 1+1=？
   - A. 1
   - B. 2
   - C. 3
   - D. 4
   - 答案：B

### 填空题
2. 中国的首都是____
   - 答案：北京

### 简答题
3. 请简述水的化学式。
   - 答案：H2O

### 选择题
4. 下列哪个是编程语言？
   - A. HTML
   - B. CSS
   - C. JavaScript
   - D. 以上都是
   - 答案：D";

        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, markdownContent);

        try
        {
            // Act
            var result = await _markdownParser.ParseAsync(tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Questions.Count);

            // 验证题目类型
            Assert.Equal("选择题", result.Questions[0].Type);
            Assert.Equal("填空题", result.Questions[1].Type);
            Assert.Equal("简答题", result.Questions[2].Type);
            Assert.Equal("选择题", result.Questions[3].Type);

            // 验证选择题选项
            Assert.Equal(4, result.Questions[0].Options.Count);
            Assert.Equal(4, result.Questions[3].Options.Count);
            
            // 验证填空题和简答题没有选项
            Assert.Empty(result.Questions[1].Options);
            Assert.Empty(result.Questions[2].Options);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}