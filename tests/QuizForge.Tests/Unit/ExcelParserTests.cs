using QuizForge.Infrastructure.Parsers;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// Excel解析器单元测试
/// </summary>
public class ExcelParserTests
{
    private readonly IExcelParser _excelParser;

    public ExcelParserTests()
    {
        _excelParser = new ExcelParser();
    }

    [Fact]
    public async Task ParseAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentFilePath = "non_existent_file.xlsx";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => _excelParser.ParseAsync(nonExistentFilePath));
        
        Assert.Contains(nonExistentFilePath, exception.Message);
    }

    [Fact]
    public async Task ValidateFormatAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentFilePath = "non_existent_file.xlsx";

        // Act
        var result = await _excelParser.ValidateFormatAsync(nonExistentFilePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFormatAsync_WithInvalidExcelFile_ShouldReturnFalse()
    {
        // Arrange
        var invalidContent = "这是一个无效的Excel文件内容";
        var tempFilePath = Path.GetTempFileName() + ".xlsx";
        await File.WriteAllTextAsync(tempFilePath, invalidContent);

        try
        {
            // Act
            var result = await _excelParser.ValidateFormatAsync(tempFilePath);

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
    public async Task ParseAsync_WithValidExcelFile_ShouldReturnQuestionBank()
    {
        // Arrange
        var tempFilePath = CreateTestExcelFile();

        try
        {
            // Act
            var result = await _excelParser.ParseAsync(tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("测试题库", result.Name);
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
            Assert.Equal("直角三角形的两条直角边的平方和等于斜边的平方", essayQuestion.CorrectAnswer);
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
    public async Task ValidateFormatAsync_WithValidExcelFile_ShouldReturnTrue()
    {
        // Arrange
        var tempFilePath = CreateTestExcelFile();

        try
        {
            // Act
            var result = await _excelParser.ValidateFormatAsync(tempFilePath);

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
    public async Task ParseAsync_WithMultipleChoiceQuestions_ShouldParseCorrectly()
    {
        // Arrange
        var tempFilePath = CreateMultipleChoiceTestExcelFile();

        try
        {
            // Act
            var result = await _excelParser.ParseAsync(tempFilePath);

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
    public async Task ParseAsync_WithMissingRequiredColumns_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tempFilePath = CreateInvalidExcelFile();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _excelParser.ParseAsync(tempFilePath));
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

    /// <summary>
    /// 创建测试用的Excel文件
    /// </summary>
    /// <returns>Excel文件路径</returns>
    private string CreateTestExcelFile()
    {
        var tempFilePath = Path.GetTempFileName() + ".xlsx";
        
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("题库");

        // 添加标题行
        worksheet.Cell(1, 1).Value = "题型";
        worksheet.Cell(1, 2).Value = "题目";
        worksheet.Cell(1, 3).Value = "选项A";
        worksheet.Cell(1, 4).Value = "选项B";
        worksheet.Cell(1, 5).Value = "选项C";
        worksheet.Cell(1, 6).Value = "选项D";
        worksheet.Cell(1, 7).Value = "答案";

        // 添加选择题
        worksheet.Cell(2, 1).Value = "选择题";
        worksheet.Cell(2, 2).Value = "1+1=？";
        worksheet.Cell(2, 3).Value = "1";
        worksheet.Cell(2, 4).Value = "2";
        worksheet.Cell(2, 5).Value = "3";
        worksheet.Cell(2, 6).Value = "4";
        worksheet.Cell(2, 7).Value = "B";

        // 添加填空题
        worksheet.Cell(3, 1).Value = "填空题";
        worksheet.Cell(3, 2).Value = "2+2=____";
        worksheet.Cell(3, 7).Value = "4";

        // 添加简答题
        worksheet.Cell(4, 1).Value = "简答题";
        worksheet.Cell(4, 2).Value = "请简述勾股定理。";
        worksheet.Cell(4, 7).Value = "直角三角形的两条直角边的平方和等于斜边的平方";

        workbook.SaveAs(tempFilePath);
        return tempFilePath;
    }

    /// <summary>
    /// 创建包含多个选择题的测试Excel文件
    /// </summary>
    /// <returns>Excel文件路径</returns>
    private string CreateMultipleChoiceTestExcelFile()
    {
        var tempFilePath = Path.GetTempFileName() + ".xlsx";
        
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("题库");

        // 添加标题行
        worksheet.Cell(1, 1).Value = "题型";
        worksheet.Cell(1, 2).Value = "题目";
        worksheet.Cell(1, 3).Value = "选项A";
        worksheet.Cell(1, 4).Value = "选项B";
        worksheet.Cell(1, 5).Value = "选项C";
        worksheet.Cell(1, 6).Value = "选项D";
        worksheet.Cell(1, 7).Value = "答案";

        // 添加第一题
        worksheet.Cell(2, 1).Value = "选择题";
        worksheet.Cell(2, 2).Value = "什么是勾股定理？";
        worksheet.Cell(2, 3).Value = "直角三角形的两条直角边的和等于斜边";
        worksheet.Cell(2, 4).Value = "直角三角形的两条直角边的平方和等于斜边的平方";
        worksheet.Cell(2, 5).Value = "直角三角形的两条直角边的积等于斜边";
        worksheet.Cell(2, 6).Value = "直角三角形的两条直角边的差等于斜边";
        worksheet.Cell(2, 7).Value = "B";

        // 添加第二题
        worksheet.Cell(3, 1).Value = "选择题";
        worksheet.Cell(3, 2).Value = "下列哪个是质数？";
        worksheet.Cell(3, 3).Value = "4";
        worksheet.Cell(3, 4).Value = "6";
        worksheet.Cell(3, 5).Value = "7";
        worksheet.Cell(3, 6).Value = "9";
        worksheet.Cell(3, 7).Value = "C";

        workbook.SaveAs(tempFilePath);
        return tempFilePath;
    }

    /// <summary>
    /// 创建无效的测试Excel文件（缺少必要列）
    /// </summary>
    /// <returns>Excel文件路径</returns>
    private string CreateInvalidExcelFile()
    {
        var tempFilePath = Path.GetTempFileName() + ".xlsx";
        
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("题库");

        // 添加不完整的标题行（缺少"题型"列）
        worksheet.Cell(1, 1).Value = "题目";
        worksheet.Cell(1, 2).Value = "选项A";
        worksheet.Cell(1, 3).Value = "选项B";
        worksheet.Cell(1, 4).Value = "答案";

        // 添加数据行
        worksheet.Cell(2, 1).Value = "1+1=？";
        worksheet.Cell(2, 2).Value = "1";
        worksheet.Cell(2, 3).Value = "2";
        worksheet.Cell(2, 4).Value = "B";

        workbook.SaveAs(tempFilePath);
        return tempFilePath;
    }
}