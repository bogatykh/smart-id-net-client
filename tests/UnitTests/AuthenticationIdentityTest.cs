using Xunit;

namespace SK.SmartId
{
    public class AuthenticationIdentityTest
    {
        [Fact]
        public void GetIdentityCode()
        {
            AuthenticationIdentity authenticationIdentity = new AuthenticationIdentity();
            authenticationIdentity.IdentityNumber = "identityNumber";

            Assert.Equal("identityNumber", authenticationIdentity.IdentityCode);
        }

        [Fact]
        public void SetIdentityCode()
        {
            AuthenticationIdentity authenticationIdentity = new AuthenticationIdentity();
            authenticationIdentity.IdentityCode = "identityCode";

            Assert.Equal("identityCode", authenticationIdentity.IdentityNumber);
        }
    }
}