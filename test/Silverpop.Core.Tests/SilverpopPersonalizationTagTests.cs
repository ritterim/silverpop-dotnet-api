using Xunit;

namespace Silverpop.Core.Tests
{
    public class SilverpopPersonalizationTagTests
    {
        public class ConstructorTests
        {
            [Fact]
            public void SetsName()
            {
                var sut = new SilverpopPersonalizationTag("test_name");
                Assert.Equal("test_name", sut.Name);
            }
        }
    }
}