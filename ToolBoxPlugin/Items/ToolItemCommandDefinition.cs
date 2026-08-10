using ToolBox.Commons;

namespace ToolBox.Items
{
    /// <summary>
    /// アイテムが提供する 1 つの操作。<see cref="Name"/> はビュー側の ICommand 依存関係プロパティ名と一致させること。
    /// </summary>
    /// <param name="displayName">マクロの手順一覧などに出す名前。省略すると <paramref name="name"/> がそのまま使われる。</param>
    /// <param name="requiresItemParameter">
    /// 実行に配置済みアイテムの設定インスタンスが要る操作かどうか。
    /// true の操作はパラメーターなしでは動かないため、マクロの手順一覧には出さない。
    /// </param>
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
