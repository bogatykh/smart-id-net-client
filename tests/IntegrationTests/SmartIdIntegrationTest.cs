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
        private const string DOCUMENT_NUMBER = "PNOLT-30303039914-PBZK-Q";
        private const string DATA_TO_SIGN = "Well hello there!";
        private const string CERTIFICATE_LEVEL_QUALIFIED = "QUALIFIED";
        private readonly SmartIdClient client;

        /**
         *  Allows switching off tests going against smart-id demo env.
         *  This is sometimes needed if the test data in smart-id is temporarily broken.
         */
        public static readonly bool TEST_AGAINST_SMART_ID_DEMO = true;

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

            Assert.Equal("PNOLT-30303039914-PBZK-Q", certificateResponse.DocumentNumber);
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
                    .WithSemanticsIdentifier(new SemanticsIdentifier(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.LV, "329999-99901"))
                .WithCertificateLevel(CERTIFICATE_LEVEL_QUALIFIED)
                    .WithNonce("012345678901234567890123456789")
                    .FetchAsync();

            Assert.Equal("PNOLV-329999-99901-AAAA-Q", certificateResponse.DocumentNumber);
            Assert.Equal("QUALIFIED", certificateResponse.CertificateLevel);
            Assert.Equal("MIIIpDCCBoygAwIBAgIQSADgqesOeFFhSzm98/SC0zANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJFRTEiMCAGA1UECgwZQVMgU2VydGlmaXRzZWVyaW1pc2tlc2t1czEXMBUGA1UEYQwOTlRSRUUtMTA3NDcwMTMxHDAaBgNVBAMME1RFU1Qgb2YgRUlELVNLIDIwMTYwHhcNMjEwOTIyMTQxMjEzWhcNMjQwOTIyMTQxMjEzWjBmMQswCQYDVQQGEwJMVjEXMBUGA1UEAwwOVEVTVE5VTUJFUixCT0QxEzARBgNVBAQMClRFU1ROVU1CRVIxDDAKBgNVBCoMA0JPRDEbMBkGA1UEBRMSUE5PTFYtMzI5OTk5LTk5OTAxMIIDIjANBgkqhkiG9w0BAQEFAAOCAw8AMIIDCgKCAwEApkGnh6imYQXES9PP2BGBwwX07KtViUOFffiQgW2WJ8k8UYFgVcjhSRWxz/JaYCtjnDYMa+BKrFShGIUFT78rtFy8HhHFYkQUmybLovv+YiJE3Opm5ppwbfgBq00mxsSTj173uTQYuAbiv0aMVUOjFuKRbUgRXccNhabX+l/3ZNnd0R2Jtyv686HUmtr4pe1ZR8rLM1MAurk35SKK9U6VH3cD3AeKhOQT0cQNFEkFhOhfJ2mANTHH4WkUlqVp4OmIv3NYrtzKZNSgdoj5wcM8/PXuzhvyQu2ejv2Pejlv7ZNftrqoWWBvz3WxJds1fWWBdRkipYHHPkUORRY72UoR0QOixnYizjD5wacQmG96FGWjb+EFJMHjkTde4lAfMfbZJA9cAXpsTl/KZIHNt/nDd/KtpJY/8STgGbyp6Su/vfMlX/oCZHX9hb+t3HD/XQAeDmngZSxKdJ5K8gffB8ZxYYcdk3n7HdULnV22Q56jwUZUSONewIqgwf892XwR3CMySaciMn0Wjf8T40CwzABf1Ih/TAt1v3Xr9uvM1c6fqdvBPPbLXhKzK+paGWxhgZjIaYJ3+AtRW3mYZNY/j4ZAlQMaX2MY5/AEaHoF/fA7+OZ0BX9JGuf1Reos/3pS3v7yiU2+50yF6PgzU5C/wHQJ+9Qh5rAafrAwMdhxUtWU9LS+INBzhbFD9U9waYNsG5lp/WhRGGa4hrtgqeGwHcJflO1+HQCmWzMS/peAJZCnCEHLUkRq4rjvzTETgK1cDXqHoiseW5twcbY9qqmmGvP1MzfBHUJfwYq4EdO8ITRVHLhrqGUmDyGiawZXLv2VQW7s/dRxAmesTFCZ2fNrsC3gdrr7ugVJEFYG9LsN9BvWkC3EE380+UnKc9ZLdnp0qGV+yr9xAUchb7EQTjPaVo/O144IfK8eAFNcTLJP7nbYkn8csRDuBqtKo1m+ZC9HcOKXJ2Zs2lfH+FjxEDaLhre3VyYZorQa5arNd9KdZ47QsJUrspz5P8L3vN70e4dR/lZXAgMBAAGjggJKMIICRjAJBgNVHRMEAjAAMA4GA1UdDwEB/wQEAwIGQDBdBgNVHSAEVjBUMEcGCisGAQQBzh8DEQIwOTA3BggrBgEFBQcCARYraHR0cHM6Ly9za2lkc29sdXRpb25zLmV1L2VuL3JlcG9zaXRvcnkvQ1BTLzAJBgcEAIvsQAECMB0GA1UdDgQWBBTo4aTlpOaClkVVIEL8qAP3iwEvczCBrgYIKwYBBQUHAQMEgaEwgZ4wCAYGBACORgEBMBUGCCsGAQUFBwsCMAkGBwQAi+xJAQEwEwYGBACORgEGMAkGBwQAjkYBBgEwXAYGBACORgEFMFIwUBZKaHR0cHM6Ly9za2lkc29sdXRpb25zLmV1L2VuL3JlcG9zaXRvcnkvY29uZGl0aW9ucy1mb3ItdXNlLW9mLWNlcnRpZmljYXRlcy8TAkVOMAgGBgQAjkYBBDAfBgNVHSMEGDAWgBSusOrhNvgmq6XMC2ZV/jodAr8StDB8BggrBgEFBQcBAQRwMG4wKQYIKwYBBQUHMAGGHWh0dHA6Ly9haWEuZGVtby5zay5lZS9laWQyMDE2MEEGCCsGAQUFBzAChjVodHRwOi8vc2suZWUvdXBsb2FkL2ZpbGVzL1RFU1Rfb2ZfRUlELVNLXzIwMTYuZGVyLmNydDAxBgNVHREEKjAopCYwJDEiMCAGA1UEAwwZUE5PTFYtMzI5OTk5LTk5OTAxLUFBQUEtUTAoBgNVHQkEITAfMB0GCCsGAQUFBwkBMREYDzE5MDMwMzAzMTIwMDAwWjANBgkqhkiG9w0BAQsFAAOCAgEAmOJs32k4syJorWQ0p9EF/yTr3RXO2/U8eEBf6pAw8LPOERy7MX1WtLaTHSctvrzpu37Tcz3B0XhTg7bCcVpn2iZVkDK+2SVLHG8CXLBNXzE5a9C2oUwUtZ9zwIK8gnRtj9vuSoI9oMvNfI0De/e1Y7oZesmUsef3Yavqp2x+qu9Gbup7U5owxpT413Ed65RQvfEGb5FStk7lF6tsT/L8fdhVDXCyat/yY6OQly8OvlxZnrOUGDgdjIxz4u+ZH1InhX9x17TEugXzgZO/3huZkxPkuXwp7CWOtP0/fliSrInS5zbcAfCSB5HZUtR4t4wApWTJ4+AQK/P10skynzJA0k0NbRTFfz8GEZ6ZhgEjwPjThXhoAuSHBPNqToYfy3ar5e7ucPh4SHd0KcUt3rty8/nFgVQd+/Ho6IciVYNAP6TAXuR9tU5XnX8dQWIzjg+wPwSpRr7WvW88qqncpVT4cdjmL+XJRjoK/czsQwfp9FRc23tOWG33dxiIj4lwmlWjPGeBVgp5tgrzAF1P4q+S6IHs70LOOztTF64fHN2YH/gjvb/T7G4oj98b7VTuGmiN7XQhULIdnqG6Kt8GKkkdjp1NziCa04vDOljr2PlChVulNujdNgVDxVfXU5RXP/HgoX2QJtQJyHZwLKvQQfw7T40C6mcN99lsLTx7/xss4Xc=", Convert.ToBase64String(certificateResponse.Certificate.RawData));

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

        [Fact]
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