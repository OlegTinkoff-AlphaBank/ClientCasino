using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CasinoClient.Models;

namespace CasinoClient.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public class ApiClient
{
    private const string BaseUrl = "http://localhost:5294/";

    private readonly HttpClient _http;

    public ApiClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public bool IsAuthenticated => _http.DefaultRequestHeaders.Authorization is not null;

    // ===================== Auth =====================

    public async Task RegisterAsync(string username, string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/Auth/register",
            new RegisterDto(username, email, password));
        await EnsureSuccess(response);
    }

    public async Task LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/Auth/login", new LoginDto(username, password));
        await EnsureSuccess(response);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                     ?? throw new ApiException("Сервер вернул пустой ответ");

        SetTokens(result.AccessToken, result.RefreshToken);
    }
    
    private void SetTokens(string accessToken, string refreshToken)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        TokenStorage.Save(accessToken, refreshToken);
    }
    
    public async Task<bool> TryRestoreSessionAsync()
    {
        var stored = TokenStorage.Load();
        if (stored is null) return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/Auth/refresh",
                new RefreshRequestDto(stored.RefreshToken));

            if (!response.IsSuccessStatusCode) { TokenStorage.Clear(); return false; }

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (result is null) return false;

            SetTokens(result.AccessToken, result.RefreshToken);
            return true;
        }
        catch { return false; }
    }

    public void SetToken(string accessToken)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public void Logout()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        TokenStorage.Clear();
    }

    // ===================== User =====================

    public async Task<UserProfileDto> GetMeAsync()
    {
        var response = await _http.GetAsync("api/v1.0/User/me");
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<UserProfileDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    // GET /User/refresh - необычно, что это GET, а не POST с телом.
    // Предполагаю, что сервер сам достаёт refresh-токен откуда-то
    // (например, из текущего Authorization) и просто выдаёт новый access-токен.
    public async Task RefreshTokenAsync()
    {
        var response = await _http.GetAsync("api/v1.0/User/refresh");
        await EnsureSuccess(response);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");

        SetToken(result.AccessToken);
    }

    // ===================== Game =====================

    public async Task<List<GameDto>> GetGamesAsync()
    {
        var response = await _http.GetAsync("api/v1.0/Game/list");
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GameDto>>() ?? new();
    }

    public async Task<GameDto> GetGameAsync(int id)
    {
        var response = await _http.GetAsync($"api/v1.0/Game/{id}");
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<GameDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    public async Task<PlayResultDto> PlayAsync(int gameId, decimal betAmount)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/Game/bet",
            new PlayDto(gameId, betAmount));
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<PlayResultDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    // ===================== Donation =====================

    public async Task<List<DonationDto>> GetDonationsAsync()
    {
        var response = await _http.GetAsync("api/v1.0/Donation/list");
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<DonationDto>>() ?? new();
    }

    public async Task<DonateResponseDto> DonateAsync(decimal amount)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/Donation/donate",
            new DonateRequestDto(amount));
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<DonateResponseDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    public async Task<CheckDonateResponseDto> CheckDonateAsync()
    {
        var response = await _http.PostAsync("api/v1.0/Donation/check_donate", null);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<CheckDonateResponseDto>()
            ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    // ===================== Общее =====================

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string message = $"Ошибка сервера ({(int)response.StatusCode})";
        try
        {
            var text = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(text)) message = text;
        }
        catch { }

        throw new ApiException(message);
    }
    
    public async Task<decimal> AdminAddBalanceAsync(string username, decimal amount)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/User/add-balance",
            new AdminAddBalanceDto(username, amount));
        await EnsureSuccess(response);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        return doc.GetProperty("balance").GetDecimal();
    }

    public async Task<GameDto> AdminCreateGameAsync(string name, string type, decimal minBet, decimal maxBet)
    {
        var response = await _http.PostAsJsonAsync("api/v1.0/User/games",
            new CreateGameDto(name, type, minBet, maxBet));
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<GameDto>()
               ?? throw new ApiException("Сервер вернул пустой ответ");
    }

    public async Task AdminCreateOutcomeAsync(int gameId, decimal Multiplier, decimal Weight)
    {
        var response = await _http.PostAsJsonAsync($"api/v1.0/User/games/outcomes",
            new CreateGameOutcomeDto(gameId, Multiplier, Weight));
        await EnsureSuccess(response);
    }
    
    public async Task<List<GameOutcomeDto>> GetOutcomesAsync(int gameId)
    {
        var response = await _http.GetAsync($"api/v1.0/Game/{gameId}/outcomes");
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GameOutcomeDto>>() ?? new();
    }

    public async Task<Avalonia.Media.Imaging.Bitmap?> LoadImageAsync(string imageUrl)
    {
        // imageUrl с сервера относительный ("/images/outcomes/xxx.png") -
        // достраиваем до полного адреса
        var url = imageUrl.StartsWith("http")
            ? imageUrl
            : new Uri(new Uri(BaseUrl), imageUrl).ToString();

        var bytes = await _http.GetByteArrayAsync(url);
        using var stream = new MemoryStream(bytes);
        return new Avalonia.Media.Imaging.Bitmap(stream);
    }
}
