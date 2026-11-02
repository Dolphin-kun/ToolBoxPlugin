using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Controls;
using ToolBox.Items.ColorPalette.Settings;
using ToolBox.View;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Resources.Icons;
using YukkuriMovieMaker.Settings;
using YmmPalette = YukkuriMovieMaker.Settings.ColorPalette;
using YmmTexts = YukkuriMovieMaker.Resources.Localization.Texts;

namespace ToolBox.Items.ColorPalette.Views
{
    public sealed class ColorPaletteItem : ToolItemView
    {
        private const int MaxDensity = 4;
        private const int EmptySlotCount = 40;

        private static readonly string[] defaultPaletteFactories = ["GetBasicPallet", "GetMaterialPallet"];

        private YmmPalette? palette;
        private INotifyCollectionChanged? palettes;

        public ColorPaletteItem()
        {
            Loaded += (_, _) => Attach();
            Unloaded += (_, _) => Detach();
            DataContextChanged += (_, _) =>
            {
                if (IsLoaded)
                    Attach();
            };
        }

        private ColorPaletteSettings? Settings => DataContext as ColorPaletteSettings;

        private static ColorSettings ColorSettings => ColorSettings.Default;

        protected override void OnCellSpanChanged() => Build();

        private void Attach()
        {
            Detach();

            palettes = ColorSettings.Palettes;
            palettes.CollectionChanged += OnPalettesChanged;

            palette = ColorSettings.Palettes.FirstOrDefault(p => p.Name == Settings?.PaletteName)
                ?? ColorSettings.Palettes.ElementAtOrDefault(ColorSettings.SelectedPaletteIndex)
                ?? ColorSettings.Palettes.FirstOrDefault();

            if (palette != null)
                palette.Colors.CollectionChanged += OnColorsChanged;

            Build();
        }

        private void Detach()
        {
            if (palettes != null)
                palettes.CollectionChanged -= OnPalettesChanged;
            if (palette != null)
                palette.Colors.CollectionChanged -= OnColorsChanged;

            palettes = null;
            palette = null;
        }

        private void OnPalettesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Attach();

        private void OnColorsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Build();

        private void Build()
        {
            if (!IsLoaded)
                return;

            var colors = palette?.Colors.Where(color => !IsEmpty(color)).ToList() ?? [];
            var (columns, rows) = GetLayout(colors.Count + 1);

            var grid = new UniformGrid { Columns = columns, Rows = rows };
            foreach (var color in colors.Take(columns * rows - 1))
                grid.Children.Add(CreateSwatch(color));
            grid.Children.Add(CreateAddButton());

            Content = grid;
        }

        private (int Columns, int Rows) GetLayout(int count)
        {
            var width = Math.Max(1, (int)Math.Round(CellSpan.Width));
            var height = Math.Max(1, (int)Math.Round(CellSpan.Height));

            var density = 2;
            while (width * height * density * density < count && density < MaxDensity)
                density++;

            return (width * density, height * density);
        }

        private Button CreateSwatch(Color color)
        {
            var swatch = new Border
            {
                Background = new SolidColorBrush(color),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
            };
            swatch.SetResourceReference(Border.BorderBrushProperty, SystemColors.ActiveBorderBrushKey);

            var button = CreateCellButton(swatch, $"{Code(color)}\n{Texts.ColorPaletteSwatchHint}");
            button.Click += (_, e) =>
            {
                e.Handled = true;
                Apply(color, null);
            };
            button.MouseRightButtonDown += (_, e) => e.Handled = true;
            button.MouseRightButtonUp += (_, e) =>
            {
                e.Handled = true;
                ShowSwatchMenu(button, color);
            };
            return button;
        }

        private Button CreateAddButton()
        {
            var button = CreateCellButton(new Viewbox { Child = new IconControl { IconData = IconKeys.Plus } }, Texts.ColorPaletteAddHint);
            button.SetResourceReference(ForegroundProperty, YukkuriMovieMaker.Theme.YMM4Colors.IconBrushKey);
            button.Click += (_, e) =>
            {
                e.Handled = true;
                Dispatcher.BeginInvoke(() => ShowAddMenu(button), System.Windows.Threading.DispatcherPriority.Input);
            };
            return button;
        }

