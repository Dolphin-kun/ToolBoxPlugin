using System.Windows.Input;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public static class HotkeyGesture
    {
        public static string? Format(Key key, ModifierKeys modifiers)
        {
            if (key is Key.None)
                return null;

            var parts = new List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            parts.Add(key.ToString());

            return string.Join("+", parts);
        }

        public static KeyGesture? Parse(string? gesture)
        {
            if (string.IsNullOrWhiteSpace(gesture))
                return null;

            var modifiers = ModifierKeys.None;
            Key? key = null;

            foreach (var raw in gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (raw.ToLowerInvariant())
                {
                    case "ctrl" or "control": modifiers |= ModifierKeys.Control; break;
                    case "alt": modifiers |= ModifierKeys.Alt; break;
                    case "shift": modifiers |= ModifierKeys.Shift; break;
                    case "win" or "windows": modifiers |= ModifierKeys.Windows; break;
                    default:
                        if (!Enum.TryParse<Key>(raw, ignoreCase: true, out var parsed))
                            return null;
                        key = parsed;
                        break;
                }
            }

            if (key is null or Key.None)
                return null;

            try
            {
                return new KeyGesture(key.Value, modifiers);
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        /// <summary>実際に割り当てとして使えるジェスチャなら返す。使えない場合は null。</summary>
        public static KeyGesture? ParseAssignable(ToolItemSettings item)
        {
            if (!item.IsInvokable)
                return null;

            var gesture = Parse(item.HotkeyGesture);
            return gesture != null && IsAssignable(gesture.Modifiers) ? gesture : null;
        }

        public static bool IsAssignable(ModifierKeys modifiers) =>
            !modifiers.HasFlag(ModifierKeys.Windows)
            && (modifiers.HasFlag(ModifierKeys.Control) || modifiers.HasFlag(ModifierKeys.Alt));
    }
}
