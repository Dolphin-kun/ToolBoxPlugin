using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.LayerEditor.Settings;
using ToolBox.Items.LayerEditor.Views;
using YukkuriMovieMaker.Project;

namespace ToolBox.Items.LayerEditor
{
    internal static class LayerEditorCommand
    {
        public static bool CanExecute(IToolBoxContext ctx) => ctx.Timeline != null;

        public static void Open(IToolBoxContext ctx)
        {
            if (ctx.Timeline is not { } timeline || GetSettings(ctx) is not { } settings)
                return;

            var dialog = new LayerEditorDialog(settings)
            {
                Owner = Application.Current?.MainWindow,
            };

            if (dialog.ShowDialog() != true)
                return;

            if (dialog.ResetRequested)
                Reset(ctx, timeline, settings);
            else
                Apply(ctx, timeline, settings);
        }

        public static void Reset(IToolBoxContext ctx)
        {
            if (ctx.Timeline is { } timeline && GetSettings(ctx) is { } settings)
                Reset(ctx, timeline, settings);
        }

        private static void Apply(IToolBoxContext ctx, Timeline timeline, LayerEditorSettings settings)
        {
            if (!settings.ChangeEnabled && !settings.ChangeLabel && !settings.ChangeColor && !settings.ChangeVolume)
            {
                Notify(Texts.LayerEditorNothingToDo);
                return;
            }

            var targets = GetTargetLayers(timeline, settings);
            if (targets.Count == 0)
            {
                Notify(Texts.LayerEditorNoTarget);
                return;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var layer = targets[i];

                if (settings.ChangeEnabled)
                    timeline.LayerSettings.IsVisibles[layer] = settings.IsLayerEnabled;

                if (settings.ChangeLabel)
                    timeline.LayerSettings.Labels[layer] = BuildLabel(settings.LabelTemplate, layer, i + 1)!;

                if (settings.ChangeColor)
                    timeline.LayerSettings.Colors[layer] = settings.LayerColor;

                if (settings.ChangeVolume)
                    timeline.LayerSettings.Volumes[layer] = settings.LayerVolume;
            }

            ctx.UndoRedoManager?.Record();
            Notify(string.Format(Texts.LayerEditorApplied, targets.Count));
        }

        private static void Reset(IToolBoxContext ctx, Timeline timeline, LayerEditorSettings settings)
        {
            var targets = GetTargetLayers(timeline, settings);
            if (targets.Count == 0)
            {
                Notify(Texts.LayerEditorNoTarget);
                return;
            }

            var answer = MessageBox.Show(
                Texts.LayerEditorResetConfirm,
                Texts.LayerEditorDisplayName,
                MessageBoxButton.OKCancel);

            if (answer != MessageBoxResult.OK)
                return;

            foreach (var layer in targets)
            {
                var blank = new LayerSetting(layer);
                timeline.LayerSettings.IsVisibles[layer] = !blank.IsHidden;
                timeline.LayerSettings.Labels[layer] = blank.Label;
                timeline.LayerSettings.Colors[layer] = blank.Color;
                timeline.LayerSettings.Volumes[layer] = blank.Volume;
            }

            ctx.UndoRedoManager?.Record();
            Notify(string.Format(Texts.LayerEditorApplied, targets.Count));
        }

        private static IReadOnlyList<int> GetTargetLayers(Timeline timeline, LayerEditorSettings settings)
        {
            IEnumerable<int> layers = settings.Scope == LayerTargetScope.Range
                ? Enumerable.Range(
                    Math.Min(settings.FromLayer, settings.ToLayer),
                    Math.Abs(settings.ToLayer - settings.FromLayer) + 1)
                : TimelineHelper.GetSelectedLayers(timeline);

            return [.. layers.Where(layer => layer >= 0).Distinct().Order()];
        }

        private static string? BuildLabel(string template, int layer, int index)
        {
            if (string.IsNullOrEmpty(template))
                return null;

            try
            {
                return string.Format(template, layer, index);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        private static void Notify(string message) =>
            MessageBox.Show(message, Texts.LayerEditorDisplayName, MessageBoxButton.OK);

        private static LayerEditorSettings? GetSettings(IToolBoxContext ctx) => ctx.FindItemSettings<LayerEditorSettings>();
    }
}
