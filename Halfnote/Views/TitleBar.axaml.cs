using Avalonia.Controls;
using Avalonia.Interactivity;
using Halfnote.ViewModels;

namespace Halfnote.Views
{
    public partial class TitleBar : UserControl
    {
        private string _priorName;
        public MainWindow ParentWindow { get; set; }

        public TitleBar()
        {
            InitializeComponent();
        }

        public void ToggleFocus(object? sender, RoutedEventArgs e)
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
            }
        }

        private void LostFocusHandler(object sender, RoutedEventArgs e)
        {
            if (_priorName != TextBox1.Text)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.RenamePage();
                }
            }

            TextBox1.IsEnabled = false;
            // Deselects text
            TextBox1.SelectionStart = TextBox1.CaretIndex;
            TextBox1.SelectionEnd = TextBox1.CaretIndex;
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

        private void TitleBarViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.TitleBarView();
        }
    }
}
