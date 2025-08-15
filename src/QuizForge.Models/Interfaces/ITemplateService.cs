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
    
    /// <summary>
    /// 获取模板节列表
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>模板节列表</returns>
    Task<List<TemplateSection>> GetTemplateSectionsAsync(Guid templateId);
    
    /// <summary>
    /// 创建模板节
    /// </summary>
    /// <param name="section">模板节数据</param>
    /// <returns>创建后的模板节数据</returns>
    Task<TemplateSection> CreateTemplateSectionAsync(TemplateSection section);
    
    /// <summary>
    /// 更新模板节
    /// </summary>
    /// <param name="section">模板节数据</param>
    /// <returns>更新后的模板节数据</returns>
    Task<TemplateSection> UpdateTemplateSectionAsync(TemplateSection section);
    
    /// <summary>
    /// 删除模板节
    /// </summary>
    /// <param name="sectionId">模板节ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteTemplateSectionAsync(Guid sectionId);
    
    /// <summary>
    /// 根据模板ID删除所有模板节
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteTemplateSectionsByTemplateIdAsync(Guid templateId);
    
    /// <summary>
    /// 搜索模板
    /// </summary>
    /// <param name="searchText">搜索文本</param>
    /// <returns>匹配的模板列表</returns>
    Task<List<ExamTemplate>> SearchTemplatesAsync(string searchText);
    
    /// <summary>
    /// 导出模板到LaTeX文件
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>导出结果</returns>
    Task<bool> ExportTemplateToLaTeXAsync(Guid templateId, string filePath);
    
    /// <summary>
    /// 导出模板到JSON文件
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>导出结果</returns>
    Task<bool> ExportTemplateToJsonAsync(Guid templateId, string filePath);
    
    /// <summary>
    /// 从LaTeX文件导入模板
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>导入的模板数据</returns>
    Task<ExamTemplate?> ImportTemplateFromLaTeXAsync(string filePath);
    
    /// <summary>
    /// 从JSON文件导入模板
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>导入的模板数据</returns>
    Task<ExamTemplate?> ImportTemplateFromJsonAsync(string filePath);
    
    /// <summary>
    /// 加载模板
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>加载的模板数据</returns>
    Task<ExamTemplate?> LoadTemplateAsync(string filePath);
}