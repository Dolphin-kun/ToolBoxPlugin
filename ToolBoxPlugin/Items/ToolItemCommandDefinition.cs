namespace ToolBox.Items
{
    public class ToolItemCommandDefinition(string name, Func<IToolBoxContext, object?, bool> canExecute, Action<IToolBoxContext, object?> execute)
    {
        public string Name { get; } = name;
        public Func<IToolBoxContext, object?, bool> CanExecute { get; } = canExecute;
        public Action<IToolBoxContext, object?> Execute { get; } = execute;

        public ToolItemCommandDefinition(string name, Func<IToolBoxContext, bool> canExecute, Action<IToolBoxContext> execute)
            : this(name, (ctx, _) => canExecute(ctx), (ctx, _) => execute(ctx))
        {
        }
    }
}
