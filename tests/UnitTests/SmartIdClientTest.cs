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

using Moq;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static SK.SmartId.Rest.Dao.SemanticsIdentifier;

namespace SK.SmartId
{
    public class SmartIdClientTest
    {
        private readonly Mock<HttpMessageHandler> handlerMock;
        private readonly SmartIdClient client;

        public SmartIdClientTest()
        {
            handlerMock = new Mock<HttpMessageHandler>();

            client = new SmartIdClient
            {
                RelyingPartyUUID = "de305d54-75b4-431b-adb2-eb6b9e546014",
                RelyingPartyName = "BANK123"
            };
            client.SetHostUrl("http://localhost:18089");
            client.SetConfiguredClient(new HttpClient(handlerMock.Object));

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequest.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequestWithSha512.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequestWithNonce.json", "responses/signatureSessionResponse.json");

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/etsi/PNOEE-31111111111", "requests/signatureSessionRequest.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/etsi/PASEE-987654321012", "requests/signatureSessionRequest.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/etsi/IDCEE-AA3456789", "requests/signatureSessionRequest.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusForSuccessfulCertificateRequest.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusForSuccessfulSigningRequest.json");

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-31111111111", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/PNOEE-31111111111", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/PASEE-987654321012", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/IDCEE-AA3456789", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/PASEE-987654321012", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/IDCEE-AA3456789", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");
        }

        [Fact]
        public void TestSetup()
        {
            Assert.Equal("de305d54-75b4-431b-adb2-eb6b9e546014", client.RelyingPartyUUID);
            Assert.Equal("BANK123", client.RelyingPartyName);
        }

        [Fact]
        public async Task GetCertificateAndSign_fullExample()
        {
            // Provide data bytes to be signed (Default hash type is SHA-512)
            SignableData dataToSign = new SignableData(Encoding.UTF8.GetBytes("Hello World!"));

            // Calculate verification code
            Assert.Equal("4664", dataToSign.CalculateVerificationCode());

            // Get certificate and document number
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithSemanticsIdentifier(new SemanticsIdentifier("PNO", "EE", "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            X509Certificate2 x509Certificate = certificateResponse.Certificate;
            string documentNumber = certificateResponse.DocumentNumber;

            // Sign the data using the document number
            SmartIdSignature signature = await client
                .CreateSignature()
                .WithDocumentNumber(documentNumber)
                .WithSignableData(dataToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") })
                .SignAsync();

            byte[] signatureValue = signature.Value;
            string algorithmName = signature.AlgorithmName; // Returns "sha512WithRSAEncryption"

            string interactionFlowUsed = signature.InteractionFlowUsed;

            Assert.Equal("displayTextAndPIN", interactionFlowUsed);
            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task GetCertificateAndSign_withExistingHash()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithSemanticsIdentifier(new SemanticsIdentifier("PNO", "EE", "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            string documentNumber = certificateResponse.DocumentNumber;

            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            SmartIdSignature signature = await client
                .CreateSignature()
                .WithDocumentNumber(documentNumber)
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task GetCertificateUsingSemanticsIdentifier()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier("PNO", "EE", "31111111111");

            SmartIdCertificate certificate = await client
                .GetCertificate()
                .WithSemanticsIdentifier(semanticsIdentifier)
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            AssertCertificateResponseValid(certificate);
        }

        [Fact]
        public async Task GetCertificateUsingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-31111111111-ADVANCED-LEVEL", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");

            SmartIdCertificate certificate = await client
                .GetCertificate()
                .WithDocumentNumber("PNOEE-31111111111-ADVANCED-LEVEL")
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            AssertCertificateResponseValid(certificate);
        }

        [Fact]
        public async Task GetCertificateWithNonce()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-31111111111-NONCE", "requests/certificateChoiceRequestWithNonce.json", "responses/certificateChoiceResponse.json");

            SmartIdCertificate certificate = await client
                .GetCertificate()
                .WithDocumentNumber("PNOEE-31111111111-NONCE")
                .WithCertificateLevel("ADVANCED")
                .WithNonce("zstOt2umlc")
                .FetchAsync();

            AssertCertificateResponseValid(certificate);
        }

        [Fact]
        public async Task GetCertificateWithManualSessionStatusRequesting()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-31111111111-ADVANCED-LEVEL", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");

            CertificateRequestBuilder builder = client.GetCertificate();
            string sessionId = await builder
                    .WithDocumentNumber("PNOEE-31111111111-ADVANCED-LEVEL")
                    .WithCertificateLevel("ADVANCED")
                    .InitiateCertificateChoiceAsync();

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdCertificate certificate = builder.CreateSmartIdCertificate(sessionStatus);

            AssertCertificateResponseValid(certificate);
        }

