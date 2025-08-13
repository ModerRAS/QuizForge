using ClosedXML.Excel;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Infrastructure.Parsers;

/// <summary>
/// Excel解析器实现
/// </summary>
public class ExcelParser : IExcelParser
{
    /// <summary>
    /// 解析Excel文件内容为题库
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <returns>解析后的题库</returns>
    public async Task<QuestionBank> ParseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Excel文件不存在: {filePath}");
        }

        var questionBank = new QuestionBank
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileNameWithoutExtension(filePath),
            Description = "从Excel文件导入",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Questions = new List<Question>()
        };

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1); // 使用第一个工作表

            // 查找标题行
            var headerRow = FindHeaderRow(worksheet);
            if (headerRow == 0)
            {
                throw new InvalidOperationException("无法找到Excel文件中的标题行");
            }

            // 获取列索引
            var columnIndices = GetColumnIndices(worksheet.Row(headerRow));

            // 从标题行下一行开始读取数据
            var currentRow = headerRow + 1;
            while (!worksheet.Row(currentRow).IsEmpty())
            {
                var question = ParseQuestion(worksheet.Row(currentRow), columnIndices);
                if (question != null)
                {
                    questionBank.Questions.Add(question);
                }
                currentRow++;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"解析Excel文件时发生错误: {ex.Message}", ex);
        }

        return questionBank;
    }

    /// <summary>
    /// 验证Excel文件格式
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateFormatAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            // 检查是否有工作表
            if (worksheet == null)
            {
                return false;
            }

            // 查找标题行
            var headerRow = FindHeaderRow(worksheet);
            if (headerRow == 0)
            {
                return false;
            }

            // 检查必要的列是否存在
            var columnIndices = GetColumnIndices(worksheet.Row(headerRow));
            if (columnIndices.TypeColumn == 0 || 
                columnIndices.QuestionColumn == 0 || 
                columnIndices.AnswerColumn == 0)
            {
                return false;
            }

            // 检查是否有数据行
            var firstDataRow = headerRow + 1;
            if (worksheet.Row(firstDataRow).IsEmpty())
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 查找标题行
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <returns>标题行号，如果未找到返回0</returns>
    private int FindHeaderRow(IXLWorksheet worksheet)
    {
        // 查找包含"题型"的行作为标题行
        for (int row = 1; row <= 10; row++) // 只检查前10行
        {
            var cellValue = worksheet.Cell(row, 1).GetString();
            if (cellValue.Contains("题型"))
            {
                return row;
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取列索引
    /// </summary>
    /// <param name="headerRow">标题行</param>
    /// <returns>列索引信息</returns>
    private ColumnIndices GetColumnIndices(IXLRow headerRow)
    {
        var indices = new ColumnIndices();

        foreach (var cell in headerRow.Cells())
        {
            var cellValue = cell.GetString();
            var columnIndex = cell.Address.ColumnNumber;

            switch (cellValue)
            {
                case var _ when cellValue.Contains("题型"):
                    indices.TypeColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("题目"):
                    indices.QuestionColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("选项A"):
                    indices.OptionAColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("选项B"):
                    indices.OptionBColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("选项C"):
                    indices.OptionCColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("选项D"):
                    indices.OptionDColumn = columnIndex;
                    break;
                case var _ when cellValue.Contains("答案"):
                    indices.AnswerColumn = columnIndex;
                    break;
            }
        }

        return indices;
    }

    /// <summary>
    /// 解析单行数据为题目
    /// </summary>
    /// <param name="row">数据行</param>
    /// <param name="columnIndices">列索引</param>
    /// <returns>题目对象</returns>
    private Question? ParseQuestion(IXLRow row, ColumnIndices columnIndices)
    {
        var type = row.Cell(columnIndices.TypeColumn).GetString();
        var content = row.Cell(columnIndices.QuestionColumn).GetString();
        var answer = row.Cell(columnIndices.AnswerColumn).GetString();

        // 验证必要字段
        if (string.IsNullOrWhiteSpace(type) || 
            string.IsNullOrWhiteSpace(content) || 
            string.IsNullOrWhiteSpace(answer))
        {
            return null;
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Type = type.Trim(),
            Content = content.Trim(),
            Difficulty = "中等",
            Category = "默认",
            Points = 1,
            CorrectAnswer = answer.Trim(),
            Options = new List<QuestionOption>()
        };

        // 如果是选择题，解析选项
        if (type.Contains("选择"))
        {
            ParseOptions(row, columnIndices, question);
        }

        return question;
    }

    /// <summary>
    /// 解析选择题选项
    /// </summary>
    /// <param name="row">数据行</param>
    /// <param name="columnIndices">列索引</param>
    /// <param name="question">题目对象</param>
    private void ParseOptions(IXLRow row, ColumnIndices columnIndices, Question question)
    {
        if (columnIndices.OptionAColumn > 0)
        {
            var optionA = row.Cell(columnIndices.OptionAColumn).GetString();
            if (!string.IsNullOrWhiteSpace(optionA))
            {
                question.Options.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Key = "A",
                    Value = optionA.Trim(),
                    IsCorrect = question.CorrectAnswer.Equals("A", StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        if (columnIndices.OptionBColumn > 0)
        {
            var optionB = row.Cell(columnIndices.OptionBColumn).GetString();
            if (!string.IsNullOrWhiteSpace(optionB))
            {
                question.Options.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Key = "B",
                    Value = optionB.Trim(),
                    IsCorrect = question.CorrectAnswer.Equals("B", StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        if (columnIndices.OptionCColumn > 0)
        {
            var optionC = row.Cell(columnIndices.OptionCColumn).GetString();
            if (!string.IsNullOrWhiteSpace(optionC))
            {
                question.Options.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Key = "C",
                    Value = optionC.Trim(),
                    IsCorrect = question.CorrectAnswer.Equals("C", StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        if (columnIndices.OptionDColumn > 0)
        {
            var optionD = row.Cell(columnIndices.OptionDColumn).GetString();
            if (!string.IsNullOrWhiteSpace(optionD))
            {
                question.Options.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Key = "D",
                    Value = optionD.Trim(),
                    IsCorrect = question.CorrectAnswer.Equals("D", StringComparison.OrdinalIgnoreCase)
                });
            }
        }
    }

    /// <summary>
    /// 列索引信息
    /// </summary>
    private struct ColumnIndices
    {
        public int TypeColumn { get; set; }
        public int QuestionColumn { get; set; }
        public int OptionAColumn { get; set; }
        public int OptionBColumn { get; set; }
        public int OptionCColumn { get; set; }
        public int OptionDColumn { get; set; }
        public int AnswerColumn { get; set; }
    }
}