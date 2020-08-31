using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace vtb.Utils.MongoDb
{
    public abstract class MongoDbBaseRepository<TEntity> : IMongoDbBaseRepository<TEntity>
        where TEntity : IMongoDbEntity
    {
        private readonly string _collectionName;
        private readonly IMongoDatabase _mongoDb;
        private readonly ITenantIdProvider _tenantIdProvider;

        protected FilterDefinitionBuilder<TEntity> FilterBuilder = Builders<TEntity>.Filter;
        protected SortDefinitionBuilder<TEntity> SortBuilder = Builders<TEntity>.Sort;

        protected MongoDbBaseRepository(IMongoDatabase mongoDb, ITenantIdProvider tenantIdProvider,
            string collectionName)
        {
            _mongoDb = mongoDb;
            _tenantIdProvider = tenantIdProvider;
            _collectionName = collectionName;
        }

        private IMongoCollection<TEntity> _collection => GetCollection();

        public Task AddOneAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.TenantId = _tenantIdProvider.TenantId;
            return _collection.InsertOneAsync(entity, default, cancellationToken);
        }

        public Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Check.GuidNotEmpty(id, nameof(id));
            return Query(FilterBuilder.Eq(x => x.Id, id)).FirstOrDefaultAsync(cancellationToken);
        }

        public Task BatchAddAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
            Array.ForEach(entities, e => e.TenantId = _tenantIdProvider.TenantId);
            return _collection.InsertManyAsync(entities, default, cancellationToken);
        }

        public Task ReplaceAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.TenantId = _tenantIdProvider.TenantId;

            var filter = FilterBuilder.Eq(e => e.Id, entity.Id) &
                         FilterBuilder.Eq(x => x.TenantId, _tenantIdProvider.TenantId);
            return _collection.FindOneAndReplaceAsync(filter, entity, null, cancellationToken);
        }

        public Task BatchReplaceAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
            var updates = new List<WriteModel<TEntity>>();
            foreach (var entity in entities)
            {
                entity.TenantId = _tenantIdProvider.TenantId;

                var filter = FilterBuilder.Eq(x => x.Id, entity.Id) &
                             FilterBuilder.Eq(x => x.TenantId, _tenantIdProvider.TenantId);
                updates.Add(new ReplaceOneModel<TEntity>(filter, entity) { IsUpsert = true });
            }

            return _collection.BulkWriteAsync(updates, null, cancellationToken);
        }

        public Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = FilterBuilder.Eq(e => e.Id, id) &
                         FilterBuilder.Eq(e => e.TenantId, _tenantIdProvider.TenantId);
            return _collection.DeleteOneAsync(filter, cancellationToken);
        }

        protected IFindFluent<TEntity, TEntity> Query(FilterDefinition<TEntity> filter)
        {
            return _collection.Find(FilterBuilder.Eq(x => x.TenantId, _tenantIdProvider.TenantId) & filter);
        }

        private IMongoCollection<TEntity> GetCollection()
        {
            if (string.IsNullOrEmpty(_collectionName))
                throw new InvalidOperationException("Collection name was not set.");

            return _mongoDb.GetCollection<TEntity>(_collectionName);
        }

        protected FilterDefinition<TEntity> ApplyDateRangeFilter(DateTime? dateFrom, DateTime? dateTo,
            Expression<Func<TEntity, DateTime?>> field)
        {
            var filterDefinition = FilterBuilder.Empty;

            if (dateFrom != null || dateTo != null)
            {
                filterDefinition &= FilterBuilder.Ne(field, null);

                if (dateFrom.HasValue) filterDefinition &= FilterBuilder.Gte(field, dateFrom);
                if (dateTo.HasValue) filterDefinition &= FilterBuilder.Lte(field, dateTo);
            }

            return filterDefinition;
        }

        protected SortDefinition<TEntity> BuildSortDefinition(string sortBy, MongoDbSortDirection sortDirection)
        {
            var mongoSort = new BsonDocument();
            mongoSort.Add(sortBy, sortDirection);

            return mongoSort;
        }
    }
}