using Nancy;
using Nancy.Testing;
using Silverpop.Client.WebTester.Models;
using Silverpop.Core;
using System.Collections.Generic;
using Xunit;

namespace Silverpop.Client.WebTester.Tests.Modules
{
    public class HomeModuleTests
    {
        private readonly Browser _browser;

        public HomeModuleTests()
        {
            StaticConfiguration.DisableErrorTraces = false;

            var bootstrapper = GetConfigurableBootstrapper();
            _browser = new Browser(bootstrapper);
        }

        public class GetRootTests : HomeModuleTests
        {
            private readonly BrowserResponse _response;

            public GetRootTests()
            {
                _response = _browser.Get("/", with =>
                {
                    with.HttpRequest();
                });
            }

            [Fact]
            public void ReturnsOk()
            {
                Assert.Equal(HttpStatusCode.OK, _response.StatusCode);
            }

            [Fact]
            public void ReturnsIndexView()
            {
                var html = _response.Body.AsString();

                var moduleName = _response.GetModuleName();
                var modulePath = _response.GetModulePath();
                var viewName = _response.GetViewName();
                var contentType = _response.ContentType;

                Assert.IsType<IndexModel>(_response.GetModel<IndexModel>());
            }
        }

        public class PostSendTests : HomeModuleTests
        {
            [Fact]
            public void ReturnsBadRequestWhenNoModel()
            {
                var response = _browser.Post("/send", with =>
                {
                    with.AjaxRequest();
                });

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }

            [Fact]
            public void ReturnsBadRequestWhenModelMissingCampaignId()
            {
                var response = _browser.Post("/send", with =>
                {
                    with.AjaxRequest();
                    with.JsonBody<SendModel>(new SendModel()
                    {
                        CampaignId = null,
                        ToAddress = "test@example.com"
                    });
                });

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Contains("'Campaign Id' should not be empty.", response.Body.AsString());
            }

            [Fact]
            public void ReturnsBadRequestWhenMissingToAddress()
            {
                var response = _browser.Post("/send", with =>
                {
                    with.AjaxRequest();
                    with.JsonBody<SendModel>(new SendModel()
                    {
                        CampaignId = "123",
                        ToAddress = null
                    });
                });

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Contains("'To Address' should not be empty.", response.Body.AsString());
            }

            [Fact]
            public void ReturnsInternalServerErrorForSendingError()
            {
                var testSpecificBootstrapper = GetConfigurableBootstrapper(
                    errorStringToUse: "Some error happened.");
                var testSpecificBrowser = new Browser(testSpecificBootstrapper);

                var response = testSpecificBrowser.Post("/send", with =>
                {
                    with.AjaxRequest();
                    with.JsonBody<SendModel>(new SendModel()
                    {
                        CampaignId = "123",
                        ToAddress = "test@example.com"
                    });
                });

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("Some error happened.", response.Body.AsString());
            }

            [Fact]
            public void ReturnsOkForSuccess()
            {
                var response = _browser.Post("/send", with =>
                {
                    with.AjaxRequest();
                    with.JsonBody<SendModel>(new SendModel()
                    {
                        CampaignId = "123",
                        ToAddress = "test@example.com"
                    });
                });

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        private ConfigurableBootstrapper GetConfigurableBootstrapper(
            string errorStringToUse = null)
        {
            return new ConfigurableBootstrapper(with =>
            {
                with.ApplicationStartup((container, pipelines) =>
                {
                    container.Register<TransactClient>((x, y) => new TransactClient(
                        new TransactClientConfiguration()
                        {
                            PodNumber = 0,
                        },
                        new FakeTransactMessageEncoder("encodeOutput_data"),
                        new FakeTransactMessageResponseDecoder(new TransactMessageResponse()
                        {
                            Error = new KeyValuePair<int, string>(1, errorStringToUse),
                            Status = errorStringToUse == null
                                ? TransactMessageResponseStatus.NoErrorsAllRecipientsSent
                                : TransactMessageResponseStatus.EncounteredErrorsNoMessagesSent
                        }),
                        () => new FakeSilverpopCommunicationsClient()));
                });

                with.RootPathProvider<TestingRootPathProvider>();
                with.ViewFactory<TestingViewFactory>();
                with.AllDiscoveredModules();
            });
        }
    }
}