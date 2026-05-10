/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Auth;
using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Util;
using System;
using System.Text;

namespace SK.SmartId
{
    /// <summary>Validates notification-based authentication session status (Java <c>NotificationAuthenticationResponseValidator</c>).</summary>
    public sealed class NotificationAuthenticationResponseValidator
    {
        private readonly ICertificateValidator certificateValidator;
        private readonly IAuthenticationResponseMapper authenticationResponseMapper;
        private readonly ISignatureValueValidator signatureValueValidator;
        private readonly IAuthenticationCertificatePurposeValidatorFactory authenticationCertificatePurposeValidatorFactory;

        public NotificationAuthenticationResponseValidator(
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

        public static NotificationAuthenticationResponseValidator DefaultSetupWithCertificateValidator(ICertificateValidator certificateValidator) =>
            new NotificationAuthenticationResponseValidator(
                certificateValidator,
                new AuthenticationResponseMapperImpl(),
                new SignatureValueValidatorImpl(),
                new AuthenticationCertificatePurposeValidatorFactoryImpl());

        public AuthenticationIdentity Validate(
            SessionStatus sessionStatus,
            NotificationAuthenticationSessionRequest authenticationSessionRequest,
            string schemaName) =>
            Validate(sessionStatus, authenticationSessionRequest, schemaName, null);

        public AuthenticationIdentity Validate(
            SessionStatus sessionStatus,
            NotificationAuthenticationSessionRequest authenticationSessionRequest,
            string schemaName,
            string brokeredRpName)
        {
            ValidateInputs(sessionStatus, authenticationSessionRequest, schemaName);
            AuthenticationResponse authenticationResponse = authenticationResponseMapper.From(sessionStatus);
            ValidateCertificate(authenticationResponse, GetRequestedCertificateLevel(authenticationSessionRequest));
            ValidateSignature(authenticationResponse, authenticationSessionRequest, schemaName, brokeredRpName);
            return AuthenticationIdentityMapper.From(authenticationResponse.Certificate);
        }

        private static void ValidateInputs(SessionStatus sessionStatus, NotificationAuthenticationSessionRequest authenticationSessionRequest, string schemaName)
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

        private static AuthenticationCertificateLevel GetRequestedCertificateLevel(NotificationAuthenticationSessionRequest authenticationSessionRequest) =>
            string.IsNullOrEmpty(authenticationSessionRequest.CertificateLevel)
                ? AuthenticationCertificateLevel.QUALIFIED
                : (AuthenticationCertificateLevel)Enum.Parse(typeof(AuthenticationCertificateLevel), authenticationSessionRequest.CertificateLevel);

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
            NotificationAuthenticationSessionRequest authenticationSessionRequest,
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
            NotificationAuthenticationSessionRequest authenticationSessionRequest,
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
                "",
                authenticationResponse.FlowType.GetApiValue()
            };
            return Encoding.UTF8.GetBytes(string.Join("|", payload));
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
