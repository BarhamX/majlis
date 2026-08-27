using Majlis.Contracts.DailyMajlis;

namespace Majlis.Application.DailyMajlis;

public interface IDailyMajlisService
{
    Task<DailyMajlisResponse?> GetTodayAsync(CancellationToken cancellationToken = default);

    Task<LocalizedDailyMajlisResponse?> GetTodayAsync(
        string? acceptLanguage,
        CancellationToken cancellationToken = default);
}
