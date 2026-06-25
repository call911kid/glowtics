using System;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.Commands.Diagnostics;
using Glowtics.BLL.Constants;
using Glowtics.BLL.Exceptions;
using Glowtics.BLL.Interfaces;
using Glowtics.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Glowtics.BLL.Orchestrators
{
    public record AnalyzeOrchestratorRequest(byte[] PhotoBytes, string FileName, string Domain) : IRequest<AnalyzeResponse>;

    public class AnalyzeResponse
    {
        public Guid SessionId { get; set; }
        public string SkinProfileResult { get; set; } = string.Empty;
    }

    public class AnalyzeOrchestrator : IRequestHandler<AnalyzeOrchestratorRequest, AnalyzeResponse>
    {
        private readonly GlowticsDbContext _dbContext;
        private readonly ILangflowService _langflowService;
        private readonly IMediator _mediator;

        public AnalyzeOrchestrator(
            GlowticsDbContext dbContext,
            ILangflowService langflowService,
            IMediator mediator)
        {
            _dbContext = dbContext;
            _langflowService = langflowService;
            _mediator = mediator;
        }

        public async Task<AnalyzeResponse> Handle(AnalyzeOrchestratorRequest request, CancellationToken cancellationToken)
        {
            // 1. Get Retailer by domain
            var retailer = await _dbContext.Retailers
                .FirstOrDefaultAsync(r => r.Domain == request.Domain, cancellationToken)
                ?? throw new EntityNotFoundException(ErrorCodes.RetailerNotFound, $"Retailer with domain '{request.Domain}' was not found.");

            // 2. Call Langflow service
            var diagnosisResult = await _langflowService.DiagnoseAsync(
                request.PhotoBytes, 
                request.FileName, 
                retailer.MongoCollectionName, 
                cancellationToken);

            // 3. Dispatch AddDiagnosticSessionCommand
            var command = new AddDiagnosticSessionCommand(
                retailer.Id,
                diagnosisResult.SkinProfileResult,
                diagnosisResult.ExternalProductIds
            );

            var addSessionResult = await _mediator.Send(command, cancellationToken);

            // 4. Return response
            return new AnalyzeResponse
            {
                SessionId = addSessionResult.SessionId,
                SkinProfileResult = diagnosisResult.SkinProfileResult
            };
        }
    }
}
