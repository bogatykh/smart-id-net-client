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
    /// <summary>Validates raw signature bytes (Java <c>SignatureValueValidator</c>).</summary>
    public interface ISignatureValueValidator
    {
        void Validate(
            byte[] signatureValue,
            byte[] payload,
            X509Certificate2 certificate,
            ISignatureFactory signatureFactory);
    }
}
