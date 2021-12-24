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

using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId.IntegrationTests
{
    public class SmartIdIntegrationTest
    {
        private const string HOST_URL = "https://sid.demo.sk.ee/smart-id-rp/v2/";
        private const string RELYING_PARTY_UUID = "00000000-0000-0000-0000-000000000000";
        private const string RELYING_PARTY_NAME = "DEMO";
        private const string DOCUMENT_NUMBER = "PNOLT-30303039914-PBZK-Q";
        private const string DATA_TO_SIGN = "Well hello there!";
        private const string CERTIFICATE_LEVEL_QUALIFIED = "QUALIFIED";
        private readonly SmartIdClient client;

        public SmartIdIntegrationTest()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };

            var httpClient = new HttpClient(httpClientHandler);

            client = new SmartIdClient
            {
                RelyingPartyUUID = RELYING_PARTY_UUID,
                RelyingPartyName = RELYING_PARTY_NAME
            };
            client.SetHostUrl(HOST_URL);
            client.SetConfiguredClient(httpClient);
        }

        [Fact]
        public async Task GetCertificate_bySemanticsIdentifier()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                    .WithRelyingPartyName(RELYING_PARTY_NAME)
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "30303039914"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOEE-30303039914-5QSV-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));
        }

        [Fact]
        public async Task GetCertificateEE_byDocumentNumber()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                .WithRelyingPartyName(RELYING_PARTY_NAME)
                .WithDocumentNumber(DOCUMENT_NUMBER)
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                .FetchAsync();

            Assert.Equal("PNOLT-30303039914-PBZK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));
        }

        [Fact]
        public async Task GetCertificateAndSignHash_withValidRelayingPartyAndUser_successfulCertificateRequestAndDataSigning()
        {
            SmartIdCertificate certificateResponse = await client
                 .GetCertificate()
                 .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                 .WithRelyingPartyName(RELYING_PARTY_NAME)
                 .WithDocumentNumber("PNOLT-30303039914-PBZK-Q")
                 .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                 .FetchAsync();

            AssertCertificateChosen(certificateResponse);

            string documentNumber = certificateResponse.DocumentNumber;
            SignableData dataToSign = new SignableData(Encoding.UTF8.GetBytes(DATA_TO_SIGN));

            SmartIdSignature signature = await client
                 .CreateSignature()
                 .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                 .WithRelyingPartyName(RELYING_PARTY_NAME)
                 .WithDocumentNumber(documentNumber)
                 .WithSignableData(dataToSign)
                 .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                 .WithAllowedInteractionsOrder(
                         new List<Interaction> { Interaction.DisplayTextAndPIN("012345678901234567890123456789012345678901234567890123456789") }
                 )
                 .SignAsync();

            AssertSignatureCreated(signature);
        }

        [Fact]
        public async Task Authenticate_withValidUserAndRelayingPartyAndHash_successfulAuthentication()
        {
            AuthenticationHash authenticationHash = AuthenticationHash.GenerateRandomHash();
            Assert.NotNull(authenticationHash.CalculateVerificationCode());

            SmartIdAuthenticationResponse authenticationResponse = await client
                 .CreateAuthentication()
                 .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                 .WithRelyingPartyName(RELYING_PARTY_NAME)
                 .WithDocumentNumber(DOCUMENT_NUMBER)
                 .WithAuthenticationHash(authenticationHash)
                 .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                 .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                 .AuthenticateAsync();

            AssertAuthenticationResponseCreated(authenticationResponse, authenticationHash.HashInBase64);

            AuthenticationResponseValidator authenticationResponseValidator = new AuthenticationResponseValidator();
            AuthenticationIdentity authenticationIdentity = authenticationResponseValidator.Validate(authenticationResponse);

            Assert.Equal("QUALIFIED OK", authenticationIdentity.GivenName);
            Assert.Equal("TESTNUMBER", authenticationIdentity.Surname);
            Assert.Equal("30303039914", authenticationIdentity.IdentityNumber);
            Assert.Equal("LT", authenticationIdentity.Country);
        }

        private void AssertSignatureCreated(SmartIdSignature signature)
        {
            Assert.NotNull(signature);
            Assert.False(string.IsNullOrEmpty(signature.ValueInBase64));
        }

        private void AssertCertificateChosen(SmartIdCertificate certificateResponse)
        {
            Assert.NotNull(certificateResponse);
            Assert.False(string.IsNullOrEmpty(certificateResponse.DocumentNumber));
            Assert.NotNull(certificateResponse.Certificate);
        }

        private void AssertAuthenticationResponseCreated(SmartIdAuthenticationResponse authenticationResponse, string expectedHashToSignInBase64)
        {
            Assert.NotNull(authenticationResponse);
            Assert.False(string.IsNullOrEmpty(authenticationResponse.EndResult));
            Assert.Equal(expectedHashToSignInBase64, authenticationResponse.SignedHashInBase64);
            Assert.False(string.IsNullOrEmpty(authenticationResponse.SignatureValueInBase64));
            Assert.NotNull(authenticationResponse.Certificate);
            Assert.NotNull(authenticationResponse.CertificateLevel);
        }
    }
}