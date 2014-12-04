using System;
using System.Collections.Generic;
using System.Linq;

namespace Silverpop.Core.Performance
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var numberOfTags = 15;
            var tagValue = new string(
                Enumerable.Repeat("ABC", 1000)
                    .SelectMany(x => x)
                    .ToArray());

            var personalizationTags = new Dictionary<string, string>();

            for (var i = 0; i < numberOfTags; i++)
                personalizationTags.Add("Tag" + i, tagValue);

            Console.WriteLine("Testing 1 million recipients with {0} tags using batches of 5000:", numberOfTags);
            new TransactMessagePerformance()
                .InvokeGetRecipientBatchedMessages(1000000, 5000, personalizationTags);

            Console.WriteLine("Testing 10 million recipients with {0} tags using batches of 5000:", numberOfTags);
            new TransactMessagePerformance()
                .InvokeGetRecipientBatchedMessages(10000000, 5000, personalizationTags);

            Console.ReadLine();
        }
    }
}