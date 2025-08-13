using Microsoft.EntityFrameworkCore;
using QuizForge.Data.Contexts;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Data.Repositories;

/// <summary>
/// 模板数据访问实现
/// </summary>
public class TemplateRepository : ITemplateRepository
{
    private readonly QuizDbContext _context;

    /// <summary>
    /// 初始化模板仓库
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public TemplateRepository(QuizDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>模板数据</returns>
    public async Task<ExamTemplate?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Templates
                .Include(t => t.Sections)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取模板失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有模板
    /// </summary>
    /// <returns>模板列表</returns>
    public async Task<List<ExamTemplate>> GetAllAsync()
    {
        try
        {
            return await _context.Templates
                .Include(t => t.Sections)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取所有模板失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 添加模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>添加后的模板数据</returns>
    public async Task<ExamTemplate> AddAsync(ExamTemplate template)
    {
        try
        {
            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
            }

            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return template;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"添加模板失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新模板
    /// </summary>
    /// <param name="template">模板数据</param>
    /// <returns>更新后的模板数据</returns>
    public async Task<ExamTemplate> UpdateAsync(ExamTemplate template)
    {
        try
        {
            var existingTemplate = await _context.Templates
                .Include(t => t.Sections)
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            if (existingTemplate == null)
            {
                throw new Exception($"未找到ID为 {template.Id} 的模板");
            }

            // 更新基本信息
            existingTemplate.Name = template.Name;
            existingTemplate.Description = template.Description;
            existingTemplate.PaperSize = template.PaperSize;
            existingTemplate.Style = template.Style;
            existingTemplate.HeaderContent = template.HeaderContent;
            existingTemplate.FooterContent = template.FooterContent;
            existingTemplate.SealLine = template.SealLine;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            // 处理章节更新
            // 先删除不存在的章节
            var existingSectionIds = existingTemplate.Sections.Select(s => s.Id).ToList();
            var newSectionIds = template.Sections.Select(s => s.Id).ToList();
            var sectionsToDelete = existingSectionIds.Except(newSectionIds).ToList();

            foreach (var sectionId in sectionsToDelete)
            {
                var sectionToDelete = existingTemplate.Sections.FirstOrDefault(s => s.Id == sectionId);
                if (sectionToDelete != null)
                {
                    existingTemplate.Sections.Remove(sectionToDelete);
                }
            }

            // 添加或更新章节
            foreach (var section in template.Sections)
            {
                var existingSection = existingTemplate.Sections.FirstOrDefault(s => s.Id == section.Id);
                
                if (existingSection == null)
                {
                    // 新章节
                    if (section.Id == Guid.Empty)
                    {
                        section.Id = Guid.NewGuid();
                    }
                    existingTemplate.Sections.Add(section);
                }
                else
                {
                    // 更新现有章节
                    existingSection.Title = section.Title;
                    existingSection.Instructions = section.Instructions;
                    existingSection.QuestionCount = section.QuestionCount;
                    existingSection.TotalPoints = section.TotalPoints;
                    existingSection.QuestionIds = section.QuestionIds;
                }
            }

            await _context.SaveChangesAsync();
            return existingTemplate;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"更新模板失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除模板
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>删除结果</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var template = await _context.Templates
                .Include(t => t.Sections)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return false;
            }

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"删除模板失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据模板ID获取章节
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>章节列表</returns>
    public async Task<List<TemplateSection>> GetSectionsByTemplateIdAsync(Guid templateId)
    {
        try
        {
            return await _context.TemplateSections
                .Where(s => EF.Property<Guid>(s, "TemplateId") == templateId)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"根据模板ID获取章节失败: {ex.Message}", ex);
        }
    }
}