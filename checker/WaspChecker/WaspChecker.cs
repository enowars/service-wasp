using Bogus;
using EnoCore;
using EnoCore.Checker;
using EnoCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaspChecker.Models;

namespace WaspChecker
{
    public class WaspChecker : IChecker
    {
        private readonly ILogger<WaspChecker> logger;
        private readonly WaspCheckerDb checkerDb;
        private readonly WaspClient waspClient;

        public WaspChecker(ILogger<WaspChecker> logger, WaspClient waspClient, WaspCheckerDb checkerDb)
        {
            this.logger = logger;
            this.checkerDb = checkerDb;
            this.waspClient = waspClient;
        }

        public async Task HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            var tag = ToWaspTag(task);
            DatabaseAttack dbAttack = await checkerDb.GetByTag(tag, token);
            var matches = await waspClient.SearchAttack(task.Address, dbAttack.Tag, token);
            long id = 0;
            foreach (var result in matches)
            {
                if (result.Content?.Content == dbAttack.Description)
                {
                    logger.LogInformation($"Found attack {result}!");
                    id = result.Id;
                    break;
                }
            }
            if (id == 0)
            {
                throw new MumbleException("Missing attack in /api/SearchAttack result");
            }

            Attack attack = await waspClient.GetAttack(task.Address, id, dbAttack.Password, token);
            if (attack.Content?.Content != dbAttack.Description ||
                attack.Location != dbAttack.Location ||
                attack.AttackDate != dbAttack.AttackDate)
            {
                throw new MumbleException("Corrupted /api/GetAttack result");
            }
        }

        public Task HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task HandleHavoc(CheckerTaskMessage task, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            var tag = ToWaspTag(task);
            var storeIndex = task.FlagIndex % 2;
            DatabaseAttack attack = CreateAttack(tag);
            if (storeIndex == 0)
            {
                attack.AttackDate = task.Flag!;
            }
            else
            {
                attack.Location = task.Flag!;
            }
            
            await waspClient.CreateAttack(task.Address, attack, token);
            logger.LogDebug($"Saving DatabaseAttack {attack}");
            await checkerDb.AddAttack(attack, token);
        }

        public Task HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private static DatabaseAttack CreateAttack(string tag)
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] pw = new byte[16];
            rng.GetNonZeroBytes(pw);

            var o = new Faker<DatabaseAttack>("de")
                .RuleFor(a => a.AttackDate, (f, u) => f.Date.Future().ToString()) // TODO random formatters
                .RuleFor(a => a.Description, (f, u) => f.Hacker.Phrase() + $" {tag}")
                .RuleFor(a => a.Location, (f, u) => f.Address.FullAddress())
                .RuleFor(a => a.Password, (f, u) => f.Internet.Password())
                .RuleFor(a => a.Tag, (f, u) => tag);
            var attack = o.Generate();
            return attack;
        }

        private string ToWaspTag(CheckerTaskMessage ctm)
        {
            string rawTag = $"{Enum.GetName(ctm.Method)?.Replace("put", "").Replace("get", "")}_{ctm.RelatedRoundId}_{ctm.TeamId}_{ctm.FlagIndex}";
            logger.LogInformation($"WaspTag = {rawTag}");
            return GetHashString(rawTag);
        }

        public static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
