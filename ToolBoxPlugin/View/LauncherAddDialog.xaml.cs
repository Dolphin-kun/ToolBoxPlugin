using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ToolBox.Commons;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.View
{
    public partial class LauncherAddDialog : Window
    {
        private const CompareOptions MatchOptions =
            CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

        private readonly IReadOnlyList<LauncherCandidate> candidates;
        private readonly ICollectionView view;

        public IReadOnlyList<LauncherNode> SelectedNodes { get; private set; } = [];

        public LauncherAddDialog(IReadOnlyList<LauncherCandidate> candidates)
        {
            InitializeComponent();
            WindowTheme.Attach(this);

            this.candidates = candidates;

            view = CollectionViewSource.GetDefaultView(candidates);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LauncherCandidate.GroupName)));
            view.Filter = Matches;
            CandidateList.ItemsSource = view;

            Loaded += (_, _) => SearchBox.Focus();
        }

        private bool Matches(object value)
        {
            if (value is not LauncherCandidate candidate)
                return false;

            var terms = SearchBox.Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var compare = CultureInfo.CurrentCulture.CompareInfo;

            return terms.All(term =>
                compare.IndexOf(candidate.DisplayName, term, MatchOptions) >= 0
                || compare.IndexOf(candidate.GroupName, term, MatchOptions) >= 0);
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => view.Refresh();

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            SelectedNodes = [.. candidates.Where(candidate => candidate.IsChecked).Select(candidate => candidate.CreateNode())];
            DialogResult = true;
        }
    }
}
