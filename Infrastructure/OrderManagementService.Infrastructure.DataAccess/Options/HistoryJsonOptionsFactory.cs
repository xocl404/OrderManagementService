using OrderManagementService.Infrastructure.DataAccess.Serializers;
using System.Text.Json;

namespace OrderManagementService.Infrastructure.DataAccess.Options;

public static class HistoryJsonOptionsFactory
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        options.Converters.Insert(0, new OrderHistoryPayloadConverter());
        return options;
    }
}