/*-
 * #%L
 * Port of Java <c>ee.sk.smartid.auth.NonQualifiedAuthenticationCertificatePurposeValidatorTest</c>.
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using SK.SmartId.Auth;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.TestSupport;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId;

public sealed class NonQualifiedAuthenticationCertificatePurposeValidatorTest
{
    private static readonly X509Certificate2 NqAuthCert = TestCertificateUtil.ParsePemCertificateFromOutput(
        Path.Combine("Resources", "test-certs", "nq-auth-cert-40504049999.crt"));

    private static readonly X509Certificate2 NqSignCert = TestCertificateUtil.ParsePemCertificateFromOutput(
        Path.Combine("Resources", "test-certs", "nq-signing-cert.pem"));

    private const string SkNonQualifiedPolicyOid = "1.3.6.1.4.1.10015.17.1";
    private const string NcpPolicyOid = "0.4.0.2042.1.1";

    private readonly NonQualifiedAuthenticationCertificatePurposeValidator purposeValidator =
        new NonQualifiedAuthenticationCertificatePurposeValidator();

    [Fact]
    public void Validate_ok()
    {
        var ex = Record.Exception(() => purposeValidator.Validate(NqAuthCert));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_certificateNotProvided_throwException()
    {
        var ex = Assert.Throws<SmartIdClientException>(() => purposeValidator.Validate(null!));
        Assert.Equal("Parameter 'certificate' is not provided", ex.Message);
    }

    [Fact]
    public void Validate_certificatePoliciesAreMissing_throwException()
    {
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder().CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal(
            "Certificate does not have certificate policy OIDs and is not a non-qualified Smart-ID certificate",
            ex.Message);
    }

    [Fact]
    public void Validate_invalidCertificatePolicies_throwException()
    {
        var policyInfo = new PolicyInformation(new DerObjectIdentifier("1.3.6.1.4.1.99999.1"), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(policyInfo);
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder().WithPolicies(policies).CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Certificate is not a non-qualified Smart-ID certificate", ex.Message);
    }

    [Fact]
    public void Validate_extendedKeyUsageIsMissing_throwException()
    {
        CertificatePolicies policies = ToNonQualifiedAuthCertificate();
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder()
            .WithPolicies(policies)
            .WithExtendedKeyUsage(null!)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    [Fact]
    public void Validate_invalidExtendedKeyProvided_throwException()
    {
        CertificatePolicies policies = ToNonQualifiedAuthCertificate();
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.id_kp_smartcardlogon);
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder()
            .WithPolicies(policies)
            .WithExtendedKeyUsage(extendedKeyUsage)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    [Fact]
    public void Validate_keyUsageIsMissing()
    {
        CertificatePolicies policies = ToNonQualifiedAuthCertificate();
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.id_kp_clientAuth);
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder()
            .WithPolicies(policies)
            .WithExtendedKeyUsage(extendedKeyUsage)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    [Fact]
    public void Validate_keyUsageNotSmartIdAuth()
    {
        CertificatePolicies policies = ToNonQualifiedAuthCertificate();
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.id_kp_clientAuth);
        var keyUsage = new KeyUsage(KeyUsage.NonRepudiation);
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder()
            .WithPolicies(policies)
            .WithExtendedKeyUsage(extendedKeyUsage)
            .WithKeyUsage(keyUsage)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    [Fact]
    public void Validate_certificateCannotBeUsedForAuthentication_throwException()
    {
        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(NqSignCert));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    private static CertificatePolicies ToNonQualifiedAuthCertificate()
    {
        var skPolicy = new PolicyInformation(new DerObjectIdentifier(SkNonQualifiedPolicyOid), new DerSequence());
        var ncpPolicy = new PolicyInformation(new DerObjectIdentifier(NcpPolicyOid), new DerSequence());
        return InvalidCertificateGenerator.CreateCertificatePolicies(skPolicy, ncpPolicy);
    }
}
