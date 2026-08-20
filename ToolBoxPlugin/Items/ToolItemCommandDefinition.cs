using ToolBox.Commons;

namespace ToolBox.Items
{
    public class ToolItemCommandDefinition(
        string name,
        Func<IToolBoxContext, object?, bool> canExecute,
        Action<IToolBoxContext, object?> execute,
        string? displayName = null,
        bool requiresItemParameter = false)
    {
        public string Name { get; } = name;
        public Func<IToolBoxContext, object?, bool> CanExecute { get; } = canExecute;
        public Action<IToolBoxContext, object?> Execute { get; } = execute;
        public string DisplayName { get; } = displayName ?? name;
        public bool RequiresItemParameter { get; } = requiresItemParameter;

        public ToolItemCommandDefinition(
            string name,
            Func<IToolBoxContext, bool> canExecute,
            Action<IToolBoxContext> execute,
            string? displayName = null)
            : this(name, (ctx, _) => canExecute(ctx), (ctx, _) => execute(ctx), displayName)
        {
        }
    }
}
