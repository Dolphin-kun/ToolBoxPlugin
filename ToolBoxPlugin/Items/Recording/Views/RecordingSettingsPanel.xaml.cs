using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ToolBox.Items.Recording.Settings;

namespace ToolBox.Items.Recording.Views
{
    public partial class RecordingSettingsPanel : UserControl
    {
        public static readonly DependencyProperty DeviceListProperty =
            DependencyProperty.Register(nameof(DeviceList), typeof(List<KeyValuePair<int, string>>), typeof(RecordingSettingsPanel));

        public List<KeyValuePair<int, string>> DeviceList
        {
            get => (List<KeyValuePair<int, string>>?)GetValue(DeviceListProperty) ?? [];
            set => SetValue(DeviceListProperty, value);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RecordingSettingsPanel),
                new PropertyMetadata(false));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public RecordingSettingsPanel() : this(null) { }

        public RecordingSettingsPanel(RecordingSettings? recordingSettings)
        {
            InitializeComponent();
            DataContext = recordingSettings;

            var devices = RecordingCommand.GetAvailableDevices();
            DeviceList = [.. devices.Select((name, index) => new KeyValuePair<int, string>(index, name))];

            Loaded += (_, _) => AttachDropDownHandlers();
        }


        private void AttachDropDownHandlers()
        {
            var popup = FindVisualChild<Popup>(DeviceComboBox);
            if (popup != null)
            {
                HookPopup(popup);
                return;
            }

            DeviceComboBox.LayoutUpdated += DeviceComboBox_LayoutUpdated;
        }

        private void DeviceComboBox_LayoutUpdated(object? sender, EventArgs e)
        {
            var popup = FindVisualChild<Popup>(DeviceComboBox);
            if (popup == null) return;

            DeviceComboBox.LayoutUpdated -= DeviceComboBox_LayoutUpdated;
            HookPopup(popup);
        }

        private void HookPopup(Popup popup)
        {
            popup.Opened += (_, _) => IsDropDownOpen = true;
            popup.Closed += (_, _) => IsDropDownOpen = false;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;

                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }

            return null;
        }
    }
}
