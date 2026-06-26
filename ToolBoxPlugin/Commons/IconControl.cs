using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToolBox.Commons
{
    public class IconControl : ContentControl
    {
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register(nameof(IconData), typeof(string), typeof(IconControl),
                new PropertyMetadata(string.Empty, OnIconDataChanged));

        public string IconData
        {
            get => (string)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

        private static void OnIconDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconControl control)
            {
                control.UpdateIcon();
            }
        }

        private void UpdateIcon()
        {
            if (string.IsNullOrWhiteSpace(IconData))
            {
                Content = null;
                return;
            }

            var resource = Application.Current.TryFindResource(IconData);
            if (resource != null)
            {
                if (resource is UIElement element)
                {
                    Content = new Viewbox
                    {
                        Child = element,
                        Stretch = Stretch.Uniform
                    };
                }
                else if (resource is Geometry geom)
                {
                    var path = new Path
                    {
                        Data = geom,
                        Stretch = Stretch.Uniform
                    };
                    path.SetBinding(Shape.FillProperty, new Binding("Foreground") { Source = this });
                    Content = path;
                }
                else
                {
                    Content = resource;
                }
                return;
            }

            try
            {
                var geometry = Geometry.Parse(IconData);
                var path = new Path
                {
                    Data = geometry,
                    Stretch = Stretch.Uniform
                };
                
                path.SetBinding(Shape.FillProperty, new Binding("Foreground") { Source = this });
                
                Content = path;
            }
            catch
            {
                Content = null;
            }
        }
    }
}
