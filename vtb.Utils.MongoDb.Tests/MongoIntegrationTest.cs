using Mongo2Go;
using MongoDB.Driver;

namespace vtb.Utils.MongoDb.Tests
{
    public class MongoIntegrationTest
    {
        protected IMongoDatabase _database;
        protected MongoDbRunner _runner;

        protected void CreateConnection()
        {
            _runner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_runner.ConnectionString);
            _database = mongoClient.GetDatabase("IntegrationTest");
        }

        public void DestroyConnection()
        {
            _runner?.Dispose();
        }
    }
}