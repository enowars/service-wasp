using EnoCore.Checker;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaspChecker
{
    public class WaspCheckerInitializer : ICheckerInitializer
    {
        public string ServiceName => "WASP";

        public void Initialize(IServiceCollection collection)
        {
            collection.AddSingleton(new WaspCheckerDb());
        }
    }
}
