using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace DI.HttpClientHandlers
{
    public class ClientCredentialOptions
    {
        public string ClientSecret { get; set; }

        public string Authority { get; set; }
    }

    public class ClientCredentialsRefreshHandler : DelegatingHandler
    {
        private static string _token;
        private readonly HttpClient _client;
        private readonly ILoggerFactory _factory;
        private readonly ClientCredentialOptions _options;

        public ClientCredentialsRefreshHandler(ClientCredentialOptions options, ILoggerFactory factory)
        {
            _options = options;
            _factory = factory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_token) || IsNearExpiry(_token))
            {
                var logger = _factory.CreateLogger<ClientCredentialsRefreshHandler>();
                using (logger.BeginScope("RequestToken"))
                {
                    try
                    {
                        _token = await GetToken();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to request token from {authEndpoint}", _options.Authority);
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    }
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetToken()
        {
            // discover endpoints from metadata
            var disco = await _client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _options.Authority, Policy = new DiscoveryPolicy {RequireHttps = true}
            });

            if (disco.IsError)
                throw new ArgumentException($"Failed to connect to auth endpoint {_options.Authority}, {disco.Error}");
            // request token
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "id",
                ClientSecret = _options.ClientSecret,
                Scope = "myscope"
            });

            if (tokenResponse.IsError)
                throw new ArgumentException(
                    $"Failed to request credientials token from {_options.Authority}, {tokenResponse.Error}");

            return tokenResponse.Json.TryGetString("access_token");
        }

        private static bool IsNearExpiry(string token)
        {
            var jwt = new JwtSecurityToken(token);
            return jwt.ValidTo.ToUniversalTime() - DateTime.UtcNow <= TimeSpan.FromSeconds(3);
        }
    }
}