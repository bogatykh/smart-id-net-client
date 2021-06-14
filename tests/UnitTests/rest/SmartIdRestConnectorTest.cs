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
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId.Rest
{
    public class SmartIdRestConnectorTest
    {
        private Mock<HttpMessageHandler> handlerMock;
        private ISmartIdConnector connector;

        public SmartIdRestConnectorTest()
        {
            handlerMock = new Mock<HttpMessageHandler>();
            connector = new SmartIdRestConnector("http://localhost:18089", new HttpClient(handlerMock.Object));
        }

        [Fact]
        public async Task getNotExistingSessionStatus()
        {
            SmartIdRestServiceStubs.stubNotFoundResponse(handlerMock, "/session/de305d54-75b4-431b-adb2-eb6b9e546016");
            SessionStatusRequest request = new SessionStatusRequest("de305d54-75b4-431b-adb2-eb6b9e546016");
            await Assert.ThrowsAsync<SessionNotFoundException>(() => connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016"));
        }

        [Fact]
        public async Task getRunningSessionStatus()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusRunning.json");
            Assert.NotNull(sessionStatus);
            Assert.Equal("RUNNING", sessionStatus.State);
        }

        [Fact]
        public async Task getRunningSessionStatus_withIgnoredProperties()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusRunningWithIgnoredProperties.json");
            Assert.NotNull(sessionStatus);
            Assert.Equal("RUNNING", sessionStatus.State);
            Assert.NotNull(sessionStatus.IgnoredProperties);
            Assert.Equal(2, sessionStatus.IgnoredProperties.Length);
            Assert.Equal("testingIgnored", sessionStatus.IgnoredProperties[0]);
            Assert.Equal("testingIgnoredTwo", sessionStatus.IgnoredProperties[1]);
        }

        [Fact]
        public async Task getSessionStatus_forSuccessfulCertificateRequest()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusForSuccessfulCertificateRequest.json");
            assertSuccessfulResponse(sessionStatus);
            Assert.NotNull(sessionStatus.Cert);
            Assert.StartsWith("MIIHhjCCBW6gAwIBAgIQDNYLtVwrKURYStrYApYViTANBgkqhkiG9", sessionStatus.Cert.Value);
            Assert.Equal("QUALIFIED", sessionStatus.Cert.CertificateLevel);
        }

        [Fact]
        public async Task getSessionStatus_forSuccessfulSigningRequest()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusForSuccessfulSigningRequest.json");
            assertSuccessfulResponse(sessionStatus);
            Assert.NotNull(sessionStatus.Signature);
            Assert.StartsWith("luvjsi1+1iLN9yfDFEh/BE8hXtAKhAIxilv", sessionStatus.Signature.Value);
            Assert.Equal("sha256WithRSAEncryption", sessionStatus.Signature.Algorithm);
        }

        [Fact]
        public async Task getSessionStatus_userHasRefused()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedGeneral.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED");
        }

        [Fact]
        public async Task getSessionStatus_userHasRefusedConfirmationMessage()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedConfirmationMessage.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED_CONFIRMATIONMESSAGE");
        }

        [Fact]
        public async Task getSessionStatus_userHasRefusedRefusedConfirmationMessageWithVerificationCodeChoice()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedConfirmationMessageWithVerificationCodeChoice.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED_CONFIRMATIONMESSAGE_WITH_VC_CHOICE");
        }

        [Fact]
        public async Task getSessionStatus_userHasRefusedWhenUserRefusedDisplayTextAndPin()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedDisplayTextAndPin.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED_DISPLAYTEXTANDPIN");
        }

        [Fact]
        public async Task getSessionStatus_userHasRefusedWhenUserRefusedGeneral()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedGeneral.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED");
        }

        [Fact]
        public async Task getSessionStatus_userHasRefusedWhenUserRefusedVerificationCodeChoice()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserRefusedVerificationCodeChoice.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "USER_REFUSED_VC_CHOICE");
        }

        [Fact]
        public async Task getSessionStatus_timeout()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenTimeout.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "TIMEOUT");
        }

        [Fact]
        public async Task getSessionStatus_userHasSelectedWrongVcCode()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenUserHasSelectedWrongVcCode.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "WRONG_VC");
        }

        [Fact]
        public async Task getSessionStatus_whenDocumentUnusable()
        {
            SessionStatus sessionStatus = await GetStubbedSessionStatusWithResponseAsync("responses/sessionStatusWhenDocumentUnusable.json");
            assertSessionStatusErrorWithEndResult(sessionStatus, "DOCUMENT_UNUSABLE");
        }

        [Fact]
        public async Task getSessionStatus_withTimeoutParameter()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/de305d54-75b4-431b-adb2-eb6b9e546016?timeoutMs=10000", "responses/sessionStatusForSuccessfulCertificateRequest.json");
            connector.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromSeconds(10L));
            SessionStatus sessionStatus = await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
            assertSuccessfulResponse(sessionStatus);
        }

        [Fact]
        public async Task getCertificate_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");
            CertificateRequest request = createDummyCertificateRequest();
            CertificateChoiceResponse response = await connector.GetCertificateAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", response.SessionId);
        }

        [Fact]
        public async Task getCertificate_usingSemanticsIdentifier()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/PASKZ-987654321012", "requests/certificateChoiceRequest.json", "responses/certificateChoiceResponse.json");

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier("PASKZ-987654321012");

            CertificateRequest request = createDummyCertificateRequest();
            CertificateChoiceResponse response = await connector.GetCertificateAsync(semanticsIdentifier, request);
            Assert.NotNull(response);
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", response.SessionId);
        }

        [Fact]
        public async Task getCertificate_withNonce_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequestWithNonce.json", "responses/certificateChoiceResponse.json");
            CertificateRequest request = createDummyCertificateRequest();
            request.Nonce = "zstOt2umlc";
            CertificateChoiceResponse response = await connector.GetCertificateAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", response.SessionId);
        }

        [Fact]
        public async Task getCertificate_withNonce_usingSemanticsIdentifier()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/certificatechoice/etsi/IDCCZ-1234567890", "requests/certificateChoiceRequestWithNonce.json", "responses/certificateChoiceResponse.json");
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.IDC, "CZ", "1234567890");
            CertificateRequest request = createDummyCertificateRequest();
            request.Nonce = "zstOt2umlc";
            CertificateChoiceResponse response = await connector.GetCertificateAsync(semanticsIdentifier, request);
            Assert.NotNull(response);
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", response.SessionId);
        }

        //[Fact] (expected = UserAccountNotFoundException)
        //public void getCertificate_whenDocumentNumberNotFound_shoudThrowException()
        //{
        //    stubNotFoundResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json");
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        //[Fact] (expected = UserAccountNotFoundException)
        //public void getCertificate_semanticsIdentifierNotFound_shouldThrowException()
        //{
        //    stubNotFoundResponse("/certificatechoice/etsi/IDCCZ-1234567890", "requests/certificateChoiceRequest.json");

        //    SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier("IDCCZ-1234567890");

        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate(semanticsIdentifier, request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void getCertificate_withWrongAuthenticationParams_shuldThrowException()
        //{
        //    stubUnauthorizedResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json");
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void getCertificate_withWrongRequestParams_shouldThrowException()
        //{
        //    stubBadRequestResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json");
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void getCertificate_whenRequestForbidden_shouldThrowException()
        //{
        //    stubForbiddenResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json");
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void getCertificate_whenClientSideAPIIsNotSupportedAnymore_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json", 480);
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        //[Fact] (expected = ServerMaintenanceException)
        //public void getCertificate_whenSystemUnderMaintenance_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/certificatechoice/document/PNOEE-123456", "requests/certificateChoiceRequest.json", 580);
        //    CertificateRequest request = createDummyCertificateRequest();
        //    connector.getCertificate("PNOEE-123456", request);
        //}

        [Fact]
        public async Task sign_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();
            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }



        [Fact]
        public async Task sign_withNonce_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signatureSessionRequestWithNonce.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();
            request.Nonce = "zstOt2umlc";
            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        [Fact]
        public async Task sign_withAllowedInteractionsOrder_confirmationMessageAndFallbackToDisplayTextAndPIN()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signingRequest_confirmationMessage_fallbackTo_displayTextAndPIN.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();

            Interaction confirmationMessageInteraction = Interaction.ConfirmationMessage("Do you want to transfer 200 Bison dollars from savings account to Oceanic Airlines?");
            Interaction fallbackInteraction = Interaction.DisplayTextAndPIN("Transfer 200 BSD to Oceanic Airlines?");
            request.AllowedInteractionsOrder = new List<Interaction> { confirmationMessageInteraction, fallbackInteraction };

            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        [Fact]
        public async Task sign_withAllowedInteractionsOrder_confirmationMessageAndNoFallback()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signingRequest_confirmationMessage_noFallback.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();

            Interaction confi = Interaction.ConfirmationMessage("Do you want to transfer 999 Bison dollars from savings account to Oceanic Airlines?");
            request.AllowedInteractionsOrder = new List<Interaction> { confi };

            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        [Fact]
        public async Task sign_withAllowedInteractionsOrder_verificationCodeChoiceAndFallbackToDisplayTextAndPIN()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signingRequest_verificationCodeChoice_fallbackTo_displayTextAndPIN.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();

            Interaction verificationCodeChoice = Interaction.VerificationCodeChoice("Transfer 444 BSD to Oceanic Airlines?");
            Interaction fallbackToDisplayTextAndPIN = Interaction.DisplayTextAndPIN("Transfer 444 BSD to Oceanic Airlines?");
            request.AllowedInteractionsOrder = new List<Interaction> { verificationCodeChoice, fallbackToDisplayTextAndPIN };

            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        [Fact]
        public async Task sign_withAllowedInteractionsOrder_confirmationMessageAndFallbackToVerificationCodeChoice()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signingRequest_confirmationMessage_fallbackTo_verificationCodeChoice.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();

            Interaction confirmationMessage = Interaction.ConfirmationMessage("Do you want to transfer 707 Bison dollars from savings account to Oceanic Airlines?");
            Interaction fallbackToVerificationCodeChoice = Interaction.VerificationCodeChoice("Transfer 707 BSD to Oceanic Airlines?");
            request.AllowedInteractionsOrder = new List<Interaction> { confirmationMessage, fallbackToVerificationCodeChoice };

            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        [Fact]
        public async Task sign_withAllowedInteractionsOrder_confirmationMessageAndVerificationCodeChoice_fallbackToVerificationCodeChoice()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/signature/document/PNOEE-123456", "requests/signingRequest_confirmationMessageAndVerificationCodeChoice_fallbackTo_verificationCodeChoice.json", "responses/signatureSessionResponse.json");
            SignatureSessionRequest request = createDummySignatureSessionRequest();

            Interaction confirmationMessage = Interaction.ConfirmationMessage("Do you want to transfer 707 Bison dollars from savings account to Oceanic Airlines?");
            Interaction fallbackToVerificationCodeChoice = Interaction.VerificationCodeChoice("Transfer 707 BSD to Oceanic Airlines?");
            request.AllowedInteractionsOrder = new List<Interaction> { confirmationMessage, fallbackToVerificationCodeChoice };

            SignatureSessionResponse response = await connector.SignAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("2c52caf4-13b0-41c4-bdc6-aa268403cc00", response.SessionId);
        }

        //[Fact] (expected = UserAccountNotFoundException)
        //public void sign_whenDocumentNumberNotFound_shouldThrowException()
        //{
        //    stubNotFoundResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json");
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void sign_withWrongAuthenticationParams_shouldThrowException()
        //{
        //    stubUnauthorizedResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json");
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void sign_withWrongRequestParams_shouldThrowException()
        //{
        //    stubBadRequestResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json");
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void sign_whenRequestForbidden_shouldThrowException()
        //{
        //    stubForbiddenResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json");
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void sign_whenClientSideAPIIsNotSupportedAnymore_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json", 480);
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        //[Fact] (expected = ServerMaintenanceException)
        //public void sign_whenSystemUnderMaintenance_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/signature/document/PNOEE-123456", "requests/signatureSessionRequest.json", 580);
        //    SignatureSessionRequest request = createDummySignatureSessionRequest();
        //    connector.sign("PNOEE-123456", request);
        //}

        [Fact]
        public async Task authenticate_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");
            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            AuthenticationSessionResponse response = await connector.AuthenticateAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }

        [Fact]
        public async Task authenticate_usingSemanticsIdentifier()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/PASKZ-987654321012", "requests/authenticationSessionRequest.json", "responses/authenticationSessionResponse.json");

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PAS, "KZ", "987654321012");

            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            AuthenticationSessionResponse response = await connector.AuthenticateAsync(semanticsIdentifier, request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }

        [Fact]
        public async Task authenticate_withNonce_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-123456", "requests/authenticationSessionRequestWithNonce.json", "responses/authenticationSessionResponse.json");
            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            request.Nonce = "g9rp4kjca3";
            AuthenticationSessionResponse response = await connector.AuthenticateAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }

        [Fact]
        public async Task authenticate_withNonce_usingSemanticsIdentifier()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/PASEE-48308230504", "requests/authenticationSessionRequestWithNonce.json", "responses/authenticationSessionResponse.json");

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PAS, SemanticsIdentifier.CountryCode.EE, "48308230504");

            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            request.Nonce = "g9rp4kjca3";
            AuthenticationSessionResponse response = await connector.AuthenticateAsync(semanticsIdentifier, request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }


        [Fact]
        public async Task authenticate_withSingleAllowedInteraction_usingSemanticsIdentifier()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/etsi/PNOLT-48010010101", "requests/authenticationSessionRequestWithSingleAllowedInteraction.json", "responses/authenticationSessionResponse.json");

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier("PNOLT-48010010101");

            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            request.AllowedInteractionsOrder = new List<Interaction> { Interaction.DisplayTextAndPIN("Log into internet banking system") };

            AuthenticationSessionResponse response = await connector.AuthenticateAsync(semanticsIdentifier, request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }


        [Fact]
        public async Task authenticate_withSingleAllowedInteraction_usingDocumentNumber()
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/authentication/document/PNOEE-123456", "requests/authenticationSessionRequestWithSingleAllowedInteraction.json", "responses/authenticationSessionResponse.json");
            AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
            request.AllowedInteractionsOrder = new List<Interaction> { Interaction.DisplayTextAndPIN("Log into internet banking system") };

            AuthenticationSessionResponse response = await connector.AuthenticateAsync("PNOEE-123456", request);
            Assert.NotNull(response);
            Assert.Equal("1dcc1600-29a6-4e95-a95c-d69b31febcfb", response.SessionId);
        }

        //[Fact] (expected = UserAccountNotFoundException)
        //public void authenticate_whenDocumentNumberNotFound_shouldThrowException()
        //{
        //    stubNotFoundResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json");
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        //[Fact] (expected = UserAccountNotFoundException)
        //public void authenticate_whenSemanticsIdentifierNotFound_shouldThrowException()
        //{
        //    stubNotFoundResponse("/authentication/etsi/IDCLV-230883-19894", "requests/authenticationSessionRequest.json");

        //    SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.IDC, SemanticsIdentifier.CountryCode.LV, "230883-19894");

        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate(semanticsIdentifier, request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void authenticate_withWrongAuthenticationParams_shuldThrowException()
        //{
        //    stubUnauthorizedResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json");
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void authenticate_withWrongRequestParams_shouldThrowException()
        //{
        //    stubBadRequestResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json");
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        //[Fact] (expected = RelyingPartyAccountConfigurationException)
        //public void authenticate_whenRequestForbidden_shouldThrowException()
        //{
        //    stubForbiddenResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json");
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        //[Fact] (expected = SmartIdClientException)
        //public void authenticate_whenClientSideAPIIsNotSupportedAnymore_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json", 480);
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        //[Fact] (expected = ServerMaintenanceException)
        //public void authenticate_whenSystemUnderMaintenance_shouldThrowException()
        //{
        //    SmartIdRestServiceStubs.stubErrorResponse("/authentication/document/PNOEE-123456", "requests/authenticationSessionRequest.json", 580);
        //    AuthenticationSessionRequest request = createDummyAuthenticationSessionRequest();
        //    connector.authenticate("PNOEE-123456", request);
        //}

        private void assertSuccessfulResponse(SessionStatus sessionStatus)
        {
            Assert.Equal("COMPLETE", sessionStatus.State);
            Assert.NotNull(sessionStatus.Result);
            Assert.Equal("OK", sessionStatus.Result.EndResult);
            Assert.Equal("PNOEE-31111111111", sessionStatus.Result.DocumentNumber);
        }

        private void assertSessionStatusErrorWithEndResult(SessionStatus sessionStatus, string endResult)
        {
            Assert.Equal("COMPLETE", sessionStatus.State);
            Assert.Equal(endResult, sessionStatus.Result.EndResult);
        }

        private async Task<SessionStatus> GetStubbedSessionStatusWithResponseAsync(string responseFile)
        {
            SmartIdRestServiceStubs.StubRequestWithResponse(handlerMock, "/session/de305d54-75b4-431b-adb2-eb6b9e546016", responseFile);
            return await connector.GetSessionStatusAsync("de305d54-75b4-431b-adb2-eb6b9e546016");
        }

        private CertificateRequest createDummyCertificateRequest()
        {
            CertificateRequest request = new CertificateRequest();
            request.RelyingPartyUUID = "de305d54-75b4-431b-adb2-eb6b9e546014";
            request.RelyingPartyName = "BANK123";
            request.CertificateLevel = "ADVANCED";
            return request;
        }

        private SignatureSessionRequest createDummySignatureSessionRequest()
        {
            SignatureSessionRequest request = new SignatureSessionRequest();
            request.RelyingPartyUUID = "de305d54-75b4-431b-adb2-eb6b9e546014";
            request.RelyingPartyName = "BANK123";
            request.CertificateLevel = "ADVANCED";
            request.Hash = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            request.HashType = "SHA256";
            request.AllowedInteractionsOrder = new List<Interaction> {
                Interaction.ConfirmationMessage("Authorize transfer of 1 unit from account 113245344343 to account 7677323232?"),
                Interaction.DisplayTextAndPIN("Transfer 1 unit to account 7677323232?")
            };
            return request;
        }

        private AuthenticationSessionRequest createDummyAuthenticationSessionRequest()
        {
            AuthenticationSessionRequest request = new AuthenticationSessionRequest();
            request.RelyingPartyUUID = "de305d54-75b4-431b-adb2-eb6b9e546014";
            request.RelyingPartyName = "BANK123";
            request.CertificateLevel = "ADVANCED";
            request.Hash = "K74MSLkafRuKZ1Ooucvh2xa4Q3nz+R/hFWIShN96SPHNcem+uQ6mFMe9kkJQqp5EaoZnJeaFpl310TmlzRgNyQ==";
            request.HashType = "SHA512";
            request.AllowedInteractionsOrder = new List<Interaction> {
                Interaction.ConfirmationMessageAndVerificationCodeChoice("Log in to self-service?"),
                Interaction.DisplayTextAndPIN("Log in?")
            };
            return request;
        }
    }
}