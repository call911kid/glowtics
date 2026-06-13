using AutoMapper;
using Glowtics.Api.DTOs.Responses;
using Glowtics.BLL.Responses;

namespace Glowtics.Api.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<LoginResponse, LoginResponseDto>();
        }
    }
}
