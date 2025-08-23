# QuizForge CLI 测试覆盖率要求

## 概述

本文档定义了QuizForge CLI项目的测试覆盖率要求，目标是达到90%以上的代码覆盖率，确保代码质量和功能正确性。

## 测试覆盖率目标

### 整体目标
- **单元测试覆盖率**: ≥ 90%
- **集成测试覆盖率**: ≥ 80%
- **端到端测试覆盖率**: ≥ 70%
- **总体测试覆盖率**: ≥ 85%

### 分层目标
- **CLI层**: ≥ 95%
- **服务层**: ≥ 90%
- **业务逻辑层**: ≥ 90%
- **基础设施层**: ≥ 85%
- **数据访问层**: ≥ 85%

## 测试类型和范围

### UNIT-001: 单元测试

#### 覆盖范围
- 所有公共方法
- 所有私有方法（通过公共方法测试）
- 所有属性
- 所有异常路径
- 所有边界条件

#### 测试框架
- **主要框架**: xUnit
- **Mock框架**: Moq
- **断言框架**: FluentAssertions
- **覆盖率工具**: Coverlet

#### 测试标准
```csharp
// 示例：单元测试结构
public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _mockRepository;
    private readonly QuestionService _service;

    public QuestionServiceTests()
    {
        _mockRepository = new Mock<IQuestionRepository>();
        _service = new QuestionService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetQuestionsAsync_WithValidBankId_ReturnsQuestions()
    {
        // Arrange
        var bankId = 1;
        var expectedQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Test Question 1" },
            new Question { Id = 2, Content = "Test Question 2" }
        };
        
        _mockRepository.Setup(r => r.GetByBankIdAsync(bankId))
            .ReturnsAsync(expectedQuestions);

        // Act
        var result = await _service.GetQuestionsAsync(bankId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedQuestions);
    }
}
```

### UNIT-002: 集成测试

#### 覆盖范围
- CLI命令测试
- 文件处理集成
- 数据库操作集成
- 外部服务集成

#### 测试环境
- 内存数据库（SQLite）
- 临时文件系统
- Mock外部服务
- 隔离测试环境

#### 测试标准
```csharp
// 示例：集成测试结构
public class GenerateCommandIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _testDataPath;
    private readonly string _outputPath;

    public GenerateCommandIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _testDataPath = Path.Combine("TestData", "ExcelFiles");
        _outputPath = Path.Combine("TestOutput", "GeneratedFiles");
        Directory.CreateDirectory(_outputPath);
    }

    [Fact]
    public async Task GenerateCommand_WithValidExcelFile_GeneratesPdf()
    {
        // Arrange
        var inputFile = Path.Combine(_testDataPath, "valid-questions.xlsx");
        var outputFile = Path.Combine(_outputPath, "test-output.pdf");
        
        // Act
        var result = await Program.Main(new[] { "generate", "--input", inputFile, "--output", outputFile });

        // Assert
        result.Should().Be(0);
        File.Exists(outputFile).Should().BeTrue();
        new FileInfo(outputFile).Length.Should().BeGreaterThan(0);
    }
}
```

### UNIT-003: 端到端测试

#### 覆盖范围
- 完整的用户工作流
- 实际文件处理
- 真实LaTeX编译
- 错误处理场景

#### 测试数据
- 真实的Excel文件
- 各种边界情况文件
- 损坏文件测试
- 大文件测试

#### 测试标准
```csharp
// 示例：端到端测试结构
public class EndToEndTests
{
    private readonly ITestOutputHelper _output;
    private readonly TestContext _testContext;

    public EndToEndTests(ITestOutputHelper output)
    {
        _output = output;
        _testContext = new TestContext();
    }

    [Fact]
    public async Task CompleteWorkflow_ExcelToPdf_GeneratesValidDocument()
    {
        // Arrange
        var testFile = Path.Combine("TestData", "E2E", "complete-workflow.xlsx");
        var outputFile = Path.Combine("TestOutput", "E2E", "workflow-result.pdf");
        
        // Act
        var exitCode = await RunQuizForgeCli(new[] {
            "generate",
            "--input", testFile,
            "--output", outputFile,
            "--title", "E2E Test",
            "--verbose"
        });

        // Assert
        exitCode.Should().Be(0);
        File.Exists(outputFile).Should().BeTrue();
        
        // 验证PDF内容
        var pdfContent = ExtractPdfText(outputFile);
        pdfContent.Should().Contain("E2E Test");
        pdfContent.Should().Contain("Question 1");
        pdfContent.Should().Contain("Question 2");
    }
}
```

