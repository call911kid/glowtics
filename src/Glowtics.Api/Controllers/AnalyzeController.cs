using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.Responses;
using Glowtics.BLL.Commands.Sessions;
using Glowtics.BLL.Orchestrators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Glowtics.Api.Controllers
{
    [ApiController]
    [Route("v1/analyze")]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyzeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Analyze([FromForm] AnalyzeRequestDto request)
        {
            if (request.Photo == null || request.Photo.Length == 0)
            {
                return BadRequest(ApiResponse.Failure("ERR_VALIDATION", "Validation failed", new List<string> { "Photo is required." }));
            }

            if (string.IsNullOrWhiteSpace(request.Domain))
            {
                return BadRequest(ApiResponse.Failure("ERR_VALIDATION", "Validation failed", new List<string> { "Domain is required." }));
            }

            using var memoryStream = new MemoryStream();
            await request.Photo.CopyToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();

            var command = new AdvancedAnalyzeOrchestratorRequest(photoBytes, request.Photo.FileName, request.Photo.ContentType, request.Domain, request.ExternalUserId);
            
            var result = await _mediator.Send(command);

            return Ok(ApiResponse.Success(result));
        }

        [HttpPost("feedback")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> AddFeedback([FromBody] AddFeedbackRequestDto request)
        {
            var retailerId = Guid.Parse(User.FindFirstValue("RetailerId")!);
            var command = new AddFeedbackCommand(retailerId, request.ExternalUserId, request.Feedback);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success("Feedback submitted successfully."));
        }
    }
}
