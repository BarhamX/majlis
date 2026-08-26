using Majlis.Application.DailyMajlis;
using Majlis.Domain.DailyMajlis;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.DailyMajlis;

public sealed class EfDailyMajlisRepository(MajlisDbContext dbContext) : IDailyMajlisRepository
{
    public Task<DailyMajlisEntity?> GetPublishedByDateAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DailyMajlis
            .AsNoTracking()
            .Where(dailyMajlis =>
                dailyMajlis.PublishDate == publishDate &&
                dailyMajlis.Status == DailyMajlisStatus.Published)
            .Include(dailyMajlis => dailyMajlis.Challenge)
            .ThenInclude(challenge => challenge.Options.OrderBy(option => option.SortOrder))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
