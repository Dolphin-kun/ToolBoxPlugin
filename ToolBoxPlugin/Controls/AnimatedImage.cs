using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ToolBox.Commons;

namespace ToolBox.Controls
{
    public sealed class AnimatedImage : Image
    {
        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register(nameof(SourcePath), typeof(string), typeof(AnimatedImage),
                new FrameworkPropertyMetadata(string.Empty, OnSourcePathChanged));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        public AnimatedImage()
        {
            IsVisibleChanged += (_, _) => Refresh();
            Loaded += (_, _) => Refresh();
            Unloaded += (_, _) => StopAnimation();
        }

        private static void OnSourcePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedImage image)
                image.Refresh();
        }

        private void Refresh()
        {
            StopAnimation();

            if (!IsVisible)
            {
                SetCurrentValue(SourceProperty, ImageLoader.LoadStatic(SourcePath));
                return;
            }

            if (ImageLoader.LoadAnimation(SourcePath) is { Count: > 1 } frames)
            {
                StartAnimation(frames);
                return;
            }

            SetCurrentValue(SourceProperty, ImageLoader.LoadStatic(SourcePath));
        }

        private void StartAnimation(IReadOnlyList<AnimationFrame> frames)
        {
            var animation = new ObjectAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };

            var elapsed = TimeSpan.Zero;
            foreach (var frame in frames)
            {
                animation.KeyFrames.Add(new DiscreteObjectKeyFrame(frame.Image, KeyTime.FromTimeSpan(elapsed)));
                elapsed += frame.Delay;
            }

            animation.Duration = elapsed;
            BeginAnimation(SourceProperty, animation);
        }

        private void StopAnimation() => BeginAnimation(SourceProperty, null);
    }
}
