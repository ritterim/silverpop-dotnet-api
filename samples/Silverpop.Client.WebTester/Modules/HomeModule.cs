using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Validation;
using Silverpop.Client.WebTester.Models;
using Silverpop.Core;
using System.Linq;

namespace Silverpop.Client.WebTester.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(TransactClient client)
        {
            Get["/"] = _ => View["Index", new IndexModel(client.Configuration)];

            Post["/send"] = _ =>
            {
                var model = this.Bind<SendModel>();

                // Validate request
                var validationResult = this.Validate<SendModel>(model);
                if (!validationResult.IsValid)
                    return new TextResponse(
                        HttpStatusCode.BadRequest,
                        string.Join(" ", validationResult.Errors.SelectMany(x => x.Value)));

                // Create message
                var recipient = TransactMessageRecipient.Create(model.ToAddress);
                recipient.PersonalizationTags = model.PersonalizationTags;

                var message = TransactMessage.Create(model.CampaignId, recipient);

                // Send message
                TransactMessageResponse response = null;
                try
                {
                    response = client.SendMessage(message);
                }
                catch (TransactClientException ex)
                {
                    return new TextResponse(
                        HttpStatusCode.InternalServerError,
                        ex.Message);
                }

                return Response.AsJson(response);
            };
        }
    }
}