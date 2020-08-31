using System;
using System.Threading;
using System.Threading.Tasks;

namespace vtb.Utils.MongoDb
{
    public interface IMongoDbBaseRepository<T>
    {
        Task AddOneAsync(T entity, CancellationToken cancellationToken = default);

        Task BatchAddAsync(T[] entities, CancellationToken cancellationToken = default);

        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task ReplaceAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default);

        Task BatchReplaceAsync(T[] entities, CancellationToken cancellationToken = default);
    }
}