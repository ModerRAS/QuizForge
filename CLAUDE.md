# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

QuizForge 是一个基于 .NET 8 的跨平台试卷生成系统，使用 Avalonia UI 构建用户界面。系统支持从多种格式的题库（Markdown、Excel）自动生成试卷，并提供 LaTeX 模板系统来生成高质量的 PDF 试卷。

## Architecture

系统采用分层架构设计：

1. **用户界面层 (Presentation Layer)** - QuizForge.App
   - 基于 Avalonia UI 11.0.0 的跨平台桌面应用
   - 使用 MVVM 模式和 ReactiveUI
   - 包含主窗口、题库管理、模板管理、试卷生成和 PDF 预览视图

2. **应用服务层 (Application Service Layer)** - QuizForge.Services
   - 提供题库服务、模板服务、生成服务和导出服务
   - 协调业务逻辑层和用户界面层

3. **业务逻辑层 (Business Logic Layer)** - QuizForge.Core
   - 核心业务逻辑实现
   - 包含试卷生成器、内容生成器、模板处理器等

4. **数据访问层 (Data Access Layer)** - QuizForge.Data
   - 基于 Entity Framework Core 8.0.0 和 SQLite
   - 使用仓储模式提供数据访问抽象

5. **基础设施层 (Infrastructure Layer)** - QuizForge.Infrastructure
   - 提供文件解析（Markdown、Excel）、PDF生成等底层功能
   - 集成第三方库如 Markdig、ClosedXML、PdfSharp 等

## Build and Development Commands

### 构建项目
```bash
# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 构建发布版本
dotnet build --configuration Release
```

### 运行测试
```bash
# 运行所有测试
dotnet test

# 运行特定项目的测试
dotnet test tests/QuizForge.Tests/QuizForge.Tests.csproj

# 运行测试并显示详细信息
dotnet test --verbosity normal
```

### 运行应用程序
```bash
# 运行开发版本
dotnet run --project src/QuizForge.App/QuizForge.App.csproj

# 发布应用程序
dotnet publish --configuration Release --output ./publish
```

### 代码质量检查
```bash
# 查看项目状态
git status

# 查看修改
git diff

# 查看最近的提交
git log --oneline -10
```

## Key Dependencies

### 核心框架
- **.NET 8.0** - 目标框架
- **Avalonia UI 11.0.0** - 跨平台 UI 框架
- **ReactiveUI** - MVVM 和响应式编程
- **CommunityToolkit.Mvvm 8.2.2** - MVVM 工具包

### 数据处理
- **Entity Framework Core 8.0.0** - ORM 框架
- **Microsoft.EntityFrameworkCore.Sqlite 8.0.0** - SQLite 数据库支持

### 文件解析
- **Markdig 0.31.0** - Markdown 解析器
- **ClosedXML 0.102.1** - Excel 文件处理

### PDF 生成
- **PdfSharp 1.50.5147** - PDF 生成库
- **QuestPDF 2023.12.2** - 现代 PDF 库
- **SkiaSharp 2.88.7** - 图形处理库

### 其他依赖
- **Microsoft.Extensions.DependencyInjection 8.0.0** - 依赖注入
- **Microsoft.Extensions.Configuration 8.0.0** - 配置管理
- **SixLabors.ImageSharp 3.1.2** - 图像处理

## Project Structure

```
QuizForge/
├── src/
│   ├── QuizForge.App/          # 主应用程序
│   ├── QuizForge.Core/         # 核心业务逻辑
│   ├── QuizForge.Data/         # 数据访问层
│   ├── QuizForge.Infrastructure/ # 基础设施层
│   ├── QuizForge.Services/     # 应用服务层
│   └── QuizForge.Models/       # 数据模型
├── tests/
│   └── QuizForge.Tests/        # 单元测试和集成测试
├── docs/                       # 项目文档
└── architecture-design.md      # 架构设计文档
```

## Key Components

### 数据模型 (QuizForge.Models)
- `Question` - 题目模型
- `QuestionBank` - 题库模型
- `ExamPaper` - 试卷模型
- `ExamTemplate` - 试卷模板模型
- `HeaderConfig` - 页眉配置模型

### 核心服务 (QuizForge.Services)
- `QuestionService` - 题库服务
- `TemplateService` - 模板服务
- `GenerationService` - 生成服务
- `ExportService` - 导出服务

### 核心业务逻辑 (QuizForge.Core)
- `ExamPaperGenerator` - 试卷生成器
- `ContentGenerator` - 内容生成器
- `LaTeXDocumentGenerator` - LaTeX 文档生成器
- `QuestionBankProcessor` - 题库处理器

### 基础设施 (QuizForge.Infrastructure)
- `MarkdownParser` - Markdown 解析器
- `ExcelParser` - Excel 解析器
- `LatexPdfEngine` - LaTeX PDF 引擎
- `BatchGenerationService` - 批量生成服务

## Development Guidelines

### 代码风格
- 使用 C# 8.0+ 特性
- 启用 nullable 引用类型
- 使用异步编程模式（async/await）
- 遵循 SOLID 原则

### 测试策略
- 单元测试位于 `tests/QuizForge.Tests/Unit/`
- 集成测试位于 `tests/QuizForge.Tests/Integration/`
- 使用 xUnit 测试框架

### 配置管理
- 应用程序配置在 `appsettings.json`
- 测试配置在 `appsettings.test.json`
- 使用 Microsoft.Extensions.Configuration 进行配置管理

### 数据库
- 使用 SQLite 作为数据库
- 数据库文件：`quizforge.db`
- 支持 Entity Framework Core 迁移

## Common Tasks

### 添加新的题库格式支持
1. 在 `QuizForge.Models/Interfaces/IQuestionParser.cs` 中扩展接口
2. 在 `QuizForge.Infrastructure/Parsers/` 中实现新的解析器
3. 在 `QuizForge.Services/QuestionService.cs` 中注册新解析器

### 添加新的试卷模板
1. 创建 LaTeX 模板文件 (.tex)
2. 在 `QuizForge.Infrastructure/Templates/` 中添加模板
3. 在 `QuizForge.Models/ExamTemplate.cs` 中配置模板属性

### 添加新的导出格式
1. 在 `QuizForge.Models/Interfaces/IExportService.cs` 中扩展接口
2. 在 `QuizForge.Services/ExportService.cs` 中实现新的导出方法
3. 在 `QuizForge.Infrastructure/Engines/` 中添加相应的引擎

## Environment Requirements

- .NET 8.0 SDK
- LaTeX 发行版（可选，用于 PDF 生成）
- 支持 Windows、macOS、Linux

## Current Status

项目已完成核心功能实现，包括：
- ✅ 数据模型层
- ✅ 核心业务逻辑层
- ✅ 数据访问层
- ✅ 基础设施层（大部分）
- ✅ 应用服务层（大部分）
- ✅ 用户界面层（基本完成）

未完成任务主要集中在前端 UI 功能完善、测试覆盖率和部署准备方面。