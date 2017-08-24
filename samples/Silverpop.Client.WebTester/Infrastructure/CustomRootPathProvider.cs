using Nancy;
using System.IO;

namespace Silverpop.Client.WebTester.Infrastructure
{
    // https://github.com/nandotech/NancyAzureFileUpload/blob/4b837eaf10c64c35c4f9ecc9046ac9ac14c4ed5c/src/NancyAzureFileUpload/Helpers/CustomRootPathProvider.cs
    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
