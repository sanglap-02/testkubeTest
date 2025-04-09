using MongoDB.Driver;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.utilities;

namespace SharedLibrary.main.auto.framework.mongo
{
    public class MongoClientMethods
    {
        private static MongoClient? _mongoClient;
        public static IMongoDatabase? Database;
        private readonly ILogging _logging;

        public MongoClientMethods(ILogging logging) => _logging = logging;

        private MongoClient _GetMongoClient(string username, string password, string host, string port)
        {
            _logging.LogInformation("MongoClientMethods - _GetMongoClient");
            try
            {
                var uri = $"mongodb://{username}:{password}@{host}:{port}";
                var dbClient = new MongoClient(uri);
                return dbClient;
            }
            catch (Exception e)
            {
                throw new Exception("Error Setting Mongo Client: " + e.Message);
            }
        }

        private MongoClient GetMongoClient(Dictionary<string, string> environmentDetails)
        {
            _logging.LogInformation("MongoClientMethods - getMongoClient");
            var decode = new EncodingDecoding(new Logging());
            
            var mongoHost = environmentDetails["mongoServer"];
            var mongoUsername = decode.GetDecodedStringValue(environmentDetails["mongoUsername"]);
            var mongoPassword = decode.GetDecodedStringValue(environmentDetails["mongoPassword"]);
            var mongoPort = decode.GetDecodedStringValue(environmentDetails["mongoport"]);

            return _GetMongoClient(mongoUsername, mongoPassword, mongoHost, mongoPort);
        }

        public IMongoDatabase? GetMongoDatabase(Dictionary<string, string> environmentDetails, string databaseName)
        {
           _logging.LogInformation("MongoClientMethods - GetMongoDatabase");
            
            try
            {
                _mongoClient = GetMongoClient(environmentDetails);

                if (_mongoClient != null)
                {
                    return _mongoClient.GetDatabase(databaseName);
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("GetMongoDatabase - Error connecting to MongoDB", ex);
            }
            return null;
        }
    }
}
