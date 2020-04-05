using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASP.Models;

namespace WASP.Storage
{
    public class WaspDbContext : DbContext
    {
        public DbSet<Attack> Attacks { get; set; }
        public DbSet<AttackDescriptionContent> Descriptions { get; set; }

        public WaspDbContext(DbContextOptions<WaspDbContext> options) : base(options) { }
    }
}
