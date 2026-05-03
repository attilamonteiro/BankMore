namespace BankMore.Shared.Idempotency;

public interface IIdempotencyService
{
    Task<(bool IsDuplicate, string? CachedResult)> CheckAndClaimAsync(string key, string requestJson);
    Task SaveResultAsync(string key, string resultJson);
}
