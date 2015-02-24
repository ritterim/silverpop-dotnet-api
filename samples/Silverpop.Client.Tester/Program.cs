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
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan SleepDuration = TimeSpan.FromSeconds(30);

        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private async static Task MainAsync(string[] args)
        {
            var client = TransactClient.Create(
                -1, // Change this to the correct pod number
                "username",
                "password");

            var oAuthClient = TransactClient.CreateOAuthOnly(
                -1, // Change this to the correct pod number
                oAuthClientId: "00000000-0000-0000-0000-000000000000",
                oAuthClientSecret: "00000000-0000-0000-0000-000000000000",
                oAuthRefreshToken: "00000000-0000-0000-0000-000000000000");

            var sendMessageResponse = client.SendMessage(GetTestMessage("SendMessage"));
            Console.WriteLine("sendMessageResponse:");
            Console.WriteLine(sendMessageResponse.RawResponse);
            Console.WriteLine();

            var sendMessageAsyncResponse = await client.SendMessageAsync(GetTestMessage("SendMessageAsync"));
            Console.WriteLine("sendMessageAsyncResponse:");
            Console.WriteLine(sendMessageAsyncResponse.RawResponse);
            Console.WriteLine();

            var sendMessageOAuthResponse = oAuthClient.SendMessage(GetTestMessage("SendMessage-UsingOAuth"));
            Console.WriteLine("sendMessageOAuthResponse:");
            Console.WriteLine(sendMessageOAuthResponse.RawResponse);
            Console.WriteLine();

            var sendMessageAsyncOAuthResponse = await oAuthClient.SendMessageAsync(GetTestMessage("SendMessageAsync-UsingOAuth"));
            Console.WriteLine("sendMessageAsyncOAuthResponse:");
            Console.WriteLine(sendMessageAsyncOAuthResponse.RawResponse);
            Console.WriteLine();

            var sendMessageBatchResponse = client.SendMessageBatch(GetTestMessage("SendMessageBatch"));
            Console.WriteLine("sendMessageBatchResponse:");
            Console.WriteLine(sendMessageBatchResponse.Single());
            Console.WriteLine();

            var getStatusOfMessageBatchStart = DateTime.Now;
            while (true)
            {
                try
                {
                    var getStatusOfMessageBatchResponse =
                        client.GetStatusOfMessageBatch(sendMessageBatchResponse.Single());

                    Console.WriteLine("getStatusOfMessageBatchResponse:");
                    Console.WriteLine(getStatusOfMessageBatchResponse.RawResponse);
                    Console.WriteLine();

                    break;
                }
                catch (TransactClientException ex)
                {
                    if (ex.Message != string.Format(
                        "Requested file transact/status/{0} does not currently exist.",
                        sendMessageBatchResponse.Single()))
                    {
                        throw;
                    }

                    Console.WriteLine("getStatusOfMessageBatchResponse:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();

                    Console.WriteLine(string.Format("Sleeping for {0}...", SleepDuration.ToString()));
                    Thread.Sleep(SleepDuration);
                    Console.WriteLine();
                }

                if (getStatusOfMessageBatchStart.Add(Timeout) < DateTime.Now)
                {
                    Console.WriteLine(string.Format(
                        "getStatusOfMessageBatchResponse: Response timed out after {0}.",
                        Timeout.ToString()));
                    break;
                }
            }

            var sendMessageBatchAsyncResponse = await client.SendMessageBatchAsync(GetTestMessage("SendMessageBatchAsync"));
            Console.WriteLine("sendMessageBatchAsyncResponse:");
            Console.WriteLine(sendMessageBatchAsyncResponse.Single());
            Console.WriteLine();

            var getStatusOfMessageBatchAsyncStart = DateTime.Now;
            while (true)
            {
                try
                {
                    var getStatusOfMessageBatchAsyncResponse =
                        await client.GetStatusOfMessageBatchAsync(sendMessageBatchAsyncResponse.Single());

                    Console.WriteLine("getStatusOfMessageBatchAsyncResponse:");
                    Console.WriteLine(getStatusOfMessageBatchAsyncResponse.RawResponse);
                    Console.WriteLine();

                    break;
                }
                catch (TransactClientException ex)
                {
                    if (ex.Message != string.Format(
                        "Requested file transact/status/{0} does not currently exist.",
                        sendMessageBatchAsyncResponse.Single()))
                    {
                        throw;
                    }

                    Console.WriteLine("getStatusOfMessageBatchAsyncResponse:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();

                    Console.WriteLine(string.Format("Sleeping for {0}...", SleepDuration.ToString()));
                    Thread.Sleep(SleepDuration);
                    Console.WriteLine();
                }

                if (getStatusOfMessageBatchAsyncStart.Add(Timeout) < DateTime.Now)
                {
                    Console.WriteLine(string.Format(
                        "getStatusOfMessageBatchAsyncResponse: Response timed out after {0}.",
                        Timeout.ToString()));
                    break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Operation completed.");
            Console.ReadLine();
        }

        private static TransactMessage GetTestMessage(string testType)
        {
            return TransactMessage.Create("123456", TransactMessageRecipient.Create(
                "user@example.com",
                TransactMessageRecipientBodyType.Html,
                new Dictionary<string, string>()
                {
                    { "tag1", "tag1-value" }
                }));
        }
    }
}