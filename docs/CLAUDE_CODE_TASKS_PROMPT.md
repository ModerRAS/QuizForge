# QuizForge 项目任务执行指南

## 项目概述

### 项目背景
QuizForge是一个基于.NET的试卷生成系统，旨在为教育机构、教师和考试组织者提供一个高效、灵活的试卷生成解决方案。项目起源于对传统试卷制作流程繁琐、耗时的痛点，通过自动化技术简化试卷生成过程，提高教育工作效率。

### 项目目标
- **核心目标**：实现"自动通过题库生成试卷"的功能，支持多种题库格式（Markdown、Excel等），提供灵活的模板系统，并能生成高质量的PDF试卷文档。
- **技术目标**：采用分层架构设计，确保系统的可维护性、可扩展性和高内聚低耦合。
- **用户体验目标**：提供直观易用的用户界面，支持跨平台操作（Windows、macOS和Linux）。

### 技术栈
- **前端框架**：Avalonia UI 11.0.0+ - 跨平台UI框架
- **后端技术**：.NET 8 - 提供高性能运行时和现代化C#语言特性
- **UI模式**：MVVM (Model-View-ViewModel) - 使用ReactiveUI实现响应式编程
- **文档生成**：LaTeX (MiKTeX或TeX Live) - 专业排版系统，支持中文处理
- **数据处理**：Markdig 0.31.0+、ClosedXML 0.102.1+
- **PDF处理**：PdfSharp或iTextSharp
- **数据库**：SQLite - 轻量级数据库

## 已完成功能

### 1. 数据模型层 (Models)
- **完成状态**：✅ 已完成
- **主要组件**：
  - `Question` - 题目模型，包含题目内容、选项、答案等
  - `QuestionBank` - 题库模型，管理题目集合
  - `ExamPaper` - 试卷模型，包含生成后的试卷内容
  - `ExamTemplate` - 试卷模板模型，定义试卷布局和样式
  - `HeaderConfig` - 页眉配置模型
  - `QuestionOption` - 题目选项模型
  - `TemplateSection` - 模板章节模型
- **技术亮点**：
  - 完整的数据模型设计，支持复杂的试卷结构
  - 灵活的模板配置系统，支持多种纸张尺寸和样式
  - 详细的密封线配置，支持动态位置调整
  - 完善的枚举类型定义，提高代码可读性

### 2. 核心业务逻辑层 (Core)
- **完成状态**：✅ 已完成
- **主要组件**：
  - `ExamPaperGenerator` - 试卷生成器，整合题库解析、内容生成和模板处理
  - `ContentGenerator` - 内容生成器，负责LaTeX内容生成
  - `DynamicContentInserter` - 动态内容插入器，处理模板中的动态内容
  - `LaTeXDocumentGenerator` - LaTeX文档生成器
  - `MathFormulaProcessor` - 数学公式处理器
  - `QuestionBankProcessor` - 题库处理器
  - `TemplateProcessor` - 模板处理器
- **技术亮点**：
  - 模块化设计，各组件职责明确
  - 支持多种试卷生成方式（随机、手动、批量）
  - 灵活的题目选择算法，支持难度和类别分布
  - 完善的LaTeX内容生成系统

### 3. 数据访问层 (Data)
- **完成状态**：✅ 已完成
- **主要组件**：
  - `QuizDbContext` - 数据库上下文
  - `QuestionRepository` - 题库数据访问
  - `TemplateRepository` - 模板数据访问
  - `ConfigRepository` - 配置数据访问
  - `DatabaseService` - 数据库服务
- **技术亮点**：
  - 基于Entity Framework Core的数据访问层
  - 仓储模式实现，提供统一的数据访问接口
  - 依赖注入支持，便于测试和维护
  - 数据库迁移支持，便于版本管理

### 4. 基础设施层 (Infrastructure)
- **完成状态**：✅ 大部分完成
- **主要组件**：
  - `LatexPdfEngine` - LaTeX PDF引擎，依赖外部LaTeX发行版
  - `NativePdfEngine` - 原生PDF引擎（规划中）
  - `MarkdownParser` - Markdown解析器
  - `ExcelParser` - Excel解析器
  - `LatexParser` - LaTeX解析器
  - `FileService` - 文件系统服务
  - `PdfCacheService` - PDF缓存服务
  - `BatchGenerationService` - 批量生成服务
  - `PdfErrorReportingService` - PDF错误报告服务
  - `PrintPreviewService` - 打印预览服务
