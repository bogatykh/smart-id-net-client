/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.NotificationAuthenticationResponseValidatorTest</c>.
 * #L%
 */

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
    public sealed class NotificationAuthenticationResponseValidatorTest
    {
        private const string AuthCertDemoFile = "auth-cert-40504040001-demo-q.crt";
        private const string SignCertFile = "sign-cert-40504040001.pem.crt";

        private const string SignatureValue =
            "DR6pERkYxg5+pa7c0675yEmithtHzEnsqMyOD7RgZlwJgyR/z7VBxOZOUxdakjkT2LK6Jfo3RqxMeYciGfidieJ6vbdyzLDoSnrreaJguo1W5n5blz6Zqb+bkum/30qex7S31ubmRnNM/yIIVJ+/uuAgZgoQIwUV/KmTOE+0GEWFbqvxqFr7BkfrMX4luRrzfXkzpiWqlO8DXoQq4zfo3c00JsvCiM7PSfK4TLVR1FldXmGiV4ftcIep+YoPIxzzIbToyZ0+XYLIgobBio3EHyp2Z3rEWjASfY7+27c0TLkx8gRchxUcowxepioS49lz0trhMzbxNe1NCskHUAa3oodIH0xPNVD/B03uEKziK0r8mGWanHFvOhlqxnCfeN3AuQi5BJ0X7oybMWEvJ06dHlRBc3LrKhM1RrKkSiMy/eI0lTXDajJPupp7Zq/Ck41GbFnn52woFwYAB0hP2kUf7patya9C5C4QyeWB7SnRqtWTXprOMlPHG/KAjh7d61BhjV94zrFKj6YHcDxoQ6a31laYuyhkPMhqdzui1E/4BhWNiJsMkiqdB++VEgL5eT/76xHQuHIUD4GXHmAJnsQjBjFx5ws/yl5pFWsc/GR5H5oNT73Iaw2WSPReXLr7ZD8XEWmTV/GhjXoRUoEjtJrEIv30dYjXqE9Kv+B89tVk2gPHutgNuJJwwoZUaP61ym9w3WawR7ElJ3A8lvYjBPPOY3nYK/hu10imk/9cjdBJaNnMAlfsyzaXtBwBqdu5d80ibFAXkQ9aLwkqURX/Xnmw+lXIzj+p4T2BzhaGR7994qCVksoWPP/0xdvO+lYDM0YLPTvZTXN2PZVgt9NqYTEZHG6/4bcGoIkDTutAxF859rHBplzlMOGDz+sZPKHnLrKMnWaSaSbCVHi7pwF2vcq6QxkzY0grRAKYmmObPP7ORhIjXt5ENoW6n5CptgowizS4CckiaAe0u3QtMp+NoGYg/LSeef7NFhDDf8tUK0azHlAUDb3HPGUtQ3dvYX3JlCoX";

        private readonly NotificationAuthenticationResponseValidator validator;

        public NotificationAuthenticationResponseValidatorTest()
        {
            ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
            {
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
            });
            validator = NotificationAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
        }

        [Fact]
        public void Validate_ok()
        {
            SessionStatus sessionStatus = ToSessionsStatus(AuthCertDemoFile, "QUALIFIED", SignatureValue);

            AuthenticationIdentity identity = validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo", null);

            Assert.Equal("40504040001", identity.IdentityCode);
            Assert.Equal("EE", identity.Country);
        }

        public sealed class ValidateInputs
        {
            private readonly NotificationAuthenticationResponseValidator validator;

            public ValidateInputs()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(Array.Empty<X509Certificate2>());
                validator = NotificationAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_sessionStatusNotProvided_throwException()
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(null, ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo", null));
                Assert.Equal("Parameter 'sessionStatus' is not provided", ex.Message);
            }

            [Fact]
            public void Validate_authenticationSessionRequestIsNotProvided_throwException()
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(new SessionStatus(), null, "smart-id-demo", null));
                Assert.Equal("Parameter 'authenticationSessionRequest' is not provided", ex.Message);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void Validate_emptySchemaNameIsProvided_throwException(string schemaName)
            {
                var ex = Assert.Throws<SmartIdClientException>(() =>
                    validator.Validate(new SessionStatus(), ToAuthenticationSessionRequest("QUALIFIED"), schemaName, null));
                Assert.Equal("Parameter 'schemaName' is not provided", ex.Message);
            }
        }

        [Fact]
        public void Validate_sessionStatusResultIsNotProvided_throwException()
        {
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(new SessionStatus(), ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo", null));
            Assert.Equal("Authentication session status field 'result' is empty", ex.Message);
        }

        public sealed class ValidateSessionStatusCertificate
        {
            private readonly NotificationAuthenticationResponseValidator validator;

            public ValidateSessionStatusCertificate()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
                {
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
                });
                validator = NotificationAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_certificateLevelLowerThanRequested_throwException()
            {
                SessionStatus sessionStatus = ToSessionsStatus(AuthCertDemoFile, "ADVANCED", SignatureValue);

                var ex = Assert.Throws<CertificateLevelMismatchException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo"));

                Assert.Equal("Signer's certificate is below requested certificate level", ex.Message);
            }

            [Fact]
            public void Validate_certificateCannotBeUsedForAuthentication_throwException()
            {
                SessionStatus sessionStatus = ToSessionsStatus(SignCertFile, "QUALIFIED", SignatureValue);

                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo"));

                Assert.Equal("Certificate is not a qualified Smart-ID authentication certificate", ex.Message);
            }
        }

        public sealed class ValidateAuthenticationSignature
        {
            private readonly NotificationAuthenticationResponseValidator validator;

            public ValidateAuthenticationSignature()
            {
                ICertificateValidator certificateValidator = new CertificateValidatorImpl(new List<X509Certificate2>
                {
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_SK_ROOT_G1_2021E.pem.crt"),
                    TestCertificateUtil.ParseTestCertificate("trusted_certificates/TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt")
                });
                validator = NotificationAuthenticationResponseValidator.DefaultSetupWithCertificateValidator(certificateValidator);
            }

            [Fact]
            public void Validate_invalidSignature_throwException()
            {
                SessionStatus sessionStatus = ToSessionsStatus(AuthCertDemoFile, "QUALIFIED", ToBase64("invalidSignature"));

                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                    validator.Validate(sessionStatus, ToAuthenticationSessionRequest("QUALIFIED"), "smart-id-demo"));

                Assert.Equal(
                    "Provided signature value does not match the calculated signature value",
                    ex.Message);
            }
        }

        private static NotificationAuthenticationSessionRequest ToAuthenticationSessionRequest(string certificateLevel) =>
            new NotificationAuthenticationSessionRequest
            {
                RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                RelyingPartyName = "DEMO",
                CertificateLevel = certificateLevel,
                SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                SignatureProtocolParameters = new AcspV2SignatureProtocolParameters
                {
                    RpChallenge =
                        "3mhDkd0ulDR/WVZx678FcrNw4pUhrZxcQsmejf8jQ1HtSp3GAxCH/Fi9EEiuULp44G/KNKONPXZELqCSZw4AoA==",
                    SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                    SignatureAlgorithmParameters = new SignatureAlgorithmParameters { HashAlgorithm = "SHA3-512" }
                },
                Interactions =
                    "W3sidHlwZSI6ImRpc3BsYXlUZXh0QW5kUElOIiwiZGlzcGxheVRleHQ2MCI6IkxvZyBpbiB3aXRoIFNtYXJ0LUlEIGRlbW8/In1d",
                RequestProperties = null,
                Capabilities = null,
                VcType = "numeric4"
            };

        private static SessionStatus ToSessionsStatus(string certFile, string certificateLevel, string signatureValue)
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
                ServerRandom = "9eZeWMTJ9YYBtjj5jK8p1sLm",
                UserChallenge = "RvrVNS1GJYCsuEnEqPCdHHn5vl65F3XiBjmxB4zSosw",
                Value = signatureValue,
                FlowType = FlowType.Notification.GetApiValue(),
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
                Result = new SessionResult { EndResult = "OK", DocumentNumber = "PNOEE-40504040001-DEMO-Q" },
                SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                Signature = signature,
                Cert = cert,
                InteractionTypeUsed = "displayTextAndPIN"
            };
        }

        private static string ToBase64(string data) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }
}
