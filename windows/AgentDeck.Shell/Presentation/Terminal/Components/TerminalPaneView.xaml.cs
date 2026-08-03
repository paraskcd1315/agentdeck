using System.ComponentModel;

using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalPaneView : UserControl
{
    private const double ActiveOpacity = 1;
    private const double InactiveOpacity = 0.45;

    private TerminalViewModel? _viewModel;

    public TerminalPaneView()
    {
        InitializeComponent();
        GridView.GridSizeChanged += OnGridSizeChanged;
        SplitButton.Content = TerminalGlyphs.Split;
        CloseButton.Content = TerminalGlyphs.Close;

        ToolTipService.SetToolTip(SplitButton, AppServices.Strings.Get(StringKeys.TerminalSplit));
        ToolTipService.SetToolTip(CloseButton, AppServices.Strings.Get(StringKeys.TerminalPaneClose));
    }

    public event EventHandler<TextGridSize>? GridSizeChanged;

    public event EventHandler<TerminalSplitRequest>? SplitRequested;

    public event EventHandler<TerminalPane>? CloseRequested;

    public event EventHandler<TerminalPane>? DragStarted;

    public event EventHandler<TerminalDropRequest>? PaneDropped;

    public TerminalPane? Pane { get; private set; }

    public void Bind(TerminalPane pane, bool active, bool closable)
    {
        if (_viewModel is { } previous)
        {
            previous.GridChanged -= OnGridChanged;
            previous.PropertyChanged -= OnViewModelPropertyChanged;
        }

        Pane = pane;
        _viewModel = pane.ViewModel;
        _viewModel.GridChanged += OnGridChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        FootText.Text = _viewModel.Status;

        GridView.Model = _viewModel.Grid;
        CloseButton.Visibility = closable ? Visibility.Visible : Visibility.Collapsed;

        SetActive(active);
        RenderStrip();
        Redraw();
    }

    public void SetActive(bool active) => SessionChip.Opacity = active ? ActiveOpacity : InactiveOpacity;

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

    private void RenderStrip()
    {
        if (Pane is not { } pane || _viewModel is not { } viewModel)
        {
            return;
        }

        SessionText.Text = TerminalChrome.Session(pane.Title, viewModel.PtyId);
        StripMeta.Text = TerminalChrome.Dimensions(viewModel);
    }

    private void OnStripDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        if (Pane is not { } pane)
        {
            args.Cancel = true;
            return;
        }

        args.Data.RequestedOperation = DataPackageOperation.Move;
        args.Data.SetText(pane.Title);
        DragStarted?.Invoke(this, pane);
    }

    private void OnPaneDragOver(object sender, DragEventArgs args)
    {
        args.AcceptedOperation = DataPackageOperation.Move;
        args.DragUIOverride.IsGlyphVisible = false;
        args.Handled = true;

        var zone = TerminalDropZones.Resolve(
            args.GetPosition(Surface),
            Surface.ActualWidth,
            Surface.ActualHeight);

        ShowDropHint(zone);
    }

    private void OnPaneDragLeave(object sender, DragEventArgs args) =>
        DropHint.Visibility = Visibility.Collapsed;

    private void OnPaneDrop(object sender, DragEventArgs args)
    {
        DropHint.Visibility = Visibility.Collapsed;
        args.Handled = true;

        if (Pane is not { } pane)
        {
            return;
        }

        var zone = TerminalDropZones.Resolve(
            args.GetPosition(Surface),
            Surface.ActualWidth,
            Surface.ActualHeight);

        PaneDropped?.Invoke(this, new TerminalDropRequest(pane, zone.Orientation, zone.Before));
    }

    private void ShowDropHint(TerminalDropZone zone)
    {
        DropHint.Visibility = Visibility.Visible;

        var horizontal = zone.Orientation == TerminalSplitOrientation.Horizontal;

        DropHint.HorizontalAlignment = horizontal
            ? (zone.Before ? HorizontalAlignment.Left : HorizontalAlignment.Right)
            : HorizontalAlignment.Stretch;

        DropHint.VerticalAlignment = horizontal
            ? VerticalAlignment.Stretch
            : (zone.Before ? VerticalAlignment.Top : VerticalAlignment.Bottom);

        DropHint.Width = horizontal ? Surface.ActualWidth / 2 : double.NaN;
        DropHint.Height = horizontal ? double.NaN : Surface.ActualHeight / 2;
    }

    private void OnSurfaceSizeChanged(object sender, SizeChangedEventArgs args) =>
        Surface.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height),
        };

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(TerminalViewModel.Status) && sender is TerminalViewModel viewModel)
        {
            FootText.Text = viewModel.Status;
        }
    }

    private void OnGridChanged(object? sender, EventArgs args) => Redraw();

    private void OnGridSizeChanged(object? sender, TextGridSize size)
    {
        GridSizeChanged?.Invoke(this, size);
        RenderStrip();
    }

    private void OnSplitClick(object sender, RoutedEventArgs args)
    {
        if (Pane is not { } pane)
        {
            return;
        }

        var menu = new MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
        menu.Items.Add(BuildSplitItem(
            StringKeys.TerminalSplitRight,
            TerminalGlyphs.SplitRight,
            pane,
            TerminalSplitOrientation.Horizontal));
        menu.Items.Add(BuildSplitItem(
            StringKeys.TerminalSplitDown,
            TerminalGlyphs.SplitDown,
            pane,
            TerminalSplitOrientation.Vertical));

        menu.ShowAt(SplitButton);
    }

    private MenuFlyoutItem BuildSplitItem(
        string key,
        string glyph,
        TerminalPane pane,
        TerminalSplitOrientation orientation)
    {
        var item = new MenuFlyoutItem
        {
            Text = AppServices.Strings.Get(key),
            Icon = new FontIcon
            {
                Glyph = glyph,
                FontFamily = Panels.Utils.PanelResources.Font("AdFontIcon"),
            },
        };

        item.Click += (_, _) =>
            SplitRequested?.Invoke(this, new TerminalSplitRequest(pane, orientation));

        return item;
    }

    private void OnCloseClick(object sender, RoutedEventArgs args)
    {
        if (Pane is { } pane)
        {
            CloseRequested?.Invoke(this, pane);
        }
    }
}
