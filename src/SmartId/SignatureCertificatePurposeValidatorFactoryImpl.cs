/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions.Permanent;

namespace SK.SmartId
{
    /// <summary>
    /// Default factory (Java <c>SignatureCertificatePurposeValidatorFactoryImpl</c>).
    /// </summary>
    public sealed class SignatureCertificatePurposeValidatorFactoryImpl : ISignatureCertificatePurposeValidatorFactory
    {
        public ISignatureCertificatePurposeValidator Create(CertificateLevel certificateLevel)
        {
            if (certificateLevel == CertificateLevel.ADVANCED)
            {
                return new NonQualifiedSignatureCertificatePurposeValidator();
            }
            if (certificateLevel == CertificateLevel.QUALIFIED || certificateLevel == CertificateLevel.QSCD)
            {
                return new QualifiedSignatureCertificatePurposeValidator();
            }
            throw new SmartIdClientException("Unsupported certificate level: " + certificateLevel);
        }
    }
}
