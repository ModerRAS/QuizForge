using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.Data.Contexts;
using QuizForge.Data.Repositories;
using QuizForge.Data.Services;
using QuizForge.Models.Interfaces;

namespace QuizForge.Data;

/// <summary>
/// 依赖注入扩展类
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 添加数据访问层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string databasePath)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("数据库路径不能为空", nameof(databasePath));
        }

        // 注册数据库上下文
        services.AddDbContext<QuizDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        // 注册仓储
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        // 注册数据库服务
        services.AddScoped<DatabaseService>();

        return services;
    }

    /// <summary>
    /// 添加数据访问层服务（使用连接字符串）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDataAccessWithConnectionString(this IServiceCollection services, string connectionString)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("连接字符串不能为空", nameof(connectionString));
        }

        // 注册数据库上下文
        services.AddDbContext<QuizDbContext>(options =>
            options.UseSqlite(connectionString));

        // 注册仓储
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        // 注册数据库服务
        services.AddScoped<DatabaseService>();

        return services;
    }

    /// <summary>
    /// 添加数据访问层服务（使用配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "DefaultConnection")
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("连接字符串名称不能为空", nameof(connectionStringName));
        }

        // 获取连接字符串
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"未找到名为 '{connectionStringName}' 的连接字符串");
        }

        // 注册数据库上下文
        services.AddDbContext<QuizDbContext>(options =>
            options.UseSqlite(connectionString));

        // 注册仓储
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        // 注册数据库服务
        services.AddScoped<DatabaseService>();

        return services;
    }
}