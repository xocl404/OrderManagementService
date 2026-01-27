using Grpc.Core;
using System.Net;

namespace OrderManagementService.Presentation.HttpGateway.Middleware;

public sealed class GrpcErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GrpcErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException ex)
        {
            context.Response.StatusCode = MapStatusCode(ex.StatusCode);
            await context.Response.WriteAsync(ex.Status.Detail);
        }
    }

    private static int MapStatusCode(StatusCode statusCode)
    {
        return statusCode switch
        {
            StatusCode.OK => (int)HttpStatusCode.OK,
            StatusCode.Cancelled => (int)HttpStatusCode.BadRequest,
            StatusCode.Unknown => (int)HttpStatusCode.InternalServerError,
            StatusCode.InvalidArgument => (int)HttpStatusCode.BadRequest,
            StatusCode.DeadlineExceeded => (int)HttpStatusCode.RequestTimeout,
            StatusCode.NotFound => (int)HttpStatusCode.NotFound,
            StatusCode.AlreadyExists => (int)HttpStatusCode.Conflict,
            StatusCode.PermissionDenied => (int)HttpStatusCode.Forbidden,
            StatusCode.Unauthenticated => (int)HttpStatusCode.Unauthorized,
            StatusCode.ResourceExhausted => (int)HttpStatusCode.TooManyRequests,
            StatusCode.FailedPrecondition => (int)HttpStatusCode.PreconditionFailed,
            StatusCode.Aborted => (int)HttpStatusCode.Conflict,
            StatusCode.OutOfRange => (int)HttpStatusCode.BadRequest,
            StatusCode.Unimplemented => (int)HttpStatusCode.NotImplemented,
            StatusCode.Internal => (int)HttpStatusCode.InternalServerError,
            StatusCode.Unavailable => (int)HttpStatusCode.ServiceUnavailable,
            StatusCode.DataLoss => (int)HttpStatusCode.InternalServerError,
            _ => throw new NotImplementedException(),
        };
    }
}