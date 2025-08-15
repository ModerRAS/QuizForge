using QuizForge.App.Converters;
using QuizForge.Models.Interfaces;
using Xunit;
using Avalonia.Controls;

namespace QuizForge.Tests.Converters;

/// <summary>
/// 转换器测试类
/// </summary>
public class ConverterTests
{
    /// <summary>
    /// 测试NullToVisibleConverter
    /// </summary>
    [Fact]
    public void NullToVisibleConverter_ShouldConvertCorrectly()
    {
        // Arrange
        var converter = new NullToVisibleConverter();

        // Act & Assert - 测试null值
        var nullResult = converter.Convert(null, typeof(bool), null, null);
        Assert.False((bool)nullResult);

        // Act & Assert - 测试非null值
        var notNullResult = converter.Convert("test", typeof(bool), null, null);
        Assert.True((bool)notNullResult);
    }

    /// <summary>
    /// 测试NullToVisibleConverter反向转换
    /// </summary>
    [Fact]
    public void NullToVisibleConverter_ShouldConvertBackCorrectly()
    {
        // Arrange
        var converter = new NullToVisibleConverter();

        // Act & Assert - 反向转换应该抛出NotImplementedException
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(true, typeof(object), null, null));
    }

    /// <summary>
    /// 测试ZoomConverter
    /// </summary>
    [Theory]
    [InlineData(1.0, 100.0, 100.0)]  // 100%缩放
    [InlineData(1.5, 100.0, 150.0)]  // 150%缩放
    [InlineData(0.5, 100.0, 50.0)]   // 50%缩放
    [InlineData(2.0, 100.0, 200.0)]  // 200%缩放
    public void ZoomConverter_ShouldCalculateCorrectSize(double zoomLevel, double originalSize, double expectedSize)
    {
        // Arrange
        var converter = new ZoomConverter();
        var parameters = new object[] { originalSize };

        // Act
        var result = converter.Convert(zoomLevel, typeof(double), parameters, null);

        // Assert
        Assert.Equal(expectedSize, (double)result);
    }

    /// <summary>
    /// 测试ZoomConverter无参数情况
    /// </summary>
    [Fact]
    public void ZoomConverter_WithoutParameters_ShouldReturnSameValue()
    {
        // Arrange
        var converter = new ZoomConverter();
        var zoomLevel = 1.5;

        // Act
        var result = converter.Convert(zoomLevel, typeof(double), null, null);

        // Assert
        Assert.Equal(zoomLevel, (double)result);
    }

    /// <summary>
    /// 测试ZoomConverter反向转换
    /// </summary>
    [Fact]
    public void ZoomConverter_ShouldConvertBackCorrectly()
    {
        // Arrange
        var converter = new ZoomConverter();

        // Act & Assert - 反向转换应该抛出NotImplementedException
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(150.0, typeof(object), null, null));
    }

    /// <summary>
    /// 测试EnumToDisplayNameConverter
    /// </summary>
    [Theory]
    [InlineData(PreviewDisplayMode.SinglePage, "单页")]
    [InlineData(PreviewDisplayMode.DualPage, "双页")]
    [InlineData(PreviewDisplayMode.ContinuousScroll, "连续")]
    public void EnumToDisplayNameConverter_ShouldConvertPreviewDisplayMode(PreviewDisplayMode mode, string expectedDisplayName)
    {
        // Arrange
        var converter = new EnumToDisplayNameConverter();

        // Act
        var result = converter.Convert(mode, typeof(string), null, null);

        // Assert
        Assert.Equal(expectedDisplayName, (string)result);
    }

    /// <summary>
    /// 测试EnumToDisplayNameConverter对于PreviewQuality值
    /// </summary>
    [Theory]
    [InlineData(50, "低")]
    [InlineData(75, "中")]
    [InlineData(90, "高")]
    public void EnumToDisplayNameConverter_ShouldConvertPreviewQuality(int quality, string expectedDisplayName)
    {
        // Arrange
        var converter = new EnumToDisplayNameConverter();

        // Act
        var result = converter.Convert(quality, typeof(string), null, null);

        // Assert
        Assert.Equal(expectedDisplayName, (string)result);
    }

    /// <summary>
    /// 测试EnumToDisplayNameConverter对于未知枚举值
    /// </summary>
    [Fact]
    public void EnumToDisplayNameConverter_UnknownEnumValue_ShouldReturnEnumName()
    {
        // Arrange
        var converter = new EnumToDisplayNameConverter();
        var unknownValue = (PreviewDisplayMode)999; // 不存在的枚举值

        // Act
        var result = converter.Convert(unknownValue, typeof(string), null, null);

        // Assert
        Assert.Equal("999", (string)result);
    }

    /// <summary>
    /// 测试EnumToDisplayNameConverter对于非枚举值
    /// </summary>
    [Fact]
    public void EnumToDisplayNameConverter_NonEnumValue_ShouldReturnToString()
    {
        // Arrange
        var converter = new EnumToDisplayNameConverter();
        var nonEnumValue = "test";

        // Act
        var result = converter.Convert(nonEnumValue, typeof(string), null, null);

        // Assert
        Assert.Equal("test", (string)result);
    }

    /// <summary>
    /// 测试EnumToDisplayNameConverter反向转换
    /// </summary>
    [Fact]
    public void EnumToDisplayNameConverter_ShouldConvertBackCorrectly()
    {
        // Arrange
        var converter = new EnumToDisplayNameConverter();

        // Act & Assert - 反向转换应该抛出NotImplementedException
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack("单页", typeof(object), null, null));
    }
}