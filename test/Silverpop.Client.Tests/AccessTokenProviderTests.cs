using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Silverpop.Client.Tests
{
    public class AccessTokenProviderTests
    {
        private readonly AccessTokenProvider _sut;
        private readonly MockMessageHandler _messageHandler;

        private int _accessTokenExpiresInSeconds = (int)TimeSpan.FromDays(1).TotalSeconds;

        public AccessTokenProviderTests()
        {
            Func<AccessTokenRefreshResponse> responseProvider =
                () => new AccessTokenRefreshResponse
                {
                    AccessToken = Guid.NewGuid().ToString(),
                    ExpiresIn = _accessTokenExpiresInSeconds,
                    RefreshToken = Guid.NewGuid().ToString(),
                    TokenType = "test-token-type"
                };

            _messageHandler = new MockMessageHandler(responseProvider);
            _sut = new AccessTokenProvider(
                new TransactClientConfiguration(),
                new HttpClient(_messageHandler));
        }

        public class GetMethodTests : AccessTokenProviderTests
        {
            [Fact]
            public void ShouldInitiallyRequestAnAccessToken()
            {
                _sut.Get();

                Assert.Single(_messageHandler.Requests);
            }

            [Fact]
            public void ShouldUseCachedTokenIfNotExpired()
            {
                var token1 = _sut.Get();

                var token2 = _sut.Get();

                Assert.Equal(token1, token2);
            }

            [Fact]
            public void ShouldRequestAccessTokenWhenExpired()
            {
                _accessTokenExpiresInSeconds = 0;

                var token1 = _sut.Get();

                var token2 = _sut.Get();

                Assert.NotEqual(token1, token2);
            }

            [Fact]
            public async Task ShouldUseCachedTokenFromGetAsync()
            {
                var getAsyncToken = await _sut.GetAsync();

                var getToken = _sut.Get();

                Assert.Equal(getAsyncToken, getToken);
            }
        }

        public class GetAsyncMethodTests : AccessTokenProviderTests
        {
            [Fact]
            public async Task ShouldInitiallyRequestAnAccessToken()
            {
                await _sut.GetAsync();

                Assert.Single(_messageHandler.Requests);
            }

            [Fact]
            public async Task ShouldUseCachedTokenIfNotExpired()
            {
                var token1 = await _sut.GetAsync();

                var token2 = await _sut.GetAsync();

                Assert.Equal(token1, token2);
            }

            [Fact]
            public async Task ShouldRequestAccessTokenWhenExpired()
            {
                _accessTokenExpiresInSeconds = 0;

                var token1 = await _sut.GetAsync();

                var token2 = await _sut.GetAsync();

                Assert.NotEqual(token1, token2);
            }

            [Fact]
            public async Task ShouldUseCachedTokenFromGet()
            {
                var getToken = _sut.Get();

                var getAsyncToken = await _sut.GetAsync();

                Assert.Equal(getToken, getAsyncToken);
            }
        }

        private class MockMessageHandler : HttpMessageHandler
        {
            private readonly Func<object> _responseProvider;

            public ICollection<HttpRequestMessage> Requests { get; private set; } =
                new List<HttpRequestMessage>();

            public MockMessageHandler(Func<object> responseProvider)
            {
                _responseProvider = responseProvider;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Requests.Add(request);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_responseProvider()))
                });
            }
        }
    }
}
