/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.common.notification.interactions.NotificationInteractionTest</c>.
 * #L%
 */

using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId.Common;

public sealed class NotificationInteractionTest
{
    [Fact]
    public void DisplayTextAndPin_ok()
    {
        var interaction = NotificationInteraction.DisplayTextAndPin("Log in?");

        Assert.Same(NotificationInteractionType.DisplayTextAndPin, interaction.InteractionType);
        Assert.Equal("Log in?", interaction.DisplayText60);
        Assert.Null(interaction.DisplayText200);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DisplayTextAndPin_textIsEmpty_throwException(string displayText)
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.DisplayTextAndPin(displayText));
        Assert.Equal(
            "Value for 'displayText60' must be set when type is 'displayTextAndPIN'",
            ex.Message);
    }

    [Fact]
    public void DisplayTextAndPin_textWithExceedingLength_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.DisplayTextAndPin(new string('a', 61)));
        Assert.Equal(
            "Value for 'displayText60' must not exceed 60 characters",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessage_ok()
    {
        var interaction = NotificationInteraction.ConfirmationMessage("Log in?");

        Assert.Same(NotificationInteractionType.ConfirmationMessage, interaction.InteractionType);
        Assert.Null(interaction.DisplayText60);
        Assert.Equal("Log in?", interaction.DisplayText200);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ConfirmationMessage_emptyTextUsed_throwException(string displayText)
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.ConfirmationMessage(displayText));
        Assert.Equal(
            "Value for 'displayText200' must be set when type is 'confirmationMessage'",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessage_textWithExceedingLength_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.ConfirmationMessage(new string('a', 201)));
        Assert.Equal(
            "Value for 'displayText200' must not exceed 200 characters",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessageAndVerificationCodeChoice_ok()
    {
        var interaction =
            NotificationInteraction.ConfirmationMessageAndVerificationCodeChoice("Log in?");

        Assert.Same(
            NotificationInteractionType.ConfirmationMessageAndVerificationCodeChoice,
            interaction.InteractionType);
        Assert.Null(interaction.DisplayText60);
        Assert.Equal("Log in?", interaction.DisplayText200);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ConfirmationMessageAndVerificationCodeChoice_emptyTextUsed_throwException(string displayText)
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.ConfirmationMessageAndVerificationCodeChoice(displayText));
        Assert.Equal(
            "Value for 'displayText200' must be set when type is 'confirmationMessageAndVerificationCodeChoice'",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessageAndVerificationCodeChoice_textWithExceedingLength_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            NotificationInteraction.ConfirmationMessageAndVerificationCodeChoice(new string('a', 201)));
        Assert.Equal(
            "Value for 'displayText200' must not exceed 200 characters",
            ex.Message);
    }

    [Fact]
    public void InstantiateNotificationInteractionWithNullValues_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            new NotificationInteraction(null!, null!, null));
        Assert.Equal("Value for 'type' must be set", ex.Message);
    }
}
