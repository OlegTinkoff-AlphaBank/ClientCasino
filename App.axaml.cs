using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CasinoClient.Services;
using CasinoClient.ViewModels;
using CasinoClient.Views;

namespace CasinoClient;

public partial class App : Application
{
    private readonly ApiClient _api = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var appViewModel = new AppViewModel(_api);
            desktop.MainWindow = new MainWindow { DataContext = appViewModel };

            _ = TryAutoLogin(appViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task TryAutoLogin(AppViewModel appViewModel)
    {
        if (await _api.TryRestoreSessionAsync())
            appViewModel.EnterApp();
    }
}
