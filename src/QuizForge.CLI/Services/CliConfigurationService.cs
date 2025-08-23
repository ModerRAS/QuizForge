using Microsoft.Extensions.Options;
using QuizForge.CLI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace QuizForge.CLI.Services;

/// <summary>
/// CLI配置服务接口
/// </summary>
public interface ICliConfigurationService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    T? GetValue<T>(string key, T? defaultValue = default);

    /// <summary>
    /// 设置配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    /// <returns>是否成功</returns>
    Task<bool> SetValueAsync(string key, object value);

    /// <summary>
    /// 获取所有配置
    /// </summary>
    /// <returns>配置字典</returns>
    Dictionary<string, object> GetAllValues();

    /// <summary>
    /// 重置配置到默认值
    /// </summary>
    /// <param name="key">配置键，如果为空则重置所有配置</param>
    /// <returns>是否成功</returns>
    Task<bool> ResetValueAsync(string key = "");

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    /// <param name="filePath">配置文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> SaveConfigAsync(string filePath = "");

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    /// <param name="filePath">配置文件路径</param>
    /// <returns>是否成功</returns>
    Task<bool> LoadConfigAsync(string filePath = "");

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <returns>验证结果</returns>
    Models.ValidationResult ValidateConfiguration();
}

/// <summary>
/// CLI配置服务实现
/// </summary>
public class CliConfigurationService : ICliConfigurationService
{
    private readonly ILogger<CliConfigurationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly CliOptions _options;
    private readonly string _configFilePath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="configuration">配置</param>
    /// <param name="options">CLI选项</param>
    public CliConfigurationService(
        ILogger<CliConfigurationService> logger,
        IConfiguration configuration,
        IOptions<CliOptions> options)
    {
        _logger = logger;
        _configuration = configuration;
        _options = options.Value;
        
        // 设置配置文件路径
        _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "quizforge.config.json");
    }

    /// <inheritdoc/>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        try
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return ConvertTo<T>(value, defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置值失败: {Key}", key);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SetValueAsync(string key, object value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogError("配置键不能为空");
                return false;
            }

            // 验证配置键格式
            if (!IsValidKey(key))
            {
                _logger.LogError("无效的配置键格式: {Key}", key);
                return false;
            }

            // 加载现有配置
            var config = await LoadConfigurationFileAsync();
            
            // 设置新值
            var keys = key.Split(':');
            var current = config as IDictionary<string, object>;
            
            for (int i = 0; i < keys.Length - 1; i++)
            {
                var currentKey = keys[i];
                if (!current.ContainsKey(currentKey))
                {
                    current[currentKey] = new Dictionary<string, object>();
                }
                
                if (current[currentKey] is not IDictionary<string, object> nested)
                {
                    current[currentKey] = new Dictionary<string, object>();
                    nested = (IDictionary<string, object>)current[currentKey];
                }
                
                current = nested;
            }

            current[keys[^1]] = value;

            // 保存配置
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_configFilePath, json);
            
            _logger.LogInformation("配置设置成功: {Key} = {Value}", key, value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置配置值失败: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc/>
    public Dictionary<string, object> GetAllValues()
    {
        var result = new Dictionary<string, object>();
        
        try
        {
            // 获取所有配置节
            var sections = new[] { "LaTeX", "Excel", "PDF", "Templates", "CLI" };
            
            foreach (var section in sections)
            {
                var sectionConfig = _configuration.GetSection(section);
                var sectionDict = new Dictionary<string, object>();
                
                foreach (var child in sectionConfig.GetChildren())
                {
                    sectionDict[child.Key] = child.Value ?? string.Empty;
                }
                
                result[section] = sectionDict;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有配置值失败");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> ResetValueAsync(string key = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                // 重置所有配置
                if (File.Exists(_configFilePath))
                {
                    File.Delete(_configFilePath);
                }
                
                _logger.LogInformation("已重置所有配置");
                return true;
            }
            else
            {
                // 重置特定配置
                var config = await LoadConfigurationFileAsync();
                var keys = key.Split(':');
                
                if (keys.Length == 1)
                {
                    // 删除整个节
                    config.Remove(keys[0]);
                }
                else
                {
                    // 删除嵌套配置
                    var current = config as IDictionary<string, object>;
                    for (int i = 0; i < keys.Length - 1; i++)
                    {
                        var currentKey = keys[i];
                        if (current.ContainsKey(currentKey) && current[currentKey] is IDictionary<string, object> nested)
                        {
                            current = nested;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    current.Remove(keys[^1]);
                }

                // 保存配置
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(_configFilePath, json);
                
                _logger.LogInformation("已重置配置: {Key}", key);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置配置失败: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveConfigAsync(string filePath = "")
    {
        try
        {
            var targetPath = string.IsNullOrWhiteSpace(filePath) ? _configFilePath : filePath;
            
            var config = GetAllValues();
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(targetPath, json);
            
            _logger.LogInformation("配置保存成功: {FilePath}", targetPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> LoadConfigAsync(string filePath = "")
    {
        try
        {
            var targetPath = string.IsNullOrWhiteSpace(filePath) ? _configFilePath : filePath;
            
            if (!File.Exists(targetPath))
            {
                _logger.LogWarning("配置文件不存在: {FilePath}", targetPath);
                return false;
            }

            var json = await File.ReadAllTextAsync(targetPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (config != null)
            {
                _logger.LogInformation("配置加载成功: {FilePath}", targetPath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <inheritdoc/>
    public Models.ValidationResult ValidateConfiguration()
    {
        var result = new Models.ValidationResult();

        try
        {
            // 验证必要的配置项
            var requiredConfigs = new[]
            {
                ("LaTeX:DefaultTemplate", "standard"),
                ("PDF:OutputDirectory", "./output"),
                ("Templates:Directory", "./templates")
            };

            foreach (var (key, defaultValue) in requiredConfigs)
            {
                var value = GetValue<string>(key);
                if (string.IsNullOrEmpty(value))
                {
                    result.Warnings.Add($"配置项 {key} 未设置，使用默认值: {defaultValue}");
                }
            }

            // 验证路径配置
            var pathsToValidate = new[]
            {
                ("PDF:OutputDirectory", "PDF输出目录"),
                ("Templates:Directory", "模板目录"),
                ("LaTeX:TempDirectory", "临时目录")
            };

            foreach (var (key, description) in pathsToValidate)
            {
                var path = GetValue<string>(key);
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        // 检查路径是否包含非法字符
                        var invalidChars = Path.GetInvalidPathChars();
                        if (path.IndexOfAny(invalidChars) >= 0)
                        {
                            result.Errors.Add($"{description}包含非法字符: {path}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"验证{description}失败: {ex.Message}");
                    }
                }
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"配置验证失败: {ex.Message}");
            _logger.LogError(ex, "配置验证失败");
        }

        return result;
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>配置字典</returns>
    private async Task<Dictionary<string, object>> LoadConfigurationFileAsync()
    {
        if (File.Exists(_configFilePath))
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return config ?? new Dictionary<string, object>();
        }

        return new Dictionary<string, object>();
    }

    /// <summary>
    /// 转换值到指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="value">字符串值</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>转换后的值</returns>
    private T? ConvertTo<T>(string value, T? defaultValue)
    {
        try
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)(bool.Parse(value));
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(value);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            else
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 验证配置键格式
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否有效</returns>
    private bool IsValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var parts = key.Split(':');
        if (parts.Length < 2)
        {
            return false;
        }

        // 验证第一部分是否为有效的配置节
        var validSections = new[] { "LaTeX", "Excel", "PDF", "Templates", "CLI" };
        return validSections.Contains(parts[0]);
    }
}