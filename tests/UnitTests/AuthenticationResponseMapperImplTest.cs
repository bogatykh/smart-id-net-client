/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.AuthenticationResponseMapperImplTest</c>.
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId
{
    public sealed class AuthenticationResponseMapperImplTest
    {
        private const string AuthCertFile = "auth-cert-40504040001.pem.crt";

        private static readonly IAuthenticationResponseMapper Mapper = new AuthenticationResponseMapperImpl();

        public static IEnumerable<object[]> AllFlowTypes() =>
            Enum.GetValues(typeof(FlowType)).Cast<FlowType>().Select(f => new object[] { f });

        public static IEnumerable<object[]> AllHashAlgorithms() =>
            Enum.GetValues(typeof(SmartIdHashAlgorithm)).Cast<SmartIdHashAlgorithm>().Select(h => new object[] { h });

        [Fact]
        public void From()
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
            SessionCertificate sessionCertificate = ToSessionCertificate(
                TestCertificateUtil.GetEncodedCertificateData(AuthCertFile), "QUALIFIED");
            SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature, sessionCertificate);

            AuthenticationResponse r = Mapper.From(sessionStatus);

            Assert.Equal("OK", r.EndResult);
            Assert.Equal("signatureValue", r.SignatureValueInBase64);
            Assert.Equal(TestCertificateUtil.ParseTestCertificate(AuthCertFile).RawData, r.Certificate.RawData);
            Assert.Equal(AuthenticationCertificateLevel.QUALIFIED, r.CertificateLevel);
            Assert.Equal("PNOEE-12345678901-MOCK-Q", r.DocumentNumber);
            Assert.Equal("displayTextAndPIN", r.InteractionTypeUsed);
            Assert.Equal("0.0.0.0", r.DeviceIpAddress);
        }

        [Theory]
        [MemberData(nameof(AllFlowTypes))]
        public void From_authenticationWithDifferentFlowTypes_ok(FlowType flowType)
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
            sessionSignature.FlowType = flowType.GetApiValue();
            SessionCertificate sessionCertificate = ToSessionCertificate(
                TestCertificateUtil.GetEncodedCertificateData(AuthCertFile), "QUALIFIED");
            SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature, sessionCertificate);

            AuthenticationResponse r = Mapper.From(sessionStatus);

            Assert.Equal("OK", r.EndResult);
            Assert.Equal("signatureValue", r.SignatureValueInBase64);
            Assert.Equal(TestCertificateUtil.ParseTestCertificate(AuthCertFile).RawData, r.Certificate.RawData);
            Assert.Equal(AuthenticationCertificateLevel.QUALIFIED, r.CertificateLevel);
            Assert.Equal("PNOEE-12345678901-MOCK-Q", r.DocumentNumber);
            Assert.Equal("displayTextAndPIN", r.InteractionTypeUsed);
            Assert.Equal("0.0.0.0", r.DeviceIpAddress);
        }

        [Theory]
        [MemberData(nameof(AllHashAlgorithms))]
        public void From_authenticationWithDifferentHashAlgorithms_ok(SmartIdHashAlgorithm hashAlgorithm)
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
            sessionSignature.SignatureAlgorithmParameters.HashAlgorithm = hashAlgorithm.GetApiAlgorithmName();
            sessionSignature.SignatureAlgorithmParameters.MaskGenAlgorithm.Parameters.HashAlgorithm = hashAlgorithm.GetApiAlgorithmName();
            sessionSignature.SignatureAlgorithmParameters.SaltLength = hashAlgorithm.GetDigestOctetLength();
            SessionCertificate sessionCertificate = ToSessionCertificate(
                TestCertificateUtil.GetEncodedCertificateData(AuthCertFile), "QUALIFIED");
            SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature, sessionCertificate);

            AuthenticationResponse r = Mapper.From(sessionStatus);

            Assert.Equal("OK", r.EndResult);
            Assert.Equal(hashAlgorithm, r.RsaSsaPssSignatureParameters.DigestHashAlgorithm);
            Assert.Equal(hashAlgorithm.GetDigestOctetLength(), r.RsaSsaPssSignatureParameters.SaltLength);
        }

        [Fact]
        public void From_sessionStatusNull_throwException()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => Mapper.From(null));
            Assert.Equal("Parameter 'sessionsStatus' is not provided", ex.Message);
        }

        public sealed class ValidateResult
        {
            [Fact]
            public void From_sessionResultIsNotPresent_throwException()
            {
                var sessionStatus = new SessionStatus();
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'result' is empty", ex.Message);
            }

            public static TheoryData<string> NullOrEmpty => new TheoryData<string> { null, "" };

            [Theory]
            [MemberData(nameof(NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_endResultIsNotPresent_throwException(string endResult)
            {
                var sessionResult = new SessionResult { EndResult = endResult };
                var sessionStatus = new SessionStatus { Result = sessionResult };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'result.endResult' is empty", ex.Message);
            }

            [Theory]
            [MemberData(nameof(CertificateChoiceResponseValidatorTest.SessionEndResultErrorCases), MemberType = typeof(CertificateChoiceResponseValidatorTest))]
            public void From_endResultIsError_throwException(string endResult, Type expectedException)
            {
                var sessionResult = new SessionResult { EndResult = endResult };
                var sessionStatus = new SessionStatus { Result = sessionResult };
                Assert.Throws(expectedException, () => Mapper.From(sessionStatus));
            }

            [Theory]
            [MemberData(nameof(CertificateChoiceResponseValidatorTest.UserRefusedInteractionCases), MemberType = typeof(CertificateChoiceResponseValidatorTest))]
            public void From_endResultIsUserRefusedInteraction(string interaction, Type expectedException)
            {
                var sessionResult = new SessionResult
                {
                    EndResult = "USER_REFUSED_INTERACTION",
                    Details = new SessionResultDetails { Interaction = interaction }
                };
                var sessionStatus = new SessionStatus { Result = sessionResult };
                Assert.Throws(expectedException, () => Mapper.From(sessionStatus));
            }

            [Theory]
            [MemberData(nameof(NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_documentNumberIsEmpty_throwException(string documentNumber)
            {
                SessionResult sessionResult = ToSessionResult(documentNumber);
                var sessionStatus = new SessionStatus { Result = sessionResult };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'result.documentNumber' is empty", ex.Message);
            }
        }

        [Theory]
        [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
        public void From_signatureProtocolIsNotProvided_throwException(string signatureProtocol)
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            var sessionStatus = new SessionStatus
            {
                Result = sessionResult,
                SignatureProtocol = signatureProtocol
            };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
            Assert.Equal("Authentication session status field 'signatureProtocol' is empty", ex.Message);
        }

        [Theory]
        [InlineData("INVALID")]
        [InlineData("RAW_DIGEST_SIGNATURE")]
        public void From_invalidSignatureProtocolIsProvided_throwException(string invalidSignatureProtocol)
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            var sessionStatus = new SessionStatus
            {
                Result = sessionResult,
                SignatureProtocol = invalidSignatureProtocol
            };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
            Assert.Equal("Authentication session status field 'signatureProtocol' has unsupported value", ex.Message);
        }

        public sealed class ValidateSignature
        {
            [Fact]
            public void From_signatureIsNotProvided_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2"
                };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature' is missing", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_signatureValueIsNotProvided_throwException(string signatureValue)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature { Value = signatureValue };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.value' is empty", ex.Message);
            }

            [Theory]
            [InlineData(@"\|invalidSignatureValue|")]
            [InlineData("#1234567890")]
            public void From_signatureValueDoesNotMatchThePattern_throwException(string signatureValue)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature { Value = signatureValue };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.value' does not have Base64-encoded value", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_serverRandomIsNotProvided_throwException(string serverRandom)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = serverRandom
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.serverRandom' is empty", ex.Message);
            }

            [Fact]
            public void From_serverRandomLengthIsLessThanAllowed_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 23)
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.serverRandom' value length is less than required", ex.Message);
            }

            [Theory]
            [InlineData(@"\|YXRsZWFzdDI0Y2hhcmFjdGVycw|")]
            [InlineData("#YXRsZWFzdDI0Y2hhcmFjdGVycw")]
            public void From_serverRandomValueDoesNotMatchThePattern_throwException(string serverRandom)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = serverRandom
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.serverRandom' does not have Base64-encoded value", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_userChallengeIsEmpty_throwException(string userChallenge)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 24),
                    UserChallenge = userChallenge
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.userChallenge' is empty", ex.Message);
            }

            [Theory]
            [InlineData(@"\#dXNlcmlzYmVpbmdjaGFsbGVuZ2VkYnl0aGlzdmFsd")]
            [InlineData("dXNlcmlzYmVpbmdjaGFsbGVuZ2VkYnl0aGlzdmFsdW=")]
            public void From_providedUserChallengeDoesNotMatchThePattern_throwException(string userChallenge)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 24),
                    UserChallenge = userChallenge
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.userChallenge' value does not match required pattern", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_flowTypeNotProvided_throwException(string flowType)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 24),
                    UserChallenge = new string('a', 43),
                    FlowType = flowType
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.flowType' is empty", ex.Message);
            }

            [Fact]
            public void From_flowTypeNotSupported_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 24),
                    UserChallenge = new string('a', 43),
                    FlowType = "NOT_SUPPORTED_FLOW_TYPE"
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.flowType' has unsupported value", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_signatureAlgorithmIsNotProvided_throwException(string signatureAlgorithm)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature(signatureAlgorithm);
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.signatureAlgorithm' is empty", ex.Message);
            }

            [Fact]
            public void From_signatureAlgorithmIsNotSupported_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                var sessionSignature = new SessionSignature
                {
                    Value = "signatureValue",
                    ServerRandom = new string('a', 24),
                    UserChallenge = new string('a', 43),
                    FlowType = "QR",
                    SignatureAlgorithm = "InvalidAlgorithm",
                    SignatureAlgorithmParameters = ToSessionSignature("rsassa-pss").SignatureAlgorithmParameters
                };
                SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'signature.signatureAlgorithm' has unsupported value", ex.Message);
            }

            public sealed class ValidateSignatureAlgorithmParameters
            {
                [Fact]
                public void From_signatureAlgorithmParametersAreMissing_throwException()
                {
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(null);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters' is missing", ex.Message);
                }

                [Theory]
                [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
                public void From_hashAlgorithmIsMissing_throwException(string hashAlgorithm)
                {
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters { HashAlgorithm = hashAlgorithm };
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' is empty", ex.Message);
                }

                [Theory]
                [InlineData("SHA-1")]
                [InlineData("invalid")]
                public void From_hashAlgorithmIsInvalid_throwException(string invalidHashAlgorithm)
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = invalidHashAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.hashAlgorithm' has unsupported value", ex.Message);
                }

                [Fact]
                public void From_maskGenAlgorithmIsMissing_throwException()
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512"
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' is missing", ex.Message);
                }

                [Theory]
                [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
                public void From_algorithmIsEmptyInMaskGenAlgorithm_throwException(string algorithm)
                {
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm { Algorithm = algorithm };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA-256",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.algorithm' is empty", ex.Message);
                }

                [Fact]
                public void From_algorithmValueInMaskGenAlgorithmIsInvalid_throwException()
                {
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm { Algorithm = "invalid" };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA-256",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm' has unsupported value", ex.Message);
                }

                [Fact]
                public void From_parametersInMaskGenAlgorithmAreMissing_throwException()
                {
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm
                    {
                        Algorithm = "id-mgf1",
                        Parameters = null
                    };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA-256",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters' is missing", ex.Message);
                }

                [Theory]
                [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
                public void From_hashAlgorithmInMaskGenAlgorithmParametersIsEmpty_throwException(string hashAlgorithm)
                {
                    var maskGenAlgorithmParameters = new SessionMaskGenAlgorithmParameters { HashAlgorithm = hashAlgorithm };
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm
                    {
                        Algorithm = "id-mgf1",
                        Parameters = maskGenAlgorithmParameters
                    };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA-256",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' is empty", ex.Message);
                }

                [Theory]
                [InlineData("SHA-1")]
                [InlineData("asdhfasdf")]
                public void From_hashAlgorithmInMaskGenAlgorithmParametersInvalid_throwException(string hashAlgorithm)
                {
                    var maskGenAlgorithmParameters = new SessionMaskGenAlgorithmParameters { HashAlgorithm = hashAlgorithm };
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm
                    {
                        Algorithm = "id-mgf1",
                        Parameters = maskGenAlgorithmParameters
                    };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA-256",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' has unsupported value", ex.Message);
                }

                [Fact]
                public void From_hashAlgorithmInMaskGenAlgorithmDoesNotMatchSignaturesHashAlgorithm_throwException()
                {
                    var maskGenAlgorithmParameters = new SessionMaskGenAlgorithmParameters { HashAlgorithm = "SHA-512" };
                    var maskGenAlgorithm = new SessionMaskGenAlgorithm
                    {
                        Algorithm = "id-mgf1",
                        Parameters = maskGenAlgorithmParameters
                    };
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512",
                        MaskGenAlgorithm = maskGenAlgorithm
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.maskGenAlgorithm.parameters.hashAlgorithm' value does not match 'signature.signatureAlgorithmParameters.hashAlgorithm' value", ex.Message);
                }

                [Fact]
                public void From_saltLengthIsMissing_throwException()
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512",
                        SaltLength = null,
                        MaskGenAlgorithm = new SessionMaskGenAlgorithm
                        {
                            Algorithm = "id-mgf1",
                            Parameters = ToMaskGenAlgorithmParameters()
                        }
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.saltLength' is empty", ex.Message);
                }

                [Fact]
                public void From_saltLengthDoesNotMatchHashAlgorithmOctetLength_throwException()
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512",
                        SaltLength = 20,
                        MaskGenAlgorithm = new SessionMaskGenAlgorithm
                        {
                            Algorithm = "id-mgf1",
                            Parameters = ToMaskGenAlgorithmParameters()
                        }
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.saltLength' has invalid value", ex.Message);
                }

                [Theory]
                [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
                public void From_trailerFieldIsEmpty_throwException(string trailerField)
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512",
                        SaltLength = 64,
                        TrailerField = trailerField,
                        MaskGenAlgorithm = new SessionMaskGenAlgorithm
                        {
                            Algorithm = "id-mgf1",
                            Parameters = ToMaskGenAlgorithmParameters()
                        }
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.trailerField' is empty", ex.Message);
                }

                [Fact]
                public void From_trailerFieldValueIsInvalid_throwException()
                {
                    var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
                    {
                        HashAlgorithm = "SHA3-512",
                        SaltLength = 64,
                        TrailerField = "invalid",
                        MaskGenAlgorithm = new SessionMaskGenAlgorithm
                        {
                            Algorithm = "id-mgf1",
                            Parameters = ToMaskGenAlgorithmParameters()
                        }
                    };
                    SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                    SessionSignature sessionSignature = ToSessionSignatureWithParameters(signatureAlgorithmParameters);
                    SessionStatus sessionStatus = ToSessionStatus(sessionResult, sessionSignature);
                    var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                    Assert.Equal("Authentication session status field 'signature.signatureAlgorithmParameters.trailerField' has unsupported value", ex.Message);
                }
            }

            private static SessionStatus ToSessionStatus(SessionResult sessionResult, SessionSignature sessionSignature)
            {
                return new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature
                };
            }
        }

        public sealed class ValidateCertificate
        {
            [Fact]
            public void From_sessionCertificateIsNotProvided_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature
                };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'cert' is missing", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_certificateValueIsNotProvided_throwException(string certificateValue)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
                var sessionCertificate = new SessionCertificate { Value = certificateValue };
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature,
                    Cert = sessionCertificate
                };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'cert.value' is empty", ex.Message);
            }

            [Theory]
            [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
            public void From_certificateLevelIsNotProvided_throwException(string certificateLevel)
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
                SessionCertificate sessionCertificate = ToSessionCertificate("certificateValue", certificateLevel);
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature,
                    Cert = sessionCertificate
                };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'cert.certificateLevel' is empty", ex.Message);
            }

            [Fact]
            public void From_certificateIsInvalid_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
                SessionCertificate sessionCertificate = ToSessionCertificate("invalidCertificateValue", "QUALIFIED");
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature,
                    Cert = sessionCertificate,
                    InteractionTypeUsed = "displayTextAndPIN"
                };
                var ex = Assert.Throws<SmartIdClientException>(() => Mapper.From(sessionStatus));
                Assert.StartsWith("Failed to parse X509 certificate from", ex.Message);
            }

            [Fact]
            public void From_certificateLevelIsInvalid_throwException()
            {
                SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
                SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
                SessionCertificate sessionCertificate = ToSessionCertificate(
                    TestCertificateUtil.GetEncodedCertificateData(AuthCertFile), "invalid");
                var sessionStatus = new SessionStatus
                {
                    Result = sessionResult,
                    SignatureProtocol = "ACSP_V2",
                    Signature = sessionSignature,
                    Cert = sessionCertificate,
                    InteractionTypeUsed = "displayTextAndPIN"
                };
                var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
                Assert.Equal("Authentication session status field 'cert.certificateLevel' has unsupported value", ex.Message);
            }
        }

        [Theory]
        [MemberData(nameof(ValidateResult.NullOrEmpty), MemberType = typeof(ValidateResult))]
        public void From_interactionTypeUsedNotProvided_throwException(string interactionFlowUsed)
        {
            SessionResult sessionResult = ToSessionResult("PNOEE-12345678901-MOCK-Q");
            SessionSignature sessionSignature = ToSessionSignature("rsassa-pss");
            SessionCertificate sessionCertificate = ToSessionCertificate("certificateValue", "QUALIFIED");
            var sessionStatus = new SessionStatus
            {
                Result = sessionResult,
                SignatureProtocol = "ACSP_V2",
                Signature = sessionSignature,
                Cert = sessionCertificate,
                InteractionTypeUsed = interactionFlowUsed
            };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => Mapper.From(sessionStatus));
            Assert.Equal("Authentication session status field 'interactionTypeUsed' is empty", ex.Message);
        }

        private static SessionResult ToSessionResult(string documentNumber)
        {
            return new SessionResult
            {
                EndResult = "OK",
                DocumentNumber = documentNumber
            };
        }

        private static SessionSignature ToSessionSignature(string signatureAlgorithm)
        {
            var sessionSignature = new SessionSignature
            {
                Value = "signatureValue",
                ServerRandom = "U2VydmVyUmFuZG9tTW9yZVRoYW4yNENoYXJhY3RlcnM=",
                UserChallenge = "dXNlcmlzYmVpbmdjaGFsbGVuZ2VkYnl0aGlzdmFsdWU",
                SignatureAlgorithm = signatureAlgorithm,
                FlowType = "QR"
            };
            var signatureAlgorithmParameters = new SessionSignatureAlgorithmParameters
            {
                HashAlgorithm = "SHA3-512",
                SaltLength = 64,
                TrailerField = "0xbc",
                MaskGenAlgorithm = new SessionMaskGenAlgorithm
                {
                    Algorithm = "id-mgf1",
                    Parameters = new SessionMaskGenAlgorithmParameters { HashAlgorithm = "SHA3-512" }
                }
            };
            sessionSignature.SignatureAlgorithmParameters = signatureAlgorithmParameters;
            return sessionSignature;
        }

        private static SessionSignature ToSessionSignatureWithParameters(SessionSignatureAlgorithmParameters signatureAlgorithmParameters)
        {
            return new SessionSignature
            {
                Value = "signatureValue",
                ServerRandom = new string('a', 24),
                UserChallenge = new string('a', 43),
                FlowType = "QR",
                SignatureAlgorithm = "rsassa-pss",
                SignatureAlgorithmParameters = signatureAlgorithmParameters
            };
        }

        private static SessionMaskGenAlgorithmParameters ToMaskGenAlgorithmParameters()
        {
            return new SessionMaskGenAlgorithmParameters { HashAlgorithm = "SHA3-512" };
        }

        private static SessionCertificate ToSessionCertificate(string value, string certificateLevel)
        {
            return new SessionCertificate
            {
                Value = value,
                CertificateLevel = certificateLevel
            };
        }

        private static SessionStatus ToSessionStatus(SessionResult sessionResult, SessionSignature sessionSignature, SessionCertificate sessionCertificate)
        {
            return new SessionStatus
            {
                Result = sessionResult,
                SignatureProtocol = "ACSP_V2",
                Signature = sessionSignature,
                Cert = sessionCertificate,
                InteractionTypeUsed = "displayTextAndPIN",
                DeviceIpAddress = "0.0.0.0"
            };
        }
    }
}
