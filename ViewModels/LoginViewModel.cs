using System;
using System.Threading.Tasks;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action _goToRegister;
    private readonly Action _onSuccess;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isBusy;

    public LoginViewModel(ApiClient api, Action goToRegister, Action onSuccess)
    {
        _api = api;
        _goToRegister = goToRegister;
        _onSuccess = onSuccess;
    }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.LoginAsync(Username.Trim(), Password);
            _onSuccess();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Не удалось подключиться к серверу";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoToRegister() => _goToRegister();
}
