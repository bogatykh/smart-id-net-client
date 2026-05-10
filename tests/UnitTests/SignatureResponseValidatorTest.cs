/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using Xunit;

namespace SK.SmartId
{
    public class SignatureResponseValidatorTest
    {
        private const string NqSignatureValue =
            "NVGdK0YNpyKWEK5YhyrZt0rjtczzlsSi9tw2KS8iw13cZbiPwCr1/v35By7KkGtZ7fY+s9ebG9NbiIldnJ+wtqgjI4ZlDMRsoepgMsNPQD66kAPObUylv7NdZ41O0i/RB8DUYHcd5RHnYhqN9wPdd4iNtzfkMhqlJsZLT4cYOV1cNIfQSQnHOekA8Qbq1CASt2i7i8cIQ2v5+CfFwmSBdkZGrInVlbptLK4pKpX7kYjzQ9sq+1ua9A+6ZHBE/nCdw/Oa0jXsnM3E1KDDQzSO5qafkW4LzEpGvaRn4lRXPxPmgg0m7z5TEZa0VXhBPr9qvBI7SDQDov4OMUku6WyKdEb+4qC9lR+u+T2drpPe4W9vdKodzjL/kalMyHITW4bfl9szMSdz0EF6oDUjwkNyzaUdms8kODLOkWKHMQjLK7/s00VHbt9i0uHERdUwU78XsnTBjw6oM0R1/WVdPu7FOzF/nETOZiWmziycieFj4Y2hhaPn2S/PmGqXcNpWipXw2kdVNRL+Kn7ryiz4ojXp7U2+0ZUi2r6nyt/AR/hbowSwbCn8tKFssDTZacYSsjhdpcyD6tsy3yc7tQqSHXAgAIy3k6EFqvM0ehIO0HAGCsyY4iVUjDluz4Bd3jurERFtu6GnLwGpX8fPh/CgvQh8O1XwI23cwe/Ojn6i7J155TL107kczNv1pD8oppTAd7Oe8bZCI7YDqEhFGwMpEeiSb80V5Deg3LwCYlQtenl04vFol+9Vij22RJpVvssTi0fJ8Vxgzm3Xtoak/R0U9fHiFsGB/eVrM3h27twztYwU49ti/ZYs/7Ow+RZGq7Kbr6KXyxdh9j7Mva5x5NBr2x6kJFBbJKjj0o+FRZJX6YTraup975+Oxvp13WICAPTtdNvRCkVoXKFOFjG040b4TFsPdny+iY3PBx4wTef/b4GX22MlAjVtBgw4x+XRoPO9F6X5wYFlw2UPLY0vPltWOXarR/AyXqyxBigiS/Sho090pH7nD6YZ2s7bp9jnqtWnzqWb";

        private static readonly ICertificateValidator ValidityOnlyTrust = new TestValidityOnlyCertificateValidator();

        private readonly SignatureResponseValidator signatureResponseValidator =
            new SignatureResponseValidator(ValidityOnlyTrust);

        [Fact]
        public void Validate_validRawDigestSignature()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";

