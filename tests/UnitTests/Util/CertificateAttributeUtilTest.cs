using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId.Util
{
    public class CertificateAttributeUtilTest
    {
        [Fact]
        public void getDateOfBirthFromCertificateAttribute_datePresent_returns()
        {
            X509Certificate2 certificateWithDob = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV_WITH_DOB));

            var dateOfBirthCertificateAttribute = CertificateAttributeUtil.GetDateOfBirth(certificateWithDob);

            Assert.NotNull(dateOfBirthCertificateAttribute);
            Assert.Equal(new DateTime(1903, 3, 3), dateOfBirthCertificateAttribute);
        }

        [Fact]
        public void getDateOfBirthFromCertificateAttribute_dateNotPresent_returnsEmpty()
        {
            X509Certificate2 certificateWithoutDobAttribute = AuthenticationResponseValidatorTest.GetX509Certificate(AuthenticationResponseValidatorTest.GetX509CertificateBytes(AuthenticationResponseValidatorTest.AUTH_CERTIFICATE_LV));

            var dateOfBirthCertificateAttribute = CertificateAttributeUtil.GetDateOfBirth(certificateWithoutDobAttribute);

            Assert.Null(dateOfBirthCertificateAttribute);
        }

    }
}
