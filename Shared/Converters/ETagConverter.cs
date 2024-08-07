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


//using System;
//using Newtonsoft.Json;
//using Azure;
//
//namespace BlazorApp.Shared.Converters
//{
//    public class ETagConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(ETag);
//        }
//
//        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
//        {
//            // Implement deserialization logic if needed
//            ETag result = new ETag(string.Empty);
//
//            if (reader!=null)
//                if (reader.Value != null)
//                    result = new ETag(reader.Value.ToString()??"");
//            
//            return result;
//        }
//
//        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//        {
//            if (value is ETag eTag)
//            {
//                // Convert ETag to string and write it to the JSON output
//                writer.WriteValue(eTag.ToString());
//            }
//            else
//            {
//                // Handle other cases (e.g., null or unexpected types)
//                writer.WriteNull();
//            }
//        }
//    }
//}
