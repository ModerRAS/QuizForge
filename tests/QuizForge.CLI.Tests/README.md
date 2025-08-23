# QuizForge CLI 测试套件

这是 QuizForge CLI 版本的完整测试套件，包含单元测试、集成测试、端到端测试和性能测试。

## 📁 目录结构

```
tests/QuizForge.CLI.Tests/
├── QuizForge.CLI.Tests.csproj          # 测试项目文件
├── appsettings.test.json                # 测试配置文件
├── TestRunner.cs                        # 简单测试运行器
├── run_tests.sh                         # Linux/macOS 测试脚本
├── run_tests.bat                        # Windows 测试脚本
├── Unit/                                # 单元测试
│   ├── Services/                        # 服务层测试
│   │   ├── CliGenerationServiceTests.cs
│   │   ├── CliFileServiceTests.cs
│   │   └── CliValidationServiceTests.cs
│   └── Commands/                        # 命令层测试
│       ├── GenerationCommandsTests.cs
│       ├── TemplateCommandsTests.cs
│       └── ConfigCommandsTests.cs
├── Integration/                          # 集成测试
│   ├── CommandExecution/
│   │   └── CommandExecutionIntegrationTests.cs
│   └── ServiceIntegration/
│       └── ServiceIntegrationTests.cs
├── E2E/                                 # 端到端测试
│   └── ExcelToPdfWorkflowTests.cs
├── Performance/                         # 性能测试
│   └── BatchProcessingPerformanceTests.cs
├── MockData/                            # 测试数据
│   ├── Excel/
│   ├── Markdown/
│   ├── LaTeX/
│   └── Config/
├── Fixtures/                            # 测试夹具
│   ├── Templates/
│   │   └── standard.tex
│   ├── TestBase.cs
│   └── MockServices.cs
└── TestResults/                         # 测试结果输出目录
```

## 🚀 快速开始

### 前置条件

- .NET 8.0 SDK 或更高版本
- 可选：ReportGenerator（用于生成覆盖率报告）

### 安装依赖

```bash
# 还原 NuGet 包
dotnet restore

# 安装 ReportGenerator（可选）
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### 运行测试

#### 使用脚本运行（推荐）

**Linux/macOS:**
```bash
./run_tests.sh
```

**Windows:**
```cmd
run_tests.bat
```

#### 手动运行

```bash
# 运行所有测试
dotnet test

# 运行特定类型的测试
dotnet test --filter "FullyQualifiedName~Unit"
dotnet test --filter "FullyQualifiedName~Integration"
dotnet test --filter "FullyQualifiedName~E2E"
dotnet test --filter "FullyQualifiedName~Performance"

