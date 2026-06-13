using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.DTOs.Responses;
using Glowtics.BLL.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace Glowtics.Api.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AuthController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // 1. Explicitly create the command from the DTO
            var command = new LoginCommand(request.Email, request.Password);
            
            // 2. Dispatch to MediatR
            var result = await _mediator.Send(command);

            // 3. AutoMap the CQRS Response to the API DTO
            var responseDto = _mapper.Map<LoginResponseDto>(result);

            return Ok(responseDto);
        }
    }
}
