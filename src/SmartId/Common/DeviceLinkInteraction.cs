/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
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

using SK.SmartId.Exceptions.Permanent;

namespace SK.SmartId.Common
{
    public sealed class DeviceLinkInteraction : ISmartIdInteraction
    {
        public DeviceLinkInteraction(DeviceLinkInteractionType type, string displayText60, string displayText200)
        {
            InteractionType = type ?? throw new SmartIdRequestSetupException("Value for 'type' must be set");
            if (type == DeviceLinkInteractionType.DisplayTextAndPin)
            {
                InteractionValidator.Validate(type, displayText60);
            }
            if (type == DeviceLinkInteractionType.ConfirmationMessage)
            {
                InteractionValidator.Validate(type, displayText200);
            }
            DisplayText60 = displayText60;
            DisplayText200 = displayText200;
        }

        public static DeviceLinkInteraction DisplayTextAndPin(string displayText60) =>
            new DeviceLinkInteraction(DeviceLinkInteractionType.DisplayTextAndPin, displayText60, null);

        public static DeviceLinkInteraction ConfirmationMessage(string displayText200) =>
            new DeviceLinkInteraction(DeviceLinkInteractionType.ConfirmationMessage, null, displayText200);

        public IInteractionType InteractionType { get; }

        public string DisplayText60 { get; }

        public string DisplayText200 { get; }
    }
}