        private static Button CreateCellButton(object content, string toolTip) => new()
        {
            Content = content,
            ToolTip = toolTip,
            Padding = new Thickness(2),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
        };

        private void ShowSwatchMenu(FrameworkElement target, Color color)
        {
            var menu = new ContextMenu { PlacementTarget = target, Placement = PlacementMode.Bottom };
            menu.Items.Add(new MenuItem { Header = Code(color), IsEnabled = false });

            var labels = SelectedTargets().Select(t => t.Label).Distinct().ToList();
            if (labels.Count > 0)
            {
                menu.Items.Add(new Separator());
                foreach (var label in labels)
                    menu.Items.Add(ListPanel.MenuItem(string.Format(Texts.ColorPaletteApplyTo, label), () => Apply(color, label)));
            }

            menu.Items.Add(new Separator());
            menu.Items.Add(ListPanel.MenuItem(Texts.ColorPaletteCopyCode, () => Clipboard.SetText(Code(color))));
            menu.Items.Add(ListPanel.MenuItem(Texts.ColorPalettePickReplace, () =>
            {
                if (PickColor() is { } picked)
                    Edit(colors => Replace(colors, color, picked));
            }));
            menu.Items.Add(ListPanel.MenuItem(Texts.CommonDelete, () => Edit(colors => Replace(colors, color, default))));
            menu.IsOpen = true;
        }

        private void ShowAddMenu(FrameworkElement target)
        {
            var menu = new ContextMenu { PlacementTarget = target, Placement = PlacementMode.Bottom };

            menu.Items.Add(ListPanel.MenuItem(Texts.ColorPalettePickAdd, () =>
            {
                if (PickColor() is { } picked)
                    Edit(colors => Add(colors, picked));
            }));

            var selected = SelectedTargets().Select(t => t.Value).Where(color => !IsEmpty(color)).Distinct().ToList();
            menu.Items.Add(ListPanel.MenuItem(Texts.ColorPaletteAddFromItems, () => Edit(colors =>
            {
                foreach (var color in selected.Where(color => !colors.Contains(color)))
                    Add(colors, color);
            }), selected.Count > 0));

            menu.Items.Add(new Separator());
            menu.Items.Add(CreatePaletteMenu());
            menu.Items.Add(ListPanel.MenuItem(Texts.ColorPaletteReset, Reset, palette != null));

            menu.IsOpen = true;
        }

        private MenuItem CreatePaletteMenu()
        {
            var root = new MenuItem { Header = Texts.ColorPalettePaletteMenu };

            foreach (var candidate in ColorSettings.Palettes.ToList())
            {
                var item = new MenuItem { Header = candidate.Name, IsCheckable = true, IsChecked = candidate == palette };
                item.Click += (_, _) => Select(candidate.Name);
                root.Items.Add(item);
            }

            root.Items.Add(new Separator());
            root.Items.Add(ListPanel.MenuItem(Texts.ColorPaletteNew, AddPalette));
            root.Items.Add(ListPanel.MenuItem(Texts.CommonRename, RenamePalette, palette != null));
            root.Items.Add(ListPanel.MenuItem(Texts.CommonDelete, DeletePalette, palette != null && ColorSettings.Palettes.Count > 1));
            return root;
        }

        private void Select(string name)
        {
            if (Settings is { } settings)
                settings.PaletteName = name;
            Attach();
        }

        private void AddPalette()
        {
            var names = ColorSettings.Palettes.Select(p => p.Name).ToHashSet();
            var suggestion = Enumerable.Range(1, names.Count + 1)
                .Select(i => $"{YmmTexts.ColorSettingsCustomPalletName}{i}")
                .First(name => !names.Contains(name));

            if (Prompt(Texts.ColorPaletteNew, suggestion) is not { } name)
                return;

            var created = new YmmPalette { Name = name };
            for (var i = 0; i < EmptySlotCount; i++)
                created.Colors.Add(default);

            if (Settings is { } settings)
                settings.PaletteName = name;
            ColorSettings.Palettes.Add(created);
            ColorSettings.Save();
        }

