using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using System.Threading.Tasks;
using Glowtics.DAL.Context;
using Glowtics.DAL.Entities;
using Glowtics.DAL.Enums;
using MediatR;
using Glowtics.BLL.Responses;

namespace Glowtics.BLL.Commands.Retailers
{
    public record CreateRetailerProfileCommand(Guid UserId, string Domain, string MongoCollectionName) : IRequest<CreateRetailerProfileResponse>;

    public class CreateRetailerProfileCommandHandler : IRequestHandler<CreateRetailerProfileCommand, CreateRetailerProfileResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly IMapper _mapper;

        public CreateRetailerProfileCommandHandler(GlowticsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<CreateRetailerProfileResponse> Handle(CreateRetailerProfileCommand request, CancellationToken cancellationToken)
        {
            var retailer = new Retailer
            {
                UserId = request.UserId,
                Domain = request.Domain,
                MongoCollectionName=request.MongoCollectionName,
                Status = RetailerStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Retailers.Add(retailer);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CreateRetailerProfileResponse>(retailer);
        }
    }
}
