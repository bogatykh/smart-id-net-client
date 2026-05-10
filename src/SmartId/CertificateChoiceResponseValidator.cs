/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Util;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Maps and validates certificate-choice <see cref="SessionStatus"/> (Java <c>CertificateChoiceResponseValidator</c>).
    /// </summary>
    public sealed class CertificateChoiceResponseValidator
    {
        private readonly ICertificateValidator certificateValidator;
        private readonly ISignatureCertificatePurposeValidatorFactory signatureCertificatePurposeValidatorFactory;

        public CertificateChoiceResponseValidator(ICertificateValidator certificateValidator)
            : this(certificateValidator, new SignatureCertificatePurposeValidatorFactoryImpl())
        {
        }

        public CertificateChoiceResponseValidator(
            ICertificateValidator certificateValidator,
            ISignatureCertificatePurposeValidatorFactory signatureCertificatePurposeValidatorFactory)
        {
            this.certificateValidator = certificateValidator;
            this.signatureCertificatePurposeValidatorFactory = signatureCertificatePurposeValidatorFactory;
        }

        public CertificateChoiceResponse Validate(SessionStatus sessionStatus)
        {
            return Validate(sessionStatus, CertificateLevel.QUALIFIED);
        }

        public CertificateChoiceResponse Validate(SessionStatus sessionStatus, CertificateLevel? requestedCertificateLevel)
        {
            if (sessionStatus == null)
            {
                throw new SmartIdClientException("Parameter 'sessionStatus' is not provided");
            }
            if (!requestedCertificateLevel.HasValue)
            {
                throw new SmartIdClientException("Parameter 'requestedCertificateLevel' is not provided");
            }
            ValidateResult(sessionStatus.Result);
            SessionCertificate sessionCertificate = sessionStatus.Cert;
            ValidateSessionStatusCertificate(sessionCertificate);
            if (!CertificateLevelExtensions.TryParse(sessionCertificate.CertificateLevel, out var certificateLevel))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate choice session status field 'cert.certificateLevel' has unsupported value");
            }
            X509Certificate2 certificate = GetValidateX509Certificate(
                sessionCertificate.Value, certificateLevel, requestedCertificateLevel.Value);
            return ToCertificateChoiceResponse(sessionStatus, certificate, certificateLevel);
        }

        private X509Certificate2 GetValidateX509Certificate(
            string certificateBase64,
            CertificateLevel certificateLevel,
            CertificateLevel requestedCertificateLevel)
        {
            if (!certificateLevel.IsSameLevelOrHigher(requestedCertificateLevel))
            {
                throw new CertificateLevelMismatchException(
                    "Certificate choice session status response certificate level is lower than requested");
            }
            X509Certificate2 certificate = CertificateParser.ParseX509Certificate(certificateBase64);
            certificateValidator.Validate(certificate);
            ISignatureCertificatePurposeValidator purposeValidator = signatureCertificatePurposeValidatorFactory.Create(certificateLevel);
            purposeValidator.Validate(certificate);
            return certificate;
        }

        private static void ValidateResult(SessionResult sessionResult)
        {
            if (sessionResult == null)
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'result' is missing");
            }
            if (StringUtil.IsEmpty(sessionResult.EndResult))
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'result.endResult' is empty");
            }
            if (!"OK".Equals(sessionResult.EndResult, System.StringComparison.OrdinalIgnoreCase))
            {
                ErrorResultHandler.Handle(sessionResult);
            }
            if (StringUtil.IsEmpty(sessionResult.DocumentNumber))
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'result.documentNumber' is empty");
            }
        }

        private static void ValidateSessionStatusCertificate(SessionCertificate sessionCertificate)
        {
            if (sessionCertificate == null)
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'cert' is missing");
            }
            if (StringUtil.IsEmpty(sessionCertificate.Value))
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'cert.value' has empty value");
            }
            if (StringUtil.IsEmpty(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException("Certificate choice session status field 'cert.certificateLevel' has empty value");
            }
            if (!CertificateLevelExtensions.IsSupported(sessionCertificate.CertificateLevel))
            {
                throw new UnprocessableSmartIdResponseException(
                    "Certificate choice session status field 'cert.certificateLevel' has unsupported value");
            }
        }

        private static CertificateChoiceResponse ToCertificateChoiceResponse(
            SessionStatus sessionStatus,
            X509Certificate2 certificate,
            CertificateLevel certificateLevel)
        {
            return new CertificateChoiceResponse
            {
                EndResult = sessionStatus.Result.EndResult,
                DocumentNumber = sessionStatus.Result.DocumentNumber,
                Certificate = certificate,
                CertificateLevel = certificateLevel,
                InteractionFlowUsed = sessionStatus.InteractionTypeUsedOrLegacy,
                DeviceIpAddress = sessionStatus.DeviceIpAddress
            };
        }
    }
}
