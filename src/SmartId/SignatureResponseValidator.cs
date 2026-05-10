/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Util;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SK.SmartId
{
    /// <summary>
    /// Maps and validates signing <see cref="SessionStatus"/> (Java <c>SignatureResponseValidator</c>).
    /// </summary>
    public sealed class SignatureResponseValidator
    {
        private static readonly Regex SignatureValueBase64Pattern = new Regex(
            "^[a-zA-Z0-9+/]+={0,2}$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly ICertificateValidator certificateValidator;
        private readonly ISignatureCertificatePurposeValidatorFactory signatureCertificatePurposeValidatorFactory;

        public SignatureResponseValidator(ICertificateValidator certificateValidator)
            : this(certificateValidator, new SignatureCertificatePurposeValidatorFactoryImpl())
        {
        }

        public SignatureResponseValidator(
            ICertificateValidator certificateValidator,
            ISignatureCertificatePurposeValidatorFactory signatureCertificatePurposeValidatorFactory)
        {
            this.certificateValidator = certificateValidator;
            this.signatureCertificatePurposeValidatorFactory = signatureCertificatePurposeValidatorFactory;
        }

        public SignatureResponse Validate(SessionStatus sessionStatus, CertificateLevel requestedCertificateLevel)
        {
            ValidateSessionsStatus(sessionStatus, requestedCertificateLevel);

            SessionResult sessionResult = sessionStatus.Result;
            SessionSignature sessionSignature = sessionStatus.Signature;
            SessionCertificate certificate = sessionStatus.Cert;

            if (!SigningSignatureAlgorithmExtensions.TryParseFromApiAlgorithmName(sessionSignature.SignatureAlgorithmOrLegacy, out var signingAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithm' has unsupported value");
            }

            var response = new SignatureResponse
            {
                EndResult = sessionResult.EndResult,
                SignatureValueInBase64 = sessionSignature.Value,
                AlgorithmName = sessionSignature.SignatureAlgorithmOrLegacy,
                SignatureAlgorithm = signingAlgorithm,
                FlowType = FlowTypeExtensions.ParseApiValue(sessionSignature.FlowType),
                Certificate = CertificateParser.ParseX509Certificate(certificate.Value),
                RequestedCertificateLevel = requestedCertificateLevel,
                DocumentNumber = sessionResult.DocumentNumber,
                InteractionFlowUsed = sessionStatus.InteractionTypeUsedOrLegacy,
                DeviceIpAddress = sessionStatus.DeviceIpAddress
            };

            if (!CertificateLevelExtensions.TryParse(certificate.CertificateLevel, out var certLevelEnum))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'cert.certificateLevel' has unsupported value");
            }
            response.CertificateLevel = certLevelEnum;

            if (!SigningSignatureAlgorithmExtensions.IsLegacyRsaApiAlgorithmName(sessionSignature.SignatureAlgorithmOrLegacy))
            {
                SessionSignatureAlgorithmParameters p = sessionSignature.SignatureAlgorithmParameters;
                SmartIdHashAlgorithmExtensions.TryParse(p.HashAlgorithm, out var digestHash);
                SmartIdHashAlgorithmExtensions.TryParse(p.MaskGenAlgorithm.Parameters.HashAlgorithm, out var maskHash);
                response.RsaSsaPssParameters = new RsaSsaPssParameters
                {
                    DigestHashAlgorithm = digestHash,
                    MaskGenAlgorithm = p.MaskGenAlgorithm.Algorithm,
                    MaskHashAlgorithm = maskHash,
                    SaltLength = p.SaltLength.Value,
                    TrailerField = p.TrailerField
                };
            }

            return response;
        }

        private void ValidateSessionsStatus(SessionStatus sessionStatus, CertificateLevel requestedCertificateLevel)
        {
            if (sessionStatus == null)
            {
                throw new SmartIdClientException("Parameter 'sessionStatus' is not provided");
            }
            if (StringUtil.IsEmpty(sessionStatus.State))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'state' is empty");
            }
            if (!"COMPLETE".Equals(sessionStatus.State, StringComparison.OrdinalIgnoreCase))
            {
                throw new SmartIdClientException("Session is not complete. State: " + sessionStatus.State);
            }
            ValidateSessionResult(sessionStatus, requestedCertificateLevel);
        }

        private void ValidateSessionResult(SessionStatus sessionStatus, CertificateLevel requestedCertificateLevel)
        {
            SessionResult sessionResult = sessionStatus.Result;
            if (sessionResult == null)
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'result' is missing");
            }
            if (StringUtil.IsEmpty(sessionResult.EndResult))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'result.endResult' is empty");
            }
            if ("OK".Equals(sessionResult.EndResult, StringComparison.OrdinalIgnoreCase))
            {
                if (StringUtil.IsEmpty(sessionResult.DocumentNumber))
                {
                    throw new UnprocessableSmartIdResponseException("Signature session status field 'result.documentNumber' is empty");
                }
                if (StringUtil.IsEmpty(sessionStatus.InteractionTypeUsedOrLegacy))
                {
                    throw new UnprocessableSmartIdResponseException("Signature session status field 'interactionTypeUsed' is empty");
                }
                if (StringUtil.IsEmpty(sessionStatus.SignatureProtocol))
                {
                    throw new UnprocessableSmartIdResponseException("Signature session status field 'signatureProtocol' is empty");
                }
                ValidateCertificate(sessionStatus.Cert, requestedCertificateLevel);
                ValidateSignature(sessionStatus);
            }
            else
            {
                ErrorResultHandler.Handle(sessionResult);
            }
        }

        private void ValidateCertificate(SessionCertificate sessionCertificate, CertificateLevel requestedCertificateLevel)
        {
            if (sessionCertificate == null)
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'cert' is missing");
            }
            if (StringUtil.IsEmpty(sessionCertificate.Value))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'cert.value' is empty");
            }
            if (StringUtil.IsEmpty(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'cert.certificateLevel' is empty");
            }
            if (!CertificateLevelExtensions.IsSupported(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'cert.certificateLevel' has unsupported value");
            }
            if (!CertificateLevelExtensions.TryParse(sessionCertificate.CertificateLevel, out var certificateLevel))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'cert.certificateLevel' has unsupported value");
            }
            if (!certificateLevel.IsSameLevelOrHigher(requestedCertificateLevel))
            {
                throw new CertificateLevelMismatchException();
            }
            X509Certificate2 x509 = ParseAndCheckCertificate(sessionCertificate.Value);
            certificateValidator.Validate(x509);
            ISignatureCertificatePurposeValidator purposeValidator = signatureCertificatePurposeValidatorFactory.Create(certificateLevel);
            purposeValidator.Validate(x509);
        }

        private static X509Certificate2 ParseAndCheckCertificate(string certBase64)
        {
            X509Certificate2 certificate = CertificateParser.ParseX509Certificate(certBase64);
            var utc = DateTime.UtcNow;
            if (utc < certificate.NotBefore.ToUniversalTime() || utc > certificate.NotAfter.ToUniversalTime())
            {
                throw new UnprocessableSmartIdResponseException("Signature certificate is invalid");
            }
            return certificate;
        }

        private static void ValidateSignature(SessionStatus sessionStatus)
        {
            string signatureProtocol = sessionStatus.SignatureProtocol;
            if (!string.Equals(signatureProtocol, SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signatureProtocol' has unsupported value");
            }
            ValidateRawDigestSignature(sessionStatus);
        }

        private static void ValidateRawDigestSignature(SessionStatus sessionStatus)
        {
            SessionSignature signature = sessionStatus.Signature;
            if (signature == null)
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'signature' is missing");
            }
            ValidateSignatureValue(signature.Value);
            ValidateSignatureAlgorithmName(signature.SignatureAlgorithmOrLegacy);
            ValidateFlowType(signature.FlowType);
            if (!SigningSignatureAlgorithmExtensions.IsLegacyRsaApiAlgorithmName(signature.SignatureAlgorithmOrLegacy))
            {
                ValidateSignatureAlgorithmParameters(signature.SignatureAlgorithmParameters);
            }
        }

        private static void ValidateSignatureValue(string value)
        {
            if (StringUtil.IsEmpty(value))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field 'signature.value' is empty");
            }
            if (!SignatureValueBase64Pattern.IsMatch(value))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.value' does not have Base64-encoded value");
            }
        }

        private static void ValidateSignatureAlgorithmName(string signatureAlgorithm)
        {
            if (StringUtil.IsEmpty(signatureAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithm' is missing");
            }
            if (!SigningSignatureAlgorithmExtensions.IsSupportedApiAlgorithmName(signatureAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithm' has unsupported value");
            }
        }

        private static void ValidateFlowType(string flowType)
        {
            if (StringUtil.IsEmpty(flowType))
            {
                throw new UnprocessableSmartIdResponseException("Signature session status field `signature.flowType` is empty");
            }
            if (!FlowTypeExtensions.IsSupportedApiValue(flowType))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.flowType' has unsupported value");
            }
        }

        private static void ValidateSignatureAlgorithmParameters(SessionSignatureAlgorithmParameters sessionSignatureAlgorithmParameters)
        {
            if (sessionSignatureAlgorithmParameters == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters' is missing");
            }
            if (StringUtil.IsEmpty(sessionSignatureAlgorithmParameters.HashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' is empty");
            }
            if (!SmartIdHashAlgorithmExtensions.TryParse(sessionSignatureAlgorithmParameters.HashAlgorithm, out var hashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' has unsupported value");
            }
            SessionMaskGenAlgorithm maskGenAlgorithm = sessionSignatureAlgorithmParameters.MaskGenAlgorithm;
            if (maskGenAlgorithm == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' is missing");
            }
            if (StringUtil.IsEmpty(maskGenAlgorithm.Algorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' is empty");
            }
            if (!MaskGenAlgorithm.IsSupportedApiOrLegacyMgfName(maskGenAlgorithm.Algorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' has unsupported value");
            }
            if (maskGenAlgorithm.Parameters == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters' is missing");
            }
            if (StringUtil.IsEmpty(maskGenAlgorithm.Parameters.HashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' is empty");
            }
            if (!SmartIdHashAlgorithmExtensions.TryParse(maskGenAlgorithm.Parameters.HashAlgorithm, out var mgfHashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' has unsupported value");
            }
            if (hashAlgorithm != mgfHashAlgorithm)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' value does not match 'signature.signatureAlgorithmParameters.hashAlgorithm' value");
            }
            if (!sessionSignatureAlgorithmParameters.SaltLength.HasValue)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.saltLength' is missing");
            }
            int expectedSaltLength = hashAlgorithm.GetDigestOctetLength();
            int actualSaltLength = sessionSignatureAlgorithmParameters.SaltLength.Value;
            if (expectedSaltLength != actualSaltLength)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature session status field 'signature.signatureAlgorithmParameters.saltLength' has invalid value");
            }
            if (StringUtil.IsEmpty(sessionSignatureAlgorithmParameters.TrailerField))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature status field `signature.signatureAlgorithmParameters.trailerField` is empty");
            }
            if (!TrailerField.TryParse(sessionSignatureAlgorithmParameters.TrailerField, out _))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Signature status field `signature.signatureAlgorithmParameters.trailerField` has unsupported value");
            }
        }
    }
}
