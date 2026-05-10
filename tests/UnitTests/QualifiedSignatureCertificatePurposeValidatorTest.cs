/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.QualifiedSignatureCertificatePurposeValidatorTest</c>.
 * #L%
 */

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X509.Qualified;
using SK.SmartId.Exceptions;
using SK.SmartId.TestSupport;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId;

public sealed class QualifiedSignatureCertificatePurposeValidatorTest
{
    private const string SkQualifiedPolicyOid = "1.3.6.1.4.1.10015.17.2";
    /// <summary>Java <c>QCP_N_QSCD_OID</c> (test constant 0.4.0.194112.1.2).</summary>
    private const string QcpNQscdOid = "0.4.0.194112.1.2";

    private static readonly DerObjectIdentifier IdEtsiQctEsign = new DerObjectIdentifier("0.4.0.1862.1.6.1");
    /// <summary>eSeal — wrong purpose for qualified signing (matches Java ETSI <c>id_etsi_qct_eseal</c>).</summary>
    private static readonly DerObjectIdentifier IdEtsiQctEseal = new DerObjectIdentifier("0.4.0.1862.1.6.2");

    private readonly QualifiedSignatureCertificatePurposeValidator validator = new QualifiedSignatureCertificatePurposeValidator();

    [Fact]
    public void Validate_ok()
    {
        X509Certificate2 cert =
            TestCertificateUtil.ParseTestCertificate("cert-choice-cert-40504040001.pem.cert");
        var ex = Record.Exception(() => validator.Validate(cert));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_certificatePoliciesAreMissing_throwException()
    {
        X509Certificate2 cert = InvalidCertificateGenerator.NewBuilder().CreateCertificate();
        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(cert));
        Assert.Equal("Certificate does not have certificate policy OIDs", ex.Message);
    }

    [Fact]
    public void Validate_invalidCertificatePolicies_throwException()
    {
        var policyInfo = new PolicyInformation(new DerObjectIdentifier("1.3.6.1.4.1.99999.1"), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(policyInfo);
        X509Certificate2 cert = InvalidCertificateGenerator.NewBuilder().WithPolicies(policies).CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(cert));
        Assert.Equal("Certificate does not contain required qualified certificate policy OIDs", ex.Message);
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
        var skQPolicy = new PolicyInformation(new DerObjectIdentifier(SkQualifiedPolicyOid), new DerSequence());
        var qcp = new PolicyInformation(new DerObjectIdentifier(QcpNQscdOid), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(skQPolicy, qcp);
        var builder = InvalidCertificateGenerator.NewBuilder().WithPolicies(policies);
        if (keyUsage != null)
        {
            builder = builder.WithKeyUsage(keyUsage);
        }
        X509Certificate2 cert = builder.CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(cert));
        Assert.Equal(
            "Certificate does not have Non-Repudiation set in 'KeyUsage' extension",
            ex.Message);
    }

    [Fact]
    public void Validate_QsStatementsExtensionIsMissing_throwException()
    {
        var skQPolicy = new PolicyInformation(new DerObjectIdentifier(SkQualifiedPolicyOid), new DerSequence());
        var qcp = new PolicyInformation(new DerObjectIdentifier(QcpNQscdOid), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(skQPolicy, qcp);
        var keyUsage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.NonRepudiation);
        X509Certificate2 cert = InvalidCertificateGenerator.NewBuilder()
            .WithPolicies(policies)
            .WithKeyUsage(keyUsage)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(cert));
        Assert.Equal("Certificate does not have 'QCStatements' extension", ex.Message);
    }

    [Fact]
    public void Validate_qsStatementsDoesNotHaveElectronicSigning_throwException()
    {
        var skQPolicy = new PolicyInformation(new DerObjectIdentifier(SkQualifiedPolicyOid), new DerSequence());
        var qcp = new PolicyInformation(new DerObjectIdentifier(QcpNQscdOid), new DerSequence());
        CertificatePolicies policies = InvalidCertificateGenerator.CreateCertificatePolicies(skQPolicy, qcp);
        var keyUsage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.NonRepudiation);
        // Java: new QCStatement(ETSIQCObjectIdentifiers.id_etsi_qct_eseal) — eSeal, not eSign
        var qcStatement = new QCStatement(IdEtsiQctEseal);
        X509Certificate2 cert = InvalidCertificateGenerator.NewBuilder()
            .WithPolicies(policies)
            .WithKeyUsage(keyUsage)
            .WithQcStatement(qcStatement)
            .CreateCertificate();

        var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(cert));
        Assert.Equal(
            "Certificate does not have electronic signature OID (" + IdEtsiQctEsign.Id +
            ") in QCStatements extension.",
            ex.Message);
    }
}
