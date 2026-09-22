using System;
using System.Threading.Tasks;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action _goToLogin;
    private readonly Action _onSuccess;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isBusy;

    public RegisterViewModel(ApiClient api, Action goToLogin, Action onSuccess)
    {
        _api = api;
        _goToLogin = goToLogin;
        _onSuccess = onSuccess;
    }

    [RelayCommand]
    private async Task Register()
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.RegisterAsync(Username.Trim(), Email.Trim(), Password);
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
    private void GoToLogin() => _goToLogin();
}
