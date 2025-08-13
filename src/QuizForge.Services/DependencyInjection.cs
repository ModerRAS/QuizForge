using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using QuizForge.Models.Interfaces;
using QuizForge.Data.Repositories;
using QuizForge.Core.Interfaces;
using QuizForge.Core.Models;
using QuizForge.Core.Services;
using QuizForge.Core.Layout;
using QuizForge.Infrastructure.Parsers;
using QuizForge.Core.ContentGeneration;
using QuizForge.Services;
using QuizForge.Infrastructure.Engines;
using QuizForge.Infrastructure.Services;
using QuizForge.Infrastructure.FileSystems;
using QuizForge.Infrastructure.Renderers;

namespace QuizForge.Services;

/// <summary>
/// 依赖注入配置
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册QuizForge服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQuizForgeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册仓储
        // services.AddScoped<ITemplateRepository, TemplateRepository>(); // TODO: 实现TemplateRepository类
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        
        // 注册布局逻辑
        services.AddScoped<SealLineLayout>();
        services.AddScoped<HeaderLayout>();
        services.AddScoped<HeaderFooterLayout>();
        
        // 注册处理器
        services.AddScoped<IQuestionProcessor, QuestionBankProcessor>();
        services.AddScoped<TemplateProcessor>();
        
        // 注册解析器
        services.AddScoped<IMarkdownParser, MarkdownParser>();
        services.AddScoped<LatexParser>();
        
        // 注册渲染器
        services.AddScoped<MathRenderer>();
        
        // 注册内容生成相关服务
        services.AddScoped<ContentGenerator>();
        services.AddScoped<DynamicContentInserter>();
        services.AddScoped<ExamPaperGenerator>();
        services.AddScoped<IExcelParser, ExcelParser>();
        
        services.AddScoped<IGenerationService, GenerationService>();
        
        // 注册服务
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IExportService, ExportService>();
        
        // 注册基础设施服务
        services.AddScoped<IPdfEngine, PdfEngine>();
        services.AddScoped<IPrintPreviewService, PrintPreviewService>();
        services.AddScoped<IBatchGenerationService, BatchGenerationService>();
        services.AddScoped<IFileService, FileService>();
        
        // 注册PDF错误报告服务
        services.AddScoped<PdfErrorReportingService>();
        
        // 注册PDF缓存服务
        services.AddScoped<PdfCacheService>();
        
        // 注册PDF引擎相关服务
        services.AddScoped<LatexParser>();
        services.AddScoped<MathRenderer>();
        
        return services;
    }
}