using Avalonia.Controls;
using Avalonia.Interactivity;
using CasinoClient.Models;
using CasinoClient.ViewModels;

namespace CasinoClient.Views;

public partial class GamesView : UserControl
{
    public GamesView()
    {
        InitializeComponent();
    }

    private void GameCard_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: GameDto game } && DataContext is GamesViewModel vm)
        {
            vm.SelectGame(game);
        }
    }
}
