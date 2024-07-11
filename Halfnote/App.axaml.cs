using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Halfnote.Services;
using Halfnote.ViewModels;
using Halfnote.Views;

namespace Halfnote
{
    public partial class App : Application
    {
        private readonly ResourceDictionary _lightTheme;
        private readonly ResourceDictionary _darkTheme;
        private FileService _fs = new FileService();
        private MainWindow _mainWindow;

        public App()
        {
            _lightTheme = (ResourceDictionary)
                AvaloniaXamlLoader.Load(new Uri("avares://Halfnote/Themes/LightTheme.axaml"));
            _darkTheme = (ResourceDictionary)
                AvaloniaXamlLoader.Load(new Uri("avares://Halfnote/Themes/DarkTheme.axaml"));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            LoadTheme(_fs.AppSettings.Theme);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _mainWindow = new MainWindow(_fs) { };
                desktop.MainWindow = _mainWindow;

                desktop.ShutdownRequested += DesktopOnShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            _mainWindow.SavePage();
            await _fs.SaveSettings();
        }

        public string GetTheme()
        {
            var resources = Current.Resources.MergedDictionaries;
            if (resources.Contains(_lightTheme))
            {
                return "Light";
            }
            else
            {
                return "Dark";
            }
        }

        public void LoadTheme(string theme)
        {
            var resources = Current.Resources.MergedDictionaries;
            resources.Clear();
            switch (theme)
            {
                case "Light":
                    resources.Add(_lightTheme);
                    break;
                case "Dark":
                    resources.Add(_darkTheme);
                    break;
            }
        }
    }
}
