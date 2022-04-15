using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace AssetsRestApi
{
    public sealed class SdsSecurityHandler : DelegatingHandler
    {
        private readonly string _resource;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _accessToken;
        private DateTime _accessTokenExpiry = DateTime.MinValue;

        public SdsSecurityHandler(string resource, string clientId, string clientSecret)
        {
            _resource = resource;
            _clientId = clientId;
            _clientSecret = clientSecret;

            InnerHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip,
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false));

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_accessToken != null && DateTime.UtcNow < _accessTokenExpiry)
            {
                return _accessToken;
            }

            using HttpClient client = new ();
            using DiscoveryDocumentRequest discoveryRequest = new ()
            {
                Address = _resource + "/identity",
                Policy = new DiscoveryPolicy
                {
                    Authority = "https://identity.aveva.com",
                    ValidateEndpoints = false,
                    ValidateIssuerName = false,
                },
            };

            DiscoveryDocumentResponse discoveryResponse = await client.GetDiscoveryDocumentAsync(discoveryRequest, cancellationToken).ConfigureAwait(false);

            if (discoveryResponse.IsError)
                throw new InvalidOperationException(discoveryResponse.Error);

            using ClientCredentialsTokenRequest clientCredentialsTokenRequest = new ()
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = "ocsapi",
            };

            DateTime now = DateTime.UtcNow;

            TokenResponse tokenResponse = await client.RequestClientCredentialsTokenAsync(clientCredentialsTokenRequest, cancellationToken).ConfigureAwait(false);

            if (discoveryResponse.IsError)
                throw new InvalidOperationException(tokenResponse.Error);

            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new InvalidOperationException("Failed to acquire Access Token");

            _accessToken = tokenResponse.AccessToken;

            // Add a buffer of 30 seconds to the expiration delta.
            _accessTokenExpiry = now.AddSeconds(tokenResponse.ExpiresIn - 30);

            return _accessToken;
        }
    }
}
