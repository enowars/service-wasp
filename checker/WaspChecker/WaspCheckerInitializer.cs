using EnoCore.Checker;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WaspChecker
{
    public class WaspCheckerInitializer : ICheckerInitializer
    {
        public int FlagsPerRound => 2;
        public int NoisesPerRound => 0;
        public int HavocsPerRound => 0;
        public string ServiceName => "WASP";

        public void Initialize(IServiceCollection collection)
        {
            collection.AddSingleton(typeof(WaspCheckerDb));
            collection.AddSingleton(typeof(HttpClient));
            collection.AddScoped(typeof(WaspClient));
        }
    }
}
