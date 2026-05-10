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

using SK.SmartId.Rest.Dao;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    public interface ISmartIdConnector
    {
        Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default);

        void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime);

        Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default);

        Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default);

        Task<DeviceLinkSessionResponse> InitAnonymousDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            CancellationToken cancellationToken = default);

        Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default);

        Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default);

        Task<DeviceLinkSessionResponse> InitDeviceLinkCertificateChoiceAsync(DeviceLinkCertificateChoiceSessionRequest request,
            CancellationToken cancellationToken = default);

        Task<LinkedSignatureSessionResponse> InitLinkedNotificationSignatureAsync(LinkedSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default);

        Task<NotificationCertificateChoiceSessionResponse> InitNotificationCertificateChoiceAsync(NotificationCertificateChoiceSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default);

        Task<CertificateResponse> GetCertificateByDocumentNumberAsync(string documentNumber, CertificateByDocumentNumberRequest request,
            CancellationToken cancellationToken = default);

        Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default);

        Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default);

        Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default);

        Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default);
    }
}
