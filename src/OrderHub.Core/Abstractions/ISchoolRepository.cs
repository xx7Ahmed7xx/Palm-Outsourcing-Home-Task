namespace OrderHub.Core.Abstractions;

public interface ISchoolRepository
{
    Task<string?> GetTierCodeAsync(int schoolId, CancellationToken cancellationToken = default);
}
