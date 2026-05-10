/*-
 * #%L
 * Smart ID .NET client tests — behaviour aligned with Java <c>FileDefaultTrustedCAStoreBuilderTest</c> where applicable.
 * #
 * Java uses JKS <c>FileTrustedCAStoreBuilder</c>; .NET uses embedded PEM resources and explicit <c>X509Certificate2</c> lists
 * (no JKS). Tests here assert that contract instead of skipped JKS path/password placeholders.
 * #
 * OCSP-enabled builder scenarios are @Disabled / unimplemented in Java 3.2 as well — not asserted on .NET.
 * #L%
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using SK.SmartId.Exceptions;
using Xunit;

namespace SK.SmartId;

public sealed class FileDefaultTrustedCAStoreBuilderTest
{
    /// <summary>
    /// Mirrors Java <c>validateTrustedCaCertificatesOnInitiation_ocspValidationsDisabled</c> — default trust material is non-empty.
    /// </summary>
    [Fact]
    public void Default_embedded_pem_trust_material_is_not_empty()
    {
        IReadOnlyList<X509Certificate2> pem = EmbeddedSmartIdTrustedCaCertificates.LoadDefaultPemCertificates();
        Assert.NotEmpty(pem);
    }

    [Fact]
    public void CreateDefault_wraps_embedded_pem_material()
    {
        CertificateValidatorImpl validator = CertificateValidatorImpl.CreateDefault();
        Assert.NotNull(validator);
    }

    /// <summary>
    /// .NET analogue of rejecting unusable trust configuration: empty certificate pool cannot build a chain.
    /// </summary>
    [Fact]
    public void CertificateValidatorImpl_empty_trust_material_rejects_leaf_validation()
    {
        var validator = new CertificateValidatorImpl(Array.Empty<X509Certificate2>());
        X509Certificate2 endEntity = TestCertificateUtil.ParseTestCertificate("auth-cert-40504040001.pem.crt");
        UnprocessableSmartIdResponseException ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => validator.Validate(endEntity));
        Assert.Equal("Certificate chain validation failed", ex.Message);
    }

    [Fact]
    public void CertificateValidatorImpl_null_trust_material_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CertificateValidatorImpl(null));
    }
}
