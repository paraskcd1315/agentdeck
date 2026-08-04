using AgentDeck.Shell.Domain.Entities;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AgentDeck.Shell.Presentation.Workspace.Components;

public sealed partial class DiffView : UserControl
{
    private const double GutterWidth = 48;

    private readonly string _root;
    private readonly string _file;

    public DiffView(string root, string file)
    {
        InitializeComponent();

        _root = root;
        _file = file;
        PathText.Text = file;

        Loaded += async (_, _) => await ReloadAsync();
    }

    public async Task ReloadAsync()
    {
        LineHost.Children.Clear();

        var diff = await AppServices.Daemon.GitDiffAsync(_root, _file, CancellationToken.None);

        if (diff is null)
        {
            CountText.Text = string.Empty;
            LineHost.Children.Add(Message(StringKeys.DiffUnavailable));
            return;
        }

        CountText.Text = $"+{diff.Added}  -{diff.Removed}";

        if (diff.Binary)
        {
            LineHost.Children.Add(Message(StringKeys.DiffBinary));
            return;
        }

        if (diff.Lines.Count == 0)
        {
            LineHost.Children.Add(Message(StringKeys.DiffNoChanges));
            return;
        }

        foreach (var line in diff.Lines)
        {
            LineHost.Children.Add(BuildLine(line));
        }
    }

    private static TextBlock Message(string key) => new()
    {
        Text = AppServices.Strings.Get(key),
        TextWrapping = TextWrapping.Wrap,
        FontFamily = PanelResources.Font(PanelMetrics.FontUi),
        FontSize = PanelResources.Size(PanelMetrics.BodySize),
        Foreground = PanelResources.Brush(PanelMetrics.TextSecondary),
    };

    private static Border BuildLine(FileDiffLine line)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GutterWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(PanelMetrics.DiffGutterWidth),
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var foreground = PanelDiffPalette.Foreground(line.Kind);

        var numbers = new TextBlock
        {
            Text = $"{Number(line.OldLine)} {Number(line.NewLine)}",
            TextAlignment = TextAlignment.Right,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = PanelResources.Brush(PanelMetrics.TextTertiary),
        };

        var marker = new TextBlock
        {
            Text = PanelDiffPalette.Marker(line.Kind),
            TextAlignment = TextAlignment.Center,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = foreground,
        };

        var text = new TextBlock
        {
            Text = line.Text,
            FontFamily = PanelResources.Font(PanelMetrics.FontMono),
            FontSize = PanelResources.Size(PanelMetrics.MonoSize),
            Foreground = foreground,
            IsTextSelectionEnabled = true,
        };

        Grid.SetColumn(marker, 1);
        Grid.SetColumn(text, 2);
        grid.Children.Add(numbers);
        grid.Children.Add(marker);
        grid.Children.Add(text);

        return new Border
        {
            Child = grid,
            Background = PanelDiffPalette.Background(line.Kind),
        };
    }

    private static string Number(int? value) => value is { } number ? $"{number,4}" : "    ";
}
