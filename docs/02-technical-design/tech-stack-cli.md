# QuizForge CLI 技术栈选择

## 技术栈概览

QuizForge CLI 基于现有QuizForge项目架构，采用现代化的.NET技术栈，充分利用现有组件的同时，为CLI场景进行优化。技术栈选择考虑了性能、可维护性、跨平台支持和开发效率。

## 核心技术栈

### 1. 运行时和框架

#### .NET 8.0
**选择理由:**
- **长期支持(LTS)**: .NET 8是LTS版本，提供长期支持
- **性能优化**: 相比.NET 7有显著的性能提升
- **跨平台支持**: 原生支持Windows、macOS、Linux
- **现代化特性**: 支持最新的C#语言特性和API
- **现有兼容**: 与现有QuizForge项目完全兼容

**主要特性:**
- Native AOT编译支持
- 改进的异步编程模型
- 更好的内存管理和垃圾回收
- 内置的依赖注入容器
- 强大的配置系统

#### C# 12.0
**选择理由:**
- **现代化语法**: 提供更简洁、更安全的代码编写方式
- **性能优化**: 编译时优化和运行时性能提升
- **团队熟悉**: 开发团队对C#有丰富的经验
- **生态系统**: 拥有丰富的第三方库和工具支持

**使用特性:**
- 主构造函数
- 集合表达式
- 内联数组
- 可选的lambda参数
- ref readonly参数
- 别名任意类型

### 2. 命令行框架

#### Microsoft.Extensions.CommandLineUtils
**选择理由:**
- **官方支持**: 微软官方维护，稳定性有保障
- **功能完整**: 提供完整的命令行解析功能
- **依赖注入**: 与Microsoft.Extensions.DependencyInjection完美集成
- **配置支持**: 与Microsoft.Extensions.Configuration无缝集成
- **文档丰富**: 拥有详细的文档和示例

**主要功能:**
- 命令和子命令支持
- 参数验证和类型转换
- 帮助信息自动生成
- 版本信息显示
- 彩色输出支持

#### 替代方案考虑:
- **CommandLineParser**: 功能强大但配置复杂
- **McMaster.Extensions.CommandLineUtils**: 功能丰富但不是官方维护
- **System.CommandLine**: 新一代框架但生态不够成熟

### 3. 依赖注入

#### Microsoft.Extensions.DependencyInjection
**选择理由:**
- **官方推荐**: .NET Core官方依赖注入容器
- **性能优秀**: 轻量级且性能优异
- **生命周期管理**: 支持Singleton、Scoped、Transient生命周期
- **集成度高**: 与其他Microsoft.Extensions组件无缝集成
- **易于测试**: 支持模拟和单元测试

**配置示例:**
```csharp
// 创建服务集合
var services = new ServiceCollection();

// 注册服务
services.AddSingleton<ICliService, CliService>();
services.AddSingleton<IFileService, FileService>();
services.AddSingleton<IProgressService, ProgressService>();
services.AddSingleton<IConfigurationService, ConfigurationService>();

// 复用现有服务
services.AddSingleton<IExcelParser, ExcelParser>();
services.AddSingleton<IPdfEngine, LatexPdfEngine>();
services.AddSingleton<IGenerationService, GenerationService>();

// 构建服务提供者
var serviceProvider = services.BuildServiceProvider();
```

### 4. 配置管理

#### Microsoft.Extensions.Configuration
**选择理由:**
- **灵活性**: 支持多种配置源（JSON、XML、环境变量、命令行）
- **层次化**: 支持嵌套配置和配置覆盖
- **热重载**: 支持运行时配置更新
- **类型安全**: 支持强类型配置绑定
- **扩展性**: 易于添加新的配置提供者

**配置源优先级:**
1. 命令行参数
2. 环境变量
3. 用户配置文件
4. 应用配置文件
5. 默认配置

**配置示例:**
```csharp
// 创建配置构建器
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

// 绑定强类型配置
var latexSettings = configuration.GetSection("LatexSettings").Get<LatexSettings>();
var fileSettings = configuration.GetSection("FileSettings").Get<FileSettings>();
```

### 5. 日志记录

#### Serilog
**选择理由:**
- **结构化日志**: 支持结构化日志记录，便于查询和分析
- **多种输出**: 支持控制台、文件、数据库等多种输出方式
- **性能优秀**: 异步日志记录，对性能影响小
- **生态系统**: 拥有丰富的接收器和扩展
- **配置灵活**: 支持复杂的日志配置和过滤

**配置示例:**
```csharp
// 创建日志配置
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/quizforge-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

// 集成到Microsoft.Extensions.Logging
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});
```

## 复用现有组件

