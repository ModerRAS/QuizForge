using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizForge.CLI.Commands;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Core.ContentGeneration;
using QuizForge.Infrastructure.Engines;
using QuizForge.Services;
using QuizForge.Data;
using QuizForge.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI;

/// <summary>
/// QuizForge CLI主程序
/// </summary>
public class Program
{
    /// <summary>
    /// 主入口点
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>退出代码</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // 创建服务集合
            var services = new ServiceCollection();
            
            // 配置服务
            ConfigureServices(services, args);
            
            // 创建命令应用，使用类型注册器
            var app = new CommandApp(new TypeRegistrar(services));
            
            // 配置命令
            app.Configure(config =>
            {
                config.AddBranch("generate", generate =>
                {
                    generate.AddCommand<ExcelGenerateCommand>("excel")
                        .WithDescription("从Excel文件生成试卷PDF");
                    generate.AddCommand<MarkdownGenerateCommand>("markdown")
                        .WithDescription("从Markdown文件生成试卷PDF");
                });
                
                config.AddCommand<BatchCommand>("batch")
                    .WithDescription("批量处理多个文件");
                
                config.AddCommand<ValidateCommand>("validate")
                    .WithDescription("验证文件格式");
                
                config.AddBranch("template", template =>
                {
                    template.AddCommand<TemplateListCommand>("list")
                        .WithDescription("列出可用模板");
                    template.AddCommand<TemplateCreateCommand>("create")
                        .WithDescription("创建新模板");
                    template.AddCommand<TemplateDeleteCommand>("delete")
                        .WithDescription("删除模板");
                });
                
                config.AddBranch("config", configCmd =>
                {
                    configCmd.AddCommand<ConfigShowCommand>("show")
                        .WithDescription("显示当前配置");
                    configCmd.AddCommand<ConfigSetCommand>("set")
                        .WithDescription("设置配置项");
                    configCmd.AddCommand<ConfigResetCommand>("reset")
                        .WithDescription("重置配置");
                });
            });

            // 执行命令
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]错误:[/] {0}", ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="args">命令行参数</param>
    private static void ConfigureServices(IServiceCollection services, string[] args)
    {
        // 配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // 注册基础设施服务
        services.AddScoped<IExcelParser, ExcelParser>();
        services.AddScoped<IMarkdownParser, MarkdownParser>();
        services.AddScoped<IPdfEngine, LatexPdfEngine>();

        // 注册CLI专用服务
        services.AddScoped<ICliFileService, CliFileService>();
        services.AddScoped<ICliProgressService, CliProgressService>();
        services.AddScoped<ICliConfigurationService, CliConfigurationService>();
        services.AddScoped<ICliGenerationService, CliGenerationService>();
        services.AddScoped<ICliValidationService, CliValidationService>();

        // 注册核心组件
        services.AddScoped<LaTeXDocumentGenerator>();
        services.AddScoped<ContentGenerator>();

        // 注册QuizForge服务
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IGenerationService, GenerationService>();
        services.AddScoped<IExportService, ExportService>();

        // 注册日志服务
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // 配置选项
        services.Configure<LaTeXOptions>(configuration.GetSection("LaTeX"));
        services.Configure<ExcelOptions>(configuration.GetSection("Excel"));
        services.Configure<PdfOptions>(configuration.GetSection("PDF"));
        services.Configure<TemplateOptions>(configuration.GetSection("Templates"));
        services.Configure<CliOptions>(configuration.GetSection("CLI"));

        // 注册命令类
        services.AddScoped<ExcelGenerateCommand>();
        services.AddScoped<MarkdownGenerateCommand>();
        services.AddScoped<BatchCommand>();
        services.AddScoped<ValidateCommand>();
        services.AddScoped<TemplateListCommand>();
        services.AddScoped<TemplateCreateCommand>();
        services.AddScoped<TemplateDeleteCommand>();
        services.AddScoped<ConfigShowCommand>();
        services.AddScoped<ConfigSetCommand>();
        services.AddScoped<ConfigResetCommand>();
    }
}

/// <summary>
/// Spectre.Console类型注册器
/// </summary>
public class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_services.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _services.AddScoped(service, implementation);
        // 同时注册实现类型，以便Spectre.Console可以解析
        _services.AddScoped(implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _services.AddSingleton(service, provider => factory());
    }
}

/// <summary>
/// Spectre.Console类型解析器
/// </summary>
public class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type)
    {
        return type != null ? _provider.GetService(type) : null;
    }
}