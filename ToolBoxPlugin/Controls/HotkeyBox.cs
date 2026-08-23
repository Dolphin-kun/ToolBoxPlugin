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

        public static readonly DependencyProperty ScopeProperty =
            DependencyProperty.Register(nameof(Scope), typeof(HotkeyScope), typeof(HotkeyBox),
                new FrameworkPropertyMetadata(HotkeyScope.PanelItem));

        public HotkeyScope Scope
        {
            get => (HotkeyScope)GetValue(ScopeProperty);
            set => SetValue(ScopeProperty, value);
        }

        private bool isCapturing;

        public HotkeyBox()
        {
            IsReadOnly = true;
            IsReadOnlyCaretVisible = false;
            Cursor = Cursors.Hand;
            UpdateText();

            ContextMenuOpening += (_, e) => e.Handled = true;

            Unloaded += (_, _) => StopCapturing();
        }

        private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyBox box)
                box.UpdateText();
        }

        private void UpdateText() =>
            SetCurrentValue(TextProperty,
                string.IsNullOrEmpty(Gesture) ? Texts.CommonUnset : HotkeyGesture.Describe(Gesture));

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
            PanelInputRouter.AddFirst(Capture);
        }

        private void StopCapturing()
        {
            if (!isCapturing)
                return;

            isCapturing = false;
            PanelInputRouter.Remove(Capture);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            e.Handled = Capture(HotkeyTrigger.FromKey(key, Keyboard.Modifiers));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var trigger = HotkeyTrigger.FromMouse(e.ChangedButton, Keyboard.Modifiers);

            if (!HotkeyGesture.IsAssignable(trigger, Scope))
            {
                base.OnPreviewMouseDown(e);
                return;
            }

            e.Handled = Capture(trigger);
        }

        private bool Capture(HotkeyTrigger trigger)
        {
            if (!trigger.IsMouse)
            {
                if (trigger.Key is Key.Escape or Key.Back or Key.Delete)
                {
                    SetCurrentValue(GestureProperty, string.Empty);
                    return true;
                }

                if (IsModifierKey(trigger.Key))
                    return true;
            }

            if (trigger.Modifiers.HasFlag(ModifierKeys.Windows))
            {
                SetCurrentValue(TextProperty, Texts.HotkeyWinKeyNotAllowed);
                return true;
            }

            if (!HotkeyGesture.IsAssignable(trigger, Scope))
            {
                SetCurrentValue(TextProperty, Scope == HotkeyScope.RadialLauncher
                    ? Texts.HotkeyNeedMouseButton
                    : Texts.HotkeyNeedCtrlOrAlt);
                return true;
            }

            if (HotkeyGesture.Format(trigger, Scope) is not { } gesture)
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
