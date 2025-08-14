using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 生成服务接口
/// </summary>
public interface IGenerationService
{
    /// <summary>
    /// 生成试卷
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="questionBankId">题库ID</param>
    /// <param name="options">试卷选项</param>
    /// <returns>生成的试卷</returns>
    Task<ExamPaper> GenerateExamPaperAsync(Guid templateId, Guid questionBankId, ExamPaperOptions options);
    
    /// <summary>
    /// 生成LaTeX内容
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>LaTeX内容</returns>
    Task<string> GenerateLaTeXContentAsync(ExamPaper examPaper);
    
    /// <summary>
    /// 生成预览
    /// </summary>
    /// <param name="latexContent">LaTeX内容</param>
    /// <returns>预览流</returns>
    Task<Stream> GeneratePreviewAsync(string latexContent);
    
    /// <summary>
    /// 验证试卷
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateExamPaperAsync(ExamPaper examPaper);
    
    /// <summary>
    /// 重新生成试卷
    /// </summary>
    /// <param name="examPaper">试卷数据</param>
    /// <returns>重新生成的试卷</returns>
    Task<ExamPaper> RegenerateExamPaperAsync(ExamPaper examPaper);
}

/// <summary>
/// 试卷选项
/// </summary>
public class ExamPaperOptions
{
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 随机题目
    /// </summary>
    public bool RandomQuestions { get; set; } = true;
    
    /// <summary>
    /// 题目数量
    /// </summary>
    public int QuestionCount { get; set; } = 50;
    
    /// <summary>
    /// 包含答案
    /// </summary>
    public bool IncludeAnswers { get; set; } = false;
    
    /// <summary>
    /// 难度分布
    /// </summary>
    public Dictionary<string, int> DifficultyDistribution { get; set; } = new();
    
    /// <summary>
    /// 类别分布
    /// </summary>
    public Dictionary<string, int> CategoryDistribution { get; set; } = new();
    
    /// <summary>
    /// 考试时间（分钟）
    /// </summary>
    public int ExamTime { get; set; } = 120;
    
    /// <summary>
    /// 学校名称
    /// </summary>
    public string School { get; set; } = string.Empty;
    
    /// <summary>
    /// 班级名称
    /// </summary>
    public string Class { get; set; } = string.Empty;
    
    /// <summary>
    /// 教师姓名
    /// </summary>
    public string Teacher { get; set; } = string.Empty;
    
    /// <summary>
    /// 考试日期
    /// </summary>
    public DateTime ExamDate { get; set; } = DateTime.Now;
}