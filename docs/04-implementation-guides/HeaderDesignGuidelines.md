# QuizForge 试卷抬头设计指南

## 概述

QuizForge 提供了灵活且美观的试卷抬头设计功能，支持多种样式和布局选项，满足不同考试场景的需求。本指南将介绍如何使用这些功能创建符合考试规范的试卷抬头。

## 抬头样式

### 1. 标准样式 (HeaderStyle.Standard)

标准样式是最常用的抬头设计，适合大多数考试场景。它包含以下元素：
- 试卷标题（大号字体，粗体）
- 水平分割线
- 考试信息表格（科目、时间、总分、日期）

**LaTeX 输出示例：**
```latex
% 标准样式抬头部分
\begin{center}
{\Large \textbf{2023学年第一学期期末考试}}
\vspace{0.8cm}
\rule{\textwidth}{0.4pt}
\vspace{0.5cm}
\begin{tabular}{ll}
\textbf{考试科目}：高等数学 & \textbf{考试时间}：120分钟 \\
\textbf{总分}：100分 & \textbf{考试日期}：\underline{\hspace{3cm}} \\
\end{tabular}
\end{center}
```

### 2. 简洁样式 (HeaderStyle.Simple)

简洁样式适用于小型测试或单元测验，设计更加紧凑：
- 试卷标题（中等字体，粗体）
- 细水平分割线
- 单行考试信息

**LaTeX 输出示例：**
```latex
% 简洁样式抬头部分
\begin{center}
{\large \textbf{单元测试}}
\vspace{0.5cm}
\rule{\textwidth}{0.3pt}
\vspace{0.3cm}
\begin{tabular}{l}
考试科目：线性代数 \quad 考试时间：60分钟 \quad 总分：50分 \\
\end{tabular}
\end{center}
```

### 3. 详细样式 (HeaderStyle.Detailed)

详细样式适用于正式考试或重要测试，包含更全面的考试信息：
- 试卷标题（特大号字体，粗体）
- 双水平分割线
- 三列考试信息表格
- 支持学院、专业、班级、学期等额外信息

**LaTeX 输出示例：**
```latex
% 详细样式抬头部分
\begin{center}
{\LARGE \textbf{2023-2024学年第一学期期中考试}}
\vspace{0.8cm}
\rule{\textwidth}{0.4pt}
\vspace{0.1cm}
\rule{\textwidth}{0.4pt}
\vspace{0.6cm}
\begin{tabular}{p{4cm}p{4cm}p{4cm}}
\textbf{考试科目}：概率论与数理统计 & \textbf{考试时间}：90分钟 & \textbf{总分}：100分 \\
\vspace{0.3cm}
\textbf{考试日期}：\underline{\hspace{3cm}} & \textbf{学校}：示例大学 & \textbf{考试地点}：\underline{\hspace{3cm}} \\
\textbf{学院}：\underline{\hspace{3cm}} & \textbf{专业}：\underline{\hspace{3cm}} & \textbf{班级}：\underline{\hspace{3cm}} \\
\textbf{学期}：\underline{\hspace{3cm}} & \textbf{考试类型}：\underline{\hspace{3cm}} &  \\
\end{tabular}
\end{center}
```

### 4. 自定义样式 (HeaderStyle.Custom)

自定义样式允许用户完全控制抬头的设计，使用自定义模板：
- 完全自定义的LaTeX模板
- 支持占位符替换
- 适用于特殊考试需求

**使用示例：**
```csharp
var config = new HeaderConfig
{
    Style = HeaderStyle.Custom,
    CustomTemplate = @"
\begin{center}
{\huge \textbf{{EXAM_TITLE}}} \\
\vspace{0.5cm}
{\Large {SCHOOL_NAME}} \\
\vspace{0.3cm}
{\large 考试科目：{SUBJECT}} \\
\vspace{0.2cm}
考试时间：{EXAM_TIME}分钟 \quad 总分：{TOTAL_POINTS}分 \\
\end{center}",
    ExamTitle = "自定义考试",
    Subject = "物理",
    ExamTime = 120,
    TotalPoints = 100,
    SchoolName = "示例大学"
};
```

## 考生信息填写区域

考生信息填写区域提供了多种布局选项，以满足不同考试的需求：

### 1. 水平布局 (StudentInfoLayout.Horizontal)

所有字段水平排列在一行，适合空间有限的场景：
```latex
\begin{tabular}{l}
姓名\underline{\hspace{3cm}} \quad 学号\underline{\hspace{3cm}} \quad 班级\underline{\hspace{3cm}} \quad 日期\underline{\hspace{3cm}} \\
\end{tabular}
```

### 2. 垂直布局 (StudentInfoLayout.Vertical)

所有字段垂直排列在单列，适合需要较多字段的场景：
```latex
\begin{tabular}{l}
姓名\underline{\hspace{3cm}} \\
学号\underline{\hspace{3cm}} \\
班级\underline{\hspace{3cm}} \\
日期\underline{\hspace{3cm}} \\
\end{tabular}
```

### 3. 网格布局 (StudentInfoLayout.Grid)

字段以网格形式排列，适合需要整齐布局的场景：
```latex
\begin{tabular}{|l|l|l|l|}
\hline
姓名\underline{\hspace{3cm}} & 学号\underline{\hspace{3cm}} & 班级\underline{\hspace{3cm}} & 日期\underline{\hspace{3cm}} \\
\hline
学校\underline{\hspace{3cm}} & 科目\underline{\hspace{3cm}} & 专业\underline{\hspace{3cm}} & 年级\underline{\hspace{3cm}} \\
\hline
\end{tabular}
```

### 4. 两列布局 (StudentInfoLayout.TwoColumn)

