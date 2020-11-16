namespace WaspChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore;
    using EnoCore.Checker;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using WaspChecker.Models;

    public class WaspCheckerDb
    {
        private readonly IMongoCollection<DatabaseAttack> attacks;
        private readonly InsertOneOptions insertOneOptions = new InsertOneOptions() { BypassDocumentValidation = false };
        private readonly ILogger<WaspCheckerDb> logger;

        public WaspCheckerDb(ILogger<WaspCheckerDb> logger)
        {
            this.logger = logger;
            var mongo = new MongoClient(MongoConnection);
            var db = mongo.GetDatabase("WaspCheckerDb");
            this.attacks = db.GetCollection<DatabaseAttack>("Attacks");
            this.attacks.Indexes.CreateOne(new CreateIndexModel<DatabaseAttack>(Builders<DatabaseAttack>.IndexKeys
                .Ascending(a => a.Tag)));
        }

        public static string MongoHost => Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";

        public static string MongoPort => Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017";

        public static string MongoUser => Environment.GetEnvironmentVariable("MONGO_USER") ?? string.Empty;

        public static string MongoPw => Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? string.Empty;

        public static string MongoConnection => $"mongodb://{MongoHost}:{MongoPort}";

        public async Task AddAttack(DatabaseAttack a, CancellationToken cancellationToken)
        {
            await this.attacks.InsertOneAsync(a, this.insertOneOptions, cancellationToken);
        }

        public async Task<DatabaseAttack> GetByTag(string tag, CancellationToken token)
        {
            var results = await this.attacks.FindAsync(a => a.Tag == tag, cancellationToken: token);
            try
            {
                var dbAttack = await results.SingleAsync(cancellationToken: token);
                return dbAttack;
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"Could not find old attack: {e.ToFancyString()}");
                throw new MumbleException("Could not find old attack, most likely putflag failed");
            }
        }
    }
}
