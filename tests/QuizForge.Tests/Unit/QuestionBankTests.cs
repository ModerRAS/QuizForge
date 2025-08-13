using QuizForge.Models;
using Xunit;

namespace QuizForge.Tests.Unit;

/// <summary>
/// 题库单元测试
/// </summary>
public class QuestionBankTests
{
    [Fact]
    public void QuestionBank_CreateWithDefaultValues_ShouldHaveEmptyCollections()
    {
        // Arrange & Act
        var questionBank = new QuestionBank();
        
        // Assert
        Assert.NotNull(questionBank);
        Assert.Equal(Guid.Empty, questionBank.Id);
        Assert.Empty(questionBank.Name);
        Assert.Empty(questionBank.Description);
        Assert.NotNull(questionBank.Questions);
        Assert.Empty(questionBank.Questions);
    }
    
    [Fact]
    public void QuestionBank_CreateWithValues_ShouldHaveCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "测试题库";
        var description = "这是一个测试题库";
        var createdAt = DateTime.Now;
        var updatedAt = DateTime.Now;
        
        // Act
        var questionBank = new QuestionBank
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        
        // Assert
        Assert.Equal(id, questionBank.Id);
        Assert.Equal(name, questionBank.Name);
        Assert.Equal(description, questionBank.Description);
        Assert.Equal(createdAt, questionBank.CreatedAt);
        Assert.Equal(updatedAt, questionBank.UpdatedAt);
        Assert.NotNull(questionBank.Questions);
        Assert.Empty(questionBank.Questions);
    }
}