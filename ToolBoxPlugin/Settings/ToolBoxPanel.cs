using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    /// <summary>
    /// ツールボックスパネル 1 枚分の状態。
    /// アイテムの配置とグリッド数はパネルごとに独立しており、
    /// ToolBoxViewModel の SaveState/LoadState を通じてインスタンス単位で永続化される。
    /// 色やセルサイズなどの見た目の設定は全パネル共通なので <see cref="ToolBoxSettings"/> 側に残る。
    /// </summary>
    public class ToolBoxPanel : Bindable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
        };

        /// <summary>このパネルに配置されているアイテム。</summary>
        public ObservableCollection<ToolItemSettings> Items { get; } = [];

        /// <summary>ユーザーが付けたパネル名。空ならタイトルにはプラグイン名だけを表示する。</summary>
        private string panelName = string.Empty;
        public string PanelName
        {
            get => panelName;
            set => Set(ref panelName, value);
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

        /// <summary>非表示にしたアイテムの元の位置。再表示したときに元の場所へ戻すために使う。</summary>
        public Dictionary<string, Point> HiddenItemPositions { get; } = [];

        /// <summary>
        /// 新規パネルの初期状態を作る。全体設定に保存されている内容を複製して使うため、
        /// 既存ユーザーがこれまで組んだ配置がそのまま最初のパネルへ引き継がれる。
        /// </summary>
        public static ToolBoxPanel CreateDefault()
        {
            var settings = ToolBoxSettings.Default;
            var panel = new ToolBoxPanel
            {
                GridRowCount = settings.GridRowCount,
                GridColumnCount = settings.GridColumnCount,
            };

            foreach (var item in CloneItems(settings.Items))
                panel.Items.Add(item);

            foreach (var (key, position) in settings.HiddenItemPositions)
                panel.HiddenItemPositions[key] = position;

            return panel;
        }

        public string ToJson() => JsonSerializer.Serialize(new PanelState
        {
            PanelName = PanelName,
            Items = [.. Items],
            GridRowCount = GridRowCount,
            GridColumnCount = GridColumnCount,
            HiddenItemPositions = new Dictionary<string, Point>(HiddenItemPositions),
        }, JsonOptions);

        /// <summary>
        /// 保存された JSON から状態を復元する。Items のインスタンス自体は差し替えず中身を入れ替えるため、
        /// CollectionChanged を購読している ViewModel やバインドが切れない。
        /// </summary>
        public void LoadJson(string json)
        {
            PanelState? state;
            try
            {
                state = JsonSerializer.Deserialize<PanelState>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load panel state: {ex}");
                return;
            }

            if (state == null)
                return;

            PanelName = state.PanelName;
            GridRowCount = state.GridRowCount;
            GridColumnCount = state.GridColumnCount;

            Items.Clear();
            // 型を解決できなかった要素 (アドオン削除など) は null で返るため取り除く
            foreach (var item in state.Items.OfType<ToolItemSettings>())
                Items.Add(item);

            HiddenItemPositions.Clear();
            foreach (var (key, position) in state.HiddenItemPositions)
                HiddenItemPositions[key] = position;
        }

        private static List<ToolItemSettings> CloneItems(IEnumerable<ToolItemSettings> source)
        {
            // ToolItemSettings は派生型を含むため、$type 判別子を扱う既存のコンバーター経由で複製する
            var json = JsonSerializer.Serialize(source.ToList(), JsonOptions);
            var clones = JsonSerializer.Deserialize<List<ToolItemSettings?>>(json, JsonOptions) ?? [];
            return [.. clones.OfType<ToolItemSettings>()];
        }

        /// <summary>永続化用のプレーンなデータ表現。</summary>
        private sealed class PanelState
        {
            public string PanelName { get; set; } = string.Empty;
            public List<ToolItemSettings?> Items { get; set; } = [];
            public int GridRowCount { get; set; } = 6;
            public int GridColumnCount { get; set; } = 6;
            public Dictionary<string, Point> HiddenItemPositions { get; set; } = [];
        }
    }
}
