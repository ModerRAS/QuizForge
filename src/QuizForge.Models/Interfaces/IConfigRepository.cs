namespace QuizForge.Models.Interfaces;

/// <summary>
/// 配置仓库接口
/// </summary>
public interface IConfigRepository
{
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="key">配置键</param>
    /// <returns>配置值</returns>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// 设置配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    Task SetAsync<T>(string key, T value) where T : class;
    
    /// <summary>
    /// 移除配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>移除结果</returns>
    Task<bool> RemoveAsync(string key);
    
    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// 获取所有配置
    /// </summary>
    /// <returns>配置字典</returns>
    Task<Dictionary<string, object>> GetAllAsync();
}