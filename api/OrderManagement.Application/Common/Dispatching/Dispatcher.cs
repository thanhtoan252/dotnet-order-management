using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Common.Dispatching;

public class Dispatcher(IServiceProvider sp) : IDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = sp.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, ct);
    }

    public Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = sp.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, ct);
    }
}