### 1. 核心业务逻辑

#### QuizForge.Core
**复用组件:**
- `ExamPaperGenerator`: 试卷生成器
- `ContentGenerator`: 内容生成器
- `LaTeXDocumentGenerator`: LaTeX文档生成器
- `MathFormulaProcessor`: 数学公式处理器
- `QuestionBankProcessor`: 题库处理器

**优势:**
- **功能完整**: 包含完整的试卷生成逻辑
- **测试覆盖**: 已有完整的单元测试
- **性能优化**: 经过性能测试和优化
- **稳定可靠**: 在现有项目中稳定运行

#### QuizForge.Models
**复用模型:**
- `Question`: 题目模型
- `QuestionBank`: 题库模型
- `ExamPaper`: 试卷模型
- `ExamTemplate`: 试卷模板模型
- `HeaderConfig`: 页眉配置模型

### 2. 基础设施

#### QuizForge.Infrastructure
**复用组件:**
- `ExcelParser`: Excel解析器
- `LatexPdfEngine`: LaTeX PDF引擎
- `PdfEngine`: PDF引擎基类
- `BatchGenerationService`: 批量生成服务
- `PdfCacheService`: PDF缓存服务

**优势:**
- **功能稳定**: 在现有项目中经过充分测试
- **性能优化**: 针对大文件处理进行了优化
- **错误处理**: 完善的错误处理机制
- **扩展性强**: 支持多种文件格式和输出格式

### 3. 应用服务

#### QuizForge.Services
**复用服务:**
- `GenerationService`: 生成服务
- `QuestionService`: 题库服务
- `TemplateService`: 模板服务
- `ExportService`: 导出服务

**优势:**
- **业务逻辑完整**: 包含完整的业务逻辑
- **接口设计良好**: 面向接口设计，易于测试和扩展
- **依赖注入**: 支持依赖注入，易于集成

## 新增依赖库

### 1. CLI专用库

#### Spectre.Console
**选择理由:**
- **美观的CLI界面**: 提供现代化的命令行界面
- **进度条**: 支持各种类型的进度条
- **表格**: 支持格式化表格输出
- **图表**: 支持简单的图表显示
- **跨平台**: 支持Windows、macOS、Linux

**主要功能:**
```csharp
// 创建进度条
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var task = ctx.AddTask("[green]处理中[/]");
        for (int i = 0; i <= 100; i++)
        {
            task.Value = i;
            await Task.Delay(50);
        }
    });

// 创建表格
var table = new Table();
table.AddColumn("文件");
table.AddColumn("状态");
table.AddColumn("大小");
table.AddRow("questions.xlsx", "✓ 成功", "2.5 MB");
AnsiConsole.Write(table);
```

#### McMaster.Extensions.Hosting.CommandLine
**选择理由:**
- **Host集成**: 与Microsoft.Extensions.Hosting完美集成
- **生命周期管理**: 支持应用程序生命周期管理
- **依赖注入**: 内置依赖注入支持
- **配置集成**: 与配置系统集成

### 2. 文件处理

#### SixLabors.ImageSharp
**选择理由:**
- **跨平台**: 支持Windows、macOS、Linux
- **性能优秀**: 内存使用效率高
- **功能完整**: 支持多种图像格式和处理功能
- **现代API**: 使用现代C#特性，API设计友好

**主要功能:**
```csharp
// 图像处理
using var image = await Image.LoadAsync(inputPath);
image.Mutate(x => x.Resize(new ResizeOptions
{
    Size = new Size(800, 600),
    Mode = ResizeMode.Max
}));
await image.SaveAsync(outputPath);
```

### 3. 并发处理

#### TPL Dataflow
**选择理由:**
- **高性能**: 专门为数据流处理优化
- **灵活性**: 支持复杂的处理流程
- **内置缓冲**: 支持数据缓冲和流量控制
- **错误处理**: 完善的错误处理机制

**主要功能:**
```csharp
// 创建数据流管道
var transformBlock = new TransformBlock<string, QuestionBank>(async filePath =>
{
    return await excelParser.ParseAsync(filePath);
}, new ExecutionDataflowBlockOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount
});

var actionBlock = new ActionBlock<QuestionBank>(async questionBank =>
{
    var result = await generationService.GenerateExamPaperAsync(questionBank);
    // 处理结果
});

// 连接数据流
transformBlock.LinkTo(actionBlock);
```

## 构建和部署方案

### 1. 构建配置

