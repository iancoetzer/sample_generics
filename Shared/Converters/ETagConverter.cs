using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;

namespace BlazorApp.Shared.Converters
{
    public class ETagConverter : JsonConverter<ETag>
    {
        public override ETag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new ETag(reader.GetString() ?? string.Empty);
        }

        public override void Write(Utf8JsonWriter writer, ETag value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}