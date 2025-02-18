using FluentAssertions;
using Interview.Domain.Questions;
using Interview.Domain.Questions.UpsertQuestionTree;

namespace Interview.Test.Units;

public class UpsertQuestionTreeRequestTest
{
    [Fact]
    public void IsValidTree_WhenTreeHasNoNodes_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>(),
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has no nodes");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasNoParentNode_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has no parent node");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasMoreThanOneParentNode_ShouldReturnFalse()
    {
        // Arrange
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has more than one parent node");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasDuplicateNodeIds_ShouldReturnFalse()
    {
        // Arrange
        var duplicateId = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = duplicateId, ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = duplicateId, ParentQuestionSubjectTreeId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be($"Duplicate node id: {duplicateId}");
    }

    [Fact]
    public void IsValidTree_WhenTreeHasCycle_ShouldReturnFalse()
    {
        // Arrange
        var node1Id = Guid.NewGuid();
        var node2Id = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = node1Id, ParentQuestionSubjectTreeId = node2Id, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = node2Id, ParentQuestionSubjectTreeId = node1Id, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().Be("Tree has cycle");
    }

    [Fact]
    public void IsValidTree_WhenTreeIsValid_ShouldReturnTrue()
    {
        // Arrange
        var parentNodeId = Guid.NewGuid();
        var request = new UpsertQuestionTreeRequest
        {
            Tree = new List<UpsertQuestionSubjectTreeRequest>
            {
                new() { Id = parentNodeId, ParentQuestionSubjectTreeId = null, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 1 },
                new() { Id = Guid.NewGuid(), ParentQuestionSubjectTreeId = parentNodeId, QuestionId = Guid.NewGuid(), Type = EVQuestionSubjectTreeType.Question, Order = 2 }
            },
            Name = "t"
        };

        // Act
        var result = request.IsValidTree(out var errorMessage);

        // Assert
        result.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }
}
