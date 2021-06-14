using Xunit;

namespace SK.SmartId
{
    public class AuthenticationIdentityTest
    {
        [Fact]
        public void GetSurName()
        {
            AuthenticationIdentity authenticationIdentity = new AuthenticationIdentity();
            authenticationIdentity.Surname = "surname";

#pragma warning disable CS0612 // Type or member is obsolete
            Assert.Equal("surname", authenticationIdentity.SurName);
#pragma warning restore CS0612 // Type or member is obsolete
        }

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