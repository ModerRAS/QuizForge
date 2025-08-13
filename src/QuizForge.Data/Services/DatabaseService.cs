using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizForge.Data.Contexts;
using QuizForge.Data.Migrations;

namespace QuizForge.Data.Services;

/// <summary>
/// 数据库服务，用于初始化数据库和执行迁移
/// </summary>
public class DatabaseService
{
    private readonly QuizDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    /// <summary>
    /// 初始化数据库服务
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public DatabaseService(QuizDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>初始化结果</returns>
    public async Task<bool> InitializeAsync(string databasePath)
    {
        try
        {
            _logger.LogInformation("开始初始化数据库...");

            // 创建数据库连接选项
            var options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            // 创建新的数据库上下文实例
            using var dbContext = new QuizDbContext(options);

            // 执行迁移
            var migration = new InitialMigration(dbContext);
            var result = await migration.UpAsync();

            if (result)
            {
                _logger.LogInformation("数据库初始化成功");
            }
            else
            {
                _logger.LogError("数据库初始化失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化数据库时发生错误");
            throw new Exception($"初始化数据库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 迁移数据库
    /// </summary>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>迁移结果</returns>
    public async Task<bool> MigrateAsync(string databasePath)
    {
        try
        {
            _logger.LogInformation("开始迁移数据库...");

            // 创建数据库连接选项
            var options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            // 创建新的数据库上下文实例
            using var dbContext = new QuizDbContext(options);

            // 执行迁移
            var migration = new InitialMigration(dbContext);
            var result = await migration.UpAsync();

            if (result)
            {
                _logger.LogInformation("数据库迁移成功");
            }
            else
            {
                _logger.LogError("数据库迁移失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "迁移数据库时发生错误");
            throw new Exception($"迁移数据库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 回滚数据库
    /// </summary>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>回滚结果</returns>
    public async Task<bool> RollbackAsync(string databasePath)
    {
        try
        {
            _logger.LogInformation("开始回滚数据库...");

            // 创建数据库连接选项
            var options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            // 创建新的数据库上下文实例
            using var dbContext = new QuizDbContext(options);

            // 执行回滚
            var migration = new InitialMigration(dbContext);
            var result = await migration.DownAsync();

            if (result)
            {
                _logger.LogInformation("数据库回滚成功");
            }
            else
            {
                _logger.LogError("数据库回滚失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回滚数据库时发生错误");
            throw new Exception($"回滚数据库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查数据库是否存在
    /// </summary>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>是否存在</returns>
    public bool DatabaseExists(string databasePath)
    {
        try
        {
            return File.Exists(databasePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查数据库是否存在时发生错误");
            throw new Exception($"检查数据库是否存在失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 创建数据库备份
    /// </summary>
    /// <param name="databasePath">数据库文件路径</param>
    /// <param name="backupPath">备份文件路径</param>
    /// <returns>备份结果</returns>
    public async Task<bool> BackupDatabaseAsync(string databasePath, string backupPath)
    {
        try
        {
            _logger.LogInformation("开始备份数据库...");

            if (!File.Exists(databasePath))
            {
                _logger.LogError("数据库文件不存在: {DatabasePath}", databasePath);
                return false;
            }

            // 确保备份目录存在
            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            // 复制数据库文件
            await Task.Run(() => File.Copy(databasePath, backupPath, true));

            _logger.LogInformation("数据库备份成功: {BackupPath}", backupPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "备份数据库时发生错误");
            throw new Exception($"备份数据库失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 恢复数据库备份
    /// </summary>
    /// <param name="backupPath">备份文件路径</param>
    /// <param name="databasePath">数据库文件路径</param>
    /// <returns>恢复结果</returns>
    public async Task<bool> RestoreDatabaseAsync(string backupPath, string databasePath)
    {
        try
        {
            _logger.LogInformation("开始恢复数据库备份...");

            if (!File.Exists(backupPath))
            {
                _logger.LogError("备份文件不存在: {BackupPath}", backupPath);
                return false;
            }

            // 确保数据库目录存在
            var databaseDirectory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(databaseDirectory) && !Directory.Exists(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            // 如果数据库文件存在，先删除
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }

            // 复制备份文件
            await Task.Run(() => File.Copy(backupPath, databasePath, true));

            _logger.LogInformation("数据库恢复成功: {DatabasePath}", databasePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复数据库备份时发生错误");
            throw new Exception($"恢复数据库备份失败: {ex.Message}", ex);
        }
    }
}