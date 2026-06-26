using System.Linq;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.SeparateAudio
{
    internal static class AudioCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any(item => item is VideoItem videoItem && !string.IsNullOrEmpty(videoItem.FilePath));
        }

        public static void DetachAudioFromVideoItem(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null || undoRedoManager is null || selectedItems.Count == 0) return;

            var videoItems = selectedItems
                .OfType<VideoItem>()
                .Where(item => !string.IsNullOrEmpty(item.FilePath))
                .ToList();

            if (videoItems.Count == 0) return;

            try
            {
                foreach (var videoItem in videoItems)
                {
                    if (videoItem.Volume?.Values != null)
                    {
                        foreach (var val in videoItem.Volume.Values)
                        {
                            val.Value = 0;
                        }
                    }

                    var audioItem = new AudioItem
                    {
                        Frame = videoItem.Frame,
                        Length = videoItem.Length,
                        Layer = videoItem.Layer + 1,
                        FilePath = videoItem.FilePath,
                    };

                    timeline.TryAddItems([audioItem], audioItem.Frame, audioItem.Layer);
                }
            }
            finally
            {
                timeline.ResolveAllItemsCollision();
                timeline.RefreshTimelineLengthAndMaxLayer();
                undoRedoManager.Record();
            }
        }
    }
}
