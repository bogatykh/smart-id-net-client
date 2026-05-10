/*-
 * #%L
 * Smart ID .NET client — unit tests
 * Port of Java <c>ee.sk.smartid.rest.SmartIdRestConnectorTest</c> nested classes:
 * <see cref="SemanticsIdentifierDeviceLinkAuthentication"/>, <see cref="AnonymousDeviceLinkAuthentication"/>,
 * <see cref="SemanticsIdentifierNotificationAuthentication"/>, <see cref="DeviceLinkCertificateChoiceTests"/>.
 * #L%
 */

using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using SK.SmartId;
using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Support;
using SK.SmartId.Util;
using Xunit;

namespace SK.SmartId.Rest
{
    public partial class SmartIdRestConnectorTest
    {
        public class SemanticsIdentifierDeviceLinkAuthentication
        {
            private const string AuthenticationWithPersonCodePath = "/authentication/device-link/etsi/PNOEE-30303039914";

            [Fact]
            public async Task InitDeviceLinkAuthentication_qrCodeFlow_ok()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFile(
                    AuthenticationWithPersonCodePath,
                    "requests/auth/device-link/device-link-authentication-session-request-qr-code.json",
                    "responses/auth/device-link/device-link-authentication-session-response.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var semanticsIdentifier = new SemanticsIdentifier("PNOEE-30303039914");
                DeviceLinkSessionResponse r = await connector.InitDeviceLinkAuthenticationAsync(
                    DeviceLinkAuthenticationSessionRequestFactory.QrCodeMinimal(),
                    semanticsIdentifier);
                Assert.Equal("00000000-0000-0000-0000-000000000000", r.SessionID);
            }
        }

        public class AnonymousDeviceLinkAuthentication
        {
            private const string AnonymousAuthenticationPath = "/authentication/device-link/anonymous";

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_qrCodeFlow_ok()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFile(
                    AnonymousAuthenticationPath,
                    "requests/auth/device-link/device-link-authentication-session-request-qr-code.json",
                    "responses/auth/device-link/device-link-authentication-session-response.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                DeviceLinkSessionResponse r = await connector.InitAnonymousDeviceLinkAuthenticationAsync(
                    DeviceLinkAuthenticationSessionRequestFactory.QrCodeMinimal());
                Assert.Equal("00000000-0000-0000-0000-000000000000", r.SessionID);
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_badRequest_throwException()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFileWithStatus(
                    AnonymousAuthenticationPath,
                    "requests/auth/device-link/device-link-authentication-session-request-invalid-request.json",
                    HttpStatusCode.BadRequest);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                await Assert.ThrowsAsync<SmartIdClientException>(() =>
                    connector.InitAnonymousDeviceLinkAuthenticationAsync(
                        DeviceLinkAuthenticationSessionRequestFactory.InvalidMissingProtocolParameters()));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_userAccountNotFound_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows(HttpStatusCode.NotFound, typeof(UserAccountNotFoundException));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_requestIsUnauthorized_throwException()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFileWithStatus(
                    AnonymousAuthenticationPath,
                    "requests/auth/device-link/device-link-authentication-session-request-qr-code.json",
                    HttpStatusCode.Unauthorized);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var ex = await Assert.ThrowsAsync<RelyingPartyAccountConfigurationException>(() =>
                    connector.InitAnonymousDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequestFactory.QrCodeMinimal()));
                Assert.Contains("Request is unauthorized", ex.Message);
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_forbiddenForRP_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows(HttpStatusCode.Forbidden, typeof(RelyingPartyAccountConfigurationException));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_suitableAccountNotFound_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows((HttpStatusCode)471, typeof(NoSuitableAccountOfRequestedTypeFoundException));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_issueWithUserAccount_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows((HttpStatusCode)472, typeof(PersonShouldViewSmartIdPortalException));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_apiClientBeingUsedIsOutdated_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows((HttpStatusCode)480, typeof(SmartIdClientException));
            }

            [Fact]
            public async Task InitAnonymousDeviceLinkAuthentication_systemUnderMaintenance_throwException()
            {
                await AssertAnonymousDeviceLinkErrorThrows((HttpStatusCode)580, typeof(ServerMaintenanceException));
            }

