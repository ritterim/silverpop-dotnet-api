using Silverpop.Core;

namespace Silverpop.Client.WebTester.Tests
{
    public class FakeTransactMessageResponseDecoder : TransactMessageResponseDecoder
    {
        private readonly TransactMessageResponse _response;

        public FakeTransactMessageResponseDecoder(TransactMessageResponse response)
        {
            _response = response;
        }

        public override TransactMessageResponse Decode(string xmlResponse)
        {
            return _response;
        }
    }
}