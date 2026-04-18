using Shared.Core.Domain;

namespace Inventory.Api.Extensions;

internal static class ResultExtensions
{
    internal static IResult ToProblem(this Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.EndsWith(".NotFound") => StatusCodes.Status404NotFound,
            var c when c.EndsWith(".AlreadyExists") => StatusCodes.Status409Conflict,
            var c when c.EndsWith(".InvalidState") || c.EndsWith(".InsufficientStock") || c.EndsWith(".InsufficientReserved") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(error.Message, statusCode: statusCode);
    }
}
