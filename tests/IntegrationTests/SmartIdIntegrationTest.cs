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
        private const string DOCUMENT_NUMBER = "PNOEE-10101010005-Z1B2-Q";
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
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "10101010005"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOEE-10101010005-Z1B2-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("MIIHWzCCBUOgAwIBAgIQYlSlJAiqEmNch9Rh21QrtjANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJFRTEiMCAGA1UECgwZQVMgU2VydGlmaXRzZWVyaW1pc2tlc2t1czEXMBUGA1UEYQwOTlRSRUUtMTA3NDcwMTMxHDAaBgNVBAMME1RFU1Qgb2YgRUlELVNLIDIwMTYwIBcNMTkwMzEyMTU0NjQxWhgPMjAzMDEyMTcyMzU5NTlaMIGJMRIwEAYDVQQLDAlTSUdOQVRVUkUxKDAmBgNVBAMMH1NNQVJULUlELERFTU8sUE5PRUUtMTAxMDEwMTAwMDUxGjAYBgNVBAUTEVBOT0VFLTEwMTAxMDEwMDA1MQ0wCwYDVQQqDARERU1PMREwDwYDVQQEDAhTTUFSVC1JRDELMAkGA1UEBhMCRUUwggIhMA0GCSqGSIb3DQEBAQUAA4ICDgAwggIJAoICAFInm9JOZh8RPj2JXHViKJkBMopp4ABnPiaCJkUlQFX+OJh1eeSolzOJhQqryhsQMkscnddIDC/U6yWEmqIttE66MPIhlq8ihsAtULoTssanw+US4AE6cFl2G4MJy5DWFQMeUh9fuQoIzCzGBWse0Uj0iVDdob/gSarrct2asvVZpz6tlWTUVgUdQdA+ghhaQ6wXCV9CRUPT5OJxx648Cu9Z0ZH9h0YYP+kl6HzSowYYhactvhjuDK3G4ko23lRI9lGJY2ntiiMby1kpuZWdt714//3bhLpnY+b3ZhrRqLoUf0sITl30bZFNAGcZzDkxQaRIdmrjHdNxnZcCIJg9ML7a2N+yRJWTI5T4mLrnjDSkcHCbfWBvMBCEf9HBGY6oDHJDUHtskFC6M/X912tWcRqST5xogv0WMCxT2jmVZ3N2KthrJ/BQpNihZdr974WlvwAuVgfuPrP//rVUCToIPhvPqriXTAMZI+6Km8BVXpNKOO/El4kY1Iaecke5WQcDywpnVzh1Nh0VhJx2FSyaGtG5+8tVE2xu1b9CVd/DiCO7mz6+piNl/QId6XIYZY8+fW+1HNl6aOJCYqYD7t90JO0DZ6rWn0Ovt65VMEqF7YTvgWsJKwJWxaZSVD99yfhiTSou2aEAXQIjy9176PZrp+x3lFPuVk2FlB8w7Ij3yS4jAgMBAAGjggHcMIIB2DAJBgNVHRMEAjAAMA4GA1UdDwEB/wQEAwIGQDBWBgNVHSAETzBNMEAGCisGAQQBzh8DEQIwMjAwBggrBgEFBQcCARYkaHR0cHM6Ly93d3cuc2suZWUvZW4vcmVwb3NpdG9yeS9DUFMvMAkGBwQAi+xAAQIwHQYDVR0OBBYEFHCpgoim2RknAhmzYufjhA6/PaCDMIGjBggrBgEFBQcBAwSBljCBkzAIBgYEAI5GAQEwFQYIKwYBBQUHCwIwCQYHBACL7EkBATATBgYEAI5GAQYwCQYHBACORgEGATBRBgYEAI5GAQUwRzBFFj9odHRwczovL3NrLmVlL2VuL3JlcG9zaXRvcnkvY29uZGl0aW9ucy1mb3ItdXNlLW9mLWNlcnRpZmljYXRlcy8TAkVOMAgGBgQAjkYBBDAfBgNVHSMEGDAWgBSusOrhNvgmq6XMC2ZV/jodAr8StDB9BggrBgEFBQcBAQRxMG8wKQYIKwYBBQUHMAGGHWh0dHA6Ly9haWEuZGVtby5zay5lZS9laWQyMDE2MEIGCCsGAQUFBzAChjZodHRwczovL3NrLmVlL3VwbG9hZC9maWxlcy9URVNUX29mX0VJRC1TS18yMDE2LmRlci5jcnQwDQYJKoZIhvcNAQELBQADggIBAHJ046Beif8pBPkjY1XsVXs4bhUKuP8ZHjk5BDctu2ZnMzyeMu1Kpy2h95ycBIj/2e7smby8S//TNOQKz+9JOg56Ji5hiyr32BNj9wGYBKH03GIPISf7SKO75Sir3UiBvdcjFlmRlyk9QCR+HDprIsxoc3bsHUh6rWAo/jTPxA2YRxw3uM578Wp58pceoE/uJLsRrK6krUADHleUfZiaVHQNTtKrIRS1Q1OJyu1Clpkv69wb+r0+jOhG4vmcqp/oABTtzLQnorcYuHhR53o9yRIGrFzIOOhjeZnVea/Zbfiq9DEwFxet8joRsn4w3nIPTE3KS/DteNIdMXYioBtuSGlm8S8A8FmtYCCgEpG6LskF2Z/2T4Zoa7BjtN1Hdi8xuQiZkAAENVARRgH+TJE1Jk2HBbbojZlPXq+KZDbjgM4LpJRJjrTDp5qnSudY9hLwO5bsnHvyO5cWE4VgfoTcDud2nQUzL3oE9bjQB7Rc9VkMAyCJx5NDUVAZVuJymAZOix1fBNBIDEsVsYCrlIpBtmUn1ruuF1ANAkwATUd3ZKBgGzHCSJgljhNQVwNFQoHB7Gckw198HU9qSxFiMGd5tGVFmpO5oH6eYR95LPDHidZCjciY353fbX4pTBevIg4rkmdtcEIidcTNDUShB33wl2O9zAvNwGGoolxSyC1/77XO", Convert.ToBase64String(certificateResponse.Certificate.RawData));
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

            Assert.Equal("PNOEE-10101010005-Z1B2-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("MIIHWzCCBUOgAwIBAgIQYlSlJAiqEmNch9Rh21QrtjANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJFRTEiMCAGA1UECgwZQVMgU2VydGlmaXRzZWVyaW1pc2tlc2t1czEXMBUGA1UEYQwOTlRSRUUtMTA3NDcwMTMxHDAaBgNVBAMME1RFU1Qgb2YgRUlELVNLIDIwMTYwIBcNMTkwMzEyMTU0NjQxWhgPMjAzMDEyMTcyMzU5NTlaMIGJMRIwEAYDVQQLDAlTSUdOQVRVUkUxKDAmBgNVBAMMH1NNQVJULUlELERFTU8sUE5PRUUtMTAxMDEwMTAwMDUxGjAYBgNVBAUTEVBOT0VFLTEwMTAxMDEwMDA1MQ0wCwYDVQQqDARERU1PMREwDwYDVQQEDAhTTUFSVC1JRDELMAkGA1UEBhMCRUUwggIhMA0GCSqGSIb3DQEBAQUAA4ICDgAwggIJAoICAFInm9JOZh8RPj2JXHViKJkBMopp4ABnPiaCJkUlQFX+OJh1eeSolzOJhQqryhsQMkscnddIDC/U6yWEmqIttE66MPIhlq8ihsAtULoTssanw+US4AE6cFl2G4MJy5DWFQMeUh9fuQoIzCzGBWse0Uj0iVDdob/gSarrct2asvVZpz6tlWTUVgUdQdA+ghhaQ6wXCV9CRUPT5OJxx648Cu9Z0ZH9h0YYP+kl6HzSowYYhactvhjuDK3G4ko23lRI9lGJY2ntiiMby1kpuZWdt714//3bhLpnY+b3ZhrRqLoUf0sITl30bZFNAGcZzDkxQaRIdmrjHdNxnZcCIJg9ML7a2N+yRJWTI5T4mLrnjDSkcHCbfWBvMBCEf9HBGY6oDHJDUHtskFC6M/X912tWcRqST5xogv0WMCxT2jmVZ3N2KthrJ/BQpNihZdr974WlvwAuVgfuPrP//rVUCToIPhvPqriXTAMZI+6Km8BVXpNKOO/El4kY1Iaecke5WQcDywpnVzh1Nh0VhJx2FSyaGtG5+8tVE2xu1b9CVd/DiCO7mz6+piNl/QId6XIYZY8+fW+1HNl6aOJCYqYD7t90JO0DZ6rWn0Ovt65VMEqF7YTvgWsJKwJWxaZSVD99yfhiTSou2aEAXQIjy9176PZrp+x3lFPuVk2FlB8w7Ij3yS4jAgMBAAGjggHcMIIB2DAJBgNVHRMEAjAAMA4GA1UdDwEB/wQEAwIGQDBWBgNVHSAETzBNMEAGCisGAQQBzh8DEQIwMjAwBggrBgEFBQcCARYkaHR0cHM6Ly93d3cuc2suZWUvZW4vcmVwb3NpdG9yeS9DUFMvMAkGBwQAi+xAAQIwHQYDVR0OBBYEFHCpgoim2RknAhmzYufjhA6/PaCDMIGjBggrBgEFBQcBAwSBljCBkzAIBgYEAI5GAQEwFQYIKwYBBQUHCwIwCQYHBACL7EkBATATBgYEAI5GAQYwCQYHBACORgEGATBRBgYEAI5GAQUwRzBFFj9odHRwczovL3NrLmVlL2VuL3JlcG9zaXRvcnkvY29uZGl0aW9ucy1mb3ItdXNlLW9mLWNlcnRpZmljYXRlcy8TAkVOMAgGBgQAjkYBBDAfBgNVHSMEGDAWgBSusOrhNvgmq6XMC2ZV/jodAr8StDB9BggrBgEFBQcBAQRxMG8wKQYIKwYBBQUHMAGGHWh0dHA6Ly9haWEuZGVtby5zay5lZS9laWQyMDE2MEIGCCsGAQUFBzAChjZodHRwczovL3NrLmVlL3VwbG9hZC9maWxlcy9URVNUX29mX0VJRC1TS18yMDE2LmRlci5jcnQwDQYJKoZIhvcNAQELBQADggIBAHJ046Beif8pBPkjY1XsVXs4bhUKuP8ZHjk5BDctu2ZnMzyeMu1Kpy2h95ycBIj/2e7smby8S//TNOQKz+9JOg56Ji5hiyr32BNj9wGYBKH03GIPISf7SKO75Sir3UiBvdcjFlmRlyk9QCR+HDprIsxoc3bsHUh6rWAo/jTPxA2YRxw3uM578Wp58pceoE/uJLsRrK6krUADHleUfZiaVHQNTtKrIRS1Q1OJyu1Clpkv69wb+r0+jOhG4vmcqp/oABTtzLQnorcYuHhR53o9yRIGrFzIOOhjeZnVea/Zbfiq9DEwFxet8joRsn4w3nIPTE3KS/DteNIdMXYioBtuSGlm8S8A8FmtYCCgEpG6LskF2Z/2T4Zoa7BjtN1Hdi8xuQiZkAAENVARRgH+TJE1Jk2HBbbojZlPXq+KZDbjgM4LpJRJjrTDp5qnSudY9hLwO5bsnHvyO5cWE4VgfoTcDud2nQUzL3oE9bjQB7Rc9VkMAyCJx5NDUVAZVuJymAZOix1fBNBIDEsVsYCrlIpBtmUn1ruuF1ANAkwATUd3ZKBgGzHCSJgljhNQVwNFQoHB7Gckw198HU9qSxFiMGd5tGVFmpO5oH6eYR95LPDHidZCjciY353fbX4pTBevIg4rkmdtcEIidcTNDUShB33wl2O9zAvNwGGoolxSyC1/77XO", Convert.ToBase64String(certificateResponse.Certificate.RawData));
        }

        [Fact]
        public async Task GetCertificateAndSignHash_withValidRelayingPartyAndUser_successfulCertificateRequestAndDataSigning()
        {
            SmartIdCertificate certificateResponse = await client
                 .GetCertificate()
                 .WithRelyingPartyUUID(RELYING_PARTY_UUID)
                 .WithRelyingPartyName(RELYING_PARTY_NAME)
                 .WithDocumentNumber("PNOLT-10101010005-Z52N-Q")
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

            Assert.Equal("DEMO", authenticationIdentity.GivenName);
            Assert.Equal("SMART-ID", authenticationIdentity.Surname);
            Assert.Equal("10101010005", authenticationIdentity.IdentityNumber);
            Assert.Equal("EE", authenticationIdentity.Country);
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