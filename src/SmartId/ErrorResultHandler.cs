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
using SK.SmartId.Util;

namespace SK.SmartId
{
    /// <summary>
    /// Maps session <see cref="SessionResult.EndResult"/> to typed exceptions (Java <c>ErrorResultHandler</c>).
    /// </summary>
    public static class ErrorResultHandler
    {
        /// <summary>
        /// Throws a typed exception for a non-success <paramref name="sessionResult"/>; does not return.
        /// </summary>
        /// <exception cref="SmartIdClientException"><paramref name="sessionResult"/> is null</exception>
        public static void Handle(SessionResult sessionResult)
        {
            if (sessionResult == null)
            {
                throw new SmartIdClientException("Parameter 'sessionResult' is not provided");
            }

            switch (sessionResult.EndResult)
            {
                case "USER_REFUSED":
                    throw new UserRefusedException();
                case "TIMEOUT":
                    throw new SessionTimeoutException();
                case "DOCUMENT_UNUSABLE":
                    throw new DocumentUnusableException();
                case "WRONG_VC":
                    throw new UserSelectedWrongVerificationCodeException();
                case "REQUIRED_INTERACTION_NOT_SUPPORTED_BY_APP":
                    throw new RequiredInteractionNotSupportedByAppException();
                case "USER_REFUSED_CERT_CHOICE":
                    throw new UserRefusedCertChoiceException();
                case "USER_REFUSED_INTERACTION":
                    if (sessionResult.Details == null || StringUtil.IsEmpty(sessionResult.Details.Interaction))
                    {
                        throw new UnprocessableSmartIdResponseException("Details for refused interaction are missing");
                    }
                    switch (sessionResult.Details.Interaction)
                    {
                        case "displayTextAndPIN":
                            throw new UserRefusedDisplayTextAndPinException();
                        case "confirmationMessage":
                            throw new UserRefusedConfirmationMessageException();
                        case "confirmationMessageAndVerificationCodeChoice":
                            throw new UserRefusedConfirmationMessageWithVerificationChoiceException();
                        default:
                            throw new UnprocessableSmartIdResponseException("Unexpected interaction type: " + sessionResult.Details.Interaction);
                    }
                case "PROTOCOL_FAILURE":
                    throw new ProtocolFailureException();
                case "EXPECTED_LINKED_SESSION":
                    throw new ExpectedLinkedSessionException();
                case "SERVER_ERROR":
                    throw new SmartIdServerException();
                case "ACCOUNT_UNUSABLE":
                    throw new UserAccountUnusableException();
                default:
                    throw new UnprocessableSmartIdResponseException("Unexpected session result: " + sessionResult.EndResult);
            }
        }
    }
}
