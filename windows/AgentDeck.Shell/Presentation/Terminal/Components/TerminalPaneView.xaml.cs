using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalPaneView : UserControl
{
    private TerminalViewModel? _viewModel;

    public TerminalPaneView()
    {
        InitializeComponent();
        GridView.GridSizeChanged += OnGridSizeChanged;
        SizeChanged += OnSizeChanged;
    }

    public event EventHandler<TextGridSize>? GridSizeChanged;

    public TerminalPane? Pane { get; private set; }

    public void Bind(TerminalPane pane)
    {
        if (_viewModel is { } previous)
        {
            previous.GridChanged -= OnGridChanged;
        }

        Pane = pane;
        _viewModel = pane.ViewModel;
        _viewModel.GridChanged += OnGridChanged;

        GridView.Model = _viewModel.Grid;
        Redraw();
    }

    public int ColumnAt(double x) => GridView.ColumnAt(x);

    public int RowAt(double y) => GridView.RowAt(y);

    public void Redraw()
    {
        if (_viewModel is not { } viewModel)
        {
            return;
        }

        GridView.Invalidate();
        ScrollIndicator.Update(viewModel.History, viewModel.DisplayOffset, viewModel.Grid.Rows);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args) =>
        Surface.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height),
        };

    private void OnGridChanged(object? sender, EventArgs args) => Redraw();

    private void OnGridSizeChanged(object? sender, TextGridSize size) =>
        GridSizeChanged?.Invoke(this, size);
}
