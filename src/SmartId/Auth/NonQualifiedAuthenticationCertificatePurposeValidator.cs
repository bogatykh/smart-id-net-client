/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Common.Certificate;
using SK.SmartId.Exceptions.Permanent;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId.Auth
{
    public sealed class NonQualifiedAuthenticationCertificatePurposeValidator : IAuthenticationCertificatePurposeValidator
    {
        public void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new SmartIdClientException("Parameter 'certificate' is not provided");
            }

            NonQualifiedSmartIdCertificateValidator.Validate(certificate);
            SmartIdAuthenticationCertificateValidator.Validate(certificate);
        }
    }
}
