using Glowtics.BLL.Constants;
using Glowtics.BLL.Queries.Dashboard;
using Glowtics.BLL.Queries.Retailers;
using Glowtics.BLL.Commands.Retailers;
using Glowtics.BLL.Commands.Sessions;
using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Glowtics.Api.Controllers
{
    [ApiController]
    [Route("v1/retailers/dashboard")]
    [Authorize(Roles = Roles.Retailer)]
    public class RetailerDashboardController : ControllerBase
    {
        private readonly ISender _mediator;

        public RetailerDashboardController(ISender mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private Guid GetRetailerId()
        {
            return Guid.Parse(User.FindFirstValue("RetailerId")!);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] GetTopProductsRequestDto request)
        {
            var query = new GetRetailerTopProductsQuery(GetUserId(), request.Limit);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<GetRetailerTopProductsResponse>.Success(result));
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetPaginatedSessions([FromQuery] PaginationRequestDto request)
        {
            var query = new GetRetailerPaginatedSessionsQuery(GetUserId(), request.PageNumber, request.PageSize);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<GetRetailerPaginatedSessionsResponse>.Success(result));
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetPaginatedProducts([FromQuery] PaginationRequestDto request)
        {
            var query = new GetRetailerPaginatedProductsQuery(GetUserId(), request.PageNumber, request.PageSize);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<GetRetailerPaginatedProductsResponse>.Success(result));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var query = new GetRetailerProfileQuery(GetUserId());
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<GetRetailerProfileResponse>.Success(result));
        }

        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateRetailerProfileRequestDto request)
        {
            var command = new UpdateRetailerProfileCommand(
                GetUserId(), 
                request.Domain, 
                request.CartRedirectUrl, 
                request.BrandLogoUrl);
                
            await _mediator.Send(command);
            
            return Ok(ApiResponse.Success("Profile updated successfully."));
        }

        [HttpPost("sessions/feedback")]
        public async Task<IActionResult> AddFeedback([FromBody] AddFeedbackRequestDto request)
        {
            var command = new AddFeedbackCommand(GetRetailerId(), request.ExternalUserId, request.Feedback);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success("Feedback submitted successfully."));
        }
    }
}
