/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System.Collections.Generic;
using SK.SmartId.Common;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId.Common
{
    public class InteractionsMapperTest
    {
        [Fact]
        public void From_deviceLinkInteraction()
        {
            var deviceLinkInteraction = new DeviceLinkInteraction(DeviceLinkInteractionType.DisplayTextAndPin, "Log in?", null);
            Interaction interaction = InteractionsMapper.From(deviceLinkInteraction);

            Assert.Equal(DeviceLinkInteractionType.DisplayTextAndPin.Code, interaction.Type);
            Assert.Equal("Log in?", interaction.DisplayText60);
            Assert.Null(interaction.DisplayText200);
        }

        [Fact]
        public void From_deviceLinkInteractionsList()
        {
            var deviceLinkInteraction = new DeviceLinkInteraction(DeviceLinkInteractionType.DisplayTextAndPin, "Log in?", null);
            List<Interaction> interactions = InteractionsMapper.From(new List<ISmartIdInteraction> { deviceLinkInteraction });

            Assert.NotEmpty(interactions);
            Interaction interaction = interactions[0];
            Assert.Equal(DeviceLinkInteractionType.DisplayTextAndPin.Code, interaction.Type);
            Assert.Equal("Log in?", interaction.DisplayText60);
            Assert.Null(interaction.DisplayText200);
        }

        [Fact]
        public void From_notificationInteraction()
        {
            var notificationInteraction = new NotificationInteraction(NotificationInteractionType.DisplayTextAndPin, "Log in?", null);
            Interaction interaction = InteractionsMapper.From(notificationInteraction);

            Assert.Equal(DeviceLinkInteractionType.DisplayTextAndPin.Code, interaction.Type);
            Assert.Equal("Log in?", interaction.DisplayText60);
            Assert.Null(interaction.DisplayText200);
        }

        [Fact]
        public void From_notificationInteractionsList()
        {
            var notificationInteraction = new NotificationInteraction(NotificationInteractionType.DisplayTextAndPin, "Log in?", null);
            List<Interaction> interactions = InteractionsMapper.From(new List<ISmartIdInteraction> { notificationInteraction });

            Assert.NotEmpty(interactions);
            Interaction interaction = interactions[0];
            Assert.Equal(DeviceLinkInteractionType.DisplayTextAndPin.Code, interaction.Type);
            Assert.Equal("Log in?", interaction.DisplayText60);
            Assert.Null(interaction.DisplayText200);
        }

        [Fact]
        public void From_nullList_returnsEmpty()
        {
            Assert.Empty(InteractionsMapper.From((IReadOnlyList<ISmartIdInteraction>)null));
        }
    }
}
