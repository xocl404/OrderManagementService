using OrderManagementService.Presentation.HttpGateway.Mappings;
using OrderManagementService.Presentation.HttpGateway.Models;
using OrderManagementService.Presentation.HttpGateway.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Products;

namespace OrderManagementService.Presentation.HttpGateway.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductController : ControllerBase
{
    private readonly ProductsService.ProductsServiceClient _client;

    public ProductController(ProductsService.ProductsServiceClient client)
    {
        _client = client;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<long>> Create([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        CreateProductResponse response = await _client.CreateProductAsync(
            new CreateProductRequest { Name = request.Name, Price = (double)request.Price },
            cancellationToken: cancellationToken);

        return Ok(response.ProductId);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> Query(
        [FromQuery] long[]? ids,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? nameSubstring,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var filterDto = new ProductQueryRequestDto(ids, minPrice, maxPrice, nameSubstring, pageSize, pageToken);
        QueryProductsResponse response = await _client.QueryProductsAsync(
            new QueryProductsRequest { Filter = filterDto.ToProto() },
            cancellationToken: cancellationToken);

        ProductDto[] items = response.Result.Items.Select(p => p.ToDto()).ToArray();
        var result = new PagedResultDto<ProductDto>(items, string.IsNullOrWhiteSpace(response.Result.NextPageToken) ? null : response.Result.NextPageToken);
        return Ok(result);
    }
}
