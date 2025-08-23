using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using QuizForge.CLI.Services;
using QuizForge.CLI.Models;

namespace QuizForge.CLI.Tests.Unit.Services;

[TestClass]
public class CliFileServiceTests : TestBase
{
    private ICliFileService _fileService = null!;
    private Mock<ILogger<CliFileService>> _mockLogger = null!;
    private CliOptions _testOptions = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CliFileService>>();
        _testOptions = new CliOptions 
        { 
            AutoCreateDirectories = true,
            DefaultOutputDirectory = TestTempPath
        };

        // 创建测试服务
        _fileService = new CliFileService(
            _mockLogger.Object,
            Options.Create(_testOptions)
        );
    }

    [TestCleanup]
    public void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public async Task ValidateFileAsync_ExistingFile_ReturnsValidResult()
    {
        // Arrange
        var testFile = CreateTestExcelFile("valid.xlsx");

        // Act
        var result = await _fileService.ValidateFileAsync(testFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateFileAsync_NonExistingFile_ReturnsInvalidResult()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");

        // Act
        var result = await _fileService.ValidateFileAsync(nonExistentFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("文件不存在");
    }

    [TestMethod]
    public async Task ValidateFileAsync_EmptyFileName_ReturnsInvalidResult()
    {
        // Arrange
        var emptyFileName = "";

        // Act
        var result = await _fileService.ValidateFileAsync(emptyFileName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("文件名不能为空");
    }

    [TestMethod]
    public async Task ValidateFileAsync_TooLargeFile_ReturnsInvalidResult()
    {
        // Arrange
        var testFile = Path.Combine(TestTempPath, "large.xlsx");
        var largeContent = new byte[11 * 1024 * 1024]; // 11MB
        await File.WriteAllBytesAsync(testFile, largeContent);

        // Act
        var result = await _fileService.ValidateFileAsync(testFile);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("文件大小超过限制");
    }

    [TestMethod]
    public async Task GetFilesAsync_ExistingDirectory_ReturnsFileList()
    {
        // Arrange
        var testDir = Path.Combine(TestTempPath, "testdir");
        Directory.CreateDirectory(testDir);
        
        // 创建测试文件
        var testFiles = new List<string>
        {
            Path.Combine(testDir, "file1.xlsx"),
            Path.Combine(testDir, "file2.md"),
            Path.Combine(testDir, "file3.xlsx")
        };
        
        foreach (var file in testFiles)
        {
            await File.WriteAllTextAsync(file, "test content");
        }

        // Act
        var result = await _fileService.GetFilesAsync(testDir, "*.xlsx");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(f => f.Name).Should().Contain("file1.xlsx");
        result.Select(f => f.Name).Should().Contain("file3.xlsx");
        result.Select(f => f.Name).Should().NotContain("file2.md");
    }

    [TestMethod]
    public async Task GetFilesAsync_NonExistingDirectory_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentDir = Path.Combine(TestTempPath, "nonexistent");

        // Act
        var result = await _fileService.GetFilesAsync(nonExistentDir, "*.*");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetFilesAsync_NoMatchingFiles_ReturnsEmptyList()
    {
        // Arrange
        var testDir = Path.Combine(TestTempPath, "testdir");
        Directory.CreateDirectory(testDir);
        
        // 创建不匹配的文件
        await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "test content");
        await File.WriteAllTextAsync(Path.Combine(testDir, "file2.md"), "test content");

        // Act
        var result = await _fileService.GetFilesAsync(testDir, "*.xlsx");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task EnsureDirectoryExistsAsync_ExistingDirectory_DoesNothing()
    {
        // Arrange
        var testDir = Path.Combine(TestTempPath, "existing");
        Directory.CreateDirectory(testDir);

        // Act
        await _fileService.EnsureDirectoryExistsAsync(testDir);

        // Assert
        Directory.Exists(testDir).Should().BeTrue();
    }

    [TestMethod]
    public async Task EnsureDirectoryExistsAsync_NonExistingDirectory_CreatesDirectory()
    {
        // Arrange
        var testDir = Path.Combine(TestTempPath, "newdir");

        // Act
        await _fileService.EnsureDirectoryExistsAsync(testDir);

        // Assert
        Directory.Exists(testDir).Should().BeTrue();
    }

    [TestMethod]
    public async Task GetFileInfoAsync_ExistingFile_ReturnsFileInfo()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        var content = "test content";
        await File.WriteAllTextAsync(testFile, content);

        // Act
        var result = await _fileService.GetFileInfoAsync(testFile);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("test.xlsx");
        result.FullName.Should().Be(testFile);
        result.Length.Should().Be(content.Length);
        result.Exists.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetFileInfoAsync_NonExistingFile_ReturnsNull()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");

        // Act
        var result = await _fileService.GetFileInfoAsync(nonExistentFile);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteFileAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        var testFile = CreateTestExcelFile("test.xlsx");
        File.Exists(testFile).Should().BeTrue();

        // Act
        await _fileService.DeleteFileAsync(testFile);

        // Assert
        File.Exists(testFile).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteFileAsync_NonExistingFile_DoesNothing()
    {
        // Arrange
        var nonExistentFile = Path.Combine(TestTempPath, "nonexistent.xlsx");
        File.Exists(nonExistentFile).Should().BeFalse();

        // Act
        await _fileService.DeleteFileAsync(nonExistentFile);

        // Assert
        File.Exists(nonExistentFile).Should().BeFalse();
    }

    [TestMethod]
    public async Task CopyFileAsync_ValidSourceAndDestination_CopiesFile()
    {
        // Arrange
        var sourceFile = CreateTestExcelFile("source.xlsx");
        var destFile = Path.Combine(TestTempPath, "destination.xlsx");
        var content = "test content";
        await File.WriteAllTextAsync(sourceFile, content);

        // Act
        var result = await _fileService.CopyFileAsync(sourceFile, destFile);

        // Assert
        result.Should().BeTrue();
        File.Exists(destFile).Should().BeTrue();
        var destContent = await File.ReadAllTextAsync(destFile);
        destContent.Should().Be(content);
    }

    [TestMethod]
    public async Task CopyFileAsync_NonExistingSource_ReturnsFalse()
    {
        // Arrange
        var nonExistentSource = Path.Combine(TestTempPath, "nonexistent.xlsx");
        var destFile = Path.Combine(TestTempPath, "destination.xlsx");

        // Act
        var result = await _fileService.CopyFileAsync(nonExistentSource, destFile);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task CopyFileAsync_WithOverwrite_OverwritesExistingFile()
    {
        // Arrange
        var sourceFile = CreateTestExcelFile("source.xlsx");
        var destFile = CreateTestExcelFile("destination.xlsx");
        var sourceContent = "source content";
        var destContent = "destination content";
        await File.WriteAllTextAsync(sourceFile, sourceContent);
        await File.WriteAllTextAsync(destFile, destContent);

        // Act
        var result = await _fileService.CopyFileAsync(sourceFile, destFile, overwrite: true);

        // Assert
        result.Should().BeTrue();
        var finalContent = await File.ReadAllTextAsync(destFile);
        finalContent.Should().Be(sourceContent);
    }

    [TestMethod]
    public async Task CleanupTempFilesAsync_OldFiles_CleansUpFiles()
    {
        // Arrange
        var tempDir = Path.Combine(TestTempPath, "temp");
        Directory.CreateDirectory(tempDir);
        
        // 创建旧文件
        var oldFile = Path.Combine(tempDir, "old.tmp");
        await File.WriteAllTextAsync(oldFile, "old content");
        
        // 修改文件时间为2小时前
        var oldFileInfo = new FileInfo(oldFile);
        oldFileInfo.LastWriteTime = DateTime.Now.AddHours(-2);

        // 创建新文件
        var newFile = Path.Combine(tempDir, "new.tmp");
        await File.WriteAllTextAsync(newFile, "new content");

        // Act
        var cleanedCount = await _fileService.CleanupTempFilesAsync(tempDir, TimeSpan.FromHours(1));

        // Assert
        cleanedCount.Should().Be(1);
        File.Exists(oldFile).Should().BeFalse();
        File.Exists(newFile).Should().BeTrue();
    }

    [TestMethod]
    public async Task CleanupTempFilesAsync_NonExistingDirectory_ReturnsZero()
    {
        // Arrange
        var nonExistentDir = Path.Combine(TestTempPath, "nonexistent");

        // Act
        var cleanedCount = await _fileService.CleanupTempFilesAsync(nonExistentDir);

        // Assert
        cleanedCount.Should().Be(0);
    }
}