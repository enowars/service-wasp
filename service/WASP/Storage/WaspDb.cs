using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASP.Models;

namespace WASP.Storage
{
    public interface IWaspDb
    {
        void Migrate();
        Task AddAttack(string date, string location, string description, string password);
        Task<Attack> GetAttack(long id, string password);
        Task<List<Attack>> GetMatchingAttacks(string needle);
    }
    public class WaspDb : IWaspDb
    {
        private readonly ILogger Logger;
        private readonly WaspDbContext _context;

        public WaspDb(WaspDbContext context, ILogger<WaspDb> logger)
        {
            _context = context;
            Logger = logger;
        }

        public void Migrate()
        {
            var pendingMigrations = _context.Database.GetPendingMigrations().Count();
            if (pendingMigrations > 0)
            {
                Logger.LogInformation($"Applying {pendingMigrations} migration(s)");
                _context.Database.Migrate();
                _context.SaveChanges();
                Logger.LogDebug($"Database migration complete");
            }
            else
            {
                Logger.LogDebug($"No pending migrations");
            }
        }

        public async Task<Attack> GetAttack(long id, string password)
        {
            return await _context.Attacks
                .Where(a => a.Id == id && a.Password == password)
                .Include(a => a.Content)
                .SingleOrDefaultAsync();
        }

        public async Task<List<Attack>> GetMatchingAttacks(string needle)
        {
            var query = $@"
SELECT rowid, Content FROM Descriptions
WHERE Descriptions.Content match '{needle}';";
            var ftsMatches = (await _context.Descriptions
                .FromSqlRaw(query)
                .ToListAsync())
                .Select(p => p.rowid);
            return _context.Attacks
                .Where(a => ftsMatches.Contains(a.Contentrowid))
                .Include(a => a.Content)
                .AsNoTracking()
                .ToList();
        }

        public async Task AddAttack(string date, string location, string description, string password)
        {
            _context.Attacks.Add(new Attack()
            {
                AttackDate = date,
                Location = location,
                Content = new AttackDescriptionContent()
                {
                    Content = description
                },
                Password = password
            });
            await _context.SaveChangesAsync();
        }
    }
}
