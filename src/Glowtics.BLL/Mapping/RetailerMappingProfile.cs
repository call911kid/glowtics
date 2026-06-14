using AutoMapper;
using Glowtics.DAL.Entities;
using Glowtics.BLL.Commands.Retailers;
using Glowtics.BLL.Responses;

namespace Glowtics.BLL.Mapping
{
    public class RetailerMappingProfile : Profile
    {
        public RetailerMappingProfile()
        {
            CreateMap<Retailer, CreateRetailerProfileResponse>();
        }
    }
}
