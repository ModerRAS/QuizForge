using Microsoft.EntityFrameworkCore;
using QuizForge.Models;

namespace QuizForge.Data.Contexts;

/// <summary>
/// QuizForge 数据库上下文
/// </summary>
public class QuizDbContext : DbContext
{
    /// <summary>
    /// 题库集合
    /// </summary>
    public DbSet<QuestionBank> QuestionBanks { get; set; }

    /// <summary>
    /// 题目集合
    /// </summary>
    public DbSet<Question> Questions { get; set; }

    /// <summary>
    /// 题目选项集合
    /// </summary>
    public DbSet<QuestionOption> QuestionOptions { get; set; }

    /// <summary>
    /// 模板集合
    /// </summary>
    public DbSet<ExamTemplate> Templates { get; set; }

    /// <summary>
    /// 模板章节集合
    /// </summary>
    public DbSet<TemplateSection> TemplateSections { get; set; }

    /// <summary>
    /// 配置集合
    /// </summary>
    public DbSet<Configuration> Configurations { get; set; }

    /// <summary>
    /// 初始化数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置实体映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置QuestionBank实体
        modelBuilder.Entity<QuestionBank>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // 配置与Question的一对多关系
            entity.HasMany(e => e.Questions)
                  .WithOne()
                  .HasForeignKey("QuestionBankId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Question实体
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Difficulty).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CorrectAnswer).IsRequired();
            entity.Property(e => e.Explanation).HasMaxLength(2000);
            entity.Property(e => e.Points).IsRequired();
            
            // 配置与QuestionOption的一对多关系
            entity.HasMany(e => e.Options)
                  .WithOne()
                  .HasForeignKey("QuestionId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置QuestionOption实体
        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.IsCorrect).IsRequired();
        });

        // 配置ExamTemplate实体
        modelBuilder.Entity<ExamTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PaperSize).IsRequired();
            entity.Property(e => e.Style).IsRequired();
            entity.Property(e => e.HeaderContent).HasMaxLength(500);
            entity.Property(e => e.FooterContent).HasMaxLength(500);
            entity.Property(e => e.SealLine).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // 配置与TemplateSection的一对多关系
            entity.HasMany(e => e.Sections)
                  .WithOne()
                  .HasForeignKey("TemplateId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置TemplateSection实体
        modelBuilder.Entity<TemplateSection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Instructions).HasMaxLength(1000);
            entity.Property(e => e.QuestionCount).IsRequired();
            entity.Property(e => e.TotalPoints).IsRequired();
            
            // 将QuestionIds列表存储为JSON字符串
            entity.Property(e => e.QuestionIds)
                  .HasColumnType("TEXT")
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(Guid.Parse)
                           .ToList());
        });

        // 配置Configuration实体
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}

/// <summary>
/// 配置实体
/// </summary>
public class Configuration
{
    /// <summary>
    /// 配置键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 配置值（JSON格式）
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 配置类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}