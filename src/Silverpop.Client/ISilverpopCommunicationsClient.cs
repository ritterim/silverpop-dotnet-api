using System;
using System.IO;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public interface ISilverpopCommunicationsClient : IDisposable
    {
        string HttpUpload(string data, bool tryRefreshingOAuthAccessToken = true);

        Task<string> HttpUploadAsync(string data, bool tryRefreshingOAuthAccessToken = true);

        void SftpUpload(string data, string destinationPath);

        Task SftpUploadAsync(string data, string destinationPath);

        void SftpMove(string fromPath, string toPath);

        Task SftpMoveAsync(string fromPath, string toPath);

        Stream SftpDownload(string filePath);

        Task<Stream> SftpDownloadAsync(string filePath);
    }
}