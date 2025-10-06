using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoCommander.CloudApi;

/// <summary>
/// Custom converter for user_index that handles both integer and array
/// </summary>
public class UserIndexConverter: JsonConverter<ICollection<int>>
{
    public override ICollection<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch {
            JsonTokenType.Number => [reader.GetInt32()],
            JsonTokenType.String => int.TryParse(reader.GetString(), out var value) ? [value] : [],
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<int>>(ref reader, options) ?? [],
            JsonTokenType.Null => [],
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };

    public override void Write(Utf8JsonWriter writer, ICollection<int> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value) {
            writer.WriteNumberValue(item);
        }
        writer.WriteEndArray();
    }
}
