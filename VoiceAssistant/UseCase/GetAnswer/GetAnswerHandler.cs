using AuraSearch.Abstractions;
using AuraSearch.UseCases.Index.Request;
using AuraSearch.UseCases.Search;
using AuraSearch.UseCases.Search.Request;
using MediatR;
using OpenAI_API;
using System.Text;
using VoiceAssistant.Models;

namespace VoiceAssistant.UseCase.GetAnswer
{
    public sealed class GetAnswerHandler : IRequestHandler<GetAnswerRequest, GetAnswerResponse>
    {
        private readonly Settings _settings;

        // Store and index from search library
        private readonly IUseCase<StoreIndexRequest> _storeUseCase;
        private readonly IUseCase<SearchRequest> _searchUseCase;
        private readonly IContextAccessor _contextAccessor;
        private readonly ISearchOutput _searchOutput;

        public GetAnswerHandler(
            Settings settings,
            IUseCase<StoreIndexRequest> storeUseCase,
            IUseCase<SearchRequest> searchUseCase,
            IContextAccessor contextAccessor,
            ISearchOutput searchOutput)
        {
            _settings = settings;
            _storeUseCase = storeUseCase;
            _searchUseCase = searchUseCase;
            _contextAccessor = contextAccessor;
            _searchOutput = searchOutput;
        }

        public async Task<GetAnswerResponse> Handle(GetAnswerRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Input))
            {
                return new GetAnswerResponse();
            }

            var localAnswer = await FindAnswerAsync(request.Input, request.Client, cancellationToken);

            if (!string.IsNullOrEmpty(localAnswer))
            {
                return new GetAnswerResponse { Response = localAnswer, IsLocal = true }; 
            }

            var botAnswer = await GetAnswerFromBotAsync(request.Input, cancellationToken);

            await StoreBotAnswerAsync(request.Input, botAnswer, request.Client, cancellationToken);

            return new GetAnswerResponse { Response = botAnswer }; 
        }

        /// <summary>
        /// Find answer in local index
        /// </summary>
        /// <param name="question"></param>
        /// <param name="clientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> FindAnswerAsync(string question, Guid clientId, CancellationToken cancellationToken)
        {
            var context = await _contextAccessor.GetContextAsync(clientId, cancellationToken);

            var searchRequest = new SearchRequest
            {
                ClientIdentifier = clientId,
                Request = question,
                Thresshold = _settings.SearchThresshold
            };

            await _searchUseCase.Execute(searchRequest, context, cancellationToken);

            var allResults = _searchOutput.GetLatestResult(clientId);

            if (allResults == null || !allResults.Result.Succeeded)
            {
                return string.Empty;
            }

            return allResults.Result.Results.OrderByDescending(x => x.Score).First().TopIdea.Overview;
        }

        private async Task<string> GetAnswerFromBotAsync(string question, CancellationToken cancellationToken)
        {
            var api = new OpenAIAPI(_settings.ChatGptToken);

            var chat = api.Chat.CreateConversation();

            chat.AppendUserInput(question);

            var resultBuilder = new StringBuilder();

            await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
            {
                resultBuilder.Append(res);
            }

            return resultBuilder.ToString();
        }

        private async Task StoreBotAnswerAsync(string question, string answer, Guid clientId, CancellationToken cancellationToken)
        {
            var context = await _contextAccessor.GetContextAsync(clientId, cancellationToken);

            var storeRequest = new StoreIndexRequest
            {
                ClientIdentifier = clientId,
                Payloads = new List<Payload> {
                                                 new Payload
                                                 {
                                                      Document = answer,
                                                      Title = question
                                                 }
                                             }
            };

            await _storeUseCase.Execute(storeRequest, context, cancellationToken);
        }
    }
}
