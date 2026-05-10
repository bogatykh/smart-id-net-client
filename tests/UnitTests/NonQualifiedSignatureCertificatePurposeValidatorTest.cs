/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.NonQualifiedSignatureCertificatePurposeValidatorTest</c>.
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using SK.SmartId.Exceptions;
using SK.SmartId.TestSupport;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId;

public sealed class NonQualifiedSignatureCertificatePurposeValidatorTest
{
    private const string SkNonQualifiedPolicyOid = "1.3.6.1.4.1.10015.17.1";
    private const string NcpPolicyOid = "0.4.0.2042.1.1";

    private readonly NonQualifiedSignatureCertificatePurposeValidator validator =
        new NonQualifiedSignatureCertificatePurposeValidator();

    [Fact]
    public void Validate_ok()
    {
        X509Certificate2 certificate = TestCertificateUtil.ParseTestCertificate("nq-signing-cert.pem");
        var ex = Record.Exception(() => validator.Validate(certificate));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_certificatePoliciesAreMissing_throwException()
    {
        X509Certificate2 certificate = InvalidCertificateGenerator.NewBuilder().CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(certificate));
        Assert.Equal(
            "Certificate does not have certificate policy OIDs and is not a non-qualified Smart-ID certificate",
            ex.Message);
    }

    [Fact]
    public void Validate_invalidCertificatePolicies_throwException()
    {
        var policyInfo = new PolicyInformation(new DerObjectIdentifier("1.3.6.1.4.1.99999.1"), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(policyInfo);
        X509Certificate2 certificate = InvalidCertificateGenerator.NewBuilder().WithPolicies(policies).CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(certificate));
        Assert.Equal("Certificate is not a non-qualified Smart-ID certificate", ex.Message);
    }

    public static TheoryData<KeyUsage> KeyUsageMissingNonRepudiation() => new TheoryData<KeyUsage>
    {
        null!,
        new KeyUsage(KeyUsage.DigitalSignature),
    };

    [Theory]
    [MemberData(nameof(KeyUsageMissingNonRepudiation))]
    public void Validate_keyUsageNonRepudiationIsMissing_throwException(KeyUsage keyUsage)
    {
        var skNq = new PolicyInformation(new DerObjectIdentifier(SkNonQualifiedPolicyOid), new DerSequence());
        var ncp = new PolicyInformation(new DerObjectIdentifier(NcpPolicyOid), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(skNq, ncp);
        var builder = InvalidCertificateGenerator.NewBuilder().WithPolicies(policies);
        if (keyUsage != null)
        {
            builder = builder.WithKeyUsage(keyUsage);
        }

        X509Certificate2 certificate = builder.CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(certificate));
        Assert.Equal(
            "Certificate does not have Non-Repudiation set in 'KeyUsage' extension",
            ex.Message);
    }
}