- **技术亮点**：
  - 多种文件格式解析支持
  - 完整的LaTeX到PDF转换流程
  - PDF缓存机制，提高性能
  - 批量生成服务，支持大规模试卷生成

### 5. 应用服务层 (Services)
- **完成状态**：✅ 大部分完成
- **主要组件**：
  - `QuestionService` - 题库服务实现
  - `TemplateService` - 模板服务实现
  - `GenerationService` - 生成服务实现
  - `ExportService` - 导出服务实现
- **技术亮点**：
  - 完整的服务层实现，提供业务逻辑封装
  - 异步操作支持，提高系统响应性
  - 完善的错误处理机制
  - 服务层依赖注入，便于测试和扩展

### 6. 用户界面层 (App)
- **完成状态**：✅ 基本完成
- **主要组件**：
  - `MainWindow` - 主窗口
  - `QuestionBankView` - 题库管理视图
  - `TemplateView` - 模板管理视图
  - `ExamGenerationView` - 试卷生成视图
  - `PdfPreviewView` - PDF预览视图
  - `MainViewModel` - 主窗口视图模型
  - `QuestionBankViewModel` - 题库管理视图模型
  - `TemplateViewModel` - 模板管理视图模型
  - `ExamGenerationViewModel` - 试卷生成视图模型
  - `PdfPreviewViewModel` - PDF预览视图模型
- **技术亮点**：
  - 基于Avalonia UI的跨平台界面
  - MVVM架构模式，实现UI和业务逻辑分离
  - 响应式编程支持，提高用户体验
  - 完整的视图模型实现，支持数据绑定和命令

### 7. 测试框架
- **完成状态**：✅ 部分完成
- **主要组件**：
  - 单元测试项目 `QuizForge.Core.Tests` 和 `QuizForge.Tests`
  - 基本的测试用例覆盖核心功能
- **技术亮点**：
  - 使用xUnit测试框架
  - 包含内容生成、布局、解析器等核心组件的测试
  - 支持模拟和依赖注入测试

## 未完成任务

### 1. 打印预览功能

#### 任务描述
完善打印预览功能，提供高质量的PDF预览体验，包括多页面预览、缩放、旋转等功能。

#### 技术要求
- 实现PDF页面的高质量渲染
- 支持多页面预览和导航
- 提供缩放功能（放大、缩小、适应宽度、适应页面）
- 支持页面旋转（0°、90°、180°、270°）
- 实现缩略图视图，便于快速导航
- 优化大文件预览性能

#### 相关文件
- `src/QuizForge.Infrastructure/Services/PrintPreviewService.cs`
- `src/QuizForge.App/Views/PdfPreviewView.axaml`
- `src/QuizForge.App/Views/PdfPreviewView.axaml.cs`
- `src/QuizForge.App/ViewModels/PdfPreviewViewModel.cs`
- `src/QuizForge.App/Converters/ZoomConverter.cs`

#### 实现难点
- PDF页面渲染到图像的转换
- 大文件预览的性能优化
- 缩略图生成和缓存机制
- 跨平台渲染一致性

#### 依赖关系
- 依赖PDF引擎（LatexPdfEngine或NativePdfEngine）
- 依赖图像处理库

### 2. 完善用户界面功能

#### 任务描述
完善用户界面功能，提升用户体验，包括主窗口功能实现、视图模型完善和用户界面优化。

#### 技术要求
- 实现MainViewModel中的TODO标记方法（新建题库、导入题库、保存、导出等）
- 完善各视图模型的实现，确保数据绑定和命令处理正常工作
- 优化界面布局和交互流程
- 添加加载状态指示和进度反馈
- 实现错误提示和异常处理UI
- 优化响应式布局，支持不同屏幕尺寸

#### 相关文件
- `src/QuizForge.App/Views/MainWindow.axaml`
- `src/QuizForge.App/Views/MainWindow.axaml.cs`
- `src/QuizForge.App/ViewModels/MainViewModel.cs`
- `src/QuizForge.App/Views/QuestionBankView.axaml`
- `src/QuizForge.App/Views/QuestionBankView.axaml.cs`
- `src/QuizForge.App/ViewModels/QuestionBankViewModel.cs`
- `src/QuizForge.App/Views/TemplateView.axaml`
- `src/QuizForge.App/Views/TemplateView.axaml.cs`
- `src/QuizForge.App/ViewModels/TemplateViewModel.cs`
- `src/QuizForge.App/Views/ExamGenerationView.axaml`
- `src/QuizForge.App/Views/ExamGenerationView.axaml.cs`
- `src/QuizForge.App/ViewModels/ExamGenerationViewModel.cs`

