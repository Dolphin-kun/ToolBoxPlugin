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
        string IconPath
    );

    public static class ShortcutDefinitions
    {
        private static ShortcutDefinition Create(CommandType cmd, string icon, string? customLabel = null)
        {
            var field = typeof(CommandType).GetField(cmd.ToString());
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            string name = displayAttr?.Name ?? cmd.ToString();
            string displayName = Texts.GetString(name);
            return new ShortcutDefinition(displayName, cmd, customLabel ?? displayName, icon);
        }

        public static readonly IReadOnlyList<ShortcutDefinition> Allowed =
        [
            Create(CommandType.AddVoiceItem, YukkuriMovieMaker.Resources.Icons.IconKeys.MessageTextOutline),
            Create(CommandType.AddTextItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Text),
            Create(CommandType.AddVideoItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Video),
            Create(CommandType.AddAudioItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Music),
            Create(CommandType.AddImageItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Image),
            Create(CommandType.AddShapeItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Shape),
            Create(CommandType.AddTachieItem, YukkuriMovieMaker.Resources.Icons.IconKeys.Account),
            Create(CommandType.AddFaceItem, YukkuriMovieMaker.Resources.Icons.IconKeys.EmoticonOutline),
            Create(CommandType.Delete, YukkuriMovieMaker.Resources.Icons.IconKeys.Delete),
            Create(CommandType.AddTimelineSpace, YukkuriMovieMaker.Resources.Icons.IconKeys.ArrowExpandHorizontal),
            Create(CommandType.SelectAll, YukkuriMovieMaker.Resources.Icons.IconKeys.SelectAll),
            Create(CommandType.Undo, YukkuriMovieMaker.Resources.Icons.IconKeys.Undo),
            Create(CommandType.Redo, YukkuriMovieMaker.Resources.Icons.IconKeys.Redo),
            Create(CommandType.Cut, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentCut),
            Create(CommandType.Copy, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentCopy),
            Create(CommandType.Paste, YukkuriMovieMaker.Resources.Icons.IconKeys.ContentPaste),
        ];
    }
}
