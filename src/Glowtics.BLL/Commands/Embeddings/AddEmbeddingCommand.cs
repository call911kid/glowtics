using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glowtics.BLL.DTOs;
using Glowtics.BLL.Interfaces;
using MediatR;

namespace Glowtics.BLL.Commands.Embeddings
{
    public record AddEmbeddingCommand(
        string CollectionName,
        string ExternalProductId,
        string Name,
        List<string> TargetConditions,
        List<string> ActiveIngredients,
        List<string> Conflicts
    ) : IRequest<bool>;

    public class AddEmbeddingCommandHandler : IRequestHandler<AddEmbeddingCommand, bool>
    {
        private readonly ILangflowService _langflowService;

        public AddEmbeddingCommandHandler(ILangflowService langflowService)
        {
            _langflowService = langflowService;
        }

        public async Task<bool> Handle(AddEmbeddingCommand request, CancellationToken cancellationToken)
        {
            
            var dto = new LangflowEmbeddingDto
            {
                CollectionName = request.CollectionName,
                ExternalProductId = request.ExternalProductId,
                Name = request.Name,
                TargetConditions = request.TargetConditions,
                ActiveIngredients = request.ActiveIngredients,
                Conflicts = request.Conflicts,
                IsAvailable = true
            };

            await _langflowService.ProcessProductEmbeddingsAsync(dto, cancellationToken);
            
            return true;
        }
    }
}
