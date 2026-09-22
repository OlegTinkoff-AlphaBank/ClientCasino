using System;
using System.IO;
using System.Text.Json;

namespace CasinoClient.Services;

public record StoredTokens(string AccessToken, string RefreshToken);

public static class TokenStorage
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CasinoClient", "tokens.json");

    public static void Save(string accessToken, string refreshToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(new StoredTokens(accessToken, refreshToken)));
    }

    public static StoredTokens? Load()
    {
        if (!File.Exists(FilePath)) return null;
        try { return JsonSerializer.Deserialize<StoredTokens>(File.ReadAllText(FilePath)); }
        catch { return null; }
    }

    public static void Clear()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}