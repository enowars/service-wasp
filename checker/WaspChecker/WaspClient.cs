namespace WaspChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore;
    using EnoCore.Checker;
    using Microsoft.Extensions.Logging;
    using WaspChecker.Models;

    public class WaspClient
    {
        private const int Port = 8000;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public WaspClient(ILogger<WaspClient> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        public async Task CreateAttack(string address, DatabaseAttack attack, CancellationToken token)
        {
            var url = $"http://{address}:{Port}/api/AddAttack";
            this.logger.LogDebug($"CreateAttack {attack}");
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("date", attack.AttackDate),
                    new KeyValuePair<string?, string?>("location", attack.Location),
                    new KeyValuePair<string?, string?>("description", attack.Description),
                    new KeyValuePair<string?, string?>("password", attack.Password),
                }),
            };
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"CreateAttack failed: ({e.ToFancyString()}");
                throw new OfflineException("Registration failed");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("/api/AddAttack returned unsuccessful status code");
            }
        }

        public async Task<List<Attack>> SearchAttack(string address, string needle, CancellationToken token)
        {
            var url = $"http://{address}:{Port}/api/SearchAttacks?needle={needle}";
            this.logger.LogDebug($"SearchAttack {needle}");
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.GetAsync(url, token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"CreateAttack failed: ({e.ToFancyString()}");
                throw new OfflineException($"Registration failed {e.GetType()}");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("/api/SearchAttacks returned unsuccessful status code");
            }

            List<Attack>? receivedMatches;
            try
            {
                receivedMatches = JsonSerializer.Deserialize<List<Attack>>(
                    await response.Content.ReadAsStringAsync(token), this.jsonOptions);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"SearchAttack failed: ({e.ToFancyString()}");
                throw new MumbleException("Could not deserialize /api/SearchAttacks result");
            }

            if (receivedMatches is null)
            {
                this.logger.LogWarning($"Deserialization returned null");
                throw new MumbleException("Could not deserialize /api/SearchAttacks result");
            }

            foreach (var match in receivedMatches)
            {
                if (match.Id == 0)
                {
                    this.logger.LogWarning($"/api/SearchAttacks returned attack with id=0");
                    throw new MumbleException("Could not deserialize /api/SearchAttacks result");
                }
            }

            return receivedMatches;
        }

        public async Task CheckWaspImage(string address, CancellationToken token)
        {
            var result = await this.httpClient.GetAsync($"http://{address}:{Port}/images/wasp.jpg", token);
            if (!result.IsSuccessStatusCode)
            {
                throw new MumbleException("Could not get static files");
            }
        }

        public async Task CheckIndexHtml(string address, CancellationToken token)
        {
            var result = await this.httpClient.GetAsync($"http://{address}:{Port}/index.html", token);
            if (!result.IsSuccessStatusCode)
            {
                throw new MumbleException("Could not get static files");
            }
        }

        public async Task<Attack> GetAttack(string address, long id, string pw, CancellationToken token)
        {
            var url = $"http://{address}:{Port}/api/GetAttack?id={id}&password={pw}";
            this.logger.LogDebug($"SearchAttack {id} {pw}");
            string response;
            try
            {
                response = await this.httpClient.GetStringAsync(new Uri(url), token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"GetAttack failed: ({e.ToFancyString()}");
                throw new OfflineException($"GetAttack failed ({e.GetType()})");
            }

            Attack? a;
            try
            {
                a = JsonSerializer.Deserialize<Attack>(response, this.jsonOptions);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"Could not deserialize /api/GetAttack result: {e.ToFancyString()}");
                throw new MumbleException($"Could not deserialize /api/GetAttack result");
            }

            if (a is null)
            {
                this.logger.LogWarning($"Deserialization of /api/GetAttack result is null");
                throw new MumbleException($"Could not deserialize /api/GetAttack result");
            }

            return a;
        }
    }
}
