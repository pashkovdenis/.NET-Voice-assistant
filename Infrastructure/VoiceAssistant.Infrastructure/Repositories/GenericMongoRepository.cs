using AuraSearch.Abstractions;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace VoiceAssistant.Infrastructure.Repositories
{
    /// <summary>
    /// Aura generic repository 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GenericMongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> _collection;

        public GenericMongoRepository(IMongoDatabase database) => _collection = database.GetCollection<T>(typeof(T).Name);

        public async Task<T> GetById(string id, CancellationToken cancellationToken = default) =>
            (await _collection.FindAsync(e => e.Id == Guid.Parse(id), cancellationToken: cancellationToken)).FirstOrDefault();

        public async Task RemoveById(string id, CancellationToken cancellationToken = default)
            => await _collection.DeleteOneAsync(Builders<T>.Filter.Eq(e => e.Id.ToString(), id));

        public async Task Update(T entity, CancellationToken cancellationToken = default)
            => await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq(e => e.Id, entity.Id), entity, new ReplaceOptions { IsUpsert = true });

        public async Task Insert(T entity, CancellationToken cancellationToken = default) => await _collection.InsertOneAsync(entity);

        public Task<IQueryable<T>> GetAll(CancellationToken cancellationToken = default) => Task.FromResult(_collection.AsQueryable().AsQueryable());

        public async Task<IEnumerable<T>> FindByPredicate(Expression<Func<T, bool>> expression, int skip = 0, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Where(expression);

            var data = await _collection.FindAsync(filter,
                new FindOptions<T, T>
                {
                    Skip = skip,
                    Sort = Builders<T>.Sort.Descending("Created")
                });

            return await data.ToListAsync();
        }

        public async Task<T> GetOneByPredicateAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Where(expression);
            var data = await _collection.FindAsync(filter);
            return (await data.ToListAsync()).FirstOrDefault();
        }
    }
}
