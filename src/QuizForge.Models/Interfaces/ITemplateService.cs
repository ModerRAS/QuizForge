using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 模板服务接口
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// 获取模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>模板数据</returns>
    Task<ExamTemplate?> GetTemplateAsync(Guid id);
    
    /// <summary>
    /// 获取所有模板
    /// </summary>
    /// <returns>模板列表</returns>
    Task<List<ExamTemplate>> GetAllTemplatesAsync();
    
    /// <summary>
    /// 创建模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>创建后的模板数据</returns>
    Task<ExamTemplate> CreateTemplateAsync(ExamTemplate template);
    
    /// <summary>
    /// 更新模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>更新后的模板数据</returns>
    Task<ExamTemplate> UpdateTemplateAsync(ExamTemplate template);
    
    /// <summary>
    /// 删除模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteTemplateAsync(Guid id);
    
    /// <summary>
    /// 生成模板内容
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <param name="questions">题目列表</param>
    /// <returns>模板内容</returns>
    Task<string> GenerateTemplateContentAsync(ExamTemplate template, List<Question> questions);
    
    /// <summary>
    /// 获取可用的模板样式
    /// </summary>
    /// <returns>模板样式列表</returns>
    Task<List<string>> GetAvailableTemplateStylesAsync();
}