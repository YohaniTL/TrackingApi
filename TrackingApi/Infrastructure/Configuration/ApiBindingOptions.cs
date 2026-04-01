namespace TrackingApi.Infrastructure.Configuration;

public sealed class ApiBindingOptions
{
    public const string SectionName = "ApiBinding";

    public string Url { get; init; } = string.Empty;
}
