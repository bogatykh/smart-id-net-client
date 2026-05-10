/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

namespace SK.SmartId
{
    /// <summary>
    /// Factory for <see cref="ISignatureCertificatePurposeValidator"/> (Java <c>SignatureCertificatePurposeValidatorFactory</c>).
    /// </summary>
    public interface ISignatureCertificatePurposeValidatorFactory
    {
        ISignatureCertificatePurposeValidator Create(CertificateLevel certificateLevel);
    }
}
