/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.common.devicelink.interactions.DeviceLinkInteractionTest</c>.
 * #L%
 */

using SK.SmartId.Common;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId.Common;

public sealed class DeviceLinkInteractionTest
{
    [Fact]
    public void DisplayTextAndPin_ok()
    {
        var interaction = DeviceLinkInteraction.DisplayTextAndPin("Log in?");

        Assert.Same(DeviceLinkInteractionType.DisplayTextAndPin, interaction.InteractionType);
        Assert.Equal("Log in?", interaction.DisplayText60);
        Assert.Null(interaction.DisplayText200);
    }

    /// <remarks>
    /// Java throws SmartIdClientException for this branch; .NET raises SmartIdRequestSetupException from the same InteractionValidator path.
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DisplayTextAndPin_textIsEmpty_throwException(string displayText)
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            DeviceLinkInteraction.DisplayTextAndPin(displayText));
        Assert.Equal(
            "Value for 'displayText60' must be set when type is 'displayTextAndPIN'",
            ex.Message);
    }

    [Fact]
    public void DisplayTextAndPin_textWithExceedingLength_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            DeviceLinkInteraction.DisplayTextAndPin(new string('a', 61)));
        Assert.Equal(
            "Value for 'displayText60' must not exceed 60 characters",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessage_ok()
    {
        var interaction = DeviceLinkInteraction.ConfirmationMessage("Log in?");

        Assert.Same(DeviceLinkInteractionType.ConfirmationMessage, interaction.InteractionType);
        Assert.Null(interaction.DisplayText60);
        Assert.Equal("Log in?", interaction.DisplayText200);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ConfirmationMessage_emptyTextUsed_throwException(string displayText)
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            DeviceLinkInteraction.ConfirmationMessage(displayText));
        Assert.Equal(
            "Value for 'displayText200' must be set when type is 'confirmationMessage'",
            ex.Message);
    }

    [Fact]
    public void ConfirmationMessage_textWithExceedingLength_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            DeviceLinkInteraction.ConfirmationMessage(new string('a', 201)));
        Assert.Equal(
            "Value for 'displayText200' must not exceed 200 characters",
            ex.Message);
    }

    [Fact]
    public void InstantiateDeviceLinkWithNullValues_throwException()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            new DeviceLinkInteraction(null!, null!, null));
        Assert.Equal("Value for 'type' must be set", ex.Message);
    }
}
