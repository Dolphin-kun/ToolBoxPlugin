using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;

namespace ToolBox.Controls
{
    public class HotkeyBox : TextBox
    {
        public static readonly DependencyProperty GestureProperty =
            DependencyProperty.Register(nameof(Gesture), typeof(string), typeof(HotkeyBox),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnGestureChanged));

        public string Gesture
        {
            get => (string)GetValue(GestureProperty);
            set => SetValue(GestureProperty, value);
        }

        public HotkeyBox()
        {
            IsReadOnly = true;
            IsReadOnlyCaretVisible = false;
            Cursor = Cursors.Hand;
            UpdateText();
        }

        private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyBox box)
                box.UpdateText();
        }

        private void UpdateText() =>
            SetCurrentValue(TextProperty, string.IsNullOrEmpty(Gesture) ? "(未設定)" : Gesture);

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            e.Handled = true;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key is Key.Escape or Key.Back or Key.Delete)
            {
                SetCurrentValue(GestureProperty, string.Empty);
                return;
            }

            if (IsModifierKey(key))
                return;

            var modifiers = Keyboard.Modifiers;

            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                SetCurrentValue(TextProperty, "Win キーは使えません");
                return;
            }

            if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Alt))
            {
                SetCurrentValue(TextProperty, "Ctrl か Alt を含めてください");
                return;
            }

            var gesture = HotkeyGesture.Format(key, modifiers);
            if (gesture != null)
                SetCurrentValue(GestureProperty, gesture);
        }

        protected override void OnLostFocus(System.Windows.RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdateText();
        }

        private static bool IsModifierKey(Key key) => key
            is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.System;
    }
}
