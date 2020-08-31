using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace vtb.Utils.MongoDb
{
    public abstract class BaseSeed
    {
        protected Task DoRun<TEntity>(IMongoCollection<TEntity> collection, TEntity[] entities, bool replace = false,
            CancellationToken cancellationToken = default)
            where TEntity : IMongoDbEntity
        {
            var fb = Builders<TEntity>.Filter;
            var ops = new List<WriteModel<TEntity>>();

            foreach (var entity in entities)
                if (replace)
                    ops.Add(new ReplaceOneModel<TEntity>(fb.Eq(x => x.Id, entity.Id), entity));
                else
                    ops.Add(new InsertOneModel<TEntity>(entity));

            return collection.BulkWriteAsync(ops, null, cancellationToken);
        }
    }
}