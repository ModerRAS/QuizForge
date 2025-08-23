# QuizForge 文档目录说明

本文档说明了 QuizForge 项目的文档存放结构和相关要求。

## 文档目录结构

```
docs/
├── CLAUDE.md                                    # 文档目录说明（本文件）
├── 01-project-overview/                         # 项目概述和规划
│   ├── PROJECT_OVERVIEW.md                      # 项目总体概述和背景
│   ├── requirements.md                          # 项目需求规格说明
│   └── user-stories.md                          # 用户故事和功能需求
├── 02-technical-design/                         # 技术设计和架构
│   ├── architecture-cli.md                      # CLI 架构设计和技术方案
│   ├── tech-stack-cli.md                        # CLI 技术栈选择和依赖说明
│   └── api-spec-cli.md                          # CLI 接口规范和 API 设计
├── 03-functional-specs/                         # 功能规范和验收
│   └── acceptance-criteria.md                    # 验收标准和测试条件
├── 04-implementation-guides/                    # 实现指南和示例
│   ├── HeaderDesignGuidelines.md                # 页眉设计规范和实现指南
│   └── enhanced_example.md                      # 增强功能实现示例
└── 05-tooling-config/                           # 工具配置和说明
    └── CLAUDE_CODE_TASKS_PROMPT.md              # Claude Code 工具的任务配置
```

## 文档分类说明

### 01-project-overview (项目概述和规划)
- **PROJECT_OVERVIEW.md** - 项目总体概述、背景、已完成功能和未完成任务
- **requirements.md** - 详细的功能需求、安全性需求、测试需求和质量标准
- **user-stories.md** - 基于用户场景的功能需求和验收标准

### 02-technical-design (技术设计和架构)
- **architecture-cli.md** - CLI 系统架构设计、组件关系和部署方案
- **tech-stack-cli.md** - 技术栈选择、依赖管理和构建部署方案
- **api-spec-cli.md** - 命令行接口设计、内部服务接口和错误处理机制

### 03-functional-specs (功能规范和验收)
- **acceptance-criteria.md** - 完整的功能验收标准、测试用例和质量要求

### 04-implementation-guides (实现指南和示例)
- **HeaderDesignGuidelines.md** - 试卷抬头设计的详细指南和技术规范
- **enhanced_example.md** - 实际的试卷示例和格式说明

### 05-tooling-config (工具配置和说明)
- **CLAUDE_CODE_TASKS_PROMPT.md** - Claude Code AI 工具的任务执行指南
- **CLAUDE.md** - 文档目录说明（本文件）

## 文档编写要求

### 命名规范
- 使用英文文件名，单词间用连字符分隔
- 文件名应简洁明了，能够准确反映文档内容
- 统一使用 `.md` 扩展名
- 目录使用数字前缀排序（01-, 02-, 03-等）

### 内容结构
- 每个文档都应该有清晰的标题和层级结构
- 使用 Markdown 语法进行格式化
- 包含适当的目录导航（TOC）
- 技术文档应包含代码示例和配置说明

### 更新维护
- 文档应与代码同步更新
- 重大变更需要更新相关文档
- 定期检查文档的准确性和完整性
- 文档变更应该记录在版本控制中

## 文档管理

### 新增文档
1. 确定文档类型和存放位置
2. 遵循命名规范创建文件
3. 更新本目录说明文件（CLAUDE.md）

### 修改文档
1. 确保修改不影响现有结构
2. 更新相关引用文档
3. 记录修改原因和日期

### 删除文档
1. 确认文档不再需要
2. 检查并清理相关引用
3. 更新目录结构说明

## 文档质量标准

### 准确性
- 文档内容必须准确反映项目实际情况
- 技术参数和配置必须与代码保持一致
- 示例代码必须能够正常运行

### 完整性
- 涵盖项目的所有重要方面
- 提供足够的使用指导
- 包含必要的背景说明

### 可读性
- 语言简洁明了
- 结构清晰合理
- 格式统一规范

## 工具支持

### 编辑工具
- 推荐使用 Visual Studio Code
- 安装 Markdown 相关插件
- 支持实时预览功能

### 版本控制
- 所有文档都应纳入 Git 版本控制
- 遵循项目的分支管理策略
- 重要的文档变更应该有明确的提交信息

## 注意事项

1. **保持更新** - 文档应该随着项目的发展及时更新
2. **避免重复** - 不同文档之间应该避免内容重复
3. **便于查找** - 文档组织应该便于快速查找需要的信息
4. **团队协作** - 文档编写应该考虑团队成员的使用需求