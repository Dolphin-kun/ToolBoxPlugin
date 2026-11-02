using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.View.Settings
{
    public partial class ToolItemInstancesPanel : UserControl
    {
        public ToolItemInstancesPanel()
        {
            InitializeComponent();

            var list = InstanceList.ListBox!;
            list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ItemEntryViewModelBase.Instances)));
            list.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty,
                new Binding(nameof(ItemEntryViewModelBase.SelectedInstance)) { Mode = BindingMode.TwoWay });
            list.ItemTemplate = (DataTemplate)FindResource("InstanceTemplate");

            DataContextChanged += (_, _) => BuildButtons();
        }

        private void BuildButtons()
        {
            if (DataContext is not ItemEntryViewModelBase entry || InstanceList.Tag == entry)
                return;

            InstanceList.Tag = entry;
            InstanceList.ClearButtons();
            InstanceList.AddCommandButton(GeometryKeys.Plus, Texts.ToolItemAddAnother, entry.AddInstanceCommand);
            InstanceList.AddCommandButton(GeometryKeys.Minus, Texts.ToolItemRemovePlacement, entry.RemoveInstanceCommand, passSelectedItem: true);
            InstanceList.AddMenuButton(GeometryKeys.DotsVertical, Texts.CommonMoreActions, menu =>
                menu.Items.Add(Controls.ListPanel.MenuItem(
                    Texts.ItemCommonResetColors,
                    () => entry.ResetAppearanceCommand.Execute(null),
                    entry.ResetAppearanceCommand.CanExecute(null))));
        }
    }
}
