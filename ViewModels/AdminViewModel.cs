using System;
using System.Threading.Tasks;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    
    private readonly Action _onBalanceChanged;

    [ObservableProperty] private string _targetUsername = "";
    [ObservableProperty] private decimal _addAmount = 100;
    [ObservableProperty] private string _balanceStatus = "";

    [ObservableProperty] private string _newGameName = "";
    [ObservableProperty] private string _newGameType = "";
    [ObservableProperty] private decimal _newGameMinBet = 10;
    [ObservableProperty] private decimal _newGameMaxBet = 1000;
    [ObservableProperty] private string _gameStatus = "";

    [ObservableProperty] private int _outcomeGameId;
    [ObservableProperty] private decimal _outcomeM;
    [ObservableProperty] private decimal _outcomeW;
    [ObservableProperty] private string _outcomeStatus = "";

    public AdminViewModel(ApiClient api, Action onBalanceChanged)
    {
        _api = api;
        _onBalanceChanged = onBalanceChanged;
    }

    [RelayCommand]
    private async Task AddBalance()
    {
        try
        {
            var balance = await _api.AdminAddBalanceAsync(TargetUsername.Trim(), AddAmount);
            BalanceStatus = $"Новый баланс {TargetUsername}: {balance:0.##}";
            _onBalanceChanged();
        }
        catch (ApiException ex) { BalanceStatus = ex.Message; }
    }

    [RelayCommand]
    private async Task CreateGame()
    {
        try
        {
            var game = await _api.AdminCreateGameAsync(NewGameName, NewGameType, NewGameMinBet, NewGameMaxBet);
            GameStatus = $"Создана игра #{game.Id}: {game.Name}";
        }
        catch (ApiException ex) { GameStatus = ex.Message; }
    }

    [RelayCommand]
    private async Task CreateOutcome()
    {
        try
        {
            await _api.AdminCreateOutcomeAsync(OutcomeGameId, OutcomeM, OutcomeW);
            OutcomeStatus = "Исход добавлен";
        }
        catch (ApiException ex) { OutcomeStatus = ex.Message; }
    }
}