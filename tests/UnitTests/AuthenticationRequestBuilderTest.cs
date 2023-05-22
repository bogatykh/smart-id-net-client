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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId
{
    public class AuthenticationRequestBuilderTest
    {
        private readonly SmartIdConnectorSpy connector;
        private readonly AuthenticationRequestBuilder builder;

        public AuthenticationRequestBuilderTest()
        {
            connector = new SmartIdConnectorSpy
            {
                authenticationSessionResponseToRespond = CreateDummyAuthenticationSessionResponse(),
                sessionStatusToRespond = CreateDummySessionStatusResponse()
            };
            builder = new AuthenticationRequestBuilder(connector, new SessionStatusPoller(connector));
        }

        [Fact]
        public async Task AuthenticateWithDocumentNumberAndGeneratedHash()
        {
            AuthenticationHash authenticationHash = AuthenticationHash.GenerateRandomHash();

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithAuthenticationHash(authenticationHash)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithDocumentNumber(authenticationHash.HashInBase64, "QUALIFIED");
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, authenticationHash.HashInBase64);
        }

        [Fact]
        public async Task AuthenticateWithHash()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithAuthenticationHash(authenticationHash)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithCapabilities("ADVANCED")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithDocumentNumber("7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==", "QUALIFIED");
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==");
        }

        [Fact]
        public async Task Authenticate_usingSemanticsIdentifier()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithAuthenticationHash(authenticationHash)
                .WithSemanticsIdentifier(new SemanticsIdentifier("IDCCZ-1234567890"))
                .WithCapabilities(Capability.ADVANCED)
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithSemanticsIdentifier(authenticationHash.HashInBase64, "QUALIFIED");
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==");
        }

        [Fact]
        public async Task Authenticate_usingSemanticsIdentifierAsString()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            SmartIdAuthenticationResponse authenticationResponse = await builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithAuthenticationHash(authenticationHash)
                    .WithSemanticsIdentifierAsString("IDCCZ-1234567890")
                    .WithCapabilities(Capability.ADVANCED)
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithSemanticsIdentifier(authenticationHash.HashInBase64, "QUALIFIED");
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==");
        }

        [Fact]
        public async Task authenticateWithoutCertificateLevel_shouldPass()
        {
            AuthenticationHash authenticationHash = AuthenticationHash.GenerateRandomHash();

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithAuthenticationHash(authenticationHash)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithDocumentNumber(authenticationHash.HashInBase64, null);
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, authenticationHash.HashInBase64);
        }

        [Fact]
        public async Task authenticate_withShareMdClientIpAddressTrue()
        {
            AuthenticationHash authenticationHash = AuthenticationHash.GenerateRandomHash();

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithAuthenticationHash(authenticationHash)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .WithShareMdClientIpAddress(true)
                .AuthenticateAsync();

            Assert.False(connector.authenticationSessionRequestUsed.RequestProperties is null,
                "getRequestProperties must be set withShareMdClientIpAddress");

            Assert.True(connector.authenticationSessionRequestUsed.RequestProperties.ShareMdClientIpAddress,
                "requestProperties.shareMdClientIpAddress must be true");

            AssertCorrectAuthenticationRequestMadeWithDocumentNumber(authenticationHash.HashInBase64, "QUALIFIED");
            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, authenticationHash.HashInBase64);
        }

        [Fact]
        public async Task authenticate_withShareMdClientIpAddressFalse()
        {
            AuthenticationHash authenticationHash = AuthenticationHash.GenerateRandomHash();

            SmartIdAuthenticationResponse authenticationResponse = await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithCertificateLevel("QUALIFIED")
                .WithAuthenticationHash(authenticationHash)
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                .WithShareMdClientIpAddress(false)
                .AuthenticateAsync();

            AssertCorrectAuthenticationRequestMadeWithDocumentNumber(authenticationHash.HashInBase64, "QUALIFIED");

            Assert.False(connector.authenticationSessionRequestUsed.RequestProperties is null,
                "getRequestProperties must be set withShareMdClientIpAddress");

            Assert.False(connector.authenticationSessionRequestUsed.RequestProperties.ShareMdClientIpAddress,
                "requestProperties.shareMdClientIpAddress must be false");

            AssertCorrectSessionRequestMade();
            AssertAuthenticationResponseCorrect(authenticationResponse, authenticationHash.HashInBase64);
        }

        [Fact]
        public async Task authenticate_withoutDocumentNumber_withoutSemanticsIdentifier_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Either documentNumber or semanticsIdentifier must be set", exception.Message);
        }

        [Fact]
        public async Task authenticate_withDocumentNumberAndWithSemanticsIdentifier_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithSemanticsIdentifierAsString("IDCCZ-1234567890")
                    .WithCertificateLevel("QUALIFIED")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Exactly one of documentNumber or semanticsIdentifier must be set", exception.Message);
        }

        [Fact]
        public async Task authenticate_withoutHashAndWithoutDataToSign_shouldThrowException()
        {
            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task authenticateWithHash_withoutHashType_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w=="
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithAuthenticationHash(authenticationHash)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task authenticateWithHash_withoutHash_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithCertificateLevel("QUALIFIED")
                    .WithAuthenticationHash(authenticationHash)
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Either dataToSign or hash with hashType must be set", exception.Message);
        }

        [Fact]
        public async Task authenticateWithoutRelyingPartyUuid_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Parameter relyingPartyUUID must be set", exception.Message);
        }

        [Fact]
        public async Task authenticateWithoutRelyingPartyName_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .AuthenticateAsync()
            );
            Assert.Equal("Parameter relyingPartyName must be set", exception.Message);
        }

        [Fact]
        public async Task authenticate_withTooLongNonce_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to internet bank?") })
                    .WithNonce("THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890")
                    .AuthenticateAsync()
            );
            Assert.Equal("Nonce cannot be longer that 30 chars. You supplied: 'THIS_IS_LONGER_THAN_ALLOWED_30_CHARS_0123456789012345678901234567890'", exception.Message);
        }

        [Fact]
        public async Task authenticate_missingAllowedInteractionOrder_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash();
            authenticationHash.HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==";
            authenticationHash.HashType = HashType.SHA512;

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .AuthenticateAsync()
            );
            Assert.Equal("Missing or empty mandatory parameter allowedInteractionsOrder", exception.Message);
        }

        [Fact]
        public async Task authenticate_displayTextAndPinTextTooLong_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.DisplayTextAndPIN("This text here is longer than 60 characters allowed for displayTextAndPIN") }
                    )
                    .AuthenticateAsync()
            );
            Assert.Equal("displayText60 must not be longer than 60 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_verificationCodeChoiceTextTooLong_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.VerificationCodeChoice("This text here is longer than 60 characters allowed for verificationCodeChoice") }
                    )
                    .AuthenticateAsync()
            );
            Assert.Equal("displayText60 must not be longer than 60 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_confirmationMessageTextTooLong_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessage("This text here is longer than 200 characters allowed for confirmationMessage. Lorem ipsum dolor sit amet, " +
                                    "consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                                    "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                                    "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                                    "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.") }
                    )
                    .AuthenticateAsync()
            );
            Assert.Equal("displayText200 must not be longer than 200 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_confirmationMessageAndVerificationCodeChoiceTextTooLong_shouldThrowException()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            var exception = await Assert.ThrowsAsync<SmartIdClientException>(() =>
                builder
                    .WithRelyingPartyUUID("relying-party-uuid")
                    .WithRelyingPartyName("relying-party-name")
                    .WithAuthenticationHash(authenticationHash)
                    .WithCertificateLevel("QUALIFIED")
                    .WithDocumentNumber("PNOEE-31111111111")
                    .WithAllowedInteractionsOrder(new List<Interaction> {
                            Interaction.ConfirmationMessageAndVerificationCodeChoice("This text here is longer than 200 characters allowed for confirmationMessage. Lorem ipsum dolor sit amet, " +
                                    "consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                                    "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                                    "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                                    "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.") }
                    )
                    .AuthenticateAsync()
            );
            Assert.Equal("displayText200 must not be longer than 200 characters", exception.Message);
        }

        [Fact]
        public async Task authenticate_userRefused_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED");
            await Assert.ThrowsAsync<UserRefusedException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task authenticate_userRefusedCertChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CERT_CHOICE");
            await Assert.ThrowsAsync<UserRefusedCertChoiceException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task authenticate_userRefusedDisplayTextAndPin_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_DISPLAYTEXTANDPIN");
            await Assert.ThrowsAsync<UserRefusedDisplayTextAndPinException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_userRefusedVerificationChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_VC_CHOICE");
            await Assert.ThrowsAsync<UserRefusedVerificationChoiceException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_userRefusedConfirmationMessage_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CONFIRMATIONMESSAGE");
            await Assert.ThrowsAsync<UserRefusedConfirmationMessageException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_userRefusedConfirmationMessageWithVerificationChoice_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserRefusedSessionStatus("USER_REFUSED_CONFIRMATIONMESSAGE_WITH_VC_CHOICE");
            await Assert.ThrowsAsync<UserRefusedConfirmationMessageWithVerificationChoiceException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_userSelectedWrongVerificationCode_shouldThrowException()
        {
            connector.sessionStatusToRespond = DummyData.createUserSelectedWrongVerificationCode();
            await Assert.ThrowsAsync<UserSelectedWrongVerificationCodeException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_resultMissingInResponse_shouldThrowException()
        {
            connector.sessionStatusToRespond.Result = null;
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_signatureMissingInResponse_shouldThrowException()
        {
            connector.sessionStatusToRespond.Signature = null;
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeAuthenticationRequestAsync);
        }

        [Fact]
        public async Task Authenticate_certificateMissingInResponse_shouldThrowException()
        {
            connector.sessionStatusToRespond.Cert = null;
            await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(MakeAuthenticationRequestAsync);
        }

        private void AssertCorrectAuthenticationRequestMadeWithDocumentNumber(string expectedHashToSignInBase64, string expectedCertificateLevel)
        {
            Assert.Equal("PNOEE-31111111111", connector.documentNumberUsed);
            Assert.Equal("relying-party-uuid", connector.authenticationSessionRequestUsed.RelyingPartyUUID);
            Assert.Equal("relying-party-name", connector.authenticationSessionRequestUsed.RelyingPartyName);
            Assert.Equal(expectedCertificateLevel, connector.authenticationSessionRequestUsed.CertificateLevel);
            Assert.Equal("SHA512", connector.authenticationSessionRequestUsed.HashType);
            Assert.Equal(expectedHashToSignInBase64, connector.authenticationSessionRequestUsed.Hash);
        }

        private void AssertCorrectAuthenticationRequestMadeWithSemanticsIdentifier(string expectedHashToSignInBase64, string expectedCertificateLevel)
        {
            Assert.Equal("IDCCZ-1234567890", connector.semanticsIdentifierUsed.Identifier);
            Assert.Equal("relying-party-uuid", connector.authenticationSessionRequestUsed.RelyingPartyUUID);
            Assert.Equal("relying-party-name", connector.authenticationSessionRequestUsed.RelyingPartyName);
            Assert.Equal(expectedCertificateLevel, connector.authenticationSessionRequestUsed.CertificateLevel);
            Assert.Equal("SHA512", connector.authenticationSessionRequestUsed.HashType);
            Assert.Equal(expectedHashToSignInBase64, connector.authenticationSessionRequestUsed.Hash);
        }

        private void AssertCorrectSessionRequestMade()
        {
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", connector.sessionIdUsed);
        }

        private void AssertAuthenticationResponseCorrect(SmartIdAuthenticationResponse authenticationResponse, string expectedHashToSignInBase64)
        {
            Assert.NotNull(authenticationResponse);
            Assert.Equal("OK", authenticationResponse.EndResult);
            Assert.Equal(expectedHashToSignInBase64, authenticationResponse.SignedHashInBase64);
            Assert.Equal("c2FtcGxlIHNpZ25hdHVyZQ0K", authenticationResponse.SignatureValueInBase64);
            Assert.Equal("sha512WithRSAEncryption", authenticationResponse.AlgorithmName);
            Assert.Equal(DummyData.CERTIFICATE, Convert.ToBase64String(authenticationResponse.Certificate.RawData));
            Assert.Equal("QUALIFIED", authenticationResponse.CertificateLevel);

            Assert.Equal("displayTextAndPIN", authenticationResponse.InteractionFlowUsed);
        }

        private AuthenticationSessionResponse CreateDummyAuthenticationSessionResponse()
        {
            AuthenticationSessionResponse response = new AuthenticationSessionResponse()
            {
                SessionId = "97f5058e-e308-4c83-ac14-7712b0eb9d86"
            };
            return response;
        }

        private SessionStatus CreateDummySessionStatusResponse()
        {
            SessionSignature signature = new SessionSignature()
            {
                Value = "c2FtcGxlIHNpZ25hdHVyZQ0K",
                Algorithm = "sha512WithRSAEncryption"
            };

            SessionCertificate certificate = new SessionCertificate()
            {
                CertificateLevel = "QUALIFIED",
                Value = DummyData.CERTIFICATE
            };

            SessionStatus status = new SessionStatus()
            {
                State = "COMPLETE",
                Result = DummyData.createSessionEndResult(),
                Signature = signature,
                Cert = certificate,
                InteractionFlowUsed = "displayTextAndPIN",
                DeviceIpAddress = "4.4.4.4"
            };
            return status;
        }

        private async Task MakeAuthenticationRequestAsync()
        {
            AuthenticationHash authenticationHash = new AuthenticationHash()
            {
                HashInBase64 = "7iaw3Ur350mqGo7jwQrpkj9hiYB3Lkc/iBml1JQODbJ6wYX4oOHV+E+IvIh/1nsUNzLDBMxfqa2Ob1f1ACio/w==",
                HashType = HashType.SHA512
            };

            await builder
                .WithRelyingPartyUUID("relying-party-uuid")
                .WithRelyingPartyName("relying-party-name")
                .WithAuthenticationHash(authenticationHash)
                .WithCertificateLevel("QUALIFIED")
                .WithDocumentNumber("PNOEE-31111111111")
                .WithAllowedInteractionsOrder(new List<Interaction> { Interaction.DisplayTextAndPIN("Log in to self-service?") })
                .AuthenticateAsync();
        }

    }
}