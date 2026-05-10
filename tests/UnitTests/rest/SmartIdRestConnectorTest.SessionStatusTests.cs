/*-
 * #%L
 * Smart ID .NET client — unit tests
 * Port of Java <c>ee.sk.smartid.rest.SmartIdRestConnectorTest.SessionStatusTests</c>.
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Rest.Dao;
using SK.SmartId.Support;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId.Rest
{
    public partial class SmartIdRestConnectorTest
    {
        public class SessionStatusTests
        {
            private const string SessionPath = "/session/de305d54-75b4-431b-adb2-eb6b9e546016";
            private static readonly Regex ServerRandomPattern = new Regex("^[a-zA-Z0-9+\\/]+={0,2}$", RegexOptions.Compiled);

            private static bool SessionPathMatch(Uri u) =>
                u.AbsolutePath.Equals(SessionPath, StringComparison.OrdinalIgnoreCase);

            [Fact]
            public async Task GetSessionStatus_running()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-running.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.NotNull(s);
                Assert.Equal("RUNNING", s.State);
            }

            [Fact]
            public async Task GetSessionStatus_running_withIgnoredProperties()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-running-with-ignored-properties.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("RUNNING", s.State);
                Assert.NotNull(s.IgnoredProperties);
                Assert.Equal(2, s.IgnoredProperties.Length);
                Assert.Equal("testingIgnored", s.IgnoredProperties[0]);
                Assert.Equal("testingIgnoredTwo", s.IgnoredProperties[1]);
            }

            [Fact]
            public async Task GetSessionStatus_forSuccessfulAuthenticationRequest()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-successful-authentication.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                AssertSuccessfulResponse(s);
                Assert.Equal("displayTextAndPIN", s.InteractionTypeUsedOrLegacy);
                Assert.Equal("ACSP_V2", s.SignatureProtocol);
                SessionSignature sig = s.Signature;
                Assert.NotNull(sig);
                Assert.Matches(ServerRandomPattern, sig.Value);
                Assert.Equal("J0iyCYOu8cTWuoD8rD05IIrZ", sig.ServerRandom);
                Assert.Matches("^[a-zA-Z0-9-_]{43}$", sig.UserChallenge);
                Assert.Equal("QR", sig.FlowType);
                Assert.Equal("rsassa-pss", sig.SignatureAlgorithmOrLegacy);
                AssertSignatureAlgorithmParameters(sig, "SHA3-512");
                Assert.NotNull(s.Cert);
                Assert.Matches(ServerRandomPattern, s.Cert.Value);
                Assert.Equal("QUALIFIED", s.Cert.CertificateLevel);
            }

            [Fact]
            public async Task GetSessionStatus_hasUserAgentHeader()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-successful-authentication.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                string ua = stub.LastRequest.Headers.UserAgent.ToString();
                Assert.Contains("smart-id-net-client/", ua);
                Assert.Contains(".NET/", ua);
            }

            [Fact]
            public async Task GetSessionStatus_withTimeoutParameter_appendsQuery()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, u => SessionPathMatch(u) && u.Query == "?timeoutMs=10000", "responses/session-status-successful-authentication.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                connector.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(10));
                await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("?timeoutMs=10000", stub.LastRequest.RequestUri.Query);
            }

            [Fact]
            public async Task GetSessionStatus_whenSessionNotFound_throws()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddGetNotFound(SessionPathMatch);
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                await Assert.ThrowsAsync<SessionNotFoundException>(() =>
                    connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016"));
            }

            public class UserRefusedInteractions
            {
                private static bool SessionPathMatch(Uri u) =>
                    u.AbsolutePath.Equals("/session/de305d54-75b4-431b-adb2-eb6b9e546016", StringComparison.OrdinalIgnoreCase);

                [Fact]
                public async Task GetSessionStatus_userHasRefused()
                {
                    var stub = new StubHttpMessageHandler();
                    stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused.json");
                    var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                    SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                    Assert.Equal("COMPLETE", s.State);
                    Assert.Equal("USER_REFUSED", s.Result.EndResult);
                }

                [Fact]
                public async Task GetSessionStatus_userHasRefusedConfirmationMessage()
                {
                    var stub = new StubHttpMessageHandler();
                    stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused-confirmation.json");
                    var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                    SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                    Assert.Equal("COMPLETE", s.State);
                    Assert.Equal("USER_REFUSED_INTERACTION", s.Result.EndResult);
                    Assert.Equal("confirmationMessage", s.Result.Details?.Interaction);
                }

                [Fact]
                public async Task GetSessionStatus_userHasRefusedConfirmationMessageWithVerificationCodeChoice()
                {
                    var stub = new StubHttpMessageHandler();
                    stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused-confirmation-vc-choice.json");
                    var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                    SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                    Assert.Equal("COMPLETE", s.State);
                    Assert.Equal("USER_REFUSED_INTERACTION", s.Result.EndResult);
                    Assert.Equal("confirmationMessageAndVerificationCodeChoice", s.Result.Details?.Interaction);
                }

                [Fact]
                public async Task GetSessionStatus_userHasRefusedDisplayTextAndPin()
                {
                    var stub = new StubHttpMessageHandler();
                    stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused-display-text-and-pin.json");
                    var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                    SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                    Assert.Equal("COMPLETE", s.State);
                    Assert.Equal("USER_REFUSED_INTERACTION", s.Result.EndResult);
                    Assert.Equal("displayTextAndPIN", s.Result.Details?.Interaction);
                }

                [Fact]
                public async Task GetSessionStatus_userHasRefusedVerificationCodeChoice()
                {
                    var stub = new StubHttpMessageHandler();
                    stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused-vc-choice.json");
                    var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                    SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                    Assert.Equal("COMPLETE", s.State);
                    Assert.Equal("USER_REFUSED_INTERACTION", s.Result.EndResult);
                    Assert.Equal("verificationCodeChoice", s.Result.Details?.Interaction);
                }
            }

            [Fact]
            public async Task GetSessionStatus_userHasRefusedCertChoice()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-user-refused-cert-choice.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("USER_REFUSED_CERT_CHOICE", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_timeout()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-timeout.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("TIMEOUT", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_userHasSelectedWrongVcCode()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-wrong-vc.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("WRONG_VC", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_documentUnusable()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-document-unusable.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("DOCUMENT_UNUSABLE", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_protocolFailure()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-protocol-failure.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("PROTOCOL_FAILURE", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_expectedLinkedSession()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-expected-linked-session.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("EXPECTED_LINKED_SESSION", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_serverError()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-server-error.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("SERVER_ERROR", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_accountUnusable()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-account-unusable.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                Assert.Equal("COMPLETE", s.State);
                Assert.Equal("ACCOUNT_UNUSABLE", s.Result.EndResult);
            }

            [Fact]
            public async Task GetSessionStatus_forSuccessfulCertificateRequest()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-successful-certificate-choice.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                AssertSuccessfulResponse(s);
                Assert.NotNull(s.Cert);
                Assert.StartsWith("MIIHTTCCBtSgAwIBAgIQZjAo7ibA2G30zeIncWmIlTAKBggqhkjOPQQDAzBxMSww", s.Cert.Value);
                Assert.Equal("QUALIFIED", s.Cert.CertificateLevel);
            }

            [Fact]
            public async Task GetSessionStatus_forSuccessfulSignatureRequest()
            {
                var stub = new StubHttpMessageHandler();
                stub.AddJsonResponse(HttpMethod.Get, SessionPathMatch, "responses/session-status-successful-signature.json");
                var connector = new SmartIdRestConnector("http://localhost/", new HttpClient(stub));
                SessionStatus s = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
                AssertSuccessfulResponse(s);
                Assert.Equal("verificationCodeChoice", s.InteractionTypeUsedOrLegacy);
                Assert.Equal("RAW_DIGEST_SIGNATURE", s.SignatureProtocol);
                SessionSignature sig = s.Signature;
                Assert.NotNull(sig);
                Assert.StartsWith("fa6riQ8ZXb6esyDpsag9xwupVv5c64jjlvIJ5b+A9g45onozUnd3MMM8S5UYmrgL", sig.Value);
                Assert.Equal("QR", sig.FlowType);
                Assert.Equal("rsassa-pss", sig.SignatureAlgorithmOrLegacy);
                AssertSignatureAlgorithmParameters(sig, "SHA-512");
                Assert.NotNull(s.Cert);
                Assert.StartsWith("MIIHTTCCBtSgAwIBAgIQZjAo7ibA2G30zeIncWmIlTAKBggqhkjOPQQDAzBxMSww", s.Cert.Value);
                Assert.Equal("QUALIFIED", s.Cert.CertificateLevel);
            }

            private static void AssertSuccessfulResponse(SessionStatus sessionStatus)
            {
                Assert.Equal("COMPLETE", sessionStatus.State);
                Assert.NotNull(sessionStatus.Result);
                Assert.Equal("OK", sessionStatus.Result.EndResult);
                Assert.Equal("PNOEE-40504040001-MOCK-Q", sessionStatus.Result.DocumentNumber);
            }

            private static void AssertSignatureAlgorithmParameters(SessionSignature sessionSignature, string expectedHashAlgorithm)
            {
                SessionSignatureAlgorithmParameters p = sessionSignature.SignatureAlgorithmParameters;
                Assert.Equal(expectedHashAlgorithm, p.HashAlgorithm);
                Assert.Equal("id-mgf1", p.MaskGenAlgorithm.Algorithm);
                SessionMaskGenAlgorithmParameters mp = p.MaskGenAlgorithm.Parameters;
                Assert.Equal(expectedHashAlgorithm, mp.HashAlgorithm);
                Assert.Equal(64, p.SaltLength);
                Assert.Equal("0xbc", p.TrailerField);
            }
        }
    }
}
