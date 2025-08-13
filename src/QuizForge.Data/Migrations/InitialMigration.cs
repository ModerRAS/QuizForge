using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using QuizForge.Data.Contexts;
using QuizForge.Models;

namespace QuizForge.Data.Migrations;

/// <summary>
/// 初始数据库迁移
/// </summary>
public class InitialMigration
{
    private readonly QuizDbContext _context;

    /// <summary>
    /// 初始化迁移
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public InitialMigration(QuizDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 执行向上迁移
    /// </summary>
    /// <returns>迁移结果</returns>
    public async Task<bool> UpAsync()
    {
        try
        {
            // 确保数据库已创建
            await _context.Database.EnsureCreatedAsync();
            
            // 应用种子数据
            await SeedDataAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"执行向上迁移失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 执行向下迁移
    /// </summary>
    /// <returns>迁移结果</returns>
    public async Task<bool> DownAsync()
    {
        try
        {
            // 删除数据库
            await _context.Database.EnsureDeletedAsync();
            return true;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"执行向下迁移失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 应用种子数据
    /// </summary>
    /// <returns>种子数据应用结果</returns>
    private async Task SeedDataAsync()
    {
        try
        {
            // 检查是否已经有数据
            if (await _context.QuestionBanks.AnyAsync() || 
                await _context.Templates.AnyAsync() || 
                await _context.Configurations.AnyAsync())
            {
                return; // 已有数据，不需要添加种子数据
            }

            // 添加示例题库
            var sampleQuestionBank = new QuestionBank
            {
                Id = Guid.NewGuid(),
                Name = "示例题库",
                Description = "这是一个示例题库，包含各种类型的题目",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Type = "单选题",
                        Content = "以下哪个是C#的关键字？",
                        Difficulty = "简单",
                        Category = "编程基础",
                        CorrectAnswer = "A",
                        Explanation = "class是C#的关键字，用于定义类。",
                        Points = 5,
                        Options = new List<QuestionOption>
                        {
                            new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "class", IsCorrect = true },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "function", IsCorrect = false },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = "method", IsCorrect = false },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = "variable", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Type = "多选题",
                        Content = "以下哪些是面向对象编程的特性？（多选）",
                        Difficulty = "中等",
                        Category = "编程概念",
                        CorrectAnswer = "A,B,C",
                        Explanation = "封装、继承、多态是面向对象编程的三大特性。",
                        Points = 10,
                        Options = new List<QuestionOption>
                        {
                            new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "封装", IsCorrect = true },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "继承", IsCorrect = true },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "C", Value = "多态", IsCorrect = true },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "D", Value = "递归", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Id = Guid.NewGuid(),
                        Type = "判断题",
                        Content = "C#支持多重继承。",
                        Difficulty = "简单",
                        Category = "编程概念",
                        CorrectAnswer = "B",
                        Explanation = "C#不支持类的多重继承，但支持接口的多重继承。",
                        Points = 3,
                        Options = new List<QuestionOption>
                        {
                            new QuestionOption { Id = Guid.NewGuid(), Key = "A", Value = "正确", IsCorrect = false },
                            new QuestionOption { Id = Guid.NewGuid(), Key = "B", Value = "错误", IsCorrect = true }
                        }
                    }
                }
            };

            await _context.QuestionBanks.AddAsync(sampleQuestionBank);

            // 添加示例模板
            var sampleTemplate = new ExamTemplate
            {
                Id = Guid.NewGuid(),
                Name = "标准考试模板",
                Description = "这是一个标准的考试模板，适用于大多数考试场景",
                PaperSize = PaperSize.A4,
                Style = TemplateStyle.Basic,
                HeaderContent = "某某学校期末考试",
                FooterContent = "第 {page} 页，共 {totalpages} 页",
                SealLine = SealLinePosition.Left,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Sections = new List<TemplateSection>
                {
                    new TemplateSection
                    {
                        Id = Guid.NewGuid(),
                        Title = "第一部分：选择题",
                        Instructions = "请从下列选项中选择最合适的答案。",
                        QuestionCount = 10,
                        TotalPoints = 50,
                        QuestionIds = new List<Guid> { sampleQuestionBank.Questions[0].Id }
                    },
                    new TemplateSection
                    {
                        Id = Guid.NewGuid(),
                        Title = "第二部分：多选题",
                        Instructions = "请选择所有正确的答案，多选或少选均不得分。",
                        QuestionCount = 5,
                        TotalPoints = 25,
                        QuestionIds = new List<Guid> { sampleQuestionBank.Questions[1].Id }
                    },
                    new TemplateSection
                    {
                        Id = Guid.NewGuid(),
                        Title = "第三部分：判断题",
                        Instructions = "判断下列陈述是否正确。",
                        QuestionCount = 5,
                        TotalPoints = 15,
                        QuestionIds = new List<Guid> { sampleQuestionBank.Questions[2].Id }
                    }
                }
            };

            await _context.Templates.AddAsync(sampleTemplate);

            // 添加示例配置
            var defaultConfigurations = new List<Configuration>
            {
                new Configuration
                {
                    Key = "DefaultPaperSize",
                    Value = "\"A4\"",
                    Type = "String",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Configuration
                {
                    Key = "DefaultTemplateStyle",
                    Value = "\"Basic\"",
                    Type = "String",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Configuration
                {
                    Key = "MaxQuestionsPerSection",
                    Value = "50",
                    Type = "Int32",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Configuration
                {
                    Key = "EnableRandomQuestionOrder",
                    Value = "true",
                    Type = "Boolean",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Configuration
                {
                    Key = "DefaultPointsPerQuestion",
                    Value = "5",
                    Type = "Decimal",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Configurations.AddRangeAsync(defaultConfigurations);

            // 保存所有更改
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"应用种子数据失败: {ex.Message}", ex);
        }
    }
}