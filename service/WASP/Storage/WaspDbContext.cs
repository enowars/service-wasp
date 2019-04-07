using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASP.Models;

namespace WASP.Storage
{
    public class WaspDbContext : DbContext
    {
        private static readonly object DBLock = new object();
        public DbSet<Attack> Attacks { get; set; }
        public DbSet<AttackDescriptionContent> Descriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Filename=wwwroot/data/Wasp.db", x => x.SuppressForeignKeyEnforcement());
        }

        public static void Migrate()
        {
            lock (DBLock)
            {
                using (var ctx = new WaspDbContext())
                {
                    if (ctx.Database.GetPendingMigrations().Count() > 0)
                    {
                        ctx.Database.Migrate();
                    }
                }
            }
        }

        public static Attack GetAttack(long id, string password)
        {
            lock (DBLock)
            {
                using (var ctx = new WaspDbContext())
                {
                    return ctx.Attacks
                        .Where(a => a.Id == id && a.Password == password)
                        .Include(a => a.Content)
                        .SingleOrDefault();
                }
            }
        }

        public static List<Attack> GetMatchingAttacks(string needle)
        {
            lock (DBLock)
            {
                using (var ctx = new WaspDbContext())
                {
                    var query = $@"
SELECT rowid, Content FROM Descriptions
WHERE Descriptions.Content match '{needle}';";
                    var ftsMatches = ctx.Descriptions
                        .FromSql(query)
                        .ToList()
                        .Select(p => p.rowid);
                    return ctx.Attacks
                        .Where(a => ftsMatches.Contains(a.Contentrowid))
                        .Include(a => a.Content)
                        .AsNoTracking()
                        .ToList();
                }
            }
        }

        public static void AddAttack(string date, string location, string description, string password)
        {
            lock (DBLock)
            {
                using (var ctx = new WaspDbContext())
                {
                    ctx.Attacks.Add(new Attack()
                    {
                        AttackDate = date,
                        Location = location,
                        Content = new AttackDescriptionContent()
                        {
                            Content = description
                        },
                        Password = password
                    });
                    ctx.SaveChanges();
                }
            }
        }
    }
}
