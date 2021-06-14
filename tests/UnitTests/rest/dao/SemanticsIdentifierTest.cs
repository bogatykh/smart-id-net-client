using Xunit;

namespace SK.SmartId.Rest.Dao
{
    public class SemanticsIdentifierTest
    {
        [Fact]
        public void constructor1()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier("AAA", "BB", "C123");

            Assert.Equal("AAABB-C123", semanticsIdentifier.Identifier);
        }

        [Fact]
        public void constructor2()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, "BB", "CCC");

            Assert.Equal("PNOBB-CCC", semanticsIdentifier.Identifier);
        }

        [Fact]
        public void constructor3()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "CCC-DDDDD");

            Assert.Equal("PNOLV-CCC-DDDDD", semanticsIdentifier.Identifier);
        }

    }
}