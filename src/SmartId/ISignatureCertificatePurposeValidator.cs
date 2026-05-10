/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Validates signing certificate purpose (Java <c>SignatureCertificatePurposeValidator</c>).
    /// </summary>
    public interface ISignatureCertificatePurposeValidator
    {
        void Validate(X509Certificate2 certificate);
    }
}
