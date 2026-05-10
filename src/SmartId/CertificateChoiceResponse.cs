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
    /// Validated certificate-choice session outcome (Java <c>CertificateChoiceResponse</c>).
    /// </summary>
    public sealed class CertificateChoiceResponse
    {
        public string EndResult { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public CertificateLevel CertificateLevel { get; set; }

        public string DocumentNumber { get; set; }

        public string InteractionFlowUsed { get; set; }

        public string DeviceIpAddress { get; set; }
    }
}
