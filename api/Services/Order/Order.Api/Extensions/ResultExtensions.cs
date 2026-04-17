using Shared.Core.Domain;

namespace Order.Api.Extensions;

internal static class ResultExtensions
{
    internal static IResult ToProblem(this Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.EndsWith(".NotFound") => StatusCodes.Status404NotFound,
            var c when c.EndsWith(".InvalidState") || c.EndsWith(".InsufficientStock") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(error.Message, statusCode: statusCode);
    }
}
