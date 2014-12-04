using System.Collections.Generic;
using System.Linq;

namespace Silverpop.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <remarks>
        /// This method is from http://stackoverflow.com/a/13731854/941536
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items,
                                                           int maxItems)
        {
            return items.Select((item, inx) => new { item, inx })
                        .GroupBy(x => x.inx / maxItems)
                        .Select(g => g.Select(x => x.item));
        }
    }
}