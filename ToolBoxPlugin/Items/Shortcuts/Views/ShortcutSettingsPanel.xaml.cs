using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;
using YukkuriMovieMaker.Resources.Localization;

namespace ToolBox.Items.Shortcuts.Views
{
    public partial class ShortcutSettingsPanel : UserControl
    {
        public ShortcutSettingsPanel()
        {
            InitializeComponent();
            
            var commandTypes = Enum.GetValues<YukkuriMovieMaker.Settings.CommandType>()
                .Cast<YukkuriMovieMaker.Settings.CommandType>()
                .Select(e => new 
                { 
                    Value = e, 
                    DisplayName = GetEnumDisplayName(e) 
                })
                .Where(x => x.DisplayName != null)
                .ToList();

            CommandTypeComboBox.ItemsSource = commandTypes;
        }

        private static string? GetEnumDisplayName(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null) return null;

            var displayAttr = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .Cast<DisplayAttribute>()
                .FirstOrDefault();

            if (displayAttr == null || string.IsNullOrEmpty(displayAttr.Name)) return null;

            if (!displayAttr.Name.StartsWith("CommandType") || !displayAttr.Name.EndsWith("Name")) return null;

            var localizedName = Texts.GetString(displayAttr.Name);
            if (!string.IsNullOrEmpty(localizedName)) return localizedName;

            return displayAttr.GetName() ?? displayAttr.Name;
        }
    }
}
