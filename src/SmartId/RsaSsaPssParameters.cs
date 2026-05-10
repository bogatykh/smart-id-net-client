/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

namespace SK.SmartId
{
    /// <summary>
    /// RSASSA-PSS parameters from a signing session (Java <c>RsaSsaPssParameters</c>).
    /// </summary>
    public sealed class RsaSsaPssParameters
    {
        public SmartIdHashAlgorithm DigestHashAlgorithm { get; set; }

        public string MaskGenAlgorithm { get; set; }

        public SmartIdHashAlgorithm MaskHashAlgorithm { get; set; }

        public int SaltLength { get; set; }

        public string TrailerField { get; set; }
    }
}
