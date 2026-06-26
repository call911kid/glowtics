using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.Responses;
using Glowtics.BLL.Orchestrators;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            var command = new AnalyzeOrchestratorRequest(photoBytes, request.Photo.FileName, request.Domain);
            
            var result = await _mediator.Send(command);

            return Ok(ApiResponse.Success(result));
        }
    }
}