## 覆盖率工具配置

### TOOL-001: Coverlet配置

#### 配置文件
```xml
<!-- coverlet.runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>opencover</Format>
          <Include>[QuizForge]*</Include>
          <Exclude>[*Tests]*</Exclude>
          <Exclude>[*]Microsoft.*</Exclude>
          <Exclude>[*]System.*</Exclude>
          <SingleHit>false</SingleHit>
          <UseSourceLink>true</UseSourceLink>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

#### 运行命令
```bash
# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# 生成HTML报告
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory TestResults
reportgenerator -reports:TestResults/coverage.xml -targetdir:TestResults/Report -reporttypes:Html
```

### TOOL-002: 持续集成配置

#### GitHub Actions配置
```yaml
# .github/workflows/test.yml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run tests with coverage
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./TestResults/coverage.xml
```

## 测试数据管理

### DATA-001: 测试数据结构

#### 目录结构
```
tests/
├── QuizForge.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   ├── Commands/
│   │   └── Infrastructure/
│   ├── Integration/
│   │   ├── CLI/
│   │   ├── FileProcessing/
│   │   └── Database/
│   ├── EndToEnd/
│   │   ├── Workflows/
│   │   └── Scenarios/
│   └── TestData/
│       ├── ExcelFiles/
│       ├── ConfigFiles/
│       ├── Templates/
│       └── ExpectedOutputs/
```

#### 测试数据文件
- **标准Excel文件**: 包含所有题型的标准格式文件
- **边界情况文件**: 空文件、单行文件、最大行数文件
- **错误文件**: 格式错误、数据错误、损坏文件
- **配置文件**: 各种配置组合的测试文件

### DATA-002: 测试数据生成

#### 自动生成测试数据
```csharp
public class TestDataGenerator
{
    public static QuestionBank GenerateStandardQuestionBank()
    {
        return new QuestionBank
        {
            Id = 1,
            Name = "Test Question Bank",
            Questions = new List<Question>
            {
                new Question
                {
                    Id = 1,
                    Type = QuestionType.SingleChoice,
                    Content = "What is the capital of France?",
                    Options = new List<string> { "London", "Berlin", "Paris", "Madrid" },
                    CorrectAnswer = "C",
                    Difficulty = Difficulty.Easy,
                    Points = 5
                },
                // 更多题目...
            }
        };
    }

    public static void GenerateExcelFile(string filePath, QuestionBank questionBank)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Questions");
            
            // 添加标题行
            worksheet.Cell(1, 1).Value = "题型";
            worksheet.Cell(1, 2).Value = "题目";
            worksheet.Cell(1, 3).Value = "选项A";
            worksheet.Cell(1, 4).Value = "选项B";
            worksheet.Cell(1, 5).Value = "选项C";
            worksheet.Cell(1, 6).Value = "选项D";
            worksheet.Cell(1, 7).Value = "答案";
            worksheet.Cell(1, 8).Value = "难度";
            worksheet.Cell(1, 9).Value = "分值";
            
            // 添加数据行
            var row = 2;
            foreach (var question in questionBank.Questions)
            {
                worksheet.Cell(row, 1).Value = question.Type.ToString();
                worksheet.Cell(row, 2).Value = question.Content;
                if (question.Options != null && question.Options.Count >= 4)
                {
                    worksheet.Cell(row, 3).Value = question.Options[0];
                    worksheet.Cell(row, 4).Value = question.Options[1];
                    worksheet.Cell(row, 5).Value = question.Options[2];
                    worksheet.Cell(row, 6).Value = question.Options[3];
                }
                worksheet.Cell(row, 7).Value = question.CorrectAnswer;
                worksheet.Cell(row, 8).Value = question.Difficulty.ToString();
                worksheet.Cell(row, 9).Value = question.Points;
                row++;
            }
            
