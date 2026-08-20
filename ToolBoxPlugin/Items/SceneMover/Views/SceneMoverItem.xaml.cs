using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Items.SceneMover.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.SceneMover.Views
{
    public partial class SceneMoverItem : UserControl
    {
        private const string Caption = "シーンへ移動";

        private readonly AdornerUtil adornerUtil = new();

        public SceneMoverItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not SceneMoverSettings settings)
                return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new SceneMoverSettingsPanel { DataContext = settings });
        }

        private void OnOpenMenu(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is not Button button)
                    return;

                e.Handled = true;

                var menu = BuildMenu(DataContext as SceneMoverSettings);
                menu.PlacementTarget = button;
                menu.Placement = PlacementMode.Bottom;
                menu.IsOpen = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open the scene menu: {ex}");
                MessageBox.Show(
                    $"移動先の一覧を開けませんでした。\n{ex.Message}",
                    Caption,
                    MessageBoxButton.OK);
            }
        }

        private ContextMenu BuildMenu(SceneMoverSettings? settings)
        {
            var menu = new ContextMenu();
            var context = FindContext();

            if (context == null)
            {
                menu.Items.Add(new MenuItem { Header = "ツールボックスを初期化できていません。", IsEnabled = false });
                return menu;
            }

            var newScene = new MenuItem { Header = "新しいシーンへ" };
            newScene.Click += (_, _) => SceneMoverCommand.MoveToNewScene(context, settings);
            menu.Items.Add(newScene);

            var others = SceneMoverCommand.GetOtherScenes(context);
            if (others.Count == 0)
            {
                menu.Items.Add(new Separator());
                menu.Items.Add(new MenuItem { Header = "ほかのシーンはありません", IsEnabled = false });
                return menu;
            }

            menu.Items.Add(new Separator());

            foreach (var timeline in others)
            {
                var target = timeline;
                var entry = new MenuItem { Header = string.IsNullOrWhiteSpace(target.Name) ? "(名前なし)" : target.Name };
                entry.Click += (_, _) => SceneMoverCommand.MoveToScene(context, settings, target);
                menu.Items.Add(entry);
            }

            return menu;
        }

        private IToolBoxContext? FindContext()
        {
            DependencyObject? current = this;
            while (current != null)
            {
                if (current is FrameworkElement { DataContext: ToolBoxViewModel owner })
                    return owner;

                current = VisualTreeHelper.GetParent(current);
            }

            return ToolBoxViewModel.Instance;
        }
    }
}
