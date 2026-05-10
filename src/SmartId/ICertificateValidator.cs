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
    /// Trust and validity checks for session certificates (Java <c>CertificateValidator</c>).
    /// </summary>
    public interface ICertificateValidator
    {
        void Validate(X509Certificate2 certificate);
    }
}
