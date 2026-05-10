/*-
 * #%L
 * Port of Java <c>ee.sk.smartid.auth.QualifiedAuthenticationCertificatePurposeValidatorTest</c>.
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

public sealed class QualifiedAuthenticationCertificatePurposeValidatorTest
{
    private static readonly X509Certificate2 AuthCert = TestCertificateUtil.ParsePemCertificateFromOutput(
        Path.Combine("Resources", "test-certs", "auth-cert-40504040001-demo-q.crt"));

    private const string SkQualifiedAuthPolicyOid = "1.3.6.1.4.1.10015.17.2";
    private const string NcpPlusPolicyOid = "0.4.0.2042.1.2";

    private readonly QualifiedAuthenticationCertificatePurposeValidator purposeValidator = new QualifiedAuthenticationCertificatePurposeValidator();

    [Fact]
    public void Validate_authCert_afterApril2025_ok()
    {
        var ex = Record.Exception(() => purposeValidator.Validate(AuthCert));
        Assert.Null(ex);
    }

    /// <summary>
    /// Java <c>validate_authCert_beforeApril2025_ok</c> is @Disabled: demo cert uses legacy policy OID
    /// <c>1.3.6.1.4.1.10015.3.17.2</c> instead of required <c>1.3.6.1.4.1.10015.17.2</c>. We assert rejection (same as current Java behaviour if enabled).
    /// </summary>
    [Fact]
    public void Validate_authCert_beforeApril2025_legacy_policy_oid_rejected()
    {
        X509Certificate2 legacy = TestCertificateUtil.ParsePemCertificateFromOutput(
            Path.Combine("Resources", "test-certs", "auth-pnolv-020100-29990-mock-q.crt"));

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(legacy));
        Assert.Equal("Certificate is not a qualified Smart-ID authentication certificate", ex.Message);
    }

    [Fact]
    public void Validate_certificateIsNotProvided_throwException()
    {
        var ex = Assert.Throws<SmartIdClientException>(() => purposeValidator.Validate(null!));
        Assert.Equal("Parameter 'certificate' is not provided", ex.Message);
    }

    [Fact]
    public void Validate_certificatePoliciesAreMissing_throwException()
    {
        X509Certificate2 cert = InvalidCertificateGenerator.Builder().CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(cert));
        Assert.Equal(
            "Certificate does not have certificate policy OIDs and is not a qualified Smart-ID authentication certificate",
            ex.Message);
    }

    [Fact]
    public void Validate_invalidCertificatePolicies_throwException()
    {
        var policyInfo = new PolicyInformation(new DerObjectIdentifier("1.3.6.1.4.1.99999.1"), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(policyInfo);
        X509Certificate2 cert = InvalidCertificateGenerator.Builder().WithPolicies(policies).CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(cert));
        Assert.Equal("Certificate is not a qualified Smart-ID authentication certificate", ex.Message);
    }

    [Fact]
    public void Validate_extendedKeyUsageIsMissing_throwException()
    {
        CertificatePolicies policies = ToQualifiedSmartIdAuthPolicy();
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
        CertificatePolicies policies = ToQualifiedSmartIdAuthPolicy();
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
        CertificatePolicies policies = ToQualifiedSmartIdAuthPolicy();
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
        CertificatePolicies policies = ToQualifiedSmartIdAuthPolicy();
        var keyUsage = new KeyUsage(KeyUsage.NonRepudiation);
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.id_kp_clientAuth);
        X509Certificate2 certificate = InvalidCertificateGenerator.Builder()
            .WithPolicies(policies)
            .WithExtendedKeyUsage(extendedKeyUsage)
            .WithKeyUsage(keyUsage)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => purposeValidator.Validate(certificate));
        Assert.Equal("Provided certificate cannot be used for authentication", ex.Message);
    }

    private static CertificatePolicies ToQualifiedSmartIdAuthPolicy()
    {
        var skQPolicy = new PolicyInformation(new DerObjectIdentifier(SkQualifiedAuthPolicyOid), new DerSequence());
        var ncpPolicy = new PolicyInformation(new DerObjectIdentifier(NcpPlusPolicyOid), new DerSequence());
        return InvalidCertificateGenerator.CreateCertificatePolicies(skQPolicy, ncpPolicy);
    }
}
