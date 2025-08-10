using Xunit;
using WebApplication1.Worker;
using System.Text.Json;

namespace WebApplication1.Tests;

public class PersonTests
{
    [Fact]
    public void Person_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Assert
        Assert.Equal("John", person.FirstName);
        Assert.Equal("Doe", person.LastName);
        Assert.Equal("Male", person.Gender);
        Assert.Equal(new DateTime(1990, 1, 1), person.BirthDate);
    }

    [Fact]
    public void Person_ShouldBeSerializableToJson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Gender = "Female",
            BirthDate = new DateTime(1985, 5, 15)
        };

        // Act
        var json = JsonSerializer.Serialize(person);
        var deserializedPerson = JsonSerializer.Deserialize<Person>(json);

        // Assert
        Assert.NotNull(deserializedPerson);
        Assert.Equal(person.FirstName, deserializedPerson.FirstName);
        Assert.Equal(person.LastName, deserializedPerson.LastName);
        Assert.Equal(person.Gender, deserializedPerson.Gender);
        Assert.Equal(person.BirthDate, deserializedPerson.BirthDate);
    }
}

public class RawRecordTests
{
    [Fact]
    public void RawRecord_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var rawRecord = new RawRecord
        {
            Id = 1,
            Json = """{"name": "test"}""",
            Status = RawRecordStatus.New
        };

        // Assert
        Assert.Equal(1, rawRecord.Id);
        Assert.Equal("""{"name": "test"}""", rawRecord.Json);
        Assert.Equal(RawRecordStatus.New, rawRecord.Status);
    }

    [Fact]
    public void RawRecord_StatusProperty_ShouldSupportAllEnumValues()
    {
        // Arrange
        var rawRecord = new RawRecord { Id = 1, Json = "{}" };

        // Act & Assert - Test all enum values
        rawRecord.Status = RawRecordStatus.New;
        Assert.Equal(RawRecordStatus.New, rawRecord.Status);

        rawRecord.Status = RawRecordStatus.Parsed;
        Assert.Equal(RawRecordStatus.Parsed, rawRecord.Status);

        rawRecord.Status = RawRecordStatus.Failed;
        Assert.Equal(RawRecordStatus.Failed, rawRecord.Status);
    }
}

public class RawRecordStatusTests
{
    [Fact]
    public void RawRecordStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)RawRecordStatus.New);
        Assert.Equal(1, (int)RawRecordStatus.Parsed);
        Assert.Equal(2, (int)RawRecordStatus.Failed);
    }

    [Fact]
    public void RawRecordStatus_ShouldBeConvertibleToString()
    {
        // Act & Assert
        Assert.Equal("New", RawRecordStatus.New.ToString());
        Assert.Equal("Parsed", RawRecordStatus.Parsed.ToString());
        Assert.Equal("Failed", RawRecordStatus.Failed.ToString());
    }
}