#### 项目结构
```
QuizForge.CLI/
├── QuizForge.CLI.csproj          # 主项目文件
├── Program.cs                    # 程序入口
├── appsettings.json              # 应用配置
├── Properties/
│   ├── launchSettings.json       # 启动配置
│   └── assemblyInfo.cs           # 程序集信息
├── Controllers/                   # 命令控制器
├── Services/                     # CLI服务
├── Models/                       # CLI模型
├── Interfaces/                   # 接口定义
├── Configuration/                # 配置类
├── Exceptions/                   # 异常定义
└── Events/                      # 事件定义
```

#### 项目配置 (QuizForge.CLI.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>quizforge</AssemblyName>
    <RootNamespace>QuizForge.CLI</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <EnablePublishSingleFile>true</EnablePublishSingleFile>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Spectre.Console" Version="0.49.1" />
    <PackageReference Include="McMaster.Extensions.Hosting.CommandLine" Version="4.1.0" />
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.2" />
    <PackageReference Include="System.Threading.Channels" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\QuizForge.Core\QuizForge.Core.csproj" />
    <ProjectReference Include="..\QuizForge.Infrastructure\QuizForge.Infrastructure.csproj" />
    <ProjectReference Include="..\QuizForge.Services\QuizForge.Services.csproj" />
    <ProjectReference Include="..\QuizForge.Models\QuizForge.Models.csproj" />
  </ItemGroup>

</Project>
```

### 2. 构建脚本

#### build.sh (Linux/macOS)
```bash
#!/bin/bash

# 设置变量
PROJECT="QuizForge.CLI/QuizForge.CLI.csproj"
CONFIGURATION="Release"
OUTPUT_DIR="./publish"

# 清理之前的构建
echo "清理之前的构建..."
dotnet clean -c $CONFIGURATION

# 还原依赖
echo "还原依赖..."
dotnet restore

# 构建项目
echo "构建项目..."
dotnet build -c $CONFIGURATION --no-restore

# 运行测试
echo "运行测试..."
dotnet test --no-restore --verbosity normal

# 发布应用程序
echo "发布应用程序..."
dotnet publish $PROJECT -c $CONFIGURATION -o $OUTPUT_DIR --self-contained true --runtime linux-x64

# 设置执行权限
chmod +x $OUTPUT_DIR/quizforge

echo "构建完成！"
echo "可执行文件: $OUTPUT_DIR/quizforge"
```

#### build.ps1 (Windows)
```powershell
# 设置变量
$PROJECT = "QuizForge.CLI\QuizForge.CLI.csproj"
$CONFIGURATION = "Release"
$OUTPUT_DIR = ".\publish"

# 清理之前的构建
Write-Host "清理之前的构建..." -ForegroundColor Green
dotnet clean -c $CONFIGURATION

# 还原依赖
Write-Host "还原依赖..." -ForegroundColor Green
dotnet restore

# 构建项目
Write-Host "构建项目..." -ForegroundColor Green
dotnet build -c $CONFIGURATION --no-restore

# 运行测试
Write-Host "运行测试..." -ForegroundColor Green
dotnet test --no-restore --verbosity normal

# 发布应用程序
Write-Host "发布应用程序..." -ForegroundColor Green
dotnet publish $PROJECT -c $CONFIGURATION -o $OUTPUT_DIR --self-contained true --runtime win-x64

Write-Host "构建完成！" -ForegroundColor Green
Write-Host "可执行文件: $OUTPUT_DIR\quizforge.exe" -ForegroundColor Green
```

### 3. 部署方案

#### 本地部署
```bash
# 复制可执行文件
cp ./publish/quizforge /usr/local/bin/

# 创建配置目录
mkdir -p ~/.config/quizforge

# 创建默认配置文件
cp appsettings.json ~/.config/quizforge/

# 设置执行权限
chmod +x /usr/local/bin/quizforge

# 验证安装
quizforge --version
```

#### Docker部署
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 复制项目文件
COPY *.csproj ./
COPY ../*.csproj ../
COPY ../*/*.csproj ../*/

# 还原依赖
RUN dotnet restore

# 复制源代码
COPY . .

# 构建和发布
RUN dotnet publish -c Release -o /app/publish

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 安装LaTeX
RUN apt-get update && apt-get install -y \
    texlive-full \
    && rm -rf /var/lib/apt/lists/*

# 复制发布文件
COPY --from=build /app/publish .

# 创建输出目录
RUN mkdir -p /app/output

# 设置环境变量
ENV DOTNET_ENVIRONMENT=Production
ENV QUIZFORGE_OUTPUT_DIR=/app/output

# 暴露卷
VOLUME ["/app/input", "/app/output"]

# 设置入口点
ENTRYPOINT ["dotnet", "QuizForge.CLI.dll"]
```

#### 包管理器部署

