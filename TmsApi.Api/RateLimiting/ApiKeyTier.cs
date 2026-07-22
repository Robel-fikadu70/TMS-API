namespace TmsApi.Api.RateLimiting;

// Define our levels of service
public enum ApiKeyTier
{
    Anonymous,
    Free,
    Paid,
}

public static class ApiKeyResolver
{
    // A simple hardcoded lookup table for the lab
    private static readonly Dictionary<string, ApiKeyTier> Keys = new(StringComparer.Ordinal)
    {
        ["tms-free-demo-001"] = ApiKeyTier.Free,
        ["tms-paid-001"] = ApiKeyTier.Paid,
    };

    public static (string PartitionKey, ApiKeyTier Tier) Resolve(HttpContext ctx)
    {
        // Look for the "X-Api-Key" header
        var key = ctx.Request.Headers["X-Api-Key"].ToString();

        if (string.IsNullOrEmpty(key))
        {
            // No key? Treat them as anonymous using their IP address as the key
            return (
                ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                ApiKeyTier.Anonymous
            );
        }

        // If the key exists in our dictionary, return that tier. Otherwise, they are anonymous.
        return Keys.TryGetValue(key, out var tier) ? (key, tier) : (key, ApiKeyTier.Anonymous);
    }
}
