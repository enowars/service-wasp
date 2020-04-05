using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WASP.Storage
{
    public class WaspDbContextFactory : IDesignTimeDbContextFactory<WaspDbContext>
    {
        public static string CONNECTION_STRING = "Data Source=wwwroot/data/Wasp.db;Foreign Keys=False";
        public WaspDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WaspDbContext>();
            optionsBuilder.UseSqlite(CONNECTION_STRING);
            return new WaspDbContext(optionsBuilder.Options);
        }
    }
}
