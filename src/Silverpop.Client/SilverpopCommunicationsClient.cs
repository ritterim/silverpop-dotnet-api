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
        private readonly Func<SftpClient> _sftpConnectedClientFactory;

        public SilverpopCommunicationsClient(TransactClientConfiguration configuration)
        {
            _configuration = configuration;
            _accessTokenProvider = new AccessTokenProvider(configuration);

            _transactHttpsUrl = string.Format(
                "https://transact{0}.silverpop.com/XTMail",
                configuration.PodNumber);

            _httpClientFactory = () => new HttpClient();

            if (string.IsNullOrEmpty(_configuration.Username) ||
                string.IsNullOrEmpty(_configuration.Password))
            {
                _sftpConnectedClientFactory = () =>
                {
                    throw new InvalidOperationException(
                        $"{nameof(_configuration.Username)} and {nameof(_configuration.Password)} must be configured for SFTP usage.");
                };
            }
            else
            {
                _sftpConnectedClientFactory = () =>
                {
                    var sftpClient = new SftpClient(
                        $"transfer{configuration.PodNumber}.silverpop.com",
                        _configuration.Username,
                        _configuration.Password);

                    sftpClient.Connect();

                    return sftpClient;
                };
            }
        }

        public string HttpUpload(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            var httpClient = GetAuthorizedHttpClient();

            if (OAuthSpecified())
            {
                try
                {
                    var response = httpClient.PostAsync(_transactHttpsUrl, new StringContent(data))
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    return response.Content.ReadAsStringAsync()
                        .ConfigureAwait(false).GetAwaiter().GetResult();
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
                var response = httpClient.PostAsync(_transactHttpsUrl, new StringContent(data))
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                return response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public async Task<string> HttpUploadAsync(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            var httpClient = await GetAuthorizedHttpClientAsync().ConfigureAwait(false);

            if (OAuthSpecified())
            {
                ExceptionDispatchInfo capturedException = null;
                try
                {
                    var response = await httpClient.PostAsync(_transactHttpsUrl, new StringContent(data));
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                        return await HttpUploadAsync(data, tryRefreshingOAuthAccessToken = false).ConfigureAwait(false);
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
                var response = await httpClient.PostAsync(_transactHttpsUrl, new StringContent(data)).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public void SftpCreateDirectoryIfNotExists(string path)
        {
            using (var sftpClient = _sftpConnectedClientFactory())
            {
                if (!sftpClient.Exists(path))
                {
                    sftpClient.CreateDirectory(path);
                }
            }
        }

        public void SftpGzipUpload(string data, string destinationPath)
        {
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

                    using (var sftpClient = _sftpConnectedClientFactory())
                    {
                        sftpClient.UploadFile(ms, destinationPath, /* canOverride: */ false);
                    }
                }
            }
        }

        public Task SftpGzipUploadAsync(string data, string destinationPath)
        {
            using (var file = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var ms = new MemoryStream();

                using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal, /* leaveOpen: */ true))
                {
                    file.WriteTo(gzipStream);
                    gzipStream.Flush();
                }

                ms.Seek(0, SeekOrigin.Begin);

                var sftpClient = _sftpConnectedClientFactory();

                var task = Task.Factory.FromAsync(
                    sftpClient.BeginUploadFile(ms, destinationPath, /* canOverride: */ false, null, null),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Dispose();
                        ms.Dispose();
                    });

                task.ConfigureAwait(false);

                return task;
            }
        }

        public void SftpMove(string fromPath, string toPath)
        {
            using (var sftpClient = _sftpConnectedClientFactory())
            {
                sftpClient.RenameFile(fromPath, toPath);
            }
        }

        public async Task SftpMoveAsync(string fromPath, string toPath)
        {
            await Task.Run(() =>
            {
                using (var sftpClient = _sftpConnectedClientFactory())
                {
                    sftpClient.RenameFile(fromPath, toPath);
                }
            }).ConfigureAwait(false);
        }

        public Stream SftpDownload(string filePath)
        {
            var ms = new MemoryStream();

            try
            {
                using (var sftpClient = _sftpConnectedClientFactory())
                {
                    sftpClient.DownloadFile(filePath, ms);
                }
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

            var sftpClient = _sftpConnectedClientFactory();

            var task = Task.Factory.FromAsync<Stream>(
                sftpClient.BeginDownloadFile(filePath, ms),
                x =>
                {
                    try
                    {
                        sftpClient.EndDownloadFile(x);
                    }
                    catch (SftpPathNotFoundException)
                    {
                        return null;
                    }
                    finally
                    {
                        sftpClient.Dispose();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                });

            task.ConfigureAwait(false);

            return task;
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
                var accessToken = await _accessTokenProvider.GetAsync().ConfigureAwait(false);
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
        }
    }
}