using QuizForge.Core.ContentGeneration;
using QuizForge.Core.Layout;
using QuizForge.Models;
using QuizForge.Models.Interfaces;

namespace QuizForge.Examples;

/// <summary>
/// 抬头功能使用示例
/// </summary>
public class HeaderUsageExamples
{
    /// <summary>
    /// 创建标准样式抬头配置示例
    /// </summary>
    /// <returns>标准样式抬头配置</returns>
    public HeaderConfig CreateStandardHeaderExample()
    {
        return new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "2023学年第一学期期末考试",
            Subject = "高等数学",
            ExamTime = 120,
            TotalPoints = 100,
            ShowSealLine = true,
            ShowStudentInfo = true,
            StudentInfo = new StudentInfoConfig
            {
                ShowName = true,
                NameLabel = "姓名",
                ShowStudentId = true,
                StudentIdLabel = "学号",
                ShowClass = true,
                ClassLabel = "班级",
                ShowDate = true,
                DateLabel = "日期",
                Layout = StudentInfoLayout.Horizontal,
                UnderlineLength = 3
            },
            Alignment = HeaderAlignment.Center,
            TitleFontSize = HeaderFontSize.Large,
            ShowOnFirstPageOnly = true
        };
    }

    /// <summary>
    /// 创建简洁样式抬头配置示例
    /// </summary>
    /// <returns>简洁样式抬头配置</returns>
    public HeaderConfig CreateSimpleHeaderExample()
    {
        return new HeaderConfig
        {
            Style = HeaderStyle.Simple,
            ExamTitle = "单元测试",
            Subject = "线性代数",
            ExamTime = 60,
            TotalPoints = 50,
            ShowSealLine = false,
            ShowStudentInfo = false,
            Alignment = HeaderAlignment.Center,
            TitleFontSize = HeaderFontSize.Normal,
            ShowOnFirstPageOnly = true
        };
    }

    /// <summary>
    /// 创建详细样式抬头配置示例
    /// </summary>
    /// <returns>详细样式抬头配置</returns>
    public HeaderConfig CreateDetailedHeaderExample()
    {
        return new HeaderConfig
        {
            Style = HeaderStyle.Detailed,
            ExamTitle = "2023-2024学年第一学期期中考试",
            Subject = "概率论与数理统计",
            ExamTime = 90,
            TotalPoints = 100,
            ExamLocation = "主教学楼A101",
            SchoolName = "示例大学",
            ExamDate = "2023-11-15",
            ShowSealLine = true,
            ShowStudentInfo = true,
            StudentInfo = new StudentInfoConfig
            {
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
                SubjectLabel = "科目",
                ShowCustomField1 = true,
                CustomField1Label = "专业",
                ShowCustomField2 = true,
                CustomField2Label = "年级",
                Layout = StudentInfoLayout.Vertical,
                UnderlineLength = 3.5m
            },
            Alignment = HeaderAlignment.Center,
            TitleFontSize = HeaderFontSize.Large,
            ShowOnFirstPageOnly = true
        };
    }

    /// <summary>
    /// 创建自定义样式抬头配置示例
    /// </summary>
    /// <returns>自定义样式抬头配置</returns>
    public HeaderConfig CreateCustomHeaderExample()
    {
        return new HeaderConfig
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
\vspace{0.2cm}
考试地点：{EXAM_LOCATION} \quad 考试日期：{EXAM_DATE}
\end{center}

\vspace{0.5cm}

\begin{center}
\begin{tabular}{|c|c|c|c|}
\hline
姓名 & 学号 & 班级 & 专业 \\
\hline
\underline{\hspace{3cm}} & \underline{\hspace{3cm}} & \underline{\hspace{3cm}} & \underline{\hspace{3cm}} \\
\hline
\end{tabular}
\end{center}

\vspace{0.5cm}",
            ExamTitle = "2023-2024学年第一学期期末考试",
            Subject = "大学物理",
            ExamTime = 120,
            TotalPoints = 100,
            ExamLocation = "物理楼201",
            SchoolName = "示例科技大学",
            ExamDate = "2024-01-10",
            ShowSealLine = true,
            ShowStudentInfo = false, // 自定义模板中已经包含学生信息
            Alignment = HeaderAlignment.Center,
            ShowOnFirstPageOnly = true
        };
    }

    /// <summary>
    /// 创建带有考生信息填写区域的抬头配置示例
    /// </summary>
    /// <returns>带有考生信息填写区域的抬头配置</returns>
    public HeaderConfig CreateHeaderWithStudentInfoExample()
    {
        return new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "全国统一考试",
            Subject = "计算机科学基础",
            ExamTime = 150,
            TotalPoints = 150,
            ExamLocation = "考试中心",
            SchoolName = "全国考试委员会",
            ShowSealLine = true,
            ShowStudentInfo = true,
            StudentInfo = new StudentInfoConfig
            {
                ShowName = true,
                NameLabel = "姓名",
                ShowStudentId = true,
                StudentIdLabel = "准考证号",
                ShowClass = true,
                ClassLabel = "考场号",
                ShowDate = true,
                DateLabel = "考试日期",
                ShowSchool = true,
                SchoolLabel = "考点",
                ShowSubject = true,
                SubjectLabel = "考试科目",
                ShowCustomField1 = true,
                CustomField1Label = "身份证号",
                ShowCustomField2 = true,
                CustomField2Label = "座位号",
                Layout = StudentInfoLayout.Grid,
                UnderlineLength = 4m
            },
            Alignment = HeaderAlignment.Center,
            TitleFontSize = HeaderFontSize.Large,
            ShowOnFirstPageOnly = true
        };
    }

    /// <summary>
    /// 创建分页试卷抬头配置示例
    /// </summary>
    /// <returns>分页试卷抬头配置</returns>
    public HeaderConfig CreateMultiPageHeaderExample()
    {
        return new HeaderConfig
        {
            Style = HeaderStyle.Standard,
            ExamTitle = "2023-2024学年第一学期综合考试",
            Subject = "综合能力测试",
            ExamTime = 180,
            TotalPoints = 200,
            ShowSealLine = true,
            ShowStudentInfo = true,
            StudentInfo = new StudentInfoConfig
            {
                ShowName = true,
                NameLabel = "姓名",
                ShowStudentId = true,
                StudentIdLabel = "学号",
                ShowClass = true,
                ClassLabel = "班级",
                ShowDate = true,
                DateLabel = "日期",
                Layout = StudentInfoLayout.Horizontal,
                UnderlineLength = 3m
            },
            Alignment = HeaderAlignment.Center,
            TitleFontSize = HeaderFontSize.Large,
            ShowOnFirstPageOnly = true // 只在第一页显示抬头
        };
    }

    /// <summary>
    /// 生成示例LaTeX代码
    /// </summary>
    /// <param name="headerConfig">抬头配置</param>
    /// <returns>LaTeX代码</returns>
    public string GenerateExampleLaTeX(HeaderConfig headerConfig)
    {
        var headerLayout = new HeaderLayout();
        return headerLayout.GenerateHeader(headerConfig, 1);
    }

    /// <summary>
    /// 演示如何使用新的抬头功能
    /// </summary>
    public void DemonstrateHeaderUsage()
    {
        Console.WriteLine("=== 抬头功能使用示例 ===\n");

        // 示例1：标准样式抬头
        Console.WriteLine("1. 标准样式抬头：");
        var standardHeader = CreateStandardHeaderExample();
        var standardLatex = GenerateExampleLaTeX(standardHeader);
        Console.WriteLine(standardLatex);
        Console.WriteLine();

        // 示例2：简洁样式抬头
        Console.WriteLine("2. 简洁样式抬头：");
        var simpleHeader = CreateSimpleHeaderExample();
        var simpleLatex = GenerateExampleLaTeX(simpleHeader);
        Console.WriteLine(simpleLatex);
        Console.WriteLine();

        // 示例3：详细样式抬头
        Console.WriteLine("3. 详细样式抬头：");
        var detailedHeader = CreateDetailedHeaderExample();
        var detailedLatex = GenerateExampleLaTeX(detailedHeader);
        Console.WriteLine(detailedLatex);
        Console.WriteLine();

        // 示例4：自定义样式抬头
        Console.WriteLine("4. 自定义样式抬头：");
        var customHeader = CreateCustomHeaderExample();
        var customLatex = GenerateExampleLaTeX(customHeader);
        Console.WriteLine(customLatex);
        Console.WriteLine();

        // 示例5：带有考生信息填写区域的抬头
        Console.WriteLine("5. 带有考生信息填写区域的抬头：");
        var studentInfoHeader = CreateHeaderWithStudentInfoExample();
        var studentInfoLatex = GenerateExampleLaTeX(studentInfoHeader);
        Console.WriteLine(studentInfoLatex);
        Console.WriteLine();

        Console.WriteLine("=== 演示完成 ===");
    }
}