using System.Reflection;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Settings;
using System.Collections.Generic;
using YukkuriMovieMaker.Resources.Localization;

namespace ToolBox.Items.Shortcuts
{
    public record ShortcutDefinition(
        string DisplayName,
        CommandType CommandType,
        string DefaultLabel,
        string IconPath,
        string Description
    );

    public static class ShortcutDefinitions
    {
        private static ShortcutDefinition Create(CommandType cmd, string icon, string description, string? customLabel = null)
        {
            var field = typeof(CommandType).GetField(cmd.ToString());
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            string name = displayAttr?.Name ?? cmd.ToString();
            string displayName = Texts.GetString(name);
            return new ShortcutDefinition(displayName, cmd, customLabel ?? displayName, icon, description);
        }

        public static readonly IReadOnlyList<ShortcutDefinition> Allowed =
        [
            Create(CommandType.AddVoiceItem, YukkuriMovieMaker.Resources.Icons.IconKeys.MessageTextOutline,
                "音声アイテムをタイムラインに追加します。"),
            Create(CommandType.AddTextItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Text,
                "テキストアイテムをタイムラインに追加します。"),
            Create(CommandType.AddVideoItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Video,
                "動画アイテムをタイムラインに追加します。"),
            Create(CommandType.AddAudioItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Music,
                "音声ファイルのアイテムをタイムラインに追加します。"),
            Create(CommandType.AddImageItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Image,
                "画像アイテムをタイムラインに追加します。"),
            Create(CommandType.AddShapeItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Shape,
                "図形アイテムをタイムラインに追加します。"),
            Create(CommandType.AddTachieItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Account,
                "立ち絵アイテムをタイムラインに追加します。"),
            Create(CommandType.AddFaceItem, YukkuriMovieMaker.Resources.Icons.IconKeys.EmoticonOutline,
                "表情アイテムをタイムラインに追加します。"),
            Create(CommandType.Delete, YukkuriMovieMaker.Resources.Icons.IconKeys.Delete,
                "選択中のアイテムを削除します。"),
            Create(CommandType.AddTimelineSpace, YukkuriMovieMaker.Resources.Icons.IconKeys.ArrowExpandHorizontal,
                "タイムラインに空白を挿入して、以降のアイテムを後ろへずらします。"),
            Create(CommandType.SelectAll, YukkuriMovieMaker.Resources.Icons.IconKeys.SelectAll,
                "タイムライン上のアイテムをすべて選択します。"),
            Create(CommandType.Undo, YukkuriMovieMaker.Resources.Icons.IconKeys.Undo,
                "直前の操作を元に戻します。"),
            Create(CommandType.Redo, YukkuriMovieMaker.Resources.Icons.IconKeys.Redo,
                "元に戻した操作をやり直します。"),
            Create(CommandType.Cut, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentCut,
                "選択中のアイテムを切り取ります。"),
            Create(CommandType.Copy, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentCopy,
                "選択中のアイテムをコピーします。"),
            Create(CommandType.Paste, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentPaste,
                "コピーしたアイテムを貼り付けます。"),
        ];
    }
}
