using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;

namespace ToolBox.Controls
{
    public class ToolItemView : UserControl
    {
        private readonly AdornerUtil adornerUtil = new();

        public bool IsSettingsPanelOpen => adornerUtil.IsVisible;

        protected virtual FrameworkElement? CreateSettingsPanel() => ItemSettingsPanel.Create(DataContext);

        public void CloseSettingsPanel() => adornerUtil.Hide();

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (e.Handled)
                return;

            if (adornerUtil.IsVisible)
            {
                adornerUtil.Hide();
                e.Handled = true;
                return;
            }

            if (CreateSettingsPanel() is not { } panel)
                return;

            adornerUtil.Show(this, () => panel);
            e.Handled = true;
        }
    }
}
