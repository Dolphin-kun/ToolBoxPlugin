using ToolBox.Items.Recording.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Items.Recording.ViewModels
{
    public class RecordingViewModel : Bindable, IDisposable
    {
        private readonly RecordingSettings settings;
        private bool isRecording;
        private readonly DisposeCollector disposer = new();

        public RecordingSettings Settings => settings;

        public bool IsRecording
        {
            get => isRecording;
            private set => Set(ref isRecording, value);
        }

        public RecordingViewModel() : this(new RecordingSettings())
        {
        }

        public RecordingViewModel(RecordingSettings settings)
        {
            this.settings = settings;
            isRecording = RecordingCommand.IsRecording;
            RecordingCommand.IsRecordingChanged += OnIsRecordingChanged;
            disposer.CollectAction(this, () => RecordingCommand.IsRecordingChanged -= OnIsRecordingChanged);
        }

        private void OnIsRecordingChanged(object? sender, EventArgs e)
        {
            IsRecording = RecordingCommand.IsRecording;
        }

        public void Dispose()
        {
            disposer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
