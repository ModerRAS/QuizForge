using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QuizForge.App.Converters
{
    /// <summary>
    /// 缩放转换器
    /// </summary>
    public class ZoomConverter : IValueConverter
    {
        /// <summary>
        /// 将缩放级别转换为实际尺寸
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double zoomLevel && parameter is string baseSizeStr && double.TryParse(baseSizeStr, out double baseSize))
            {
                return baseSize * (zoomLevel / 100.0);
            }

            return value;
        }

        /// <summary>
        /// 将实际尺寸转换回缩放级别
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}