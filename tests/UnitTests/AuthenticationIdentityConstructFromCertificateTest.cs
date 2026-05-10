/*-
 * #%L
 * Smart ID .NET client — unit tests
 * Ports Java AuthenticationIdentityMapperTest using <see cref="AuthenticationResponseValidator.ConstructAuthenticationIdentity"/>.
 * #L%
 */

using SK.SmartId.Util;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId
{
    public class AuthenticationIdentityConstructFromCertificateTest
    {
        [Fact]
        public void Construct_fromMockAuthCertificate_ok()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Resources", "test-certs", "auth-cert-40504040001.pem.crt");
            string pemOrB64 = File.ReadAllText(path);
            X509Certificate2 cert = pemOrB64.TrimStart().StartsWith("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal)
                ? X509Certificate2.CreateFromPem(pemOrB64)
                : CertificateParser.ParseX509Certificate(pemOrB64);
            AuthenticationIdentity id = AuthenticationResponseValidator.ConstructAuthenticationIdentity(cert);

            Assert.Equal("OK", id.GivenName);
            Assert.Equal("TESTNUMBER", id.Surname);
            Assert.Equal("40504040001", id.IdentityNumber);
            Assert.Equal("EE", id.Country);
            Assert.Same(cert, id.AuthCertificate);

            Assert.NotNull(id.DateOfBirth);
            Assert.Equal(new DateTime(1905, 4, 4), id.DateOfBirth.Value.Date);
        }
    }
}
