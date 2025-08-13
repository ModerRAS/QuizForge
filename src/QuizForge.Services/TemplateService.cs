using QuizForge.Models;
using QuizForge.Models.Interfaces;
using QuizForge.Data.Repositories;
using QuizForge.Core.Services;

namespace QuizForge.Services;

/// <summary>
/// 模板服务实现
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly TemplateProcessor _templateProcessor;

    public TemplateService(
        ITemplateRepository templateRepository,
        TemplateProcessor templateProcessor)
    {
        _templateRepository = templateRepository;
        _templateProcessor = templateProcessor;
    }

    /// <summary>
    /// 获取模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>模板数据</returns>
    public async Task<ExamTemplate?> GetTemplateAsync(Guid id)
    {
        return await _templateRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// 获取所有模板
    /// </summary>
    /// <returns>模板列表</returns>
    public async Task<List<ExamTemplate>> GetAllTemplatesAsync()
    {
        return await _templateRepository.GetAllAsync();
    }

    /// <summary>
    /// 创建模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>创建后的模板数据</returns>
    public async Task<ExamTemplate> CreateTemplateAsync(ExamTemplate template)
    {
        // 验证模板数据
        if (!_templateProcessor.ValidateTemplate(template))
        {
            throw new InvalidOperationException("模板数据验证失败");
        }
        
        // 处理模板数据
        template = _templateProcessor.ProcessTemplate(template);
        
        // 保存到数据存储
        return await _templateRepository.AddAsync(template);
    }

    /// <summary>
    /// 更新模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>更新后的模板数据</returns>
    public async Task<ExamTemplate> UpdateTemplateAsync(ExamTemplate template)
    {
        // 验证模板数据
        if (!_templateProcessor.ValidateTemplate(template))
        {
            throw new InvalidOperationException("模板数据验证失败");
        }
        
        // 处理模板数据
        template = _templateProcessor.ProcessTemplate(template);
        
        // 更新到数据存储
        return await _templateRepository.UpdateAsync(template);
    }

    /// <summary>
    /// 删除模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>删除结果</returns>
    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        return await _templateRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 生成模板内容
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <param name="questions">题目列表</param>
    /// <returns>模板内容</returns>
    public async Task<string> GenerateTemplateContentAsync(ExamTemplate template, List<Question> questions)
    {
        return _templateProcessor.GenerateTemplateContent(template, questions);
    }

    /// <summary>
    /// 获取可用的模板样式
    /// </summary>
    /// <returns>模板样式列表</returns>
    public async Task<List<string>> GetAvailableTemplateStylesAsync()
    {
        // TODO: 实现获取可用模板样式的逻辑
        await Task.CompletedTask;
        return new List<string>
        {
            "基础样式",
            "高级样式",
            "自定义样式"
        };
    }
}