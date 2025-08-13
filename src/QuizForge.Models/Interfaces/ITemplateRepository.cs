using QuizForge.Models;

namespace QuizForge.Models.Interfaces;

/// <summary>
/// 模板仓库接口
/// </summary>
public interface ITemplateRepository
{
    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>模板数据</returns>
    Task<ExamTemplate?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 获取所有模板
    /// </summary>
    /// <returns>模板列表</returns>
    Task<List<ExamTemplate>> GetAllAsync();
    
    /// <summary>
    /// 添加模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>添加后的模板数据</returns>
    Task<ExamTemplate> AddAsync(ExamTemplate template);
    
    /// <summary>
    /// 更新模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>更新后的模板数据</returns>
    Task<ExamTemplate> UpdateAsync(ExamTemplate template);
    
    /// <summary>
    /// 删除模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 根据模板ID获取章节
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>章节列表</returns>
    Task<List<TemplateSection>> GetSectionsByTemplateIdAsync(Guid templateId);
}