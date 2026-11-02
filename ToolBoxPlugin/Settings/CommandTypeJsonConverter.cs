using System.Text.Json;
using System.Text.Json.Serialization;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Settings
{
    public sealed class CommandTypeJsonConverter : JsonConverter<CommandType>
    {
        private static readonly Dictionary<int, string> LegacyNames = new()
        {
            [1] = nameof(CommandType.Cut),
            [2] = nameof(CommandType.Copy),
            [3] = nameof(CommandType.Paste),
            [4] = nameof(CommandType.Delete),
            [5] = nameof(CommandType.Clone),
            [6] = nameof(CommandType.DeleteAndMoveAll),
            [8] = nameof(CommandType.DeleteItemSpace),
            [10] = nameof(CommandType.DeleteAllLayerSpaces),
            [14] = nameof(CommandType.ResetItemLength),
            [15] = nameof(CommandType.AlignItemsLeft),
            [16] = nameof(CommandType.AlignItemsRight),
            [17] = nameof(CommandType.AlignItemsCenter),
            [18] = nameof(CommandType.AlignItemsHorizontal),
            [19] = nameof(CommandType.AlignItemsVertical),
            [20] = nameof(CommandType.AlignPositionAndLengthToAboveItem),
            [21] = nameof(CommandType.AlignPositionAndLengthToBelowItem),
            [26] = nameof(CommandType.MoveSelectedItemsToLeftFrame),
            [27] = nameof(CommandType.MoveSelectedItemsToRightFrame),
            [28] = nameof(CommandType.MoveSelectedItemsToUpLayer),
            [29] = nameof(CommandType.MoveSelectedItemsToDownLayer),
            [30] = nameof(CommandType.MoveSelectedItemsToPreviousSecond),
            [31] = nameof(CommandType.MoveSelectedItemsToNextSecond),
            [32] = nameof(CommandType.SelectAll),
            [33] = nameof(CommandType.SelectRightItems),
            [34] = nameof(CommandType.SelectLeftItems),
            [38] = nameof(CommandType.SelectUpItem),
            [39] = nameof(CommandType.SelectDownItem),
            [40] = nameof(CommandType.SelectLeftItem),
            [41] = nameof(CommandType.SelectRightItem),
            [42] = nameof(CommandType.Group),
            [43] = nameof(CommandType.Ungroup),
            [44] = nameof(CommandType.ToggleGroup),
            [47] = nameof(CommandType.ToggleLock),
            [50] = nameof(CommandType.ToggleWaveform),
            [51] = nameof(CommandType.ToggleClipping),
            [56] = nameof(CommandType.ToggleVisibility),
            [57] = nameof(CommandType.SplitCurrentPosition),
            [60] = nameof(CommandType.SplitCurrentPositionItems),
            [64] = nameof(CommandType.SelectCurrentPositionItems),
            [69] = nameof(CommandType.Undo),
            [70] = nameof(CommandType.Redo),
            [72] = nameof(CommandType.OpenProject),
            [74] = nameof(CommandType.SaveProject),
            [75] = nameof(CommandType.SaveProjectAs),
            [77] = nameof(CommandType.OutputExo),
            [78] = nameof(CommandType.OutputVideo),
            [80] = nameof(CommandType.PlayPause),
            [81] = nameof(CommandType.Stop),
            [84] = nameof(CommandType.SeekToNextFrame),
            [85] = nameof(CommandType.SeekToPreviousFrame),
            [86] = nameof(CommandType.SeekToStart),
            [87] = nameof(CommandType.SeekToEnd),
            [88] = nameof(CommandType.SeekToNextEditPoint),
            [89] = nameof(CommandType.SeekToPrevEditPoint),
            [92] = nameof(CommandType.SwitchControlPointVisibility),
            [93] = nameof(CommandType.ToggleRepeat),
            [94] = nameof(CommandType.PlaybackRateReset),
            [95] = nameof(CommandType.PlaybackRateDown),
            [96] = nameof(CommandType.PlaybackRateUp),
            [97] = nameof(CommandType.SaveThumbnail),
            [98] = nameof(CommandType.SaveThumbnailToClipboard),
            [103] = nameof(CommandType.ZoomInTimeline),
            [104] = nameof(CommandType.ZoomOutTimeline),
            [105] = nameof(CommandType.ZoomInPreview),
            [106] = nameof(CommandType.ZoomOutPreview),
            [107] = nameof(CommandType.FitPreviewZoom),
            [108] = nameof(CommandType.SizeUpLayer),
            [109] = nameof(CommandType.SizeDownLayer),
            [115] = nameof(CommandType.AddLayer),
            [116] = nameof(CommandType.DeleteLayer),
            [117] = nameof(CommandType.MoveUpLayer),
            [118] = nameof(CommandType.MoveDownLayer),
            [119] = nameof(CommandType.ToggleLayerVisibility),
            [123] = nameof(CommandType.DisableOtherLayers),
            [124] = nameof(CommandType.EnableAllLayers),
            [126] = nameof(CommandType.ChangePreviousCharacter),
            [127] = nameof(CommandType.ChangeNextCharacter),
            [138] = nameof(CommandType.OpenUserDirectory),
            [139] = nameof(CommandType.OpenPluginDirectory),
            [140] = nameof(CommandType.OpenTemporaryDirectory),
            [142] = nameof(CommandType.OpenCharacterEditorWindow),
            [143] = nameof(CommandType.OpenScenarioWindow),
            [144] = nameof(CommandType.OpenResourceWindow),
            [145] = nameof(CommandType.OpenDictionaryWindow),
            [146] = nameof(CommandType.OpenVideoInfoEditorWindow),
            [149] = nameof(CommandType.OpenTemplateEditorWindow),
            [151] = nameof(CommandType.OpenSettingWindow),
            [154] = nameof(CommandType.OpenSearchWindow),
            [155] = nameof(CommandType.OpenReplaceWindow),
            [158] = nameof(CommandType.ToggleVerticalLine),
            [159] = nameof(CommandType.AddTimelineSpace),
            [160] = nameof(CommandType.AddFileItem),
            [161] = nameof(CommandType.AddVoiceItem),
            [162] = nameof(CommandType.AddTextItem),
            [163] = nameof(CommandType.AddVideoItem),
            [164] = nameof(CommandType.AddAudioItem),
            [165] = nameof(CommandType.AddImageItem),
            [166] = nameof(CommandType.AddShapeItem),
            [167] = nameof(CommandType.AddEffectItem),
            [168] = nameof(CommandType.AddTransitionItem),
            [169] = nameof(CommandType.AddFrameBufferItem),
            [170] = nameof(CommandType.AddGroupItem),
            [171] = nameof(CommandType.AddTachieItem),
            [172] = nameof(CommandType.AddFaceItem),
            [173] = nameof(CommandType.AddSceneItem),
            [184] = nameof(CommandType.SeparateVoiceTextToTextItem),
            [185] = nameof(CommandType.CreateFaceItemFromVoiceItem),
            [191] = nameof(CommandType.AddKeyFrameAtCurrentFrame),
            [192] = nameof(CommandType.RemoveKeyFrameAtCurrentFrame),
            [195] = nameof(CommandType.DistributeKeyFramesEvenly),
            [197] = nameof(CommandType.SplitAtAllKeyFrames),
            [199] = nameof(CommandType.RemoveAllKeyFrames),
            [222] = nameof(CommandType.CreateNewScene),
            [227] = nameof(CommandType.SelectNextScene),
            [228] = nameof(CommandType.SelectPreviousScene),
        };

        public override CommandType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return Enum.TryParse<CommandType>(reader.GetString(), out var named) ? named : default;

            if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var number))
                return default;

            if (LegacyNames.TryGetValue(number, out var name) && Enum.TryParse<CommandType>(name, out var legacy))
                return legacy;

            return (CommandType)number;
        }

        public override void Write(Utf8JsonWriter writer, CommandType value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
