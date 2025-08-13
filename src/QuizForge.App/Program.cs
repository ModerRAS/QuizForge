using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.App.ViewModels;
using QuizForge.App.Views;
using QuizForge.Models.Interfaces;
using QuizForge.Services;
using System;

namespace QuizForge.App;

class Program
{
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(builder =>
            {
                // 配置依赖注入
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();
                
                // 将服务提供者存储在应用程序资源中
                var app = (App)builder.Instance;
                app.Services = serviceProvider;
            });

    // 配置服务
    private static void ConfigureServices(IServiceCollection services)
    {
        // 注册视图
        services.AddSingleton<MainWindow>();
        services.AddTransient<QuestionBankView>();
        services.AddTransient<TemplateView>();
        services.AddTransient<ExamGenerationView>();
        services.AddTransient<PdfPreviewView>();
        
        // 注册视图模型
        services.AddSingleton<MainViewModel>();
        services.AddTransient<QuestionBankViewModel>();
        services.AddTransient<TemplateViewModel>();
        services.AddTransient<ExamGenerationViewModel>();
        services.AddTransient<PdfPreviewViewModel>();
        
        // 注册服务
        services.AddSingleton<IQuestionService, QuestionService>();
        services.AddSingleton<ITemplateService, TemplateService>();
        services.AddSingleton<IGenerationService, GenerationService>();
        services.AddSingleton<IExportService, ExportService>();
    }

    // Application entry point. Your main function will be here eventually.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}