##### Homebrew (macOS)
```ruby
# Formula.rb
class Quizforge < Formula
  desc "CLI tool for generating exam papers from question banks"
  homepage "https://github.com/your-org/quizforge"
  url "https://github.com/your-org/quizforge/archive/v1.0.0.tar.gz"
  sha256 "your-sha256-hash"
  license "MIT"

  depends_on "dotnet"
  depends_on "texlive"

  def install
    # 构建项目
    system "dotnet", "publish", "-c", "Release", "-o", "publish"
    
    # 安装可执行文件
    libexec.install Dir["publish/*"]
    bin.install_symlink libexec/"quizforge" => "quizforge"
    
    # 安装配置文件
    etc.install "appsettings.json" => "quizforge.json"
  end

  test do
    # 测试安装
    system "#{bin}/quizforge", "--version"
  end
end
```

##### Chocolatey (Windows)
```xml
<!-- quizforge.nuspec -->
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>quizforge</id>
    <version>1.0.0</version>
    <title>QuizForge CLI</title>
    <authors>Your Name</authors>
    <description>CLI tool for generating exam papers from question banks</description>
    <dependencies>
      <dependency id="dotnet-sdk" version="8.0.0" />
    </dependencies>
  </metadata>
  <files>
    <file src="tools\**" target="tools" />
  </files>
</package>
```

## 开发和测试工具

### 1. 开发工具

#### Visual Studio Code
**推荐扩展:**
- C# Dev Kit
- .NET Runtime Install Tool
- NuGet Package Manager
- GitLens
- Docker
- Remote Development

#### Rider
**优势:**
- 专业的.NET IDE
- 强大的调试功能
- 集成的测试运行器
- 优秀的性能

### 2. 测试框架

#### xUnit
**选择理由:**
- **现代设计**: 支持现代C#特性
- **并行测试**: 支持并行测试执行
- **数据驱动**: 支持数据驱动测试
- **扩展性强**: 易于扩展和自定义

**测试配置:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.6.1" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
  <PackageReference Include="Moq" Version="4.20.69" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

### 3. 代码质量工具

#### .NET Analyzers
**包含的分析器:**
- Microsoft.CodeAnalysis.NetAnalyzers
- SonarAnalyzer.CSharp
- Roslynator.Analyzers
- ErrorProne.NET.CoreAnalyzers

#### 代码格式化
```json
{
  "dotnet_code_quality.enable_all_rules": true,
  "dotnet_style_require_accessibility_modifiers": "always",
  "dotnet_style_operator_placement_when_wrapping": "beginning_of_line",
  "dotnet_style_prefer_auto_properties": true,
  "dotnet_style_readonly_field": true,
  "dotnet_style_object_initializer": true,
  "dotnet_style_collection_initializer": true,
  "dotnet_style_prefer_conditional_expression_over_assignment": true,
  "dotnet_style_prefer_conditional_expression_over_return": true
}
```

## 性能优化

### 1. 编译优化

#### Native AOT
```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

#### 单文件发布
```xml
<PropertyGroup>
  <EnablePublishSingleFile>true</EnablePublishSingleFile>
  <PublishSingleFile>true</PublishSingleFile>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

### 2. 运行时优化

#### 内存池
```csharp
// 使用ArrayPool减少内存分配
var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
try
{
    // 使用缓冲区
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);
}
```

#### 异步编程
```csharp
// 使用ValueTask减少异步开销
public async ValueTask<QuestionBank> ParseQuestionBankAsync(string filePath)
{
    // 异步操作
}
```

## 监控和诊断

### 1. 应用监控

#### OpenTelemetry
```csharp
// 配置OpenTelemetry
services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("QuizForge.CLI")
            .AddConsoleExporter()
            .AddOtlpExporter();
    });
```

### 2. 性能分析

#### BenchmarkDotNet
```csharp
[MemoryDiagnoser]
public class ExcelParserBenchmark
{
    [Benchmark]
    public async Task<QuestionBank> ParseLargeFile()
    {
        var parser = new ExcelParser();
        return await parser.ParseAsync("large_questions.xlsx");
    }
}
```

## 总结

QuizForge CLI 的技术栈选择基于以下原则：

1. **现代化**: 采用最新的.NET 8和C# 12特性
2. **跨平台**: 支持Windows、macOS、Linux
3. **高性能**: 优化编译和运行时性能
4. **可维护性**: 清晰的架构和代码组织
5. **可扩展性**: 易于添加新功能和命令
6. **用户体验**: 美观的CLI界面和友好的错误处理
7. **部署友好**: 支持多种部署方式

该技术栈充分利用了现有QuizForge项目的基础设施，同时为CLI场景进行了专门的优化，确保能够提供高效、稳定、易用的命令行工具。