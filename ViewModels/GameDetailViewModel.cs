using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class GameDetailViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action _onBalanceChanged;

    [ObservableProperty] private GameDto _game;
    [ObservableProperty] private decimal _betAmount;
    [ObservableProperty] private string _resultMessage = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _lastWin;
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private OutcomeItemViewModel? _currentSpinOutcome;

    public bool IsSlots => Game.Type == "Slots";
    public bool IsRoulette => Game.Type == "Roulette";

    public ObservableCollection<OutcomeItemViewModel> Outcomes { get; } = new();

    public GameDetailViewModel(ApiClient api, GameDto game, Action onBalanceChanged)
    {
        _api = api;
        _game = game;
        _betAmount = game.MinBet;
        _onBalanceChanged = onBalanceChanged;
        _ = LoadOutcomes();
    }

    private async Task LoadOutcomes()
    {
        try
        {
            var outcomes = await _api.GetOutcomesAsync(Game.Id);
            Outcomes.Clear();
            foreach (var o in outcomes)
                Outcomes.Add(new OutcomeItemViewModel(_api, o));
        }
        catch (ApiException)
        {
            // для игр без исходов (например, Кости) список просто останется пустым
        }
    }

    [RelayCommand]
    private async Task PlaceBet()
    {
        ResultMessage = "";
        HasResult = false;

        if (BetAmount < Game.MinBet || BetAmount > Game.MaxBet)
        {
            ResultMessage = $"Ставка должна быть от {Game.MinBet} до {Game.MaxBet}";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _api.PlayAsync(Game.Id, BetAmount);

            if (Outcomes.Count > 0)
            {
                int winIndex = result.WinningOutcomeId is null
                    ? -1
                    : Outcomes.ToList().FindIndex(o => o.Id == result.WinningOutcomeId);

                await RunSpinAnimation(winIndex);
            }

            LastWin = result.IsWin;
            HasResult = true;
            ResultMessage = result.IsWin
                ? $"Победа! +{result.Payout:0.##}"
                : $"Проигрыш -{BetAmount:0.##}";

            _onBalanceChanged();
        }
        catch (ApiException ex)
        {
            ResultMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Честная анимация: результат уже получен от сервера ДО начала цикла,
    // сама прокрутка - чисто визуальная имитация "доезда" до готового ответа.
    private async Task RunSpinAnimation(int winIndex)
    {
        if (Outcomes.Count == 0) return;
        if (winIndex < 0) winIndex = 0;

        int steps = Outcomes.Count * 3 + winIndex; // несколько кругов + доезд
        int delay = 40;

        for (int i = 0; i <= steps; i++)
        {
            CurrentSpinOutcome = Outcomes[i % Outcomes.Count];

            if (i > steps - Outcomes.Count)
                delay += 25; // замедление ближе к концу - имитация инерции

            await Task.Delay(delay);
        }

        CurrentSpinOutcome = Outcomes[winIndex];
    }
}