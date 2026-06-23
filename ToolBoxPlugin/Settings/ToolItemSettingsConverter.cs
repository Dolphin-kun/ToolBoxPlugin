using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToolBox.Settings
{
    public sealed class ToolItemSettingsConverter : JsonConverter<ToolItemSettings>
    {
        private static readonly Lazy<Dictionary<string, Type>> TypeMap = new(() =>
        {
            var baseType = typeof(ToolItemSettings);
            var dict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in baseType.Assembly
                         .GetTypes()
                         .Where(t => !t.IsAbstract && t != baseType && baseType.IsAssignableFrom(t)))
            {
                var key = GetDiscriminator(type);
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!dict.ContainsKey(key)) dict[key] = type;
            }

            return dict;
        });

        public override ToolItemSettings? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);

            if (!doc.RootElement.TryGetProperty("$type", out var typeProp))
            {
                return DeserializeFallback(doc, options);
            }

            var typeName = typeProp.GetString();
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return DeserializeFallback(doc, options);
            }

            if (!TypeMap.Value.TryGetValue(typeName, out var targetType))
            {
                return DeserializeFallback(doc, options);
            }

            return (ToolItemSettings?)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), targetType, options);
        }

        public override void Write(Utf8JsonWriter writer, ToolItemSettings value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            var discriminator = string.IsNullOrWhiteSpace(value.Name)
                ? GetDiscriminator(type)
                : value.Name;
            var safeOptions = CreateOptionsWithoutSelf(options);
            var element = JsonSerializer.SerializeToElement(value, type, safeOptions);

            writer.WriteStartObject();
            writer.WriteString("$type", discriminator);

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.NameEquals("$type")) continue;
                prop.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        private static ToolItemSettings? DeserializeFallback(JsonDocument doc, JsonSerializerOptions options)
        {
            var safeOptions = CreateOptionsWithoutSelf(options);
            return JsonSerializer.Deserialize<ToolItemSettings>(doc.RootElement.GetRawText(), safeOptions);
        }

        private static string GetDiscriminator(Type type)
        {
            try
            {
                if (Activator.CreateInstance(type) is ToolItemSettings settings)
                {
                    if (!string.IsNullOrWhiteSpace(settings.Name))
                        return settings.Name;
                }
            }
            catch { }

            var name = type.Name;
            return name.EndsWith("Settings", StringComparison.Ordinal)
                ? name[..^"Settings".Length]
                : name;
        }

        private static JsonSerializerOptions CreateOptionsWithoutSelf(JsonSerializerOptions options)
        {
            var clone = new JsonSerializerOptions(options);
            for (int i = clone.Converters.Count - 1; i >= 0; i--)
            {
                if (clone.Converters[i] is ToolItemSettingsConverter)
                {
                    clone.Converters.RemoveAt(i);
                }
            }

            return clone;
        }
    }
}
