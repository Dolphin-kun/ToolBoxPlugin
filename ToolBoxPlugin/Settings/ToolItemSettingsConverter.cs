using System.Text.Json;
using System.Text.Json.Serialization;
using ToolBox.Commons;

namespace ToolBox.Settings
{
    public sealed class ToolItemSettingsConverter : JsonConverter<ToolItemSettings>
    {
        private static readonly Lazy<Dictionary<string, Type>> BuiltInTypeMap = new(() =>
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
                return null;

            var typeName = typeProp.GetString();
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var targetType = ResolveType(typeName);
            if (targetType == null)
                return null;

            return (ToolItemSettings?)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), targetType, options);
        }

        private static Type? ResolveType(string typeName)
        {
            if (BuiltInTypeMap.Value.TryGetValue(typeName, out var builtIn))
                return builtIn;

            foreach (var settings in AddonLoader.GetAllSettings())
            {
                var addonType = settings.GetType();
                if (string.Equals(GetDiscriminator(addonType), typeName, StringComparison.OrdinalIgnoreCase))
                    return addonType;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, ToolItemSettings value, JsonSerializerOptions options)
        {
            var type = value.GetType();

            var discriminator = GetDiscriminator(type);
            var element = JsonSerializer.SerializeToElement(value, type, options);

            writer.WriteStartObject();
            writer.WriteString("$type", discriminator);

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.NameEquals("$type")) continue;
                prop.WriteTo(writer);
            }

            writer.WriteEndObject();
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
    }
}
