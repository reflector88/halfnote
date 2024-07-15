using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Halfnote.ViewModels;

namespace Halfnote.Views
{
    public partial class TitleBar : UserControl
    {
        private string _priorName;
        public MainWindow ParentWindow { get; set; }
        private bool _renameFlag;

        public TitleBar()
        {
            InitializeComponent();
        }

        public void ToggleFocus(object? sender, RoutedEventArgs e)
        {
            DoToggleFocus();
        }

        public void DoToggleFocus()
        {
            if (TextBox1.IsEnabled)
            {
                TextBox1.IsEnabled = false;
            }
            else
            {
                _priorName = TextBox1.Text;
                TextBox1.IsEnabled = true;

                TextBox1.Focus();
                TextBox1.CaretIndex = TextBox1.Text.Length;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && TextBox1.IsFocused)
            {
                _renameFlag = true;
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.RenamePage();
                }
                ParentWindow.Editor.editor.Focus();
            }
        }

        private void LostFocusHandler(object sender, RoutedEventArgs e)
        {
            if (_priorName != TextBox1.Text && !_renameFlag)
            {
                TextBox1.Text = _priorName;
            }

            TextBox1.IsEnabled = false;
            _renameFlag = false;
        }

        private void EditorViewHandler(object sender, RoutedEventArgs e)
        {
            EditorButton.IsChecked = true;
            SplitButton.IsChecked = false;
            PreviewButton.IsChecked = false;
            ParentWindow?.EditorView();
        }

        private void SplitViewHandler(object sender, RoutedEventArgs e)
        {
            EditorButton.IsChecked = false;
            SplitButton.IsChecked = true;
            PreviewButton.IsChecked = false;
            ParentWindow?.SplitView();
        }

        private void PreviewViewHandler(object sender, RoutedEventArgs e)
        {
            EditorButton.IsChecked = false;
            SplitButton.IsChecked = false;
            PreviewButton.IsChecked = true;
            ParentWindow?.PreviewView();
        }

        private void Binding(object? sender, Avalonia.Interactivity.RoutedEventArgs e) { }
    }
}
