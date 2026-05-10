using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SK.SmartId.Common;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId.IntegrationTests
{
    /// <summary>
    /// Ports QR device-link authentication flows from Java <c>ReadmeIntegrationTest.DeviceLinkBasedExamples.Authentication</c>
    /// (mock completion via <c>https://sid.demo.sk.ee/mock/device-link</c>).
    /// Trust: Java uses JKS for TLS + <c>FileTrustedCAStoreBuilder</c> for response validation; .NET uses OS TLS for HTTPS and
    /// the same demo CA PEMs as unit tests (see <c>Resources/test-certs/trusted_certificates</c>) for PKIX parity with README.
    /// </summary>
    [Trait("Category", "DemoIntegration")]
    [Trait("Category", "DemoIntegrationReadme")]
    public sealed class ReadmeDeviceLinkAuthenticationIntegrationTest : IClassFixture<SmartIdDemoFixture>
    {
        private const string MockDeviceLinkUrl = "https://sid.demo.sk.ee/mock/device-link";

        /// <summary>Java <c>DeviceLinkType.QR_CODE.getValue()</c>.</summary>
        private const string FlowTypeQr = "QR";

        private static readonly JsonSerializerOptions MockRequestJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly SmartIdDemoFixture fixture;

        public ReadmeDeviceLinkAuthenticationIntegrationTest(SmartIdDemoFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>Java README uses <c>FileTrustedCAStoreBuilder</c>; DEMO chains validate with the same material as <c>DeviceLinkAuthenticationResponseValidatorTest</c>.</summary>
        private static CertificateValidatorImpl CreateReadmeCertificateValidator()
        {
            string baseDir = AppContext.BaseDirectory;
            X509Certificate2 root = LoadPemCertificate(
                Path.Combine(baseDir, "Resources", "test-certs", "trusted_certificates", "TEST_SK_ROOT_G1_2021E.pem.crt"));
            X509Certificate2 eidQ = LoadPemCertificate(
                Path.Combine(baseDir, "Resources", "test-certs", "trusted_certificates", "TEST_of_SK_ID_Solutions_EID-Q_2024E.pem.crt"));
            return new CertificateValidatorImpl(new[] { root, eidQ });
        }

        private static X509Certificate2 LoadPemCertificate(string path)
        {
            string pem = File.ReadAllText(path).Trim();
            string b64 = pem.Replace("-----BEGIN CERTIFICATE-----", "", StringComparison.Ordinal)
                .Replace("-----END CERTIFICATE-----", "", StringComparison.Ordinal)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
            return CertificateParser.ParseX509Certificate(b64);
        }

        private SmartIdClient CreateClient()
        {
            var client = new SmartIdClient();
            client.SetRelyingPartyUUID(SmartIdRestIntegrationTestBase.RelyingPartyUuid);
            client.SetRelyingPartyName(SmartIdRestIntegrationTestBase.RelyingPartyName);
            client.SetHostUrl(SmartIdDemoFixture.DemoEndpoint);
            client.SetConfiguredClient(fixture.HttpClient);
            return client;
        }

        /// <summary>
        /// Runs both Java README QR scenarios in one test: the demo mock is user-scoped, so two concurrent xUnit facts
        /// for the same mock identity cause TIMEOUT; order is document-number first (more stable on demo), then semantics.
        /// </summary>
        [Fact]
        public async Task Authentication_withQrCode_documentNumberThenSemanticIdentifier()
        {
            await RunAuthenticationWithDocumentNumberAndQrCode();
            await RunAuthenticationWithSemanticIdentifierAndQrCode();
        }

        private async Task RunAuthenticationWithSemanticIdentifierAndQrCode()
        {
            SmartIdClient smartIdClient = CreateClient();
            var semanticsIdentifier = new SemanticsIdentifier(
                SemanticsIdentifier.IdentityType.PNO,
                SemanticsIdentifier.CountryCode.EE,
                "40404040009");

            string rpChallenge = RpChallengeGenerator.Generate().ToBase64EncodedValue();

            DeviceLinkAuthenticationSessionRequestBuilder builder = smartIdClient
                .CreateDeviceLinkAuthentication()
                .WithSemanticsIdentifier(semanticsIdentifier)
                .WithRpChallenge(rpChallenge)
                .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Log in?") });

            DeviceLinkSessionResponse authenticationSessionResponse = await builder.InitAsync();
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest = builder.GetAuthenticationSessionRequest();

            string sessionId = authenticationSessionResponse.SessionID;
            string sessionToken = authenticationSessionResponse.SessionToken;
            string sessionSecret = authenticationSessionResponse.SessionSecret;
            long elapsedSeconds = (long)(DateTimeOffset.UtcNow - authenticationSessionResponse.ReceivedAt).TotalSeconds;
            if (elapsedSeconds < 0)
            {
                elapsedSeconds = 0;
            }

            Uri deviceLink = smartIdClient.CreateDynamicContent()
                .WithSchemeName("smart-id-demo")
                .WithDeviceLinkBase(authenticationSessionResponse.DeviceLinkBase)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithSessionToken(sessionToken)
                .WithDigest(rpChallenge)
                .WithElapsedSeconds(elapsedSeconds)
                .WithInteractions(authenticationSessionRequest.Interactions)
                .WithLang("est")
                .BuildDeviceLink(sessionSecret);

            // Java README calls QrCodeGenerator.generateDataUri for frontend; core .NET client has no bundled QR — use device link string with your UI/library.

            await SubmitDeviceLinkToMockServiceAsync(
                fixture.HttpClient,
                new DeviceLinkMockPayload(
                    "PNOEE-40404040009-MOCK-Q",
                    deviceLink.AbsoluteUri,
                    FlowTypeQr));

            SessionStatusPoller poller = smartIdClient.GetSessionStatusPoller();
            SessionStatus sessionStatus = await poller.FetchFinalSessionStatusAsync(sessionId);

            Assert.Equal("COMPLETE", sessionStatus.State);

            CertificateValidatorImpl certificateValidator = CreateReadmeCertificateValidator();
            AuthenticationIdentity authenticationIdentity = DeviceLinkAuthenticationResponseValidator
                .DefaultSetupWithCertificateValidator(certificateValidator)
                .Validate(sessionStatus, authenticationSessionRequest, userChallengeVerifier: null, schemaName: "smart-id-demo");

            Assert.Equal("40404040009", authenticationIdentity.IdentityCode);
            Assert.Equal("OK", authenticationIdentity.GivenName);
            Assert.Equal("TEST", authenticationIdentity.Surname);
            Assert.Equal("EE", authenticationIdentity.Country);
        }

        private async Task RunAuthenticationWithDocumentNumberAndQrCode()
        {
            const string documentNumber = "PNOEE-40404040009-MOCK-Q";
            SmartIdClient smartIdClient = CreateClient();

            string rpChallenge = RpChallengeGenerator.Generate().ToBase64EncodedValue();

            DeviceLinkAuthenticationSessionRequestBuilder builder = smartIdClient
                .CreateDeviceLinkAuthentication()
                .WithDocumentNumber(documentNumber)
                .WithRpChallenge(rpChallenge)
                .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Log in?") });

            DeviceLinkSessionResponse authenticationSessionResponse = await builder.InitAsync();
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest = builder.GetAuthenticationSessionRequest();

            string sessionId = authenticationSessionResponse.SessionID;
            string sessionToken = authenticationSessionResponse.SessionToken;
            string sessionSecret = authenticationSessionResponse.SessionSecret;
            long elapsedSeconds = (long)(DateTimeOffset.UtcNow - authenticationSessionResponse.ReceivedAt).TotalSeconds;
            if (elapsedSeconds < 0)
            {
                elapsedSeconds = 0;
            }

            Uri deviceLink = smartIdClient.CreateDynamicContent()
                .WithSchemeName("smart-id-demo")
                .WithDeviceLinkBase(authenticationSessionResponse.DeviceLinkBase)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithSessionToken(sessionToken)
                .WithDigest(rpChallenge)
                .WithElapsedSeconds(elapsedSeconds)
                .WithInteractions(authenticationSessionRequest.Interactions)
                .WithLang("est")
                .BuildDeviceLink(sessionSecret);

            await SubmitDeviceLinkToMockServiceAsync(
                fixture.HttpClient,
                new DeviceLinkMockPayload(documentNumber, deviceLink.AbsoluteUri, FlowTypeQr));

            SessionStatusPoller poller = smartIdClient.GetSessionStatusPoller();
            SessionStatus sessionStatus = await poller.FetchFinalSessionStatusAsync(sessionId);

            Assert.Equal("COMPLETE", sessionStatus.State);

            CertificateValidatorImpl certificateValidator = CreateReadmeCertificateValidator();
            AuthenticationIdentity authenticationIdentity = DeviceLinkAuthenticationResponseValidator
                .DefaultSetupWithCertificateValidator(certificateValidator)
                .Validate(sessionStatus, authenticationSessionRequest, userChallengeVerifier: null, schemaName: "smart-id-demo");

            Assert.Equal("40404040009", authenticationIdentity.IdentityCode);
            Assert.Equal("OK", authenticationIdentity.GivenName);
            Assert.Equal("TEST", authenticationIdentity.Surname);
            Assert.Equal("EE", authenticationIdentity.Country);
        }

        /// <summary>Java <c>DeviceLinkMockRequest</c> (optional fields omitted when null, matching Jackson <c>NON_EMPTY</c>).</summary>
        private sealed record DeviceLinkMockPayload(
            string DocumentNumber,
            string DeviceLink,
            string FlowType,
            string BrowserCookie = null,
            string InitialCallbackUrl = null);

        private static async Task SubmitDeviceLinkToMockServiceAsync(HttpClient httpClient, DeviceLinkMockPayload payload)
        {
            string body = JsonSerializer.Serialize(payload, MockRequestJsonOptions);
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await httpClient.PostAsync(MockDeviceLinkUrl, content);
            string responseBody = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Mock device-link submission failed: {(int)response.StatusCode} {responseBody}");
            }
        }
    }
}
