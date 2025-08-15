using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuizForge.Infrastructure.Services;
using QuizForge.Models;
using QuizForge.Models.Interfaces;
using Xunit;

namespace QuizForge.Tests.Services;

/// <summary>
/// 批量生成服务测试类
/// </summary>
public class BatchGenerationServiceTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _testTempDirectory;
    private readonly Mock<IExportService> _exportServiceMock;
    private readonly Mock<IQuestionService> _questionServiceMock;
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly Mock<IGenerationService> _generationServiceMock;
    private readonly Mock<IPdfEngine> _pdfEngineMock;
    private readonly Mock<PdfCacheService> _pdfCacheServiceMock;

    /// <summary>
    /// 构造函数
    /// </summary>
    public BatchGenerationServiceTests()
    {
        // 创建测试临时目录
        _testTempDirectory = Path.Combine(Path.GetTempPath(), "QuizForge.Tests.Batch");
        
        if (!Directory.Exists(_testTempDirectory))
        {
            Directory.CreateDirectory(_testTempDirectory);
        }

        // 创建模拟对象
        _exportServiceMock = new Mock<IExportService>();
        _questionServiceMock = new Mock<IQuestionService>();
        _templateServiceMock = new Mock<ITemplateService>();
        _generationServiceMock = new Mock<IGenerationService>();
        _pdfEngineMock = new Mock<IPdfEngine>();
        _pdfCacheServiceMock = new Mock<PdfCacheService>(MockBehavior.Loose, null, null);

        // 设置模拟服务的行为
        SetupMockServices();

        // 创建服务集合
        var services = new ServiceCollection();
        
        // 添加日志
        services.AddLogging(builder => builder.AddConsole());
        
        // 注册服务
        services.AddScoped<IExportService>(_ => _exportServiceMock.Object);
        services.AddScoped<IQuestionService>(_ => _questionServiceMock.Object);
        services.AddScoped<ITemplateService>(_ => _templateServiceMock.Object);
        services.AddScoped<IGenerationService>(_ => _generationServiceMock.Object);
        services.AddScoped<IPdfEngine>(_ => _pdfEngineMock.Object);
        services.AddScoped<PdfCacheService>(_ => _pdfCacheServiceMock.Object);
        services.AddScoped<IBatchGenerationService, BatchGenerationService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 设置模拟服务的行为
    /// </summary>
    private void SetupMockServices()
    {
        // 设置题库服务
        var questionBank = new QuestionBank
        {
            Id = Guid.NewGuid(),
            Name = "测试题库",
            Questions = new List<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    Content = "测试问题1",
                    Type = "单选题",
                    Difficulty = "1",
                    Options = new List<QuestionOption> 
                    {
                        new QuestionOption { Key = "A", Value = "选项A" },
                        new QuestionOption { Key = "B", Value = "选项B" },
                        new QuestionOption { Key = "C", Value = "选项C" },
                        new QuestionOption { Key = "D", Value = "选项D" }
                    },
                    CorrectAnswer = "A",
                    Points = 5
                },
                new Question
                {
                    Id = Guid.NewGuid(),
                    Content = "测试问题2",
                    Type = "多选题",
                    Difficulty = "2",
                    Options = new List<QuestionOption> 
                    {
                        new QuestionOption { Key = "A", Value = "选项A" },
                        new QuestionOption { Key = "B", Value = "选项B" },
                        new QuestionOption { Key = "C", Value = "选项C" },
                        new QuestionOption { Key = "D", Value = "选项D" }
                    },
                    CorrectAnswer = "A,B",
                    Points = 10
                }
            }
        };

        _questionServiceMock.Setup(s => s.GetQuestionBankAsync(It.IsAny<Guid>()))
            .ReturnsAsync(questionBank);

        // 设置模板服务
        var template = new ExamTemplate
        {
            Id = Guid.NewGuid(),
            Name = "测试模板",
            Description = "测试模板内容"
        };

        _templateServiceMock.Setup(s => s.GetTemplateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(template);

        // 设置生成服务
        _generationServiceMock.Setup(s => s.GenerateLaTeXContentAsync(It.IsAny<ExamPaper>()))
            .ReturnsAsync("\\documentclass{article}\\begin{document}测试内容\\end{document}");

        // 设置导出服务
        _exportServiceMock.Setup(s => s.ExportToPdfAsync(It.IsAny<string>(), It.IsAny<ExportConfiguration>()))
            .ReturnsAsync((string content, ExportConfiguration config) => 
            {
                var fileName = $"{config.FileName}_{Guid.NewGuid():N}.pdf";
                var filePath = Path.Combine(config.OutputPath, fileName);
                File.WriteAllBytes(filePath, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }); // 模拟PDF文件头
                return filePath;
            });

        // 设置PDF引擎
        _pdfEngineMock.Setup(s => s.GeneratePdfAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // 设置PDF缓存服务
        _pdfCacheServiceMock.Setup(s => s.GetCachedPdf(It.IsAny<string>()))
            .Returns((string)null); // 默认没有缓存

        _pdfCacheServiceMock.Setup(s => s.CachePdfAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync((string content, byte[] data) => Path.Combine(_testTempDirectory, $"{Guid.NewGuid():N}.pdf"));
    }

    /// <summary>
    /// 测试批量生成试卷
    /// </summary>
    [Fact]
    public async Task BatchGenerateExamPapersAsync_ShouldGenerateExamPapers()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new BatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 3,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "测试试卷",
            Format = ExportFormat.PDF,
            ParallelCount = 2
        };

        // 执行
        var result = await batchService.BatchGenerateExamPapersAsync(request);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(BatchGenerationStatus.Created, result.Status);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(0, result.CompletedCount);
        Assert.Equal(0, result.FailedCount);

        // 等待批量生成完成
        await Task.Delay(2000); // 等待生成完成

        // 获取进度
        var progress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(progress);
        Assert.Equal(3, progress.TotalCount);
        Assert.True(progress.CompletedCount > 0 || progress.FailedCount > 0);
    }

    /// <summary>
    /// 测试获取批量生成进度
    /// </summary>
    [Fact]
    public async Task GetBatchGenerationProgressAsync_ShouldReturnCorrectProgress()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new BatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 5,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "进度测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1
        };

        // 创建批量生成任务
        var result = await batchService.BatchGenerateExamPapersAsync(request);

        // 等待一段时间
        await Task.Delay(1000);

        // 执行
        var progress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);

        // 断言
        Assert.NotNull(progress);
        Assert.Equal(result.BatchId, progress.BatchId);
        Assert.Equal(5, progress.TotalCount);
        Assert.True(progress.ElapsedSeconds > 0);
    }

    /// <summary>
    /// 测试取消批量生成任务
    /// </summary>
    [Fact]
    public async Task CancelBatchGenerationAsync_ShouldCancelBatchGeneration()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new BatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 10, // 较大的数量，确保有时间取消
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "取消测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1
        };

        // 创建批量生成任务
        var result = await batchService.BatchGenerateExamPapersAsync(request);

        // 等待一段时间
        await Task.Delay(500);

        // 执行
        var cancelResult = await batchService.CancelBatchGenerationAsync(result.BatchId);

        // 断言
        Assert.True(cancelResult);

        // 等待取消生效
        await Task.Delay(500);

        // 获取进度
        var progress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);

        // 断言
        Assert.NotNull(progress);
        Assert.Equal(BatchGenerationStatus.Cancelled, progress.Status);
    }

    /// <summary>
    /// 测试获取批量生成历史
    /// </summary>
    [Fact]
    public async Task GetBatchGenerationHistoryAsync_ShouldReturnHistory()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new BatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 2,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "历史测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1
        };

        // 创建批量生成任务
        var result = await batchService.BatchGenerateExamPapersAsync(request);

        // 等待生成完成
        await Task.Delay(2000);

        // 执行
        var history = await batchService.GetBatchGenerationHistoryAsync(10, 1);

        // 断言
        Assert.NotNull(history);
        Assert.NotNull(history.Items);
        Assert.True(history.Items.Count > 0);
        Assert.Contains(result.BatchId, history.Items.Select(h => h.BatchId));
    }

    /// <summary>
    /// 测试清理完成的批量生成任务
    /// </summary>
    [Fact]
    public async Task CleanupCompletedBatchGenerationsAsync_ShouldCleanupCompletedTasks()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new BatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 2,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "清理测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1
        };

        // 创建批量生成任务
        var result = await batchService.BatchGenerateExamPapersAsync(request);

        // 等待生成完成
        await Task.Delay(2000);

        // 执行
        var cleanedCount = await batchService.CleanupCompletedBatchGenerationsAsync(0); // 清理所有已完成的任务

        // 断言
        Assert.True(cleanedCount >= 1);

        // 尝试获取已清理的任务进度，应该抛出异常
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            batchService.GetBatchGenerationProgressAsync(result.BatchId));
    }

    /// <summary>
    /// 测试无效的批量生成请求
    /// </summary>
    [Fact]
    public async Task BatchGenerateExamPapersAsync_ShouldHandleInvalidRequest()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();

        // 执行和断言 - 空题库ID列表
        await Assert.ThrowsAsync<ArgumentException>(() => 
            batchService.BatchGenerateExamPapersAsync(new BatchGenerationRequest
            {
                QuestionBankIds = new List<Guid>(),
                TemplateId = Guid.NewGuid(),
                Count = 1,
                OutputDirectory = _testTempDirectory,
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));

        // 执行和断言 - 无效的生成数量
        await Assert.ThrowsAsync<ArgumentException>(() => 
            batchService.BatchGenerateExamPapersAsync(new BatchGenerationRequest
            {
                QuestionBankIds = new List<Guid> { Guid.NewGuid() },
                TemplateId = Guid.NewGuid(),
                Count = 0,
                OutputDirectory = _testTempDirectory,
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));

        // 执行和断言 - 空输出目录
        await Assert.ThrowsAsync<ArgumentException>(() => 
            batchService.BatchGenerateExamPapersAsync(new BatchGenerationRequest
            {
                QuestionBankIds = new List<Guid> { Guid.NewGuid() },
                TemplateId = Guid.NewGuid(),
                Count = 1,
                OutputDirectory = "",
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));
    }

    /// <summary>
    /// 测试获取不存在的批量生成进度
    /// </summary>
    [Fact]
    public async Task GetBatchGenerationProgressAsync_ShouldHandleNonExistentTask()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var nonExistentId = Guid.NewGuid();

        // 执行和断言
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            batchService.GetBatchGenerationProgressAsync(nonExistentId));
    }

    /// <summary>
    /// 测试取消不存在的批量生成任务
    /// </summary>
    [Fact]
    public async Task CancelBatchGenerationAsync_ShouldHandleNonExistentTask()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var nonExistentId = Guid.NewGuid();

        // 执行
        var result = await batchService.CancelBatchGenerationAsync(nonExistentId);

        // 断言
        Assert.False(result);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        try
        {
            // 清理测试目录
            if (Directory.Exists(_testTempDirectory))
            {
                Directory.Delete(_testTempDirectory, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

    #region 高级批量生成测试

    /// <summary>
    /// 测试高级批量生成试卷
    /// </summary>
    [Fact]
    public async Task AdvancedBatchGenerateExamPapersAsync_ShouldGenerateExamPapers()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new AdvancedBatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 3,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "高级测试试卷",
            Format = ExportFormat.PDF,
            ParallelCount = 2,
            Options = new AdvancedBatchGenerationOptions
            {
                VaryQuestionCombination = true,
                VaryQuestionOrder = true,
                VaryOptionOrder = true,
                GenerateAnswerPapers = true,
                GenerateStatisticsReport = true
            },
            OutputOptions = new OutputOrganizationOptions
            {
                OrganizationMethod = OutputOrganizationMethod.ByBatch,
                CreateBatchSubdirectories = true,
                CreateIndexFile = true
            },
            NotificationOptions = new NotificationOptions
            {
                NotifyOnCompletion = true,
                NotificationMethod = NotificationMethod.Log
            }
        };

        // 执行
        var result = await batchService.AdvancedBatchGenerateExamPapersAsync(request);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(BatchGenerationStatus.Created, result.Status);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(0, result.CompletedCount);
        Assert.Equal(0, result.FailedCount);

        // 等待批量生成完成
        await Task.Delay(3000); // 等待生成完成

        // 获取进度
        var progress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(progress);
        Assert.Equal(3, progress.TotalCount);
        Assert.True(progress.CompletedCount > 0 || progress.FailedCount > 0);

        // 获取报告
        var report = await batchService.GetBatchGenerationReportAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(report);
        Assert.Equal(result.BatchId, report.BatchId);
        Assert.Equal("高级测试试卷", report.TaskName);
    }

    /// <summary>
    /// 测试暂停和恢复批量生成任务
    /// </summary>
    [Fact]
    public async Task PauseAndResumeBatchGenerationAsync_ShouldWorkCorrectly()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new AdvancedBatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 5, // 较大的数量，确保有时间暂停
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "暂停恢复测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1 // 使用单线程，便于测试暂停
        };

        // 创建批量生成任务
        var result = await batchService.AdvancedBatchGenerateExamPapersAsync(request);

        // 等待一段时间
        await Task.Delay(500);

        // 暂停任务
        var pauseResult = await batchService.PauseBatchGenerationAsync(result.BatchId);
        
        // 断言
        Assert.True(pauseResult);

        // 等待一段时间
        await Task.Delay(1000);

        // 获取进度
        var pausedProgress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(pausedProgress);
        Assert.Equal(BatchGenerationStatus.Paused, pausedProgress.Status);

        // 恢复任务
        var resumeResult = await batchService.ResumeBatchGenerationAsync(result.BatchId);
        
        // 断言
        Assert.True(resumeResult);

        // 等待一段时间
        await Task.Delay(500);

        // 获取进度
        var resumedProgress = await batchService.GetBatchGenerationProgressAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(resumedProgress);
        Assert.Equal(BatchGenerationStatus.InProgress, resumedProgress.Status);
    }

    /// <summary>
    /// 测试文件名规则
    /// </summary>
    [Fact]
    public async Task AdvancedBatchGenerateExamPapersAsync_ShouldRespectFileNameRule()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new AdvancedBatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 2,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "文件名测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1,
            FileNameRule = new FileNameRule
            {
                Template = "{prefix}_{date}_{index:D3}",
                IncludeDate = true,
                DateFormat = "yyyyMMdd"
            }
        };

        // 执行
        var result = await batchService.AdvancedBatchGenerateExamPapersAsync(request);

        // 等待批量生成完成
        await Task.Delay(2000);

        // 获取报告
        var report = await batchService.GetBatchGenerationReportAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(report);
        Assert.Equal(2, report.GeneratedFiles.Count);
        
        // 检查文件名是否符合规则
        var expectedDate = DateTime.Now.ToString("yyyyMMdd");
        foreach (var file in report.GeneratedFiles)
        {
            Assert.Contains(expectedDate, file.FileName);
            Assert.Contains("文件名测试", file.FileName);
        }
    }

    /// <summary>
    /// 测试输出组织选项
    /// </summary>
    [Fact]
    public async Task AdvancedBatchGenerateExamPapersAsync_ShouldOrganizeOutputCorrectly()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new AdvancedBatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 2,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "组织测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1,
            OutputOptions = new OutputOrganizationOptions
            {
                OrganizationMethod = OutputOrganizationMethod.ByBatch,
                CreateBatchSubdirectories = true
            }
        };

        // 执行
        var result = await batchService.AdvancedBatchGenerateExamPapersAsync(request);

        // 等待批量生成完成
        await Task.Delay(2000);

        // 获取报告
        var report = await batchService.GetBatchGenerationReportAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(report);
        Assert.NotEqual(_testTempDirectory, report.OutputDirectory);
        Assert.Contains("Batch_", report.OutputDirectory);
    }

    /// <summary>
    /// 测试高级批量生成选项
    /// </summary>
    [Fact]
    public async Task AdvancedBatchGenerateExamPapersAsync_ShouldRespectAdvancedOptions()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var request = new AdvancedBatchGenerationRequest
        {
            QuestionBankIds = new List<Guid> { Guid.NewGuid() },
            TemplateId = Guid.NewGuid(),
            Count = 2,
            OutputDirectory = _testTempDirectory,
            FileNamePrefix = "高级选项测试",
            Format = ExportFormat.PDF,
            ParallelCount = 1,
            Options = new AdvancedBatchGenerationOptions
            {
                VaryQuestionCombination = true,
                QuestionVariationRate = 0.5,
                VaryQuestionOrder = true,
                VaryOptionOrder = true,
                GenerateAnswerPapers = true,
                GenerateStatisticsReport = true
            }
        };

        // 执行
        var result = await batchService.AdvancedBatchGenerateExamPapersAsync(request);

        // 等待批量生成完成
        await Task.Delay(3000);

        // 获取报告
        var report = await batchService.GetBatchGenerationReportAsync(result.BatchId);
        
        // 断言
        Assert.NotNull(report);
        Assert.Equal(2, report.GeneratedFiles.Count);
        Assert.Equal(2, report.AnswerPaperFiles.Count);
        
        // 检查统计信息
        Assert.True(report.Statistics.TotalQuestions > 0);
        Assert.True(report.Statistics.TotalPoints > 0);
        Assert.NotEmpty(report.Statistics.QuestionTypeDistribution);
    }

    /// <summary>
    /// 测试无效的高级批量生成请求
    /// </summary>
    [Fact]
    public async Task AdvancedBatchGenerateExamPapersAsync_ShouldHandleInvalidRequest()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();

        // 执行和断言 - 空题库ID列表
        await Assert.ThrowsAsync<ArgumentException>(() =>
            batchService.AdvancedBatchGenerateExamPapersAsync(new AdvancedBatchGenerationRequest
            {
                QuestionBankIds = new List<Guid>(),
                TemplateId = Guid.NewGuid(),
                Count = 1,
                OutputDirectory = _testTempDirectory,
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));

        // 执行和断言 - 无效的生成数量
        await Assert.ThrowsAsync<ArgumentException>(() =>
            batchService.AdvancedBatchGenerateExamPapersAsync(new AdvancedBatchGenerationRequest
            {
                QuestionBankIds = new List<Guid> { Guid.NewGuid() },
                TemplateId = Guid.NewGuid(),
                Count = 0,
                OutputDirectory = _testTempDirectory,
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));

        // 执行和断言 - 空输出目录
        await Assert.ThrowsAsync<ArgumentException>(() =>
            batchService.AdvancedBatchGenerateExamPapersAsync(new AdvancedBatchGenerationRequest
            {
                QuestionBankIds = new List<Guid> { Guid.NewGuid() },
                TemplateId = Guid.NewGuid(),
                Count = 1,
                OutputDirectory = "",
                FileNamePrefix = "无效测试",
                Format = ExportFormat.PDF
            }));
    }

    /// <summary>
    /// 测试获取不存在的高级批量生成报告
    /// </summary>
    [Fact]
    public async Task GetBatchGenerationReportAsync_ShouldHandleNonExistentTask()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var nonExistentId = Guid.NewGuid();

        // 执行和断言
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            batchService.GetBatchGenerationReportAsync(nonExistentId));
    }

    /// <summary>
    /// 测试暂停不存在的高级批量生成任务
    /// </summary>
    [Fact]
    public async Task PauseBatchGenerationAsync_ShouldHandleNonExistentTask()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var nonExistentId = Guid.NewGuid();

        // 执行
        var result = await batchService.PauseBatchGenerationAsync(nonExistentId);

        // 断言
        Assert.False(result);
    }

    /// <summary>
    /// 测试恢复不存在的高级批量生成任务
    /// </summary>
    [Fact]
    public async Task ResumeBatchGenerationAsync_ShouldHandleNonExistentTask()
    {
        // 准备
        var batchService = _serviceProvider.GetRequiredService<IBatchGenerationService>();
        var nonExistentId = Guid.NewGuid();

        // 执行
        var result = await batchService.ResumeBatchGenerationAsync(nonExistentId);

        // 断言
        Assert.False(result);
    }

    #endregion
}