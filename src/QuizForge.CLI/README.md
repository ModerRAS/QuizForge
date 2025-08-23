# QuizForge CLI

QuizForge CLI 是一个基于 .NET 8 的跨平台试卷生成命令行工具。它支持从 Excel 和 Markdown 文件生成高质量的 PDF 试卷。

## 功能特性

- 🚀 从 Excel 文件生成试卷 PDF
- 📝 从 Markdown 文件生成试卷 PDF
- 🔍 验证文件格式
- 📋 批量处理多个文件
- 🎨 管理 LaTeX 模板
- ⚙️ 配置管理
- 🌐 跨平台支持（Windows、macOS、Linux）

## 安装

### 从源码构建

```bash
# 克隆项目
git clone <repository-url>
cd QuizForge

# 还原依赖
dotnet restore

# 构建 CLI 工具
dotnet build src/QuizForge.CLI/QuizForge.CLI.csproj

# 运行 CLI
dotnet run --project src/QuizForge.CLI/QuizForge.CLI.csproj -- --help
```

### 作为全局工具安装

```bash
# 发布为全局工具
dotnet publish src/QuizForge.CLI/QuizForge.CLI.csproj -c Release -o ./publish

# 安装为全局工具
dotnet tool install --global --add-path ./publish quizforge
```

## 使用方法

### 基本命令

```bash
# 显示帮助
quizforge --help

# 从 Excel 生成试卷
quizforge generate excel -i questions.xlsx -o exam.pdf

# 从 Markdown 生成试卷
quizforge generate markdown -i questions.md -o exam.pdf

# 验证文件格式
quizforge validate -i questions.xlsx

# 批量处理
quizforge batch -i ./input -o ./output -p "*.xlsx"

# 列出可用模板
quizforge template list

# 显示配置
quizforge config show
```

### 详细参数

#### generate 命令

```bash
quizforge generate excel [OPTIONS]

选项:
  -i, --input <FILE>         输入文件路径
  -o, --output <FILE>        输出文件路径
  -t, --template <NAME>      模板名称
  --title <TITLE>            试卷标题
  --subject <SUBJECT>        考试科目
  --time <MINUTES>           考试时间（分钟）
  --validate                 验证输入文件
  --verbose                  显示详细输出
  --no-progress              禁用进度显示
```

#### batch 命令

```bash
quizforge batch [OPTIONS]

选项:
  -i, --input <DIR>          输入目录
  -o, --output <DIR>         输出目录
  -p, --pattern <PATTERN>    文件匹配模式
  -t, --template <NAME>      模板名称
  --parallel <N>             并行处理数量
  --continue-on-error        失败时继续
  --verbose                  显示详细输出
```

## 配置文件

CLI 支持通过 JSON 配置文件进行配置：

```json
{
  "LaTeX": {
    "DefaultTemplate": "standard",
    "TempDirectory": "/tmp/quizforge",
    "EnableChineseSupport": true,
    "DocumentClass": "article",
    "FontSize": "12pt",
    "PageMargin": "2.5cm"
  },
  "Excel": {
    "DefaultSheetIndex": 1,
    "HeaderRowKeywords": ["题型", "题目", "答案"],
    "MaxRows": 1000,
    "Encoding": "UTF-8"
  },
  "PDF": {
    "OutputDirectory": "./output",
    "DefaultDPI": 300,
    "EnablePreview": true,
    "AutoCleanup": true
  },
  "Templates": {
    "Directory": "./templates",
    "DefaultTemplate": "standard.tex"
  },
  "CLI": {
    "ShowProgress": true,
    "ColoredOutput": true,
    "VerboseLogging": false,
    "AutoCreateDirectories": true
  }
}
```

## Excel 文件格式

Excel 文件应包含以下列：

| 列名 | 说明 | 必需 |
|------|------|------|
| 题型 | 题目类型（如：选择题、填空题） | 是 |
| 题目 | 题目内容 | 是 |
| 选项A | 选项A内容 | 否 |
| 选项B | 选项B内容 | 否 |
| 选项C | 选项C内容 | 否 |
| 选项D | 选项D内容 | 否 |
| 答案 | 正确答案 | 是 |

示例：

```
| 题型   | 题目               | 选项A | 选项B | 选项C | 选项D | 答案 |
|--------|--------------------|--------|--------|--------|--------|------|
| 选择题 | 1+1=?              | 1      | 2      | 3      | 4      | B    |
| 填空题 | 中华人民共和国成立于___年 |        |        |        |        | 1949 |
```

## Markdown 文件格式

Markdown 文件支持以下格式：

```markdown
# 试卷标题

## 考试科目：数学
## 考试时间：120分钟

### 选择题

1. 1+1=?
   - A. 1
   - B. 2
   - C. 3
   - D. 4
   
   答案：B

### 填空题

2. 中华人民共和国成立于____年。
   
   答案：1949
```

## 系统要求

- .NET 8.0 SDK 或更高版本
- LaTeX 发行版（如 MiKTeX、TeX Live，可选）
- 支持的操作系统：
  - Windows 10+
  - macOS 10.15+
  - Linux (Ubuntu 18.04+, CentOS 8+)

## 故障排除

### 常见问题

1. **PDF 生成失败**
   - 确保 LaTeX 已正确安装
   - 检查临时目录权限
   - 验证输入文件格式

2. **Excel 解析失败**
   - 检查文件格式是否正确
   - 确保包含必要的列
   - 验证文件编码

3. **权限问题**
   - 确保有读写输入/输出目录的权限
   - 检查临时目录权限

### 日志和调试

启用详细日志：

```bash
quizforge generate excel -i questions.xlsx -o exam.pdf --verbose
```

查看配置：

```bash
quizforge config show --all
```

## 许可证

本项目采用 MIT 许可证。详见 LICENSE 文件。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

如有问题或建议，请通过以下方式联系：

- 提交 Issue
- 发送邮件至：[your-email@example.com]