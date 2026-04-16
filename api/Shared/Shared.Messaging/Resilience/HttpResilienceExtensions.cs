using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Shared.Messaging.Resilience;

public static class HttpResilienceExtensions
{
    /// <summary>
    ///     Adds a named HttpClient with standard resilience pipeline:
    ///     retry (3 attempts, exponential backoff) + circuit breaker + timeouts.
    /// </summary>
    public static IServiceCollection AddResilientHttpClient(this IServiceCollection services, string name, string baseAddress)
    {
        services
            .AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
            .AddStandardResilienceHandler(options =>
            {
                // Retry: 3 attempts with exponential backoff + jitter
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

                // Circuit breaker: break after 50% failure rate in 10s window, stay open 30s
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.MinimumThroughput = 5;

                // Attempt timeout: 10s per attempt
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

                // Total request timeout: 30s across all retries
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
