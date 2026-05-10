using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SK.SmartId.Common;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId.IntegrationTests
{
    /// <summary>
    /// Ports Java <c>ee.sk.smartid.integration.SmartIdRestIntegrationTest</c> (live Smart-ID demo).
    /// </summary>
    internal static class SmartIdRestIntegrationTestBase
    {
        internal const string RelyingPartyUuid = "00000000-0000-4000-8000-000000000000";
        internal const string RelyingPartyName = "DEMO";

        internal static readonly Regex UuidPattern =
            new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal static readonly Regex VerificationCodePattern =
            new Regex("^[A-Za-z0-9]{4}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal static readonly Regex SessionTokenPattern =
            new Regex("^[A-Za-z0-9]{24}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal static readonly Regex SessionSecretPattern =
            new Regex("^[A-Za-z0-9+/]{24}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal static readonly SemanticsIdentifier SemanticsIdentifier40504040001 = new SemanticsIdentifier("PNOEE-40504040001");
        internal const string DocumentNumber40404040009MockQ = "PNOEE-40404040009-MOCK-Q";
        internal const string DocumentNumber50001029996DemoQ = "PNOEE-50001029996-DEMO-Q";
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationDeviceLinkAuthenticationTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationDeviceLinkAuthenticationTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        [Fact]
        public async Task InitAnonymousDeviceLinkAuthentication()
        {
            DeviceLinkAuthenticationSessionRequest request = ToDeviceLinkAuthenticationSessionRequest();
            DeviceLinkSessionResponse sessionResponse = await connector.InitAnonymousDeviceLinkAuthenticationAsync(request);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }

        [Fact]
        public async Task InitDeviceLinkAuthentication_withDocumentNumber()
        {
            DeviceLinkAuthenticationSessionRequest request = ToDeviceLinkAuthenticationSessionRequest();
            DeviceLinkSessionResponse sessionResponse =
                await connector.InitDeviceLinkAuthenticationAsync(request, SmartIdRestIntegrationTestBase.DocumentNumber40404040009MockQ);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }

        [Fact]
        public async Task InitDeviceLinkAuthentication_withSemanticsIdentifier()
        {
            DeviceLinkAuthenticationSessionRequest request = ToDeviceLinkAuthenticationSessionRequest();
            DeviceLinkSessionResponse sessionResponse =
                await connector.InitDeviceLinkAuthenticationAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }

        private static DeviceLinkAuthenticationSessionRequest ToDeviceLinkAuthenticationSessionRequest()
        {
            var signatureParameters = new AcspV2SignatureProtocolParameters
            {
                RpChallenge = RpChallengeGenerator.Generate().ToBase64EncodedValue(),
                SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName()
                }
            };
            return new DeviceLinkAuthenticationSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                CertificateLevel = "QUALIFIED",
                SignatureProtocol = SignatureProtocol.ACSP_V2,
                SignatureProtocolParameters = signatureParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Log in?") })
            };
        }
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationDeviceLinkCertificateChoiceTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationDeviceLinkCertificateChoiceTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        [Fact]
        public async Task InitDeviceLinkCertificateChoice()
        {
            var request = new DeviceLinkCertificateChoiceSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName
            };
            DeviceLinkSessionResponse sessionResponse = await connector.InitDeviceLinkCertificateChoiceAsync(request);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.False(string.IsNullOrEmpty(sessionResponse.DeviceLinkBase));
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationDeviceLinkSignatureTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationDeviceLinkSignatureTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        [Fact]
        public async Task InitDeviceLinkSignature_withSemanticIdentifier()
        {
            var signatureProtocolParameters = new RawDigestSignatureProtocolParameters
            {
                Digest = Convert.ToBase64String(DigestCalculator.CalculateDigest(
                    Encoding.UTF8.GetBytes("test"), SmartIdHashAlgorithm.SHA3_512)),
                SignatureAlgorithm = SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName()
                }
            };
            var request = new DeviceLinkSignatureSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Sign it!") })
            };
            DeviceLinkSessionResponse sessionResponse =
                await connector.InitDeviceLinkSignatureAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }

        [Fact]
        public async Task InitDeviceLinkSignature_withDocumentNumber()
        {
            var signatureProtocolParameters = new RawDigestSignatureProtocolParameters
            {
                Digest = Convert.ToBase64String(DigestCalculator.CalculateDigest(
                    Encoding.UTF8.GetBytes("test"), SmartIdHashAlgorithm.SHA_512)),
                SignatureAlgorithm = SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName()
                }
            };
            var request = new DeviceLinkSignatureSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Sign it!") })
            };
            DeviceLinkSessionResponse sessionResponse =
                await connector.InitDeviceLinkSignatureAsync(request, SmartIdRestIntegrationTestBase.DocumentNumber40404040009MockQ);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionTokenPattern, sessionResponse.SessionToken);
            Assert.Matches(SmartIdRestIntegrationTestBase.SessionSecretPattern, sessionResponse.SessionSecret);
            Assert.NotEqual(default, sessionResponse.ReceivedAt);
        }
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationNotificationAuthenticationTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationNotificationAuthenticationTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        [Fact]
        public async Task InitNotificationAuthentication_withSemanticIdentifier()
        {
            NotificationAuthenticationSessionRequest request = ToAuthenticationRequest();
            NotificationAuthenticationSessionResponse sessionResponse =
                await connector.InitNotificationAuthenticationAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
        }

        [Fact]
        public async Task InitNotificationAuthentication_withDocumentNumber()
        {
            NotificationAuthenticationSessionRequest request = ToAuthenticationRequest();
            NotificationAuthenticationSessionResponse sessionResponse =
                await connector.InitNotificationAuthenticationAsync(request, SmartIdRestIntegrationTestBase.DocumentNumber50001029996DemoQ);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
        }

        private static NotificationAuthenticationSessionRequest ToAuthenticationRequest()
        {
            var signatureParameters = new AcspV2SignatureProtocolParameters
            {
                RpChallenge = RpChallengeGenerator.Generate().ToBase64EncodedValue(),
                SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName()
                }
            };
            return new NotificationAuthenticationSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                CertificateLevel = "QUALIFIED",
                SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                SignatureProtocolParameters = signatureParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Log in?") }),
                RequestProperties = new RequestProperties { ShareMdClientIpAddress = true },
                VcType = VerificationCodeType.NUMERIC4.GetValue()
            };
        }
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationNotificationCertificateChoiceTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationNotificationCertificateChoiceTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        [Fact]
        public async Task InitNotificationCertificateChoice_withSemanticIdentifier()
        {
            var request = new NotificationCertificateChoiceSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName
            };
            NotificationCertificateChoiceSessionResponse sessionResponse =
                await connector.InitNotificationCertificateChoiceAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
        }
    }

    [Trait("Category", "DemoIntegration")]
    public sealed class SmartIdRestIntegrationNotificationSignatureTest : IClassFixture<SmartIdDemoFixture>
    {
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationNotificationSignatureTest(SmartIdDemoFixture fixture)
        {
            connector = fixture.Connector;
        }

        public static IEnumerable<object[]> LegacyRsaAlgorithms()
        {
            yield return new object[] { SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION };
            yield return new object[] { SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION };
            yield return new object[] { SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION };
        }

        [Fact]
        public async Task InitNotificationSignature_withSemanticIdentifier()
        {
            NotificationSignatureSessionRequest request = ToSignatureSessionRequest();
            NotificationSignatureSessionResponse sessionResponse =
                await connector.InitNotificationSignatureAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.VerificationCodePattern, sessionResponse.Vc.Value);
            Assert.Equal(VerificationCodeType.NUMERIC4.GetValue(), sessionResponse.Vc.Type);
        }

        [Fact]
        public async Task InitNotificationSignature_withDocumentNumber()
        {
            NotificationSignatureSessionRequest request = ToSignatureSessionRequest();
            NotificationSignatureSessionResponse sessionResponse =
                await connector.InitNotificationSignatureAsync(request, SmartIdRestIntegrationTestBase.DocumentNumber50001029996DemoQ);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.VerificationCodePattern, sessionResponse.Vc.Value);
            Assert.Equal(VerificationCodeType.NUMERIC4.GetValue(), sessionResponse.Vc.Type);
        }

        [Theory]
        [MemberData(nameof(LegacyRsaAlgorithms), MemberType = typeof(SmartIdRestIntegrationNotificationSignatureTest))]
        public async Task InitNotificationSignature_withRsaSsaPkcs1Algorithm_andSemanticIdentifier(
            SigningSignatureAlgorithm signatureAlgorithm)
        {
            NotificationSignatureSessionRequest request = ToSignatureSessionRequestWithRsaSsaPkcs1(signatureAlgorithm);
            NotificationSignatureSessionResponse sessionResponse =
                await connector.InitNotificationSignatureAsync(request, SmartIdRestIntegrationTestBase.SemanticsIdentifier40504040001);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.VerificationCodePattern, sessionResponse.Vc.Value);
            Assert.Equal(VerificationCodeType.NUMERIC4.GetValue(), sessionResponse.Vc.Type);
        }

        [Theory]
        [MemberData(nameof(LegacyRsaAlgorithms), MemberType = typeof(SmartIdRestIntegrationNotificationSignatureTest))]
        public async Task InitNotificationSignature_withRsaSsaPkcs1Algorithm_andDocumentNumber(
            SigningSignatureAlgorithm signatureAlgorithm)
        {
            NotificationSignatureSessionRequest request = ToSignatureSessionRequestWithRsaSsaPkcs1(signatureAlgorithm);
            NotificationSignatureSessionResponse sessionResponse =
                await connector.InitNotificationSignatureAsync(request, SmartIdRestIntegrationTestBase.DocumentNumber50001029996DemoQ);
            Assert.Matches(SmartIdRestIntegrationTestBase.UuidPattern, sessionResponse.SessionID);
            Assert.Matches(SmartIdRestIntegrationTestBase.VerificationCodePattern, sessionResponse.Vc.Value);
            Assert.Equal(VerificationCodeType.NUMERIC4.GetValue(), sessionResponse.Vc.Type);
        }

        private static NotificationSignatureSessionRequest ToSignatureSessionRequestWithRsaSsaPkcs1(
            SigningSignatureAlgorithm signatureAlgorithm)
        {
            SmartIdHashAlgorithm hash = signatureAlgorithm.GetHashAlgorithmForLegacy().Value;
            byte[] digest = DigestCalculator.CalculateDigest(Encoding.UTF8.GetBytes("test"), hash);
            var signatureProtocolParameters = new RawDigestSignatureProtocolParameters
            {
                Digest = Convert.ToBase64String(digest),
                SignatureAlgorithm = signatureAlgorithm.GetAlgorithmName(),
                SignatureAlgorithmParameters = null
            };
            return new NotificationSignatureSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                CertificateLevel = "QUALIFIED",
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Sign it!") })
            };
        }

        private static NotificationSignatureSessionRequest ToSignatureSessionRequest()
        {
            var signatureProtocolParameters = new RawDigestSignatureProtocolParameters
            {
                Digest = Convert.ToBase64String(DigestCalculator.CalculateDigest(
                    Encoding.UTF8.GetBytes("test"), SmartIdHashAlgorithm.SHA_512)),
                SignatureAlgorithm = SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName()
                }
            };
            return new NotificationSignatureSessionRequest
            {
                RelyingPartyUUID = SmartIdRestIntegrationTestBase.RelyingPartyUuid,
                RelyingPartyName = SmartIdRestIntegrationTestBase.RelyingPartyName,
                CertificateLevel = "QUALIFIED",
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(new[] { global::SK.SmartId.Rest.Dao.Interaction.DisplayTextAndPIN("Sign it!") })
            };
        }
    }
}
