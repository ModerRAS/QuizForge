using QuizForge.Core.ContentGeneration;
using QuizForge.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace QuizForge.Tests
{
    /// <summary>
    /// 动态内容插入器测试类
    /// </summary>
    public class DynamicContentInserterTests
    {
        private readonly DynamicContentInserter _dynamicContentInserter;

        public DynamicContentInserterTests()
        {
            _dynamicContentInserter = new DynamicContentInserter();
        }

        /// <summary>
        /// 测试生成页眉内容
        /// </summary>
        [Fact]
        public void GenerateHeaderContent_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    ExamTitle = "测试试卷",
                    Subject = "数学",
                    EnableOddEvenHeaderFooter = false
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyhead[C]", result);
            Assert.Contains("测试试卷", result);
            Assert.Contains("数学", result);
        }

        /// <summary>
        /// 测试生成页脚内容
        /// </summary>
        [Fact]
        public void GenerateFooterContent_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    ShowPageNumberInFooter = true,
                    PageNumberFormat = PageNumberFormat.Chinese,
                    PageNumberPosition = PageNumberPosition.Center
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateFooterContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("第1页/共1页", result);
        }

        /// <summary>
        /// 测试生成页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateHeaderFooterSetup_WithHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    EnableOddEvenHeaderFooter = false
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderFooterSetup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhf{}", result);
            Assert.Contains("\\renewcommand{\\headrulewidth}{0.4pt}", result);
            Assert.Contains("\\renewcommand{\\footrulewidth}{0.4pt}", result);
        }

        /// <summary>
        /// 测试插入动态内容
        /// </summary>
        [Fact]
        public void InsertDynamicContent_WithHeaderConfig_ShouldReplacePlaceholders()
        {
            // Arrange
            var templateContent = @"
{HEADER_FOOTER_SETUP}
{HEADER_CONTENT}
{FOOTER_CONTENT}
{EXAM_TITLE}
{SUBJECT}
{EXAM_TIME}
{TOTAL_POINTS}
";
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    ExamTitle = "测试试卷",
                    Subject = "数学",
                    ShowPageNumberInFooter = true,
                    PageNumberFormat = PageNumberFormat.Chinese,
                    PageNumberPosition = PageNumberPosition.Center
                }
            };
            var questions = new List<Question>
            {
                new Question { Id = Guid.NewGuid(), Text = "测试题目", Points = 10 }
            };

            // Act
            var result = _dynamicContentInserter.InsertDynamicContent(templateContent, template, questions);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhead[C]", result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("测试试卷", result);
            Assert.Contains("数学", result);
            Assert.Contains("120", result);
            Assert.Contains("10", result);
            Assert.DoesNotContain("{HEADER_FOOTER_SETUP}", result);
            Assert.DoesNotContain("{HEADER_CONTENT}", result);
            Assert.DoesNotContain("{FOOTER_CONTENT}", result);
            Assert.DoesNotContain("{EXAM_TITLE}", result);
            Assert.DoesNotContain("{SUBJECT}", result);
            Assert.DoesNotContain("{EXAM_TIME}", result);
            Assert.DoesNotContain("{TOTAL_POINTS}", result);
        }

        /// <summary>
        /// 测试插入多页动态内容
        /// </summary>
        [Fact]
        public void InsertMultiPageDynamicContent_WithHeaderConfig_ShouldReplacePlaceholders()
        {
            // Arrange
            var templateContent = @"
{HEADER_FOOTER_SETUP}
{HEADER_CONTENT}
{FOOTER_CONTENT}
{EXAM_TITLE}
{SUBJECT}
{EXAM_TIME}
{TOTAL_POINTS}
{CONTENT}
{ANSWER_SHEET_CONTENT}
{LAYOUT_ELEMENTS}
";
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    ExamTitle = "测试试卷",
                    Subject = "数学",
                    ShowPageNumberInFooter = true,
                    PageNumberFormat = PageNumberFormat.Chinese,
                    PageNumberPosition = PageNumberPosition.Center
                }
            };
            var questions = new List<Question>
            {
                new Question { Id = Guid.NewGuid(), Text = "测试题目1", Points = 10 },
                new Question { Id = Guid.NewGuid(), Text = "测试题目2", Points = 20 },
                new Question { Id = Guid.NewGuid(), Text = "测试题目3", Points = 30 }
            };

            // Act
            var result = _dynamicContentInserter.InsertMultiPageDynamicContent(templateContent, template, questions, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhead[C]", result);
            Assert.Contains("\\fancyfoot[C]", result);
            Assert.Contains("测试试卷", result);
            Assert.Contains("数学", result);
            Assert.Contains("120", result);
            Assert.Contains("60", result);
            Assert.Contains("测试题目1", result);
            Assert.Contains("测试题目2", result);
            Assert.Contains("测试题目3", result);
            Assert.DoesNotContain("{HEADER_FOOTER_SETUP}", result);
            Assert.DoesNotContain("{HEADER_CONTENT}", result);
            Assert.DoesNotContain("{FOOTER_CONTENT}", result);
            Assert.DoesNotContain("{EXAM_TITLE}", result);
            Assert.DoesNotContain("{SUBJECT}", result);
            Assert.DoesNotContain("{EXAM_TIME}", result);
            Assert.DoesNotContain("{TOTAL_POINTS}", result);
            Assert.DoesNotContain("{CONTENT}", result);
            Assert.DoesNotContain("{ANSWER_SHEET_CONTENT}", result);
            Assert.DoesNotContain("{LAYOUT_ELEMENTS}", result);
        }

        /// <summary>
        /// 测试生成奇偶页不同的页眉内容
        /// </summary>
        [Fact]
        public void GenerateHeaderContent_WithOddEvenHeaderFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    ExamTitle = "测试试卷",
                    Subject = "数学",
                    EnableOddEvenHeaderFooter = true,
                    OddPageHeaderContent = "奇数页页眉",
                    EvenPageHeaderContent = "偶数页页眉"
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyhead[O]", result);
            Assert.Contains("\\fancyhead[E]", result);
            Assert.Contains("奇数页页眉", result);
            Assert.Contains("偶数页页眉", result);
        }

        /// <summary>
        /// 测试生成奇偶页不同的页脚内容
        /// </summary>
        [Fact]
        public void GenerateFooterContent_WithOddEvenHeaderFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    EnableOddEvenHeaderFooter = true,
                    ShowPageNumberInFooter = true,
                    PageNumberFormat = PageNumberFormat.Chinese,
                    PageNumberPosition = PageNumberPosition.Center,
                    OddPageFooterContent = "奇数页页脚",
                    EvenPageFooterContent = "偶数页页脚"
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateFooterContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\fancyfoot[O]", result);
            Assert.Contains("\\fancyfoot[E]", result);
            Assert.Contains("奇数页页脚", result);
            Assert.Contains("偶数页页脚", result);
        }

        /// <summary>
        /// 测试生成奇偶页不同的页眉页脚设置
        /// </summary>
        [Fact]
        public void GenerateHeaderFooterSetup_WithOddEvenHeaderFooter_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = new HeaderConfig
                {
                    EnableOddEvenHeaderFooter = true
                }
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderFooterSetup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhf{}", result);
            Assert.Contains("\\renewcommand{\\headrulewidth}{0.4pt}", result);
            Assert.Contains("\\renewcommand{\\footrulewidth}{0.4pt}", result);
        }

        /// <summary>
        /// 测试向后兼容性（不使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateHeaderContent_WithoutHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderContent = "自定义页眉内容",
                HeaderConfig = null
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("自定义页眉内容", result);
        }

        /// <summary>
        /// 测试向后兼容性（不使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateFooterContent_WithoutHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                FooterContent = "自定义页脚内容",
                HeaderConfig = null
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateFooterContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("自定义页脚内容", result);
        }

        /// <summary>
        /// 测试向后兼容性（不使用HeaderConfig）
        /// </summary>
        [Fact]
        public void GenerateHeaderFooterSetup_WithoutHeaderConfig_ShouldReturnValidLaTeXCode()
        {
            // Arrange
            var template = new ExamTemplate
            {
                Name = "测试试卷",
                Description = "数学",
                HeaderConfig = null
            };
            var questions = new List<Question>();

            // Act
            var result = _dynamicContentInserter.GetType()
                .GetMethod("GenerateHeaderFooterSetup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dynamicContentInserter, new object[] { template, questions }) as string;

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\\pagestyle{fancy}", result);
            Assert.Contains("\\fancyhf{}", result);
            Assert.Contains("\\renewcommand{\\headrulewidth}{0.4pt}", result);
            Assert.Contains("\\renewcommand{\\footrulewidth}{0.4pt}", result);
        }
    }
}