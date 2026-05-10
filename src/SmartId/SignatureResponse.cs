/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Validated signing session outcome (Java <c>SignatureResponse</c>).
    /// </summary>
    public sealed class SignatureResponse
    {
        public string EndResult { get; set; }

        public string SignatureValueInBase64 { get; set; }

        public byte[] SignatureValue
        {
            get
            {
                try
                {
                    return Convert.FromBase64String(SignatureValueInBase64);
                }
                catch (FormatException)
                {
                    throw new UnprocessableSmartIdResponseException(
                        "Failed to parse signature value in base64. Incorrectly encoded base64 string: '" + SignatureValueInBase64 + "'");
                }
            }
        }

        public string AlgorithmName { get; set; }

        public SigningSignatureAlgorithm SignatureAlgorithm { get; set; }

        public FlowType FlowType { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public CertificateLevel RequestedCertificateLevel { get; set; }

        public CertificateLevel CertificateLevel { get; set; }

        public string DocumentNumber { get; set; }

        /// <summary>Maps session <c>interactionTypeUsed</c> / legacy <c>interactionFlowUsed</c>.</summary>
        public string InteractionFlowUsed { get; set; }

        public string DeviceIpAddress { get; set; }

        public RsaSsaPssParameters RsaSsaPssParameters { get; set; }
    }
}
