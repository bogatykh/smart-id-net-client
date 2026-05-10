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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SK.SmartId.Exceptions;
using SK.SmartId.Util;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;

namespace SK.SmartId
{
    public sealed class DeviceLinkCertificateChoiceSessionRequestBuilder
    {
        private static readonly Regex InitialCallbackUrlPattern = new Regex("^https://[^|]+$", RegexOptions.Compiled);

        private readonly ISmartIdConnector connector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private CertificateLevel? certificateLevel;
        private string nonce;
        private HashSet<string> capabilities;
        private bool? shareMdClientIpAddress;
        private string initialCallbackUrl;

        public DeviceLinkCertificateChoiceSessionRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            this.relyingPartyUUID = relyingPartyUUID;
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithCertificateLevel(CertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithNonce(string nonce)
        {
            this.nonce = nonce;
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithShareMdClientIpAddress(bool shareMdClientIpAddress)
        {
            this.shareMdClientIpAddress = shareMdClientIpAddress;
            return this;
        }

        public DeviceLinkCertificateChoiceSessionRequestBuilder WithInitialCallbackUrl(string initialCallbackUrl)
        {
            this.initialCallbackUrl = initialCallbackUrl;
            return this;
        }

        public async Task<DeviceLinkSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var request = new DeviceLinkCertificateChoiceSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel?.ToString(),
                Nonce = nonce,
                Capabilities = capabilities,
                RequestProperties = shareMdClientIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareMdClientIpAddress.Value }
                    : null,
                InitialCallbackUrl = initialCallbackUrl
            };
            var response = await connector.InitDeviceLinkCertificateChoiceAsync(request, cancellationToken);
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
                throw new SmartIdRequestSetupException("Value for 'nonce' must have length between 1 and 30 characters");
            }
            ValidateInitialCallbackUrl();
        }

        private void ValidateInitialCallbackUrl()
        {
            if (!string.IsNullOrEmpty(initialCallbackUrl) && !InitialCallbackUrlPattern.IsMatch(initialCallbackUrl))
            {
                throw new SmartIdRequestSetupException(
                    "Value for 'initialCallbackUrl' must match pattern ^https://[^|]+$ and must not contain unencoded vertical bars");
            }
        }

        private static void ValidateResponseParameters(DeviceLinkSessionResponse response)
        {
            if (string.IsNullOrEmpty(response.SessionID))
            {
                throw new UnprocessableSmartIdResponseException("Device link certificate choice session initialisation response field 'sessionID' is missing or empty");
            }
            if (string.IsNullOrEmpty(response.SessionToken))
            {
                throw new UnprocessableSmartIdResponseException("Device link certificate choice session initialisation response field 'sessionToken' is missing or empty");
            }
            if (string.IsNullOrEmpty(response.SessionSecret))
            {
                throw new UnprocessableSmartIdResponseException("Device link certificate choice session initialisation response field 'sessionSecret' is missing or empty");
            }
            if (string.IsNullOrWhiteSpace(response.DeviceLinkBase))
            {
                throw new UnprocessableSmartIdResponseException("Device link certificate choice session initialisation response field 'deviceLinkBase' is missing or empty");
            }
        }
    }
}
