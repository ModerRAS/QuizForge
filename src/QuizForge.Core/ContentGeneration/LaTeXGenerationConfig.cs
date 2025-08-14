using System.Text.Json.Serialization;

namespace QuizForge.Core.ContentGeneration;

/// <summary>
/// LaTeX生成配置
/// </summary>
public class LaTeXGenerationConfig
{
    /// <summary>
    /// 文档类
    /// </summary>
    [JsonPropertyName("documentClass")]
    public string DocumentClass { get; set; } = "article";

    /// <summary>
    /// 字体大小
    /// </summary>
    [JsonPropertyName("fontSize")]
    public string FontSize { get; set; } = "12pt";

    /// <summary>
    /// 页面边距
    /// </summary>
    [JsonPropertyName("pageMargin")]
    public string PageMargin { get; set; } = "1in";

    /// <summary>
    /// 是否包含ctex包（用于中文支持）
    /// </summary>
    [JsonPropertyName("includeCtex")]
    public bool IncludeCtex { get; set; } = true;

    /// <summary>
    /// 是否包含数学包
    /// </summary>
    [JsonPropertyName("includeMathPackages")]
    public bool IncludeMathPackages { get; set; } = true;

    /// <summary>
    /// 是否包含图形包
    /// </summary>
    [JsonPropertyName("includeGraphicPackages")]
    public bool IncludeGraphicPackages { get; set; } = true;

    /// <summary>
    /// 是否包含表格包
    /// </summary>
    [JsonPropertyName("includeTablePackages")]
    public bool IncludeTablePackages { get; set; } = true;

    /// <summary>
    /// 题目编号格式
    /// </summary>
    [JsonPropertyName("questionNumberFormat")]
    public string QuestionNumberFormat { get; set; } = "题目 {0}";

    /// <summary>
    /// 选项格式
    /// </summary>
    [JsonPropertyName("optionFormat")]
    public string OptionFormat { get; set; } = "\\item {0}";

    /// <summary>
    /// 填空线长度
    /// </summary>
    [JsonPropertyName("blankLineLength")]
    public string BlankLineLength { get; set; } = "3cm";

    /// <summary>
    /// 简答题答题区域高度
    /// </summary>
    [JsonPropertyName("essayAnswerHeight")]
    public string EssayAnswerHeight { get; set; } = "5cm";

    /// <summary>
    /// 是否显示分值
    /// </summary>
    [JsonPropertyName("showPoints")]
    public bool ShowPoints { get; set; } = true;

    /// <summary>
    /// 是否显示难度
    /// </summary>
    [JsonPropertyName("showDifficulty")]
    public bool ShowDifficulty { get; set; } = false;

    /// <summary>
    /// 是否显示类别
    /// </summary>
    [JsonPropertyName("showCategory")]
    public bool ShowCategory { get; set; } = false;

    /// <summary>
    /// 自定义LaTeX包
    /// </summary>
    [JsonPropertyName("customPackages")]
    public List<string> CustomPackages { get; set; } = new();

    /// <summary>
    /// 自定义LaTeX命令
    /// </summary>
    [JsonPropertyName("customCommands")]
    public List<string> CustomCommands { get; set; } = new();

    /// <summary>
    /// 答题卡选择题宽度
    /// </summary>
    [JsonPropertyName("answerSheetChoiceWidth")]
    public string AnswerSheetChoiceWidth { get; set; } = "3cm";

    /// <summary>
    /// 答题卡填空题宽度
    /// </summary>
    [JsonPropertyName("answerSheetBlankWidth")]
    public string AnswerSheetBlankWidth { get; set; } = "5cm";

    /// <summary>
    /// 答题卡简答题高度
    /// </summary>
    [JsonPropertyName("answerSheetEssayHeight")]
    public string AnswerSheetEssayHeight { get; set; } = "5cm";

    /// <summary>
    /// 章节命令
    /// </summary>
    [JsonPropertyName("sectionCommand")]
    public string SectionCommand { get; set; } = "subsection*";
}