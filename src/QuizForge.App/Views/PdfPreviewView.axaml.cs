using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia;
using QuizForge.App.ViewModels;

namespace QuizForge.App.Views;

/// <summary>
/// PDF预览视图
/// </summary>
public partial class PdfPreviewView : UserControl
{
    private bool _isDragging = false;
    private Point _lastMousePosition;

    /// <summary>
    /// 初始化PDF预览视图
    /// </summary>
    public PdfPreviewView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 预览图像鼠标滚轮事件处理
    /// </summary>
    private void PreviewImage_PointerWheel(object sender, PointerWheelEventArgs e)
    {
        if (DataContext is PdfPreviewViewModel viewModel)
        {
            viewModel.HandleMouseWheel(e.Delta.Y > 0 ? 1 : -1);
        }
    }

    /// <summary>
    /// 预览图像鼠标按下事件处理
    /// </summary>
    private void PreviewImage_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (DataContext is PdfPreviewViewModel viewModel && viewModel.IsDragNavigationEnabled)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);
            Cursor = new Cursor(StandardCursorType.Hand);
        }
    }

    /// <summary>
    /// 预览图像鼠标移动事件处理
    /// </summary>
    private void PreviewImage_PointerMoved(object sender, PointerEventArgs e)
    {
        if (_isDragging && DataContext is PdfPreviewViewModel viewModel)
        {
            var currentPosition = e.GetPosition(this);
            var deltaX = currentPosition.X - _lastMousePosition.X;
            var deltaY = currentPosition.Y - _lastMousePosition.Y;
            
            viewModel.HandleMouseDrag(deltaX, deltaY);
            
            _lastMousePosition = currentPosition;
        }
    }

    /// <summary>
    /// 预览图像鼠标释放事件处理
    /// </summary>
    private void PreviewImage_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
        Cursor = Cursor.Default;
    }

    /// <summary>
    /// 缩略图鼠标按下事件处理
    /// </summary>
    private void Thumbnail_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && DataContext is PdfPreviewViewModel viewModel)
        {
            // 获取缩略图项
            if (border.DataContext is PdfPreviewViewModel.ThumbnailItem thumbnailItem)
            {
                viewModel.HandleThumbnailClick(thumbnailItem.PageNumber - 1);
            }
        }
    }
}