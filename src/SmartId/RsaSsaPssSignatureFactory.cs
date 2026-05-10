/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>RSASSA-PSS verification (Java <c>RsaSsaPssSignatureFactory</c>).</summary>
    public sealed class RsaSsaPssSignatureFactory : ISignatureFactory
    {
        private readonly RsaSsaPssParameters rsaSsaPssParameters;

        public RsaSsaPssSignatureFactory(RsaSsaPssParameters rsaSsaPssParameters)
        {
            if (rsaSsaPssParameters == null)
            {
                throw new SmartIdClientException("Parameter 'rsaSsaPssParameters' is not provided");
            }
            this.rsaSsaPssParameters = rsaSsaPssParameters;
        }

        public bool VerifySignature(X509Certificate2 certificate, byte[] data, byte[] signatureValue)
        {
            if (!MaskGenAlgorithm.IsSupportedApiOrLegacyMgfName(rsaSsaPssParameters.MaskGenAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException("Invalid signature algorithm parameters were provided");
            }
            var bcCert = new X509CertificateParser().ReadCertificate(certificate.RawData);
            var pub = bcCert.GetPublicKey();
            if (!(pub is RsaKeyParameters rsaPub))
            {
                throw new UnprocessableSmartIdResponseException("Invalid signature algorithm parameters were provided");
            }
            IDigest contentDigest = CreateDigest(rsaSsaPssParameters.DigestHashAlgorithm);
            IDigest mgfDigest = CreateDigest(rsaSsaPssParameters.MaskHashAlgorithm);
            var pss = new PssSigner(new RsaEngine(), contentDigest, mgfDigest, rsaSsaPssParameters.SaltLength);
            pss.Init(false, rsaPub);
            pss.BlockUpdate(data, 0, data.Length);
            return pss.VerifySignature(signatureValue);
        }

        private static IDigest CreateDigest(SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                    return new Sha256Digest();
                case SmartIdHashAlgorithm.SHA_384:
                    return new Sha384Digest();
                case SmartIdHashAlgorithm.SHA_512:
                    return new Sha512Digest();
                case SmartIdHashAlgorithm.SHA3_256:
                    return new Sha3Digest(256);
                case SmartIdHashAlgorithm.SHA3_384:
                    return new Sha3Digest(384);
                case SmartIdHashAlgorithm.SHA3_512:
                    return new Sha3Digest(512);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }
    }
}
