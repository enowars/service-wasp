namespace WaspChecker
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Bogus;
    using EnoCore;
    using EnoCore.Checker;
    using EnoCore.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using WaspChecker.Models;

    public class WaspCheckerHandler : IChecker
    {
        private readonly ILogger<WaspCheckerHandler> logger;
        private readonly WaspCheckerDb checkerDb;
        private readonly WaspClient waspClient;

        public WaspCheckerHandler(ILogger<WaspCheckerHandler> logger, WaspClient waspClient, WaspCheckerDb checkerDb)
        {
            this.logger = logger;
            this.checkerDb = checkerDb;
            this.waspClient = waspClient;
        }

        public async Task HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            var tag = this.ToWaspTag(task);
            DatabaseAttack dbAttack = await this.checkerDb.GetByTag(tag, token);
            var matches = await this.waspClient.SearchAttack(task.Address, dbAttack.Tag, token);
            long id = 0;
            foreach (var result in matches)
            {
                if (result.Content?.Content == dbAttack.Description)
                {
                    this.logger.LogInformation($"Found attack {result}!");
                    id = result.Id;
                    break;
                }
            }

            if (id == 0)
            {
                throw new MumbleException("Missing attack in /api/SearchAttack result");
            }

            Attack attack = await this.waspClient.GetAttack(task.Address, id, dbAttack.Password, token);
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

        public async Task HandleHavoc(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.FlagIndex % 2 == 0)
            {
                await this.waspClient.CheckIndexHtml(task.Address, token);
            }
            else if (task.FlagIndex % 2 == 1)
            {
                await this.waspClient.CheckWaspImage(task.Address, token);
            }
        }

        public async Task HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            var tag = this.ToWaspTag(task);
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

            await this.waspClient.CreateAttack(task.Address, attack, token);
            this.logger.LogDebug($"Saving DatabaseAttack {attack}");
            await this.checkerDb.AddAttack(attack, token);
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

        private static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        private string ToWaspTag(CheckerTaskMessage ctm)
        {
            string rawTag = $"{Enum.GetName(ctm.Method)?.Replace("put", string.Empty).Replace("get", string.Empty)}_{ctm.RelatedRoundId}_{ctm.TeamId}_{ctm.FlagIndex}";
            this.logger.LogInformation($"WaspTag = {rawTag}");
            return GetHashString(rawTag);
        }
    }
}
