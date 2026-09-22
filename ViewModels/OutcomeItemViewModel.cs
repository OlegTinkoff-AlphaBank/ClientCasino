using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoClient.ViewModels;

public partial class OutcomeItemViewModel : ObservableObject
{
    public int Id { get; }
    public string Name { get; }
    public decimal Odds { get; }

    [ObservableProperty] private Bitmap? _image;

    public OutcomeItemViewModel(ApiClient api, GameOutcomeDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Odds = dto.Odds;

        if (!string.IsNullOrEmpty(dto.ImageUrl))
            _ = LoadImage(api, dto.ImageUrl);
    }

    private async Task LoadImage(ApiClient api, string imageUrl)
    {
        try
        {
            Image = await api.LoadImageAsync(imageUrl);
        }
        catch
        {
            // картинка не критична для самой ставки - просто не покажется
        }
    }
}