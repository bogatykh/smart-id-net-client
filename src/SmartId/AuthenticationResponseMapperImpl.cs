/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Util;
using System;
using System.Text.RegularExpressions;

namespace SK.SmartId
{
    /// <summary>Validates and maps session status to <see cref="AuthenticationResponse"/> (Java <c>AuthenticationResponseMapperImpl</c>).</summary>
    public sealed class AuthenticationResponseMapperImpl : IAuthenticationResponseMapper
    {
        private const string UserChallengePattern = "^[a-zA-Z0-9-_]{43}$";
        private const string Base64FormatPattern = "^[a-zA-Z0-9+/]+={0,2}$";
        private const int MinimumServerRandomLength = 24;
        private static readonly Regex UserChallengeRegex = new Regex(UserChallengePattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex Base64FormatRegex = new Regex(Base64FormatPattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public AuthenticationResponse From(SessionStatus sessionStatus)
        {
            ValidateSessionStatus(sessionStatus);

            SessionResult sessionResult = sessionStatus.Result;
            SessionSignature sessionSignature = sessionStatus.Signature;
            SessionCertificate sessionCertificate = sessionStatus.Cert;

            SessionSignatureAlgorithmParameters p = sessionSignature.SignatureAlgorithmParameters;
            SmartIdHashAlgorithmExtensions.TryParse(p.HashAlgorithm, out var digestHash);
            SmartIdHashAlgorithmExtensions.TryParse(p.MaskGenAlgorithm.Parameters.HashAlgorithm, out var maskHash);

            var rsa = new RsaSsaPssParameters
            {
                DigestHashAlgorithm = digestHash,
                MaskGenAlgorithm = p.MaskGenAlgorithm.Algorithm,
                MaskHashAlgorithm = maskHash,
                SaltLength = p.SaltLength.Value,
                TrailerField = p.TrailerField
            };

            return new AuthenticationResponse
            {
                EndResult = sessionResult.EndResult,
                DocumentNumber = sessionResult.DocumentNumber,
                ServerRandom = sessionSignature.ServerRandom,
                UserChallenge = sessionSignature.UserChallenge,
                FlowType = FlowTypeExtensions.ParseApiValue(sessionSignature.FlowType),
                SignatureValueInBase64 = sessionSignature.Value,
                RsaSsaPssSignatureParameters = rsa,
                Certificate = CertificateParser.ParseX509Certificate(sessionCertificate.Value),
                CertificateLevel = (AuthenticationCertificateLevel)Enum.Parse(typeof(AuthenticationCertificateLevel), sessionCertificate.CertificateLevel),
                InteractionTypeUsed = sessionStatus.InteractionTypeUsed,
                DeviceIpAddress = sessionStatus.DeviceIpAddress
            };
        }

        private static void ValidateSessionStatus(SessionStatus sessionStatus)
        {
            if (sessionStatus == null)
            {
                throw new SmartIdClientException("Parameter 'sessionsStatus' is not provided");
            }

            ValidateResult(sessionStatus.Result);
            ValidateSignatureProtocol(sessionStatus);
            ValidateSignature(sessionStatus.Signature);
            ValidateCertificate(sessionStatus.Cert);

            if (StringUtil.IsEmpty(sessionStatus.InteractionTypeUsed))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'interactionTypeUsed' is empty");
            }
        }

        private static void ValidateResult(SessionResult sessionResult)
        {
            if (sessionResult == null)
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'result' is empty");
            }
            if (StringUtil.IsEmpty(sessionResult.EndResult))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'result.endResult' is empty");
            }
            if (!string.Equals(sessionResult.EndResult, "OK", StringComparison.Ordinal))
            {
                ErrorResultHandler.Handle(sessionResult);
            }
            if (StringUtil.IsEmpty(sessionResult.DocumentNumber))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'result.documentNumber' is empty");
            }
        }

        private static void ValidateSignatureProtocol(SessionStatus sessionStatus)
        {
            if (StringUtil.IsEmpty(sessionStatus.SignatureProtocol))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signatureProtocol' is empty");
            }
            if (!string.Equals(sessionStatus.SignatureProtocol, SignatureProtocol.ACSP_V2.ToString(), StringComparison.Ordinal))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signatureProtocol' has unsupported value");
            }
        }

        private static void ValidateSignature(SessionSignature sessionSignature)
        {
            if (sessionSignature == null)
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signature' is missing");
            }
            if (StringUtil.IsEmpty(sessionSignature.Value))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signature.value' is empty");
            }
            if (!Base64FormatRegex.IsMatch(sessionSignature.Value))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.value' does not have Base64-encoded value");
            }
            if (StringUtil.IsEmpty(sessionSignature.ServerRandom))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signature.serverRandom' is empty");
            }
            if (sessionSignature.ServerRandom.Length < MinimumServerRandomLength)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.serverRandom' value length is less than required");
            }
            if (!Base64FormatRegex.IsMatch(sessionSignature.ServerRandom))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.serverRandom' does not have Base64-encoded value");
            }
            if (StringUtil.IsEmpty(sessionSignature.UserChallenge))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signature.userChallenge' is empty");
            }
            if (!UserChallengeRegex.IsMatch(sessionSignature.UserChallenge))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.userChallenge' value does not match required pattern");
            }
            if (StringUtil.IsEmpty(sessionSignature.FlowType))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'signature.flowType' is empty");
            }
            if (!FlowTypeExtensions.IsSupportedApiValue(sessionSignature.FlowType))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.flowType' has unsupported value");
            }
            if (StringUtil.IsEmpty(sessionSignature.SignatureAlgorithmOrLegacy))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithm' is empty");
            }
            if (!AuthenticationSignatureAlgorithmExtensions.IsSupportedApiAlgorithmName(sessionSignature.SignatureAlgorithmOrLegacy))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithm' has unsupported value");
            }
            ValidateSignatureAlgorithmParameters(sessionSignature);
        }

        private static void ValidateSignatureAlgorithmParameters(SessionSignature sessionSignature)
        {
            SessionSignatureAlgorithmParameters signatureAlgorithmParameters = sessionSignature.SignatureAlgorithmParameters;
            if (signatureAlgorithmParameters == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters' is missing");
            }
            if (StringUtil.IsEmpty(signatureAlgorithmParameters.HashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' is empty");
            }
            if (!SmartIdHashAlgorithmExtensions.TryParse(signatureAlgorithmParameters.HashAlgorithm, out var hashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' has unsupported value");
            }
            SessionMaskGenAlgorithm maskGenAlgorithm = signatureAlgorithmParameters.MaskGenAlgorithm;
            if (maskGenAlgorithm == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' is missing");
            }
            if (StringUtil.IsEmpty(maskGenAlgorithm.Algorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' is empty");
            }
            if (!MaskGenAlgorithm.TryParse(maskGenAlgorithm.Algorithm, out _))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' has unsupported value");
            }
            if (maskGenAlgorithm.Parameters == null)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters' is missing");
            }
            if (StringUtil.IsEmpty(maskGenAlgorithm.Parameters.HashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' is empty");
            }
            if (!SmartIdHashAlgorithmExtensions.TryParse(maskGenAlgorithm.Parameters.HashAlgorithm, out var maskGenHashAlgorithm))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' has unsupported value");
            }
            if (hashAlgorithm != maskGenHashAlgorithm)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' value does not match 'signature.signatureAlgorithmParameters.hashAlgorithm' value");
            }
            if (!signatureAlgorithmParameters.SaltLength.HasValue)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.saltLength' is empty");
            }
            int octetLength = hashAlgorithm.GetDigestOctetLength();
            if (octetLength != signatureAlgorithmParameters.SaltLength.Value)
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.saltLength' has invalid value");
            }
            if (StringUtil.IsEmpty(signatureAlgorithmParameters.TrailerField))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.trailerField' is empty");
            }
            if (!TrailerField.TryParse(signatureAlgorithmParameters.TrailerField, out _))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'signature.signatureAlgorithmParameters.trailerField' has unsupported value");
            }
        }

        private static void ValidateCertificate(SessionCertificate sessionCertificate)
        {
            if (sessionCertificate == null)
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'cert' is missing");
            }
            if (StringUtil.IsEmpty(sessionCertificate.Value))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'cert.value' is empty");
            }
            if (StringUtil.IsEmpty(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException("Authentication session status field 'cert.certificateLevel' is empty");
            }
            if (!AuthenticationCertificateLevelExtensions.IsSupported(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Authentication session status field 'cert.certificateLevel' has unsupported value");
            }
        }
    }
}
