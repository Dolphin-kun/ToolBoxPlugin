using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToolBox.Controls
{
    public class SettingsRow : Grid
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SettingsRow),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register(nameof(LabelWidth), typeof(GridLength), typeof(SettingsRow),
                new PropertyMetadata(GridLength.Auto));

        public static readonly DependencyProperty ContentMinWidthProperty =
            DependencyProperty.Register(nameof(ContentMinWidth), typeof(double), typeof(SettingsRow),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty LabelStyleProperty =
            DependencyProperty.Register(nameof(LabelStyle), typeof(Style), typeof(SettingsRow));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public GridLength LabelWidth
        {
            get => (GridLength)GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        public double ContentMinWidth
        {
            get => (double)GetValue(ContentMinWidthProperty);
            set => SetValue(ContentMinWidthProperty, value);
        }

        public Style? LabelStyle
        {
            get => (Style?)GetValue(LabelStyleProperty);
            set => SetValue(LabelStyleProperty, value);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            foreach (UIElement child in Children)
                SetColumn(child, 1);

            ColumnDefinitions.Clear();
            ColumnDefinitions.Add(new ColumnDefinition { Width = LabelWidth });
            ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
                MinWidth = ContentMinWidth,
            });

            var text = LabelStyle is { } style
                ? new TextBlock { Style = style }
                : new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0),
                };

            text.SetBinding(TextBlock.TextProperty, new Binding(nameof(Label)) { Source = this });
            SetColumn(text, 0);
            Children.Insert(0, text);
        }
    }
}
