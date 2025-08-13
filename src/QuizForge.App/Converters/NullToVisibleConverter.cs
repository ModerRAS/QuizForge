using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QuizForge.App.Converters
{
    /// <summary>
    /// 空值到可见性转换器
    /// </summary>
    public class NullToVisibleConverter : IValueConverter
    {
        /// <summary>
        /// 将空值转换为可见性
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString() == "Invert";
            bool isNull = value == null;

            return invert ? isNull : !isNull;
        }

        /// <summary>
        /// 将可见性转换回空值
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}