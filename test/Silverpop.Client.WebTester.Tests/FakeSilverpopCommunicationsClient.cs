using System.IO;
using System.Threading.Tasks;

namespace Silverpop.Client.WebTester.Tests
{
    public class FakeSilverpopCommunicationsClient : ISilverpopCommunicationsClient
    {
        public string HttpUpload(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            return null;
        }

        public Task<string> HttpUploadAsync(string data, bool tryRefreshingOAuthAccessToken = true)
        {
            return Task.FromResult(string.Empty);
        }

        public void SftpCreateDirectoryIfNotExists(string path)
        {
        }

        public void SftpGzipUpload(string data, string destinationPath)
        {
        }

        public Task SftpGzipUploadAsync(string data, string destinationPath)
        {
            return Task.FromResult(0);
        }

        public void SftpMove(string fromPath, string toPath)
        {
        }

        public Task SftpMoveAsync(string fromPath, string toPath)
        {
            return Task.FromResult(0);
        }

        public Stream SftpDownload(string filePath)
        {
            return new MemoryStream();
        }

        public Task<Stream> SftpDownloadAsync(string filePath)
        {
            return Task.FromResult((Stream)new MemoryStream());
        }

        public void Dispose()
        {
        }
    }
}