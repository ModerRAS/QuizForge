using Markdig;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using System.Text.RegularExpressions;

namespace QuizForge.Infrastructure.Parsers;

/// <summary>
/// Markdown 解析器实现
/// </summary>
public class MarkdownParser : IMarkdownParser
{
    /// <summary>
    /// 解析 Markdown 文件内容为题库
    /// </summary>
    /// <param name="filePath">Markdown 文件路径</param>
    /// <returns>解析后的题库</returns>
    public async Task<QuestionBank> ParseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Markdown文件不存在: {filePath}");
        }

        var markdown = await File.ReadAllTextAsync(filePath);
        var lines = markdown.Split('\n');
        
        var questionBank = new QuestionBank
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileNameWithoutExtension(filePath),
            Description = "从Markdown文件导入",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Questions = new List<Question>()
        };

        // 解析试卷标题
        var titleMatch = Regex.Match(markdown, @"^# (.+)$", RegexOptions.Multiline);
        if (titleMatch.Success)
        {
            questionBank.Name = titleMatch.Groups[1].Value.Trim();
        }

        // 解析考试科目和时间
        var subjectMatch = Regex.Match(markdown, @"^## 考试科目：(.+)$", RegexOptions.Multiline);
        var timeMatch = Regex.Match(markdown, @"^## 考试时间：(.+)$", RegexOptions.Multiline);
        
        if (subjectMatch.Success || timeMatch.Success)
        {
            var descriptionParts = new List<string>();
            if (subjectMatch.Success)
            {
                descriptionParts.Add($"考试科目：{subjectMatch.Groups[1].Value.Trim()}");
            }
            if (timeMatch.Success)
            {
                descriptionParts.Add($"考试时间：{timeMatch.Groups[1].Value.Trim()}");
            }
            questionBank.Description = string.Join("，", descriptionParts);
        }

        // 解析题目
        var currentQuestionType = string.Empty;
        var currentQuestion = new Question();
        var questionNumber = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // 检测题目类型
            if (trimmedLine.StartsWith("### 选择题"))
            {
                currentQuestionType = "选择题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 单选题"))
            {
                currentQuestionType = "单选题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 多选题"))
            {
                currentQuestionType = "多选题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 判断题"))
            {
                currentQuestionType = "判断题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 填空题"))
            {
                currentQuestionType = "填空题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 简答题"))
            {
                currentQuestionType = "简答题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 论述题"))
            {
                currentQuestionType = "论述题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 编程题"))
            {
                currentQuestionType = "编程题";
                continue;
            }
            else if (trimmedLine.StartsWith("### 应用题"))
            {
                currentQuestionType = "应用题";
                continue;
            }

            // 解析题目
            var questionMatch = Regex.Match(trimmedLine, @"^(\d+)\.\s*(.+)$");
            if (questionMatch.Success)
            {
                // 保存上一题（如果有）
                if (currentQuestion.Id != Guid.Empty)
                {
                    questionBank.Questions.Add(currentQuestion);
                }

                // 创建新题目
                questionNumber++;
                currentQuestion = new Question
                {
                    Id = Guid.NewGuid(),
                    Type = currentQuestionType,
                    Content = questionMatch.Groups[2].Value.Trim(),
                    Difficulty = "中等",
                    Category = "默认",
                    Points = 1,
                    Options = new List<QuestionOption>()
                };
            }
            // 解析选项
            else if ((currentQuestionType == "选择题" || currentQuestionType == "单选题" || currentQuestionType == "多选题") && Regex.IsMatch(trimmedLine, @"^-\s*[A-Z]\.\s*(.+)$"))
            {
                var optionMatch = Regex.Match(trimmedLine, @"^-\s*([A-Z])\.\s*(.+)$");
                if (optionMatch.Success)
                {
                    currentQuestion.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        Key = optionMatch.Groups[1].Value.Trim(),
                        Value = optionMatch.Groups[2].Value.Trim(),
                        IsCorrect = false
                    });
                }
            }
            // 解析判断题选项
            else if (currentQuestionType == "判断题" && Regex.IsMatch(trimmedLine, @"^-\s*(正确|错误|√|×|T|F)\s*$"))
            {
                var optionMatch = Regex.Match(trimmedLine, @"^-\s*(正确|错误|√|×|T|F)\s*$");
                if (optionMatch.Success)
                {
                    var optionText = optionMatch.Groups[1].Value.Trim();
                    var normalizedText = NormalizeJudgmentOption(optionText);
                    
                    currentQuestion.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        Key = normalizedText,
                        Value = optionText,
                        IsCorrect = false
                    });
                }
            }
            // 解析答案
            else if (trimmedLine.StartsWith("- 答案："))
            {
                var answer = trimmedLine.Substring("- 答案：".Length).Trim();
                currentQuestion.CorrectAnswer = answer;

                // 如果是单选题或多选题，标记正确选项
                if (currentQuestionType == "选择题" || currentQuestionType == "单选题")
                {
                    foreach (var option in currentQuestion.Options)
                    {
                        if (option.Key.Equals(answer, StringComparison.OrdinalIgnoreCase))
                        {
                            option.IsCorrect = true;
                        }
                    }
                }
                // 如果是多选题，处理多个答案
                else if (currentQuestionType == "多选题")
                {
                    var answers = answer.Split(new[] { ',', '，', '、', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var option in currentQuestion.Options)
                    {
                        if (answers.Contains(option.Key, StringComparer.OrdinalIgnoreCase))
                        {
                            option.IsCorrect = true;
                        }
                    }
                }
                // 如果是判断题，标记正确选项
                else if (currentQuestionType == "判断题")
                {
                    var normalizedAnswer = NormalizeJudgmentOption(answer);
                    foreach (var option in currentQuestion.Options)
                    {
                        if (option.Key.Equals(normalizedAnswer, StringComparison.OrdinalIgnoreCase))
                        {
                            option.IsCorrect = true;
                        }
                    }
                }
            }
        }

        // 添加最后一题
        if (currentQuestion.Id != Guid.Empty)
        {
            questionBank.Questions.Add(currentQuestion);
        }

        return questionBank;
    }

    /// <summary>
    /// 标准化判断题选项
    /// </summary>
    /// <param name="option">选项文本</param>
    /// <returns>标准化后的选项文本</returns>
    private string NormalizeJudgmentOption(string option)
    {
        return option.ToLower() switch
        {
            "正确" or "√" or "t" => "正确",
            "错误" or "×" or "f" => "错误",
            _ => option
        };
    }
    
    /// <summary>
    /// 验证 Markdown 文件格式
    /// </summary>
    /// <param name="filePath">Markdown 文件路径</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateFormatAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            var markdown = await File.ReadAllTextAsync(filePath);
            
            // 检查是否包含试卷标题
            var hasTitle = Regex.IsMatch(markdown, @"^# .+$", RegexOptions.Multiline);
            
            // 检查是否包含题目类型
            var hasQuestionTypes = Regex.IsMatch(markdown, @"^### (选择题|单选题|多选题|判断题|填空题|简答题|论述题|编程题|应用题)$", RegexOptions.Multiline);
            
            // 检查是否包含题目
            var hasQuestions = Regex.IsMatch(markdown, @"^\d+\.\s.+$", RegexOptions.Multiline);
            
            return hasTitle && hasQuestionTypes && hasQuestions;
        }
        catch
        {
            return false;
        }
    }
}