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
using SK.SmartId.Util;
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
        private const string DOCUMENT_NUMBER = "PNOLT-30303039914-MOCK-Q";
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
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LT, "30303039914"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLT-30303039914-MOCK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));
        }

        [Fact]
        public async Task getCertificate_bySemanticsIdentifier_latvian()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                    .WithRelyingPartyName(RELYING_PARTY_NAME)
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "030403-10075"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLV-030403-10075-MOCK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));
        }

        [Fact]
        public async Task getCertificate_bySemanticsIdentifier_dateOfBirthParsedFromFieldInCertificate()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                    .WithRelyingPartyName(RELYING_PARTY_NAME)
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "030303-10215"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLV-030303-10215-MOCK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificateResponse.Certificate);
            Assert.NotNull(identity.DateOfBirth);
            Assert.Equal(new DateTime(1903, 3, 3), identity.DateOfBirth);
        }

        [Fact]
        public async Task GetCertificate_EstonianByDocumentNumber_dateOfBirthParsedFromFieldInCertificate()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                .WithRelyingPartyName(RELYING_PARTY_NAME)
                .WithDocumentNumber("PNOEE-40404049996-MOCK-Q")
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                .WithNonce("012345678901234567890123456789")
                .FetchAsync();

            Assert.Equal("PNOEE-40404049996-MOCK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("MIIIoTCCBomgAwIBAgIQDnRWtLc1cm9jj2SA/ncFwzANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJFRTEiMCAGA1UECgwZQVMgU2VydGlmaXRzZWVyaW1pc2tlc2t1czEXMBUGA1UEYQwOTlRSRUUtMTA3NDcwMTMxHDAaBgNVBAMME1RFU1Qgb2YgRUlELVNLIDIwMTYwIBcNMjIxMjA2MTU0OTE5WhgPMjAzMDEyMTcyMzU5NTlaMGMxCzAJBgNVBAYTAkVFMRYwFAYDVQQDDA1URVNUTlVNQkVSLE9LMRMwEQYDVQQEDApURVNUTlVNQkVSMQswCQYDVQQqDAJPSzEaMBgGA1UEBRMRUE5PRUUtNDA0MDQwNDk5OTYwggMhMA0GCSqGSIb3DQEBAQUAA4IDDgAwggMJAoIDAHTW5RQN6eA/Iu51xFsFGJKyepBpovEzZ33XfvzJUbuNlsaQC/gEGZqkSG1NqcLx00AJXyxWiWXfwv5PGYYZoS4MVLFacUT/WkiI/cth6PevslhDVYxITooCYMhirmimKHvPd01XVzbGpvO498zW3qetLsv/FZcQyNV0Xh4JTVPEk05j6nQSZNh5dHSBzvLe41fzKPCw+N5KV3Szr3+Ov0i00jNbdV5kHgqSCvbr46iWrnew8MTO+Se6O4LatlZkAocwIQgpuYmvGL/ThhUHws4uVyKFHpdFsxdBA3BD4PpsXp3g4we3FNl2ZCj9W/o25jY3kryHcGZimE2iYa/139kpu+RggXZDQlQ+R6/p6ClM2W53hAtcr0HnZ+VEhMZ88MQTjvgqntyrMVbFqYrkpmlC5CPYhO5UDrUS6VFnv46iKP69QddWSkFQMUvjg7YDCGwFWtagYhRLK2hjTc3bF6CAV436SnDasY67RIFJrIrYnRbj0lv8SPph6nv/+khXwYp/DeF9xriuy69tPtoFlA3LxCeqPMMrUNgY3o/GcNqVh0TrUB0671DR9jmTrjl1dWfie6xdyO255MHWptBO1wys85LKNuy822DS0tdQLOZHsGXSNYCJUn0//9eeAMApX1a720G/C6qwyRf/wX1N1qhPJgMpTCFaWxfgmjFjYPnw7JjP+cCqZyIIH4+PPirLu1awVtcuPtTEHDEkUWnELKouXSltw8OpcblIs8ocVdfSy0Mil+09yz1fawi2zgulfLOj8I/liJo8c9KFvwOotFYRf2qVV8VuLM4OS1ucSLIH+fp2PtnyjyZOy1+2J0KlrxHRrTTejLRS/i4fkq+VWg2hIoAsYgpwgRNPqN7jvdaguaQcqyc9E8ht+w9pWep/SexC9bCKaDp8GUHu9ft9emoJQOOLB4RtI+O6V4arC8T3UbelL9u4zodKpUJiC2GTl8U6IrKjMSYqNObCbRM+fwF83/VP6WEK71EN3S9kFWRnGYE/bamIEaIBte3bc9cuIQIDAQABo4ICSTCCAkUwCQYDVR0TBAIwADAOBgNVHQ8BAf8EBAMCBkAwXQYDVR0gBFYwVDBHBgorBgEEAc4fAxECMDkwNwYIKwYBBQUHAgEWK2h0dHBzOi8vc2tpZHNvbHV0aW9ucy5ldS9lbi9yZXBvc2l0b3J5L0NQUy8wCQYHBACL7EABAjAdBgNVHQ4EFgQUaiwzCeEb6XKZ5WlgUMZj5/7264wwga4GCCsGAQUFBwEDBIGhMIGeMAgGBgQAjkYBATAVBggrBgEFBQcLAjAJBgcEAIvsSQEBMBMGBgQAjkYBBjAJBgcEAI5GAQYBMFwGBgQAjkYBBTBSMFAWSmh0dHBzOi8vc2tpZHNvbHV0aW9ucy5ldS9lbi9yZXBvc2l0b3J5L2NvbmRpdGlvbnMtZm9yLXVzZS1vZi1jZXJ0aWZpY2F0ZXMvEwJFTjAIBgYEAI5GAQQwHwYDVR0jBBgwFoAUrrDq4Tb4JqulzAtmVf46HQK/ErQwfAYIKwYBBQUHAQEEcDBuMCkGCCsGAQUFBzABhh1odHRwOi8vYWlhLmRlbW8uc2suZWUvZWlkMjAxNjBBBggrBgEFBQcwAoY1aHR0cDovL3NrLmVlL3VwbG9hZC9maWxlcy9URVNUX29mX0VJRC1TS18yMDE2LmRlci5jcnQwMAYDVR0RBCkwJ6QlMCMxITAfBgNVBAMMGFBOT0VFLTQwNDA0MDQ5OTk2LU1PQ0stUTAoBgNVHQkEITAfMB0GCCsGAQUFBwkBMREYDzE5MDQwNDA0MTIwMDAwWjANBgkqhkiG9w0BAQsFAAOCAgEAFdJJqV/lvpVU489Ti0//cgynwgTE99wAVBpArgd8rD8apVMBoEn+Tu0Lez5YnfbK6+Dx1WvdM4t74xxkUlXkMIXLJI6iYM6mDiueDTvF94k51f1UWQo+/0GVO+dIDE1gmIm5K3eV/J7+/duSkrA72VHNJGCd8HVnj2UUOvo5VLBfQi7WjGjhff8LBXINUnBHIfs6CXrDJiLPwQQy/5pv03maJOG+isPT/IrhnkYBgOWDKaPCAkAvaGDaAPJGVNpu4QijuqKEzKrW9AGpmf1WxPhnp63zWOiEYuPhuqUnKH2IqG9gThi2l23zKU/7EbxOLd1vrElqAyHLvLS/PgSgiR/XxBUotxceeXYtnL20NxfzuYdEM1gz8UFyix4M5L905j/5Yuwksq/QN0c1A3gFQtHhtVrlSxzQpipd967HJezJxdsh6VlxuI0r6MSzcDOYVkOo3oE1sV/kyHtnhdWAVOh9u3EVtXBPyfWOMcPiloIDTJhbQ0pJFRLgEELSlYwObDzeqtRXMmtNpilK3feKu98PQekaQp1xv4dHyMIUsKLxNgyhGtV9o1mWoGpFaQImsF8jDeP2XckzmWh7s33SDm1/O4BgyyXbMNOa3HjP6l8LKb341M2lQAGs6JjelwIkOOUGYKr56SYshueeC92Xd/kOUY+pTCFQ87krYpBFETk=", Convert.ToBase64String(certificateResponse.Certificate.RawData));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificateResponse.Certificate);
            Assert.Equal(new DateTime(1904, 4, 4), identity.DateOfBirth);
        }

        [Fact]
        public async Task GetCertificate_LithuanianByDocumentNumber_dateOfBirthParsedFromFieldInCertificate()
        {
            SmartIdCertificate certificateResponse = await client
                    .GetCertificate()
                    .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                    .WithRelyingPartyName(RELYING_PARTY_NAME)
                    .WithDocumentNumber("PNOLT-30303039816-MOCK-Q")
                    .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLT-30303039816-MOCK-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificateResponse.Certificate);
            Assert.Equal(new DateTime(1903, 3, 3), identity.DateOfBirth);
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

            Assert.Equal("PNOLT-30303039914-MOCK-Q", certificateResponse.DocumentNumber);
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
                 .WithDocumentNumber("PNOLT-30303039914-MOCK-Q")
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

            Assert.Equal("OK", authenticationIdentity.GivenName);
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