#### 实现难点
- 异步操作处理和UI线程同步
- 复杂数据绑定和状态管理
- 跨平台UI一致性和性能优化
- 用户体验设计和交互流程优化

#### 依赖关系
- 依赖各功能服务的实现
- 依赖MVVM框架和响应式编程模式

### 3. 增加测试覆盖

#### 任务描述
增加测试覆盖率，确保代码质量和系统稳定性，包括单元测试完善、集成测试实现和端到端测试。

#### 技术要求
- 提高核心功能单元测试覆盖率达到80%以上
- 添加边界条件和异常情况测试
- 实现各组件间的集成测试
- 添加端到端功能测试
- 实现自动化测试流程
- 添加性能测试和负载测试

#### 相关文件
- `src/QuizForge.Core.Tests/LaTeXGenerationTests.cs`
- `src/QuizForge.Tests/DynamicContentInserterTests.cs`
- `src/QuizForge.Tests/HeaderFooterLayoutTests.cs`
- `src/QuizForge.Tests/HeaderIntegrationTests.cs`
- `src/QuizForge.Tests/HeaderLayoutTests.cs`
- `src/QuizForge.Tests/ContentGeneration/ContentGenerationTests.cs`
- `src/QuizForge.Tests/Converters/ConverterTests.cs`
- `src/QuizForge.Tests/Layout/SealLineLayoutTests.cs`
- `src/QuizForge.Tests/Layout/SealLinePreviewTests.cs`
- `src/QuizForge.Tests/Parsers/LatexParserTests.cs`
- `src/QuizForge.Tests/PdfGeneration/PdfGenerationTests.cs`
- `src/QuizForge.Tests/Services/BatchGenerationServiceTests.cs`
- `src/QuizForge.Tests/Services/PdfCacheServiceTests.cs`
- `src/QuizForge.Tests/Services/PdfEngineTests.cs`
- `src/QuizForge.Tests/Unit/ExcelParserTests.cs`
- `src/QuizForge.Tests/Unit/LayoutLogicTests.cs`
- `src/QuizForge.Tests/Unit/MarkdownParserTests.cs`
- `src/QuizForge.Tests/Unit/QuestionBankTests.cs`
- `src/QuizForge.Tests/Unit/TemplateProcessorTests.cs`
- `src/QuizForge.Tests/ViewModels/PdfPreviewViewModelTests.cs`
- `src/QuizForge.Tests/Views/PdfPreviewViewTests.cs`

#### 实现难点
- 模拟依赖组件和外部服务
- 测试数据准备和管理
- 异步测试和并发测试
- UI组件测试和交互测试
- 性能测试和基准测试

#### 依赖关系
- 依赖各组件的基本实现
- 依赖测试框架和模拟库

### 4. 完善异常处理机制

#### 任务描述
完善异常处理机制，提供健壮的错误处理和用户友好的错误提示，包括全局异常处理、特定异常处理和错误日志记录。

#### 技术要求
- 实现全局异常处理机制
- 完善各层特定异常处理
- 提供用户友好的错误提示
- 实现错误日志记录和分析
- 添加错误恢复机制
- 实现错误报告和反馈功能

#### 相关文件
- `src/QuizForge.Core/Exceptions/`
- `src/QuizForge.Models/Exceptions/QuizForgeException.cs`
- `src/QuizForge.Infrastructure/Exceptions/PdfGenerationException.cs`
- `src/QuizForge.App/Program.cs`（全局异常处理）
- `src/QuizForge.App/appsettings.json`（日志配置）

#### 实现难点
- 全局异常捕获和处理
- 异常信息的本地化和用户友好展示
- 异常上下文信息的收集和传递
- 异常恢复和系统状态管理
- 错误报告的生成和分析

#### 依赖关系
- 依赖日志记录功能
- 依赖用户界面层的错误展示

### 5. 增强日志记录功能

#### 任务描述
增强日志记录功能，提供详细的操作日志和错误日志，便于系统监控、问题诊断和性能分析。

#### 技术要求
- 实现结构化日志记录
- 支持多种日志级别（Trace、Debug、Information、Warning、Error、Critical）
- 支持多种日志输出目标（文件、数据库、控制台等）
- 实现日志轮转和归档机制
- 添加性能监控和诊断日志
- 实现日志查询和分析功能

#### 相关文件
- `src/QuizForge.App/appsettings.json`（日志配置）
- `src/QuizForge.App/Program.cs`（日志配置）
- `src/QuizForge.Data/Contexts/QuizDbContext.cs`（日志数据模型）
- `src/QuizForge.Data/Repositories/ConfigRepository.cs`（日志配置访问）

