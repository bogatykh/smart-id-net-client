using SK.SmartId.Exceptions;
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

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(eeCertificate);


            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1801, 1, 1), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_latvianIdCode_returns()
        {
            X509Certificate2 lvCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV_DOB_03_APRIL_1903));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(lvCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1903, 4, 3), dateOfBirth);
        }

        [Fact]
        public void getDateOfBirthFromIdCode_lithuanianIdCode_returns()
        {
            X509Certificate2 ltCertificate = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LT));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(ltCertificate);

            var dateOfBirth = NationalIdentityNumberUtil.GetDateOfBirth(identity);

            Assert.NotNull(dateOfBirth);
            Assert.Equal(new DateTime(1960, 9, 6), dateOfBirth);
        }
        [Fact]
        public void parseLvDateOfBirth_withoutDateOfBirth_returnsNull()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("321205-1234");
            Assert.Null(birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_21century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131205-2234");
            Assert.Equal(new DateTime(2005, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_20century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131265-1234");
            Assert.Equal(new DateTime(1965, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_19century()
        {
            var birthDate = NationalIdentityNumberUtil.ParseLvDateOfBirth("131265-0234");
            Assert.Equal(new DateTime(1865, 12, 13), birthDate);
        }

        [Fact]
        public void parseLvDateOfBirth_invalidMonth_throwsException()
        {
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => NationalIdentityNumberUtil.ParseLvDateOfBirth("131365-1234"));

            Assert.Equal("Unable get birthdate from Latvian personal code 131365-1234", exception.Message);
        }

        [Fact]
        public void parseLvDateOfBirth_invalidIdCode_throwsException()
        {
            var exception = Assert.Throws<UnprocessableSmartIdResponseException>(() => NationalIdentityNumberUtil.ParseLvDateOfBirth("331265-0234"));
            Assert.Equal("Unable get birthdate from Latvian personal code 331265-0234", exception.Message);
    }
    }
}
