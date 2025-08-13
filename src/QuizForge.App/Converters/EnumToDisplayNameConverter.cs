using System;
using System.Globalization;
using Avalonia.Data.Converters;
using QuizForge.Models;

namespace QuizForge.App.Converters
{
    /// <summary>
    /// 枚举值到显示名称转换器
    /// </summary>
    public class EnumToDisplayNameConverter : IValueConverter
    {
        /// <summary>
        /// 将枚举值转换为显示名称
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "未知";
            }

            return value switch
            {
                PreviewDisplayMode mode => GetPreviewDisplayModeName(mode),
                _ => value.ToString()
            };
        }

        /// <summary>
        /// 将显示名称转换回枚举值
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取预览显示模式名称
        /// </summary>
        private static string GetPreviewDisplayModeName(PreviewDisplayMode mode)
        {
            return mode switch
            {
                PreviewDisplayMode.SinglePage => "单页模式",
                PreviewDisplayMode.DualPage => "双页模式",
                PreviewDisplayMode.Thumbnail => "缩略图模式",
                PreviewDisplayMode.FullScreen => "全屏模式",
                PreviewDisplayMode.Presentation => "演示模式",
                _ => "未知模式"
            };
        }
    }
}