/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.DeviceLinkAuthenticationResponseValidatorTest</c>.
 * #L%
 */

using SK.SmartId.Common;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace SK.SmartId
{
    public sealed class DeviceLinkAuthenticationResponseValidatorTest
    {
        private const string AuthCertFile = "auth-cert-40504040001.pem.crt";
        private const string SignCertFile = "sign-cert-40504040001.pem.crt";

        private readonly DeviceLinkAuthenticationResponseValidator validator;

        public DeviceLinkAuthenticationResponseValidatorTest()
        {
            ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
            {
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
            });
            validator = DeviceLinkAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
        }

        public sealed class ValidateInputs
        {
            private readonly DeviceLinkAuthenticationResponseValidator validator;

            public ValidateInputs()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(Array.Empty<X509Certificate2>());
                validator = DeviceLinkAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_sessionStatusNotProvided_throwException()
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(null, ToAuthenticationSessionRequest("QUALIFIED"), null, "smart-id-demo", null));
                Assert.Equal("Parameter 'sessionStatus' is not provided", ex.Message);
            }

            [Fact]
            public void Validate_authenticationSessionRequestIsNotProvided_throwException()
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(new SessionStatus(), null, null, "smart-id-demo", null));
                Assert.Equal("Parameter 'authenticationSessionRequest' is not provided", ex.Message);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void Validate_emptySchemaNameIsProvided_throwException(string schemaName)
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(new SessionStatus(), ToAuthenticationSessionRequest("QUALIFIED"), null, schemaName, null));
                Assert.Equal("Parameter 'schemaName' is not provided", ex.Message);
            }
        }

        [Fact]
        public void Validate_sessionStatusResultIsNotProvided_throwException()
        {
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(new SessionStatus(), ToAuthenticationSessionRequest("QUALIFIED"), null, "smart-id-demo", null));
            Assert.Equal("Authentication session status field 'result' is empty", ex.Message);
        }

        public sealed class ValidateUserChallenge
        {
            private readonly DeviceLinkAuthenticationResponseValidator validator;

            public ValidateUserChallenge()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
                {
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
                });
                validator = DeviceLinkAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void Validate_sameDeviceFlowButUserChallengeVerifierNotProvided_throwException(string userChallengeVerifier)
            {
                SessionStatus sessionStatus = ToSessionStatus(
                    AuthCertFile,
                    "ADVANCED",
                    "",
                    "Cjy8feLy_DB1GNF6lLpXf0VbzCMfTaLHzYOOpdXevSc",
                    FlowType.Web2App);

                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), userChallengeVerifier, "smart-id-demo", null));

                Assert.Equal("Parameter 'userChallengeVerifier' must be provided for 'flowType' - Web2App", ex.Message);
            }
        }

        public sealed class ValidateSessionStatusCertificate
        {
            private readonly DeviceLinkAuthenticationResponseValidator validator;

            public ValidateSessionStatusCertificate()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
                {
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
                });
                validator = DeviceLinkAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_certificateLevelLowerThanRequested_throwException()
            {
                SessionStatus sessionStatus = ToSessionStatus(AuthCertFile, "ADVANCED", "");

                var ex = Assert.Throws<CertificateLevelMismatchException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), null, "smart-id-demo", null));

                Assert.Equal("Signer's certificate is below requested certificate level", ex.Message);
            }

            [Fact]
            public void Validate_certificateCannotBeUsedForAuthentication_throwException()
            {
                SessionStatus sessionStatus = ToSessionStatus(SignCertFile, "QUALIFIED", "");

                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), null, "smart-id-demo", null));

                Assert.Equal("Certificate is not a qualified Smart-ID authentication certificate", ex.Message);
            }
        }

        public sealed class ValidateAuthenticationSignature
        {
            private readonly DeviceLinkAuthenticationResponseValidator validator;

            public ValidateAuthenticationSignature()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
                {
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
                });
                validator = DeviceLinkAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_invalidSignature_throwException()
            {
                SessionStatus sessionStatus = ToSessionStatus(AuthCertFile, "QUALIFIED", ToBase64("invalidSignature"));

                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), null, "smart-id-demo", null));

                Assert.Equal(
                    "Provided signature value does not match the calculated signature value",
                    ex.Message);
            }
        }

        private static SessionStatus ToSessionStatus(string certFile, string certificateLevel, string signatureValue) =>
            ToSessionStatus(certFile, certificateLevel, signatureValue, "TLSjYRH2oYw8tW2bq0it0IUb7WIFkCLgF8NTc7-4Zq4", FlowType.QR);

        private static SessionStatus ToSessionStatus(
            string certFile,
            string certificateLevel,
            string signatureValue,
            string userChallenge,
            FlowType flowType)
        {
            var sessionMaskGenAlgorithmParameters = new SessionMaskGenAlgorithmParameters { HashAlgorithm = "SHA3-512" };
            var maskGenAlgorithm = new SessionMaskGenAlgorithm
            {
                Algorithm = "id-mgf1",
                Parameters = sessionMaskGenAlgorithmParameters
            };
            var sessionSignatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
            {
                HashAlgorithm = "SHA3-512",
                TrailerField = "0xbc",
                SaltLength = 64,
                MaskGenAlgorithm = maskGenAlgorithm
            };
            var signature = new SessionSignature
            {
                ServerRandom = ToBase64(new string('a', 43)),
                UserChallenge = userChallenge,
                Value = string.IsNullOrEmpty(signatureValue) ? ToBase64("signatureValue") : signatureValue,
                FlowType = flowType.GetApiValue(),
                SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = sessionSignatureAlgorithmParameters
            };
            var cert = new SessionCertificate
            {
                Value = TestCertificateUtil.GetEncodedCertificateData(certFile),
                CertificateLevel = certificateLevel
            };
            return new SessionStatus
            {
                State = "COMPLETE",
                Result = new SessionResult { EndResult = "OK", DocumentNumber = "PNOEE-1234567890-MOCK-Q" },
                SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                Signature = signature,
                Cert = cert,
                InteractionTypeUsed = "displayTextAndPIN"
            };
        }

        private static DeviceLinkAuthenticationSessionRequest ToAuthenticationSessionRequest(string certificateLevel)
        {
            string interactions = InteractionUtil.EncodeToBase64(new List<Interaction>
            {
                Interaction.DisplayTextAndPIN("Log in?")
            });
            return new DeviceLinkAuthenticationSessionRequest
            {
                RelyingPartyUUID = "00000000-0000-0000-0000-000000000001",
                RelyingPartyName = "DEMO",
                CertificateLevel = certificateLevel,
                SignatureProtocol = SignatureProtocol.ACSP_V2,
                SignatureProtocolParameters = new AcspV2SignatureProtocolParameters
                {
                    RpChallenge = "rpChallenge",
                    SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                    SignatureAlgorithmParameters = new SignatureAlgorithmParameters { HashAlgorithm = "SHA3-512" }
                },
                Interactions = interactions,
                RequestProperties = null,
                Capabilities = null,
                InitialCallbackUrl = null
            };
        }

        private static string ToBase64(string data) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }
}