字段以两列形式排列，平衡了空间利用率和可读性：
```latex
\begin{tabular}{ll}
姓名\underline{\hspace{3cm}} & 学号\underline{\hspace{3cm}} \\
班级\underline{\hspace{3cm}} & 日期\underline{\hspace{3cm}} \\
学校\underline{\hspace{3cm}} & 科目\underline{\hspace{3cm}} \\
\end{tabular}
```

### 5. 单列布局 (StudentInfoLayout.SingleColumn)

字段以单列形式排列，与垂直布局类似，但保留了更多的自定义选项。

## 字体和对齐选项

### 字体大小 (HeaderFontSize)

- **Small** (\normalsize): 适合内容较多的抬头
- **Medium** (\large): 适合标准抬头
- **Large** (\Large): 适合重要考试（推荐）
- **ExtraLarge** (\LARGE): 适合正式考试或特殊场合

### 对齐方式 (HeaderAlignment)

- **Left** (\begin{flushleft}): 左对齐，适合需要从左侧开始阅读的场景
- **Center** (\begin{center}): 居中对齐，适合大多数考试场景（推荐）
- **Right** (\begin{flushright}): 右对齐，适合特殊设计需求

## 设计规范建议

### 1. 标题设计

- 使用粗体 (\textbf) 突出标题
- 标题字体大小建议使用 Large 或更大
- 标题与内容之间保留适当间距（建议 0.5cm-0.8cm）

### 2. 信息组织

- 使用表格布局组织信息，保持整齐
- 重要信息（如科目、时间、总分）使用粗体标签
- 需要填写的字段使用下划线 (\underline{\hspace{Xcm}})

### 3. 视觉分隔

- 使用水平线 (\rule{\textwidth}{Xpt}) 分隔不同部分
- 标准样式使用 0.4pt 线宽
- 简洁样式使用 0.3pt 线宽
- 详细样式可使用双线增强视觉效果

### 4. 间距控制

- 标题与分割线之间：0.5cm-0.8cm
- 分割线与内容之间：0.3cm-0.5cm
- 内容行之间：0.3cm
- 抬头与正文之间：0.5cm-1.0cm

### 5. 考生信息填写

- 下划线长度建议：3cm-4cm
- 标签与下划线之间不加空格
- 使用表格布局保持对齐
- 考虑使用带边框的表格增强正式感

## 使用示例

### 创建标准样式抬头

```csharp
var headerConfig = new HeaderConfig
{
    Style = HeaderStyle.Standard,
    ExamTitle = "2023学年第一学期期末考试",
    Subject = "高等数学",
    ExamTime = 120,
    TotalPoints = 100,
    ExamDate = DateTime.Now,
    Alignment = HeaderAlignment.Center,
    TitleFontSize = HeaderFontSize.Large,
    TitleBold = true,
    ShowStudentInfo = true,
    StudentInfo = new StudentInfoConfig
    {
        Layout = StudentInfoLayout.Horizontal,
        ShowName = true,
        NameLabel = "姓名",
        ShowStudentId = true,
        StudentIdLabel = "学号",
        ShowClass = true,
        ClassLabel = "班级",
        ShowDate = true,
        DateLabel = "日期",
        UnderlineLength = 3
    }
};

var headerLayout = new HeaderLayout();
var latexHeader = headerLayout.GenerateHeader(headerConfig, 1);
```

### 创建详细样式抬头

```csharp
var headerConfig = new HeaderConfig
{
    Style = HeaderStyle.Detailed,
    ExamTitle = "2023-2024学年第一学期期中考试",
    Subject = "概率论与数理统计",
    ExamTime = 90,
    TotalPoints = 100,
    ExamLocation = "主教学楼A101",
    SchoolName = "示例大学",
    ExamDate = DateTime.Now,
    Department = "数学系",
    Major = "统计学",
    Class = "统计2021级",
    Semester = "2023-2024学年第一学期",
    ExamType = "闭卷考试",
    Alignment = HeaderAlignment.Center,
    TitleFontSize = HeaderFontSize.ExtraLarge,
    TitleBold = true,
    ShowStudentInfo = true,
    StudentInfo = new StudentInfoConfig
    {
        Layout = StudentInfoLayout.Grid,
        ShowName = true,
        NameLabel = "姓名",
        ShowStudentId = true,
        StudentIdLabel = "学号",
        ShowClass = true,
        ClassLabel = "班级",
        ShowDate = true,
        DateLabel = "考试日期",
        ShowSchool = true,
        SchoolLabel = "学校",
        ShowSubject = true,
        SubjectLabel = "考试科目",
        ShowCustomField1 = true,
        CustomField1Label = "专业",
        ShowCustomField2 = true,
        CustomField2Label = "年级",
        UnderlineLength = 3.5m
    }
};

var headerLayout = new HeaderLayout();
var latexHeader = headerLayout.GenerateHeader(headerConfig, 1);
```

## 注意事项

1. **兼容性**：新抬头功能与现有系统完全兼容，未设置 HeaderConfig 的模板将使用默认抬头。

2. **分页处理**：默认情况下，抬头只在第一页显示。如需在所有页面显示，设置 `ShowOnFirstPageOnly = false`。

3. **LaTeX 转义**：系统会自动处理特殊字符的转义，无需手动处理。

4. **中文支持**：抬头功能完全支持中文内容，确保在 LaTeX 模板中包含 `ctex` 宏包。

5. **测试验证**：建议在使用前生成测试文档，验证抬头效果是否符合预期。

## 总结

QuizForge 的抬头功能提供了灵活且美观的试卷抬头设计选项，支持多种样式和布局，满足不同考试场景的需求。通过合理配置 HeaderConfig 和 StudentInfoConfig，可以创建符合考试规范且视觉吸引力强的试卷抬头。