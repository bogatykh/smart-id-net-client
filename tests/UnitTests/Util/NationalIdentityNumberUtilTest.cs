using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId.Util
{
    public class NationalIdentityNumberUtilTest
    {
        [Fact]
        public void getDateOfBirthFromIdCode_estonianIdCode_returns()
        {
            X509Certificate2 eeCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_EE));

            AuthenticationResponseValidator validator = new AuthenticationResponseValidator();
            AuthenticationIdentity identity = validator.ConstructAuthenticationIdentity(eeCertificate);


            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1801, 1, 1), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_latvianIdCode_returns()
        {
            X509Certificate2 lvCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV));

            AuthenticationResponseValidator validator = new AuthenticationResponseValidator();
            AuthenticationIdentity identity = validator.ConstructAuthenticationIdentity(lvCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(2017, 1, 1), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_lithuanianIdCode_returns()
        {
            X509Certificate2 ltCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LT));

            AuthenticationResponseValidator validator = new AuthenticationResponseValidator();
            AuthenticationIdentity identity = validator.ConstructAuthenticationIdentity(ltCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1960, 9, 6), dateOfBirth);
        }

    }
}
