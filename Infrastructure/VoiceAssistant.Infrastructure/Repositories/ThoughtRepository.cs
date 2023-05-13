using AuraSearch.Abstractions;
using AuraSearch.Domain;
using MongoDB.Driver;

namespace VoiceAssistant.Infrastructure.Repositories
{
    public sealed class ThoughtRepository : GenericMongoRepository<Thought>, IThoughtRepository
    {
        public ThoughtRepository(IMongoDatabase database) : base(database)
        {
        }

        public async Task<List<Thought>> GetPreMatchBySymbols(
            IEnumerable<string> words, Guid clientIdentifier, CancellationToken cancellationToken)
        {
            var thoughts = await FindByPredicate(
                x => x.ClientIdentifier == clientIdentifier && x.Ideas.Any(x => x.Symbols.Any(x => words.Contains(x.Word))), cancellationToken: cancellationToken);


            return thoughts.ToList();
        }
    }
}
