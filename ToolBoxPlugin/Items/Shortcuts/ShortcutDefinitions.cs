using System.Windows;
using Microsoft.Win32;
using ToolBox.Commons;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.Resources.Icons;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.Shortcuts
{
    public record ShortcutDefinition(
        string DisplayName,
        CommandType CommandType,
        string DefaultLabel,
        string IconPath,
        string Description,
        string Group,
        Func<IToolBoxContext?, bool> CanExecute,
        Action<IToolBoxContext?> Execute
    )
    {
        public string FullDisplayName => $"{Group} / {DisplayName}";
    }

    public static class ShortcutDefinitions
    {
        private static string CurrentGroup = string.Empty;

        private static ShortcutDefinition Create(
            ICommandId id,
            string icon,
            string description,
            Func<IToolBoxContext?, bool> canExecute,
            Action<IToolBoxContext?> execute)
        {
            var displayName = CommandRegistry.GetDisplayName(id.Key);

            return new ShortcutDefinition(
                displayName,
                Enum.Parse<CommandType>(id.Key),
                displayName,
                icon,
                description,
                CurrentGroup,
                canExecute,
                execute);
        }

        private static ShortcutDefinition Create(ICommandId<NoParameter> id, string icon) =>
            Create(id, icon, DescriptionOf(Texts.ShortcutDefaultDescription, id),
                _ => YmmCommand.CanExecute(id),
                _ => YmmCommand.Execute(id));

        private static ShortcutDefinition CreateForLayer(ICommandId<int> id, string icon) =>
            Create(id, icon, DescriptionOf(Texts.ShortcutSelectedLayerDescription, id),
                ctx => SelectedLayer(ctx) is { } layer && YmmCommand.CanExecute(id, layer),
                ctx =>
                {
                    if (SelectedLayer(ctx) is { } layer)
                        YmmCommand.Execute(id, layer);
                });

        private static ShortcutDefinition CreateForLayer(ICommandId<Point> id, string icon) =>
            Create(id, icon, DescriptionOf(Texts.ShortcutSelectedLayerDescription, id),
                ctx => SelectedLayer(ctx) is { } layer && YmmCommand.CanExecute(id, LayerPoint(layer)),
                ctx =>
                {
                    if (SelectedLayer(ctx) is { } layer)
                        YmmCommand.Execute(id, LayerPoint(layer));
                });

        private static ShortcutDefinition CreateForItems(ICommandId<IItem> id, string icon) =>
            Create(id, icon, DescriptionOf(Texts.ShortcutSelectedItemsDescription, id),
                ctx => ctx?.SelectedItems?.Any(item => YmmCommand.CanExecute(id, item)) == true,
                ctx =>
                {
                    foreach (var item in ctx?.SelectedItems?.OrderBy(item => item.Frame).ToList() ?? [])
                        YmmCommand.Execute(id, item);
                });

        private static ShortcutDefinition CreateForFiles(ICommandId<IEnumerable<string>> id, string icon) =>
            Create(id, icon, DescriptionOf(Texts.ShortcutSelectFilesDescription, id),
                _ => YmmCommand.CanExecute(id, []),
                _ =>
                {
                    var dialog = new OpenFileDialog { Multiselect = true };
                    if (dialog.ShowDialog(Application.Current?.MainWindow) == true)
                        YmmCommand.Execute<IEnumerable<string>>(id, dialog.FileNames);
                });

        private sealed record AppCommandId<TParameter>(string Key) : ICommandId<TParameter>;

        private static AppCommandId<TParameter> AppCommand<TParameter>(string key) => new(key);

        private static string DescriptionOf(string format, ICommandId id) =>
            string.Format(format, CommandRegistry.GetDisplayName(id.Key));

        private static int? SelectedLayer(IToolBoxContext? ctx) =>
            ctx?.SelectedItems is { Count: > 0 } items ? items.Min(item => item.Layer) : null;

        private static Point LayerPoint(int layer) =>
            new(0, (layer + 0.5) * YMMSettings.Default.LayerHeight);

        public static readonly IReadOnlyList<ShortcutDefinition> Allowed = BuildAll();

        private static readonly Dictionary<CommandType, ShortcutDefinition> byCommandType =
            Allowed.DistinctBy(d => d.CommandType).ToDictionary(d => d.CommandType);

        public static ShortcutDefinition? Find(CommandType commandType) =>
            byCommandType.GetValueOrDefault(commandType);

        public static IReadOnlyList<string> Groups =>
            [.. Allowed.Select(d => d.Group).Distinct()];

        public static IReadOnlyList<ShortcutDefinition> InGroup(string group) =>
            [.. Allowed.Where(d => d.Group == group)];

        private static List<ShortcutDefinition> BuildAll()
        {
            var all = new List<ShortcutDefinition>();

            CurrentGroup = Texts.ShortcutGroupEdit;
            all.AddRange(
            [
                Create(Commands.Undo, IconKeys.Undo),
                Create(Commands.Redo, IconKeys.Redo),
                Create(Commands.Cut, IconKeys.ContentCut),
                Create(Commands.Copy, IconKeys.ContentCopy),
                Create(Commands.Paste, IconKeys.ContentPaste),
                Create(Commands.Clone, IconKeys.ContentDuplicate),
                Create(Commands.Delete, IconKeys.Delete),
                Create(Commands.DeleteAndMoveAll, IconKeys.DeleteSweep),
                Create(Commands.DeleteAndMoveLayer, IconKeys.DeleteSweepOutline),
                Create(Commands.CopyCurrentPositionItems, IconKeys.ContentCopy),
                Create(Commands.CutCurrentPositionItems, IconKeys.ContentCut),
                Create(Commands.DeleteCurrentPositionItems, IconKeys.DeleteOutline),
                CreateForItems(AppCommand<IItem>(nameof(CommandType.DeleteItemSpace)), IconKeys.ArrowCollapseHorizontal),
                CreateForLayer(Commands.DeleteAllLayerSpaces, IconKeys.ArrowCollapseHorizontal),
                Create(Commands.DeleteTimelineSpace, IconKeys.ArrowCollapseHorizontal),
                Create(Commands.DeleteAllTimelineSpaces, IconKeys.ArrowCollapseHorizontal),
                Create(Commands.DeleteIsolatedRanges, IconKeys.Eraser),
                Create(Commands.AddTimelineSpace, IconKeys.ArrowExpandHorizontal),
            ]);

            CurrentGroup = Texts.ShortcutGroupItem;
            all.AddRange(
            [
                Create(Commands.SplitCurrentPosition, IconKeys.PaperCutVertical),
                Create(Commands.SplitCurrentPositionItems, IconKeys.ChartGantt),
                Create(Commands.ChangeItemLength, IconKeys.ArrowLeftRight),
                Create(Commands.ResetItemLength, IconKeys.Restore),
                Create(Commands.Group, IconKeys.Group),
                Create(Commands.Ungroup, IconKeys.Ungroup),
                Create(Commands.ToggleGroup, IconKeys.Group),
                Create(Commands.ToggleVisibility, IconKeys.EyeOutline),
                Create(Commands.EnableVisibility, IconKeys.Eye),
                Create(Commands.DisableVisibility, IconKeys.EyeOff),
                Create(Commands.ToggleLock, IconKeys.Lock),
                Create(Commands.Lock, IconKeys.Lock),
                Create(Commands.Unlock, IconKeys.LockOpenOutline),
                Create(Commands.ToggleWaveform, IconKeys.Waveform),
                Create(Commands.EnableWaveform, IconKeys.Waveform),
                Create(Commands.DisableWaveform, IconKeys.WaveformOff),
                Create(Commands.ToggleClipping, IconKeys.VectorClipping),
                Create(Commands.EnableClipping, IconKeys.VectorClipping),
                Create(Commands.DisableClipping, IconKeys.VectorArrangeAbove),
                Create(Commands.SetToDefaultItem, IconKeys.Restore),
                Create(Commands.SetToCharacterVoice, IconKeys.AccountVoice),
                Create(Commands.SetToCharacterJimaku, IconKeys.Subtitles),
                Create(Commands.SetToCharacterFace, IconKeys.EmoticonOutline),
                Create(Commands.SeparateVoiceTextToTextItem, IconKeys.Text),
                Create(Commands.CreateFaceItemFromVoiceItem, IconKeys.EmoticonOutline),
                Create(Commands.MonotonizeSelectedAquesTalkVoice, IconKeys.Waveform),
            ]);

            CurrentGroup = Texts.ShortcutGroupArrange;
            all.AddRange(
            [
                Create(Commands.AlignItemsLeft, IconKeys.AlignHorizontalLeft),
                Create(Commands.AlignItemsRight, IconKeys.AlignHorizontalRight),
                Create(Commands.AlignItemsCenter, IconKeys.AlignHorizontalCenter),
                Create(Commands.AlignItemsHorizontal, IconKeys.FormatAlignMiddle),
                Create(Commands.AlignItemsVertical, IconKeys.FormatAlignCenter),
                Create(Commands.AlignPositionAndLengthToAboveItem, IconKeys.ArrowUp),
                Create(Commands.AlignPositionAndLengthToBelowItem, IconKeys.ArrowDown),
                Create(Commands.AlignHeadToAboveItem, IconKeys.FormatHorizontalAlignLeft),
                Create(Commands.AlignTailToAboveItem, IconKeys.FormatHorizontalAlignRight),
                Create(Commands.AlignHeadToBelowItem, IconKeys.FormatHorizontalAlignLeft),
                Create(Commands.AlignTailToBelowItem, IconKeys.FormatHorizontalAlignRight),
                Create(Commands.MoveSelectedItemsToLeftFrame, IconKeys.ChevronLeft),
                Create(Commands.MoveSelectedItemsToRightFrame, IconKeys.ChevronRight),
                Create(Commands.MoveSelectedItemsToUpLayer, IconKeys.ChevronUp),
                Create(Commands.MoveSelectedItemsToDownLayer, IconKeys.ChevronDown),
                Create(Commands.MoveSelectedItemsToPreviousSecond, IconKeys.ChevronDoubleLeft),
                Create(Commands.MoveSelectedItemsToNextSecond, IconKeys.ChevronDoubleRight),
                Create(Commands.MoveToHigherLayer, IconKeys.ArrowUpBold),
                Create(Commands.MoveToLowerLayer, IconKeys.ArrowDownBold),
            ]);

            CurrentGroup = Texts.ShortcutGroupSelection;
            all.AddRange(
            [
                Create(Commands.SelectAll, IconKeys.SelectAll),
                Create(Commands.SelectRightItems, IconKeys.ArrowExpandRight),
                Create(Commands.SelectLeftItems, IconKeys.ArrowExpandLeft),
                Create(Commands.SelectRightItemsOfSelectedItems, IconKeys.ArrowExpandRight),
                Create(Commands.SelectLeftItemsOfSelectedItems, IconKeys.ArrowExpandLeft),
                Create(Commands.SelectUpItem, IconKeys.ArrowUp),
                Create(Commands.SelectDownItem, IconKeys.ArrowDown),
                Create(Commands.SelectLeftItem, IconKeys.ArrowLeft),
                Create(Commands.SelectRightItem, IconKeys.ArrowRight),
                Create(Commands.SelectCurrentPositionItems, IconKeys.ChartGantt),
            ]);

            CurrentGroup = Texts.ShortcutGroupAddItem;
            all.AddRange(
            [
                Create(AppCommand<NoParameter>(nameof(CommandType.AddVoiceItem)), IconKeys.YMM4.VoiceItem),
                Create(Commands.AddTextItem, IconKeys.YMM4.TextItem),
                Create(Commands.AddVideoItem, IconKeys.YMM4.VideoItem),
                Create(Commands.AddAudioItem, IconKeys.YMM4.AudioItem),
                Create(Commands.AddImageItem, IconKeys.YMM4.ImageItem),
                Create(Commands.AddShapeItem, IconKeys.YMM4.ShapeItem),
                Create(AppCommand<NoParameter>(nameof(CommandType.AddTachieItem)), IconKeys.YMM4.TachieItem),
                Create(AppCommand<NoParameter>(nameof(CommandType.AddFaceItem)), IconKeys.YMM4.FaceItem),
                Create(Commands.AddEffectItem, IconKeys.YMM4.EffectItem),
                Create(Commands.AddTransitionItem, IconKeys.YMM4.TransitionItem),
                Create(Commands.AddGroupItem, IconKeys.YMM4.GroupItem),
                Create(Commands.AddFrameBufferItem, IconKeys.YMM4.FrameBufferItem),
                Create(Commands.AddSceneItem, IconKeys.YMM4.SceneItem),
                CreateForFiles(Commands.AddFileItem, IconKeys.FilePlusOutline),
            ]);

            CurrentGroup = Texts.ShortcutGroupMidpoint;
            all.AddRange(
            [
                Create(Commands.AddKeyFrameAtCurrentFrame, IconKeys.PlusThick),
                Create(Commands.RemoveKeyFrameAtCurrentFrame, IconKeys.Minus),
                Create(Commands.AddKeyFrameAtCursorPosition, IconKeys.Plus),
                Create(Commands.RemoveKeyFrameAtCursorPosition, IconKeys.Minus),
                Create(Commands.AddSymmetricKeyFrames, IconKeys.ReflectHorizontal),
                Create(Commands.DistributeKeyFramesEvenly, IconKeys.FormatAlignJustify),
                Create(Commands.SplitAtKeyFrame, IconKeys.ContentCut),
                Create(Commands.SplitAtAllKeyFrames, IconKeys.ContentCut),
                Create(Commands.RemoveAllKeyFrames, IconKeys.Delete),
            ]);

            CurrentGroup = Texts.ShortcutGroupPlayback;
            all.AddRange(
            [
                Create(Commands.PlayPause, IconKeys.Play),
                Create(Commands.Stop, IconKeys.Stop),
                Create(Commands.SeekToStart, IconKeys.SkipPrevious),
                Create(Commands.SeekToEnd, IconKeys.SkipNext),
                Create(Commands.SeekToPreviousFrame, IconKeys.StepBackward),
                Create(Commands.SeekToNextFrame, IconKeys.StepForward),
                Create(Commands.SeekToPreviousSecond, IconKeys.Rewind5),
                Create(Commands.SeekToNextSecond, IconKeys.FastForward5),
                Create(Commands.SeekToPrevEditPoint, IconKeys.StepBackward2),
                Create(Commands.SeekToNextEditPoint, IconKeys.StepForward2),
                Create(Commands.ToggleRepeat, IconKeys.RepeatCircleArrow),
                Create(Commands.PlaybackRateDown, IconKeys.Rewind),
                Create(Commands.PlaybackRateUp, IconKeys.FastForward),
                Create(Commands.PlaybackRateReset, IconKeys.Restore),
                Create(Commands.ReloadPlayer, IconKeys.Refresh),
            ]);

            CurrentGroup = Texts.ShortcutGroupView;
            all.AddRange(
            [
                Create(Commands.ZoomInTimeline, IconKeys.MagnifyPlusOutline),
                Create(Commands.ZoomOutTimeline, IconKeys.MagnifyMinusOutline),
                Create(Commands.ZoomInPreview, IconKeys.MagnifyPlusOutline),
                Create(Commands.ZoomOutPreview, IconKeys.MagnifyMinusOutline),
                Create(Commands.FitPreviewZoom, IconKeys.FitToScreen),
                Create(Commands.SizeUpLayer, IconKeys.ArrowExpandVertical),
                Create(Commands.SizeDownLayer, IconKeys.ArrowCollapseVertical),
                Create(Commands.SizeUpToolBar, IconKeys.ArrowExpand),
                Create(Commands.SizeDownToolBar, IconKeys.ArrowCollapse),
                Create(Commands.ToggleVerticalLine, IconKeys.Grid),
                Create(Commands.EnableVerticalLine, IconKeys.Grid),
                Create(Commands.DisableVerticalLine, IconKeys.GridOff),
                Create(Commands.SwitchControlPointVisibility, IconKeys.VectorPolyline),
                Create(Commands.TogglePeakMeterVisibility, IconKeys.Poll),
                Create(Commands.ToggleLoudnessMeterVisibility, IconKeys.Gauge),
            ]);

            CurrentGroup = Texts.ShortcutGroupLayer;
            all.AddRange(
            [
                CreateForLayer(Commands.AddLayer, IconKeys.PlusThick),
                CreateForLayer(Commands.DeleteLayer, IconKeys.Delete),
                CreateForLayer(Commands.MoveUpLayer, IconKeys.ArrowUp),
                CreateForLayer(Commands.MoveDownLayer, IconKeys.ArrowDown),
                CreateForLayer(Commands.ToggleLayerVisibility, IconKeys.EyeOutline),
                CreateForLayer(Commands.DisableOtherLayers, IconKeys.EyeOffOutline),
                Create(Commands.EnableAllLayers, IconKeys.EyeOutline),
                Create(Commands.ResetAllLayerSettings, IconKeys.Restore),
            ]);

            CurrentGroup = Texts.ShortcutGroupCharacterScene;
            all.AddRange(
            [
                Create(Commands.ChangeCharacter, IconKeys.AccountSwitch),
                Create(Commands.ChangePreviousCharacter, IconKeys.AccountArrowLeft),
                Create(Commands.ChangeNextCharacter, IconKeys.AccountArrowRight),
                Create(Commands.ChangeVoiceMode, IconKeys.AccountVoice),
                Create(Commands.CreateNewScene, IconKeys.MovieOpenPlusOutline),
                Create(Commands.DeleteScene, IconKeys.MovieRemoveOutline),
                Create(Commands.RenameScene, IconKeys.RenameOutline),
                Create(Commands.SelectPreviousScene, IconKeys.ChevronLeft),
                Create(Commands.SelectNextScene, IconKeys.ChevronRight),
                Create(Commands.MoveSceneLeft, IconKeys.ArrowLeftBold),
                Create(Commands.MoveSceneRight, IconKeys.ArrowRightBold),
                Create(Commands.OpenScenePreviewWindow, IconKeys.MonitorEye),
                Create(Commands.SaveSceneAsTemplate, IconKeys.ContentSaveOutline),
            ]);

            CurrentGroup = Texts.ShortcutGroupProject;
            all.AddRange(
            [
                Create(Commands.CreateProject, IconKeys.FilePlus),
                Create(Commands.OpenProject, IconKeys.FolderOpen),
                Create(Commands.CloseProject, IconKeys.CloseBoxOutline),
                Create(Commands.SaveProject, IconKeys.ContentSave),
                Create(Commands.SaveProjectAs, IconKeys.ContentSaveEdit),
                Create(Commands.OpenScenarioFile, IconKeys.CommentTextMultipleOutline),
                Create(Commands.ImportSubtitleFile, IconKeys.Subtitles),
                Create(Commands.ImportSubtitleFileAsVoiceItem, IconKeys.SubtitlesOutline),
                Create(Commands.OutputVideo, IconKeys.FileVideoOutline2),
                Create(Commands.OutputExo, IconKeys.FileFilmstripOutline),
                Create(Commands.SaveThumbnail, IconKeys.Camera),
                Create(Commands.SaveThumbnailToClipboard, IconKeys.ClipboardCameraOutline),
            ]);

            CurrentGroup = Texts.ShortcutGroupTemplate;
            all.AddRange(
            [
                Create(AppCommand<NoParameter>(nameof(CommandType.SaveTemplate)), IconKeys.ContentSaveOutline),
                Create(Commands.ImportTemplate, IconKeys.Import),
                Create(AppCommand<NoParameter>(nameof(CommandType.ExportItemTemplate)), IconKeys.Export),
                Create(Commands.ExportVideoEffectTemplate, IconKeys.Export),
                Create(Commands.ExportAudioEffectTemplate, IconKeys.Export),
                Create(Commands.OpenTemplateMenu, IconKeys.FormatListBulleted),
                Create(Commands.OpenVoiceTemplateMenu, IconKeys.YMM4.VoiceItem),
                Create(Commands.OpenTextTemplateMenu, IconKeys.YMM4.TextItem),
                Create(Commands.OpenVideoTemplateMenu, IconKeys.YMM4.VideoItem),
                Create(Commands.OpenAudioTemplateMenu, IconKeys.YMM4.AudioItem),
                Create(Commands.OpenImageTemplateMenu, IconKeys.YMM4.ImageItem),
                Create(Commands.OpenShapeTemplateMenu, IconKeys.YMM4.ShapeItem),
                Create(Commands.OpenEffectTemplateMenu, IconKeys.YMM4.EffectItem),
                Create(Commands.OpenFrameBufferTemplateMenu, IconKeys.YMM4.FrameBufferItem),
                Create(Commands.OpenGroupTemplateMenu, IconKeys.YMM4.GroupItem),
                Create(Commands.OpenTachieTemplateMenu, IconKeys.YMM4.TachieItem),
                Create(Commands.OpenFaceTemplateMenu, IconKeys.YMM4.FaceItem),
            ]);

            CurrentGroup = Texts.ShortcutGroupWindowFolder;
            all.AddRange(
            [
                Create(Commands.OpenSettingWindow, IconKeys.Cog),
                Create(Commands.OpenScenarioWindow, IconKeys.CommentTextMultipleOutline),
                Create(Commands.OpenDictionaryWindow, IconKeys.BookOpenPageVariant),
                Create(Commands.OpenResourceWindow, IconKeys.ListBoxOutline),
                Create(Commands.OpenSearchWindow, IconKeys.Magnify),
                Create(Commands.OpenReplaceWindow, IconKeys.FindReplace),
                Create(Commands.OpenVideoInfoEditorWindow, IconKeys.Tune),
                Create(Commands.OpenCharacterEditorWindow, IconKeys.AccountEdit),
                Create(Commands.OpenTemplateEditorWindow, IconKeys.FileEdit),
                Create(Commands.OpenVerticalLineSettingsWindow, IconKeys.Grid),
                Create(Commands.CreateNewWindow, IconKeys.DockWindow),
                Create(Commands.OpenApplicationDirectory, IconKeys.FolderOpen),
                Create(Commands.OpenUserDirectory, IconKeys.FolderOpen),
                Create(Commands.OpenPluginDirectory, IconKeys.FolderOpen),
                Create(Commands.OpenTemporaryDirectory, IconKeys.FolderOpen),
            ]);

            CurrentGroup = Texts.ShortcutGroupOther;
            all.AddRange(
            [
                Create(AppCommand<NoParameter>(nameof(CommandType.GenerateScenarioLineByAI)), IconKeys.Creation),
                Create(Commands.FocusToDefaultPropertyEditor, IconKeys.FormTextbox),
                Create(Commands.SaveLayoutPreset, IconKeys.ViewDashboardOutline),
                Create(Commands.ResetLayout, IconKeys.ViewDashboardOutline),
                Create(Commands.ResetUIScale, IconKeys.FormatSize),
                Create(Commands.ExportLogFile, IconKeys.FileDocumentOutline),
                Create(Commands.OpenAboutWindow, IconKeys.InformationOutline),
                Create(Commands.OpenFeedbackWindow, IconKeys.MessageAlertOutline),
                Create(Commands.OpenUpdaterWindow, IconKeys.Refresh),
                Create(Commands.CloseWindow, IconKeys.WindowClose),
                Create(Commands.Exit, IconKeys.ExitToApp),
            ]);

            return all;
        }
    }
}
