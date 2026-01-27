namespace OrderManagementService.Presentation.HttpGateway.Middleware;

public static class GrpcErrorHandlingExtensions
{
    public static IApplicationBuilder UseGrpcErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GrpcErrorHandlingMiddleware>();
    }
}