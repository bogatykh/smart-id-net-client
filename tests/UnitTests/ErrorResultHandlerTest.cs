/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId
{
    public class ErrorResultHandlerTest
    {
        [Fact]
        public void Handle_nullInput()
        {
            var ex = Assert.Throws<SmartIdClientException>(() => ErrorResultHandler.Handle(null));
            Assert.Equal("Parameter 'sessionResult' is not provided", ex.Message);
        }

        [Theory]
        [InlineData("USER_REFUSED", typeof(UserRefusedException))]
        [InlineData("TIMEOUT", typeof(SessionTimeoutException))]
        [InlineData("DOCUMENT_UNUSABLE", typeof(DocumentUnusableException))]
        [InlineData("WRONG_VC", typeof(UserSelectedWrongVerificationCodeException))]
        [InlineData("REQUIRED_INTERACTION_NOT_SUPPORTED_BY_APP", typeof(RequiredInteractionNotSupportedByAppException))]
        [InlineData("USER_REFUSED_CERT_CHOICE", typeof(UserRefusedCertChoiceException))]
        [InlineData("PROTOCOL_FAILURE", typeof(ProtocolFailureException))]
        [InlineData("EXPECTED_LINKED_SESSION", typeof(ExpectedLinkedSessionException))]
        [InlineData("SERVER_ERROR", typeof(SmartIdServerException))]
        [InlineData("UNKNOWN_RESULT", typeof(UnprocessableSmartIdResponseException))]
        [InlineData("ACCOUNT_UNUSABLE", typeof(UserAccountUnusableException))]
        public void Handle_notOKEndResults(string endResult, System.Type expectedException)
        {
            var sessionResult = new SessionResult { EndResult = endResult };
            Assert.Throws(expectedException, () => ErrorResultHandler.Handle(sessionResult));
        }

        [Theory]
        [InlineData("")]
        [InlineData("UNKNOWN")]
        public void Handle_unknownEndResult(string unknownEndResult)
        {
            var sessionResult = new SessionResult { EndResult = unknownEndResult };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => ErrorResultHandler.Handle(sessionResult));
            Assert.Equal("Unexpected session result: " + unknownEndResult, ex.Message);
        }

        [Fact]
        public void Handle_endResultIsUserRefusedInteraction_detailsMissing()
        {
            var sessionResult = new SessionResult { EndResult = "USER_REFUSED_INTERACTION" };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => ErrorResultHandler.Handle(sessionResult));
            Assert.Equal("Details for refused interaction are missing", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Handle_endResultIsUserRefusedInteraction_interactionIsEmpty(string interaction)
        {
            var sessionResult = new SessionResult
            {
                EndResult = "USER_REFUSED_INTERACTION",
                Details = new SessionResultDetails { Interaction = interaction }
            };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => ErrorResultHandler.Handle(sessionResult));
            Assert.Equal("Details for refused interaction are missing", ex.Message);
        }

        [Fact]
        public void Handle_endResultIsUserRefusedInteraction_interactionIsInvalidValue()
        {
            var sessionResult = new SessionResult
            {
                EndResult = "USER_REFUSED_INTERACTION",
                Details = new SessionResultDetails { Interaction = "invalid interaction" }
            };
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() => ErrorResultHandler.Handle(sessionResult));
            Assert.Equal("Unexpected interaction type: invalid interaction", ex.Message);
        }

        [Theory]
        [InlineData("displayTextAndPIN", typeof(UserRefusedDisplayTextAndPinException))]
        [InlineData("confirmationMessage", typeof(UserRefusedConfirmationMessageException))]
        [InlineData("confirmationMessageAndVerificationCodeChoice", typeof(UserRefusedConfirmationMessageWithVerificationChoiceException))]
        public void Handle_endResultIsUserRefusedInteraction(string interaction, System.Type expectedException)
        {
            var sessionResult = new SessionResult
            {
                EndResult = "USER_REFUSED_INTERACTION",
                Details = new SessionResultDetails { Interaction = interaction }
            };
            Assert.Throws(expectedException, () => ErrorResultHandler.Handle(sessionResult));
        }
    }
}
