using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaspChecker.Models;
using EnoCore.Checker;
using Microsoft.Extensions.Logging;
using EnoCore;

namespace WaspChecker
{
    public class WaspCheckerDb
    {
        public static string MongoHost => Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
        public static string MongoPort => Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017";
        public static string MongoUser => Environment.GetEnvironmentVariable("MONGO_USER") ?? "";
        public static string MongoPw => Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? "";
        public static string MongoConnection => $"mongodb://{MongoHost}:{MongoPort}";
        private readonly IMongoCollection<DatabaseAttack> Attacks;
        private readonly InsertOneOptions InsertOneOptions = new InsertOneOptions() { BypassDocumentValidation = false };
        private readonly ILogger<WaspCheckerDb> logger;

        public WaspCheckerDb(ILogger<WaspCheckerDb> logger)
        {
            this.logger = logger;
            var mongo = new MongoClient(MongoConnection);
            var db = mongo.GetDatabase("WaspCheckerDb");
            Attacks = db.GetCollection<DatabaseAttack>("Attacks");
            Attacks.Indexes.CreateOne(new CreateIndexModel<DatabaseAttack>(Builders<DatabaseAttack>.IndexKeys
                .Ascending(a => a.Tag)));
        }

        public async Task AddAttack(DatabaseAttack a, CancellationToken cancellationToken)
        {
            await Attacks.InsertOneAsync(a, InsertOneOptions, cancellationToken);
        }

        public async Task<DatabaseAttack> GetByTag(string tag, CancellationToken token)
        {
            var results = await Attacks.FindAsync(a => a.Tag == tag, cancellationToken: token);
            try
            {
                var dbAttack = await results.SingleAsync(cancellationToken: token);
                return dbAttack;
            }
            catch (Exception e)
            {
                logger.LogWarning($"Could not find old attack: {e.ToFancyString()}");
                throw new MumbleException("Could not find old attack, most likely putflag failed");
            }
        }
    }
}
