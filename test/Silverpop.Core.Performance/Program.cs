using System;
using System.Collections.Generic;
using System.Linq;

namespace Silverpop.Core.Performance
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tagValue = new string(
                Enumerable.Repeat("ABC", 1000)
                    .SelectMany(x => x)
                    .ToArray());

            var personalizationTags = new TestPersonalizationTags()
            {
                TagA = tagValue,
                TagB = tagValue,
                TagC = tagValue,
                TagD = tagValue,
                TagE = tagValue,
                TagF = tagValue,
                TagG = tagValue,
                TagH = tagValue,
                TagI = tagValue,
                TagJ = tagValue,
                TagK = tagValue,
                TagL = tagValue,
                TagM = tagValue,
                TagN = tagValue,
                TagO = tagValue
            };

            var numberOfTags = personalizationTags.GetType().GetProperties().Count();

            Console.WriteLine("Testing 1 million recipients with {0} tags using batches of 5000:", numberOfTags);
            new TransactMessagePerformance()
                .InvokeGetRecipientBatchedMessages(1000000, 5000, personalizationTags);

            Console.WriteLine("Testing 5 million recipients with {0} tags using batches of 5000:", numberOfTags);
            new TransactMessagePerformance()
                .InvokeGetRecipientBatchedMessages(5000000, 5000, personalizationTags);

            Console.ReadLine();
        }
    }
}