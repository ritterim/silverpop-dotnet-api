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
                //PodNumber = ,
                Username = "username",
                Password = "password"
            };

            var oAuthConfiguration = new TransactClientConfiguration()
            {
                //PodNumber = ,
                OAuthClientId = "00000000-0000-0000-0000-000000000000",
                OAuthClientSecret = "00000000-0000-0000-0000-000000000000",
                OAuthRefreshToken = "00000000-0000-0000-0000-000000000000"
            };

            var client = new TransactClient(configuration);
            var oAuthClient = new TransactClient(oAuthConfiguration);

            var sendMessageResponse = client.SendMessage(GetTestMessage("SendMessage"));
            Console.WriteLine("sendMessageResponse:");
            Console.WriteLine(sendMessageResponse);
            Console.WriteLine();

            var sendMessageAsyncResponse = await client.SendMessageAsync(GetTestMessage("SendMessageAsync"));
            Console.WriteLine("sendMessageAsyncResponse:");
            Console.WriteLine(sendMessageAsyncResponse);
            Console.WriteLine();

            var sendMessageOAuthResponse = oAuthClient.SendMessage(GetTestMessage("SendMessage-UsingOAuth"));
            Console.WriteLine("sendMessageOAuthResponse:");
            Console.WriteLine(sendMessageOAuthResponse);
            Console.WriteLine();

            var sendMessageAsyncOAuthResponse = await oAuthClient.SendMessageAsync(GetTestMessage("SendMessageAsync-UsingOAuth"));
            Console.WriteLine("sendMessageAsyncOAuthResponse:");
            Console.WriteLine(sendMessageAsyncOAuthResponse);
            Console.WriteLine();

            var sendMessageBatchResponse = client.SendMessageBatch(GetTestMessage("SendMessageBatch"));
            Console.WriteLine("sendMessageBatchResponse:");
            Console.WriteLine(sendMessageBatchResponse);
            Console.WriteLine();

            var sendMessageBatchAsyncResponse = await client.SendMessageBatchAsync(GetTestMessage("SendMessageBatchAsync"));
            Console.WriteLine("sendMessageBatchAsyncResponse:");
            Console.WriteLine(sendMessageBatchAsyncResponse);
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

        private static TransactMessage GetTestMessage(string testType)
        {
            return new TransactMessage()
            {
                CampaignId = "123456",
                TransactionId = string.Format("{0}Test-{1}", testType, Guid.NewGuid().ToString()),
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