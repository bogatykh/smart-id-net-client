/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.DefaultTrustedCAStoreBuilderTest</c>.
 * #
 * Java <c>DefaultTrustedCAStoreBuilder</c>: trust anchors, intermediate CAs, optional OCSP (OCSP-on path throws or is @Disabled in Java 3.2).
 * .NET: embedded PEM via <see cref="CertificateValidatorImpl.CreateDefault"/> or explicit <c>IList&lt;X509Certificate2&gt;</c> (PKIX via <c>SmartIdPkixTrust</c>, revocation off).
 * #L%
 */

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId;

public sealed class DefaultTrustedCAStoreBuilderTest
{
    /// <summary>
    /// Parity with Java <c>buildDefaultTrustedCACertStore_ocspValidationDisabled</c> — building a validator
    /// from test trust anchors and intermediate CA succeeds (no exception).
    /// </summary>
    [Fact]
    public void Explicit_test_trust_anchors_and_intermediate_builds_like_Java_ocsp_disabled()
    {
        var trust = new List<X509Certificate2>
        {
            TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
            TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt"),
        };
        var validator = new CertificateValidatorImpl(trust);
        X509Certificate2 endEntity = TestCertificateUtil.ParseTestCertificate("auth-cert-40504040001.pem.crt");
        validator.Validate(endEntity);
    }

    [Fact]
    public void CreateDefault_loads_embedded_trust_material_without_throwing()
    {
        var validator = CertificateValidatorImpl.CreateDefault();
        Assert.NotNull(validator);
    }
}
