using Spectre.Console.Cli;
using Spectre.Console;
using QuizForge.CLI.Models;
using QuizForge.CLI.Services;
using Microsoft.Extensions.Logging;

namespace QuizForge.CLI.Commands;

/// <summary>
/// 配置显示命令
/// </summary>
public class ConfigShowCommand : AsyncCommand<ConfigParameters>
{
    private readonly ILogger<ConfigShowCommand> _logger;
    private readonly ICliConfigurationService _configService;
    private readonly ICliProgressService _progressService;

    public ConfigShowCommand(
        ILogger<ConfigShowCommand> logger,
        ICliConfigurationService configService,
        ICliProgressService progressService)
    {
        _logger = logger;
        _configService = configService;
        _progressService = progressService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigParameters parameters)
    {
        try
        {
            _progressService.ShowTitle("当前配置");

            if (parameters.ShowAll)
            {
                // 显示所有配置
                var allConfig = _configService.GetAllValues();
                
                foreach (var section in allConfig)
                {
                    _progressService.ShowTitle(section.Key);
                    
                    if (section.Value is Dictionary<string, object> sectionValues)
                    {
                        var rows = new List<string[]>();
                        foreach (var kvp in sectionValues)
                        {
                            rows.Add(new[] { kvp.Key, kvp.Value?.ToString() ?? "" });
                        }
                        
                        _progressService.ShowTable($"{section.Key} 配置", new[] { "键", "值" }, rows);
                    }
                }
            }
            else
            {
                // 显示特定配置
                if (!string.IsNullOrWhiteSpace(parameters.Key))
                {
                    var value = _configService.GetValue<string>(parameters.Key);
                    _progressService.ShowInfo($"{parameters.Key}: {value ?? "(未设置)"}");
                }
                else
                {
                    // 显示主要配置
                    var mainConfigs = new[]
                    {
                        ("LaTeX:DefaultTemplate", "默认模板"),
                        ("PDF:OutputDirectory", "PDF输出目录"),
                        ("Templates:Directory", "模板目录"),
                        ("CLI:ShowProgress", "显示进度"),
                        ("CLI:ColoredOutput", "彩色输出")
                    };

                    var rows = new List<string[]>();
                    foreach (var (key, description) in mainConfigs)
                    {
                        var value = _configService.GetValue<string>(key);
                        rows.Add(new[] { description, value ?? "(未设置)" });
                    }

                    _progressService.ShowTable("主要配置", new[] { "配置项", "值" }, rows);
                }
            }

            // 验证配置
            var validationResult = _configService.ValidateConfiguration();
            if (!validationResult.IsValid)
            {
                _progressService.ShowTitle("配置验证结果");
                foreach (var error in validationResult.Errors)
                {
                    _progressService.ShowError(error);
                }
                foreach (var warning in validationResult.Warnings)
                {
                    _progressService.ShowWarning(warning);
                }
            }
            else
            {
                _progressService.ShowSuccess("配置验证通过");
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行配置显示命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// 配置设置命令
/// </summary>
public class ConfigSetCommand : AsyncCommand<ConfigParameters>
{
    private readonly ILogger<ConfigSetCommand> _logger;
    private readonly ICliConfigurationService _configService;
    private readonly ICliProgressService _progressService;

    public ConfigSetCommand(
        ILogger<ConfigSetCommand> logger,
        ICliConfigurationService configService,
        ICliProgressService progressService)
    {
        _logger = logger;
        _configService = configService;
        _progressService = progressService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigParameters parameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameters.Key))
            {
                _progressService.ShowError("配置键不能为空");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(parameters.Value))
            {
                _progressService.ShowError("配置值不能为空");
                return 1;
            }

            _progressService.ShowTitle("设置配置");

            // 显示当前值
            var currentValue = _configService.GetValue<string>(parameters.Key);
            if (!string.IsNullOrEmpty(currentValue))
            {
                _progressService.ShowInfo($"当前值: {currentValue}");
            }

            // 设置新值
            var success = await _configService.SetValueAsync(parameters.Key, parameters.Value);
            if (!success)
            {
                _progressService.ShowError("配置设置失败");
                return 1;
            }

            _progressService.ShowSuccess($"配置设置成功: {parameters.Key} = {parameters.Value}");

            // 验证新配置
            var validationResult = _configService.ValidateConfiguration();
            if (!validationResult.IsValid)
            {
                _progressService.ShowWarning("配置验证发现问题");
                foreach (var warning in validationResult.Warnings)
                {
                    _progressService.ShowWarning(warning);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行配置设置命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// 配置重置命令
/// </summary>
public class ConfigResetCommand : AsyncCommand<ConfigParameters>
{
    private readonly ILogger<ConfigResetCommand> _logger;
    private readonly ICliConfigurationService _configService;
    private readonly ICliProgressService _progressService;

    public ConfigResetCommand(
        ILogger<ConfigResetCommand> logger,
        ICliConfigurationService configService,
        ICliProgressService progressService)
    {
        _logger = logger;
        _configService = configService;
        _progressService = progressService;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigParameters parameters)
    {
        try
        {
            _progressService.ShowTitle("重置配置");

            var key = parameters.Key;
            var resetAll = string.IsNullOrWhiteSpace(key);

            if (resetAll)
            {
                // 重置所有配置
                if (!AnsiConsole.Confirm("确定要重置所有配置到默认值吗？"))
                {
                    _progressService.ShowInfo("重置操作已取消");
                    return 0;
                }

                var success = await _configService.ResetValueAsync();
                if (!success)
                {
                    _progressService.ShowError("配置重置失败");
                    return 1;
                }

                _progressService.ShowSuccess("所有配置已重置为默认值");
            }
            else
            {
                // 重置特定配置
                var currentValue = _configService.GetValue<string>(key);
                if (string.IsNullOrEmpty(currentValue))
                {
                    _progressService.ShowInfo($"配置 {key} 已经是默认值");
                    return 0;
                }

                if (!AnsiConsole.Confirm($"确定要重置配置 '{key}' 吗？"))
                {
                    _progressService.ShowInfo("重置操作已取消");
                    return 0;
                }

                var success = await _configService.ResetValueAsync(key);
                if (!success)
                {
                    _progressService.ShowError($"配置重置失败: {key}");
                    return 1;
                }

                _progressService.ShowSuccess($"配置已重置: {key}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行配置重置命令失败");
            _progressService.ShowError($"执行失败: {ex.Message}");
            return 1;
        }
    }
}