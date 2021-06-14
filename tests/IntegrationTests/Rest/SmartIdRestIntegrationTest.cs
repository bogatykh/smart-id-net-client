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

using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId.IntegrationTests.Rest
{
    public class SmartIdRestIntegrationTest
    {
        private const string RELYING_PARTY_UUID = "00000000-0000-0000-0000-000000000000";
        private const string RELYING_PARTY_NAME = "DEMO";
        private const string DOCUMENT_NUMBER = "PNOEE-10101010005-Z1B2-Q";
        private const string DOCUMENT_NUMBER_LT = "PNOLT-10101010005-Z52N-Q";
        private const string DATA_TO_SIGN = "Hello World!";
        private const string CERTIFICATE_LEVEL_QUALIFIED = "QUALIFIED";
        private readonly ISmartIdConnector connector;

        public SmartIdRestIntegrationTest()
        {
            connector = new SmartIdRestConnector("https://sid.demo.sk.ee/smart-id-rp/v2/");
        }

        [Fact]
        public async Task getCertificateAndSignHash()
        {
            CertificateChoiceResponse certificateChoiceResponse = await FetchCertificateChoiceSessionAsync(DOCUMENT_NUMBER_LT);

            SessionStatus sessionStatus = await PollSessionStatusAsync(certificateChoiceResponse.SessionId);
            AssertCertificateChosen(sessionStatus);

            String documentNumber = sessionStatus.Result.DocumentNumber;
            SignatureSessionResponse signatureSessionResponse = await CreateRequestAndFetchSignatureSessionAsync(documentNumber);
            sessionStatus = await PollSessionStatusAsync(signatureSessionResponse.SessionId);
            AssertSignatureCreated(sessionStatus);
        }

        [Fact]
        public async Task authenticate_withSemanticsIdentifier()
        {
            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "010101-10006");

            AuthenticationSessionRequest request = createAuthenticationSessionRequest();
            AuthenticationSessionResponse authenticationSessionResponse = await connector.AuthenticateAsync(semanticsIdentifier, request);

            Assert.NotNull(authenticationSessionResponse);
            Assert.False(string.IsNullOrEmpty(authenticationSessionResponse.SessionId));

            SessionStatus sessionStatus = await PollSessionStatusAsync(authenticationSessionResponse.SessionId);
            assertAuthenticationResponseCreated(sessionStatus);
        }

        [Fact]
        public async Task authenticate_withDocumentNumber()
        {
            AuthenticationSessionRequest request = createAuthenticationSessionRequest();
            AuthenticationSessionResponse authenticationSessionResponse = await connector.AuthenticateAsync(DOCUMENT_NUMBER, request);

            Assert.NotNull(authenticationSessionResponse);
            Assert.False(string.IsNullOrEmpty(authenticationSessionResponse.SessionId));

            SessionStatus sessionStatus = await PollSessionStatusAsync(authenticationSessionResponse.SessionId);

            Assert.Equal("displayTextAndPIN", sessionStatus.InteractionFlowUsed);

            assertAuthenticationResponseCreated(sessionStatus);
        }


        [Fact]
        public async Task authenticate_withDocumentNumber_advancedInteraction()
        {
            AuthenticationSessionRequest authenticationSessionRequest = new AuthenticationSessionRequest();
            authenticationSessionRequest.RelyingPartyUUID = RELYING_PARTY_UUID;
            authenticationSessionRequest.RelyingPartyName = RELYING_PARTY_NAME;
            authenticationSessionRequest.CertificateLevel = CERTIFICATE_LEVEL_QUALIFIED;
            authenticationSessionRequest.HashType = "SHA512";
            authenticationSessionRequest.Hash = calculateHashInBase64(Encoding.UTF8.GetBytes(DATA_TO_SIGN));

            authenticationSessionRequest.AllowedInteractionsOrder = new List<Interaction> {
                    Interaction.ConfirmationMessage("Do you want to log in to internet banking system of Oceanic Bank?"),
                            Interaction.DisplayTextAndPIN("Log into internet banking system?")};

            AuthenticationSessionResponse authenticationSessionResponse = await connector.AuthenticateAsync(DOCUMENT_NUMBER, authenticationSessionRequest);

            Assert.NotNull(authenticationSessionResponse);
            Assert.False(string.IsNullOrEmpty(authenticationSessionResponse.SessionId));

            SessionStatus sessionStatus = await PollSessionStatusAsync(authenticationSessionResponse.SessionId);

            Assert.Equal("confirmationMessage", sessionStatus.InteractionFlowUsed);

            assertAuthenticationResponseCreated(sessionStatus);
        }

        //[Fact] CURRENTLY IGNORED AS DEMO DOESN'T RESPOND BACK IGNORED PROPERTIES
        public async Task getIgnoredProperties_withSign_getIgnoredProperties_withAuthenticate_testAccountsIgnoreVcChoice()
        {
            CertificateChoiceResponse certificateChoiceResponse = await FetchCertificateChoiceSessionAsync(DOCUMENT_NUMBER);

            SessionStatus sessionStatus = await PollSessionStatusAsync(certificateChoiceResponse.SessionId);
            AssertCertificateChosen(sessionStatus);

            String documentNumber = sessionStatus.Result.DocumentNumber;

            SignatureSessionRequest signatureSessionRequest = createSignatureSessionRequest();

            SignatureSessionResponse signatureSessionResponse = await FetchSignatureSessionAsync(documentNumber, signatureSessionRequest);
            sessionStatus = await PollSessionStatusAsync(signatureSessionResponse.SessionId);

            Assert.Equal("displayTextAndPIN", sessionStatus.InteractionFlowUsed);


            AssertSignatureCreated(sessionStatus);
            Assert.NotNull(sessionStatus.IgnoredProperties);

            Assert.Contains("testingIgnored", sessionStatus.IgnoredProperties);
            Assert.Contains("testingIgnoredTwo", sessionStatus.IgnoredProperties);
            Assert.Equal(2, sessionStatus.IgnoredProperties.Length);

        }

        //[Fact] //CURRENTLY IGNORED AS DEMO DOESN'T RESPOND BACK IGNORED PROPERTIES
        public async Task getIgnoredProperties_withAuthenticate()
        {
            AuthenticationSessionRequest authenticationSessionRequest = createAuthenticationSessionRequest();

            SemanticsIdentifier semanticsIdentifier = new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "010101-10006");


            AuthenticationSessionResponse authenticationSessionResponse = await connector.AuthenticateAsync(semanticsIdentifier, authenticationSessionRequest);

            Assert.NotNull(authenticationSessionResponse);
            Assert.False(string.IsNullOrEmpty(authenticationSessionResponse.SessionId));

            SessionStatus sessionStatus = await PollSessionStatusAsync(authenticationSessionResponse.SessionId);

            Assert.Equal("displayTextAndPIN", sessionStatus.InteractionFlowUsed);

            assertAuthenticationResponseCreated(sessionStatus);
            Assert.NotNull(sessionStatus.IgnoredProperties);

            Assert.Contains("testingIgnored", sessionStatus.IgnoredProperties);
            Assert.Contains("testingIgnoredTwo", sessionStatus.IgnoredProperties);
        }

        private async Task<CertificateChoiceResponse> FetchCertificateChoiceSessionAsync(string documentNumber)
        {
            CertificateRequest request = createCertificateRequest();
            CertificateChoiceResponse certificateChoiceResponse = await connector.GetCertificateAsync(documentNumber, request);
            Assert.NotNull(certificateChoiceResponse);
            Assert.False(string.IsNullOrEmpty(certificateChoiceResponse.SessionId));
            return certificateChoiceResponse;
        }

        private CertificateRequest createCertificateRequest()
        {
            CertificateRequest request = new CertificateRequest();
            request.RelyingPartyUUID = RELYING_PARTY_UUID;
            request.RelyingPartyName = RELYING_PARTY_NAME;
            request.CertificateLevel = CERTIFICATE_LEVEL_QUALIFIED;
            return request;
        }

        private async Task<SignatureSessionResponse> CreateRequestAndFetchSignatureSessionAsync(string documentNumber)
        {
            SignatureSessionRequest signatureSessionRequest = createSignatureSessionRequest();
            return await FetchSignatureSessionAsync(documentNumber, signatureSessionRequest);
        }

        private async Task<SignatureSessionResponse> FetchSignatureSessionAsync(string documentNumber, SignatureSessionRequest signatureSessionRequest)
        {
            SignatureSessionResponse signatureSessionResponse = await connector.SignAsync(documentNumber, signatureSessionRequest);
            Assert.False(string.IsNullOrEmpty(signatureSessionResponse.SessionId));
            return signatureSessionResponse;
        }

        private SignatureSessionRequest createSignatureSessionRequest()
        {
            SignatureSessionRequest signatureSessionRequest = new SignatureSessionRequest();
            signatureSessionRequest.RelyingPartyUUID = RELYING_PARTY_UUID;
            signatureSessionRequest.RelyingPartyName = RELYING_PARTY_NAME;
            signatureSessionRequest.CertificateLevel = CERTIFICATE_LEVEL_QUALIFIED;
            signatureSessionRequest.HashType = "SHA512";
            String hashInBase64 = calculateHashInBase64(Encoding.UTF8.GetBytes(DATA_TO_SIGN));
            signatureSessionRequest.Hash = hashInBase64;
            signatureSessionRequest.AllowedInteractionsOrder = new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to bank?") };
            return signatureSessionRequest;
        }

        private AuthenticationSessionRequest createAuthenticationSessionRequest()
        {
            AuthenticationSessionRequest authenticationSessionRequest = new AuthenticationSessionRequest();
            authenticationSessionRequest.RelyingPartyUUID = RELYING_PARTY_UUID;
            authenticationSessionRequest.RelyingPartyName = RELYING_PARTY_NAME;
            authenticationSessionRequest.CertificateLevel = CERTIFICATE_LEVEL_QUALIFIED;
            authenticationSessionRequest.HashType = "SHA512";
            String hashInBase64 = calculateHashInBase64(Encoding.UTF8.GetBytes(DATA_TO_SIGN));
            authenticationSessionRequest.Hash = hashInBase64;

            authenticationSessionRequest.AllowedInteractionsOrder = new List<Interaction> { Interaction.DisplayTextAndPIN("Log into internet banking system") };

            return authenticationSessionRequest;
        }

        private async Task<SessionStatus> PollSessionStatusAsync(string sessionId)
        {
            SessionStatus sessionStatus = null;
            while (sessionStatus == null || string.Equals("RUNNING", sessionStatus.State, StringComparison.OrdinalIgnoreCase))
            {
                sessionStatus = await connector.GetSessionStatusAsync(sessionId);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            Assert.Equal("COMPLETE", sessionStatus.State);
            return sessionStatus;
        }

        private void AssertSignatureCreated(SessionStatus sessionStatus)
        {
            Assert.NotNull(sessionStatus);
            Assert.NotNull(sessionStatus.Signature);
            Assert.False(string.IsNullOrEmpty(sessionStatus.Signature.Value));
        }

        private void AssertCertificateChosen(SessionStatus sessionStatus)
        {
            Assert.NotNull(sessionStatus);
            string documentNumber = sessionStatus.Result.DocumentNumber;
            Assert.False(string.IsNullOrEmpty(documentNumber));
            Assert.False(string.IsNullOrEmpty(sessionStatus.Cert.Value));
        }

        private void assertAuthenticationResponseCreated(SessionStatus sessionStatus)
        {
            Assert.NotNull(sessionStatus);

            Assert.False(string.IsNullOrEmpty(sessionStatus.Result.EndResult));
            Assert.False(string.IsNullOrEmpty(sessionStatus.Cert.Value));
            Assert.False(string.IsNullOrEmpty(sessionStatus.Cert.CertificateLevel));
        }

        private string calculateHashInBase64(byte[] dataToSign)
        {
            byte[] digestValue = DigestCalculator.CalculateDigest(dataToSign, HashType.SHA512);
            return Convert.ToBase64String(digestValue);
        }
    }
}