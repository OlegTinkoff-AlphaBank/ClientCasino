using System;
using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action _onLogout;

    [ObservableProperty] private UserProfileDto? _profile;
    [ObservableProperty] private object _currentPage;
    [ObservableProperty] private string _activeSection = "games";

    public ShellViewModel(ApiClient api, Action onLogout)
    {
        _api = api;
        _onLogout = onLogout;
        _currentPage = new GamesViewModel(_api, OpenGame);

        _ = LoadProfile();
    }

    public async Task LoadProfile()
    {
        try
        {
            Profile = await _api.GetMeAsync();
            OnPropertyChanged(nameof(IsAdmin));
        }
        catch (ApiException)
        {
            // не удалось получить профиль - баланс просто не покажется,
            // остальной функционал не блокируем
        }
    }

    private void OpenGame(GameDto game) =>
        CurrentPage = new GameDetailViewModel(_api, game, RefreshBalance);

    private void RefreshBalance() => _ = LoadProfile();

    [RelayCommand]
    private void NavigateGames()
    {
        ActiveSection = "games";
        CurrentPage = new GamesViewModel(_api, OpenGame);
    }

    [RelayCommand]
    private void NavigateDonations()
    {
        ActiveSection = "donations";
        CurrentPage = new DonationsViewModel(_api, RefreshBalance);
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        ActiveSection = "profile";
        CurrentPage = new ProfileViewModel(_api);
    }

    [RelayCommand]
    private void Logout() => _onLogout();
    
    public bool IsAdmin => Profile?.Role == "Admin";
    public string? Role => Profile?.Role + Profile?.Username + Profile?.Balance;

    [RelayCommand]
    private void NavigateAdmin()
    {
        ActiveSection = "admin";
        CurrentPage = new AdminViewModel(_api, RefreshBalance);
    }
}
