using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QuizForge.App.ViewModels;
using QuizForge.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace QuizForge.App.Views;

/// <summary>
/// 主窗口
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 初始化主窗口
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 初始化主窗口（带依赖注入）
    /// </summary>
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }
}