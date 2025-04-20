using Microsoft.Extensions.Logging;
using System.Text.Json;
using Sales.Domain.Interfaces;

namespace Sales.Infrastructure.Services;

public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(object domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().Name;
        var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        logger.LogInformation("----- Domain Event Published: {EventType} - Data: {EventData} -----",
            eventType, eventData);

        return Task.CompletedTask;
    }
}