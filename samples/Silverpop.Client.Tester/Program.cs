using Silverpop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Silverpop.Client.Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private async static Task MainAsync(string[] args)
        {
            var configuration = new TransactClientConfiguration()
            {
                TransactHttpsHost = "transact#.silverpop.com/XTMail",
                TransactSftpHost = "transfer#.silverpop.com",
                Username = "username",
                Password = "password"
            };

            var client = new TransactClient(configuration);

            var sendMessageResponse = client.SendMessage(GetTestMessage("SendMessage"));
            Console.WriteLine("sendMessageResponse:");
            Console.WriteLine(sendMessageResponse);
            Console.WriteLine();

            var sendMessageAsyncResponse = await client.SendMessageAsync(GetTestMessage("SendMessageAsync"));
            Console.WriteLine("sendMessageAsyncResponse:");
            Console.WriteLine(sendMessageAsyncResponse);
            Console.WriteLine();

            var sendMessageBatchResponse = client.SendMessageBatch(GetTestMessage("SendMessageBatch"));
            Console.WriteLine("sendMessageBatchResponse:");
            Console.WriteLine(sendMessageBatchResponse);
            Console.WriteLine();

            var sendMessageBatchAsyncResponse = await client.SendMessageBatchAsync(GetTestMessage("SendMessageBatchAsync"));
            Console.WriteLine("sendMessageBatchAsyncResponse:");
            Console.WriteLine(sendMessageBatchResponse);
            Console.WriteLine();

            var timeout = TimeSpan.FromMinutes(5);
            var sleepDuration = TimeSpan.FromSeconds(15);

            var getStatusOfMessageBatchStart = DateTime.Now;
            while (true)
            {
                var getStatusOfMessageBatchResponse = client.GetStatusOfMessageBatch(sendMessageBatchResponse.Single());

                if (getStatusOfMessageBatchResponse == null)
                {
                    Console.WriteLine(
                        "getStatusOfMessageBatchResponse: Response not yet available, sleeping for " +
                        sleepDuration.ToString());

                    Thread.Sleep(sleepDuration);
                }
                else
                {
                    Console.WriteLine("getStatusOfMessageBatchResponse:");
                    Console.WriteLine(getStatusOfMessageBatchResponse.RawResponse);
                    Console.WriteLine();

                    break;
                }

                if (getStatusOfMessageBatchStart.Add(timeout) > DateTime.Now)
                {
                    Console.WriteLine("getStatusOfMessageBatchResponse: Response timed out after " + timeout);
                    break;
                }
            }

            var getStatusOfMessageBatchAsyncStart = DateTime.Now;
            while (true)
            {
                var getStatusOfMessageBatchAsyncResponse = await client.GetStatusOfMessageBatchAsync(sendMessageBatchResponse.Single());

                if (getStatusOfMessageBatchAsyncResponse == null)
                {
                    Console.WriteLine(
                        "getStatusOfMessageBatchAsyncResponse: Response not yet available, sleeping for " +
                        sleepDuration.ToString());

                    Thread.Sleep(sleepDuration);
                }
                else
                {
                    Console.WriteLine("getStatusOfMessageBatchAsyncResponse:");
                    Console.WriteLine(getStatusOfMessageBatchAsyncResponse.RawResponse);
                    Console.WriteLine();

                    break;
                }

                if (getStatusOfMessageBatchAsyncStart.Add(timeout) > DateTime.Now)
                {
                    Console.WriteLine("getStatusOfMessageBatchAsyncResponse: Response timed out after " + timeout);
                    break;
                }
            }

            Console.ReadLine();
        }

        private static TransactMessage GetTestMessage(string testMethod)
        {
            return new TransactMessage()
            {
                CampaignId = "123456",
                TransactionId = string.Format("{0}Test-{1}", testMethod, Guid.NewGuid().ToString()),
                Recipients = new List<TransactMessageRecipient>()
                {
                    new TransactMessageRecipient()
                    {
                        BodyType = TransactMessageRecipientBodyType.Html,
                        EmailAddress = "user@example.com",
                        PersonalizationTags = new Dictionary<string, string>()
                        {
                            { "tag1", "tag1-value" }
                        }
                    }
                }
            };
        }
    }
}