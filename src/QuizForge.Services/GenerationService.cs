using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Core.ContentGeneration;
using QuizForge.Core.Interfaces;
using QuizForge.Infrastructure.Engines;
using Microsoft.Extensions.Logging;

namespace QuizForge.Services;

/// <summary>
/// 生成服务实现，提供试卷生成的各种功能
/// </summary>
public class GenerationService : IGenerationService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IQuestionProcessor _questionProcessor;
    private readonly ExamPaperGenerator _examPaperGenerator;

    public GenerationService(
        IQuestionRepository questionRepository,
        ITemplateRepository templateRepository,
        IQuestionProcessor questionProcessor,
        ExamPaperGenerator examPaperGenerator)
    {
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _questionProcessor = questionProcessor ?? throw new ArgumentNullException(nameof(questionProcessor));
        _examPaperGenerator = examPaperGenerator ?? throw new ArgumentNullException(nameof(examPaperGenerator));
    }

    /// <summary>
    /// 生成试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="options">试卷选项</param>
    /// <returns>生成的试卷</returns>
    public async Task<ExamPaper> GenerateExamPaperAsync(Guid templateId, Guid questionBankId, ExamPaperOptions options)
    {
        return await _examPaperGenerator.GenerateExamPaperAsync(templateId, questionBankId, options);
    }

    /// <summary>
    /// 生成LaTeX内容
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>LaTeX内容</returns>
    public async Task<string> GenerateLaTeXContentAsync(ExamPaper examPaper)
    {
        if (examPaper == null)
        {
            throw new ArgumentNullException(nameof(examPaper));
        }

        // 获取模板
        var template = await _templateRepository.GetByIdAsync(examPaper.TemplateId);
        if (template == null)
        {
            throw new InvalidOperationException($"找不到ID为 {examPaper.TemplateId} 的模板");
        }

        // 创建HeaderConfig
        var headerConfig = new HeaderConfig
        {
            ExamTitle = examPaper.Title,
            Subject = "考试科目",
            ExamTime = 120,
            SchoolName = "",
            Class = "",
            ExamDate = examPaper.CreatedAt.ToString("yyyy-MM-dd")
        };
        
        // 生成LaTeX内容
        return await _examPaperGenerator.GenerateLaTeXContentAsync(template, examPaper.Questions, headerConfig);
    }

    /// <summary>
    /// 生成预览
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <returns>预览流</returns>
    public async Task<Stream> GeneratePreviewAsync(string latexContent)
    {
        if (string.IsNullOrWhiteSpace(latexContent))
        {
            throw new ArgumentException("LaTeX内容不能为空", nameof(latexContent));
        }

        // 使用PDF引擎生成PDF
        var pdfEngine = new LatexPdfEngine(new LoggerFactory().CreateLogger<PdfEngine>());
        
        // 创建临时PDF文件路径
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"QuizForge_Preview_{Guid.NewGuid()}.pdf");
        
        try
        {
            // 生成PDF
            var success = await pdfEngine.GenerateFromLatexAsync(latexContent, tempPdfPath);
            
            if (!success)
            {
                throw new InvalidOperationException("PDF生成失败");
            }
            
            // 读取PDF文件
            var pdfBytes = await File.ReadAllBytesAsync(tempPdfPath);
            return new MemoryStream(pdfBytes);
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempPdfPath))
            {
                File.Delete(tempPdfPath);
            }
        }
    }

    /// <summary>
    /// 验证试卷
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateExamPaperAsync(ExamPaper examPaper)
    {
        if (examPaper == null)
        {
            return false;
        }

        // 验证基本信息
        if (string.IsNullOrWhiteSpace(examPaper.Title) || 
            examPaper.TemplateId == Guid.Empty || 
            examPaper.Questions == null || 
            examPaper.Questions.Count == 0)
        {
            return false;
        }

        // 验证模板
        var template = await _templateRepository.GetByIdAsync(examPaper.TemplateId);
        if (template == null)
        {
            return false;
        }

        // 验证题目
        foreach (var question in examPaper.Questions)
        {
            if (!ValidateQuestion(question))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 重新生成试卷
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>重新生成的试卷</returns>
    public async Task<ExamPaper> RegenerateExamPaperAsync(ExamPaper examPaper)
    {
        if (examPaper == null)
        {
            throw new ArgumentNullException(nameof(examPaper));
        }

        return await _examPaperGenerator.RegenerateExamPaperAsync(examPaper);
    }

    /// <summary>
    /// 随机生成试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="options">试卷选项</param>
    /// <returns>随机生成的试卷</returns>
    public async Task<ExamPaper> GenerateRandomExamPaperAsync(Guid templateId, Guid questionBankId, ExamPaperOptions options)
    {
        return await _examPaperGenerator.GenerateRandomExamPaperAsync(templateId, questionBankId, options);
    }

    /// <summary>
    /// 手动生成试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionIds">题目ID列表</param>
    /// <param name="options">试卷选项</param>
    /// <returns>手动生成的试卷</returns>
    public async Task<ExamPaper> GenerateManualExamPaperAsync(Guid templateId, List<Guid> questionIds, ExamPaperOptions options)
    {
        return await _examPaperGenerator.GenerateManualExamPaperAsync(templateId, questionIds, options);
    }

    /// <summary>
    /// 生成多页试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="options">试卷选项</param>
    /// <param name="questionsPerPage">每页题目数量</param>
    /// <returns>多页试卷</returns>
    public async Task<ExamPaper> GenerateMultiPageExamPaperAsync(Guid templateId, Guid questionBankId, ExamPaperOptions options, int questionsPerPage = 5)
    {
        if (templateId == Guid.Empty)
        {
            throw new ArgumentException("模板ID不能为空", nameof(templateId));
        }

        if (questionBankId == Guid.Empty)
        {
            throw new ArgumentException("题库ID不能为空", nameof(questionBankId));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // 获取模板
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new InvalidOperationException($"找不到ID为 {templateId} 的模板");
        }

        // 获取题库
        var questionBank = await _questionRepository.GetByIdAsync(questionBankId);
        if (questionBank == null)
        {
            throw new InvalidOperationException($"找不到ID为 {questionBankId} 的题库");
        }

        // 处理题库数据
        questionBank = _questionProcessor.ProcessQuestionBank(questionBank);

        // 选择题目
        var selectedQuestions = SelectQuestions(questionBank, options);
        if (selectedQuestions.Count == 0)
        {
            throw new InvalidOperationException("没有选择到任何题目");
        }

        // 组织题目到模板章节
        OrganizeQuestionsToSections(template, selectedQuestions);

        // 生成多页试卷内容
        var latexContent = await _examPaperGenerator.GenerateMultiPageLaTeXContentAsync(template, selectedQuestions, questionsPerPage);

        // 创建试卷对象
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = options.Title,
            TemplateId = templateId,
            QuestionBankId = questionBankId,
            CreatedAt = DateTime.UtcNow,
            Content = latexContent,
            TotalPoints = selectedQuestions.Sum(q => q.Points),
            Questions = selectedQuestions
        };

        return examPaper;
    }

    /// <summary>
    /// 批量生成试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="options">试卷选项</param>
    /// <param name="count">生成数量</param>
    /// <returns>试卷列表</returns>
    public async Task<List<ExamPaper>> BatchGenerateExamPapersAsync(Guid templateId, Guid questionBankId, ExamPaperOptions options, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("生成数量必须大于0", nameof(count));
        }

        var examPapers = new List<ExamPaper>();

        for (int i = 0; i < count; i++)
        {
            // 为每份试卷创建唯一的标题
            var paperOptions = new ExamPaperOptions
            {
                Title = $"{options.Title} - {i + 1}",
                RandomQuestions = options.RandomQuestions,
                QuestionCount = options.QuestionCount,
                IncludeAnswers = options.IncludeAnswers,
                DifficultyDistribution = options.DifficultyDistribution,
                CategoryDistribution = options.CategoryDistribution
            };

            var examPaper = await GenerateExamPaperAsync(templateId, questionBankId, paperOptions);
            examPapers.Add(examPaper);
        }

        return examPapers;
    }

    /// <summary>
    /// 选择题目
    /// </summary>
    /// <param name="questionBank">题库</param>
    /// <param name="options">试卷选项</param>
    /// <returns>选中的题目列表</returns>
    private List<Question> SelectQuestions(QuestionBank questionBank, ExamPaperOptions options)
    {
        if (questionBank == null || questionBank.Questions == null || questionBank.Questions.Count == 0)
        {
            return new List<Question>();
        }

        var selectedQuestions = new List<Question>();

        // 如果有难度分布要求
        if (options.DifficultyDistribution != null && options.DifficultyDistribution.Count > 0)
        {
            foreach (var distribution in options.DifficultyDistribution)
            {
                var difficulty = distribution.Key;
                var count = distribution.Value;

                var difficultyQuestions = _questionProcessor.GetQuestionsByDifficulty(difficulty, questionBank);
                var questions = _questionProcessor.GetRandomQuestions(count, questionBank, difficulty: difficulty);
                selectedQuestions.AddRange(questions);
            }
        }
        // 如果有类别分布要求
        else if (options.CategoryDistribution != null && options.CategoryDistribution.Count > 0)
        {
            foreach (var distribution in options.CategoryDistribution)
            {
                var category = distribution.Key;
                var count = distribution.Value;

                var categoryQuestions = _questionProcessor.GetQuestionsByCategory(category, questionBank);
                var questions = _questionProcessor.GetRandomQuestions(count, questionBank, category: category);
                selectedQuestions.AddRange(questions);
            }
        }
        // 否则随机选择指定数量的题目
        else
        {
            selectedQuestions = _questionProcessor.GetRandomQuestions(options.QuestionCount, questionBank);
        }

        return selectedQuestions;
    }

    /// <summary>
    /// 组织题目到模板章节
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    private void OrganizeQuestionsToSections(ExamTemplate template, List<Question> questions)
    {
        if (template == null || template.Sections == null || template.Sections.Count == 0)
        {
            return;
        }

        // 清空章节的题目ID列表
        foreach (var section in template.Sections)
        {
            section.QuestionIds.Clear();
        }

        // 如果题目数量小于等于章节要求的题目数量，按顺序分配
        if (questions.Count <= template.Sections.Sum(s => s.QuestionCount))
        {
            var questionIndex = 0;
            foreach (var section in template.Sections)
            {
                for (int i = 0; i < section.QuestionCount && questionIndex < questions.Count; i++)
                {
                    section.QuestionIds.Add(questions[questionIndex].Id);
                    questionIndex++;
                }
            }
        }
        // 否则平均分配题目到各章节
        else
        {
            var questionsPerSection = (int)Math.Ceiling((double)questions.Count / template.Sections.Count);
            var questionIndex = 0;

            foreach (var section in template.Sections)
            {
                for (int i = 0; i < questionsPerSection && questionIndex < questions.Count; i++)
                {
                    section.QuestionIds.Add(questions[questionIndex].Id);
                    questionIndex++;
                }
            }
        }
    }

    /// <summary>
    /// 验证单个题目
    /// </summary>
    /// <param name="question">题目</param>
    /// <returns>验证结果</returns>
    private bool ValidateQuestion(Question question)
    {
        if (question == null)
        {
            return false;
        }

        // 验证基本信息
        if (string.IsNullOrWhiteSpace(question.Type) ||
            string.IsNullOrWhiteSpace(question.Content) ||
            string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            return false;
        }

        // 如果是选择题，验证选项
        if (question.Type == "选择题")
        {
            if (question.Options == null || question.Options.Count < 2)
            {
                return false;
            }

            // 验证是否有正确答案
            var hasCorrectAnswer = question.Options.Any(o => o.IsCorrect) ||
                                  question.Options.Any(o => o.Key.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
            
            if (!hasCorrectAnswer)
            {
                return false;
            }
        }

        return true;
    }
}