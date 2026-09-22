using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoClient.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    [ObservableProperty]
    private object _currentViewModel;

    public AppViewModel(ApiClient api)
    {
        _api = api;
        _currentViewModel = new LoginViewModel(_api, ShowRegister, ShowShell);
    }
    
    public void EnterApp() => ShowShell();

    private void ShowRegister() =>
        CurrentViewModel = new RegisterViewModel(_api, ShowLogin, ShowShell);

    private void ShowLogin() =>
        CurrentViewModel = new LoginViewModel(_api, ShowRegister, ShowShell);

    private void ShowShell() =>
        CurrentViewModel = new ShellViewModel(_api, ShowLogin);
}