        [Fact]
        public async Task GetCertificateWithManualSessionStatusRequesting_andCustomResponseSocketTimeout()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-31111111111-ADVANCED-LEVEL", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");

            client.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(5));
            CertificateRequestBuilder builder = client.GetCertificate();
            String sessionId = await builder
                    .WithDocumentNumber("PNOEE-31111111111-ADVANCED-LEVEL")
                    .WithCertificateLevel("ADVANCED")
                    .InitiateCertificateChoiceAsync();

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86?timeoutMs=5000", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdCertificate certificate = builder.CreateSmartIdCertificate(sessionStatus);

            AssertCertificateResponseValid(certificate);
        }

        [Fact]
        public async Task Sign_withDocumentNumber()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            Assert.Equal("1796", hashToSign.CalculateVerificationCode());

            SmartIdSignature signature = await client
                .CreateSignature()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task Sign_withSemanticsIdentifier()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            Assert.Equal("1796", hashToSign.CalculateVerificationCode());

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(IdentityType.IDC, CountryCode.EE, "AA3456789");

            SmartIdSignature signature = await client
                .CreateSignature()
                .WithSemanticsIdentifier(semanticsIdentifier)
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task SignWithNonce()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            Assert.Equal("1796", hashToSign.CalculateVerificationCode());

            SmartIdSignature signature = await client
                .CreateSignature()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithNonce("zstOt2umlc")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task SignWithManualSessionStatusRequesting()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            Assert.Equal("1796", hashToSign.CalculateVerificationCode());

            SignatureRequestBuilder builder = client.CreateSignature();
            String sessionId = await builder
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("ADVANCED")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                            Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                    )
                    .InitiateSigningAsync();

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdSignature signature = builder.CreateSmartIdSignature(sessionStatus);

            AssertValidSignatureCreated(signature);
            //TODO: verify(getRequestedFor(urlEqualTo("/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00")));
        }

        [Fact]
        public async Task SignWithManualSessionStatusRequesting_andCustomResponseSocketTimeout()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };

            Assert.Equal("1796", hashToSign.CalculateVerificationCode());

            client.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(5));
            SignatureRequestBuilder builder = client.CreateSignature();
            string sessionId = await builder
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("ADVANCED")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                            Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                    )
                    .InitiateSigningAsync();

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00?timeoutMs=5000", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdSignature signature = builder.CreateSmartIdSignature(sessionStatus);

            AssertValidSignatureCreated(signature);
        }

        [Fact]
        public async Task GetCertificate_whenUserAccountNotFound_shouldThrowException()
        {
            SmartIdRestServiceStubs.stubNotFoundResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json");
            await Assert.ThrowsAsync<UserAccountNotFoundException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenUserAccountNotFound_shouldThrowException()
        {
            SmartIdRestServiceStubs.stubNotFoundResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequest.json");
            await Assert.ThrowsAsync<UserAccountNotFoundException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenUserCancels_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusWhenUserRefusedGeneral.json");
            await Assert.ThrowsAsync<UserRefusedException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenUserCancels_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusWhenUserRefusedGeneral.json");
            await Assert.ThrowsAsync<UserRefusedException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task Sign_whenTimeout_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusWhenTimeout.json");
            await Assert.ThrowsAsync<SessionTimeoutException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenRequiredInteractionNotSupportedByApp_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", "responses/signatureSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusWhenRequiredInteractionNotSupportedByApp.json");
            await Assert.ThrowsAsync<RequiredInteractionNotSupportedByAppException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task Sign_whenRequiredInteractionNotSupportedByApp_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusWhenRequiredInteractionNotSupportedByApp.json");
            await Assert.ThrowsAsync<RequiredInteractionNotSupportedByAppException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenDocumentUnusable_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusWhenDocumentUnusable.json");
            await Assert.ThrowsAsync<DocumentUnusableException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenUnknownErrorCode_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusWhenUnknownErrorCode.json");
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenDocumentUnusable_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusWhenDocumentUnusable.json");
            await Assert.ThrowsAsync<DocumentUnusableException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenRequestForbidden_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubForbiddenResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json");
            await Assert.ThrowsAsync<RelyingPartyAccountConfigurationException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenRequestForbidden_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubForbiddenResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequest.json");
            await Assert.ThrowsAsync<RelyingPartyAccountConfigurationException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenApiReturnsErrorStatusCode471_shouldThrowNoSuitableAccountOfRequestedTypeFoundException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json", 471);
            await Assert.ThrowsAsync<NoSuitableAccountOfRequestedTypeFoundException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenApiReturnsErrorStatusCode472_shouldThrowPersonShouldViewSmartIdPortalExceptionn()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json", 472);
            await Assert.ThrowsAsync<PersonShouldViewSmartIdPortalException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenClientSideAPIIsNotSupportedAnymore_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequest.json", 480);
            await Assert.ThrowsAsync<SmartIdClientException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task GetCertificate_whenSystemUnderMaintenance_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/certificatechoice/etsi/PNOEE-31111111111", "requests/certificateChoiceRequest.json", 580);
            await Assert.ThrowsAsync<ServerMaintenanceException>(MakeGetCertificateRequestAsync);
        }

        [Fact]
        public async Task Sign_whenSystemUnderMaintenance_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/signature/document/PNOEE-31111111111", "requests/signatureSessionRequest.json", 580);
            await Assert.ThrowsAsync<ServerMaintenanceException>(MakeCreateSignatureRequestAsync);
        }

        [Fact]
        public async Task SetPollingSleepTimeoutForSignatureCreation()
        {
            var state = new SmartIdRestServiceStubs.RequestState()
            {
                State = "STARTED"
            };

            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusRunning.json", state, "STARTED", "COMPLETE");
            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "2c52caf4-13b0-41c4-bdc6-aa268403cc00", "responses/sessionStatusForSuccessfulSigningRequest.json", state, "COMPLETE", "STARTED");
            client.SetPollingSleepTimeout(TimeSpan.FromSeconds(2L));
            double duration = await MeasureSigningDurationAsync();
            Assert.True(duration > 2000L, "Duration is " + duration);
            Assert.True(duration < 3000L, "Duration is " + duration);
        }

        [Fact]
        public async Task SetPollingSleepTimeoutForCertificateChoice()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-31111111111", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");

            var state = new SmartIdRestServiceStubs.RequestState()
            {
                State = "STARTED"
            };

            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusRunning.json", state, "STARTED", "COMPLETE");
            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "97f5058e-e308-4c83-ac14-7712b0eb9d86", "responses/sessionStatusForSuccessfulCertificateRequest.json", state, "COMPLETE", "STARTED");
            client.SetPollingSleepTimeout(TimeSpan.FromSeconds(2L));
            double duration = await MeasureCertificateChoiceDurationAsync();
            Assert.True(duration > 2000L, "Duration is " + duration);
            Assert.True(duration < 3000L, "Duration is " + duration);
        }

        [Fact]
        public async Task SetSessionStatusResponseSocketTimeout()
        {
            client.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(10L));
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/2c52caf4-13b0-41c4-bdc6-aa268403cc00?timeoutMs=10000", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");
            SmartIdSignature signature = await CreateSignatureAsync();
            Assert.NotNull(signature);
        }

        [Fact]
        public async Task AuthenticateUsingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");

            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            Assert.Equal("4430", authenticationHash.CalculateVerificationCode());

            SmartIdAuthenticationResponse authenticationResponse = await client
                .CreateAuthentication()
                .WithDocumentNumber("PNOEE-32222222222-Z1B2-Q")
                .WithAuthenticationHash(authenticationHash)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();

            Assert.Equal("PNOEE-31111111111", authenticationResponse.DocumentNumber);
            AssertAuthenticationResponseValid(authenticationResponse);
        }

        [Fact]
        public async Task Authenticate_usingSemanticsIdentifier()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            Assert.Equal("4430", authenticationHash.CalculateVerificationCode());

            SmartIdAuthenticationResponse authenticationResponse = await client
                    .CreateAuthentication()
                    .WithSemanticsIdentifierAsString("PNOEE-31111111111")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("ADVANCED")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                            Interaction.DisplayTextAndPIN("Log in?") }
                    )
                    .AuthenticateAsync();

            AssertAuthenticationResponseValid(authenticationResponse);
        }

        [Fact]
        public async Task AuthenticateWithNonce()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-31111111111-WITH-NONCE", "requests/authenticationSessionRequestWithNonce.json", "responses/authenticationSessionResponse.json");


            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==";
            authenticationHash.HashType = HashType.SHA512;

            Assert.Equal("4430", authenticationHash.CalculateVerificationCode());

            SmartIdAuthenticationResponse authenticationResponse = await client
                .CreateAuthentication()
                .WithDocumentNumber("PNOEE-31111111111-WITH-NONCE")
                .WithAuthenticationHash(authenticationHash)
                .WithCertificateLevel("ADVANCED")
                .WithNonce("g9rp4kjca3")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();

            AssertAuthenticationResponseValid(authenticationResponse);
        }

        [Fact]
        public async Task AuthenticateWithManualSessionStatusRequesting()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111");

            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==";
            authenticationHash.HashType = HashType.SHA512;

            Assert.Equal("4430", authenticationHash.CalculateVerificationCode());

            AuthenticationRequestBuilder builder = client.CreateAuthentication();
            String sessionId = await builder
                    .WithSemanticsIdentifier(semanticsIdentifier)
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("ADVANCED")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                            Interaction.DisplayTextAndPIN("Log in?") }
                    )
                    .InitiateAuthenticationAsync();

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdAuthenticationResponse authenticationResponse = builder.CreateSmartIdAuthenticationResponse(sessionStatus);

            AssertAuthenticationResponseValid(authenticationResponse);
            //TODO: verify(getRequestedFor(urlEqualTo("/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb")));
        }

        [Fact]
        public async Task AuthenticateWithManualSessionStatusRequesting_andCustomResponseSocketTimeout()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111");

            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            Assert.Equal("4430", authenticationHash.CalculateVerificationCode());

            client.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(5));
            AuthenticationRequestBuilder builder = client.CreateAuthentication();
            string sessionId = await builder
                    .WithSemanticsIdentifier(semanticsIdentifier)
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("ADVANCED")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                            Interaction.DisplayTextAndPIN("Log in?") }
                    )
                    .InitiateAuthenticationAsync();

            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb?timeoutMs=5000", "responses/sessionStatusForSuccessfulAuthenticationRequest.json");

            SessionStatus sessionStatus = await client.SmartIdConnector.GetSessionStatusAsync(sessionId);
            SmartIdAuthenticationResponse authenticationResponse = builder.CreateSmartIdAuthenticationResponse(sessionStatus);

            AssertAuthenticationResponseValid(authenticationResponse);
        }

        [Fact]
        public async Task Authenticate_whenUserAccountNotFound_shouldThrowException()
        {
            SmartIdRestServiceStubs.stubNotFoundResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json");
            await Assert.ThrowsAsync<UserAccountNotFoundException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenUserCancels_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusWhenUserRefusedGeneral.json");
            await Assert.ThrowsAsync<UserRefusedException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenTimeout_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusWhenTimeout.json");
            await Assert.ThrowsAsync<SessionTimeoutException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenDocumentUnusable_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusWhenDocumentUnusable.json");
            await Assert.ThrowsAsync<DocumentUnusableException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenRequestForbidden_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubForbiddenResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json");
            await Assert.ThrowsAsync<RelyingPartyAccountConfigurationException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenClientSideAPIIsNotSupportedAnymore_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", 480);
            await Assert.ThrowsAsync<SmartIdClientException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_whenSystemUnderMaintenance_shouldThrowException()
        {
            SmartIdRestServiceStubs.StubErrorResponse(handlerMock, "/authentication/document/PNOEE-32222222222-Z1B2-Q", "requests/authenticationSessionRequest.json", 580);
            await Assert.ThrowsAsync<ServerMaintenanceException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task SetPollingSleepTimeoutForAuthentication()
        {
            var state = new SmartIdRestServiceStubs.RequestState()
            {
                State = "STARTED"
            };

            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusRunning.json", state, "STARTED", "COMPLETE");
            SmartIdRestServiceStubs.StubSessionStatusWithState(handlerMock, "1dcc1600-29a6-4e95-a95c-d69b31febcfb", "responses/sessionStatusForSuccessfulAuthenticationRequest.json", state, "COMPLETE", "STARTED");
            client.SetPollingSleepTimeout(TimeSpan.FromSeconds(2L));
            double duration = await MeasureAuthenticationDurationAsync();
            Assert.True(duration > 2000L, "Duration is " + duration);
            Assert.True(duration < 3000L, "Duration is " + duration);
        }

        [Fact]
        public void VerifySmartIdConnector_whenConnectorIsNotProvided()
        {
            ISmartIdConnector smartIdConnector = client.SmartIdConnector;
            Assert.True(smartIdConnector is SmartIdRestConnector);
        }

        [Fact]
        public async Task VerifySmartIdConnector_whenConnectorIsProvided()
        {
            const string mock = "MOCK";
            var status = new Mock<SessionStatus>();
            status.SetupGet(x => x.State).Returns(mock);
            var connector = new Mock<ISmartIdConnector>();
            connector.Setup(x => x.GetSessionStatusAsync(null, default)).ReturnsAsync(status.Object);
            client.SmartIdConnector = connector.Object;
            Assert.Equal(mock, (await client.SmartIdConnector.GetSessionStatusAsync(null)).State);
        }

        [Fact]
        public async Task GetCertificate_noIdentifierGiven()
        {
            await Assert.ThrowsAsync<SmartIdClientException>(() =>
                client
                     .GetCertificate()
                     .WithCertificateLevel("ADVANCED")
                     .FetchAsync()
            );
        }

        [Fact]
        public async Task GetCertificateByETSIPNO_ValidSemanticsIdentifier_ShouldReturnValidCertificate()
        {
            SmartIdCertificate cer = await client
                .GetCertificate()
                .WithSemanticsIdentifier(new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            AssertCertificateResponseValid(cer);
        }

        [Fact]
        public async Task GetCertificateByETSIPAS_ValidSemanticsIdentifierAsString_ShouldReturnValidCertificate()
        {
            SmartIdCertificate cer = await client
                .GetCertificate()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.PAS, CountryCode.EE, "987654321012"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            AssertCertificateResponseValid(cer);
        }

        [Fact]
        public async Task GetCertificateByETSIIDC_ValidSemanticsIdentifier_ShouldReturnValidCertificate()
        {
            SmartIdCertificate cer = await client
                .GetCertificate()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.IDC, CountryCode.EE, "AA3456789"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();

            AssertCertificateResponseValid(cer);
        }

        [Fact]
        public async Task GetAuthenticationByETSIPNO_ValidSemanticsIdentifier_ShouldReturnSuccessfulAuthentication()
        {

            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            SmartIdAuthenticationResponse authResponse = await client
                .CreateAuthentication()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .WithAuthenticationHash(authenticationHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();

            AssertAuthenticationResponseValid(authResponse);
        }

        [Fact]
        public async Task GetAuthenticationByETSIPAS_ValidSemanticsIdentifier_ShouldReturnSuccessfulAuthentication()
        {

            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==";
            authenticationHash.HashType = HashType.SHA512;

            SmartIdAuthenticationResponse authResponse = await client
                .CreateAuthentication()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.PAS, CountryCode.EE, "987654321012"))
                .WithCertificateLevel("ADVANCED")
                .WithAuthenticationHash(authenticationHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();

            AssertAuthenticationResponseValid(authResponse);
        }

        [Fact]
        public async Task GetAuthenticationByETSIIDC_ValidSemanticsIdentifier_ShouldReturnSuccessfulAuthentication()
        {

            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            SmartIdAuthenticationResponse authResponse = await client
                .CreateAuthentication()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.IDC, CountryCode.EE, "AA3456789"))
                .WithCertificateLevel("ADVANCED")
                .WithAuthenticationHash(authenticationHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();

            AssertAuthenticationResponseValid(authResponse);
        }

        [Fact]
        public async Task GetSignatureByETSIPNO_ValidSemanticsIdentifier_ShouldReturnSuccessfulSignature()
        {
            SignableHash signableHash = new SignableHash
            {
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=",
                HashType = HashType.SHA256
            };

            SmartIdSignature signResponse = await client
                .CreateSignature()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .WithSignableHash(signableHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signResponse);
        }

        [Fact]
        public async Task GetSignatureByETSIPAS_ValidSemanticsIdentifier_ShouldReturnSuccessfulSignature()
        {

            SignableHash signableHash = new SignableHash
            {
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=",
                HashType = HashType.SHA256
            };

            SmartIdSignature signResponse = await client
                .CreateSignature()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.PAS, CountryCode.EE, "987654321012"))
                .WithCertificateLevel("ADVANCED")
                .WithSignableHash(signableHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signResponse);
        }

        [Fact]
        public async Task GetSignatureByETSIIDC_ValidSemanticsIdentifier_ShouldReturnSuccessfulSignature()
        {

            SignableHash signableHash = new SignableHash
            {
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=",
                HashType = HashType.SHA256
            };

            SmartIdSignature signResponse = await client
                .CreateSignature()
                .WithSemanticsIdentifier(
                    new SemanticsIdentifier(IdentityType.IDC, CountryCode.EE, "AA3456789"))
                .WithCertificateLevel("ADVANCED")
                .WithSignableHash(signableHash)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();

            AssertValidSignatureCreated(signResponse);
        }

        private async Task<double> MeasureSigningDurationAsync()
        {
            DateTime startTime = DateTime.UtcNow;
            SmartIdSignature signature = await CreateSignatureAsync();
            DateTime endTime = DateTime.UtcNow;
            Assert.NotNull(signature);
            return (endTime - startTime).TotalMilliseconds;
        }

        private async Task<SmartIdSignature> CreateSignatureAsync()
        {
            SignableHash hashToSign = new SignableHash
            {
                HashType = HashType.SHA256,
                HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg="
            };
            return await client
                .CreateSignature()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();
        }

        private async Task<double> MeasureAuthenticationDurationAsync()
        {
            DateTime startTime = DateTime.UtcNow;
            SmartIdAuthenticationResponse AuthenticationResponse = await CreateAuthenticationAsync();
            DateTime endTime = DateTime.UtcNow;
            Assert.NotNull(AuthenticationResponse);
            return (endTime - startTime).TotalMilliseconds;
        }

        private async Task<SmartIdAuthenticationResponse> CreateAuthenticationAsync()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==",
                HashType = HashType.SHA512
            };

            return await client
                .CreateAuthentication()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAuthenticationHash(authenticationHash)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();
        }

        private async Task<double> MeasureCertificateChoiceDurationAsync()
        {
            DateTime startTime = DateTime.UtcNow;
            SmartIdCertificate certificate = await client
                .GetCertificate()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();
            DateTime endTime = DateTime.UtcNow;
            Assert.NotNull(certificate);
            return (endTime - startTime).TotalMilliseconds;
        }

        private async Task MakeGetCertificateRequestAsync()
        {
            await client
                .GetCertificate()
                .WithSemanticsIdentifier(new SemanticsIdentifier(IdentityType.PNO, CountryCode.EE, "31111111111"))
                .WithCertificateLevel("ADVANCED")
                .FetchAsync();
        }

        private async Task MakeCreateSignatureRequestAsync()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashType = HashType.SHA256;
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";

            await client
                .CreateSignature()
                .WithDocumentNumber("PNOEE-31111111111")
                .WithSignableHash(hashToSign)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                        Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?") }
                )
                .SignAsync();
        }

        private async Task MakeAuthenticationRequestAsync()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==";
            authenticationHash.HashType = HashType.SHA512;

            await client
                .CreateAuthentication()
                .WithDocumentNumber("PNOEE-32222222222-Z1B2-Q")
                .WithAuthenticationHash(authenticationHash)
                .WithCertificateLevel("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                        Interaction.DisplayTextAndPIN("Log in?") }
                )
                .AuthenticateAsync();
        }

        private void AssertCertificateResponseValid(SmartIdCertificate certificate)
        {
            Assert.NotNull(certificate);
            Assert.NotNull(certificate.Certificate);
            X509Certificate2 cert = certificate.Certificate;
            Assert.Contains("SERIALNUMBER=PNOEE-31111111111", cert.Subject);
            Assert.Equal("PNOEE-31111111111", certificate.DocumentNumber);
            Assert.Equal("QUALIFIED", certificate.CertificateLevel);
        }

        private void AssertValidSignatureCreated(SmartIdSignature signature)
        {
            Assert.NotNull(signature);
            Assert.StartsWith("luvjsi1+1iLN9yfDFEh/BE8h", signature.ValueInBase64);
            Assert.Equal("sha256WithRSAEncryption", signature.AlgorithmName);
            Assert.Equal("displayTextAndPIN", signature.InteractionFlowUsed);
        }

        private void AssertAuthenticationResponseValid(SmartIdAuthenticationResponse authenticationResponse)
        {
            Assert.NotNull(authenticationResponse);
            Assert.Equal("K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==", authenticationResponse.SignedHashInBase64);
            Assert.Equal("OK", authenticationResponse.EndResult);
            Assert.NotNull(authenticationResponse.Certificate);
            Assert.StartsWith("luvjsi1+1iLN9yfDFEh/BE8h", authenticationResponse.SignatureValueInBase64);
            Assert.Equal("sha256WithRSAEncryption", authenticationResponse.AlgorithmName);
            Assert.Equal("PNOEE-31111111111", authenticationResponse.DocumentNumber);
        }
    }
}