using System.ComponentModel;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Halfnote.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private IHighlightingDefinition _highlightProfile;

    [ObservableProperty]
    private bool _optionSyntax = true;

    [ObservableProperty]
    private bool _optionLineNumbers = false;

    [ObservableProperty]
    private bool _optionWrap = true;

    [ObservableProperty]
    private int _optionEditorFontSize = 14;

    [ObservableProperty]
    private int _optionAutosaveInterval = 60;

    [ObservableProperty]
    private float _optionPreviewDelay = 0.5f;

    [ObservableProperty]
    private FontFamily _editorFont = new FontFamily("Cascadia Code,Consolas,Menlo,Monospace");

    [ObservableProperty]
    private string _optionEditorFont = "Cascadia Code,Consolas,Menlo,Monospace";

    [ObservableProperty]
    private IHighlightingDefinition? _syntaxHighlighting = null;

    private void InitializeHighlighting()
    {
        var highlighting = HighlightingManager.Instance.GetDefinition("MarkDown");

        highlighting.GetNamedColor("Heading").Foreground = new SimpleHighlightingBrush(
            Colors.Orchid
        );

        _highlightProfile = highlighting;
    }

    private void LoadAppPreferences()
    {
        OptionSyntax = _fs.AppSettings.Syntax;
        OptionEditorFont = _fs.AppSettings.EditorFont;
        OptionAutosaveInterval = _fs.AppSettings.AutosaveInterval;
        OptionEditorFontSize = _fs.AppSettings.EditorFontSize;
        OptionPreviewDelay = _fs.AppSettings.PreviewDelay;
        OptionWrap = _fs.AppSettings.Wrap;
        OptionLineNumbers = _fs.AppSettings.LineNumbers;

        SetHighlighting();
    }

    private void OnAppSettingsChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OptionAutosaveInterval):
                _fs.AppSettings.AutosaveInterval = OptionAutosaveInterval;
                InitializeTimers();
                break;

            case nameof(OptionEditorFont):
                if (OptionEditorFont.Length < 1)
                {
                    EditorFont = "monospace";
                    _fs.AppSettings.EditorFont = OptionEditorFont;
                    return;
                }
                EditorFont = new FontFamily(OptionEditorFont);
                _fs.AppSettings.EditorFont = OptionEditorFont;
                break;

            case nameof(OptionEditorFontSize):
                _fs.AppSettings.EditorFontSize = OptionEditorFontSize;
                break;

            case nameof(OptionLineNumbers):
                _fs.AppSettings.LineNumbers = OptionLineNumbers;
                break;

            case nameof(OptionPreviewDelay):
                _fs.AppSettings.PreviewDelay = OptionPreviewDelay;
                InitializeTimers();
                break;

            case nameof(OptionSyntax):
                _fs.AppSettings.Syntax = OptionSyntax;
                SetHighlighting();
                break;

            case nameof(OptionWrap):
                _fs.AppSettings.Wrap = OptionWrap;
                break;
        }
    }

    private void SetHighlighting()
    {
        if (OptionSyntax)
        {
            SyntaxHighlighting = _highlightProfile;
        }
        else
        {
            SyntaxHighlighting = null;
        }
    }
}
