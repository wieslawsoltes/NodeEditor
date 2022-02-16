using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using NodeEditorDemo;
using NodeEditorDemo.ViewModels;
using NodeEditorDemo.Views;

namespace NodeEditor.Android
{
    [Activity(Label = "NodeEditor",
        MainLauncher = true,
        Icon = "@drawable/icon",
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainBaseActivity : AvaloniaActivity
    {
        static MainBaseActivity()
        {
            App.EnableInputOutput = true;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (Avalonia.Application.Current == null)
            {
                AppBuilder.Configure<App>()
                    .UseAndroid()
                    .SetupWithoutStarting();
            }

            base.OnCreate(savedInstanceState);

            var mainView = new MainView
            {
                DataContext = new MainWindowViewModel
                {
                    IsEditMode = true,
                    IsToolboxVisible = false
                }
            };

            Content = mainView;
        }
    }
}
