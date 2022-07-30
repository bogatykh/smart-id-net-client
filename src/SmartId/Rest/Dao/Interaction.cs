/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2022 SK ID Solutions AS
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
using System.Text.Json.Serialization;

namespace SK.SmartId.Rest.Dao
{
    public class Interaction
    {
        private Interaction(InteractionFlow type)
        {
            Type = type.Code;
        }

        public static Interaction DisplayTextAndPIN(string displayText60)
        {
            Interaction interaction = new Interaction(InteractionFlow.DISPLAY_TEXT_AND_PIN)
            {
                DisplayText60 = displayText60
            };
            return interaction;
        }

        public static Interaction VerificationCodeChoice(string displayText60)
        {
            Interaction interaction = new Interaction(InteractionFlow.VERIFICATION_CODE_CHOICE)
            {
                DisplayText60 = displayText60
            };
            return interaction;
        }

        public static Interaction ConfirmationMessage(string displayText200)
        {
            Interaction interaction = new Interaction(InteractionFlow.CONFIRMATION_MESSAGE)
            {
                DisplayText200 = displayText200
            };
            return interaction;
        }

        public static Interaction ConfirmationMessageAndVerificationCodeChoice(string displayText200)
        {
            Interaction interaction = new Interaction(InteractionFlow.CONFIRMATION_MESSAGE_AND_VERIFICATION_CODE_CHOICE)
            {
                DisplayText200 = displayText200
            };
            return interaction;
        }

        [JsonPropertyName("type")]
        public string Type { get; }

        [JsonPropertyName("displayText60")]
        public string DisplayText60 { get; set; }

        [JsonPropertyName("displayText200")]
        public string DisplayText200 { get; set; }

        public void Validate()
        {
            ValidateDisplayText60();
            ValidateDisplayText200();
        }

        private void ValidateDisplayText60()
        {
            if (Type == InteractionFlow.VERIFICATION_CODE_CHOICE.Code || Type == InteractionFlow.DISPLAY_TEXT_AND_PIN.Code)
            {
                if (DisplayText60 == null)
                {
                    throw new SmartIdClientException("displayText60 cannot be null for AllowedInteractionOrder of type " + Type);
                }
                if (DisplayText60.Length > 60)
                {
                    throw new SmartIdClientException("displayText60 must not be longer than 60 characters");
                }
                if (DisplayText200 != null)
                {
                    throw new SmartIdClientException("displayText200 must be null for AllowedInteractionOrder of type " + Type);
                }
            }
        }

        private void ValidateDisplayText200()
        {
            if (Type == InteractionFlow.CONFIRMATION_MESSAGE.Code || Type == InteractionFlow.CONFIRMATION_MESSAGE_AND_VERIFICATION_CODE_CHOICE.Code)
            {
                if (DisplayText200 == null)
                {
                    throw new SmartIdClientException("displayText200 cannot be null for AllowedInteractionOrder of type " + Type);
                }
                if (DisplayText200.Length > 200)
                {
                    throw new SmartIdClientException("displayText200 must not be longer than 200 characters");
                }
                if (DisplayText60 != null)
                {
                    throw new SmartIdClientException("displayText60 must be null for AllowedInteractionOrder of type " + Type);
                }
            }
        }

    }
}