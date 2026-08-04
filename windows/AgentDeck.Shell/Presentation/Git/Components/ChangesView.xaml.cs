using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Git.Utils;
using AgentDeck.Shell.Presentation.Git.ViewModels;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Git.Components;

public sealed partial class ChangesView : UserControl
{
    private const double MarkerWidth = 16;

    private readonly GitStatusViewModel _viewModel;

    public ChangesView(string path)
    {
        InitializeComponent();

        _viewModel = new GitStatusViewModel(AppServices.Daemon, path);
        TitleText.Text = AppServices.Strings.Get(StringKeys.GitChanges);
        RefreshButton.Content = TerminalGlyphs.Refresh;
        ToolTipService.SetToolTip(RefreshButton, AppServices.Strings.Get(StringKeys.GitRefresh));

        Loaded += async (_, _) => await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (!await _viewModel.RefreshAsync(CancellationToken.None) || _viewModel.Status is not { } status)
        {
            Visibility = Visibility.Collapsed;
            return;
        }

        Visibility = Visibility.Visible;
        SubtitleText.Text = $"{status.Branch ?? string.Empty} · {status.Head ?? string.Empty}";
        EntryHost.Children.Clear();

        if (status.Entries.Count == 0)
        {
            EntryHost.Children.Add(CleanText());
            return;
        }

        foreach (var entry in status.Entries)
        {
            EntryHost.Children.Add(BuildEntry(entry));
        }
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs args) => await RefreshAsync();

    private static TextBlock CleanText() => new()
    {
        Text = AppServices.Strings.Get(StringKeys.GitClean),
        FontFamily = PanelResources.Font(PanelMetrics.FontUi),
        FontSize = PanelResources.Size(PanelMetrics.BodySize),
        Foreground = PanelResources.Brush(PanelMetrics.TextSecondary),
    };

    private static Grid BuildEntry(GitStatusEntry entry)
    {
        var grid = new Grid { ColumnSpacing = PanelMetrics.StepRowSpacing };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(MarkerWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var marker = new TextBlock
        {
            Text = GitStatusPalette.Marker(entry.Kind),
            TextAlignment = TextAlignment.Center,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = GitStatusPalette.Brush(entry.Kind),
        };

        var path = new TextBlock
        {
            Text = entry.Path,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = PanelResources.Brush(PanelMetrics.TextPrimary),
        };

        ToolTipService.SetToolTip(path, entry.Path);
        Grid.SetColumn(path, 1);
        grid.Children.Add(marker);
        grid.Children.Add(path);
        return grid;
    }
}
