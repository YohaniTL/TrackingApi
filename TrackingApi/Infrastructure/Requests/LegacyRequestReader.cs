using Microsoft.Extensions.Primitives;

namespace TrackingApi.Infrastructure.Requests;

public sealed class LegacyRequestReader
{
    public async Task<IReadOnlyDictionary<string, string?>> ReadValuesAsync(
        HttpRequest request,
        params string[] keys)
    {
        IFormCollection? form = null;

        if (request.HasFormContentType)
        {
            form = await request.ReadFormAsync(request.HttpContext.RequestAborted);
        }

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            values[key] = ResolveValue(form, request.Query, key);
        }

        return values;
    }

    public static IReadOnlyList<string> GetMissingFields(
        IReadOnlyDictionary<string, string?> values,
        params string[] requiredKeys)
    {
        return requiredKeys
            .Where(key => !values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static string? ResolveValue(IFormCollection? form, IQueryCollection query, string key)
    {
        string? value = null;

        if (form is not null && form.TryGetValue(key, out var formValue))
        {
            value = Normalize(formValue);
        }

        if (string.IsNullOrWhiteSpace(value) && query.TryGetValue(key, out var queryValue))
        {
            value = Normalize(queryValue);
        }

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? Normalize(StringValues values)
    {
        var rawValue = values.ToString();
        return string.IsNullOrWhiteSpace(rawValue) ? null : rawValue.Trim();
    }
}
