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

            Assert.Equal("PNOLV-030403-10075-ZH4M-Q", certificateResponse.DocumentNumber);
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

        [Fact(Skip = "Missing DEMO account")]
        public async Task GetCertificate_EstonianByDocumentNumber_dateOfBirthParsedFromFieldInCertificate()
        {
            SmartIdCertificate certificateResponse = await client
                .GetCertificate()
                .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                .WithRelyingPartyName(RELYING_PARTY_NAME)
                .WithDocumentNumber("PNOEE-39912319997-AAAA-Q")
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                .WithNonce("012345678901234567890123456789")
                .FetchAsync();

            Assert.Equal("PNOEE-39912319997-AAAA-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificateResponse.Certificate);
            Assert.Equal(new DateTime(1903, 3, 3), identity.DateOfBirth);

            // NB! This certificate has a different date-of-birth value
            // in the attribute of the certificate comparing to
            // what would expect from the national identity number.
            // the following code blocks demonstrate that
            DateTime? dateOfBirthParsedFromCertificate = CertificateAttributeUtil.GetDateOfBirth(identity.AuthCertificate);
            Assert.Equal(new DateTime(1903, 3, 3), dateOfBirthParsedFromCertificate);

            DateTime? dateOfBirthParsedFromNationalIdentityNumber = NationalIdentityNumberUtil.GetDateOfBirth(identity);
            Assert.Null(dateOfBirthParsedFromNationalIdentityNumber);
        }

        [Fact(Skip = "Missing DEMO account")]
        public async Task GetCertificate_LithuanianByDocumentNumber_dateOfBirthParsedFromFieldInCertificate()
        {
            SmartIdCertificate certificateResponse = await client
                    .GetCertificate()
                    .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                    .WithRelyingPartyName(RELYING_PARTY_NAME)
                    .WithDocumentNumber("PNOLT-39912319997-AAAA-Q")
                    .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLT-39912319997-AAAA-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("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", Convert.ToBase64String(certificateResponse.Certificate.RawData));

            AuthenticationIdentity identity = AuthenticationResponseValidator.ConstructAuthenticationIdentity(certificateResponse.Certificate);
            Assert.Equal(new DateTime(1903, 3, 3), identity.DateOfBirth);

            // NB! This certificate has a different date-of-birth value
            // in the attribute of the certificate comparing to
            // what would expect from the national identity number.
            // the following code blocks demonstrate that
            DateTime? dateOfBirthParsedFromCertificate = CertificateAttributeUtil.GetDateOfBirth(identity.AuthCertificate);
            Assert.Equal(new DateTime(1903, 3, 3), dateOfBirthParsedFromCertificate);

            DateTime? dateOfBirthParsedFromNationalIdentityNumber = NationalIdentityNumberUtil.GetDateOfBirth(identity);
            Assert.Null(dateOfBirthParsedFromNationalIdentityNumber);
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
            Assert.Equal("MIIIojCCBoqgAwIBAgIQMIn1C1GQ0CxjhLpGUCvWOzANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJFRTEiMCAGA1UECgwZQVMgU2VydGlmaXRzZWVyaW1pc2tlc2t1czEXMBUGA1UEYQwOTlRSRUUtMTA3NDcwMTMxHDAaBgNVBAMME1RFU1Qgb2YgRUlELVNLIDIwMTYwIBcNMjIxMTI4MTM0MDIyWhgPMjAzMDEyMTcyMzU5NTlaMGMxCzAJBgNVBAYTAkxUMRYwFAYDVQQDDA1URVNUTlVNQkVSLE9LMRMwEQYDVQQEDApURVNUTlVNQkVSMQswCQYDVQQqDAJPSzEaMBgGA1UEBRMRUE5PTFQtMzAzMDMwMzk5MTQwggMiMA0GCSqGSIb3DQEBAQUAA4IDDwAwggMKAoIDAQChuGkmE7wK3W5yw8vESPgyHL/sAHyv+3xcrK2jUUrKHwodOn2wzCioRu26uiZixdpnQbdb4KyZBCdBAIGduo7NdsLpfmwAtyGqenJqsbBX5tpvA4Stwoh4+fK5M1tifMItArpahGc26N0zXijZiNnirwkLmPkRMcYlS1zUuJfLOpwgqca38k4nVkX/PVOmtNSwNCKW+PVOlD0iaePPAqbWqCvkuyvazhyDDzmWqhGsY23+6iJZ/cpKz4B4VzRlzTVUBsGT5PegdETIIHFpvEfN/HtMugrfrTOnkd/Ymk1WbAdsNNLYp3hIAWsdIzSU1VhrShRPtp/QCAvEmpiRnbCTGkyjErAqyscVj2wAWmOagquB1Hb5O4hQ7Ksxp37FHi0zGqzCcanhwWiItOdM7RDmtlG2nGj6T/8iyYIlPwkYFd7fW5ka3agPAZV1y8PuKNh32gcbgnNsYJcBusK5kSynOY/LaSebrmnSc0jkmG4S8odbsNRaVlJGp3QP1qNWBqqFX/jUxTdgA4AxDtKSOpsevhJp/4jhHlAmwQxwuNskpNx65JI6fIrA+IgLy9SUFBQoPsrfwDMwgmJW8Rpjlb4F6y7KVD7z8jyCnIbHK/rMR9w0R4doF2q5Oivf1X4EEqkq9da0uXCMB2BZMex7b4GHAeKS99LaO/A6XfTYhek5qmxzrIYMY/0I3/sieSzdvuaVY0YN4o71Zw70gNgp8xMH9Dze/Lk/2sQjysteNfPzk4rIfMvZrg7TnCDNdzAhgWQ0tDkRM80g+83H9xN+t6aJoXoKe7CVckkFVZxeTtzMAyxJltifIsGa38FdasjWexbYUCw57qRplZifpLPB6YJCOn2n4/qtOY6sA0hkf8t5zuUdI6DXCEKcLyRKX4l0yEdAWzB/0LTnzBcAwoQO9FrCowRBjmGavvOSwJbeolTfCQd1IdxZF5Nk35EQ6qEA2XwdnyfN6JbNdJ1MSXvyLJZiPyKfRcmh0asJzLHJA/CIpOMBupxW9aRG9cJcwpOzfr0CAwEAAaOCAkkwggJFMAkGA1UdEwQCMAAwDgYDVR0PAQH/BAQDAgZAMF0GA1UdIARWMFQwRwYKKwYBBAHOHwMRAjA5MDcGCCsGAQUFBwIBFitodHRwczovL3NraWRzb2x1dGlvbnMuZXUvZW4vcmVwb3NpdG9yeS9DUFMvMAkGBwQAi+xAAQIwHQYDVR0OBBYEFEWgA59+SJ1W3kWYF3wqP8MQxocUMIGuBggrBgEFBQcBAwSBoTCBnjAIBgYEAI5GAQEwFQYIKwYBBQUHCwIwCQYHBACL7EkBATATBgYEAI5GAQYwCQYHBACORgEGATBcBgYEAI5GAQUwUjBQFkpodHRwczovL3NraWRzb2x1dGlvbnMuZXUvZW4vcmVwb3NpdG9yeS9jb25kaXRpb25zLWZvci11c2Utb2YtY2VydGlmaWNhdGVzLxMCRU4wCAYGBACORgEEMB8GA1UdIwQYMBaAFK6w6uE2+CarpcwLZlX+Oh0CvxK0MHwGCCsGAQUFBwEBBHAwbjApBggrBgEFBQcwAYYdaHR0cDovL2FpYS5kZW1vLnNrLmVlL2VpZDIwMTYwQQYIKwYBBQUHMAKGNWh0dHA6Ly9zay5lZS91cGxvYWQvZmlsZXMvVEVTVF9vZl9FSUQtU0tfMjAxNi5kZXIuY3J0MDAGA1UdEQQpMCekJTAjMSEwHwYDVQQDDBhQTk9MVC0zMDMwMzAzOTkxNC1NT0NLLVEwKAYDVR0JBCEwHzAdBggrBgEFBQcJATERGA8xOTAzMDMwMzEyMDAwMFowDQYJKoZIhvcNAQELBQADggIBAEOyA9CFBa1mpmZbFOb0giIQE/VenBLd1oZBupVm7VcW+pjR51JF7NBY+fcDkhx0vUB3bWobo2ivlqcUH7OpeROzyVgZCMdL7ezLTx1qEDPO6IcsYU1jTEsaJhTplbtBVJ0I43SJlF/mSQ/ypK9zNy40E7JWY070ewypdI9AmiG7cjRfD5gNgBK00mllNhLPK53L4+NIrBv22pvm9v4C5xEFTjCiHgd3lWXFcDKaM206k5wUf1LrcGNRQb4yS4SbToiqSdAxGoFJ3wpxpdv96ujo0ylMch1lmf/yA1pCnxys+qMCoTToPF4vtjj/1vWg0csD3UrFuLwHwuweWsWSqJVXUb9LfpPgfM/lPdQO2hQ1cVpXDBVnLAXfGfFcSX1CFnHpT5BKqlhIPDFJSB34F4yjqCMosL4Rvm35bniv2WXkQ9Cfsx1dueNB4CX7Wtc7wp5wRPiwAxAN9fmRRlKCxny/1h3/wGwfTlTixZ8PpcvdgcDdQEsssL6CY+1WEp8EPUvJetT8qKnd8KtpudV2bCBj8Z8xlAQYknz4CN+LSGbnoUqmeRvkReviE3E9SMazgL4Dm8hQ5qQc9xmq6YJpCz589dNEm2Ljy8eXvZ8NRbx0Wua0puqTm9prSDL/817mgq475GagBP9bCimzdBtfYZU+oCkHhaIeiZsqtYCNkMHd", Convert.ToBase64String(certificateResponse.Certificate.RawData));
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