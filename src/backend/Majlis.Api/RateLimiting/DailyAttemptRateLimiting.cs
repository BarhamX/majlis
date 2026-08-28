using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Majlis.Api.RateLimiting;

internal static class DailyAttemptRateLimiting
{
    public static object AccountIdItemKey { get; } = new();

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddDailyAttemptRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    IsAttemptSubmission(context)
                        ? CreateFixedWindowPartition(
                            $"account:{GetAccountId(context):D}",
                            permitLimit: 10)
                        : RateLimitPartition.GetNoLimiter("not-attempt-submission")),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    IsAttemptSubmission(context)
                        ? CreateFixedWindowPartition(
                            $"ip:{GetIpAddress(context)}",
                            permitLimit: 60)
                        : RateLimitPartition.GetNoLimiter("not-attempt-submission")));
            options.OnRejected = WriteRejectedResponseAsync;
        });
        return services;
    }

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        string partitionKey,
        int permitLimit) => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = permitLimit,
            QueueLimit = 0,
            Window = Window,
        });

    private static Guid GetAccountId(HttpContext context) =>
        context.Items.TryGetValue(AccountIdItemKey, out var value) && value is Guid accountId
            ? accountId
            : throw new InvalidOperationException(
                "The completed-profile authorization policy did not resolve an account id.");

    private static string GetIpAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";

    private static bool IsAttemptSubmission(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<DailyAttemptRateLimitedAttribute>() is not null;

    private static async ValueTask WriteRejectedResponseAsync(
        OnRejectedContext context,
        CancellationToken _)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(
                CultureInfo.InvariantCulture);
        }

        await Results.Problem(
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Too many attempt submissions. Try again later.",
            type: "https://httpstatuses.com/429",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "rate_limit_exceeded",
                ["traceId"] = context.HttpContext.TraceIdentifier,
            }).ExecuteAsync(context.HttpContext);
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class DailyAttemptRateLimitedAttribute : Attribute;
