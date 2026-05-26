using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Domain;

namespace Shared.Web.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblem(this Error error)
    {
        return Results.Problem(error.ToProblemDetails());
    }

    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var statusCode = error.Type.ToStatusCode();

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToTitle(),
            Detail = error.Message
        };

        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            problemDetails.Extensions["code"] = error.Code;
        }

        return problemDetails;
    }

    private static int ToStatusCode(this ErrorType type)
    {
        return type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.BadGateway => StatusCodes.Status502BadGateway,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
    }

    private static string ToTitle(this ErrorType type)
    {
        return type switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.BadGateway => "Bad Gateway",
            ErrorType.Validation => "Validation Error",
            _ => "Bad Request"
        };
    }
}
