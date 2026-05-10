/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// PKIX-style trust using embedded SK test/production CA material (Java <c>CertificateValidatorImpl</c> behaviour, .NET chain).
    /// </summary>
    public sealed class CertificateValidatorImpl : ICertificateValidator
    {
        private readonly IReadOnlyList<X509Certificate2> trustedCaCertificates;

        public CertificateValidatorImpl(IReadOnlyList<X509Certificate2> trustedCaCertificates)
        {
            this.trustedCaCertificates = trustedCaCertificates ?? throw new ArgumentNullException(nameof(trustedCaCertificates));
        }

        /// <summary>
        /// Validator using the same embedded PEM trust material as <see cref="AuthenticationResponseValidator"/>.
        /// </summary>
        public static CertificateValidatorImpl CreateDefault()
        {
            return new CertificateValidatorImpl(EmbeddedSmartIdTrustedCaCertificates.LoadDefaultPemCertificates());
        }

        public void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }
            ValidateCertificateIsCurrentlyValid(certificate);
            ValidateCertificateChain(certificate);
        }

        private static void ValidateCertificateIsCurrentlyValid(X509Certificate2 certificate)
        {
            var utc = DateTime.UtcNow;
            if (utc < certificate.NotBefore.ToUniversalTime() || utc > certificate.NotAfter.ToUniversalTime())
            {
                throw new UnprocessableSmartIdResponseException("Certificate is invalid");
            }
        }

        private void ValidateCertificateChain(X509Certificate2 certificate)
        {
            if (!IsCertificateTrusted(certificate))
            {
                throw new UnprocessableSmartIdResponseException("Certificate chain validation failed");
            }
        }

        private bool IsCertificateTrusted(X509Certificate2 certificate)
        {
            return SmartIdPkixTrust.IsChainTrusted(certificate, trustedCaCertificates);
        }

    }
}
