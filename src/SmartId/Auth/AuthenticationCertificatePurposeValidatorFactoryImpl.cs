/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;

namespace SK.SmartId.Auth
{
    public sealed class AuthenticationCertificatePurposeValidatorFactoryImpl : IAuthenticationCertificatePurposeValidatorFactory
    {
        public IAuthenticationCertificatePurposeValidator Create(AuthenticationCertificateLevel certificateLevel)
        {
            switch (certificateLevel)
            {
                case AuthenticationCertificateLevel.QUALIFIED:
                    return new QualifiedAuthenticationCertificatePurposeValidator();
                case AuthenticationCertificateLevel.ADVANCED:
                    return new NonQualifiedAuthenticationCertificatePurposeValidator();
                default:
                    throw new ArgumentOutOfRangeException(nameof(certificateLevel), certificateLevel, null);
            }
        }
    }
}
