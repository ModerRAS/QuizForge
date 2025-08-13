using Microsoft.Extensions.DependencyInjection;
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
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQuizForgeServices(this IServiceCollection services)
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
        
        return services;
    }
}