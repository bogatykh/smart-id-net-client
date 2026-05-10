/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

namespace SK.SmartId.Auth
{
    public interface IAuthenticationCertificatePurposeValidatorFactory
    {
        IAuthenticationCertificatePurposeValidator Create(AuthenticationCertificateLevel certificateLevel);
    }
}
