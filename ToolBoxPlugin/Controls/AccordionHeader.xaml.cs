using System.Windows;
using System.Windows.Controls.Primitives;

namespace ToolBox.Controls
{
    public partial class AccordionHeader : ToggleButton
    {
        public static readonly DependencyProperty ShowArrowProperty =
            DependencyProperty.Register(nameof(ShowArrow), typeof(bool), typeof(AccordionHeader),
                new PropertyMetadata(true));

        public bool ShowArrow
        {
            get => (bool)GetValue(ShowArrowProperty);
            set => SetValue(ShowArrowProperty, value);
        }

        public AccordionHeader()
        {
            InitializeComponent();
        }
    }
}
