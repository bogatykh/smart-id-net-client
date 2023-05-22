/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 SK ID Solutions AS
 * %%
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * #L%
 */
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId
{
    public class CertificateRequestBuilderTest
    {
        private SmartIdConnectorSpy connector;
        private SessionStatusPoller sessionStatusPoller;
        private CertificateRequestBuilder builder;

        public CertificateRequestBuilderTest()
        {
            connector = new SmartIdConnectorSpy();
            sessionStatusPoller = new SessionStatusPoller(connector);
            connector.sessionStatusToRespond = CreateCertificateSessionStatusCompleteResponse();
            connector.certificateChoiceToRespond = CreateCertificateChoiceResponse();
            builder = new CertificateRequestBuilder(connector, sessionStatusPoller);
        }

        [Fact]
        public async Task getCertificate_usingSemanticsIdentifier()
        {
            SmartIdCertificate certificate = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifierAsString("PNOEE-31111111111")
                .WithCertificateLevel("QUALIFIED")
                .FetchAsync();
            AssertCertificateResponseValid(certificate);
            AssertCorrectSessionRequestMade();
            AssertValidCertificateChoiceRequestMade("QUALIFIED");
        }

        [Fact]
        public async Task getCertificate_usingDocumentNumber()
        {
            SmartIdCertificate certificate = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithDocumentNumber("PNOEE-31111111111")
                .WithCertificateLevel("QUALIFIED")
                .WithCapabilities("ADVANCED")
                .FetchAsync();
            AssertCertificateResponseValid(certificate);
            AssertCorrectSessionRequestMade();
            assertValidCertificateRequestMadeWithDocumentNumber("QUALIFIED");
        }

        [Fact]
        public async Task getCertificate_withoutAnyIdentifier_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .FetchAsync()
            );
            Assert.Equal("Either documentNumber or semanticsIdentifier must be set", exception.Message);
        }

        [Fact]
        public async Task getCertificate_withoutCertificateLevel()
        {
            SmartIdCertificate certificate = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                .FetchAsync();
            AssertCertificateResponseValid(certificate);
            AssertCorrectSessionRequestMade();
            AssertValidCertificateChoiceRequestMade(null);
        }

        [Fact]
        public async Task getCertificate_withShareMdClientIpAddressTrue()
        {
            SmartIdCertificate certificate = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .WithShareMdClientIpAddress(true)
                .FetchAsync();
            AssertCertificateResponseValid(certificate);

            Assert.False(connector.certificateRequestUsed.RequestProperties is null,
                "getRequestProperties must be set withShareMdClientIpAddress");
            Assert.True(connector.certificateRequestUsed.RequestProperties.ShareMdClientIpAddress,
                "requestProperties.shareMdClientIpAddress must be true");
            Assert.Equal("5.5.5.5", certificate.DeviceIpAddress);

            AssertCorrectSessionRequestMade();
            AssertValidCertificateChoiceRequestMade("ADVANCED");
        }

        [Fact]
        public async Task getCertificate_withShareMdClientIpAddressFalse()
        {
            SmartIdCertificate certificate = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .WithShareMdClientIpAddress(false)
                .FetchAsync();
            AssertCertificateResponseValid(certificate);

            Assert.False(connector.certificateRequestUsed.RequestProperties is null,
                "getRequestProperties must be set withShareMdClientIpAddress");
            Assert.False(connector.certificateRequestUsed.RequestProperties.ShareMdClientIpAddress,
                "requestProperties.shareMdClientIpAddress must be false");

            AssertCorrectSessionRequestMade();
            AssertValidCertificateChoiceRequestMade("ADVANCED");
        }

        [Fact]
        public async Task getCertificate_whenIdentityOrDocumentNumberNotSet_shouldThrowException()
        {
            await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .FetchAsync()
            );
        }

        [Fact]
        public async Task getCertificate_withoutRelyingPartyUUID_shouldThrowException()
        {
            await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyName("relying-party-name")
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                    .WithCertificateLevel("QUALIFIED")
                    .FetchAsync()
            );
        }

        [Fact]
        public async Task getCertificate_withoutRelyingPartyName_shouldThrowException()
        {
            await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                    .WithCertificateLevel("QUALIFIED")
                    .FetchAsync()
            );
        }

        [Fact]
        public async Task getCertificate_withTooLongNonce_shouldThrowException()
        {
            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                    .WithCertificateLevel("QUALIFIED")
                    .WithNonce("THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890")
                    .FetchAsync()
            );
            Assert.Equal("Nonce cannot be longer that 30 chars. You supplied: 'THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890'", exception.Message);
        }

        [Fact]
        public async Task getCertificate_withCapabilities()
        {
            await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111"))
                .WithCertificateLevel("QUALIFIED")
                .WithCapabilities(Capability.ADVANCED)
                .FetchAsync();
        }

        [Fact]
        public async Task GetCertificate_whenUserRefuses_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED");
            await Assert.ThrowsAsync<UserRefusedException>(MakeCertificateRequest);
        }

        [Fact]
        public async Task GetCertificate_withDocumentNumber_whenUserRefuses_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED");

            await Assert.ThrowsAsync<UserRefusedException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithCertificateLevel("QUALIFIED")
                    .FetchAsync()
            );
        }

        [Fact]
        public async Task getCertificate_withCertificateResponseWithoutCertificate_shouldThrowException()
        {
            connector.sessionStatusToRespond.Cert = null;
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeCertificateRequest);
        }

        [Fact]
        public async Task getCertificate_withCertificateResponseContainingEmptyCertificate_shouldThrowException()
        {
            connector.sessionStatusToRespond.Cert.Value = "";
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeCertificateRequest);
        }

        [Fact]
        public async Task getCertificate_withCertificateResponseWithoutDocumentNumber_shouldThrowException()
        {
            connector.sessionStatusToRespond.Result.DocumentNumber = null;
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeCertificateRequest);
        }

        [Fact]
        public async Task getCertificate_withCertificateResponseWithBlankDocumentNumber_shouldThrowException()
        {
            connector.sessionStatusToRespond.Result.DocumentNumber = "";
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeCertificateRequest);
        }

        private void AssertCertificateResponseValid(SmartIdCertificate certificate)
        {
            Assert.NotNull(certificate);
            Assert.NotNull(certificate.Certificate);
            X509Certificate2 cert = certificate.Certificate;
            Assert.Contains("SERIALNUMBER=PNOEE-31111111111", cert.Subject);
            Assert.Equal("QUALIFIED", certificate.CertificateLevel);
            Assert.Equal("PNOEE-31111111111", certificate.DocumentNumber);
        }

        private void AssertCorrectSessionRequestMade()
        {
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", connector.sessionIdUsed);
        }

        private void AssertValidCertificateChoiceRequestMade(string certificateLevel)
        {
            Assert.Equal("PNOEE-31111111111", connector.semanticsIdentifierUsed.Identifier);

            Assert.Equal("relying-party-uuid", connector.certificateRequestUsed.RelyingPartyUUID);
            Assert.Equal("relying-party-name", connector.certificateRequestUsed.RelyingPartyName);
            Assert.Equal(certificateLevel, connector.certificateRequestUsed.CertificateLevel);
        }

        private void assertValidCertificateRequestMadeWithDocumentNumber(string certificateLevel)
        {
            Assert.Equal("PNOEE-31111111111", connector.documentNumberUsed);
            Assert.Equal("relying-party-uuid", connector.certificateRequestUsed.RelyingPartyUUID);
            Assert.Equal("relying-party-name", connector.certificateRequestUsed.RelyingPartyName);
            Assert.Equal(certificateLevel, connector.certificateRequestUsed.CertificateLevel);
        }

        private SessionStatus CreateCertificateSessionStatusCompleteResponse()
        {
            SessionStatus status = new SessionStatus
            {
                State = "COMPLETE",
                Cert = CreateSessionCertificate(),
                Result = DummyData.createSessionEndResult(),
                DeviceIpAddress = "5.5.5.5"
            };
            return status;
        }

        private SessionCertificate CreateSessionCertificate()
        {
            SessionCertificate sessionCertificate = new SessionCertificate();
            sessionCertificate.CertificateLevel = "QUALIFIED";
            sessionCertificate.Value = DummyData.CERTIFICATE;
            return sessionCertificate;
        }

        private CertificateChoiceResponse CreateCertificateChoiceResponse()
        {
            CertificateChoiceResponse certificateChoiceResponse = new CertificateChoiceResponse();
            certificateChoiceResponse.SessionId = "97f5058e-e308-4c83-ac14-7712b0eb9d86";
            return certificateChoiceResponse;
        }

        private Task MakeCertificateRequest()
        {
            return builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSemanticsIdentifier(new SemanticsIdentifier("PNO", "EE", "31111111111"))
                .WithCertificateLevel("QUALIFIED")
                .FetchAsync();
        }
    }
}