using MongoDB.Bson;
using MongoDB.Driver;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;

namespace SharedLibrary.main.auto.framework.mongo
{
    public class MongoDataMethods
    {
        private readonly ILogging _logging;

        public MongoDataMethods(ILogging logging) => _logging = logging;
     
        public List<Dictionary<string, string>> GetAllDataFromMongo(Dictionary<string, string> config, string databaseName, string collectionName)
        {
           _logging.LogInformation("ProcessIngoMongoMethods - GetDataFromMongo");

           var mongoClientMethods = new MongoClientMethods(new Logging());
           
            try
            {
                var mongoData = new List<Dictionary<string, string>>();

                var database = mongoClientMethods.GetMongoDatabase(config, databaseName);

                var documents = database?.GetCollection<BsonDocument>(collectionName)?.Find(new BsonDocument())
                    .ToList();

                if (documents == null) return [];
                
                foreach (var document in documents)
                {
                    var storeDictionary = new Dictionary<string, string>();
                    foreach (var element in document.Elements)
                    {
                        storeDictionary[element.Name] = element.Value.ToString() ?? "";
                    }
                    mongoData.Add(storeDictionary);
                }
                return mongoData;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error", ex);
                throw new Exception(ex.ToString());
            }
        }
    }
}
