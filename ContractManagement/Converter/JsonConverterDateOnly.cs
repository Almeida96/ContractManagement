using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractManagement.Converters;

public class JsonConverterDateOnly : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String &&
            DateTime.TryParseExact(reader.GetString(), ["yyyy-MM-dd", "dd-MM-yyyy"], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date)) return date;
        throw new JsonException("Use a date in yyyy-MM-dd format.");
    }
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
