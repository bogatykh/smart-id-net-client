/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>Legacy RSASSA-PKCS#1 v1.5 verification (Java <c>RsaSsaPkcs1SignatureFactory</c>).</summary>
    public sealed class RsaSsaPkcs1SignatureFactory : ISignatureFactory
    {
        private readonly SigningSignatureAlgorithm signingSignatureAlgorithm;

        public RsaSsaPkcs1SignatureFactory(SigningSignatureAlgorithm signingSignatureAlgorithm)
            : this((SigningSignatureAlgorithm?)signingSignatureAlgorithm)
        {
        }

        public RsaSsaPkcs1SignatureFactory(SigningSignatureAlgorithm? signingSignatureAlgorithm)
        {
            if (!signingSignatureAlgorithm.HasValue)
            {
                throw new SmartIdClientException("Parameter 'signatureAlgorithmName' is not provided");
            }
            if (!signingSignatureAlgorithm.Value.IsLegacyRsa())
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature algorithm '" + signingSignatureAlgorithm.Value +
                    "' is not a legacy RSA (RSASSA-PKCS#1 v1.5) algorithm; use validate(..., RsaSsaPssParameters) for RSASSA-PSS");
            }
            this.signingSignatureAlgorithm = signingSignatureAlgorithm.Value;
        }

        public bool VerifySignature(X509Certificate2 certificate, byte[] data, byte[] signatureValue)
        {
            using (RSA rsa = certificate.GetRSAPublicKey())
            {
                if (rsa == null)
                {
                    throw new UnprocessableSmartIdResponseException("Signature value validation failed");
                }
                SmartIdHashAlgorithm? hash = signingSignatureAlgorithm.GetHashAlgorithmForLegacy();
                HashAlgorithmName hashName = ToHashAlgorithmName(hash.Value);
                return rsa.VerifyData(data, signatureValue, hashName, RSASignaturePadding.Pkcs1);
            }
        }

        private static HashAlgorithmName ToHashAlgorithmName(SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                    return HashAlgorithmName.SHA256;
                case SmartIdHashAlgorithm.SHA_384:
                    return HashAlgorithmName.SHA384;
                case SmartIdHashAlgorithm.SHA_512:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }
    }
}
