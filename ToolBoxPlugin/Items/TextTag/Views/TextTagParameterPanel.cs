using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ToolBox.Items.TextTag.Tags;
using YukkuriMovieMaker.Controls;

namespace ToolBox.Items.TextTag.Views
{
    public sealed class TextTagParameterPanel : StackPanel
    {
        private const double ControlWidth = 170;

        private readonly TextTagParameters parameters;
        private readonly Action changed;

        public TextTagParameterPanel(TextTagDefinition definition, TextTagParameters parameters, Action changed)
        {
            this.parameters = parameters;
            this.changed = changed;

            DataContext = parameters;

            foreach (var field in definition.Fields)
                Build(definition, field);

            parameters.PropertyChanged += (_, _) => changed();
        }

        private void Build(TextTagDefinition definition, TextTagField field)
        {
            switch (field)
            {
                case TextTagField.Color:
                    AddRow(Texts.TextTagFieldColor, Picker(nameof(TextTagParameters.Color)));
                    break;

                case TextTagField.OptionalColor:
                    AddRow(Texts.TextTagFieldUseColor, Toggle(nameof(TextTagParameters.UseColor)));
                    AddRow(Texts.TextTagFieldColor, Picker(nameof(TextTagParameters.Color)));
                    break;

                case TextTagField.DecorationColor:
                    AddRow(Texts.TextTagFieldUseDecorationColor, Toggle(nameof(TextTagParameters.UseDecorationColor)));
                    AddRow(Texts.TextTagFieldDecorationColor, Picker(nameof(TextTagParameters.DecorationColor)));
                    break;

                case TextTagField.FontSize:
                    AddRow(Texts.TextTagFieldFontSize, Slider(nameof(TextTagParameters.FontSize), 1, 300, 60, "0"));
                    break;

                case TextTagField.FontFamily:
                    AddRow(Texts.TextTagFieldFontFamily, Font());
                    break;

                case TextTagField.OptionalFontFamily:
                    AddRow(Texts.TextTagFieldUseFontFamily, Toggle(nameof(TextTagParameters.UseFontFamily)));
                    AddRow(Texts.TextTagFieldFontFamily, Font());
                    break;

                case TextTagField.Style:
                    AddRow(Texts.TextTagFieldStyle, Styles(definition.Kind == TextTagKind.Ruby));
                    break;

                case TextTagField.DecorationType:
                    AddRow(Texts.TextTagFieldDecorationType, DecorationTypes());
                    break;

                case TextTagField.X:
                    AddRow(Texts.TextTagFieldX, Slider(nameof(TextTagParameters.X), -999, 999, 0, "0.##"));
                    break;

                case TextTagField.Y:
                    AddRow(Texts.TextTagFieldY, Slider(nameof(TextTagParameters.Y), -999, 999, 0, "0.##"));
                    break;

                case TextTagField.Relative:
                    AddRow(Texts.TextTagFieldRelative, Toggle(nameof(TextTagParameters.IsRelative)));
                    break;

                case TextTagField.Angle:
                    AddRow(Texts.TextTagFieldAngle, Slider(nameof(TextTagParameters.Angle), -360, 360, 30, "0.##"));
                    break;

                case TextTagField.Spacing:
                    AddRow(Texts.TextTagFieldSpacing, Slider(nameof(TextTagParameters.Spacing), -100, 200, 10, "0.##"));
                    break;

                case TextTagField.RubyText:
                    AddRow(Texts.TextTagFieldRubyText, Box(nameof(TextTagParameters.RubyText)));
                    break;

                case TextTagField.RubyScale:
                    AddRow(Texts.TextTagFieldRubyScale, Slider(nameof(TextTagParameters.RubyScale), 1, 400, 50, "0"));
                    break;

                case TextTagField.Speed:
                    AddRow(Texts.TextTagFieldSpeed, Slider(nameof(TextTagParameters.Speed), 0, 200, 20, "0.##"));
                    break;

                case TextTagField.Seconds:
                    AddRow(Texts.TextTagFieldSeconds, Slider(nameof(TextTagParameters.Seconds), 0, 60, 1, "0.##"));
                    break;

                case TextTagField.PerCharacter:
                    AddRow(Texts.TextTagFieldPerCharacter, Toggle(nameof(TextTagParameters.PerCharacter)));
                    break;
            }
        }

