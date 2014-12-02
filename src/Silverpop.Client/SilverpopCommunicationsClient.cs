using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    internal class SilverpopCommunicationsClient : ISilverpopCommunicationsClient
    {
        private readonly TransactClientConfiguration _configuration;
        private readonly WebClient _webClient;

        public SilverpopCommunicationsClient(TransactClientConfiguration configuration)
        {
            _configuration = configuration;

            _webClient = new WebClient()
            {
                Credentials = new NetworkCredential(
                    _configuration.Username,
                    _configuration.Password)
            };
        }

        public string HttpUpload(string data)
        {
            return _webClient.UploadString("https://" + _configuration.TransactHttpHost, data);
        }

        public async Task<string> HttpUploadAsync(string data)
        {
            return await _webClient.UploadStringTaskAsync("https://" + _configuration.TransactHttpHost, data);
        }

        public void FtpUpload(string data, string destinationPath)
        {
            WithOpenSftpConnection(x =>
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    x.UploadFile(ms, destinationPath, /*canOverride: */ false);
                }
            });
        }

        public Task FtpUploadAsync(string data, string destinationPath)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var sftpClient = GetSftpClient();

                sftpClient.Connect();

                return Task.Factory.FromAsync(
                    sftpClient.BeginUploadFile(ms, destinationPath, /*canOverride: */ false, null, null),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Disconnect();
                    });
            }
        }

        public void FtpMove(string fromPath, string toPath)
        {
            WithOpenSftpConnection(x =>
            {
                x.RenameFile(fromPath, toPath);
            });
        }

        public Stream FtpDownload(string filePath)
        {
            var ms = new MemoryStream();

            try
            {
                WithOpenSftpConnection(x =>
                {
                    x.DownloadFile(filePath, ms);
                });
            }
            catch (SftpPathNotFoundException)
            {
                return null;
            }

            return ms;
        }

        public Task<Stream> FtpDownloadAsync(string filePath)
        {
            var ms = new MemoryStream();

            var sftpClient = GetSftpClient();

            sftpClient.Connect();

            try
            {
                return Task.Factory.FromAsync<Stream>(
                    sftpClient.BeginDownloadFile(filePath, ms),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Disconnect();

                        return ms;
                    });
            }
            catch (SftpPathNotFoundException)
            {
                return Task.FromResult<Stream>(null);
            }
        }

        private SftpClient GetSftpClient()
        {
            return new SftpClient(
                _configuration.TransactFtpHost,
                _configuration.Username,
                _configuration.Password);
        }

        private void WithOpenSftpConnection(Action<SftpClient> action)
        {
            var sftpClient = GetSftpClient();

            sftpClient.Connect();

            action(sftpClient);

            sftpClient.Disconnect();
        }
    }
}