        private void RenamePalette()
        {
            if (palette == null || Prompt(Texts.CommonRename, palette.Name) is not { } name)
                return;

            palette.Name = name;
            if (Settings is { } settings)
                settings.PaletteName = name;
            ColorSettings.Save();
        }

        private void DeletePalette()
        {
            if (palette is not { } current || !Confirm(string.Format(Texts.ColorPaletteDeleteConfirm, current.Name)))
                return;

            if (Settings is { } settings)
                settings.PaletteName = string.Empty;
            ColorSettings.Palettes.Remove(current);
            ColorSettings.Save();
        }

        private void Reset()
        {
            if (palette is not { } current || !Confirm(string.Format(Texts.ColorPaletteResetConfirm, current.Name)))
                return;

            var defaults = defaultPaletteFactories
                .Select(name => typeof(ColorSettings).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null) as YmmPalette)
                .FirstOrDefault(candidate => candidate?.Name == current.Name)?
                .Colors.ToList();

            Edit(colors =>
            {
                if (defaults == null)
                {
                    for (var i = 0; i < colors.Count; i++)
                        colors[i] = default;
                    return;
                }

                colors.Clear();
                foreach (var color in defaults)
                    colors.Add(color);
            });
        }

        private string? Prompt(string title, string current)
        {
            var dialog = new PanelNameDialog(current) { Owner = Window.GetWindow(this), Title = title };
            return dialog.ShowDialog() == true && dialog.PanelName.Length > 0 ? dialog.PanelName : null;
        }

        private static bool Confirm(string message) =>
            MessageBox.Show(message, Texts.ColorPaletteDisplayName, MessageBoxButton.OKCancel) == MessageBoxResult.OK;

        private void Edit(Action<IList<Color>> edit)
        {
            if (palette == null)
            {
                ColorSettings.Palettes.Add(new YmmPalette());
                if (palette == null)
                    return;
            }

            palette.Colors.CollectionChanged -= OnColorsChanged;
            try
            {
                edit(palette.Colors);
            }
            finally
            {
                palette.Colors.CollectionChanged += OnColorsChanged;
            }

            ColorSettings.Save();
            Build();
        }

        private static void Add(IList<Color> colors, Color color)
        {
            var empty = colors.IndexOf(default);
            if (empty >= 0)
                colors[empty] = color;
            else
                colors.Add(color);
        }

        private static void Replace(IList<Color> colors, Color from, Color to)
        {
            var index = colors.IndexOf(from);
            if (index >= 0)
                colors[index] = to;
        }

        private static bool IsEmpty(Color color) => color == default;

        private IReadOnlyList<ColorTarget> SelectedTargets() =>
            FindToolBox() is IToolBoxContext { SelectedItems: { Count: > 0 } items } ? ColorTargets.Find(items) : [];

        private void Apply(Color color, string? label)
        {
            if (FindToolBox() is not IToolBoxContext { SelectedItems: { Count: > 0 } items, UndoRedoManager: { } undoRedoManager })
                return;

            var changed = false;
            foreach (var item in items)
            {
                var targets = ColorTargets.Find(item);
                foreach (var target in label == null ? targets.Take(1) : targets.Where(t => t.Label == label))
                {
                    target.Value = color;
                    changed = true;
                }
            }

            if (changed)
                undoRedoManager.Record();
        }

        private static Color? PickColor()
        {
            var popup = new ColorPickerPopup();
            try
            {
                popup.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return IsEmpty(popup.Value) ? null : popup.Value;
        }

        private static string Code(Color color) => color.A == 255
            ? string.Create(CultureInfo.InvariantCulture, $"#{color.R:X2}{color.G:X2}{color.B:X2}")
            : string.Create(CultureInfo.InvariantCulture, $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
    }
}
