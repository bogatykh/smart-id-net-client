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
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId
{
    public class CertificateChoiceResponseValidatorTest
    {
        private static readonly ICertificateValidator ValidityOnlyTrust = new TestValidityOnlyCertificateValidator();

        private readonly CertificateChoiceResponseValidator certificateChoiceResponseValidator =
            new CertificateChoiceResponseValidator(ValidityOnlyTrust);

        [Fact]
        public void Validate_ok()
        {
            SessionStatus sessionStatus = ToSessionStatus("cert-choice-cert-40504040001.pem.cert", "QUALIFIED");
            CertificateChoiceResponse response = certificateChoiceResponseValidator.Validate(sessionStatus);

            Assert.Equal("OK", response.EndResult);
            Assert.Equal("PNOEE-40504040001-MOCK-Q", response.DocumentNumber);
            AssertCertificatesEqual(TestCertificateUtil.ParseTestCertificate("cert-choice-cert-40504040001.pem.cert"), response.Certificate);
            Assert.Equal(CertificateLevel.QUALIFIED, response.CertificateLevel);
        }

        public static IEnumerable<object[]> QualifiedOrQscdLevelNames => new object[][]
        {
            new object[] { nameof(CertificateLevel.QUALIFIED) },
            new object[] { nameof(CertificateLevel.QSCD) }
        };

        [Theory]
        [MemberData(nameof(QualifiedOrQscdLevelNames))]
        public void Validate_returnedCertificateLevelSameAsRequested_ok(string requestedCertificateLevelName)
        {
            var requestedCertificateLevel = Enum.Parse<CertificateLevel>(requestedCertificateLevelName);
            SessionStatus sessionStatus = ToSessionStatus("cert-choice-cert-40504040001.pem.cert", "QUALIFIED");
            CertificateChoiceResponse response = certificateChoiceResponseValidator.Validate(sessionStatus, requestedCertificateLevel);

            Assert.Equal("OK", response.EndResult);
            Assert.Equal("PNOEE-40504040001-MOCK-Q", response.DocumentNumber);
            AssertCertificatesEqual(TestCertificateUtil.ParseTestCertificate("cert-choice-cert-40504040001.pem.cert"), response.Certificate);
            Assert.Equal(CertificateLevel.QUALIFIED, response.CertificateLevel);
        }

        [Fact]
        public void Validate_returnedCertificateHigherThanRequested_ok()
        {
            SessionStatus sessionStatus = ToSessionStatus("cert-choice-cert-40504040001.pem.cert", "QUALIFIED");
            CertificateChoiceResponse response = certificateChoiceResponseValidator.Validate(sessionStatus, CertificateLevel.ADVANCED);

            Assert.Equal("OK", response.EndResult);
            Assert.Equal("PNOEE-40504040001-MOCK-Q", response.DocumentNumber);
            AssertCertificatesEqual(TestCertificateUtil.ParseTestCertificate("cert-choice-cert-40504040001.pem.cert"), response.Certificate);
            Assert.Equal(CertificateLevel.QUALIFIED, response.CertificateLevel);
        }

        [Fact]
        public void Validate_nqCertificate()
        {
            SessionStatus sessionStatus = ToSessionStatus("nq-signing-cert.pem", "ADVANCED");
            CertificateChoiceResponse response = certificateChoiceResponseValidator.Validate(sessionStatus, CertificateLevel.ADVANCED);

            Assert.Equal("OK", response.EndResult);
            AssertCertificatesEqual(TestCertificateUtil.ParseTestCertificate("nq-signing-cert.pem"), response.Certificate);
            Assert.Equal(CertificateLevel.ADVANCED, response.CertificateLevel);
        }

        [Fact]
        public void ValidateInputs_sessionStatusNotProvided_throwException()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => certificateChoiceResponseValidator.Validate(null));
            Assert.Equal("Parameter 'sessionStatus' is not provided", ex.Message);
        }

        [Fact]
        public void ValidateInputs_requestCertificateLevelNotProvided_throwException()
        {
            SessionStatus sessionStatus = ToSessionStatus("cert-choice-cert-40504040001.pem.cert", "QUALIFIED");
            var ex = Assert.Throws<SmartIdClientException>(() => certificateChoiceResponseValidator.Validate(sessionStatus, null));
            Assert.Equal("Parameter 'requestedCertificateLevel' is not provided", ex.Message);
        }

        [Fact]
        public void ValidateEndResult_sessionResultIsNotProvided_throwException()
        {
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(new SessionStatus()));
            Assert.Equal("Certificate choice session status field 'result' is missing", ex.Message);
        }

        [Fact]
        public void ValidateEndResult_sessionEndResultIsNotProvided_throwException()
        {
            var sessionStatus = new SessionStatus { Result = new SessionResult() };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'result.endResult' is empty", ex.Message);
        }

        [Fact]
        public void ValidateEndResult_sessionDocumentNumberIsNotProvided_throwException()
        {
            var sessionResult = new SessionResult { EndResult = "OK" };
            var sessionStatus = new SessionStatus { Result = sessionResult };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'result.documentNumber' is empty", ex.Message);
        }

        [Theory]
        [MemberData(nameof(SessionEndResultErrorCases))]
        public void ValidateEndResult_sessionEndResultIsNotOk_throwException(string endResult, Type expectedException)
        {
            var sessionResult = new SessionResult { EndResult = endResult };
            var sessionStatus = new SessionStatus { Result = sessionResult };
            Assert.Throws(expectedException, () => certificateChoiceResponseValidator.Validate(sessionStatus));
        }

        [Theory]
        [MemberData(nameof(UserRefusedInteractionCases))]
        public void ValidateEndResult_endResultIsUserRefusedInteraction(string interaction, Type expectedException)
        {
            var sessionResultDetails = new SessionResultDetails { Interaction = interaction };
            var sessionResult = new SessionResult
            {
                EndResult = "USER_REFUSED_INTERACTION",
                Details = sessionResultDetails
            };
            var sessionStatus = new SessionStatus { State = "COMPLETE", Result = sessionResult };
            Assert.Throws(expectedException, () => certificateChoiceResponseValidator.Validate(sessionStatus));
        }

        [Fact]
        public void ValidateCertificate_sessionCertificateIsNotProvided_throwException()
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-40504040001-MOCK-Q"
            };
            var sessionStatus = new SessionStatus { Result = sessionResult };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'cert' is missing", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateCertificate_sessionCertificateValueIsNotProvided_throwException(string certificateValue)
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-40504040001-MOCK-Q"
            };
            var sessionCertificate = new SessionCertificate { Value = certificateValue };
            var sessionStatus = new SessionStatus { Result = sessionResult, Cert = sessionCertificate };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'cert.value' has empty value", ex.Message);
        }

        [Fact]
        public void ValidateCertificate_sessionCertificateLevelIsNotProvided_throwException()
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-40504040001-MOCK-Q"
            };
            var sessionCertificate = new SessionCertificate { Value = "INVALID" };
            var sessionStatus = new SessionStatus { Result = sessionResult, Cert = sessionCertificate };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'cert.certificateLevel' has empty value", ex.Message);
        }

        [Fact]
        public void ValidateCertificate_sessionCertificateLevelIsNotSupported_throwException()
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-40504040001-MOCK-Q"
            };
            var sessionCertificate = new SessionCertificate
            {
                Value = "INVALID",
                CertificateLevel = "invalid"
            };
            var sessionStatus = new SessionStatus { Result = sessionResult, Cert = sessionCertificate };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status field 'cert.certificateLevel' has unsupported value", ex.Message);
        }

        [Fact]
        public void ValidateCertificate_sessionRequestCertificateLevelIsLowerThanRequested_throwException()
        {
            SessionStatus sessionStatus = ToSessionStatus("cert-choice-cert-40504040001.pem.cert", "ADVANCED");
            var ex = Assert.Throws<CertificateLevelMismatchException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate choice session status response certificate level is lower than requested", ex.Message);
        }

        [Fact]
        public void ValidateCertificate_expiredCertificateWasReturned()
        {
            SessionStatus sessionStatus = ToSessionStatus("expired-cert.pem.crt", "QUALIFIED");
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => certificateChoiceResponseValidator.Validate(sessionStatus));
            Assert.Equal("Certificate is invalid", ex.Message);
        }

        public static IEnumerable<object[]> SessionEndResultErrorCases => new object[][]
        {
            new object[] { "USER_REFUSED", typeof(UserRefusedException) },
            new object[] { "TIMEOUT", typeof(SessionTimeoutException) },
            new object[] { "DOCUMENT_UNUSABLE", typeof(DocumentUnusableException) },
            new object[] { "WRONG_VC", typeof(UserSelectedWrongVerificationCodeException) },
            new object[] { "REQUIRED_INTERACTION_NOT_SUPPORTED_BY_APP", typeof(RequiredInteractionNotSupportedByAppException) },
            new object[] { "USER_REFUSED_CERT_CHOICE", typeof(UserRefusedCertChoiceException) },
            new object[] { "PROTOCOL_FAILURE", typeof(ProtocolFailureException) },
            new object[] { "EXPECTED_LINKED_SESSION", typeof(ExpectedLinkedSessionException) },
            new object[] { "SERVER_ERROR", typeof(SmartIdServerException) },
            new object[] { "UNKNOWN_RESULT", typeof(UnprocessableSmartIdResponseException) },
            new object[] { "ACCOUNT_UNUSABLE", typeof(UserAccountUnusableException) }
        };

        public static IEnumerable<object[]> UserRefusedInteractionCases => new object[][]
        {
            new object[] { "displayTextAndPIN", typeof(UserRefusedDisplayTextAndPinException) },
            new object[] { "confirmationMessage", typeof(UserRefusedConfirmationMessageException) },
            new object[] { "confirmationMessageAndVerificationCodeChoice", typeof(UserRefusedConfirmationMessageWithVerificationChoiceException) }
        };

        private static SessionStatus ToSessionStatus(string certFile, string certificateLevel)
        {
            var sessionResult = new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = "PNOEE-40504040001-MOCK-Q"
            };
            var sessionCertificate = new SessionCertificate
            {
                Value = TestCertificateUtil.GetEncodedCertificateData(certFile),
                CertificateLevel = certificateLevel
            };
            return new SessionStatus
            {
                Result = sessionResult,
                Cert = sessionCertificate
            };
        }

        private static void AssertCertificatesEqual(X509Certificate2 expected, X509Certificate2 actual)
        {
            Assert.Equal(expected.RawData, actual.RawData);
        }
    }
}
