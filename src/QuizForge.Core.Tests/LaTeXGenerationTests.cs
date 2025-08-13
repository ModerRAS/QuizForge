using QuizForge.Core.ContentGeneration;
using QuizForge.Core.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace QuizForge.Core.Tests
{
    /// <summary>
    /// LaTeX生成功能测试类
    /// </summary>
    public class LaTeXGenerationTests
    {
        private readonly ContentGenerator _contentGenerator;
        private readonly LaTeXGenerationConfig _config;

        public LaTeXGenerationTests()
        {
            // 初始化配置
            _config = new LaTeXGenerationConfig
            {
                DocumentClass = "article",
                FontSize = "12pt",
                Margin = "1in",
                IncludeCtex = true,
                IncludeAmsMath = true,
                IncludeAmsFonts = true,
                IncludeAmssymb = true,
                QuestionNumberFormat = "题目 {0}",
                ShowPoints = true,
                ShowDifficulty = true,
                ShowCategory = true,
                SectionCommand = "section*",
                BlankLineLength = "2cm",
                EssayAnswerHeight = "5cm",
                AnswerSheetChoiceWidth = "2cm",
                AnswerSheetBlankWidth = "5cm",
                AnswerSheetEssayHeight = "5cm"
            };

            // 初始化内容生成器
            _contentGenerator = new ContentGenerator(_config);
        }

        /// <summary>
        /// 测试选择题LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateMultipleChoiceQuestion()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 1,
                Type = "选择题",
                Content = "下列哪个是正确的？",
                Points = 5,
                Difficulty = "简单",
                Category = "数学",
                Options = new List<Option>
                {
                    new Option { Id = "A", Value = "选项A" },
                    new Option { Id = "B", Value = "选项B" },
                    new Option { Id = "C", Value = "选项C" },
                    new Option { Id = "D", Value = "选项D" }
                },
                CorrectAnswer = "A"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateMultipleChoiceQuestion(question, 1);

            // 验证结果
            Assert.Contains("\\textbf{题目 1}", latexContent);
            Assert.Contains("(5分)", latexContent);
            Assert.Contains("[简单]", latexContent);
            Assert.Contains("[数学]", latexContent);
            Assert.Contains("下列哪个是正确的？", latexContent);
            Assert.Contains("\\begin{enumerate}", latexContent);
            Assert.Contains("\\item 选项A", latexContent);
            Assert.Contains("\\item 选项B", latexContent);
            Assert.Contains("\\item 选项C", latexContent);
            Assert.Contains("\\item 选项D", latexContent);
            Assert.Contains("\\end{enumerate}", latexContent);
        }

        /// <summary>
        /// 测试多选题LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateMultipleChoiceQuestion_MultipleAnswers()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 2,
                Type = "选择题",
                Content = "下列哪些是正确的？",
                Points = 10,
                Difficulty = "中等",
                Category = "物理",
                Options = new List<Option>
                {
                    new Option { Id = "A", Value = "选项A" },
                    new Option { Id = "B", Value = "选项B" },
                    new Option { Id = "C", Value = "选项C" },
                    new Option { Id = "D", Value = "选项D" }
                },
                CorrectAnswer = "A,B,C"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateMultipleChoiceQuestion(question, 2);

            // 验证结果
            Assert.Contains("\\textbf{题目 2}", latexContent);
            Assert.Contains("(10分)", latexContent);
            Assert.Contains("[中等]", latexContent);
            Assert.Contains("[物理]", latexContent);
            Assert.Contains("下列哪些是正确的？", latexContent);
            Assert.Contains("\\begin{enumerate}", latexContent);
            Assert.Contains("\\item 选项A", latexContent);
            Assert.Contains("\\item 选项B", latexContent);
            Assert.Contains("\\item 选项C", latexContent);
            Assert.Contains("\\item 选项D", latexContent);
            Assert.Contains("\\end{enumerate}", latexContent);
        }

        /// <summary>
        /// 测试填空题LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateFillInBlankQuestion()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 3,
                Type = "填空题",
                Content = "中国的首都是____。",
                Points = 5,
                Difficulty = "简单",
                Category = "地理",
                CorrectAnswer = "北京"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateFillInBlankQuestion(question, 3);

            // 验证结果
            Assert.Contains("\\textbf{题目 3}", latexContent);
            Assert.Contains("(5分)", latexContent);
            Assert.Contains("[简单]", latexContent);
            Assert.Contains("[地理]", latexContent);
            Assert.Contains("中国的首都是", latexContent);
            Assert.Contains("\\underline{\\hspace{2cm}}", latexContent);
            Assert.Contains("。", latexContent);
        }

        /// <summary>
        /// 测试简答题LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateEssayQuestion()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 4,
                Type = "简答题",
                Content = "请简述中国的历史发展。",
                Points = 20,
                Difficulty = "困难",
                Category = "历史",
                CorrectAnswer = "中国有着悠久的历史..."
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateEssayQuestion(question, 4);

            // 验证结果
            Assert.Contains("\\textbf{题目 4}", latexContent);
            Assert.Contains("(20分)", latexContent);
            Assert.Contains("[困难]", latexContent);
            Assert.Contains("[历史]", latexContent);
            Assert.Contains("请简述中国的历史发展。", latexContent);
            Assert.Contains("\\vspace{5cm}", latexContent);
        }

        /// <summary>
        /// 测试判断题LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateTrueFalseQuestion()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 5,
                Type = "判断题",
                Content = "地球是圆的。",
                Points = 5,
                Difficulty = "简单",
                Category = "地理",
                CorrectAnswer = "正确"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateTrueFalseQuestion(question, 5);

            // 验证结果
            Assert.Contains("\\textbf{题目 5}", latexContent);
            Assert.Contains("(5分)", latexContent);
            Assert.Contains("[简单]", latexContent);
            Assert.Contains("[地理]", latexContent);
            Assert.Contains("地球是圆的。", latexContent);
            Assert.Contains("\\begin{enumerate}", latexContent);
            Assert.Contains("\\item 正确", latexContent);
            Assert.Contains("\\item 错误", latexContent);
            Assert.Contains("\\end{enumerate}", latexContent);
        }

        /// <summary>
        /// 测试通用题目LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateGenericQuestion()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 6,
                Type = "匹配题",
                Content = "请匹配下列项目。",
                Points = 10,
                Difficulty = "中等",
                Category = "英语",
                Options = new List<Option>
                {
                    new Option { Id = "A", Value = "Apple" },
                    new Option { Id = "B", Value = "Banana" }
                },
                CorrectAnswer = "A-苹果, B-香蕉"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateQuestionContent(question, 6);

            // 验证结果
            Assert.Contains("\\textbf{题目 6}", latexContent);
            Assert.Contains("(10分)", latexContent);
            Assert.Contains("[中等]", latexContent);
            Assert.Contains("[英语]", latexContent);
            Assert.Contains("请匹配下列项目。", latexContent);
            Assert.Contains("\\begin{enumerate}", latexContent);
            Assert.Contains("\\item Apple", latexContent);
            Assert.Contains("\\item Banana", latexContent);
            Assert.Contains("\\end{enumerate}", latexContent);
        }

        /// <summary>
        /// 测试答题卡LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateAnswerSheetContent()
        {
            // 准备测试数据 - 选择题
            var mcQuestion = new Question
            {
                Id = 7,
                Type = "选择题",
                Content = "下列哪个是正确的？",
                Points = 5,
                CorrectAnswer = "A"
            };

            // 生成LaTeX内容
            var mcLatexContent = _contentGenerator.GenerateAnswerSheetContent(mcQuestion, 7);

            // 验证结果
            Assert.Contains("\\textbf{题目 7}", mcLatexContent);
            Assert.Contains("(5分)", mcLatexContent);
            Assert.Contains("\\underline{\\hspace{2cm}}", mcLatexContent);

            // 准备测试数据 - 填空题
            var fbQuestion = new Question
            {
                Id = 8,
                Type = "填空题",
                Content = "中国的首都是____。",
                Points = 5,
                CorrectAnswer = "北京"
            };

            // 生成LaTeX内容
            var fbLatexContent = _contentGenerator.GenerateAnswerSheetContent(fbQuestion, 8);

            // 验证结果
            Assert.Contains("\\textbf{题目 8}", fbLatexContent);
            Assert.Contains("(5分)", fbLatexContent);
            Assert.Contains("\\underline{\\hspace{5cm}}", fbLatexContent);

            // 准备测试数据 - 简答题
            var essayQuestion = new Question
            {
                Id = 9,
                Type = "简答题",
                Content = "请简述中国的历史发展。",
                Points = 20,
                CorrectAnswer = "中国有着悠久的历史..."
            };

            // 生成LaTeX内容
            var essayLatexContent = _contentGenerator.GenerateAnswerSheetContent(essayQuestion, 9);

            // 验证结果
            Assert.Contains("\\textbf{题目 9}", essayLatexContent);
            Assert.Contains("(20分)", essayLatexContent);
            Assert.Contains("\\vspace{5cm}", essayLatexContent);
        }

        /// <summary>
        /// 测试章节内容LaTeX生成
        /// </summary>
        [Fact]
        public void TestGenerateSectionContent()
        {
            // 准备测试数据
            var section = new TemplateSection
            {
                Title = "第一部分：选择题",
                Instructions = "请从下列选项中选择最合适的答案。"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateSectionContent(section);

            // 验证结果
            Assert.Contains("\\section*{第一部分：选择题}", latexContent);
            Assert.Contains("\\textbf{请从下列选项中选择最合适的答案。}", latexContent);
        }

        /// <summary>
        /// 测试数学公式处理
        /// </summary>
        [Fact]
        public void TestMathFormulaProcessing()
        {
            // 准备测试数据
            var question = new Question
            {
                Id = 10,
                Type = "选择题",
                Content = "计算 $x^2 + y^2 = z^2$ 的值。",
                Points = 10,
                Difficulty = "中等",
                Category = "数学",
                Options = new List<Option>
                {
                    new Option { Id = "A", Value = "$x=3, y=4, z=5$" },
                    new Option { Id = "B", Value = "$x=1, y=2, z=3$" }
                },
                CorrectAnswer = "A"
            };

            // 生成LaTeX内容
            var latexContent = _contentGenerator.GenerateMultipleChoiceQuestion(question, 10);

            // 验证结果
            Assert.Contains("\\textbf{题目 10}", latexContent);
            Assert.Contains("(10分)", latexContent);
            Assert.Contains("[中等]", latexContent);
            Assert.Contains("[数学]", latexContent);
            Assert.Contains("计算 $x^2 + y^2 = z^2$ 的值。", latexContent);
            Assert.Contains("\\begin{enumerate}", latexContent);
            Assert.Contains("\\item $x=3, y=4, z=5$", latexContent);
            Assert.Contains("\\item $x=1, y=2, z=3$", latexContent);
            Assert.Contains("\\end{enumerate}", latexContent);
        }

        /// <summary>
        /// 测试空值异常处理
        /// </summary>
        [Fact]
        public void TestNullQuestionException()
        {
            // 验证空值异常
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateMultipleChoiceQuestion(null, 1));
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateFillInBlankQuestion(null, 1));
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateEssayQuestion(null, 1));
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateTrueFalseQuestion(null, 1));
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateQuestionContent(null, 1));
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateAnswerSheetContent(null, 1));
        }

        /// <summary>
        /// 测试空章节异常处理
        /// </summary>
        [Fact]
        public void TestNullSectionException()
        {
            // 验证空值异常
            Assert.Throws<ArgumentNullException>(() => _contentGenerator.GenerateSectionContent(null));
        }
    }
}