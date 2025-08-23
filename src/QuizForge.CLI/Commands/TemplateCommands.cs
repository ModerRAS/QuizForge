using Spectre.Console.Cli;
using Spectre.Console;
using QuizForge.CLI.Models;
using QuizForge.CLI.Services;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Commands;

/// <summary>
/// 模板列表命令
/// </summary>
public class TemplateListCommand : AsyncCommand<TemplateParameters>
{
    private readonly ILogger<TemplateListCommand> _logger;
    private readonly ICliGenerationService _generationService;
    private readonly ICliProgressService _progressService;

    public TemplateListCommand(
        ILogger<TemplateListCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService)
    {
        _logger = logger;
        _generationService = generationService;
        _progressService = progressService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateParameters parameters)
    {
        try
        {
            _progressService.ShowTitle("可用模板列表");

            var templates = await _generationService.GetAvailableTemplatesAsync();

            if (templates.Count == 0)
            {
                _progressService.ShowWarning("没有找到可用模板");
                return 0;
            }

            // 显示模板表格
            var columns = new[] { "名称", "类型", "默认", "描述" };
            var rows = new List<string[]>();

            foreach (var template in templates)
            {
                rows.Add(new[]
                {
                    template.Name,
                    template.Type,
                    template.IsDefault ? "是" : "否",
                    template.Description
                });
            }

            _progressService.ShowTable("可用模板", columns, rows);
            _progressService.ShowInfo($"共找到 {templates.Count} 个模板");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行模板列表命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// 模板创建命令
/// </summary>
public class TemplateCreateCommand : AsyncCommand<TemplateParameters>
{
    private readonly ILogger<TemplateCreateCommand> _logger;
    private readonly ICliGenerationService _generationService;
    private readonly ICliProgressService _progressService;
    private readonly ICliFileService _fileService;
    private readonly ICliConfigurationService _configService;

    public TemplateCreateCommand(
        ILogger<TemplateCreateCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService,
        ICliConfigurationService configService)
    {
        _logger = logger;
        _generationService = generationService;
        _progressService = progressService;
        _fileService = fileService;
        _configService = configService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateParameters parameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameters.Name))
            {
                _progressService.ShowError("模板名称不能为空");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(parameters.FilePath))
            {
                _progressService.ShowError("模板文件路径不能为空");
                return 1;
            }

            _progressService.ShowTitle("创建模板");

            // 验证源文件
            var validationResult = await _fileService.ValidateFileAsync(parameters.FilePath);
            if (!validationResult.IsValid)
            {
                _progressService.ShowValidationResult(validationResult);
                return 1;
            }

            // 获取模板目录
            var templateDir = _configService.GetValue<string>("Templates:Directory", "./templates");
            await _fileService.EnsureDirectoryExistsAsync(templateDir);

            // 创建模板文件
            var templateFileName = $"{parameters.Name}.tex";
            var templatePath = Path.Combine(templateDir, templateFileName);

            var success = await _fileService.CopyFileAsync(parameters.FilePath, templatePath, true);
            if (!success)
            {
                _progressService.ShowError("模板文件创建失败");
                return 1;
            }

            // 如果设置为默认模板，更新配置
            if (parameters.IsDefault)
            {
                var configSuccess = await _configService.SetValueAsync("Templates:DefaultTemplate", templateFileName);
                if (!configSuccess)
                {
                    _progressService.ShowWarning("无法设置默认模板");
                }
            }

            _progressService.ShowSuccess($"模板创建成功: {templatePath}");
            _progressService.ShowInfo($"模板名称: {parameters.Name}");
            _progressService.ShowInfo($"模板类型: LaTeX");
            _progressService.ShowInfo($"是否默认: {(parameters.IsDefault ? "是" : "否")}");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行模板创建命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// 模板删除命令
/// </summary>
public class TemplateDeleteCommand : AsyncCommand<TemplateParameters>
{
    private readonly ILogger<TemplateDeleteCommand> _logger;
    private readonly ICliGenerationService _generationService;
    private readonly ICliProgressService _progressService;
    private readonly ICliFileService _fileService;
    private readonly ICliConfigurationService _configService;

    public TemplateDeleteCommand(
        ILogger<TemplateDeleteCommand> logger,
        ICliGenerationService generationService,
        ICliProgressService progressService,
        ICliFileService fileService,
        ICliConfigurationService configService)
    {
        _logger = logger;
        _generationService = generationService;
        _progressService = progressService;
        _fileService = fileService;
        _configService = configService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateParameters parameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameters.Name))
            {
                _progressService.ShowError("模板名称不能为空");
                return 1;
            }

            _progressService.ShowTitle("删除模板");

            // 获取模板列表
            var templates = await _generationService.GetAvailableTemplatesAsync();
            var templateToDelete = templates.FirstOrDefault(t => 
                t.Name.Equals(parameters.Name, StringComparison.OrdinalIgnoreCase));

            if (templateToDelete == null)
            {
                _progressService.ShowError($"模板不存在: {parameters.Name}");
                return 1;
            }

            // 确认删除
            if (!AnsiConsole.Confirm($"确定要删除模板 '{parameters.Name}' 吗？"))
            {
                _progressService.ShowInfo("删除操作已取消");
                return 0;
            }

            // 删除模板文件
            var success = await _fileService.DeleteFileAsync(templateToDelete.FilePath);
            if (!success)
            {
                _progressService.ShowError("模板文件删除失败");
                return 1;
            }

            // 如果是默认模板，重置配置
            if (templateToDelete.IsDefault)
            {
                var configSuccess = await _configService.SetValueAsync("Templates:DefaultTemplate", "standard.tex");
                if (!configSuccess)
                {
                    _progressService.ShowWarning("无法重置默认模板配置");
                }
            }

            _progressService.ShowSuccess($"模板删除成功: {parameters.Name}");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行模板删除命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}