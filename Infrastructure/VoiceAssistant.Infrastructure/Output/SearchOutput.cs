using AuraSearch.UseCases.Search.Response;
using AuraSearch.UseCases.Search;
namespace VoiceAssistant.Infrastructure.Output
{
    public sealed class SearchOutput : ISearchOutput
    {
        public string ErrorMessage { get; private set; }

        public Dictionary<Guid, ResultResponse> Results { get; private set; } = new Dictionary<Guid, ResultResponse>();

        public void Error(string message)
        {
            ErrorMessage = message;
        }

        public ResultResponse GetLatestResult(Guid clientIdentifier)
        {
            if (!Results.ContainsKey(clientIdentifier))
            {
                return null;
            }

            var record = Results[clientIdentifier];

            Results.Remove(clientIdentifier);

            return record;
        }

        public void Ok(ResultResponse output)
        {
            if (Results.ContainsKey(output.ClientId))
            {
                Results.Remove(output.ClientId);
            }

            Results.Add(output.ClientId, output);
        }
    }
}
