/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Runs the same not-before/not-after check as <see cref="CertificateValidatorImpl"/> without PKIX chain building.
    /// Used for session-structure unit tests; full PKIX chain checks use <see cref="CertificateValidatorImpl"/> (PEM pool + SmartIdPkixTrust), not JKS.
    /// </summary>
    internal sealed class TestValidityOnlyCertificateValidator : ICertificateValidator
    {
        public void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }
            var utc = DateTime.UtcNow;
            if (utc < certificate.NotBefore.ToUniversalTime() || utc > certificate.NotAfter.ToUniversalTime())
            {
                throw new UnprocessableSmartIdResponseException("Certificate is invalid");
            }
        }
    }
}