        private void AddRow(string label, FrameworkElement control)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(text, 0);

            control.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(control, 1);

            grid.Children.Add(text);
            grid.Children.Add(control);
            Children.Add(grid);
        }

        private FrameworkElement Picker(string path) => Bind(new ColorPicker
        {
            Width = ControlWidth,
            Height = 24,
            VerticalContentAlignment = VerticalAlignment.Center,
        }, ColorPicker.ValueProperty, path);

        private FrameworkElement Toggle(string path) =>
            Bind(new ToggleSlider(), ToggleSlider.ValueProperty, path);

        private FrameworkElement Box(string path) => Bind(new TextBox
        {
            Width = ControlWidth,
            Height = 24,
            VerticalContentAlignment = VerticalAlignment.Center,
        }, TextBox.TextProperty, path);

        private FrameworkElement Font() => Bind(new FontComboBox
        {
            Width = ControlWidth,
            Height = 24,
        }, FontComboBox.FontProperty, nameof(TextTagParameters.FontFamily));

        private FrameworkElement Slider(string path, double min, double max, double fallback, string format)
        {
            var slider = new TextBoxSlider
            {
                Width = ControlWidth,
                Height = 24,
                VerticalContentAlignment = VerticalAlignment.Center,
                Min = min,
                Max = max,
                DefaultMin = min,
                DefaultMax = max,
                DefaultValue = fallback,
                Delta = 1,
                StringFormat = format,
                IsUnitVisible = false,
            };

            return Bind(slider, TextBoxSlider.ValueProperty, path);
        }

        private FrameworkElement Styles(bool rubyOnly)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            panel.Children.Add(Check("B", nameof(TextTagParameters.Bold)));
            panel.Children.Add(Check("I", nameof(TextTagParameters.Italic)));

            if (!rubyOnly)
            {
                panel.Children.Add(Check("S", nameof(TextTagParameters.Strike)));
                panel.Children.Add(Check("U", nameof(TextTagParameters.Underline)));
            }

            return panel;
        }

        private CheckBox Check(string label, string path)
        {
            var check = new CheckBox
            {
                Content = label,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            check.SetBinding(ToggleButtonIsChecked, new Binding(path) { Source = parameters, Mode = BindingMode.TwoWay });
            return check;
        }

        private static readonly DependencyProperty ToggleButtonIsChecked =
            System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty;

        private FrameworkElement DecorationTypes()
        {
            var combo = new ComboBox
            {
                Width = ControlWidth,
                Height = 24,
                VerticalContentAlignment = VerticalAlignment.Center,
                SelectedValuePath = "Tag",
            };

            var labels = new[]
            {
                Texts.TextTagDecorationNone,
                Texts.TextTagDecoration0,
                Texts.TextTagDecoration1,
                Texts.TextTagDecoration2,
                Texts.TextTagDecoration3,
                Texts.TextTagDecoration4,
                Texts.TextTagDecoration5,
                Texts.TextTagDecoration6,
            };

            for (var i = 0; i < labels.Length; i++)
                combo.Items.Add(new ComboBoxItem { Content = labels[i], Tag = i - 1 });

            return Bind(combo, System.Windows.Controls.Primitives.Selector.SelectedValueProperty,
                nameof(TextTagParameters.DecorationType));
        }

        private T Bind<T>(T element, DependencyProperty property, string path) where T : FrameworkElement
        {
            element.SetBinding(property, new Binding(path)
            {
                Source = parameters,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

            return element;
        }
    }
}
