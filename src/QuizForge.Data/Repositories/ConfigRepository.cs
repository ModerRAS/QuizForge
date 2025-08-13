using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuizForge.Data.Contexts;
using QuizForge.Models.Interfaces;

namespace QuizForge.Data.Repositories;

/// <summary>
/// 配置数据访问实现
/// </summary>
public class ConfigRepository : IConfigRepository
{
    private readonly QuizDbContext _context;

    /// <summary>
    /// 初始化配置仓库
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public ConfigRepository(QuizDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="key">配置键</param>
    /// <returns>配置值</returns>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("配置键不能为空", nameof(key));
            }

            var config = await _context.Configurations.FindAsync(key);
            if (config == null)
            {
                return null;
            }

            if (config.Type != typeof(T).Name)
            {
                throw new InvalidOperationException($"配置类型不匹配。期望: {typeof(T).Name}, 实际: {config.Type}");
            }

            return JsonSerializer.Deserialize<T>(config.Value);
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取配置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    public async Task SetAsync<T>(string key, T value) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("配置键不能为空", nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var jsonValue = JsonSerializer.Serialize(value);
            var config = await _context.Configurations.FindAsync(key);

            if (config == null)
            {
                // 新配置
                config = new Configuration
                {
                    Key = key,
                    Value = jsonValue,
                    Type = typeof(T).Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Configurations.Add(config);
            }
            else
            {
                // 更新现有配置
                config.Value = jsonValue;
                config.Type = typeof(T).Name;
                config.UpdatedAt = DateTime.UtcNow;
                _context.Configurations.Update(config);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"设置配置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 移除配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>移除结果</returns>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("配置键不能为空", nameof(key));
            }

            var config = await _context.Configurations.FindAsync(key);
            if (config == null)
            {
                return false;
            }

            _context.Configurations.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"移除配置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("配置键不能为空", nameof(key));
            }

            return await _context.Configurations.AnyAsync(c => c.Key == key);
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"检查配置是否存在失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    /// <returns>配置字典</returns>
    public async Task<Dictionary<string, object>> GetAllAsync()
    {
        try
        {
            var configs = await _context.Configurations.ToListAsync();
            var result = new Dictionary<string, object>();

            foreach (var config in configs)
            {
                try
                {
                    // 根据类型反序列化
                    object? value = config.Type switch
                    {
                        "String" => JsonSerializer.Deserialize<string>(config.Value),
                        "Int32" => JsonSerializer.Deserialize<int>(config.Value),
                        "Int64" => JsonSerializer.Deserialize<long>(config.Value),
                        "Boolean" => JsonSerializer.Deserialize<bool>(config.Value),
                        "Decimal" => JsonSerializer.Deserialize<decimal>(config.Value),
                        "DateTime" => JsonSerializer.Deserialize<DateTime>(config.Value),
                        "String[]" => JsonSerializer.Deserialize<string[]>(config.Value),
                        "List`1" => JsonSerializer.Deserialize<List<string>>(config.Value),
                        "Dictionary`2" => JsonSerializer.Deserialize<Dictionary<string, object>>(config.Value),
                        _ => JsonSerializer.Deserialize<object>(config.Value)
                    };

                    if (value != null)
                    {
                        result[config.Key] = value;
                    }
                }
                catch (JsonException)
                {
                    // 如果反序列化失败，将原始JSON字符串作为值
                    result[config.Key] = config.Value;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            // 在实际应用中，这里应该记录日志
            throw new Exception($"获取所有配置失败: {ex.Message}", ex);
        }
    }
}