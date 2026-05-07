using Shared.Core.Domain;

namespace Identity.Api.Extensions;

internal static class ResultExtensions
{
    internal static IResult ToProblem(this Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.EndsWith(".NotFound") => StatusCodes.Status404NotFound,
            var c when c.EndsWith(".Conflict") => StatusCodes.Status409Conflict,
            var c when c.EndsWith(".InvalidState") => StatusCodes.Status409Conflict,
            var c when c.EndsWith(".UpstreamFailure") => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(error.Message, statusCode: statusCode);
    }
}
