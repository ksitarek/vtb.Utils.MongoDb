using System;
using MongoDB.Bson.Serialization.Attributes;

namespace vtb.Utils.MongoDb
{
    public interface IMongoDbEntity
    {
        Guid Id { get; set; }
        [BsonRequired] public Guid TenantId { get; set; }
    }
}