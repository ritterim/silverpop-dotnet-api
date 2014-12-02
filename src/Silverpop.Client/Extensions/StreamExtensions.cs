using System.IO;
using System.Text;

namespace Silverpop.Client.Extensions
{
    public static class StreamExtensions
    {
        public static string ToContentString(this Stream stream, Encoding encoding)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                return encoding.GetString(ms.ToArray());
            }
        }
    }
}