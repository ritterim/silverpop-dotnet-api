using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    internal class SilverpopCommunicationsClient : ISilverpopCommunicationsClient
    {
        private readonly TransactClientConfiguration _configuration;
        private readonly AccessTokenProvider _accessTokenProvider;
        private readonly string _transactHttpsUrl;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly SftpClient _sftpClient;

        public SilverpopCommunicationsClient(TransactClientConfiguration configuration)
        {
            _configuration = configuration;
            _accessTokenProvider = new AccessTokenProvider(configuration);

            _transactHttpsUrl = string.Format(
                "https://transact{0}.silverpop.com/XTMail",
                configuration.PodNumber);

            _httpClientFactory = () => new HttpClient();

            var transactSftpHost = string.Format(
                "transfer{0}.silverpop.com",
                configuration.PodNumber);

            if (_configuration.Username != null &&
                _configuration.Password != null)
            {
                _sftpClient = new SftpClient(
                    transactSftpHost,
                    _configuration.Username,
                    _configuration.Password);
            }
        }

        public string HttpUpload(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            var httpClient = GetAuthorizedHttpClient();

            if (OAuthSpecified())
            {
                try
                {
                    var response = httpClient.PostAsync(_transactHttpsUrl, new StringContent(data)).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _accessTokenProvider.Refresh();
                        return HttpUpload(data, tryRefreshingOAuthAccessToken: false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var response = httpClient.PostAsync(_transactHttpsUrl, new StringContent(data)).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<string> HttpUploadAsync(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            var httpClient = await GetAuthorizedHttpClientAsync();

            if (OAuthSpecified())
            {
                ExceptionDispatchInfo capturedException = null;
                try
                {
                    var response = await httpClient.PostAsync(_transactHttpsUrl, new StringContent(data));
                    return await response.Content.ReadAsStringAsync();
                }
                catch (WebException ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }

                if (tryRefreshingOAuthAccessToken && capturedException != null)
                {
                    var ex = capturedException.SourceException as WebException;

                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _accessTokenProvider.Refresh();
                        return await HttpUploadAsync(data, tryRefreshingOAuthAccessToken = false);
                    }
                    else
                    {
                        capturedException.Throw();
                    }
                }

                capturedException.Throw();
                return null;
            }
            else
            {
                var response = await httpClient.PostAsync(_transactHttpsUrl, new StringContent(data));
                return await response.Content.ReadAsStringAsync();
            }
        }

        public void SftpGzipUpload(string data, string destinationPath)
        {
            var sftpClient = GetConnectedSftpClient();

            using (var file = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                using (var ms = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal, /* leaveOpen: */ true))
                    {
                        file.WriteTo(gzipStream);
                        gzipStream.Flush();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    sftpClient.UploadFile(ms, destinationPath, /* canOverride: */ false);
                }
            }
        }

        public Task SftpGzipUploadAsync(string data, string destinationPath)
        {
            var sftpClient = GetConnectedSftpClient();

            using (var file = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var ms = new MemoryStream();

                using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal, /* leaveOpen: */ true))
                {
                    file.WriteTo(gzipStream);
                    gzipStream.Flush();
                }

                ms.Seek(0, SeekOrigin.Begin);
                return Task.Factory.FromAsync(
                    sftpClient.BeginUploadFile(ms, destinationPath, /* canOverride: */ false, null, null),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Disconnect();
                        ms.Dispose();
                    });
            }
        }

        public void SftpMove(string fromPath, string toPath)
        {
            var sftpClient = GetConnectedSftpClient();
            sftpClient.RenameFile(fromPath, toPath);
        }

        public async Task SftpMoveAsync(string fromPath, string toPath)
        {
            await Task.Run(() =>
            {
                var sftpClient = GetConnectedSftpClient();
                sftpClient.RenameFile(fromPath, toPath);
            });
        }

        public Stream SftpDownload(string filePath)
        {
            var ms = new MemoryStream();

            try
            {
                var sftpClient = GetConnectedSftpClient();
                sftpClient.DownloadFile(filePath, ms);
            }
            catch (SftpPathNotFoundException)
            {
                return null;
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public Task<Stream> SftpDownloadAsync(string filePath)
        {
            var ms = new MemoryStream();

            var sftpClient = GetConnectedSftpClient();

            try
            {
                return Task.Factory.FromAsync<Stream>(
                    sftpClient.BeginDownloadFile(filePath, ms),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Disconnect();

                        ms.Seek(0, SeekOrigin.Begin);
                        return ms;
                    });
            }
            catch (SftpPathNotFoundException)
            {
                return Task.FromResult<Stream>(null);
            }
        }

        private SftpClient GetConnectedSftpClient()
        {
            if (!_sftpClient.IsConnected)
                _sftpClient.Connect();

            return _sftpClient;
        }

        private HttpClient GetAuthorizedHttpClient()
        {
            var httpClient = _httpClientFactory();
            if (OAuthSpecified())
            {
                var accessToken = _accessTokenProvider.Get();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                SetUsernameAndPasswordAuthorization(httpClient);
            }
            return httpClient;
        }

        private async Task<HttpClient> GetAuthorizedHttpClientAsync()
        {
            var httpClient = _httpClientFactory();
            if (OAuthSpecified())
            {
                var accessToken = await _accessTokenProvider.GetAsync();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                SetUsernameAndPasswordAuthorization(httpClient);
            }
            return httpClient;
        }

        private bool OAuthSpecified()
        {
            if (_configuration.OAuthClientId != null ||
                _configuration.OAuthClientSecret != null ||
                _configuration.OAuthRefreshToken != null)
            {
                if (_configuration.OAuthClientId == null)
                    throw new ArgumentNullException("OAuthClientId");

                if (_configuration.OAuthClientSecret == null)
                    throw new ArgumentNullException("OAuthClientSecret");

                if (_configuration.OAuthRefreshToken == null)
                    throw new ArgumentNullException("OAuthRefreshToken");

                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetUsernameAndPasswordAuthorization(HttpClient httpClient)
        {
            // Adapted from http://stackoverflow.com/a/10181583/941536
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(
                        string.Format(
                            "{0}:{1}",
                            _configuration.Username,
                            _configuration.Password))));
        }

        public void Dispose()
        {
            if (_sftpClient != null && _sftpClient.IsConnected)
                _sftpClient.Disconnect();
        }
    }
}