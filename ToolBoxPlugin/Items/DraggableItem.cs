namespace ToolBox.Items
{
    public abstract class DraggableItem
    {
        /// <summary>設定の型</summary>
        public abstract Type SettingsType { get; }

        /// <summary>グリッド上の View の型</summary>
        public abstract Type ViewType { get; }

        /// <summary>グリッド上の ViewModel の型 (不要なら null)</summary>
        public abstract Type? ViewModelType { get; }

        /// <summary>設定パネルの View の型 (不要なら null)</summary>
        public abstract Type? SettingsViewType { get; }

        /// <summary>設定パネルの ViewModel の型 (不要なら null)</summary>
        public abstract Type? SettingsViewModelType { get; }



        /// <summary>
        /// コマンド定義を返す。
        /// コマンド名は View 側の ICommand DependencyProperty 名と一致させること。
        /// </summary>
        public abstract IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions();
    }
}
