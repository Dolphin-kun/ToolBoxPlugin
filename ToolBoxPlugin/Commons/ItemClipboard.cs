using System.Text.Json;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public static class ItemClipboard
    {
        private static string? copiedJson;

        public static bool HasContent => copiedJson != null;

        public static void Copy(ToolItemSettings settings)
        {
            copiedJson = JsonSerializer.Serialize<ToolItemSettings>(settings);
        }

        /// <summary>控えた内容から新しいアイテムを作る。貼り付けるたび別個体になるよう識別子は振り直す。</summary>
        public static ToolItemSettings? CreateCopy()
        {
            if (copiedJson == null)
                return null;

            try
            {
                var copy = JsonSerializer.Deserialize<ToolItemSettings>(copiedJson);
                if (copy == null)
                    return null;

                copy.Id = Guid.NewGuid();
                return copy;
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to paste item: {ex}");
                return null;
            }
        }
    }
}
