/*-
 * #%L
 * Smart ID .NET client — unit tests
 * ee.sk.smartid.SmartIdClientTest — selected cases (WireMock in Java → in-memory HttpClient).
 * #L%
 */

using SK.SmartId.Common;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Support;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId
{
    public class SmartIdClientTest
    {
        private const string PersonCode = "PNOEE-1234567890";
        private const string InitialCallbackUrl = "https://example.com/callback";
        private static readonly string RpChallengeB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 32)));

        private static SmartIdClient CreateClient(StubHttpMessageHandler stub)
        {
            var client = new SmartIdClient
            {
                RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                RelyingPartyName = "DEMO"
            };
            client.SetHostUrl("http://localhost");
            client.SetConfiguredClient(new HttpClient(stub));
            return client;
        }

        [Fact]
        public async Task DeviceLinkCertificateChoice_sameDevice_minimal()
        {
            var stub = new StubHttpMessageHandler();
            stub.AddPostJsonWhenBodyEqualsFile(
                "/signature/certificate-choice/device-link/anonymous",
                "requests/sign/linked/cert-choice/certificate-choice-session-request-device-link.json",
                "responses/sign/linked/certificate-choice/device-link-certificate-choice-session-response.json");
            var client = CreateClient(stub);

            DeviceLinkSessionResponse response = await client.CreateDeviceLinkCertificateRequest()
                .WithCertificateLevel(CertificateLevel.QUALIFIED)
                .WithInitialCallbackUrl(InitialCallbackUrl)
                .InitAsync();

            Assert.False(string.IsNullOrEmpty(response.SessionID));
            Assert.False(string.IsNullOrEmpty(response.SessionToken));
            Assert.False(string.IsNullOrEmpty(response.SessionSecret));
            Assert.NotNull(response.DeviceLinkBase);
            Assert.NotEqual(default, response.ReceivedAt);
        }

        [Fact]
        public async Task NotificationCertificateChoice_semantics_minimalFields()
        {
            var stub = new StubHttpMessageHandler();
            stub.AddPostJsonWhenBodyEqualsFile(
                "/signature/certificate-choice/notification/etsi/PNOEE-1234567890",
                "requests/sign/notification/cert-choice/certificate-choice-session-request-only-required-fields.json",
                "responses/sign/notification/cert-choice/notification-certificate-choice-session-response.json");
            var client = CreateClient(stub);

            NotificationCertificateChoiceSessionResponse response = await client.CreateNotificationCertificateChoice()
                .WithSemanticsIdentifier(new SemanticsIdentifier(PersonCode))
                .InitAsync();

            Assert.False(string.IsNullOrEmpty(response.SessionID));
        }

        [Fact]
        public async Task SessionsStatus_fetch_final()
        {
            var stub = new StubHttpMessageHandler();
            stub.AddJsonResponse(
                HttpMethod.Get,
                u => u.AbsolutePath.EndsWith("/session/abcdef1234567890", StringComparison.Ordinal),
                "responses/session-status-successful-authentication.json");
            var client = CreateClient(stub);

            SessionStatus status = await client.GetSessionStatusPoller().FetchFinalSessionStatusAsync("abcdef1234567890");
            Assert.Equal("COMPLETE", status.State);
            Assert.Equal("OK", status.Result.EndResult);
        }

        [Fact]
        public async Task SessionsStatus_direct_get_running()
        {
            var stub = new StubHttpMessageHandler();
            stub.AddJsonResponse(
                HttpMethod.Get,
                u => u.AbsolutePath.EndsWith("/session/abcdef1234567890", StringComparison.Ordinal),
                "responses/session-status-running.json");
            var client = CreateClient(stub);

            SessionStatus status = await client.SmartIdConnector.GetSessionStatusAsync("abcdef1234567890");
            Assert.Equal("RUNNING", status.State);
            Assert.Null(status.Result);
        }

        public static IEnumerable<object[]> SameDeviceFlows()
        {
            yield return new object[] { DeviceLinkType.WEB_2_APP };
            yield return new object[] { DeviceLinkType.APP_2_APP };
        }

        [Theory]
        [MemberData(nameof(SameDeviceFlows))]
        public async Task DynamicContent_authentication_sameDevice_flows(DeviceLinkType deviceLinkType)
        {
            var stub = new StubHttpMessageHandler();
            stub.AddPostJsonWhenBodyEqualsFile("/authentication/device-link/anonymous",
                "requests/auth/device-link/device-link-authentication-session-request-same-device-only-required-fields.json",
                "responses/auth/device-link/device-link-authentication-session-response.json");
            var client = CreateClient(stub);

            DeviceLinkAuthenticationSessionRequestBuilder builder = client.CreateDeviceLinkAuthentication()
                .WithRpChallenge(RpChallengeB64)
                .WithSignatureAlgorithm(AuthenticationSignatureAlgorithm.RSASSA_PSS)
                .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Log in?") })
                .WithHashAlgorithm(SmartIdHashAlgorithm.SHA3_512)
                .WithInitialCallbackUrl(InitialCallbackUrl);

            DeviceLinkSessionResponse response = await builder.InitAsync();
            DeviceLinkAuthenticationSessionRequest request = builder.GetAuthenticationSessionRequest();

            Uri deviceLink = client.CreateDynamicContent()
                .WithSchemeName("smart-id-demo")
                .WithDeviceLinkBase(response.DeviceLinkBase.ToString())
                .WithDeviceLinkType(deviceLinkType)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithSessionToken(response.SessionToken)
                .WithLang("eng")
                .WithDigest("YWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWE=")
                .WithInitialCallbackUrl(request.InitialCallbackUrl)
                .WithInteractions(request.Interactions)
                .BuildDeviceLink(response.SessionSecret);

            AssertDeviceLinkQuery(deviceLink, SessionType.AUTHENTICATION, deviceLinkType, response.SessionToken);
        }

        /// <remarks>Java uses full QR image generation; this test only asserts device-link URI shape.</remarks>
        [Fact]
        public async Task DynamicContent_authentication_QR_without_image()
        {
            var stub = new StubHttpMessageHandler();
            stub.AddPostJsonWhenBodyEqualsFile("/authentication/device-link/anonymous",
                "requests/auth/device-link/device-link-authentication-session-request-qr-code.json",
                "responses/auth/device-link/device-link-authentication-session-response.json");
            var client = CreateClient(stub);

            DeviceLinkAuthenticationSessionRequestBuilder builder = client.CreateDeviceLinkAuthentication()
                .WithRpChallenge(RpChallengeB64)
                .WithSignatureAlgorithm(AuthenticationSignatureAlgorithm.RSASSA_PSS)
                .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Log in?") })
                .WithHashAlgorithm(SmartIdHashAlgorithm.SHA3_512);

            DeviceLinkSessionResponse response = await builder.InitAsync();
            DeviceLinkAuthenticationSessionRequest authenticationSessionRequest = builder.GetAuthenticationSessionRequest();

            long elapsedSeconds = (long)(DateTime.UtcNow - response.ReceivedAt).TotalSeconds;
            Uri qrUri = client.CreateDynamicContent()
                .WithSchemeName("smart-id-demo")
                .WithDeviceLinkBase(response.DeviceLinkBase.ToString())
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithSessionToken(response.SessionToken)
                .WithElapsedSeconds(elapsedSeconds)
                .WithLang("eng")
                .WithDigest("YWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWE=")
                .WithInteractions(authenticationSessionRequest.Interactions)
                .BuildDeviceLink(response.SessionSecret);

            AssertDeviceLinkQuery(qrUri, SessionType.AUTHENTICATION, DeviceLinkType.QR_CODE, response.SessionToken);
        }

        private static void AssertDeviceLinkQuery(Uri qrCodeUri, SessionType sessionType, DeviceLinkType deviceLinkType, string sessionToken)
        {
            Assert.Equal("https", qrCodeUri.Scheme);
            Assert.Equal("smart-id.com", qrCodeUri.Host);
            Assert.Equal("/device-link/", qrCodeUri.AbsolutePath);

            string q = qrCodeUri.Query.TrimStart('?');
            var parts = q.Split('&');
            Assert.Contains("version=1.0", parts);
            Assert.Contains("sessionType=" + ToSessionTypeApi(sessionType), parts);
            Assert.Contains("deviceLinkType=" + ToDeviceLinkTypeApi(deviceLinkType), parts);
            Assert.Contains("sessionToken=" + sessionToken, parts);
            Assert.Contains("lang=eng", parts);
            Assert.Contains(parts, entry => entry.StartsWith("authCode=", StringComparison.Ordinal));
        }

        private static string ToSessionTypeApi(SessionType st) =>
            st switch
            {
                SessionType.AUTHENTICATION => "auth",
                SessionType.SIGNATURE => "sign",
                SessionType.CERTIFICATE_CHOICE => "cert",
                _ => throw new ArgumentOutOfRangeException(nameof(st))
            };

        private static string ToDeviceLinkTypeApi(DeviceLinkType t) =>
            t switch
            {
                DeviceLinkType.QR_CODE => "QR",
                DeviceLinkType.WEB_2_APP => "Web2App",
                DeviceLinkType.APP_2_APP => "App2App",
                _ => throw new ArgumentOutOfRangeException(nameof(t))
            };
    }
}
