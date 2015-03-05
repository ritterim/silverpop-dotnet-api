using Silverpop.Core;

namespace Silverpop.Client.WebTester.Tests
{
    public class FakeTransactMessageEncoder : TransactMessageEncoder
    {
        private readonly string _encodeOutput;

        public FakeTransactMessageEncoder(string encodeOutput)
        {
            _encodeOutput = encodeOutput;
        }

        public override string Encode(TransactMessage message)
        {
            return _encodeOutput;
        }
    }
}