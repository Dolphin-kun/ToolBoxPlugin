using NAudio.Wave;
using System.IO;
using System.Windows.Threading;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.Recording
{
    public static class RecordingCommand
    {
        private static WaveInEvent? waveIn;
        private static WaveFileWriter? writer;
        private static string? filePath;
        private static Timeline? timeline;
        private static UndoRedoManager? undoRedoManager;
        private static Dispatcher? uiDispatcher;
        private static readonly float[] waveformSamples = new float[256];
        private static int waveformIndex = 0;

        public static event EventHandler? IsRecordingChanged;
        public static event EventHandler? WaveformUpdated;

        private static bool isRecording;
        public static bool IsRecording
        {
            get => isRecording;
            private set
            {
                if (isRecording != value)
                {
                    isRecording = value;
                    IsRecordingChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static float[] GetWaveformSamples()
        {
            lock (waveformSamples)
            {
                return (float[])waveformSamples.Clone();
            }
        }

        public static void SetUIDispatcher(Dispatcher dispatcher)
        {
            uiDispatcher = dispatcher;
        }

        public static bool CanExecute(Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            return timeline != null && undoRedoManager != null;
        }

        public static List<string> GetAvailableDevices()
        {
            var devices = new List<string>();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var caps = WaveInEvent.GetCapabilities(i);
                devices.Add($"{i}: {caps.ProductName}");
            }

            return devices;
        }

        public static void ToggleRecording(Timeline? timeline, UndoRedoManager? undoRedoManager, int deviceIndex = 0)
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording(timeline, undoRedoManager, deviceIndex);
            }
        }

        private static void StartRecording(Timeline? currentTimeline, UndoRedoManager? currentUndoRedoManager, int deviceIndex)
        {
            if (currentTimeline is null || currentUndoRedoManager is null) return;

            timeline = currentTimeline;
            undoRedoManager = currentUndoRedoManager;

            try
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(44100, 1)
                };

                IsRecording = true;

                var recordingDir = Path.Combine(AppDirectories.TemporaryDirectory, "ToolBoxPlugin");
                Directory.CreateDirectory(recordingDir);
                filePath = Path.Combine(recordingDir, $"Record_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                writer = new WaveFileWriter(filePath, waveIn.WaveFormat);

                waveIn.DataAvailable += (s, e) =>
                {
                    if (writer != null)
                    {
                        writer.Write(e.Buffer, 0, e.BytesRecorded);
                        writer.Flush();

                        for (int i = 0; i < e.BytesRecorded; i += 2)
                        {
                            short sample = BitConverter.ToInt16(e.Buffer, i);
                            float normalizedSample = sample / 32768f;

                            lock (waveformSamples)
                            {
                                waveformSamples[waveformIndex] = normalizedSample;
                                waveformIndex = (waveformIndex + 1) % waveformSamples.Length;
                            }
                        }

                        if (uiDispatcher != null)
                        {
                            if (uiDispatcher.CheckAccess())
                            {
                                WaveformUpdated?.Invoke(null, EventArgs.Empty);
                            }
                            else
                            {
                                uiDispatcher.BeginInvoke(() =>
                                {
                                    WaveformUpdated?.Invoke(null, EventArgs.Empty);
                                });
                            }
                        }
                    }
                };

                waveIn.RecordingStopped += (s, e) =>
                {
                    Action action = () =>
                    {
                        IsRecording = false;

                        writer?.Flush();
                        writer?.Dispose();
                        writer = null;
                        waveIn?.Dispose();
                        waveIn = null;

                        AddItemToTimeline();
                    };

                    if (uiDispatcher != null && !uiDispatcher.CheckAccess())
                    {
                        uiDispatcher.BeginInvoke(action);
                    }
                    else
                    {
                        action();
                    }
                };

                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                IsRecording = false;
                _ = ex;
            }
        }

        private static void StopRecording()
        {
            waveIn?.StopRecording();
        }

        private static void AddItemToTimeline()
        {
            if (timeline is null || undoRedoManager is null || filePath is null || !File.Exists(filePath)) return;

            try
            {
                using var reader = new AudioFileReader(filePath);
                var duration = reader.TotalTime;
                var audioItem = new AudioItem(filePath)
                {
                    Length = (int)(duration.TotalSeconds * 30)
                };

                timeline.TryAddItems([audioItem], audioItem.Frame, audioItem.Layer);
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