using QuizForge.Models;
using System.Text;

namespace QuizForge.Core.Layout;

/// <summary>
/// 抬头布局逻辑类
/// </summary>
public class HeaderLayout
{
    /// <summary>
    /// 根据字体大小获取LaTeX字体命令
    /// </summary>
    /// <param name="fontSize">字体大小</param>
    /// <returns>LaTeX字体命令</returns>
    private string GetFontSizeCommand(HeaderFontSize fontSize)
    {
        return fontSize switch
        {
            HeaderFontSize.Small => "\\normalsize",
            HeaderFontSize.Medium => "\\large",
            HeaderFontSize.Large => "\\Large",
            HeaderFontSize.ExtraLarge => "\\LARGE",
            _ => "\\Large"
        };
    }

    /// <summary>
    /// 根据对齐方式获取LaTeX环境
    /// </summary>
    /// <param name="alignment">对齐方式</param>
    /// <returns>LaTeX环境</returns>
    private string GetAlignmentEnvironment(HeaderAlignment alignment)
    {
        return alignment switch
        {
            HeaderAlignment.Left => "flushleft",
            HeaderAlignment.Center => "center",
            HeaderAlignment.Right => "flushright",
            _ => "center"
        };
    }

    /// <summary>
    /// 生成标准样式抬头LaTeX代码
    /// </summary>
    /// <param name="config">抬头配置</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    private string GenerateStandardHeader(HeaderConfig config, int pageNumber)
    {
        if (!config.ShowHeader || pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();
        string fontSizeCmd = GetFontSizeCommand(config.FontSize);
        string alignEnv = GetAlignmentEnvironment(config.Alignment);

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 标准样式抬头部分");
        headerBuilder.AppendLine($@"\begin{{{alignEnv}}}");

        // 添加试卷标题（使用更大的字体和粗体）
        string titleFormat = config.TitleBold ? @"\textbf" : "";
        headerBuilder.AppendLine($@"{{\Large {titleFormat}{{{EscapeLaTeX(config.ExamTitle)}}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.8cm}");

        // 添加水平分割线
        headerBuilder.AppendLine(@"\rule{\textwidth}{0.4pt}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加考试信息表格
        headerBuilder.AppendLine(@"\begin{tabular}{ll}");
        
        // 考试科目
        if (!string.IsNullOrWhiteSpace(config.Subject))
        {
            headerBuilder.AppendLine($@"\textbf{{考试科目}}：{EscapeLaTeX(config.Subject)} & ");
        }
        
        // 考试时间
        headerBuilder.AppendLine($@"\textbf{{考试时间}}：{config.ExamTime}分钟 \\");
        
        // 总分
        headerBuilder.AppendLine($@"\textbf{{总分}}：{config.TotalPoints}分 & ");
        
        // 考试日期
        if (config.ExamDate.HasValue)
        {
            headerBuilder.AppendLine($@"\textbf{{考试日期}}：{config.ExamDate.Value:yyyy-MM-dd} \\");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{考试日期}}：\underline{{\hspace{{3cm}}}} \\");
        }
        
        headerBuilder.AppendLine(@"\end{tabular}");

        // 结束抬头部分
        headerBuilder.AppendLine($@"\end{{{alignEnv}}}");
        headerBuilder.AppendLine($@"\vspace{{{config.SpacingAfter}cm}}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成简洁样式抬头LaTeX代码
    /// </summary>
    /// <param name="config">抬头配置</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    private string GenerateSimpleHeader(HeaderConfig config, int pageNumber)
    {
        if (!config.ShowHeader || pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();
        string fontSizeCmd = GetFontSizeCommand(config.FontSize);
        string alignEnv = GetAlignmentEnvironment(config.Alignment);

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 简洁样式抬头部分");
        headerBuilder.AppendLine($@"\begin{{{alignEnv}}}");

        // 添加试卷标题（使用粗体）
        string titleFormat = config.TitleBold ? @"\textbf" : "";
        headerBuilder.AppendLine($@"{{{fontSizeCmd} {titleFormat}{{{EscapeLaTeX(config.ExamTitle)}}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加水平分割线（细线）
        headerBuilder.AppendLine(@"\rule{\textwidth}{0.3pt}");
        headerBuilder.AppendLine(@"\vspace{0.3cm}");

        // 添加考试信息（单行显示）
        headerBuilder.AppendLine(@"\begin{tabular}{l}");
        
        if (!string.IsNullOrWhiteSpace(config.Subject))
        {
            headerBuilder.AppendLine($@"考试科目：{EscapeLaTeX(config.Subject)} \quad ");
        }
        
        headerBuilder.AppendLine($@"考试时间：{config.ExamTime}分钟 \quad 总分：{config.TotalPoints}分 \\");
        
        headerBuilder.AppendLine(@"\end{tabular}");

        // 结束抬头部分
        headerBuilder.AppendLine($@"\end{{{alignEnv}}}");
        headerBuilder.AppendLine($@"\vspace{{{config.SpacingAfter}cm}}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成详细样式抬头LaTeX代码
    /// </summary>
    /// <param name="config">抬头配置</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    private string GenerateDetailedHeader(HeaderConfig config, int pageNumber)
    {
        if (!config.ShowHeader || pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();
        string fontSizeCmd = GetFontSizeCommand(config.FontSize);
        string alignEnv = GetAlignmentEnvironment(config.Alignment);

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 详细样式抬头部分");
        headerBuilder.AppendLine($@"\begin{{{alignEnv}}}");

        // 添加试卷标题（使用大号字体和粗体）
        string titleFormat = config.TitleBold ? @"\textbf" : "";
        headerBuilder.AppendLine($@"{{\LARGE {titleFormat}{{{EscapeLaTeX(config.ExamTitle)}}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.8cm}");

        // 添加水平分割线（双线）
        headerBuilder.AppendLine(@"\rule{\textwidth}{0.4pt}");
        headerBuilder.AppendLine(@"\vspace{0.1cm}");
        headerBuilder.AppendLine(@"\rule{\textwidth}{0.4pt}");
        headerBuilder.AppendLine(@"\vspace{0.6cm}");

        // 创建表格布局（三列）
        headerBuilder.AppendLine(@"\begin{tabular}{p{4cm}p{4cm}p{4cm}}");

        // 第一行：考试科目、考试时间和总分
        if (!string.IsNullOrWhiteSpace(config.Subject))
        {
            headerBuilder.AppendLine($@"\textbf{{考试科目}}：{EscapeLaTeX(config.Subject)} & ");
        }
        
        headerBuilder.AppendLine($@"\textbf{{考试时间}}：{config.ExamTime}分钟 & ");
        headerBuilder.AppendLine($@"\textbf{{总分}}：{config.TotalPoints}分 \\");
        
        headerBuilder.AppendLine(@"\vspace{0.3cm}");

        // 第二行：考试日期、学校和考试地点
        if (config.ExamDate.HasValue)
        {
            headerBuilder.AppendLine($@"\textbf{{考试日期}}：{config.ExamDate.Value:yyyy-MM-dd} & ");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{考试日期}}：\underline{{\hspace{{3cm}}}} & ");
        }

        if (!string.IsNullOrWhiteSpace(config.SchoolName))
        {
            headerBuilder.AppendLine($@"\textbf{{学校}}：{EscapeLaTeX(config.SchoolName)} & ");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{学校}}：\underline{{\hspace{{3cm}}}} & ");
        }

        if (!string.IsNullOrWhiteSpace(config.ExamLocation))
        {
            headerBuilder.AppendLine($@"\textbf{{考试地点}}：{EscapeLaTeX(config.ExamLocation)} \\");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{考试地点}}：\underline{{\hspace{{3cm}}}} \\");
        }

        // 第三行：学院、专业和班级
        if (!string.IsNullOrWhiteSpace(config.Department))
        {
            headerBuilder.AppendLine($@"\textbf{{学院}}：{EscapeLaTeX(config.Department)} & ");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{学院}}：\underline{{\hspace{{3cm}}}} & ");
        }

        if (!string.IsNullOrWhiteSpace(config.Major))
        {
            headerBuilder.AppendLine($@"\textbf{{专业}}：{EscapeLaTeX(config.Major)} & ");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{专业}}：\underline{{\hspace{{3cm}}}} & ");
        }

        if (!string.IsNullOrWhiteSpace(config.Class))
        {
            headerBuilder.AppendLine($@"\textbf{{班级}}：{EscapeLaTeX(config.Class)} \\");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{班级}}：\underline{{\hspace{{3cm}}}} \\");
        }

        // 第四行：学期和考试类型
        if (!string.IsNullOrWhiteSpace(config.Semester))
        {
            headerBuilder.AppendLine($@"\textbf{{学期}}：{EscapeLaTeX(config.Semester)} & ");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{学期}}：\underline{{\hspace{{3cm}}}} & ");
        }

        if (!string.IsNullOrWhiteSpace(config.ExamType))
        {
            headerBuilder.AppendLine($@"\textbf{{考试类型}}：{EscapeLaTeX(config.ExamType)} \\");
        }
        else
        {
            headerBuilder.AppendLine($@"\textbf{{考试类型}}：\underline{{\hspace{{3cm}}}} \\");
        }

        headerBuilder.AppendLine(@"\end{tabular}");

        // 结束抬头部分
        headerBuilder.AppendLine($@"\end{{{alignEnv}}}");
        headerBuilder.AppendLine($@"\vspace{{{config.SpacingAfter}cm}}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成考生信息填写区域LaTeX代码
    /// </summary>
    /// <param name="config">考生信息配置</param>
    /// <returns>考生信息LaTeX代码</returns>
    private string GenerateStudentInfo(StudentInfoConfig config)
    {
        var infoBuilder = new StringBuilder();

        infoBuilder.AppendLine(@"% 考生信息填写区域");
        infoBuilder.AppendLine(@"\begin{center}");

        switch (config.Layout)
        {
            case StudentInfoLayout.Horizontal:
                // 水平布局（单行）
                infoBuilder.AppendLine(@"\begin{tabular}{l}");
                var horizontalItems = new List<string>();
                
                if (config.ShowName)
                    horizontalItems.Add($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowStudentId)
                    horizontalItems.Add($@"{config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowClass)
                    horizontalItems.Add($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowDate)
                    horizontalItems.Add($@"{config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowSchool)
                    horizontalItems.Add($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowSubject)
                    horizontalItems.Add($@"{config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowCustomField1)
                    horizontalItems.Add($@"{config.CustomField1Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                if (config.ShowCustomField2)
                    horizontalItems.Add($@"{config.CustomField2Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                
                infoBuilder.AppendLine(string.Join(@" \quad ", horizontalItems) + @" \\");
                infoBuilder.AppendLine(@"\end{tabular}");
                break;

            case StudentInfoLayout.Vertical:
                // 垂直布局（单列）
                infoBuilder.AppendLine(@"\begin{tabular}{l}");
                if (config.ShowName)
                    infoBuilder.AppendLine($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowStudentId)
                    infoBuilder.AppendLine($@"{config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowClass)
                    infoBuilder.AppendLine($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowDate)
                    infoBuilder.AppendLine($@"{config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowSchool)
                    infoBuilder.AppendLine($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowSubject)
                    infoBuilder.AppendLine($@"{config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowCustomField1)
                    infoBuilder.AppendLine($@"{config.CustomField1Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowCustomField2)
                    infoBuilder.AppendLine($@"{config.CustomField2Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                infoBuilder.AppendLine(@"\end{tabular}");
                break;

            case StudentInfoLayout.Grid:
                // 网格布局（多行多列）
                infoBuilder.AppendLine(@"\begin{tabular}{|l|l|l|l|}");
                infoBuilder.AppendLine(@"\hline");
                
                // 第一行
                var firstRowItems = new List<string>();
                if (config.ShowName)
                    firstRowItems.Add($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    firstRowItems.Add(@" ");
                    
                if (config.ShowStudentId)
                    firstRowItems.Add($@"{config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    firstRowItems.Add(@" ");
                    
                if (config.ShowClass)
                    firstRowItems.Add($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    firstRowItems.Add(@" ");
                    
                if (config.ShowDate)
                    firstRowItems.Add($@"{config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    firstRowItems.Add(@" ");
                    
                infoBuilder.AppendLine(string.Join(" & ", firstRowItems) + @" \\");
                infoBuilder.AppendLine(@"\hline");
                
                // 第二行
                var secondRowItems = new List<string>();
                if (config.ShowSchool)
                    secondRowItems.Add($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    secondRowItems.Add(@" ");
                    
                if (config.ShowSubject)
                    secondRowItems.Add($@"{config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    secondRowItems.Add(@" ");
                    
                if (config.ShowCustomField1)
                    secondRowItems.Add($@"{config.CustomField1Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    secondRowItems.Add(@" ");
                    
                if (config.ShowCustomField2)
                    secondRowItems.Add($@"{config.CustomField2Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}}");
                else
                    secondRowItems.Add(@" ");
                    
                infoBuilder.AppendLine(string.Join(" & ", secondRowItems) + @" \\");
                infoBuilder.AppendLine(@"\hline");
                
                infoBuilder.AppendLine(@"\end{tabular}");
                break;
                
            case StudentInfoLayout.TwoColumn:
                // 两列布局
                infoBuilder.AppendLine(@"\begin{tabular}{ll}");
                
                // 第一对
                if (config.ShowName && config.ShowStudentId)
                    infoBuilder.AppendLine($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & {config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                else if (config.ShowName)
                    infoBuilder.AppendLine($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & \\");
                else if (config.ShowStudentId)
                    infoBuilder.AppendLine(@"& " + $@"{config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");

                // 第二对
                if (config.ShowClass && config.ShowDate)
                    infoBuilder.AppendLine($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & {config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                else if (config.ShowClass)
                    infoBuilder.AppendLine($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & \\");
                else if (config.ShowDate)
                    infoBuilder.AppendLine(@"& " + $@"{config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                    
                // 第三对
                if (config.ShowSchool && config.ShowSubject)
                    infoBuilder.AppendLine($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & {config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                else if (config.ShowSchool)
                    infoBuilder.AppendLine($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} & \\");
                else if (config.ShowSubject)
                    infoBuilder.AppendLine(@"& " + $@"{config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                    
                infoBuilder.AppendLine(@"\end{tabular}");
                break;
                
            case StudentInfoLayout.SingleColumn:
            default:
                // 单列布局（默认）
                infoBuilder.AppendLine(@"\begin{tabular}{l}");
                if (config.ShowName)
                    infoBuilder.AppendLine($@"{config.NameLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowStudentId)
                    infoBuilder.AppendLine($@"{config.StudentIdLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowClass)
                    infoBuilder.AppendLine($@"{config.ClassLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowDate)
                    infoBuilder.AppendLine($@"{config.DateLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowSchool)
                    infoBuilder.AppendLine($@"{config.SchoolLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowSubject)
                    infoBuilder.AppendLine($@"{config.SubjectLabel}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowCustomField1)
                    infoBuilder.AppendLine($@"{config.CustomField1Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                if (config.ShowCustomField2)
                    infoBuilder.AppendLine($@"{config.CustomField2Label}\underline{{\hspace{{{config.UnderlineLength}cm}}}} \\");
                infoBuilder.AppendLine(@"\end{tabular}");
                break;
        }

        infoBuilder.AppendLine(@"\end{center}");
        infoBuilder.AppendLine(@"\vspace{0.5cm}");

        return infoBuilder.ToString();
    }
    /// <summary>
    /// 生成抬头LaTeX代码（使用新的HeaderConfig）
    /// </summary>
    /// <param name="config">抬头配置</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    public string GenerateHeader(HeaderConfig config, int pageNumber)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var headerBuilder = new StringBuilder();

        // 根据样式生成抬头
        switch (config.Style)
        {
            case HeaderStyle.Standard:
                headerBuilder.Append(GenerateStandardHeader(config, pageNumber));
                break;
            case HeaderStyle.Simple:
                headerBuilder.Append(GenerateSimpleHeader(config, pageNumber));
                break;
            case HeaderStyle.Detailed:
                headerBuilder.Append(GenerateDetailedHeader(config, pageNumber));
                break;
            case HeaderStyle.Custom:
                if (!string.IsNullOrWhiteSpace(config.CustomContent))
                {
                    headerBuilder.Append(GenerateCustomHeader(config.CustomContent, pageNumber));
                }
                else
                {
                    // 如果没有自定义内容，默认使用标准样式
                    headerBuilder.Append(GenerateStandardHeader(config, pageNumber));
                }
                break;
        }

        // 添加考生信息填写区域
        if (config.ShowStudentInfo && pageNumber == 1)
        {
            headerBuilder.Append(GenerateStudentInfo(config.StudentInfo));
        }

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成抬头LaTeX代码（兼容旧版本）
    /// </summary>
    /// <param name="template">试卷模板</param>
    /// <param name="examTime">考试时间（分钟）</param>
    /// <param name="totalPoints">总分</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    public string GenerateHeader(ExamTemplate template, int examTime, decimal totalPoints, int pageNumber)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // 如果模板有HeaderConfig，使用新的方法
        if (template.HeaderConfig != null)
        {
            // 更新HeaderConfig中的基本信息
            template.HeaderConfig.ExamTitle = string.IsNullOrWhiteSpace(template.HeaderConfig.ExamTitle) ? template.Name : template.HeaderConfig.ExamTitle;
            template.HeaderConfig.Subject = string.IsNullOrWhiteSpace(template.HeaderConfig.Subject) ? template.Description : template.HeaderConfig.Subject;
            template.HeaderConfig.ExamTime = template.HeaderConfig.ExamTime <= 0 ? examTime : template.HeaderConfig.ExamTime;
            template.HeaderConfig.TotalPoints = template.HeaderConfig.TotalPoints <= 0 ? totalPoints : template.HeaderConfig.TotalPoints;
            
            return GenerateHeader(template.HeaderConfig, pageNumber);
        }

        // 否则使用旧的实现
        var headerBuilder = new StringBuilder();

        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加试卷标题
        headerBuilder.AppendLine($@"{{\Large \textbf{{{EscapeLaTeX(template.Name)}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加考试科目
        if (!string.IsNullOrWhiteSpace(template.Description))
        {
            headerBuilder.AppendLine($@"考试科目：{EscapeLaTeX(template.Description)}\\");
        }

        // 添加考试时间
        headerBuilder.AppendLine($@"考试时间：{examTime}分钟\\");

        // 添加总分
        headerBuilder.AppendLine($@"总分：{totalPoints}分");

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成抬头LaTeX代码（简化版）
    /// </summary>
    /// <param name="examTitle">试卷标题</param>
    /// <param name="subject">考试科目</param>
    /// <param name="examTime">考试时间（分钟）</param>
    /// <param name="totalPoints">总分</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>抬头LaTeX代码</returns>
    public string GenerateHeader(string examTitle, string subject, int examTime, decimal totalPoints, int pageNumber)
    {
        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加试卷标题
        headerBuilder.AppendLine($@"{{\Large \textbf{{{EscapeLaTeX(examTitle)}}}}}");
        headerBuilder.AppendLine(@"\vspace{0.5cm}");

        // 添加考试科目
        if (!string.IsNullOrWhiteSpace(subject))
        {
            headerBuilder.AppendLine($@"考试科目：{EscapeLaTeX(subject)}\\");
        }

        // 添加考试时间
        headerBuilder.AppendLine($@"考试时间：{examTime}分钟\\");

        // 添加总分
        headerBuilder.AppendLine($@"总分：{totalPoints}分");

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 生成自定义抬头LaTeX代码
    /// </summary>
    /// <param name="customContent">自定义抬头内容</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>自定义抬头LaTeX代码</returns>
    public string GenerateCustomHeader(string customContent, int pageNumber)
    {
        // 抬头只在第一页显示
        if (pageNumber > 1)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(customContent))
        {
            return string.Empty;
        }

        var headerBuilder = new StringBuilder();

        // 开始抬头部分
        headerBuilder.AppendLine(@"% 自定义抬头部分");
        headerBuilder.AppendLine(@"\begin{center}");

        // 添加自定义内容
        headerBuilder.AppendLine(EscapeLaTeX(customContent));

        // 结束抬头部分
        headerBuilder.AppendLine(@"\end{center}");

        return headerBuilder.ToString();
    }

    /// <summary>
    /// 转义LaTeX特殊字符
    /// </summary>
    /// <param name="text">要转义的文本</param>
    /// <returns>转义后的文本</returns>
    private string EscapeLaTeX(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // LaTeX特殊字符转义
        var escapedText = text
            .Replace("\\", "\\textbackslash ")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("#", "\\#")
            .Replace("$", "\\$")
            .Replace("%", "\\%")
            .Replace("&", "\\&")
            .Replace("_", "\\_")
            .Replace("^", "\\textasciicircum ")
            .Replace("~", "\\textasciitilde ")
            .Replace("<", "$<$")
            .Replace(">", "$>$");

        return escapedText;
    }
}