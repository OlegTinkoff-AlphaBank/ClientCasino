using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoClient.ViewModels;

public partial class GamesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action<GameDto> _onSelect;

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = "";

    public ObservableCollection<GameDto> Games { get; } = new();

    public GamesViewModel(ApiClient api, Action<GameDto> onSelect)
    {
        _api = api;
        _onSelect = onSelect;
        _ = LoadGames();
    }

    private async Task LoadGames()
    {
        IsLoading = true;
        try
        {
            var games = await _api.GetGamesAsync();
            Games.Clear();
            foreach (var g in games) Games.Add(g);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SelectGame(GameDto game) => _onSelect(game);
}
