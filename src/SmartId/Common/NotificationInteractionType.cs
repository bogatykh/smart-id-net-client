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

namespace SK.SmartId.Common
{
    public sealed class NotificationInteractionType : IInteractionType
    {
        public static readonly NotificationInteractionType DisplayTextAndPin =
            new NotificationInteractionType("displayTextAndPIN", 60);

        public static readonly NotificationInteractionType ConfirmationMessage =
            new NotificationInteractionType("confirmationMessage", 200);

        public static readonly NotificationInteractionType ConfirmationMessageAndVerificationCodeChoice =
            new NotificationInteractionType("confirmationMessageAndVerificationCodeChoice", 200);

        private NotificationInteractionType(string code, int maxLength)
        {
            Code = code;
            MaxLength = maxLength;
        }

        public string Code { get; }

        public int MaxLength { get; }
    }
}
