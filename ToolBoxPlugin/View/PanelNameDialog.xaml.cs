using System.Windows;

namespace ToolBox.View
{
    /// <summary>パネル名を入力するための小さなダイアログ。</summary>
    public partial class PanelNameDialog : Window
    {
        public string PanelName => NameTextBox.Text.Trim();

        public PanelNameDialog(string currentName)
        {
            InitializeComponent();

            NameTextBox.Text = currentName;
            Loaded += (_, _) =>
            {
                NameTextBox.SelectAll();
                NameTextBox.Focus();
            };
        }

        private void OnOkClick(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
