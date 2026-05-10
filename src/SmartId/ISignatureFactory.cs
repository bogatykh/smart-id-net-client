/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>Builds signature verification for session responses (Java <c>SignatureFactory</c>).</summary>
    public interface ISignatureFactory
    {
        /// <summary>Verifies <paramref name="signatureValue"/> over <paramref name="data"/> with the certificate public key.</summary>
        /// <returns>true if the signature is valid.</returns>
        /// <exception cref="Exceptions.UnprocessableSmartIdResponseException">Algorithm or parameters cannot be used.</exception>
        bool VerifySignature(X509Certificate2 certificate, byte[] data, byte[] signatureValue);
    }
}
