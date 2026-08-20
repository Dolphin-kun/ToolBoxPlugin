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

        private bool isCapturing;

        public HotkeyBox()
        {
            IsReadOnly = true;
            IsReadOnlyCaretVisible = false;
            Cursor = Cursors.Hand;
            UpdateText();

            Unloaded += (_, _) => StopCapturing();
        }

        private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyBox box)
                box.UpdateText();
        }

        private void UpdateText() =>
            SetCurrentValue(TextProperty, string.IsNullOrEmpty(Gesture) ? "(未設定)" : Gesture);

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            StartCapturing();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            StopCapturing();
            UpdateText();
        }

        private void StartCapturing()
        {
            if (isCapturing)
                return;

            isCapturing = true;
            PanelKeyRouter.AddFirst(OnRoutedKey);
        }

        private void StopCapturing()
        {
            if (!isCapturing)
                return;

            isCapturing = false;
            PanelKeyRouter.Remove(OnRoutedKey);
        }

        private bool OnRoutedKey(Key key, ModifierKeys modifiers) => Capture(key, modifiers);

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            e.Handled = Capture(key, Keyboard.Modifiers);
        }

        private bool Capture(Key key, ModifierKeys modifiers)
        {
            if (key is Key.Escape or Key.Back or Key.Delete)
            {
                SetCurrentValue(GestureProperty, string.Empty);
                return true;
            }

            if (IsModifierKey(key))
                return true;

            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                SetCurrentValue(TextProperty, "Win キーは使えません");
                return true;
            }

            if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Alt))
            {
                SetCurrentValue(TextProperty, "Ctrl か Alt を含めてください");
                return true;
            }

            var gesture = HotkeyGesture.Format(key, modifiers);
            if (gesture == null)
                return true;

            SetCurrentValue(GestureProperty, gesture);
            UpdateText();
            return true;
        }

        private static bool IsModifierKey(Key key) => key
            is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.System;
    }
}
