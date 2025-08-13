using QuizForge.Core.Layout;
using QuizForge.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace QuizForge.Tests
{
    /// <summary>
    /// 页眉页脚布局测试类
    /// </summary>
    public class HeaderFooterLayoutTests
    {
        private readonly HeaderFooterLayout _headerFooterLayout;

        public HeaderFooterLayoutTests()
        {
            _headerFooterLayout = new HeaderFooterLayout();
        }

        /// <summary>
        /// 测试生成页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateHeaderFooterSetup_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var expectedLaTeXCommands = new[] { "\\pagestyle{fancy}", "\\fancyhf{}", "\\renewcommand{\\headrulewidth}{0.4pt}", "\\renewcommand{\\footrulewidth}{0.4pt}" };

            // Act
            var result = _headerFooterLayout.GenerateHeaderFooterSetup();

            // Assert
            Assert.NotNull(result);
            foreach (var command in expectedLaTeXCommands)
            {
                Assert.Contains(command, result);
            }
        }

        /// <summary>
        /// 测试生成支持奇偶页不同的页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateOddEvenHeaderFooterSetup_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var expectedLaTeXCommands = new[] { "\\pagestyle{fancy}", "\\fancyhf{}", "\\renewcommand{\\headrulewidth}{0.4pt}", "\\renewcommand{\\footrulewidth}{0.4pt}" };

            // Act
            var result = _headerFooterLayout.GenerateOddEvenHeaderFooterSetup();

            // Assert
            Assert.NotNull(result);
            foreach (var command in expectedLaTeXCommands)
            {
                Assert.Contains(command, result);
            }
        }

        /// <summary>
        /// 测试生成页眉内容（使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateHeader_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var headerConfig = new HeaderConfig
            {
                ExamTitle = "测试试卷",
                Subject = "数学",
                EnableOddEvenHeaderFooter = false
            };

            // Act
            var result = _headerFooterLayout.GenerateHeader(headerConfig, "测试试卷", "数学", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyhead[C]", result);
            Assert.Contains("测试试卷", result);
            Assert.Contains("数学", result);
        }

        /// <summary>
        /// 测试生成奇偶页不同的页眉内容
        /// </summary>
        [Fact]
        public void GenerateHeader_WithOddEvenHeaderFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var headerConfig = new HeaderConfig
            {
                ExamTitle = "测试试卷",
                Subject = "数学",
                EnableOddEvenHeaderFooter = true,
                OddPageHeaderContent = "奇数页页眉",
                EvenPageHeaderContent = "偶数页页眉"
            };

            // Act
            var result = _headerFooterLayout.GenerateHeader(headerConfig, "测试试卷", "数学", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyhead[O]", result);
            Assert.Contains("\\fancyhead[E]", result);
            Assert.Contains("奇数页页眉", result);
            Assert.Contains("偶数页页眉", result);
        }

        /// <summary>
        /// 测试生成页脚内容（使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateFooter_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var headerConfig = new HeaderConfig
            {
                ShowPageNumberInFooter = true,
                PageNumberFormat = PageNumberFormat.Chinese,
                PageNumberPosition = PageNumberPosition.Center
            };

            // Act
            var result = _headerFooterLayout.GenerateFooter(headerConfig, 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("第1页/共5页", result);
        }

        /// <summary>
        /// 测试生成奇偶页不同的页脚内容
        /// </summary>
        [Fact]
        public void GenerateFooter_WithOddEvenHeaderFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var headerConfig = new HeaderConfig
            {
                EnableOddEvenHeaderFooter = true,
                ShowPageNumberInFooter = true,
                PageNumberFormat = PageNumberFormat.Chinese,
                PageNumberPosition = PageNumberPosition.Center,
                OddPageFooterContent = "奇数页页脚",
                EvenPageFooterContent = "偶数页页脚"
            };

            // Act
            var result = _headerFooterLayout.GenerateFooter(headerConfig, 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyfoot[O]", result);
            Assert.Contains("\\fancyfoot[E]", result);
            Assert.Contains("奇数页页脚", result);
            Assert.Contains("偶数页页脚", result);
        }

        /// <summary>
        /// 测试生成页眉页脚（使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateHeaderFooter_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var headerConfig = new HeaderConfig
            {
                ExamTitle = "测试试卷",
                Subject = "数学",
                ShowPageNumberInFooter = true,
                PageNumberFormat = PageNumberFormat.Chinese,
                PageNumberPosition = PageNumberPosition.Center
            };

            // Act
            var result = _headerFooterLayout.GenerateHeaderFooter(headerConfig, "测试试卷", "数学", 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhead[C]", result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("测试试卷", result);
            Assert.Contains("数学", result);
            Assert.Contains("第1页/共5页", result);
        }

        /// <summary>
        /// 测试生成首页页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateFirstPageHeaderFooterSetup_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var expectedLaTeXCommands = new[] { "\\thispagestyle{fancy}", "\\fancyhf{}", "\\renewcommand{\\headrulewidth}{0.4pt}", "\\renewcommand{\\footrulewidth}{0.4pt}" };

            // Act
            var result = _headerFooterLayout.GenerateFirstPageHeaderFooterSetup();

            // Assert
            Assert.NotNull(result);
            foreach (var command in expectedLaTeXCommands)
            {
                Assert.Contains(command, result);
            }
        }

        /// <summary>
        /// 测试生成首页页脚内容
        /// </summary>
        [Fact]
        public void GenerateFirstPageFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                FooterContent = ""
            };
            var totalPages = 5;

            // Act
            var result = _headerFooterLayout.GenerateFirstPageFooter(template, totalPages);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("第1页/共5页", result);
        }

        /// <summary>
        /// 测试生成空白页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateBlankHeaderFooterSetup_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var expectedLaTeXCommand = "\\thispagestyle{plain}";

            // Act
            var result = _headerFooterLayout.GenerateBlankHeaderFooterSetup();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(expectedLaTeXCommand, result);
        }

        /// <summary>
        /// 测试页码格式为中文
        /// </summary>
        [Fact]
        public void GetPageNumberText_WithChineseFormat_ShouldReturnCorrectText()
        {
            // Arrange
            var format = PageNumberFormat.Chinese;
            var pageNumber = 1;
            var totalPages = 5;
            var expected = "第1页/共5页";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { format, pageNumber, totalPages }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码格式为数字
        /// </summary>
        [Fact]
        public void GetPageNumberText_WithNumericFormat_ShouldReturnCorrectText()
        {
            // Arrange
            var format = PageNumberFormat.Numeric;
            var pageNumber = 1;
            var totalPages = 5;
            var expected = "1/5";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { format, pageNumber, totalPages }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码格式为英文
        /// </summary>
        [Fact]
        public void GetPageNumberText_WithEnglishFormat_ShouldReturnCorrectText()
        {
            // Arrange
            var format = PageNumberFormat.English;
            var pageNumber = 1;
            var totalPages = 5;
            var expected = "Page 1 of 5";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { format, pageNumber, totalPages }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码位置为左侧
        /// </summary>
        [Fact]
        public void GetPageNumberPositionCommand_WithLeftPosition_ShouldReturnCorrectCommand()
        {
            // Arrange
            var position = PageNumberPosition.Left;
            var expected = "L";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberPositionCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { position }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码位置为居中
        /// </summary>
        [Fact]
        public void GetPageNumberPositionCommand_WithCenterPosition_ShouldReturnCorrectCommand()
        {
            // Arrange
            var position = PageNumberPosition.Center;
            var expected = "C";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberPositionCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { position }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码位置为右侧
        /// </summary>
        [Fact]
        public void GetPageNumberPositionCommand_WithRightPosition_ShouldReturnCorrectCommand()
        {
            // Arrange
            var position = PageNumberPosition.Right;
            var expected = "R";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberPositionCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { position }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码位置为外侧
        /// </summary>
        [Fact]
        public void GetPageNumberPositionCommand_WithOutsidePosition_ShouldReturnCorrectCommand()
        {
            // Arrange
            var position = PageNumberPosition.Outside;
            var expected = "O";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberPositionCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { position }) as string;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试页码位置为内侧
        /// </summary>
        [Fact]
        public void GetPageNumberPositionCommand_WithInsidePosition_ShouldReturnCorrectCommand()
        {
            // Arrange
            var position = PageNumberPosition.Inside;
            var expected = "I";

            // Act
            var result = _headerFooterLayout.GetType()
                .GetMethod("GetPageNumberPositionCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_headerFooterLayout, new object[] { position }) as string;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}