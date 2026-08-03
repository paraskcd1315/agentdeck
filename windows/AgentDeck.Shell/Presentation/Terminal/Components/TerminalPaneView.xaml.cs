using System.ComponentModel;

using AgentDeck.Shell.Presentation.DesignSystem.TextGrid;
using AgentDeck.Shell.Presentation.Terminal.Utils;
using AgentDeck.Shell.Presentation.Panels.Utils;
using AgentDeck.Shell.Presentation.Terminal.ViewModels;
using AgentDeck.Shell.Utils;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.System;

namespace AgentDeck.Shell.Presentation.Terminal.Components;

public sealed partial class TerminalPaneView : UserControl
{
    private const string IconFontKey = "AdFontIcon";
    private const double ActiveOpacity = 1;
    private const double InactiveOpacity = 0.45;

    private TerminalViewModel? _viewModel;
    private int _index;
    private int _total;

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

    public event EventHandler? DragEnded;

    public event EventHandler<TerminalDropRequest>? PaneDropped;

    public TerminalPane? Pane { get; private set; }

    public void Bind(TerminalPane pane, bool active, bool closable, int index, int total)
    {
        _index = index;
        _total = total;

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

        SessionText.Text = TerminalChrome.Session(pane.Title, viewModel.PtyId, _index, _total);
        StripMeta.Text = TerminalChrome.Dimensions(viewModel);
    }

    private async void OnStripDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        if (Pane is not { } pane)
        {
            args.Cancel = true;
            return;
        }

        args.Data.RequestedOperation = DataPackageOperation.Move;
        args.Data.SetText(pane.Title);
        DragStarted?.Invoke(this, pane);

        var deferral = args.GetDeferral();

        try
        {
            var render = new RenderTargetBitmap();
            await render.RenderAsync(SessionChip);

            var pixels = await render.GetPixelsAsync();
            var bitmap = SoftwareBitmap.CreateCopyFromBuffer(
                pixels,
                BitmapPixelFormat.Bgra8,
                render.PixelWidth,
                render.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            args.DragUI.SetContentFromSoftwareBitmap(bitmap);
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void OnStripDropCompleted(UIElement sender, DropCompletedEventArgs args) =>
        DragEnded?.Invoke(this, EventArgs.Empty);

    private void OnRenameRequested(object sender, DoubleTappedRoutedEventArgs args)
    {
        if (Pane is not { } pane)
        {
            return;
        }

        args.Handled = true;
        SessionEdit.Text = pane.Title;
        SessionEdit.Visibility = Visibility.Visible;
        SessionText.Visibility = Visibility.Collapsed;
        SessionEdit.Focus(FocusState.Programmatic);
        SessionEdit.SelectAll();
    }

    private void OnRenameKeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (args.Key == VirtualKey.Enter)
        {
            args.Handled = true;
            CommitRename();
            return;
        }

        if (args.Key == VirtualKey.Escape)
        {
            args.Handled = true;
            CancelRename();
        }
    }

    private void OnRenameCommitted(object sender, RoutedEventArgs args) => CommitRename();

    private void CommitRename()
    {
        if (SessionEdit.Visibility == Visibility.Collapsed)
        {
            return;
        }

        var name = SessionEdit.Text.Trim();

        if (Pane is { } pane && name.Length > 0)
        {
            pane.Title = name;
            RenderStrip();
        }

        CancelRename();
    }

    private void CancelRename()
    {
        SessionEdit.Visibility = Visibility.Collapsed;
        SessionText.Visibility = Visibility.Visible;
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

        DropHint.Width = horizontal ? TerminalMetrics.DropHintThickness : double.NaN;
        DropHint.Height = horizontal ? double.NaN : TerminalMetrics.DropHintThickness;
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
                FontFamily = PanelResources.Font(IconFontKey),
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