#### 实现难点
- 高性能日志记录，避免影响系统性能
- 结构化日志设计和实现
- 日志轮转和存储管理
- 敏感信息的过滤和脱敏
- 分布式环境下的日志关联和分析

#### 依赖关系
- 依赖配置系统
- 依赖数据访问层（如果使用数据库存储日志）

## 执行指南

### 项目结构理解

QuizForge采用分层架构设计，共分为五层：

1. **用户界面层 (App)**：基于Avalonia UI实现跨平台用户交互
2. **应用服务层 (Services)**：协调业务逻辑和用户界面
3. **业务逻辑层 (Core)**：实现核心业务规则和数据处理
4. **数据访问层 (Data)**：提供数据持久化和检索功能
5. **基础设施层 (Infrastructure)**：提供底层技术支持，如文件解析、PDF生成等

这种架构设计确保了系统的可维护性、可扩展性和高内聚低耦合。

### 依赖关系

#### 层间依赖
- 用户界面层依赖应用服务层
- 应用服务层依赖业务逻辑层
- 业务逻辑层依赖数据访问层和基础设施层
- 数据访问层和基础设施层无上层依赖

#### 组件依赖
- PrintPreviewService依赖PdfEngine
- 各ViewModel依赖对应Service
- 各Service依赖对应Repository和Core组件
- Core组件依赖Infrastructure组件

### 执行顺序

建议按照以下顺序执行剩余任务：

1. **完善异常处理机制**：这是基础工作，为其他任务提供稳定的错误处理基础
2. **增强日志记录功能**：为系统监控和问题诊断提供支持
3. **增加测试覆盖**：确保代码质量和系统稳定性
4. **完善用户界面功能**：提升用户体验
5. **打印预览功能**：实现高级功能

### 代码风格和质量要求

#### 代码风格
- 遵循C#编码规范和命名约定
- 使用有意义的变量名和方法名
- 保持方法简短，单一职责
- 使用适当的访问修饰符
- 添加适当的注释和XML文档注释

#### 代码质量
- 遵循SOLID原则
- 使用设计模式解决常见问题
- 避免代码重复，提取公共功能
- 实现适当的错误处理
- 编写单元测试验证功能

#### 性能要求
- 避免不必要的对象创建和销毁
- 使用异步操作提高响应性
- 实现适当的缓存机制
- 优化数据库查询
- 避免内存泄漏

### 提交规范

#### 分支管理
- 使用功能分支开发新功能
- 功能完成后合并到主分支
- 修复问题使用热修复分支
- 保持主分支始终可部署

#### 提交信息
- 使用清晰的提交信息，描述变更内容
- 提交信息格式：`类型(范围): 描述`
  - 类型：feat（新功能）、fix（修复）、docs（文档）、style（格式）、refactor（重构）、test（测试）、chore（构建）
  - 范围：受影响的模块或组件
  - 描述：变更的简要描述
- 示例：`feat(ui): 添加打印预览缩放功能`

#### 代码审查
- 所有代码变更需要经过审查
- 审查关注点：功能正确性、代码质量、性能、安全性
- 解决所有审查意见后才能合并

### 测试要求

#### 单元测试
- 所有公共方法需要单元测试
- 测试覆盖率达到80%以上
- 使用Arrange-Act-Assert模式编写测试
- 模拟外部依赖，隔离测试单元

#### 集成测试
- 测试组件间的交互
- 使用真实数据库和文件系统
- 验证端到端功能流程

#### 性能测试
- 对关键功能进行性能测试
- 测试系统负载和并发能力
- 识别和解决性能瓶颈

### 文档要求

#### 代码文档
- 所有公共API需要XML文档注释
- 复杂算法和逻辑需要详细注释
- 保持文档与代码同步更新

#### 用户文档
- 提供用户操作手册
- 包含常见问题解答
- 提供示例和最佳实践

#### 开发文档
- 提供架构设计文档
- 包含API参考文档
- 提供部署和配置指南

## 总结

本执行指南提供了QuizForge项目的全面概述，包括已完成功能、未完成任务和执行指南。通过按照本指南执行剩余任务，可以确保项目的顺利推进和高质量交付。

在执行任务时，请遵循项目的架构设计、代码风格和质量要求，确保代码的可维护性、可扩展性和高内聚低耦合。同时，重视测试和文档工作，确保系统的稳定性和可用性。

如有任何问题或需要进一步的信息，请参考项目文档或联系项目团队。