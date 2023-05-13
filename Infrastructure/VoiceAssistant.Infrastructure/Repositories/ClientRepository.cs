using AuraSearch.Abstractions;
using AuraSearch.Domain;
using MongoDB.Driver;

namespace VoiceAssistant.Infrastructure.Repositories
{
    public sealed class ClientRepository : GenericMongoRepository<Client>, IContextRepository
    {
        public ClientRepository(IMongoDatabase database) : base(database)
        {
        }
    }
}
