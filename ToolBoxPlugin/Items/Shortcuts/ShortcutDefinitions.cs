using System.Reflection;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Settings;
using YukkuriMovieMaker.Resources.Icons;
using YukkuriMovieMaker.Resources.Localization;

namespace ToolBox.Items.Shortcuts
{
    public record ShortcutDefinition(
        string DisplayName,
        CommandType CommandType,
        string DefaultLabel,
        string IconPath,
        string Description,
        string Group
    )
    {
        public string FullDisplayName => $"{Group} / {DisplayName}";
    }

    public static class ShortcutDefinitions
    {
        private static string CurrentGroup = string.Empty;

        private static ShortcutDefinition Create(CommandType cmd, string icon, string? description = null, string? customLabel = null)
        {
            var field = typeof(CommandType).GetField(cmd.ToString());
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            var name = displayAttr?.Name ?? cmd.ToString();
            var displayName = Texts.GetString(name);

            return new ShortcutDefinition(
                displayName,
                cmd,
                customLabel ?? displayName,
                icon,
                description ?? $"本体の「{displayName}」を実行します。",
                CurrentGroup);
        }

        public static readonly IReadOnlyList<ShortcutDefinition> Allowed = BuildAll();

        public static IReadOnlyList<string> Groups =>
            [.. Allowed.Select(d => d.Group).Distinct()];

        public static IReadOnlyList<ShortcutDefinition> InGroup(string group) =>
            [.. Allowed.Where(d => d.Group == group)];

