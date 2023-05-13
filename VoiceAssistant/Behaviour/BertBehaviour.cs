using MediatR;
using Microsoft.ML.Models.BERT;
using VoiceAssistant.UseCase.GetAnswer;

namespace VoiceAssistant.Behaviour
{
    public sealed class BertBehaviour<TRequest, TResponse> : IPipelineBehavior<GetAnswerRequest, GetAnswerResponse>
    {
        private readonly BertModel _bertModel;

        public BertBehaviour(BertModel bertModel)
        {
            _bertModel = bertModel;
        }
         
        public async Task<GetAnswerResponse> Handle(GetAnswerRequest request, RequestHandlerDelegate<GetAnswerResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            // Over > 100 takes to much resources to proceed. 
            if (response.Response.Length < 100 && !response.IsLocal)
            {
                var (tokens, _) = _bertModel.Predict(response.Response, request.Input);

                if (tokens != null && tokens.Any())
                {
                    response.Response = string.Join(" ", tokens);
                }
            }

            return response;
        }
    }
}
