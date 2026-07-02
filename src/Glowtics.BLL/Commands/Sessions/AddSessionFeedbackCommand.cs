using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Exceptions;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Commands.Sessions
{
    /// <summary>Anonymous shopper feedback, tied directly to a diagnostic session by its (unguessable) id.
    /// Unlike AddFeedbackCommand (ApiKey, matched by retailer+externalUser), this is safe to call from the
    /// browser results page — the session id is only ever returned to the shopper who ran the analysis.</summary>
    public record AddSessionFeedbackCommand(Guid SessionId, string Feedback) : IRequest<bool>;

    public class AddSessionFeedbackCommandHandler : IRequestHandler<AddSessionFeedbackCommand, bool>
    {
        private readonly GlowticsDbContext _dbContext;

        public AddSessionFeedbackCommandHandler(GlowticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AddSessionFeedbackCommand request, CancellationToken cancellationToken)
        {
            var session = await _dbContext.DiagnosticSessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new EntityNotFoundException("ERR_SESSION_NOT_FOUND", $"No diagnostic session found with id '{request.SessionId}'.");

            session.Feedback = request.Feedback;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