        private static List<ShortcutDefinition> BuildAll()
        {
            var all = new List<ShortcutDefinition>();

            CurrentGroup = "編集";
            all.AddRange(
            [
                Create(CommandType.Undo, IconKeys.Undo),
                Create(CommandType.Redo, IconKeys.Redo),
                Create(CommandType.Cut, IconKeys.ContentCut),
                Create(CommandType.Copy, IconKeys.ContentCopy),
                Create(CommandType.Paste, IconKeys.ContentPaste),
                Create(CommandType.Clone, IconKeys.ContentDuplicate),
                Create(CommandType.Delete, IconKeys.Delete),
                Create(CommandType.DeleteAndMoveAll, IconKeys.DeleteSweep),
                Create(CommandType.DeleteItemSpace, IconKeys.ArrowCollapseHorizontal),
                Create(CommandType.DeleteAllLayerSpaces, IconKeys.ArrowCollapseHorizontal),
                Create(CommandType.AddTimelineSpace, IconKeys.ArrowExpandHorizontal),
            ]);

            CurrentGroup = "アイテム";
            all.AddRange(
            [
                Create(CommandType.SplitCurrentPosition, IconKeys.PaperCutVertical),
                Create(CommandType.SplitCurrentPositionItems, IconKeys.PaperCutVertical),
                Create(CommandType.ResetItemLength, IconKeys.Restore),
                Create(CommandType.Group, IconKeys.Group),
                Create(CommandType.Ungroup, IconKeys.Ungroup),
                Create(CommandType.ToggleGroup, IconKeys.Group),
                Create(CommandType.ToggleVisibility, IconKeys.EyeOutline),
                Create(CommandType.ToggleLock, IconKeys.Lock),
                Create(CommandType.ToggleWaveform, IconKeys.Waveform),
                Create(CommandType.ToggleClipping, IconKeys.Crop),
                Create(CommandType.SeparateVoiceTextToTextItem, IconKeys.Text),
                Create(CommandType.CreateFaceItemFromVoiceItem, IconKeys.EmoticonOutline),
            ]);

            CurrentGroup = "整列・移動";
            all.AddRange(
            [
                Create(CommandType.AlignItemsLeft, IconKeys.AlignHorizontalLeft),
                Create(CommandType.AlignItemsRight, IconKeys.AlignHorizontalRight),
                Create(CommandType.AlignItemsCenter, IconKeys.AlignHorizontalCenter),
                Create(CommandType.AlignItemsHorizontal, IconKeys.FormatAlignMiddle),
                Create(CommandType.AlignItemsVertical, IconKeys.FormatAlignCenter),
                Create(CommandType.AlignPositionAndLengthToAboveItem, IconKeys.ArrowUp),
                Create(CommandType.AlignPositionAndLengthToBelowItem, IconKeys.ArrowDown),
                Create(CommandType.MoveSelectedItemsToLeftFrame, IconKeys.ChevronLeft),
                Create(CommandType.MoveSelectedItemsToRightFrame, IconKeys.ChevronRight),
                Create(CommandType.MoveSelectedItemsToUpLayer, IconKeys.ChevronUp),
                Create(CommandType.MoveSelectedItemsToDownLayer, IconKeys.ChevronDown),
                Create(CommandType.MoveSelectedItemsToPreviousSecond, IconKeys.ChevronDoubleLeft),
                Create(CommandType.MoveSelectedItemsToNextSecond, IconKeys.ChevronDoubleRight),
            ]);

            CurrentGroup = "選択";
            all.AddRange(
            [
                Create(CommandType.SelectAll, IconKeys.SelectAll),
                Create(CommandType.SelectRightItems, IconKeys.ArrowExpandRight),
                Create(CommandType.SelectLeftItems, IconKeys.ArrowExpandLeft),
                Create(CommandType.SelectUpItem, IconKeys.ArrowUp),
                Create(CommandType.SelectDownItem, IconKeys.ArrowDown),
                Create(CommandType.SelectLeftItem, IconKeys.ArrowLeft),
                Create(CommandType.SelectRightItem, IconKeys.ArrowRight),
                Create(CommandType.SelectCurrentPositionItems, IconKeys.SelectAll),
            ]);

            CurrentGroup = "アイテムの追加";
            all.AddRange(
            [
                Create(CommandType.AddVoiceItem, IconKeys.MessageTextOutline),
                Create(CommandType.AddTextItem, IconKeys.Text),
                Create(CommandType.AddVideoItem, IconKeys.Video),
                Create(CommandType.AddAudioItem, IconKeys.Music),
                Create(CommandType.AddImageItem, IconKeys.Image),
                Create(CommandType.AddShapeItem, IconKeys.ShapePlus),
                Create(CommandType.AddTachieItem, IconKeys.Account),
                Create(CommandType.AddFaceItem, IconKeys.EmoticonOutline),
                Create(CommandType.AddEffectItem, IconKeys.AutoFix),
                Create(CommandType.AddTransitionItem, IconKeys.GradientHorizontal),
                Create(CommandType.AddGroupItem, IconKeys.SelectGroup),
                Create(CommandType.AddFrameBufferItem, IconKeys.ImageMultiple),
                Create(CommandType.AddSceneItem, IconKeys.MovieOpenOutline),
                Create(CommandType.AddFileItem, IconKeys.FilePlusOutline),
            ]);

            CurrentGroup = "中間点";
            all.AddRange(
            [
                Create(CommandType.AddKeyFrameAtCurrentFrame, IconKeys.PlusThick),
                Create(CommandType.RemoveKeyFrameAtCurrentFrame, IconKeys.Minus),
                Create(CommandType.DistributeKeyFramesEvenly, IconKeys.FormatAlignJustify),
                Create(CommandType.SplitAtAllKeyFrames, IconKeys.ContentCut),
                Create(CommandType.RemoveAllKeyFrames, IconKeys.Delete),
            ]);

            CurrentGroup = "再生";
            all.AddRange(
            [
                Create(CommandType.PlayPause, IconKeys.Play),
                Create(CommandType.Stop, IconKeys.Stop),
                Create(CommandType.SeekToStart, IconKeys.SkipPrevious),
                Create(CommandType.SeekToEnd, IconKeys.SkipNext),
                Create(CommandType.SeekToPreviousFrame, IconKeys.StepBackward),
                Create(CommandType.SeekToNextFrame, IconKeys.StepForward),
                Create(CommandType.SeekToPrevEditPoint, IconKeys.StepBackward2),
                Create(CommandType.SeekToNextEditPoint, IconKeys.StepForward2),
                Create(CommandType.ToggleRepeat, IconKeys.Repeat),
                Create(CommandType.PlaybackRateDown, IconKeys.Rewind),
                Create(CommandType.PlaybackRateUp, IconKeys.FastForward),
                Create(CommandType.PlaybackRateReset, IconKeys.Restore),
            ]);

            CurrentGroup = "表示";
            all.AddRange(
            [
                Create(CommandType.ZoomInTimeline, IconKeys.MagnifyPlusOutline),
                Create(CommandType.ZoomOutTimeline, IconKeys.MagnifyMinusOutline),
                Create(CommandType.ZoomInPreview, IconKeys.MagnifyPlusOutline),
                Create(CommandType.ZoomOutPreview, IconKeys.MagnifyMinusOutline),
                Create(CommandType.FitPreviewZoom, IconKeys.FitToScreen),
                Create(CommandType.SizeUpLayer, IconKeys.ArrowExpandVertical),
                Create(CommandType.SizeDownLayer, IconKeys.ArrowCollapseVertical),
                Create(CommandType.ToggleVerticalLine, IconKeys.Grid),
                Create(CommandType.SwitchControlPointVisibility, IconKeys.VectorPolyline),
            ]);

            CurrentGroup = "レイヤー";
            all.AddRange(
            [
                Create(CommandType.AddLayer, IconKeys.PlusThick),
                Create(CommandType.DeleteLayer, IconKeys.Delete),
                Create(CommandType.MoveUpLayer, IconKeys.ArrowUp),
                Create(CommandType.MoveDownLayer, IconKeys.ArrowDown),
                Create(CommandType.ToggleLayerVisibility, IconKeys.EyeOutline),
                Create(CommandType.DisableOtherLayers, IconKeys.EyeOffOutline),
                Create(CommandType.EnableAllLayers, IconKeys.EyeOutline),
            ]);

            CurrentGroup = "キャラクター・シーン";
            all.AddRange(
            [
                Create(CommandType.ChangePreviousCharacter, IconKeys.AccountArrowLeft),
                Create(CommandType.ChangeNextCharacter, IconKeys.AccountArrowRight),
                Create(CommandType.CreateNewScene, IconKeys.MovieOpenPlusOutline),
                Create(CommandType.SelectPreviousScene, IconKeys.ChevronLeft),
                Create(CommandType.SelectNextScene, IconKeys.ChevronRight),
            ]);

            CurrentGroup = "プロジェクト";
            all.AddRange(
            [
                Create(CommandType.SaveProject, IconKeys.ContentSave),
                Create(CommandType.SaveProjectAs, IconKeys.ContentSaveEdit),
                Create(CommandType.OpenProject, IconKeys.FolderOpen),
                Create(CommandType.OutputVideo, IconKeys.Movie),
                Create(CommandType.OutputExo, IconKeys.Export),
                Create(CommandType.SaveThumbnail, IconKeys.Camera),
                Create(CommandType.SaveThumbnailToClipboard, IconKeys.CameraOutline),
            ]);

            CurrentGroup = "ウィンドウ・フォルダ";
            all.AddRange(
            [
                Create(CommandType.OpenSettingWindow, IconKeys.Cog),
                Create(CommandType.OpenScenarioWindow, IconKeys.CommentTextMultipleOutline),
                Create(CommandType.OpenDictionaryWindow, IconKeys.BookOpenPageVariant),
                Create(CommandType.OpenResourceWindow, IconKeys.ListBoxOutline),
                Create(CommandType.OpenSearchWindow, IconKeys.Magnify),
                Create(CommandType.OpenReplaceWindow, IconKeys.FindReplace),
                Create(CommandType.OpenVideoInfoEditorWindow, IconKeys.CogOutline),
                Create(CommandType.OpenCharacterEditorWindow, IconKeys.AccountEdit),
                Create(CommandType.OpenTemplateEditorWindow, IconKeys.FileEdit),
                Create(CommandType.OpenUserDirectory, IconKeys.FolderOpen),
                Create(CommandType.OpenPluginDirectory, IconKeys.FolderOpen),
                Create(CommandType.OpenTemporaryDirectory, IconKeys.FolderOpen),
            ]);

            return all;
        }
    }
}
