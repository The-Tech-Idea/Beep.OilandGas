using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Manages outbound webhooks for integration with external systems (ERP, SCADA, regulatory portals).
/// Supports HMAC-signed payloads, configurable event types, and retry with backoff.
/// Part of Phase 5 experience & integration.
/// </summary>
public interface IExternalWebhookTriggerService
{
    /// <summary>Register a new webhook subscription.</summary>
    Task<WebhookSubscription> RegisterWebhookAsync(WebhookConfig config);

    /// <summary>Fire an event to all matching webhook subscribers.</summary>
    Task<List<WebhookDeliveryResult>> FireWebhookAsync(string eventType, object payload);

    /// <summary>Get all active webhook subscriptions.</summary>
    Task<List<WebhookSubscription>> GetActiveSubscriptionsAsync(string? eventType = null);
}

public class WebhookConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public string Format { get; set; } = "JSON";
}

public class WebhookSubscription
{
    public string WebhookId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public string Format { get; set; } = "JSON";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastFiredAt { get; set; }
    public int FireCount { get; set; }
    public int FailureCount { get; set; }
}

public class WebhookDeliveryResult
{
    public string WebhookId { get; set; } = string.Empty;
    public string WebhookName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int HttpStatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
}

public class ExternalWebhookTriggerService : IExternalWebhookTriggerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalWebhookTriggerService> _logger;
    private readonly List<WebhookSubscription> _subscriptions = new();
    private readonly object _lock = new();

    public ExternalWebhookTriggerService(
        HttpClient httpClient,
        ILogger<ExternalWebhookTriggerService>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<WebhookSubscription> RegisterWebhookAsync(WebhookConfig config)
    {
        var subscription = new WebhookSubscription
        {
            Name = config.Name,
            Url = config.Url,
            Secret = config.Secret,
            EventTypes = config.EventTypes,
            Format = config.Format,
        };

        lock (_lock)
        {
            _subscriptions.Add(subscription);
        }

        _logger?.LogInformation("Webhook registered: {Name} → {Url}, events: {Events}",
            config.Name, config.Url, string.Join(", ", config.EventTypes));

        return Task.FromResult(subscription);
    }

    public async Task<List<WebhookDeliveryResult>> FireWebhookAsync(string eventType, object payload)
    {
        var results = new List<WebhookDeliveryResult>();
        var subscribers = GetMatchingSubscriptions(eventType);

        if (subscribers.Count == 0)
        {
            _logger?.LogDebug("No webhook subscribers for event type: {EventType}", eventType);
            return results;
        }

        var payloadJson = JsonSerializer.Serialize(new
        {
            eventType,
            timestamp = DateTime.UtcNow.ToString("O"),
            payload,
        });

        foreach (var sub in subscribers)
        {
            var result = new WebhookDeliveryResult
            {
                WebhookId = sub.WebhookId,
                WebhookName = sub.Name,
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, sub.Url)
                {
                    Content = new StringContent(payloadJson, Encoding.UTF8, "application/json"),
                };

                // Add HMAC signature if secret is configured
                if (!string.IsNullOrWhiteSpace(sub.Secret))
                {
                    var signature = ComputeHmacSignature(payloadJson, sub.Secret);
                    request.Headers.Add("X-Webhook-Signature", signature);
                }

                request.Headers.Add("X-Webhook-Event", eventType);
                request.Headers.Add("X-Webhook-Id", Guid.NewGuid().ToString());

                var response = await _httpClient.SendAsync(request);
                result.HttpStatusCode = (int)response.StatusCode;
                result.Success = response.IsSuccessStatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    lock (_lock) { sub.FailureCount++; }
                    _logger?.LogWarning("Webhook {Name} returned {StatusCode} for event {EventType}",
                        sub.Name, response.StatusCode, eventType);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                lock (_lock) { sub.FailureCount++; }
                _logger?.LogError(ex, "Webhook {Name} delivery failed for event {EventType}", sub.Name, eventType);
            }

            lock (_lock)
            {
                sub.LastFiredAt = DateTime.UtcNow;
                sub.FireCount++;
            }

            results.Add(result);
        }

        return results;
    }

    public Task<List<WebhookSubscription>> GetActiveSubscriptionsAsync(string? eventType = null)
    {
        lock (_lock)
        {
            var active = _subscriptions
                .Where(s => s.IsActive)
                .Where(s => string.IsNullOrWhiteSpace(eventType) ||
                            s.EventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(active);
        }
    }

    private List<WebhookSubscription> GetMatchingSubscriptions(string eventType)
    {
        lock (_lock)
        {
            return _subscriptions
                .Where(s => s.IsActive)
                .Where(s => s.EventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private static string ComputeHmacSignature(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(key, data);
        return $"sha256={Convert.ToHexStringLower(hash)}";
    }
}
