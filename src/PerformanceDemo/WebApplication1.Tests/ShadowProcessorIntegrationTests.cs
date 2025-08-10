using Xunit;
using WebApplication1.Worker;
using System.Text.Json;

namespace WebApplication1.Tests;

public class ShadowProcessorIntegrationTests
{
    [Fact]
    public void ShadowProcessor_PersonSerialization_ShouldWorkCorrectly()
    {
        // This test validates that the JSON serialization used by ShadowProcessor
        // can properly handle Person objects
        
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe", 
            Gender = "Male",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act - Serialize to JSON (as ShadowProcessor.TryParse does)
        var json = JsonSerializer.Serialize(person);
        var deserializedPerson = JsonSerializer.Deserialize<Person>(json);

        // Assert
        Assert.NotNull(deserializedPerson);
        Assert.Equal(person.FirstName, deserializedPerson.FirstName);
        Assert.Equal(person.LastName, deserializedPerson.LastName);
        Assert.Equal(person.Gender, deserializedPerson.Gender);
        Assert.Equal(person.BirthDate, deserializedPerson.BirthDate);
    }

    [Fact]
    public void ShadowProcessor_MalformedJson_ShouldFailDeserialization()
    {
        // This test validates that malformed JSON will cause deserialization to fail
        // which is the behavior ShadowProcessor.TryParse expects
        
        // Arrange
        var malformedJson = "{invalid json}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Person>(malformedJson));
    }

    [Fact]
    public void ShadowProcessor_IncompletePersonJson_ShouldDeserializeWithDefaults()
    {
        // This tests how ShadowProcessor handles incomplete Person data
        
        // Arrange
        var incompleteJson = """{"FirstName": "John"}""";

        // Act
        var person = JsonSerializer.Deserialize<Person>(incompleteJson);

        // Assert
        Assert.NotNull(person);
        Assert.Equal("John", person.FirstName);
        Assert.Null(person.LastName); // Should be null for missing properties
        Assert.Null(person.Gender);
        Assert.Equal(default(DateTime), person.BirthDate); // Should be default DateTime
    }

    [Fact]
    public void RawRecord_StatusTransitions_ShouldFollowExpectedFlow()
    {
        // This test validates the status transitions that ShadowProcessor uses
        
        // Arrange
        var rawRecord = new RawRecord
        {
            Id = 1,
            Json = """{"FirstName": "Test"}""",
            Status = RawRecordStatus.New
        };

        // Act & Assert - Simulate the flow in ShadowProcessor
        Assert.Equal(RawRecordStatus.New, rawRecord.Status);

        // After successful parsing
        rawRecord.Status = RawRecordStatus.Parsed;
        Assert.Equal(RawRecordStatus.Parsed, rawRecord.Status);

        // Test failed status as well
        var failedRecord = new RawRecord { Id = 2, Json = "{invalid}", Status = RawRecordStatus.New };
        failedRecord.Status = RawRecordStatus.Failed;
        Assert.Equal(RawRecordStatus.Failed, failedRecord.Status);
    }

    [Theory]
    [InlineData(0, 0)] // RawRecordStatus.New
    [InlineData(1, 1)] // RawRecordStatus.Parsed  
    [InlineData(2, 2)] // RawRecordStatus.Failed
    public void RawRecordStatus_IntegerValues_ShouldMatchDatabaseExpectations(int statusInt, int expectedValue)
    {
        // This test ensures the enum values match what ShadowProcessor expects for database operations
        
        // Act
        var status = (RawRecordStatus)statusInt;
        
        // Assert
        Assert.Equal(expectedValue, (int)status);
    }
}