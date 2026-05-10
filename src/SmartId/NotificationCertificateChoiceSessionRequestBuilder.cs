/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2024 SK ID Solutions AS
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SK.SmartId.Exceptions;
using SK.SmartId.Util;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;

namespace SK.SmartId
{
    public sealed class NotificationCertificateChoiceSessionRequestBuilder
    {
        private readonly ISmartIdConnector connector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private CertificateLevel? certificateLevel;
        private string nonce;
        private HashSet<string> capabilities;
        private bool? shareMdClientIpAddress;
        private SemanticsIdentifier? semanticsIdentifier;

        public NotificationCertificateChoiceSessionRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            this.relyingPartyUUID = relyingPartyUUID;
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithCertificateLevel(CertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithNonce(string nonce)
        {
            this.nonce = nonce;
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithShareMdClientIpAddress(bool shareMdClientIpAddress)
        {
            this.shareMdClientIpAddress = shareMdClientIpAddress;
            return this;
        }

        public NotificationCertificateChoiceSessionRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            this.semanticsIdentifier = semanticsIdentifier;
            return this;
        }

        public async Task<NotificationCertificateChoiceSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var request = new NotificationCertificateChoiceSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel?.ToString(),
                Nonce = nonce,
                Capabilities = capabilities,
                RequestProperties = shareMdClientIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareMdClientIpAddress.Value }
                    : null
            };
            if (!semanticsIdentifier.HasValue)
            {
                throw new SmartIdRequestSetupException("Value for 'semanticIdentifier' must be set");
            }
            var response = await connector.InitNotificationCertificateChoiceAsync(request, semanticsIdentifier.Value, cancellationToken);
            ValidateResponseParameters(response);
            return response;
        }

        private void ValidateRequestParameters()
        {
            if (string.IsNullOrEmpty(relyingPartyUUID))
            {
                throw new SmartIdRequestSetupException("Value for 'relyingPartyUUID' cannot be empty");
            }
            if (string.IsNullOrEmpty(relyingPartyName))
            {
                throw new SmartIdRequestSetupException("Value for 'relyingPartyName' cannot be empty");
            }
            if (nonce != null && (nonce.Length == 0 || nonce.Length > 30))
            {
                throw new SmartIdRequestSetupException("Value for 'nonce' length must be between 1 and 30 characters");
            }
        }

        private static void ValidateResponseParameters(NotificationCertificateChoiceSessionResponse response)
        {
            if (string.IsNullOrEmpty(response.SessionID))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based certificate choice response field 'sessionID' is missing or empty");
            }
        }
    }
}
