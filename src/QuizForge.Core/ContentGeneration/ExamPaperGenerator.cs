using QuizForge.Models;
using QuizForge.Core.Interfaces;
using QuizForge.Models.Interfaces;
using System.Text;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// 试卷生成器，整合题库解析、内容生成和模板处理
/// </summary>
public class ExamPaperGenerator
{
    private readonly IQuestionProcessor _questionProcessor;
    private readonly IQuestionRepository _questionRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ContentGenerator _contentGenerator;
    private readonly DynamicContentInserter _dynamicContentInserter;

    public ExamPaperGenerator(
        IQuestionProcessor questionProcessor,
        IQuestionRepository questionRepository,
        ITemplateRepository templateRepository,
        ContentGenerator contentGenerator,
        DynamicContentInserter dynamicContentInserter)
    {
        _questionProcessor = questionProcessor ?? throw new ArgumentNullException(nameof(questionProcessor));
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _contentGenerator = contentGenerator ?? throw new ArgumentNullException(nameof(contentGenerator));
        _dynamicContentInserter = dynamicContentInserter ?? throw new ArgumentNullException(nameof(dynamicContentInserter));
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

        // 生成试卷内容
        var latexContent = await GenerateLaTeXContentAsync(template, selectedQuestions);

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
    /// 生成LaTeX内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <returns>LaTeX内容</returns>
    public async Task<string> GenerateLaTeXContentAsync(ExamTemplate template, List<Question> questions)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (questions == null)
        {
            throw new ArgumentNullException(nameof(questions));
        }

        // 获取模板文件内容
        var templateContent = await GetTemplateContentAsync(template);
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new InvalidOperationException("无法获取模板内容");
        }

        // 插入动态内容
        var latexContent = _dynamicContentInserter.InsertDynamicContent(templateContent, template, questions);

        return latexContent;
    }

    /// <summary>
    /// 生成多页试卷的LaTeX内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="questions">题目列表</param>
    /// <param name="questionsPerPage">每页题目数量</param>
    /// <returns>多页试卷的LaTeX内容</returns>
    public async Task<string> GenerateMultiPageLaTeXContentAsync(ExamTemplate template, List<Question> questions, int questionsPerPage = 5)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (questions == null)
        {
            throw new ArgumentNullException(nameof(questions));
        }

        // 获取模板文件内容
        var templateContent = await GetTemplateContentAsync(template);
        if (string.IsNullOrWhiteSpace(templateContent))
        {
            throw new InvalidOperationException("无法获取模板内容");
        }

        // 插入动态内容
        var latexContent = _dynamicContentInserter.InsertMultiPageDynamicContent(templateContent, template, questions, questionsPerPage);

        return latexContent;
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
        // 设置随机选项
        options.RandomQuestions = true;
        
        // 生成试卷
        return await GenerateExamPaperAsync(templateId, questionBankId, options);
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
        if (templateId == Guid.Empty)
        {
            throw new ArgumentException("模板ID不能为空", nameof(templateId));
        }

        if (questionIds == null || questionIds.Count == 0)
        {
            throw new ArgumentException("题目ID列表不能为空", nameof(questionIds));
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

        // 获取题目
        var questions = await GetQuestionsByIdsAsync(questionIds);

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("没有找到任何题目");
        }

        // 组织题目到模板章节
        OrganizeQuestionsToSections(template, questions);

        // 生成试卷内容
        var latexContent = await GenerateLaTeXContentAsync(template, questions);

        // 创建试卷对象
        var examPaper = new ExamPaper
        {
            Id = Guid.NewGuid(),
            Title = options.Title,
            TemplateId = templateId,
            QuestionBankId = Guid.Empty, // 手动选题没有特定的题库
            CreatedAt = DateTime.UtcNow,
            Content = latexContent,
            TotalPoints = questions.Sum(q => q.Points),
            Questions = questions
        };

        return examPaper;
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

        // 创建选项
        var options = new ExamPaperOptions
        {
            Title = examPaper.Title,
            RandomQuestions = true,
            QuestionCount = examPaper.Questions.Count,
            IncludeAnswers = false
        };

        // 重新生成试卷
        return await GenerateExamPaperAsync(examPaper.TemplateId, examPaper.QuestionBankId, options);
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
    /// 获取模板内容
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <returns>模板内容</returns>
    private async Task<string> GetTemplateContentAsync(ExamTemplate template)
    {
        // 根据模板样式获取模板文件路径
        var templatePath = template.Style switch
        {
            TemplateStyle.Basic => "src/QuizForge.Infrastructure/Templates/BasicExamTemplate.tex",
            TemplateStyle.Advanced => "src/QuizForge.Infrastructure/Templates/AdvancedExamTemplate.tex",
            _ => throw new NotSupportedException($"不支持的模板样式: {template.Style}")
        };

        try
        {
            return await File.ReadAllTextAsync(templatePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"读取模板文件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据题目ID列表获取题目
    /// </summary>
    /// <param name="questionIds">题目ID列表</param>
    /// <returns>题目列表</returns>
    private async Task<List<Question>> GetQuestionsByIdsAsync(List<Guid> questionIds)
    {
        var questions = new List<Question>();
        
        // 获取所有题库
        var allQuestionBanks = await _questionRepository.GetAllAsync();
        
        // 在所有题库中查找指定的题目
        foreach (var questionBank in allQuestionBanks)
        {
            foreach (var question in questionBank.Questions)
            {
                if (questionIds.Contains(question.Id))
                {
                    questions.Add(question);
                }
            }
        }
        
        return questions;
    }
}