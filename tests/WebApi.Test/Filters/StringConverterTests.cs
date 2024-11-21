using System.Text.Json;
using FluentAssertions;
using MyRecipeBook.Filters;
using Xunit;

namespace WebApi.Test.Filters;

public class StringConverterTests
{
    private readonly StringConverter _converter = new();

    [Fact]
    public void Read_ShouldReturnNull_WhenJsonValueIsNull()
    {
        var json = "null"; // Simulate a null JSON value
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(utf8Bytes);

        reader.Read(); // Move to the 'Null' token

        var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

        result.Should().BeNull("The converter should return null when the JSON value is null");
    }

    [Fact]
    public void Read_ShouldTrimAndRemoveExtraWhiteSpaces_WhenJsonValueIsValidString()
    {
        var json = "\"   This   is   a   test   \""; 
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(utf8Bytes);

        reader.Read(); // Move to the 'String' token

        var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

        result.Should().Be("This is a test", "The converter should trim and replace extra white spaces");
    }

    [Fact]
    public void Read_ShouldReturnEmptyString_WhenJsonValueIsEmptyString()
    {
        var json = "\"\""; // JSON empty string
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(utf8Bytes);

        reader.Read(); // Move to the 'String' token

        var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

        result.Should().Be("", "The converter should return an empty string when the JSON value is an empty string");
    }

    [Fact]
    public void Write_ShouldWriteCorrectJsonValue()
    {
        var value = "Sample string";
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        
        _converter.Write(writer, value, new JsonSerializerOptions());
        writer.Flush();
        
        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        result.Should().Be("\"Sample string\"", "The converter should correctly write the JSON string");
    }
}
