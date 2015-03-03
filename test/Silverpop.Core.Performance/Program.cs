using System;
using System.Collections.Generic;
using System.Linq;

namespace Silverpop.Core.Performance
{
    internal class Program
    {
        private static IEnumerable<int> TestRecipientCounts = new List<int>()
        {
            1000000,
            5000000,
        };

        private static int TestRecipientsPerBatch = 5000;

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

            foreach (var testRecipientCount in TestRecipientCounts)
            {
                Console.WriteLine(
                    "Testing {0} recipients with {1} tags using batches of {2}:",
                    testRecipientCount,
                    numberOfTags,
                    TestRecipientsPerBatch);

                new TransactMessagePerformance()
                    .InvokeGetRecipientBatchedMessages(
                        testRecipientCount,
                        TestRecipientsPerBatch,
                        personalizationTags);
            }

            Console.ReadLine();
        }
    }
}