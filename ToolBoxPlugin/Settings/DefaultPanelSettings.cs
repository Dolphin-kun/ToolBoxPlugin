using System.Collections.ObjectModel;
using System.Windows;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    /// <summary>新しく開くパネルの初期状態。</summary>
    public class DefaultPanelSettings : Bindable
    {
        private ObservableCollection<ToolItemSettings> items = [];
        public ObservableCollection<ToolItemSettings> Items
        {
            get => items;
            set => Set(ref items, value ?? []);
        }

        private int gridRowCount = 6;
        public int GridRowCount
        {
            get => gridRowCount;
            set => Set(ref gridRowCount, value);
        }

        private int gridColumnCount = 6;
        public int GridColumnCount
        {
            get => gridColumnCount;
            set => Set(ref gridColumnCount, value);
        }

        public Dictionary<string, Point> HiddenItemPositions { get; set; } = [];
    }
}
