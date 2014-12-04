using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Silverpop.Core.Performance
{
    public class TransactMessagePerformance
    {
        public void InvokeGetRecipientBatchedMessages(
            int recipientCount,
            int recipientsPerBatch,
            IDictionary<string, string> personalizationTagsForEachRecipient)
        {
            var message = new TransactMessage()
            {
                Recipients = Enumerable.Range(0, recipientCount)
                    .Select(x => new TransactMessageRecipient()
                    {
                        EmailAddress = Guid.NewGuid().ToString() + "@example.com",
                        PersonalizationTags = personalizationTagsForEachRecipient
                    })
                    .ToList()
            };

            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Stopwatch started.");

            Console.WriteLine("Calling GetRecipientBatchedMessages on constructed message...");
            var recipientBatchedMesssages = message.GetRecipientBatchedMessages(recipientsPerBatch);

            Console.WriteLine("{0} messages were generated.", recipientBatchedMesssages.Count());
            Console.WriteLine();

            stopwatch.Stop();
            Console.WriteLine("Operation took {0}", stopwatch.Elapsed.ToString());
            Console.WriteLine();
        }
    }
}