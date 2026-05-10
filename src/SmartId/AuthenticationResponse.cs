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
    /// Successful authentication session mapped from <see cref="Rest.Dao.SessionStatus"/> (Java <c>AuthenticationResponse</c>).
    /// </summary>
    public sealed class AuthenticationResponse
    {
        public string EndResult { get; set; }

        public string ServerRandom { get; set; }

        public string UserChallenge { get; set; }

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

        public X509Certificate2 Certificate { get; set; }

        public AuthenticationCertificateLevel CertificateLevel { get; set; }

        public string DocumentNumber { get; set; }

        public string InteractionTypeUsed { get; set; }

        public FlowType FlowType { get; set; }

        public string DeviceIpAddress { get; set; }

        public RsaSsaPssParameters RsaSsaPssSignatureParameters { get; set; }
    }
}
