using AuraSearch.Abstractions;
using AuraSearch.Domain;
using MongoDB.Driver;

namespace VoiceAssistant.Infrastructure.Repositories
{
    public sealed class WordRepository : GenericMongoRepository<Word>, IWordRepository
    {
        public WordRepository(IMongoDatabase database) : base(database)
        { 

        }

        public async Task<IQueryable<Word>> GetAllWords(bool includeNoise = false, CancellationToken cancellationToken = default)
        {
            var records = await FindByPredicate(x => x.IsNoiseSymbol == includeNoise, cancellationToken: cancellationToken);

            return records.AsQueryable();
        }
    }
}
