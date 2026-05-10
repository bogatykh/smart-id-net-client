/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Common.Certificate;
using SK.SmartId.Exceptions;
using SK.SmartId.Util;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Non-qualified signature certificate purpose (Java <c>NonQualifiedSignatureCertificatePurposeValidator</c>).
    /// </summary>
    public sealed class NonQualifiedSignatureCertificatePurposeValidator : ISignatureCertificatePurposeValidator
    {
        public void Validate(X509Certificate2 certificate)
        {
            NonQualifiedSmartIdCertificateValidator.Validate(certificate);
            if (!CertificateAttributeUtil.HasNonRepudiationKeyUsage(certificate))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate does not have Non-Repudiation set in 'KeyUsage' extension");
            }
        }
    }
}
