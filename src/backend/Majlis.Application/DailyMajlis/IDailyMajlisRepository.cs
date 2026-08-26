using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Application.DailyMajlis;

public interface IDailyMajlisRepository
{
    Task<DailyMajlisEntity?> GetPublishedByDateAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken = default);
}
