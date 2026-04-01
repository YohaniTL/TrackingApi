using TrackingApi.Contracts.Legacy;

namespace TrackingApi.Infrastructure.Responses;

public static class LegacyResponses
{
    public static LegacyResponse<TData> Success<TData>(string message, TData data) =>
        new()
        {
            Status = true,
            Message = message,
            Data = data
        };

    public static LegacyResponse<object?> Error(string message) =>
        new()
        {
            Status = false,
            Message = message,
            Data = null
        };
}
