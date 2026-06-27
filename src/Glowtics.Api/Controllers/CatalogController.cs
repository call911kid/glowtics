using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.Responses;
using Glowtics.BLL.Commands.Products;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Orchestrators;
using Glowtics.BLL.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Glowtics.Api.Controllers
{
    [ApiController]
    [Route("v1/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CatalogController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetRetailerId()
        {
            return Guid.Parse(User.FindFirstValue("RetailerId")!);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("products")]
        [Authorize(Roles = Roles.Retailer)]
        public async Task<IActionResult> GetProducts([FromQuery] PaginationRequestDto request)
        {
            var query = new GetRetailerPaginatedProductsQuery(GetUserId(), request.PageNumber, request.PageSize);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse.Success(result));
        }

        [HttpPost("products")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> AddProduct([FromBody] AddProductRequestDto request)
        {
            var command = new AddProductOrchestratorRequest(
                GetRetailerId(),
                request.ExternalProductId,
                request.Name,
                request.TargetConditions,
                request.ActiveIngredients,
                request.Conflicts,
                request.ImageUrls
            );

            var result = await _mediator.Send(command);

            return Ok(ApiResponse.Success(result));
        }

        [HttpDelete("products/{productId}")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            var command = new DeleteProductOrchestratorRequest(GetRetailerId(), productId);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success("Product deleted successfully."));
        }

        [HttpPatch("products/{productId}/availability")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> UpdateAvailability(string productId, [FromBody] UpdateProductAvailabilityRequestDto request)
        {
            var command = new UpdateProductAvailabilityOrchestratorRequest(GetRetailerId(), productId, request.IsAvailable);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success("Product availability updated successfully."));
        }
    }
}