            workbook.SaveAs(filePath);
        }
    }
}
```

## 测试覆盖率监控

### MON-001: 覆盖率监控

#### 实时监控
- 每次代码提交自动运行测试
- 生成覆盖率报告
- 与覆盖率目标比较
- 发送覆盖率变化通知

#### 报告生成
- 详细的HTML覆盖率报告
- 按模块和文件分组
- 趋势分析
- 覆盖率阈值警告

### MON-002: 质量门禁

#### 覆盖率门槛
- **单元测试**: 90%（最低85%）
- **集成测试**: 80%（最低75%）
- **端到端测试**: 70%（最低65%）
- **总体覆盖率**: 85%（最低80%）

#### 质量检查流程
```yaml
# 质量检查脚本示例
name: Quality Gate

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  quality-check:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Run tests with coverage
      run: |
        dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
    
    - name: Check coverage thresholds
      run: |
        if [ $(cat TestResults/coverage.xml | grep -o '<line-rate>[0-9.]*' | grep -o '[0-9.]*' | head -1 | awk '{print ($1 >= 0.90)}') -eq 0 ]; then
          echo "Unit test coverage below 90% threshold"
          exit 1
        fi
```

## 测试最佳实践

### BEST-001: 测试编写指南

#### 命名约定
- **测试类**: `{ClassName}Tests`
- **测试方法**: `{Scenario}_ExpectedBehavior`
- **测试数据**: `Given_When_Then`格式

#### 测试结构
```csharp
public class ExampleTests
{
    [Fact]
    public void Method_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange - 准备测试数据
        var service = new Service();
        var input = "test input";
        
        // Act - 执行测试操作
        var result = service.Method(input);
        
        // Assert - 验证结果
        result.Should().Be("expected result");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Method_WithInvalidInput_ThrowsArgumentException(string invalidInput)
    {
        // Arrange
        var service = new Service();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Method(invalidInput));
    }
}
```

### BEST-002: Mock使用指南

#### Mock原则
- 只mock外部依赖
- 不要mock实现细节
- 验证mock调用
- 设置合理的返回值

#### Mock示例
```csharp
public class ServiceTests
{
    [Fact]
    public async Task ProcessDataAsync_WithValidData_CallsRepositoryAndReturnsResult()
    {
        // Arrange
        var mockRepository = new Mock<IRepository>();
        var service = new Service(mockRepository.Object);
        
        var inputData = new InputData { Id = 1, Value = "test" };
        var expectedOutput = new OutputData { Id = 1, Result = "processed" };
        
        mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<InputData>()))
            .ReturnsAsync(expectedOutput);
        
        // Act
        var result = await service.ProcessDataAsync(inputData);
        
        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
        mockRepository.Verify(r => r.SaveAsync(inputData), Times.Once);
    }
}
```

## 测试覆盖率报告

### REPORT-001: 报告格式

#### 覆盖率指标
- **行覆盖率**: 执行的代码行百分比
- **分支覆盖率**: 执行的代码分支百分比
- **方法覆盖率**: 执行的方法百分比
- **类覆盖率**: 执行的类百分比

#### 报告内容
- 总体覆盖率统计
- 按模块分组统计
- 未覆盖代码标记
- 覆盖率趋势图
- 改进建议

### REPORT-002: 定期报告

#### 报告频率
- **每日报告**: 自动生成覆盖率报告
- **每周报告**: 覆盖率变化分析
- **每月报告**: 质量趋势分析
- **发布报告**: 发布版本覆盖率报告

#### 报告分发
- 开发团队邮件通知
- 项目仪表板显示
- 管理层定期汇报
- 质量会议讨论

## 持续改进

### IMP-001: 覆盖率提升

#### 提升策略
- 识别低覆盖率模块
- 优先测试核心功能
- 增加边界条件测试
- 完善错误处理测试

#### 改进流程
1. **分析**: 分析覆盖率报告
2. **计划**: 制定提升计划
3. **实施**: 编写测试用例
4. **验证**: 验证覆盖率提升
5. **监控**: 持续监控覆盖率

### IMP-002: 测试质量提升

#### 质量指标
- 测试用例有效性
- 测试数据质量
- 测试执行稳定性
- 测试维护成本

#### 改进措施
- 测试审查流程
- 测试重构规范
- 测试数据管理
- 测试文档维护

## 总结

本测试覆盖率要求文档为QuizForge CLI项目提供了全面的测试策略和实施指南。通过90%以上的测试覆盖率目标，结合完善的测试工具、流程和最佳实践，我们将确保代码质量和功能正确性。

关键成功因素：
- 高层管理支持
- 团队测试意识
- 充分的测试资源
- 持续的改进优化

通过实施本测试策略，我们将建立一个高质量、高可靠性的QuizForge CLI系统。