            SignatureResponse response = signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED);
            Assert.Equal("OK", response.EndResult);
        }

        [Theory]
        [MemberData(nameof(CertificateChoiceResponseValidatorTest.QualifiedOrQscdLevelNames), MemberType = typeof(CertificateChoiceResponseValidatorTest))]
        public void Validate_returnedCertificateLevelSameAsRequested(string requestedCertificateLevelName)
        {
            var certificateLevel = Enum.Parse<CertificateLevel>(requestedCertificateLevelName);
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";

            SignatureResponse response = signatureResponseValidator.Validate(sessionStatus, certificateLevel);
            Assert.Equal("OK", response.EndResult);
            Assert.Equal(CertificateLevel.QUALIFIED, response.CertificateLevel);
        }

        public static IEnumerable<object[]> FlowTypeCases
        {
            get
            {
                foreach (FlowType f in Enum.GetValues(typeof(FlowType)))
                {
                    yield return new object[] { f };
                }
            }
        }

        [Theory]
        [MemberData(nameof(FlowTypeCases))]
        public void Validate_flowTypesAreSupported(FlowType flowType)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss", flowType);
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";

            SignatureResponse response = signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED);
            Assert.Equal("OK", response.EndResult);
        }

        [Fact]
        public void Validate_nqSigning_ok()
        {
            SessionStatus sessionStatus = ToNqSignatureSessionStatus();
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";

            SignatureResponse response = signatureResponseValidator.Validate(sessionStatus, CertificateLevel.ADVANCED);
            Assert.Equal("OK", response.EndResult);
        }

        [Theory]
        [InlineData(SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION)]
        public void Validate_legacyRsaSignature_ok_withoutSignatureAlgorithmParameters(SigningSignatureAlgorithm signatureAlgorithm)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", signatureAlgorithm.GetAlgorithmName());
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";
            sessionStatus.Signature.SignatureAlgorithmParameters = null;

            SignatureResponse response = signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED);

            Assert.Equal("OK", response.EndResult);
            Assert.Equal(signatureAlgorithm.GetAlgorithmName(), response.AlgorithmName);
            Assert.Equal(signatureAlgorithm, response.SignatureAlgorithm);
            Assert.Null(response.RsaSsaPssParameters);
        }

        [Fact]
        public void Validate_stateParameterMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.State = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'state' is empty", ex.Message);
        }

        [Fact]
        public void Validate_sessionNotComplete()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.State = "RUNNING";

            var ex = Assert.Throws<SmartIdClientException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Contains("Session is not complete", ex.Message);
        }

        [Fact]
        public void Validate_sessionResultNull()
        {
            var sessionStatus = new SessionStatus { State = "COMPLETE", Result = null };

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'result' is missing", ex.Message);
        }

        [Fact]
        public void Validate_missingDocumentNumber()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Result.DocumentNumber = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'result.documentNumber' is empty", ex.Message);
        }

        [Fact]
        public void Validate_missingInteractionFlowUsed()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.InteractionTypeUsed = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'interactionTypeUsed' is empty", ex.Message);
        }

        [Fact]
        public void Validate_signatureProtocolMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.SignatureProtocol = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signatureProtocol' is empty", ex.Message);
        }

        [Fact]
        public void CertificateValidation_missingCertificate()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Cert = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'cert' is missing", ex.Message);
        }

        [Fact]
        public void CertificateValidation_missingCertificateValue()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Cert.Value = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'cert.value' is empty", ex.Message);
        }

        [Fact]
        public void CertificateValidation_certificateLevelMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Cert.CertificateLevel = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'cert.certificateLevel' is empty", ex.Message);
        }

        [Fact]
        public void CertificateValidation_certificateLevelMismatch()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Cert.CertificateLevel = "ADVANCED";

            var ex = Assert.Throws<CertificateLevelMismatchException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signer's certificate is below requested certificate level", ex.Message);
        }

        [Fact]
        public void SignatureValidation_rawDigestUnexpectedAlgorithm()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";
            sessionStatus.Signature.SignatureAlgorithm = "unexpectedAlgorithm";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureValidation_unknownSignatureProtocol()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("UNKNOWN_PROTOCOL", "rsassa-pss");
            sessionStatus.SignatureProtocol = "UNKNOWN_PROTOCOL";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signatureProtocol' has unsupported value", ex.Message);
        }

        public static IEnumerable<object[]> SessionEndResultErrorCases => CertificateChoiceResponseValidatorTest.SessionEndResultErrorCases;

        [Theory]
        [MemberData(nameof(SessionEndResultErrorCases))]
        public void SignatureValidation_handleSessionEndResultErrors(string endResult, Type expectedException)
        {
            var sessionResult = new SessionResult { EndResult = endResult };
            var sessionStatus = new SessionStatus { State = "COMPLETE", Result = sessionResult };

            Assert.Throws(expectedException, () => signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
        }

        public static IEnumerable<object[]> UserRefusedInteractionCases => CertificateChoiceResponseValidatorTest.UserRefusedInteractionCases;

        [Theory]
        [MemberData(nameof(UserRefusedInteractionCases))]
        public void SignatureValidation_endResultIsUserRefusedInteraction(string interaction, Type expectedException)
        {
            var sessionResultDetails = new SessionResultDetails { Interaction = interaction };
            var sessionResult = new SessionResult
            {
                EndResult = "USER_REFUSED_INTERACTION",
                Details = sessionResultDetails
            };
            var sessionStatus = new SessionStatus { State = "COMPLETE", Result = sessionResult };

            Assert.Throws(expectedException, () => signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
        }

        [Fact]
        public void SignatureValidation_endResultMissing_throwsException()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.SignatureProtocol = "RAW_DIGEST_SIGNATURE";
            sessionStatus.Result.EndResult = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'result.endResult' is empty", ex.Message);
        }

        [Fact]
        public void SignatureValidation_sessionStatusNull()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => signatureResponseValidator.Validate(null, CertificateLevel.QUALIFIED));
            Assert.Equal("Parameter 'sessionStatus' is not provided", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature' is missing", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureValueMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.Value = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.value' is empty", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureValueIsNotInBase64EncodedFormat_throwException()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.Value = "invalid-not+encoded+value";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.value' does not have Base64-encoded value", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureAlgorithmMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithm = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithm' is missing", ex.Message);
        }

        [Theory]
        [InlineData("SHA-1")]
        [InlineData("invalid")]
        public void SignatureValidation_invalidSignatureAlgorithmIsProvided(string invalidSignatureAlgorithm)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithm = invalidSignatureAlgorithm;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureValidation_flowTypeMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.FlowType = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field `signature.flowType` is empty", ex.Message);
        }

        [Fact]
        public void SignatureValidation_invalidFlowType()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.FlowType = "UNSUPPORTED_FLOW";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.flowType' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureAlgorithmNotSupported()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "unsupported-algorithm");

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureValidation_signatureAlgorithmNotRsassaPss()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsa");
            sessionStatus.Signature.SignatureAlgorithm = "rsa";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_signatureAlgorithmParametersMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters' is missing", ex.Message);
        }

        [Theory]
        [InlineData("")]
        public void SignatureAlgorithmParameters_hashAlgorithmMissing_empty(string hashAlgorithm)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.HashAlgorithm = hashAlgorithm;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_hashAlgorithmMissing_null()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.HashAlgorithm = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_invalidHashAlgorithm()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.HashAlgorithm = "INVALID-HASH";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_maskGenAlgorithmIsMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' is missing", ex.Message);
        }

        [Theory]
        [InlineData("")]
        public void SignatureAlgorithmParameters_maskGenAlgorithmAlgorithmIsEmpty(string algorithm)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Algorithm = algorithm;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_maskGenAlgorithmAlgorithmIsNull()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Algorithm = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_invalidMaskGenAlgorithmName()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Algorithm = "INVALID";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_maskGenHashAlgorithmParametersAreMissing_throwException()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters' is missing", ex.Message);
        }

        [Theory]
        [InlineData("")]
        public void SignatureAlgorithmParameters_hashAlgorithmInMaskGenHashAlgorithmParametersIsEmpty(string hashAlgorithm)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters.HashAlgorithm = hashAlgorithm;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_hashAlgorithmInMaskGenHashAlgorithmParametersIsNull()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters.HashAlgorithm = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_maskGenHashAlgorithmInvalid()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters.HashAlgorithm = "INVALID-HASH";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' has unsupported value", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_mismatchedHashAlgorithms()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters.HashAlgorithm = "SHA-256";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal(
                "Signature session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' value does not match 'signature.signatureAlgorithmParameters.hashAlgorithm' value",
                ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_saltLengthIsMissing()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.SaltLength = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.saltLength' is missing", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_invalidSaltLength()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.SaltLength = 32;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature session status field 'signature.signatureAlgorithmParameters.saltLength' has invalid value", ex.Message);
        }

        [Theory]
        [InlineData("")]
        public void SignatureAlgorithmParameters_signatureAlgorithmParametersTrailerFieldEmptyOrNull(string trailerField)
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.TrailerField = trailerField;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature status field `signature.signatureAlgorithmParameters.trailerField` is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_signatureAlgorithmParametersTrailerFieldNull()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.TrailerField = null;

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature status field `signature.signatureAlgorithmParameters.trailerField` is empty", ex.Message);
        }

        [Fact]
        public void SignatureAlgorithmParameters_invalidTrailerField()
        {
            SessionStatus sessionStatus = ToQualifiedSignatureSessionStatus("RAW_DIGEST_SIGNATURE", "rsassa-pss");
            sessionStatus.Signature.SignatureAlgorithmParameters.TrailerField = "0xab";

            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                signatureResponseValidator.Validate(sessionStatus, CertificateLevel.QUALIFIED));
            Assert.Equal("Signature status field `signature.signatureAlgorithmParameters.trailerField` has unsupported value", ex.Message);
        }

        private static SessionStatus ToQualifiedSignatureSessionStatus(string signatureProtocol, string signatureAlgorithm)
        {
            return ToQualifiedSignatureSessionStatus(signatureProtocol, signatureAlgorithm, FlowType.QR);
        }

        private static SessionStatus ToQualifiedSignatureSessionStatus(string signatureProtocol, string signatureAlgorithm, FlowType flowType)
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-12345678901"
            };
            var sessionCertificate = new SessionCertificate
            {
                CertificateLevel = "QUALIFIED",
                Value = TestCertificateUtil.GetEncodedCertificateData("sign-cert-40504040001.pem.crt")
            };
            SessionSignatureAlgorithmParameters parameters = ToSessionSignatureAlgorithmParams();
            SessionSignature sessionSignature = ToSessionSignature("expectedDigest", signatureAlgorithm, parameters, flowType);

            return new SessionStatus
            {
                State = "COMPLETE",
                Result = sessionResult,
                Cert = sessionCertificate,
                Signature = sessionSignature,
                SignatureProtocol = signatureProtocol,
                InteractionTypeUsed = "displayTextAndPIN"
            };
        }

        private static SessionStatus ToNqSignatureSessionStatus()
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-12345678901"
            };
            var sessionCertificate = new SessionCertificate
            {
                CertificateLevel = "ADVANCED",
                Value = TestCertificateUtil.GetEncodedCertificateData("nq-signing-cert.pem")
            };
            SessionSignatureAlgorithmParameters parameters = ToSessionSignatureAlgorithmParams();
            SessionSignature sessionSignature = ToSessionSignature(
                NqSignatureValue,
                SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                parameters,
                FlowType.QR);

            return new SessionStatus
            {
                State = "COMPLETE",
                Result = sessionResult,
                Cert = sessionCertificate,
                Signature = sessionSignature,
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                InteractionTypeUsed = "displayTextAndPIN"
            };
        }

        private static SessionSignature ToSessionSignature(
            string signatureValue,
            string signatureAlgorithm,
            SessionSignatureAlgorithmParameters parameters,
            FlowType flowType)
        {
            return new SessionSignature
            {
                Value = signatureValue,
                SignatureAlgorithm = signatureAlgorithm,
                SignatureAlgorithmParameters = parameters,
                ServerRandom = "serverRandomValue",
                UserChallenge = "QWxwaGFFenItMTIzNDU2Nzg5MDEyMzQ1Njc4OTAx",
                FlowType = flowType.GetApiValue()
            };
        }

        private static SessionSignatureAlgorithmParameters ToSessionSignatureAlgorithmParams()
        {
            var mgfParams = new SessionMaskGenAlgorithmParameters { HashAlgorithm = "SHA-512" };
            var mgf = new SessionMaskGenAlgorithm
            {
                Algorithm = "id-mgf1",
                Parameters = mgfParams
            };
            return new SessionSignatureAlgorithmParameters
            {
                HashAlgorithm = "SHA-512",
                MaskGenAlgorithm = mgf,
                SaltLength = 64,
                TrailerField = "0xbc"
            };
        }
    }
}
