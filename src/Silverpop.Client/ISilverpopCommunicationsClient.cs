using System;
using System.IO;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public interface ISilverpopCommunicationsClient : IDisposable
    {
        string HttpUpload(string data);

        Task<string> HttpUploadAsync(string data);

        void SftpUpload(string data, string destinationPath);

        Task SftpUploadAsync(string data, string destinationPath);

        void SftpMove(string fromPath, string toPath);

        Stream SftpDownload(string filePath);

        Task<Stream> SftpDownloadAsync(string filePath);
    }
}