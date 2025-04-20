namespace Sales.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(object domainEvent, CancellationToken cancellationToken = default);
}