/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System.Collections.Generic;
using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId.Common
{
    public class InteractionValidatorTest
    {
        public static IEnumerable<object[]> ValidDisplayTextForInteraction()
        {
            yield return new object[] { "a" };
            yield return new object[] { new string('a', 60) };
        }

        [Theory]
        [MemberData(nameof(ValidDisplayTextForInteraction))]
        public void Validate_deviceLinkInteraction_ok(string displayText)
        {
            InteractionValidator.Validate(DeviceLinkInteractionType.DisplayTextAndPin, displayText);
        }

        [Theory]
        [MemberData(nameof(ValidDisplayTextForInteraction))]
        public void Validate_notificationInteraction_ok(string displayText)
        {
            InteractionValidator.Validate(NotificationInteractionType.DisplayTextAndPin, displayText);
        }

        public static IEnumerable<object[]> InvalidConfirmationMessageDisplayText()
        {
            yield return new object[]
            {
                null,
                "Value for 'displayText200' must be set when type is 'confirmationMessageAndVerificationCodeChoice'"
            };
            yield return new object[]
            {
                "",
                "Value for 'displayText200' must be set when type is 'confirmationMessageAndVerificationCodeChoice'"
            };
            yield return new object[]
            {
                new string('a', 201),
                "Value for 'displayText200' must not exceed 200 characters"
            };
        }

        [Theory]
        [MemberData(nameof(InvalidConfirmationMessageDisplayText))]
        public void Validate_interactionWithInvalidDisplayTextLength_throwException(string displayText, string expectedMessage)
        {
            var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
                InteractionValidator.Validate(NotificationInteractionType.ConfirmationMessageAndVerificationCodeChoice, displayText));
            Assert.Equal(expectedMessage, ex.Message);
        }
    }
}
