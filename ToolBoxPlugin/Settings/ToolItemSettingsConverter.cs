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

        /// <summary>
        /// 判別子から具象型を解決して読み込む。
        /// 解決できない場合 (アドオンが削除された等) は null を返して読み飛ばす。
        /// ここで ToolItemSettings 自身へフォールバックすると、
        /// クラスに付いた [JsonConverter] 属性経由でこのコンバーターが再突入し無限再帰になるため行わない。
        /// </summary>
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

            // アドオンが提供する設定型は別アセンブリにあり、読み込み状況で変わるため都度探す
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
