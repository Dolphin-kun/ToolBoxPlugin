using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ToolBox.Items.TextTag.Tags;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.TextTag.Views
{
    public partial class TextTagWindow : Window
    {
        public sealed class Model : Bindable
        {
            public ObservableCollection<TextTagTarget> Targets { get; } = [];
            public ObservableCollection<TextTagButton> RangeButtons { get; } = [];
            public ObservableCollection<TextTagButton> InsertButtons { get; } = [];

            private TextTagTarget? selectedTarget;
            public TextTagTarget? SelectedTarget
            {
                get => selectedTarget;
                set => Set(ref selectedTarget, value);
            }
        }

        private static readonly TimeSpan PushDelay = TimeSpan.FromMilliseconds(400);

        private readonly Model model = new();
        private readonly DispatcherTimer pushTimer;
        private TextTagButton? pending;
        private bool loading;

        public IReadOnlyList<TextTagTarget> ChangedTargets =>
            [.. model.Targets.Where(target => target.IsChanged)];

        public TextTagWindow(IEnumerable<IItem> items)
        {
            InitializeComponent();

            foreach (var target in items.Select(TextTagTarget.From).OfType<TextTagTarget>())
                model.Targets.Add(target);

            foreach (var definition in TextTagDefinition.All)
            {
                var button = new TextTagButton(definition);
                if (definition.NeedsSelection)
                    model.RangeButtons.Add(button);
                else
                    model.InsertButtons.Add(button);
            }

            pushTimer = new DispatcherTimer { Interval = PushDelay };
            pushTimer.Tick += (_, _) => PushNow();

            model.SelectedTarget = model.Targets.FirstOrDefault();
            model.PropertyChanged += OnModelPropertyChanged;

            DataContext = model;
            Loaded += (_, _) => LoadTarget();
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.SelectedTarget))
                Dispatcher.BeginInvoke(LoadTarget);
        }

        private void LoadTarget()
        {
            PushNow();

            loading = true;
            SourceBox.Text = model.SelectedTarget?.Text ?? string.Empty;
            SourceBox.IsUndoEnabled = false;
            SourceBox.IsUndoEnabled = true;
            loading = false;

            UpdateState();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (loading)
                return;

            if (model.SelectedTarget is { } target)
                target.Text = SourceBox.Text;

            pushTimer.Stop();
            pushTimer.Start();

            UpdateState();
        }

        private void PushNow()
        {
            pushTimer.Stop();

            foreach (var target in model.Targets)
                target.Push();
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e) => UpdateState();

        private void UpdateState()
        {
            var hasSelection = SourceBox.SelectionLength > 0;

            foreach (var button in model.RangeButtons)
                button.IsEnabled = hasSelection;

            RangeHint.Text = hasSelection ? Texts.TextTagRangeReady : Texts.TextTagRangeHint;
            UndoButton.IsEnabled = SourceBox.CanUndo;
        }

        private void OnTagButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: TextTagButton button })
                return;

            if (button.Definition.Fields.Count == 0)
            {
                Apply(button);
                return;
            }

            pending = button;
            PopupTitle.Text = button.Label;
            PopupFields.Children.Clear();
            PopupFields.Children.Add(new TextTagParameterPanel(button.Definition, button.Parameters, UpdatePopupPreview));

            UpdatePopupPreview();
            ParameterPopup.IsOpen = true;
        }

        private void UpdatePopupPreview()
        {
            if (pending is not { } button)
                return;

            var body = SourceBox.SelectionLength > 0 ? SourceBox.SelectedText : Texts.TextTagSampleBody;
            var open = TextTagBuilder.BuildOpen(button.Definition.Kind, button.Parameters, body);
            var close = TextTagBuilder.BuildClose(button.Definition.Kind, button.Parameters);

            PopupPreview.Text = button.Definition.Apply switch
            {
                TextTagApply.Wrap => open + body + close,
                TextTagApply.Replace => open,
                _ => open,
            };
        }

        private void OnPopupApplyClick(object sender, RoutedEventArgs e)
        {
            ParameterPopup.IsOpen = false;

            if (pending is { } button)
                Apply(button);

            pending = null;
        }

        private void OnPopupCancelClick(object sender, RoutedEventArgs e)
        {
            ParameterPopup.IsOpen = false;
            pending = null;
            SourceBox.Focus();
        }

        private void Apply(TextTagButton button)
        {
            if (button.Definition.NeedsSelection && SourceBox.SelectionLength == 0)
                return;

            TextTagEditor.Apply(SourceBox, button.Definition, button.Parameters);
            SourceBox.Focus();
            UpdateState();
        }

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            if (SourceBox.CanUndo)
                SourceBox.Undo();

            SourceBox.Focus();
            UpdateState();
        }

        private void OnRevertClick(object sender, RoutedEventArgs e)
        {
            if (model.SelectedTarget is { } target)
                SourceBox.Text = target.Original;

            SourceBox.Focus();
        }

        private void OnCommitClick(object sender, RoutedEventArgs e)
        {
            PushNow();
            DialogResult = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            pushTimer.Stop();

            if (DialogResult != true)
            {
                foreach (var target in model.Targets)
                    target.Restore();
            }

            base.OnClosed(e);
        }
    }
}
