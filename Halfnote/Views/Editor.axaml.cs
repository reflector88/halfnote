using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Highlighting;

namespace Halfnote.Views;

public partial class Editor : UserControl
{
    private bool _isControlPressed = false;
    private ScrollViewer? _scrollViewer;
    public MainWindow ParentWindow { get; set; }

    private enum FontScale
    {
        Max = 96,
        Min = 6,
        Interval = 2,
    }

    public Editor()
    {
        InitializeComponent();
        editor.Loaded += Editor_Loaded;
        EnableSyntaxHighlighting();
    }

    private void EnableSyntaxHighlighting()
    {
        var highlighting = HighlightingManager.Instance.GetDefinition("MarkDown");

        highlighting.GetNamedColor("Heading").Foreground = new SimpleHighlightingBrush(
            Colors.Orchid
        );

        editor.SyntaxHighlighting = highlighting;
    }

    private void DisableSyntaxHighlighting()
    {
        editor.SyntaxHighlighting = null;
    }

    private void Editor_Loaded(object? sender, RoutedEventArgs e)
    {
        _scrollViewer = editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged += _scrollViewer_ScrollChanged;
        }
        editor.Document.UndoStack.SizeLimit = 100;
    }

    // Note: ScrollChanged only triggers when the editor actually scrolls. It is handled in
    // the textbox's built-in ScrollViewer component. OnPointerWheelChanged only triggers when
    // the editor does NOT scroll.

    private void _scrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer is null)
            return;

        if (_isControlPressed)
        {
            _scrollViewer.Offset -= e.OffsetDelta;
        }

        if (e.OffsetDelta.Y < 0 && editor.FontSize < (int)FontScale.Max && _isControlPressed)
        {
            ZoomIn();
            _scrollViewer.Offset += new Avalonia.Vector(0, 50);
        }
        else if (e.OffsetDelta.Y > 0 && editor.FontSize > (int)FontScale.Min && _isControlPressed)
        {
            ZoomOut();
            _scrollViewer.Offset -= new Avalonia.Vector(0, 50);
        }
    }

    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y == 1 && editor.FontSize < (int)FontScale.Max && _isControlPressed)
        {
            ZoomIn();
        }
        else if (e.Delta.Y == -1 && editor.FontSize > (int)FontScale.Min && _isControlPressed)
        {
            ZoomOut();
        }
    }

    private void ZoomIn()
    {
        editor.FontSize += (int)FontScale.Interval;
    }

    private void ZoomOut()
    {
        editor.FontSize -= (int)FontScale.Interval;
    }

    public void ClearUndoStack()
    {
        editor.Document.UndoStack.SizeLimit = 0;
        editor.Document.UndoStack.SizeLimit = 100;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlPressed = true;
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlPressed = false;
        }
    }

    private void MenuBarViewHandler(object sender, RoutedEventArgs e)
    {
        ParentWindow?.MenuBarView();
    }
}
