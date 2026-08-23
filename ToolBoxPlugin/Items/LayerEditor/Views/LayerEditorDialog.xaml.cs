using System.Windows;
using ToolBox.Items.LayerEditor.Settings;

namespace ToolBox.Items.LayerEditor.Views
{
    public partial class LayerEditorDialog : Window
    {
        public bool ResetRequested { get; private set; }

        private readonly LayerEditorSettings settings;
        private readonly LayerEditorSettings snapshot = new();

        public LayerEditorDialog(LayerEditorSettings settings)
        {
            InitializeComponent();

            this.settings = settings;
            DataContext = settings;

            Copy(settings, snapshot);
            Closed += OnClosed;
        }

        private void OnApplyClick(object sender, RoutedEventArgs e) => DialogResult = true;

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ResetRequested = true;
            DialogResult = true;
        }

        private void OnClearColorClick(object sender, RoutedEventArgs e) =>
            settings.LayerColor = LayerEditorSettings.DefaultLayerColor;

        private void OnClosed(object? sender, EventArgs e)
        {
            if (DialogResult != true)
                Copy(snapshot, settings);
        }

        private static void Copy(LayerEditorSettings from, LayerEditorSettings to)
        {
            to.Scope = from.Scope;
            to.FromLayer = from.FromLayer;
            to.ToLayer = from.ToLayer;
            to.ChangeEnabled = from.ChangeEnabled;
            to.IsLayerEnabled = from.IsLayerEnabled;
            to.ChangeLabel = from.ChangeLabel;
            to.LabelTemplate = from.LabelTemplate;
            to.ChangeColor = from.ChangeColor;
            to.LayerColorHex = from.LayerColorHex;
            to.ChangeVolume = from.ChangeVolume;
            to.LayerVolume = from.LayerVolume;
        }
    }
}
