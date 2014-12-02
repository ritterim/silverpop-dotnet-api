using System.IO;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public interface ISilverpopCommunicationsClient
    {
        string HttpUpload(string data);

        Task<string> HttpUploadAsync(string data);

        void FtpUpload(string data, string destinationPath);

        Task FtpUploadAsync(string data, string destinationPath);

        void FtpMove(string fromPath, string toPath);

        Stream FtpDownload(string filePath);

        Task<Stream> FtpDownloadAsync(string filePath);
    }
}