namespace OrderManagement.Application.Common.Interfaces;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct = default);
}
