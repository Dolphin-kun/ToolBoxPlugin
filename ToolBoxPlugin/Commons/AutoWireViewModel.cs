using System.Windows;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Commons
{
    public static class AutoWireViewModel
    {
        public static readonly DependencyProperty ViewModelTypeProperty = DependencyProperty.RegisterAttached(
            "ViewModelType", typeof(Type), typeof(AutoWireViewModel), new PropertyMetadata(null, OnViewModelTypeChanged));

        public static void SetViewModelType(DependencyObject element, Type? value) => element.SetValue(ViewModelTypeProperty, value);
        public static Type? GetViewModelType(DependencyObject element) => (Type?)element.GetValue(ViewModelTypeProperty);

        private static void OnViewModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                void handler(object s, RoutedEventArgs ev)
                {
                    fe.Loaded -= handler;

                    try
                    {
                        var vmType = GetViewModelType(fe);
                        if (vmType == null) return;

                        object? vm = null;

                        if (fe.DataContext is ToolItemSettings settings)
                        {
                            var ctor = vmType.GetConstructors()
                                .OrderByDescending(c => c.GetParameters().Length)
                                .FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.IsAssignableFrom(settings.GetType()));

                            if (ctor != null)
                            {
                                vm = ctor.Invoke([settings]);
                            }
                        }

                        if (vm == null)
                        {
                            var pCtor = vmType.GetConstructor(Type.EmptyTypes);
                            if (pCtor != null)
                                vm = Activator.CreateInstance(vmType);
                        }

                        if (vm != null)
                        {
                            fe.DataContext = vm;

                            if (vm is IDisposable disposable)
                            {
                                ToolBoxViewModel.Instance?.Disposer.Collect(disposable);
                            }
                        }
                    }
                    catch { }
                }

                fe.Loaded += handler;
            }
        }
    }
}
