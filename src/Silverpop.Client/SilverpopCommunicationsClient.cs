﻿using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    internal class SilverpopCommunicationsClient : ISilverpopCommunicationsClient
    {
        private readonly TransactClientConfiguration _configuration;
        private readonly WebClient _webClient;
        private readonly SftpClient _sftpClient;

        public SilverpopCommunicationsClient(TransactClientConfiguration configuration)
        {
            _configuration = configuration;

            _webClient = new WebClient()
            {
                Credentials = new NetworkCredential(
                    _configuration.Username,
                    _configuration.Password)
            };

            var transactSftpHost = Regex.Replace(
                _configuration.TransactSftpUrl,
                @"sftp:\/\/",
                string.Empty,
                RegexOptions.IgnoreCase);

            _sftpClient = new SftpClient(
                transactSftpHost,
                _configuration.Username,
                _configuration.Password);
        }

        public string HttpUpload(string data)
        {
            return _webClient.UploadString(_configuration.TransactHttpsUrl, data);
        }

        public async Task<string> HttpUploadAsync(string data)
        {
            return await _webClient.UploadStringTaskAsync(_configuration.TransactHttpsUrl, data);
        }

        public void FtpUpload(string data, string destinationPath)
        {
            var sftpClient = GetConnectedSftpClient();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal))
            {
                sftpClient.UploadFile(gzipStream, destinationPath + ".gz", /*canOverride: */ false);
            }
        }

        public Task FtpUploadAsync(string data, string destinationPath)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal))
            {
                var sftpClient = GetConnectedSftpClient();

                return Task.Factory.FromAsync(
                    sftpClient.BeginUploadFile(gzipStream, destinationPath + ".gz", /*canOverride: */ false, null, null),
                    x =>
                    {
                        sftpClient.EndUploadFile(x);
                        sftpClient.Disconnect();
                    });
            }
        }

        public void FtpMove(string fromPath, string toPath)
        {
            var sftpClient = GetConnectedSftpClient();
            sftpClient.RenameFile(fromPath, toPath);
        }

        public Stream FtpDownload(string filePath)
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

            return ms;
        }

        public Task<Stream> FtpDownloadAsync(string filePath)
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

        public void Dispose()
        {
            if (_sftpClient.IsConnected)
                _sftpClient.Disconnect();
        }
    }
}