namespace WaspChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using EnoCore.Checker;
    using Microsoft.Extensions.DependencyInjection;

    public class WaspCheckerInitializer : ICheckerInitializer
    {
        public int FlagsPerRound => 2;

        public int NoisesPerRound => 0;

        public int HavocsPerRound => 2;

        public string ServiceName => "WASP";

        public void Initialize(IServiceCollection collection)
        {
            collection.AddSingleton(typeof(WaspCheckerDb));
            collection.AddHttpClient<WaspClient>()
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    if (builder.PrimaryHandler is HttpClientHandler handler)
                    {
                        // https://github.com/dotnet/extensions/issues/872
                        handler.UseCookies = false;
                        handler.AllowAutoRedirect = false;
                    }
                });
            collection.AddScoped(typeof(WaspClient));
        }
    }
}
