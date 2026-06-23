using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ToolBox.Items.Recording.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Items.Recording.Views
{
    public partial class RecordingItem : UserControl, IDisposable
    {
        public static readonly DependencyProperty ToggleRecordingCommandProperty =
            DependencyProperty.Register(nameof(ToggleRecordingCommand), typeof(ICommand), typeof(RecordingItem));

        public ICommand? ToggleRecordingCommand
        {
            get => (ICommand?)GetValue(ToggleRecordingCommandProperty);
            set => SetValue(ToggleRecordingCommandProperty, value);
        }

        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register(nameof(IsRecording), typeof(bool), typeof(RecordingItem),
                new PropertyMetadata(false));

        public bool IsRecording
        {
            get => (bool)GetValue(IsRecordingProperty);
            set => SetValue(IsRecordingProperty, value);
        }

        private readonly Commons.AdornerUtil adornerUtil = new();
        private readonly DisposeCollector disposer = new();

        public RecordingItem()
        {
            InitializeComponent();

            this.SetBinding(IsRecordingProperty, new Binding("IsRecording"));

            RecordingCommand.WaveformUpdated += OnWaveformUpdated;
            disposer.CollectAction(this, () => RecordingCommand.WaveformUpdated -= OnWaveformUpdated);

            Loaded += (s, e) =>
            {
                RecordingCommand.SetUIDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
                ToolBoxViewModel.Instance?.Disposer.Collect(this);
            };
        }

        private void OnWaveformUpdated(object? sender, EventArgs e)
        {
            DrawWaveform();
        }

        public void Dispose()
        {
            disposer.Dispose();
        }

        private void DrawWaveform()
        {
            var canvas = PeakMeterCanvas;
            if (canvas == null) return;

            if (!IsRecording)
            {
                canvas.Children.Clear();
                return;
            }

            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            if (width <= 0 || height <= 0) return;

            canvas.Children.Clear();

            var samples = RecordingCommand.GetWaveformSamples();
            if (samples.Length == 0) return;

            var points = new PointCollection();
            var centerY = height / 2;

            for (int i = 0; i < samples.Length; i++)
            {
                var x = (i / (float)samples.Length) * width;
                var y = centerY - (samples[i] * centerY * 0.9f);
                points.Add(new Point(x, y));
            }

            var polyline = new Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
                StrokeThickness = 2,
                IsHitTestVisible = false
            };

            canvas.Children.Add(polyline);

            var centerLine = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = width,
                Y2 = centerY,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            canvas.Children.Add(centerLine);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            e.Handled = true;

            adornerUtil.Toggle(this, () =>
            {
                var vm = DataContext as ViewModels.RecordingViewModel;
                var recordingSettings = vm?.Settings ?? (DataContext as RecordingSettings);
                return new RecordingSettingsPanel(recordingSettings);
            });
        }
    }
}
