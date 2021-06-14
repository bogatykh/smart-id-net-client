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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId
{
    public class SignatureRequestBuilderTest
    {
        private SmartIdConnectorSpy connector;
        private SignatureRequestBuilder builder;

        public SignatureRequestBuilderTest()
        {
            connector = new SmartIdConnectorSpy();
            connector.signatureSessionResponseToRespond = createDummySignatureSessionResponse();
            connector.sessionStatusToRespond = createDummySessionStatusResponse();
            builder = new SignatureRequestBuilder(connector, new SessionStatusPoller(connector));
        }

        [Fact]
        public async Task sign_withHashToSign()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashType = HashType.SHA256;
            hashToSign.HashInBase64 = "jsflWgpkVcWOyICotnVn5lazcXdaIWvcvNOWTYPceYQ=";

            SmartIdSignature signature = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithSignableHash(hashToSign)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithCapabilities(Capability.ADVANCED)
                .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.ConfirmationMessageAndVerificationCodeChoice("Sign hash?"),
                        Interaction.VerificationCodeChoice("Sign hash?") })
                .SignAsync();

            assertCorrectSignatureRequestMade("QUALIFIED");
            assertCorrectSessionRequestMade();
            assertSignatureCorrect(signature);
        }

        [Fact]
        public async Task sign_withDataToSign()
        {
            SignableData dataToSign = new SignableData(Encoding.UTF8.GetBytes("Say 'hello' to my little friend!"));
            dataToSign.HashType = HashType.SHA256;

            SmartIdSignature signature = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithSignableData(dataToSign)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithCapabilities("QUALIFIED")
                .WithAllowedInteractionsOrder(new List<Interaction>() { Interaction.VerificationCodeChoice("Do you want to say hello?") })
                .SignAsync();

            assertCorrectSignatureRequestMade("QUALIFIED");
            assertCorrectSessionRequestMade();
            assertSignatureCorrect(signature);
        }

        [Fact]
        public async Task signWithoutCertificateLevel_shouldPass()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "jsflWgpkVcWOyICotnVn5lazcXdaIWvcvNOWTYPceYQ=";
            hashToSign.HashType = HashType.SHA256;

            SmartIdSignature signature = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithSignableHash(hashToSign)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction>{Interaction.ConfirmationMessageAndVerificationCodeChoice("Sign the contract?"),
                        Interaction.VerificationCodeChoice("Sign hash?") })
                .SignAsync();

            assertCorrectSignatureRequestMade(null);
            assertCorrectSessionRequestMade();
            assertSignatureCorrect(signature);
        }

        [Fact]
        public async Task signWithoutDocumentNumber_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                        .WithCertificateLevel("QUALIFIED")
                        .WithSignableHash(hashToSign)
                        .SignAsync()
            );
            Assert.Equal("Either documentNumber or semanticsIdentifier must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withDocumentNumberAndWithSemanticsIdentifier_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithSemanticsIdentifierAsString("IDCCZ-1234567890")
                    .WithCertificateLevel("QUALIFIED")
                    .WithAllowedInteractionsOrder(new List<Interaction>() { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .SignAsync()
            );
            Assert.Equal("Exactly one of documentNumber or semanticsIdentifier must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withoutDataToSign_withoutHash_shouldThrowException()
        {
            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
            builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithDocumentNumber("PNOEE-31111111111")
                .SignAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task signWithSignableHash_withoutHashType_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .SignAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withHash_withoutHashType_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .SignAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withoutRelyingPartyUuid_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .SignAsync()
            );
            Assert.Equal("Parameter relyingPartyUUID must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withoutRelyingPartyName_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithCertificateLevel("QUALIFIED")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .SignAsync()
            );
            Assert.Equal("Parameter relyingPartyName must be set", exception.Message);
        }

        [Fact]
        public async Task sign_withTooLongNonce_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "0nbgC2fVdLVQFZJdBbmG7oPoElpCYsQMtrY0c0wKYRg=";
            hashToSign.HashType = HashType.SHA256;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithSignableHash(hashToSign)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithNonce("THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890")
                    .SignAsync()
            );
            Assert.Equal("Nonce cannot be longer that 30 chars. You supplied: 'THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890'", exception.Message);
        }
        
        [Fact]
        public async Task authenticate_displayTextAndPinTextTooLong_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            hashToSign.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                        Interaction.DisplayTextAndPIN("This text here is longer than 60 characters allowed for displayTextAndPIN") }
                    )
                    .SignAsync()
            );
            Assert.Equal("displayText60 must not be longer than 60 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_verificationCodeChoiceTextTooLong_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            hashToSign.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.VerificationCodeChoice("This text here is longer than 60 characters allowed for verificationCodeChoice") }
                    )
                    .SignAsync()
            );
            Assert.Equal("displayText60 must not be longer than 60 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_confirmationMessageTextTooLong_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            hashToSign.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessage("This text here is longer than 200 characters allowed for confirmationMessage. Lorem ipsum dolor sit amet, " +
                                    "consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                                    "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                                    "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                                    "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.") }
                    )
                    .SignAsync()
            );
            Assert.Equal("displayText200 must not be longer than 200 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_confirmationMessageAndVerificationCodeChoiceTextTooLong_shouldThrowException()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            hashToSign.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithSignableHash(hashToSign)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessageAndVerificationCodeChoice("This text here is longer than 200 characters allowed for confirmationMessage. Lorem ipsum dolor sit amet, " +
                                    "consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                                    "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                                    "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                                    "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.") }
                    )
                    .SignAsync()
            );
            Assert.Equal("displayText200 must not be longer than 200 characters", exception.Message);
        }

        [Fact]
        public async Task sign_userRefused_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED");
            await Assert.ThrowsAsync<UserRefusedException>(MakeSigningRequest);
        }


        [Fact]
        public async Task sign_userRefusedCertChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CERT_CHOICE");
            await Assert.ThrowsAsync<UserRefusedCertChoiceException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_userRefusedDisplayTextAndPin_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_DISPLAYTEXTANDPIN");
            await Assert.ThrowsAsync<UserRefusedDisplayTextAndPinException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_userRefusedVerificationChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_VC_CHOICE");
            await Assert.ThrowsAsync<UserRefusedVerificationChoiceException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_userRefusedConfirmationMessage_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CONFIRMATIONMESSAGE");
            await Assert.ThrowsAsync<UserRefusedConfirmationMessageException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_userRefusedConfirmationMessageWithVerificationChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CONFIRMATIONMESSAGE_WITH_VC_CHOICE");
            await Assert.ThrowsAsync<UserRefusedConfirmationMessageWithVerificationChoiceException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_userSelectedWrongVerificationCode_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserSelectedWrongVerificationCode();
            await Assert.ThrowsAsync<UserSelectedWrongVerificationCodeException>(MakeSigningRequest);
        }

        [Fact]
        public async Task sign_signatureMissingInResponse_shouldThrowException()
        {
            connector.sessionStatusToRespond.Signature = null;
            var exception = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeSigningRequest);
            Assert.Equal("Signature was not present in the response", exception.Message);
        }

        private void assertCorrectSignatureRequestMade(string expectedCertificateLevel)
        {
            Assert.Equal("PNOEE-31111111111", connector.documentNumberUsed);
            Assert.Equal("relying-party-uuid", connector.signatureSessionRequestUsed.RelyingPartyUUID);
            Assert.Equal("relying-party-name", connector.signatureSessionRequestUsed.RelyingPartyName);
            Assert.Equal(expectedCertificateLevel, connector.signatureSessionRequestUsed.CertificateLevel);
            Assert.Equal("SHA256", connector.signatureSessionRequestUsed.HashType);
            Assert.Equal("jsflWgpkVcWOyICotnVn5lazcXdaIWvcvNOWTYPceYQ=", connector.signatureSessionRequestUsed.Hash);
        }

        private void assertCorrectSessionRequestMade()
        {
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", connector.sessionIdUsed);
        }

        private void assertSignatureCorrect(SmartIdSignature signature)
        {
            Assert.NotNull(signature);
            Assert.Equal("luvjsi1+1iLN9yfDFEh/BE8h", signature.ValueInBase64);
            Assert.Equal("sha256WithRSAEncryption", signature.AlgorithmName);
            Assert.Equal("PNOEE-31111111111", signature.DocumentNumber);
            Assert.Equal("verificationCodeChoice", signature.InteractionFlowUsed);
        }

        private SignatureSessionResponse createDummySignatureSessionResponse()
        {
            SignatureSessionResponse response = new SignatureSessionResponse();
            response.SessionId = "97f5058e-e308-4c83-ac14-7712b0eb9d86";
            return response;
        }

        private SessionStatus createDummySessionStatusResponse()
        {
            SessionStatus status = new SessionStatus();
            status.State = "COMPLETE";
            status.Result = DummyData.createSessionEndResult();
            SessionSignature signature = new SessionSignature();
            signature.Value = "luvjsi1+1iLN9yfDFEh/BE8h";
            signature.Algorithm = "sha256WithRSAEncryption";
            status.Signature = signature;
            status.InteractionFlowUsed = "verificationCodeChoice";
            return status;
        }

        private async Task MakeSigningRequest()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashInBase64 = "jsflWgpkVcWOyICotnVn5lazcXdaIWvcvNOWTYPceYQ=";
            hashToSign.HashType = HashType.SHA256;

            await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithSignableHash(hashToSign)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Transfer amount X to Y?") })
                .SignAsync();
        }
    }
}