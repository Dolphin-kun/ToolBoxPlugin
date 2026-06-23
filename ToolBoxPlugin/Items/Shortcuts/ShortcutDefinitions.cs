using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.Shortcuts
{
    public record ShortcutDefinition(
        string DisplayName,
        CommandType CommandType,
        string DefaultLabel,
        string IconPath
    );

    public static class ShortcutDefinitions
    {
        public static readonly IReadOnlyList<ShortcutDefinition> Allowed =
        [
            new("アイテム削除",   CommandType.Delete,             "削除",      "M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"),
            new("空白追加",       CommandType.AddTimelineSpace,   "空白追加",  "M2 12H4V17H20V7H16V5H22V19H2V12M6 5V12H14V5H6Z"),
            new("すべて選択",     CommandType.SelectAll,          "全選択",    "M4,3H5V5H3V4A1,1 0 0,1 4,3M20,3A1,1 0 0,1 21,4V5H19V3H20M15,5V3H17V5H15M11,5V3H13V5H11M7,5V3H9V5H7M21,20A1,1 0 0,1 20,21H19V19H21V20M15,21V19H17V21H15M11,21V19H13V21H11M7,21V19H9V21H7M4,21A1,1 0 0,1 3,20V19H5V21H4M3,15H5V17H3V15M21,15V17H19V15H21M3,11H5V13H3V11M21,11V13H19V11H21M3,7H5V9H3V7M21,7V9H19V7H21Z"),
            new("元に戻す",       CommandType.Undo,               "元に戻す",  "M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z"),
            new("やり直す",       CommandType.Redo,               "やり直す",  "M18.4,10.6C16.55,9 14.15,8 11.5,8C6.85,8 2.92,11.03 1.54,15.22L3.9,16C4.95,12.81 7.95,10.5 11.5,10.5C13.45,10.5 15.23,11.22 16.62,12.38L13,16H22V7L18.4,10.6Z"),
            new("切り取り",       CommandType.Cut,                "切り取り",  "M19 3L13 9L15 11L22 4V3M12 12.5A1.5 1.5 0 0 1 10.5 14A1.5 1.5 0 0 1 9 12.5A1.5 1.5 0 0 1 10.5 11A1.5 1.5 0 0 1 12 12.5M6 20A2 2 0 0 0 8 18C8 16.89 7.1 16 6 16A2 2 0 0 0 4 18A2 2 0 0 0 6 20M6 8A2 2 0 0 0 8 6C8 4.89 7.1 4 6 4A2 2 0 0 0 4 6A2 2 0 0 0 6 8M9.6 8.5C9.8 8.8 10.1 9.2 10.5 9.5L15.9 4.1C15.6 3.8 15.3 3.5 14.9 3.2L9.6 8.5Z"),
            new("コピー",         CommandType.Copy,               "コピー",    "M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z"),
            new("貼り付け",       CommandType.Paste,              "貼り付け",  "M19,20H5V4H7V7H17V4H19M12,2A1,1 0 0,1 13,3A1,1 0 0,1 12,4A1,1 0 0,1 11,3A1,1 0 0,1 12,2M19,2H14.82C14.4,0.84 13.3,0 12,0C10.7,0 9.6,0.84 9.18,2H5A2,2 0 0,0 3,4V20A2,2 0 0,0 5,22H19A2,2 0 0,0 21,20V4A2,2 0 0,0 19,2Z"),
            new("グループ化",     CommandType.Group,              "グループ化",   "M4,4H10V10H4V4M20,4V10H14V4H20M14,15H17V13H19V15H22V17H19V19H17V17H14V15M10,14H4V20H10V14Z"),
            new("グループ解除",   CommandType.Ungroup,            "グループ解除", "M12 5.69L17 10.19V18H15V11.81L12 9.12L9 11.81V18H7V10.19L12 5.69M12 3L2 12H5V20H9V14H15V20H19V12H22L12 3Z"),
        ];
    }
}
