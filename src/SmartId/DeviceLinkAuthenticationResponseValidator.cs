/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Auth;
using SK.SmartId.Common;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Util;
using System;
using System.Text;

namespace SK.SmartId
{
    /// <summary>Validates device-link authentication session status (Java <c>DeviceLinkAuthenticationResponseValidator</c>).</summary>
    public sealed class DeviceLinkAuthenticationResponseValidator
    {
        private readonly ICertificateValidator certificateValidator;
        private readonly IAuthenticationResponseMapper authenticationResponseMapper;
        private readonly ISignatureValueValidator signatureValueValidator;
        private readonly IAuthenticationCertificatePurposeValidatorFactory authenticationCertificatePurposeValidatorFactory;

        public DeviceLinkAuthenticationResponseValidator(
            ICertificateValidator certificateValidator,
            IAuthenticationResponseMapper authenticationResponseMapper,
            ISignatureValueValidator signatureValueValidator,
            IAuthenticationCertificatePurposeValidatorFactory authenticationCertificatePurposeValidatorFactory)
        {
            this.certificateValidator = certificateValidator;
            this.authenticationResponseMapper = authenticationResponseMapper;
            this.signatureValueValidator = signatureValueValidator;
            this.authenticationCertificatePurposeValidatorFactory = authenticationCertificatePurposeValidatorFactory;
        }

        public static DeviceLinkAuthenticationResponseValidator DefaultSetupWithCertificateValidator(ICertificateValidator certificateValidator) =>
            new DeviceLinkAuthenticationResponseValidator(
                certificateValidator,
                new AuthenticationResponseMapperImpl(),
                new SignatureValueValidatorImpl(),
                new AuthenticationCertificatePurposeValidatorFactoryImpl());

        public AuthenticationIdentity Validate(
            SessionStatus sessionStatus,
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest,
            string userChallengeVerifier,
            string schemaName) =>
            Validate(sessionStatus, authenticationSessionRequest, userChallengeVerifier, schemaName, null);

        public AuthenticationIdentity Validate(
            SessionStatus sessionStatus,
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest,
            string userChallengeVerifier,
            string schemaName,
            string brokeredRpName)
        {
            ValidateInputs(sessionStatus, authenticationSessionRequest, schemaName);
            AuthenticationResponse authenticationResponse = authenticationResponseMapper.From(sessionStatus);
            ValidateUserChallenge(userChallengeVerifier, authenticationResponse);
            ValidateCertificate(authenticationResponse, GetRequestedCertificateLevel(authenticationSessionRequest));
            ValidateSignature(authenticationResponse, authenticationSessionRequest, schemaName, brokeredRpName);
            return AuthenticationIdentityMapper.From(authenticationResponse.Certificate);
        }

        private static void ValidateInputs(SessionStatus sessionStatus, DeviceLinkAuthenticationSessionRequest authenticationSessionRequest, string schemaName)
        {
            if (sessionStatus == null)
            {
                throw new SmartIdClientException("Parameter 'sessionStatus' is not provided");
            }
            if (authenticationSessionRequest == null)
            {
                throw new SmartIdClientException("Parameter 'authenticationSessionRequest' is not provided");
            }
            if (StringUtil.IsEmpty(schemaName))
            {
                throw new SmartIdClientException("Parameter 'schemaName' is not provided");
            }
        }

        private static AuthenticationCertificateLevel GetRequestedCertificateLevel(DeviceLinkAuthenticationSessionRequest authenticationSessionRequest) =>
            (AuthenticationCertificateLevel)Enum.Parse(typeof(AuthenticationCertificateLevel), authenticationSessionRequest.CertificateLevel);

        private void ValidateCertificate(AuthenticationResponse authenticationResponse, AuthenticationCertificateLevel requestedCertificateLevel)
        {
            ValidateCertificateLevel(authenticationResponse, requestedCertificateLevel);
            certificateValidator.Validate(authenticationResponse.Certificate);
            IAuthenticationCertificatePurposeValidator purposeValidator =
                authenticationCertificatePurposeValidatorFactory.Create(authenticationResponse.CertificateLevel);
            purposeValidator.Validate(authenticationResponse.Certificate);
        }

        private void ValidateSignature(
            AuthenticationResponse authenticationResponse,
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest,
            string schemaName,
            string brokeredRpName)
        {
            byte[] payload = ConstructPayload(authenticationResponse, authenticationSessionRequest, schemaName, brokeredRpName);
            signatureValueValidator.Validate(
                authenticationResponse.SignatureValue,
                payload,
                authenticationResponse.Certificate,
                new RsaSsaPssSignatureFactory(authenticationResponse.RsaSsaPssSignatureParameters));
        }

        private static byte[] ConstructPayload(
            AuthenticationResponse authenticationResponse,
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest,
            string schemaName,
            string brokeredRpName)
        {
            string[] payload =
            {
                schemaName,
                SignatureProtocol.ACSP_V2.ToString(),
                authenticationResponse.ServerRandom,
                authenticationSessionRequest.SignatureProtocolParameters.RpChallenge,
                StringUtil.OrEmpty(authenticationResponse.UserChallenge),
                ToStandardBase64Utf8(authenticationSessionRequest.RelyingPartyName),
                StringUtil.IsEmpty(brokeredRpName) ? "" : ToStandardBase64Utf8(brokeredRpName),
                InteractionUtil.CalculateDigest(authenticationSessionRequest.Interactions),
                authenticationResponse.InteractionTypeUsed,
                authenticationResponse.FlowType == FlowType.QR ? "" : (authenticationSessionRequest.InitialCallbackUrl ?? ""),
                authenticationResponse.FlowType.GetApiValue()
            };
            return Encoding.UTF8.GetBytes(string.Join("|", payload));
        }

        private static void ValidateUserChallenge(string userChallengeVerifier, AuthenticationResponse authenticationResponse)
        {
            if (authenticationResponse.FlowType != FlowType.Web2App && authenticationResponse.FlowType != FlowType.App2App)
            {
                return;
            }
            if (StringUtil.IsEmpty(userChallengeVerifier))
            {
                throw new SmartIdClientException(
                    "Parameter 'userChallengeVerifier' must be provided for 'flowType' - " + authenticationResponse.FlowType.GetApiValue());
            }
            string userChallenge = authenticationResponse.UserChallenge;
            string urlUserChallenge = ToUserChallengeDigest(userChallengeVerifier);
            if (!string.Equals(userChallenge, urlUserChallenge, StringComparison.Ordinal))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Device link authentication 'signature.userChallenge' does not validate with 'userChallengeVerifier'");
            }
        }

        private static string ToUserChallengeDigest(string userChallengeVerifier)
        {
            byte[] digest = DigestCalculator.CalculateDigest(Encoding.UTF8.GetBytes(userChallengeVerifier), SmartIdHashAlgorithm.SHA_256);
            return ToUrlSafeBase64WithoutPadding(digest);
        }

        private static string ToUrlSafeBase64WithoutPadding(byte[] data)
        {
            string s = Convert.ToBase64String(data).TrimEnd('=');
            return s.Replace('+', '-').Replace('/', '_');
        }

        private static void ValidateCertificateLevel(AuthenticationResponse authenticationResponse, AuthenticationCertificateLevel requestedCertificateLevel)
        {
            if (!authenticationResponse.CertificateLevel.IsSameLevelOrHigher(requestedCertificateLevel))
            {
                throw new CertificateLevelMismatchException();
            }
        }

        private static string ToStandardBase64Utf8(string input) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(input ?? ""));
    }
}
