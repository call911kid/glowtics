using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Commands.Sessions
{
    public record AddFeedbackCommand(Guid RetailerId, string ExternalUserId, string Feedback) : IRequest<bool>;

    public class AddFeedbackCommandHandler : IRequestHandler<AddFeedbackCommand, bool>
    {
        private readonly GlowticsDbContext _dbContext;

        public AddFeedbackCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AddFeedbackCommand request, CancellationToken cancellationToken)
        {
            var session = await _dbContext.DiagnosticSessions
                .Where(s => s.RetailerId == request.RetailerId && s.ExternalUserId == request.ExternalUserId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException($"No diagnostic session found for external user '{request.ExternalUserId}'.");

            session.Feedback = request.Feedback;
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
