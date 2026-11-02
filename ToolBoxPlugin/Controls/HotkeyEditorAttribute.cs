using System.Reflection;
using System.Windows;
using System.Windows.Data;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Controls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HotkeyEditorAttribute : PropertyEditorAttribute
    {
        public HotkeyEditorAttribute() => PropertyEditorSize = PropertyEditorSize.FullWidth;

        public override FrameworkElement Create() => new HotkeyBox
        {
            Scope = HotkeyScope.PanelItem,
            Height = 24,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        public override void SetBindings(FrameworkElement control, object item, object propertyOwner, PropertyInfo propertyInfo)
        {
            if (control is HotkeyBox box)
            {
                box.SetBinding(HotkeyBox.GestureProperty, new Binding(propertyInfo.Name)
                {
                    Source = propertyOwner,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                });
            }
        }

        public override void ClearBindings(FrameworkElement control) =>
            BindingOperations.ClearBinding(control, HotkeyBox.GestureProperty);
    }
}