# 运行特定测试方法
dotnet test --filter "TestName"
```

#### 使用简单测试运行器

```bash
# 编译并运行测试运行器
dotnet build
dotnet run --project QuizForge.CLI.Tests --configuration Release
```

## 📊 测试覆盖率

测试套件配置了 90% 的代码覆盖率目标。覆盖率报告将自动生成在 `TestResults/coverage-report/` 目录中。

### 查看覆盖率报告

```bash
# 打开 HTML 覆盖率报告
open TestResults/coverage-report/index.html
```

## 🔧 测试配置

### 配置文件

测试使用 `appsettings.test.json` 配置文件，包含：

- 测试特定的路径配置
- Mock 服务配置
- 测试数据路径
- 性能测试设置

### 环境变量

可以通过环境变量覆盖配置：

```bash
export TestSettings__UseMockServices=true
export TestSettings__GenerateTestReports=true
export TestSettings__CleanTestFiles=true
```

## 📋 测试分类

### 单元测试 (Unit Tests)
- **目标**: 测试单个组件的独立功能
- **覆盖范围**:
  - `CliGenerationService` - 生成服务核心逻辑
  - `CliFileService` - 文件操作服务
  - `CliValidationService` - 验证服务
  - 所有 CLI 命令类
- **Mock**: 使用 Moq 和 NSubstitute 框架
- **隔离**: 完全隔离外部依赖

### 集成测试 (Integration Tests)
- **目标**: 测试服务间的交互
- **覆盖范围**:
  - 命令执行完整流程
  - 服务间依赖注入
  - 配置系统集成
  - 错误处理传播
- **环境**: 使用真实的依赖注入容器
- **数据**: 使用内存中的测试数据

### 端到端测试 (E2E Tests)
- **目标**: 测试完整的用户工作流
- **覆盖范围**:
  - Excel 到 PDF 完整流程
  - 批量处理流程
  - 模板管理流程
  - 配置管理流程
- **环境**: 接近生产环境的设置
- **验证**: 验证实际文件输出

### 性能测试 (Performance Tests)
- **目标**: 测试系统性能和资源使用
- **覆盖范围**:
  - 批量处理性能
  - 并发处理能力
  - 内存使用情况
  - 执行时间测量
- **工具**: 使用 BenchmarkDotNet
- **指标**: 吞吐量、延迟、内存分配

## 🎯 测试覆盖的功能

### 核心服务
- ✅ Excel 文件解析和验证
- ✅ Markdown 文件解析和验证
- ✅ LaTeX 模板处理
- ✅ PDF 生成
- ✅ 文件操作和目录管理
- ✅ 配置管理
- ✅ 进度显示

### CLI 命令
- ✅ `generate excel` - Excel 生成命令
- ✅ `generate markdown` - Markdown 生成命令
- ✅ `batch` - 批量处理命令
- ✅ `validate` - 文件验证命令
- ✅ `template list/create/delete` - 模板管理命令
- ✅ `config show/set/reset` - 配置管理命令

### 工作流
- ✅ 完整的 Excel 到 PDF 生成流程
- ✅ 批量文件处理
- ✅ 错误处理和恢复
- ✅ 资源清理
- ✅ 并发处理

## 🔍 测试数据

### Mock 数据生成
测试套件使用 `TestDataGenerator` 类生成标准化的测试数据：

- 题目数据（不同类型和难度）
- 题库数据
- 试卷数据
- 模板数据

### 测试文件
- Excel 文件：模拟真实题库格式
- Markdown 文件：模拟 Markdown 题库格式
- LaTeX 模板：标准试卷模板

## 🐛 调试测试

### 启用详细日志
```bash
dotnet test --logger "console;verbosity=detailed"
```

### 调试特定测试
```bash
# 在 VS Code 或 Visual Studio 中设置断点
# 使用调试器运行测试
dotnet test --filter "TestName" --logger "console"
```

### 查看测试输出
```bash
# 保存测试输出到文件
dotnet test --logger "console;logfile=test_output.log"
```

## 📈 性能基准

测试套件包含性能基准测试，可以：

- 测量批量处理性能
- 比较不同并行度的性能
- 监控内存使用情况
- 识别性能瓶颈

运行性能测试：
```bash
dotnet test --configuration Release --filter "FullyQualifiedName~Performance"
```

## 🔄 CI/CD 集成

测试套件设计为可以轻松集成到 CI/CD 流程中：

### GitHub Actions
```yaml
- name: Run Tests
  run: |
    cd tests/QuizForge.CLI.Tests
    ./run_tests.sh
    
- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    file: ./tests/QuizForge.CLI.Tests/TestResults/coverage.cobertura.xml
```

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'tests/QuizForge.CLI.Tests/*.csproj'
    arguments: '--configuration Release --collect:"XPlat Code Coverage"'
    
- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

## 🤝 贡献指南

### 添加新测试
1. 在相应的目录创建测试文件
2. 遵循现有测试命名约定
3. 使用 `TestBase` 作为基类
4. 添加适当的测试数据
5. 确保测试覆盖率

### 测试命名约定
- 测试类：`{ServiceName}Tests`
- 测试方法：`{Scenario}_{ExpectedResult}`
- 测试数据：`CreateTest{DataType}()`

### 最佳实践
- 使用 AAA 模式（Arrange-Act-Assert）
- 保持测试独立性和可重复性
- 使用 Mock 隔离外部依赖
- 添加有意义的断言
- 包含边界条件测试

## 📚 相关文档

- [QuizForge 项目文档](../../docs/)
- [CLI 使用说明](../../src/QuizForge.CLI/README.md)
- [架构设计](../../architecture-design.md)

## 🐛 问题报告

如果发现测试问题，请：

1. 检查是否为已知问题
2. 提供详细的复现步骤
3. 包含错误日志和堆栈跟踪
4. 说明运行环境信息

---

*本测试套件旨在确保 QuizForge CLI 版本的质量和稳定性。*