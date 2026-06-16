using Glowtics.Api.DTOs.Requests;
using Glowtics.Api.DTOs.Responses;
using Glowtics.Api.Responses;
using Glowtics.BLL.Commands.Auth;
using Glowtics.BLL.Commands.Identity;
using Glowtics.BLL.Commands.Retailers;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Orchestrators;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using System;

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
            var command = new LoginCommand(request.Email, request.Password);
            
            var result = await _mediator.Send(command);

            var responseDto = _mapper.Map<LoginResponseDto>(result);

            return Ok(ApiResponse.Success(responseDto));
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterRetailer([FromBody] RegisterRetailerRequestDto request)
        {
            var command = new RegisterRetailerOrchestratorRequest(request.Email, request.Password, request.Domain);
            var result = await _mediator.Send(command);

            var responseDto = _mapper.Map<RegisterRetailerResponseDto>(result);

            return Ok(ApiResponse.Success(responseDto));
        }

        [Authorize(Roles = Roles.Retailer)]
        [HttpPost("rotate-key")]
        public async Task<IActionResult> RotateCatalogKey()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new RotateCatalogApiKeyCommand(userId);
            var result = await _mediator.Send(command);

            var responseDto = _mapper.Map<RotateCatalogApiKeyResponseDto>(result);

            return Ok(ApiResponse.Success(responseDto));
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto request)
        {
            var command = new ConfirmEmailCommand(request.Email, request.Otp);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success("Your email has been successfully confirmed. You may now log in."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var command = new SendPasswordResetEmailCommand(request.Email);
            await _mediator.Send(command);

            // always return success to prevent email enumeration
            return Ok(ApiResponse.Success());
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var command = new ResetPasswordCommand(request.Email, request.Otp, request.NewPassword);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            await _mediator.Send(command);

            return Ok(ApiResponse.Success());
        }
    }
}