            private static async Task AssertAnonymousDeviceLinkErrorThrows(HttpStatusCode status, Type exceptionType)
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFileWithStatus(
                    AnonymousAuthenticationPath,
                    "requests/auth/device-link/device-link-authentication-session-request-qr-code.json",
                    status);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                Exception ex = await Record.ExceptionAsync(() =>
                    connector.InitAnonymousDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequestFactory.QrCodeMinimal()));
                Assert.NotNull(ex);
                Assert.IsType(exceptionType, ex);
            }
        }

        public class SemanticsIdentifierNotificationAuthentication
        {
            private const string AuthenticationWithPersonCodePath = "/authentication/notification/etsi/PNOEE-48010010101";

            [Fact]
            public async Task InitNotificationAuthentication_onlyRequiredFields_ok()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFile(
                    AuthenticationWithPersonCodePath,
                    "requests/auth/notification/notification-authentication-session-request-only-required-fields.json",
                    "responses/auth/notification/notification-session-response.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var id = new SemanticsIdentifier("PNOEE-48010010101");
                NotificationAuthenticationSessionResponse r = await connector.InitNotificationAuthenticationAsync(
                    NotificationAuthenticationSessionRequestFactory.OnlyRequiredFields(),
                    id);
                Assert.Equal("00000000-0000-0000-0000-000000000000", r.SessionID);
            }

            [Fact]
            public async Task InitNotificationAuthentication_allFields_ok()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFile(
                    AuthenticationWithPersonCodePath,
                    "requests/auth/notification/notification-authentication-session-request-all-fields.json",
                    "responses/auth/notification/notification-session-response.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var id = new SemanticsIdentifier("PNOEE-48010010101");
                NotificationAuthenticationSessionResponse r = await connector.InitNotificationAuthenticationAsync(
                    NotificationAuthenticationSessionRequestFactory.AllFields(),
                    id);
                Assert.Equal("00000000-0000-0000-0000-000000000000", r.SessionID);
            }

            [Fact]
            public async Task InitNotificationAuthentication_badRequest_throwException()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFileWithStatus(
                    AuthenticationWithPersonCodePath,
                    "requests/auth/notification/notification-authentication-session-request-invalid.json",
                    HttpStatusCode.BadRequest);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var id = new SemanticsIdentifier("PNOEE-48010010101");
                await Assert.ThrowsAsync<SmartIdClientException>(() =>
                    connector.InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequestFactory.InvalidMinimal(), id));
            }

            [Fact]
            public async Task InitNotificationAuthentication_unauthorized_throwException()
            {
                await AssertNotificationAuthErrorThrows(HttpStatusCode.Unauthorized, typeof(RelyingPartyAccountConfigurationException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_userAccountNotFound_throwException()
            {
                await AssertNotificationAuthErrorThrows(HttpStatusCode.NotFound, typeof(UserAccountNotFoundException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_forbiddenForRP_throwException()
            {
                await AssertNotificationAuthErrorThrows(HttpStatusCode.Forbidden, typeof(RelyingPartyAccountConfigurationException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_suitableAccountNotFound_throwException()
            {
                await AssertNotificationAuthErrorThrows((HttpStatusCode)471, typeof(NoSuitableAccountOfRequestedTypeFoundException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_issueWithUserAccount_throwException()
            {
                await AssertNotificationAuthErrorThrows((HttpStatusCode)472, typeof(PersonShouldViewSmartIdPortalException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_apiClientBeingUsedIsOutdated_throwException()
            {
                await AssertNotificationAuthErrorThrows((HttpStatusCode)480, typeof(SmartIdClientException));
            }

            [Fact]
            public async Task InitNotificationAuthentication_systemUnderMaintenance_throwException()
            {
                await AssertNotificationAuthErrorThrows((HttpStatusCode)580, typeof(ServerMaintenanceException));
            }

            private static async Task AssertNotificationAuthErrorThrows(HttpStatusCode status, Type exceptionType)
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostJsonWhenBodyEqualsFileWithStatus(
                    AuthenticationWithPersonCodePath,
                    "requests/auth/notification/notification-authentication-session-request-only-required-fields.json",
                    status);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var id = new SemanticsIdentifier("PNOEE-48010010101");
                Exception ex = await Record.ExceptionAsync(() =>
                    connector.InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequestFactory.OnlyRequiredFields(), id));
                Assert.NotNull(ex);
                Assert.IsType(exceptionType, ex);
            }
        }

        public class DeviceLinkCertificateChoiceTests
        {
            private const string AnonymousCertificateChoicePath = "/signature/certificate-choice/device-link/anonymous";

            [Fact]
            public async Task InitDeviceLinkCertificateChoice_throwsRelyingPartyAccountConfigurationException_whenUnauthorized()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostEmptyError(AnonymousCertificateChoicePath, HttpStatusCode.Unauthorized);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var request = new DeviceLinkCertificateChoiceSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO",
                    CertificateLevel = "ADVANCED",
                };
                var ex = await Assert.ThrowsAsync<RelyingPartyAccountConfigurationException>(() =>
                    connector.InitDeviceLinkCertificateChoiceAsync(request));
                Assert.Contains("Request is unauthorized", ex.Message);
                Assert.Contains(AnonymousCertificateChoicePath, ex.Message);
            }

            [Fact]
            public async Task InitDeviceLinkCertificateChoice_userAccountNotFound()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddPostEmptyError(AnonymousCertificateChoicePath, HttpStatusCode.NotFound);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                var request = new DeviceLinkCertificateChoiceSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO",
                    CertificateLevel = "ADVANCED",
                };
                await Assert.ThrowsAsync<UserAccountNotFoundException>(() =>
                    connector.InitDeviceLinkCertificateChoiceAsync(request));
            }
        }

        internal static class DeviceLinkAuthenticationSessionRequestFactory
        {
            internal static DeviceLinkAuthenticationSessionRequest QrCodeMinimal()
            {
                var signatureParameters = new AcspV2SignatureProtocolParameters
                {
                    RpChallenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 32))),
                    SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                    SignatureAlgorithmParameters = new SignatureAlgorithmParameters { HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName() }
                };
                return new DeviceLinkAuthenticationSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO",
                    CertificateLevel = AuthenticationCertificateLevel.QUALIFIED.ToString(),
                    SignatureProtocol = SignatureProtocol.ACSP_V2,
                    SignatureProtocolParameters = signatureParameters,
                    Interactions = InteractionUtil.EncodeToBase64(new[] { Interaction.DisplayTextAndPIN("Log in?") })
                };
            }

            internal static DeviceLinkAuthenticationSessionRequest InvalidMissingProtocolParameters()
            {
                return new DeviceLinkAuthenticationSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO",
                    SignatureProtocol = SignatureProtocol.ACSP_V2,
                    CertificateLevel = AuthenticationCertificateLevel.QUALIFIED.ToString(),
                    Interactions = InteractionUtil.EncodeToBase64(new[] { Interaction.DisplayTextAndPIN("Log in?") })
                };
            }
        }

        internal static class NotificationAuthenticationSessionRequestFactory
        {
            private static AcspV2SignatureProtocolParameters SignatureParams() =>
                new AcspV2SignatureProtocolParameters
                {
                    RpChallenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 32))),
                    SignatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(),
                    SignatureAlgorithmParameters = new SignatureAlgorithmParameters { HashAlgorithm = SmartIdHashAlgorithm.SHA3_512.GetApiAlgorithmName() }
                };

            internal static NotificationAuthenticationSessionRequest OnlyRequiredFields()
            {
                return new NotificationAuthenticationSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO",
                    SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                    SignatureProtocolParameters = SignatureParams(),
                    Interactions = InteractionUtil.EncodeToBase64(new[] { Interaction.ConfirmationMessage("Login?") }),
                    VcType = "numeric4"
                };
            }

            internal static NotificationAuthenticationSessionRequest AllFields()
            {
                var r = OnlyRequiredFields();
                r.CertificateLevel = AuthenticationCertificateLevel.QUALIFIED.ToString();
                r.RequestProperties = new RequestProperties { ShareMdClientIpAddress = true };
                return r;
            }

            internal static NotificationAuthenticationSessionRequest InvalidMinimal()
            {
                return new NotificationAuthenticationSessionRequest
                {
                    RelyingPartyUUID = "00000000-0000-4000-8000-000000000000",
                    RelyingPartyName = "DEMO"
                };
            }
        }
    }
}
