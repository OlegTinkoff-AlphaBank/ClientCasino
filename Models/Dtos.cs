namespace CasinoClient.Models;

// ===================== Auth =====================
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Username, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken);
public record RefreshRequestDto(string RefreshToken);

// ===================== User =====================
// GET /User/me - предположительная форма, сверь с реальным UserDto на сервере

// ===================== Game =====================
public record GameDto(int Id, string Name, string Type, decimal MinBet, decimal MaxBet, bool IsActive);

// POST /Game/bet - судя по коду PlayRoundByGameIdAsync(PlayDTO dto, int userId),
// который ты присылал раньше: GameId + BetAmount
public record PlayDto(int GameId, decimal BetAmount);

// Ответ ставки - поля предположительные, поправь под реальный контроллер

// ===================== Donation =====================
// GET /Donation/list - один донат в истории, предположительная форма
public record DonationDto(int Id, decimal Amount, string Status, string CreatedAt);

// POST /Donation/donate - что именно отправляется, чтобы "инициировать" донат,
// не видно из одного списка путей. Два вероятных варианта:
//  (а) сервер просто отдаёт код/ссылку для доната, тело запроса не нужно;
//  (б) нужно передать сумму. Оставляю с Amount - если эндпоинт не примет
//  тело, просто убери параметр в ApiClient.DonateAsync.
public record DonateRequestDto(decimal Amount);
public record DonateResponseDto(string DonationCode, string? PaymentUrl);

// POST /Donation/check_donate - вероятно просто дёргается без тела
// (проверяет "мои" последние донаты по токену), либо с Id конкретного доната.
// Тут тоже поправь под реальный контроллер.
public record CheckDonateResponseDto(bool Matched, decimal? CreditedAmount);

public record UserProfileDto(int Id, string Username, string Email, decimal Balance, string Role);

public record AdminAddBalanceDto(string Username, decimal Amount);
public record CreateGameDto(string Name, string Type, decimal MinBet, decimal MaxBet);
public record CreateGameOutcomeDto(int GameId, decimal Multiplier, decimal Weight);

public record GameOutcomeDto(int Id, string Name, decimal Odds, string? ImageUrl);

// PlayResultDto - добавить WinningOutcomeId
public record PlayResultDto(bool IsWin, decimal Payout, decimal BalanceAfter, int? WinningOutcomeId);
