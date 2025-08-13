using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QuizForge.App.ViewModels;
using QuizForge.App.Views;
using System;

namespace QuizForge.App;

public partial class App : Application
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? Services { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            // ExpressionFactory.DataValidationPlugins.Remove(new DataAnnotationsValidationPlugin());
            
            desktop.MainWindow = Services?.GetRequiredService<MainWindow>() ?? new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}