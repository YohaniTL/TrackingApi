namespace TrackingApi.Infrastructure.Auth;

public sealed record TokenResult(string Token, DateTime ExpiresAtLocal);
