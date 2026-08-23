using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.View
{
    public partial class ToolBoxView : UserControl
    {
        private const int WM_SIZING = 0x0214;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private HwndSource? hwndSource;

        public ToolBoxView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += (_, _) => UpdateAutoFit();
        }

        private ToolBoxPanel? subscribedPanel;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ToolBoxSettings.Default.PropertyChanged += OnToolBoxSettingsPropertyChanged;

            if (DataContext is ToolBoxViewModel vm)
            {
                subscribedPanel = vm.Panel;
                subscribedPanel.PropertyChanged += OnPanelPropertyChanged;

                vm.EnableHotkeys();
            }

            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(OnWindowMessage);

            ToolBox.Commons.PanelInputRouter.Add(OnRoutedInput);

            UpdateAutoFit();
        }

        private bool OnRoutedInput(ToolBox.Commons.HotkeyTrigger trigger)
        {
            if (trigger.IsMouse || !IsMouseOver || trigger.Modifiers != ModifierKeys.Control)
                return false;

            if (DataContext is not ToolBoxViewModel vm)
                return false;

            switch (trigger.Key)
            {
                case Key.Z when vm.UndoLayoutCommand.CanExecute(null):
                    vm.UndoLayoutCommand.Execute(null);
                    return true;
                case Key.Y when vm.RedoLayoutCommand.CanExecute(null):
                    vm.RedoLayoutCommand.Execute(null);
                    return true;
                default:
                    return false;
            }
        }

        private IntPtr OnWindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_SIZING)
                return IntPtr.Zero;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return IntPtr.Zero;

            if (!GetWindowRect(hwnd, out var current))
                return IntPtr.Zero;

            Marshal.StructureToPtr(current, lParam, fDeleteOld: false);
            handled = true;
            return new IntPtr(1);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ToolBoxSettings.Default.PropertyChanged -= OnToolBoxSettingsPropertyChanged;

            subscribedPanel?.PropertyChanged -= OnPanelPropertyChanged;
            subscribedPanel = null;

            hwndSource?.RemoveHook(OnWindowMessage);
            hwndSource = null;

            ToolBox.Commons.PanelInputRouter.Remove(OnRoutedInput);
        }

        private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ToolBoxPanel.GridRowCount)
                or nameof(ToolBoxPanel.GridColumnCount))
            {
                UpdateAutoFit();
            }
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;
            vm.IsSettingsVisible = true;
        }

        private void OnPanelContextMenuClosed(object sender, RoutedEventArgs e) =>
            Dispatcher.BeginInvoke(DropGrid.RefreshCtrlState);

        private void OnPanelContextMenuOpened(object sender, RoutedEventArgs e) =>
            PasteItemMenuItem.IsEnabled = ToolBox.Commons.ItemClipboard.HasContent;

        private void OnPasteItemClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;
            vm.PasteItem();
        }

        private void OnAddPanelClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;
            vm.AddPanel();
        }

        private void OnDuplicatePanelClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;
            vm.DuplicatePanel();
        }

        private void OnRenamePanelClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;

            var dialog = new PanelNameDialog(vm.Panel.PanelName)
            {
                Owner = Window.GetWindow(this),
            };

            if (dialog.ShowDialog() == true)
                vm.Panel.PanelName = dialog.PanelName;
        }

        private void OnToolBoxSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ToolBoxSettings.GridAdjustMode)
                or nameof(ToolBoxSettings.AutoFitSize)
                or nameof(ToolBoxSettings.AutoFitLayout)
                or nameof(ToolBoxSettings.CellSize))
            {
                UpdateAutoFit();
            }
        }

        private void UpdateAutoFit()
        {
            var settings = ToolBoxSettings.Default;

            var isAuto = settings.GridAdjustMode == GridAdjustMode.Auto;
            var autoFitLayout = isAuto && settings.AutoFitLayout;
            var autoFitSize = isAuto && settings.AutoFitSize;

            if (!autoFitLayout)
            {
                BindingOperations.GetBindingExpression(DropGrid, ToolBox.Controls.DropGrid.ColumnCountProperty)?.UpdateTarget();
            }

            if (!autoFitLayout && !autoFitSize)
            {
                BindingOperations.GetBindingExpression(DropGrid, ToolBox.Controls.DropGrid.RowCountProperty)?.UpdateTarget();
            }

            if (!autoFitSize)
            {
                BindingOperations.GetBindingExpression(DropGrid, ToolBox.Controls.DropGrid.CellSizeProperty)?.UpdateTarget();
            }

            if (!autoFitLayout && !autoFitSize) return;
            if (ActualWidth <= 0 || ActualHeight <= 0) return;

            var baseCellSize = Math.Max(1, settings.CellSize);

            var columns = autoFitLayout
                ? Math.Max(1, (int)(autoFitSize
                    ? Math.Round(ActualWidth / baseCellSize)
                    : Math.Floor(ActualWidth / baseCellSize)))
                : Math.Max(1, DropGrid.ColumnCount);

            var cellSize = autoFitSize ? Math.Max(1, ActualWidth / columns) : baseCellSize;

            var rows = autoFitLayout || autoFitSize
                ? Math.Max(1, (int)(ActualHeight / cellSize))
                : Math.Max(1, DropGrid.RowCount);

            if (autoFitSize)
            {
                DropGrid.SetCurrentValue(ToolBox.Controls.DropGrid.CellSizeProperty, cellSize);
                DropGrid.SetCurrentValue(ToolBox.Controls.DropGrid.RowCountProperty, rows);
            }

            if (autoFitLayout)
            {
                DropGrid.SetCurrentValue(ToolBox.Controls.DropGrid.ColumnCountProperty, columns);
                DropGrid.SetCurrentValue(ToolBox.Controls.DropGrid.RowCountProperty, rows);
            }
        }
    }
}
