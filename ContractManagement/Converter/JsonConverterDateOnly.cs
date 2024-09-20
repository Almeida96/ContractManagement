using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractManagement.Converters
{
    public class JsonConverterDateOnly : JsonConverter<DateTime>
    {
        private readonly string _format = "dd-MM-yyyy"; // Define o formato da data

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Lê e converte a data do formato especificado para DateTime
            if (DateTime.TryParseExact(reader.GetString(), _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new JsonException($"Unable to convert \"{reader.GetString()}\" to a DateTime with format {_format}.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Converte DateTime para o formato especificado
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}
