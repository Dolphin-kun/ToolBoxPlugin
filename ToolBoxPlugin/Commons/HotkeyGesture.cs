using System.Windows.Input;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public readonly record struct HotkeyTrigger(Key Key, MouseButton? MouseButton, ModifierKeys Modifiers)
    {
        public static HotkeyTrigger FromKey(Key key, ModifierKeys modifiers) =>
            new(key, null, modifiers);

        public static HotkeyTrigger FromMouse(MouseButton button, ModifierKeys modifiers) =>
            new(Key.None, button, modifiers);

        public bool IsMouse => MouseButton.HasValue;
    }

    public enum HotkeyScope
    {
        PanelItem,

        RadialLauncher,
    }

    public static class HotkeyGesture
    {
        private static readonly (MouseButton Button, string Token)[] MouseTokens =
        [
            (MouseButton.Left, "MouseLeft"),
            (MouseButton.Middle, "MouseMiddle"),
            (MouseButton.Right, "MouseRight"),
            (MouseButton.XButton1, "MouseX1"),
            (MouseButton.XButton2, "MouseX2"),
        ];

        public static string? Format(HotkeyTrigger trigger, HotkeyScope scope)
        {
            if (!IsAssignable(trigger, scope))
                return null;

            return trigger.MouseButton is { } button
                ? Join(trigger.Modifiers, TokenOf(button))
                : Join(trigger.Modifiers, trigger.Key.ToString());
        }

        public static HotkeyTrigger? Parse(string? gesture)
        {
            if (string.IsNullOrWhiteSpace(gesture))
                return null;

            var modifiers = ModifierKeys.None;
            Key? key = null;
            MouseButton? mouse = null;

            foreach (var raw in gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (raw.ToLowerInvariant())
                {
                    case "ctrl" or "control": modifiers |= ModifierKeys.Control; break;
                    case "alt": modifiers |= ModifierKeys.Alt; break;
                    case "shift": modifiers |= ModifierKeys.Shift; break;
                    case "win" or "windows": modifiers |= ModifierKeys.Windows; break;
                    default:
                        if (TryParseMouse(raw, out var button))
                        {
                            mouse = button;
                            break;
                        }

                        if (!Enum.TryParse<Key>(raw, ignoreCase: true, out var parsed))
                            return null;
                        key = parsed;
                        break;
                }
            }

            if (mouse is { } resolved)
                return HotkeyTrigger.FromMouse(resolved, modifiers);

            return key is null or Key.None ? null : HotkeyTrigger.FromKey(key.Value, modifiers);
        }

        public static string Describe(string? gesture)
        {
            if (Parse(gesture) is not { MouseButton: { } button } trigger)
                return gesture ?? string.Empty;

            var name = button switch
            {
                MouseButton.Left => Texts.HotkeyMouseLeft,
                MouseButton.Middle => Texts.HotkeyMouseMiddle,
                MouseButton.Right => Texts.HotkeyMouseRight,
                MouseButton.XButton1 => Texts.HotkeyMouseX1,
                MouseButton.XButton2 => Texts.HotkeyMouseX2,
                _ => TokenOf(button),
            };

            return Join(trigger.Modifiers, name);
        }

        public static HotkeyTrigger? ParseAssignable(ToolItemSettings item)
        {
            if (!item.IsInvokable)
                return null;

            return ParseAssignable(item.HotkeyGesture, HotkeyScope.PanelItem);
        }

        public static HotkeyTrigger? ParseAssignable(string? gesture, HotkeyScope scope) =>
            Parse(gesture) is { } trigger && IsAssignable(trigger, scope) ? trigger : null;

        public static bool IsAssignable(HotkeyTrigger trigger, HotkeyScope scope)
        {
            if (trigger.Modifiers.HasFlag(ModifierKeys.Windows))
                return false;

            if (trigger.MouseButton is { } button)
                return scope switch
                {
                    HotkeyScope.PanelItem => button is MouseButton.XButton1 or MouseButton.XButton2,
                    HotkeyScope.RadialLauncher => button != MouseButton.Left,
                    _ => false,
                };

            if (scope == HotkeyScope.RadialLauncher || trigger.Key is Key.None)
                return false;

            return trigger.Modifiers.HasFlag(ModifierKeys.Control)
                || trigger.Modifiers.HasFlag(ModifierKeys.Alt);
        }

        private static string TokenOf(MouseButton button) =>
            MouseTokens.FirstOrDefault(x => x.Button == button).Token ?? button.ToString();

        private static bool TryParseMouse(string raw, out MouseButton button)
        {
            foreach (var (candidate, token) in MouseTokens)
            {
                if (!string.Equals(raw, token, StringComparison.OrdinalIgnoreCase))
                    continue;

                button = candidate;
                return true;
            }

            button = default;
            return false;
        }

        private static string Join(ModifierKeys modifiers, string name)
        {
            var parts = new List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            parts.Add(name);

            return string.Join("+", parts);
        }
    }
}
