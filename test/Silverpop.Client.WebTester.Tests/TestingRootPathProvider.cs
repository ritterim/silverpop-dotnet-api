using Nancy;
using System.IO;

namespace Silverpop.Client.WebTester.Tests
{
    // Adapted from https://github.com/NancyFx/Nancy/wiki/Nancy-Testing-View-Location
    public class TestingRootPathProvider : IRootPathProvider
    {
        private static readonly string RootPath;

        static TestingRootPathProvider()
        {
            var directoryName = Path.GetDirectoryName(typeof(TestingRootPathProvider).Assembly.CodeBase);

            if (directoryName != null)
            {
                var assemblyPath = directoryName.Replace(@"file:\", string.Empty);

                RootPath = Path.Combine(assemblyPath, "..", "..", "..", "..", "..", "samples", "Silverpop.Client.WebTester");
            }
        }

        public string GetRootPath()
        {
            return RootPath;
        }
    }
}