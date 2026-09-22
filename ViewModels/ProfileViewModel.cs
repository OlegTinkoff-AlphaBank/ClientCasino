using System.Threading.Tasks;
using CasinoClient.Models;
using CasinoClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoClient.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    [ObservableProperty] private UserProfileDto? _profile;
    [ObservableProperty] private string _errorMessage = "";

    public ProfileViewModel(ApiClient api)
    {
        _ = Load(api);
    }

    private async Task Load(ApiClient api)
    {
        try
        {
            Profile = await api.GetMeAsync();
            
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
