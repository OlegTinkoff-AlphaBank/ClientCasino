using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CasinoClient.ViewModels;

public partial class DonationsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Action _onBalanceChanged;

    [ObservableProperty] private decimal _amount = 100;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string? _donationCode;
    [ObservableProperty] private string? _paymentUrl;
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<DonationDto> History { get; } = new();

    public DonationsViewModel(ApiClient api, Action onBalanceChanged)
    {
        _api = api;
        _onBalanceChanged = onBalanceChanged;
        _ = LoadHistory();
    }

    private async Task LoadHistory()
    {
        try
        {
            var donations = await _api.GetDonationsAsync();
            History.Clear();
            foreach (var d in donations) History.Add(d);
        }
        catch (ApiException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task StartDonate()
    {
        StatusMessage = "";
        IsBusy = true;
        try
        {
            var result = await _api.DonateAsync(Amount);
            DonationCode = result.DonationCode;
            PaymentUrl = result.PaymentUrl;
            StatusMessage = "Укажите этот код в комментарии к донату, затем нажмите «Проверить»";
        }
        catch (ApiException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckDonate()
    {
        IsBusy = true;
        try
        {
            var result = await _api.CheckDonateAsync();
            StatusMessage = result.Matched
                ? $"Донат зачислен: +{result.CreditedAmount:0.##}"
                : "Пока не найден — попробуйте через минуту";

            if (result.Matched)
            {
                _onBalanceChanged();
                await LoadHistory();
            }
        }
        catch (ApiException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
