/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SK.SmartId.Common;
using SK.SmartId.Util;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;

namespace SK.SmartId
{
    public sealed class DeviceLinkAuthenticationSessionRequestBuilder
    {
        private static readonly Regex InitialCallbackUrlPattern = new Regex("^https://[^|]+$", RegexOptions.Compiled);

        private readonly ISmartIdConnector connector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private AuthenticationCertificateLevel certificateLevel = AuthenticationCertificateLevel.QUALIFIED;
        private string rpChallenge;
        private AuthenticationSignatureAlgorithm signatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS;
        private SmartIdHashAlgorithm hashAlgorithm = SmartIdHashAlgorithm.SHA3_512;
        private IReadOnlyList<DeviceLinkInteraction> interactions;
        private bool? shareMdClientIpAddress;
        private HashSet<string> capabilities;
        private SemanticsIdentifier? semanticsIdentifier;
        private string documentNumber;
        private string initialCallbackUrl;
        private DeviceLinkAuthenticationSessionRequest authenticationSessionRequest;

        public DeviceLinkAuthenticationSessionRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithRelyingPartyUUID(string relyingPartUUID)
        {
            relyingPartyUUID = relyingPartUUID;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithCertificateLevel(AuthenticationCertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithRpChallenge(string rpChallenge)
        {
            this.rpChallenge = rpChallenge;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithSignatureAlgorithm(AuthenticationSignatureAlgorithm signatureAlgorithm)
        {
            this.signatureAlgorithm = signatureAlgorithm;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithHashAlgorithm(SmartIdHashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithInteractions(IReadOnlyList<DeviceLinkInteraction> interactions)
        {
            this.interactions = interactions;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithShareMdClientIpAddress(bool shareMdClientIpAddress)
        {
            this.shareMdClientIpAddress = shareMdClientIpAddress;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            this.semanticsIdentifier = semanticsIdentifier;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithDocumentNumber(string documentNumber)
        {
            this.documentNumber = documentNumber;
            return this;
        }

        public DeviceLinkAuthenticationSessionRequestBuilder WithInitialCallbackUrl(string initialCallbackUrl)
        {
            this.initialCallbackUrl = initialCallbackUrl;
            return this;
        }

        public async Task<DeviceLinkSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var authenticationRequest = CreateAuthenticationRequest();
            var response = await InitAuthenticationSessionAsync(authenticationRequest, cancellationToken);
            ValidateResponseParameters(response);
            authenticationSessionRequest = authenticationRequest;
            return response;
        }

        public DeviceLinkAuthenticationSessionRequest GetAuthenticationSessionRequest()
        {
            if (authenticationSessionRequest == null)
            {
                throw new SmartIdClientException("Device link authentication session has not been initialized yet");
            }
            return authenticationSessionRequest;
        }

        private async Task<DeviceLinkSessionResponse> InitAuthenticationSessionAsync(DeviceLinkAuthenticationSessionRequest authenticationRequest, CancellationToken cancellationToken)
        {
            if (semanticsIdentifier.HasValue && documentNumber != null)
            {
                throw new SmartIdRequestSetupException("Only one of 'semanticsIdentifier' or 'documentNumber' may be set");
            }
            if (semanticsIdentifier.HasValue)
            {
                return await connector.InitDeviceLinkAuthenticationAsync(authenticationRequest, semanticsIdentifier.Value, cancellationToken);
            }
            if (documentNumber != null)
            {
                return await connector.InitDeviceLinkAuthenticationAsync(authenticationRequest, documentNumber, cancellationToken);
            }
            return await connector.InitAnonymousDeviceLinkAuthenticationAsync(authenticationRequest, cancellationToken);
        }

        private DeviceLinkAuthenticationSessionRequest CreateAuthenticationRequest()
        {
            var signatureProtocolParameters = new AcspV2SignatureProtocolParameters
            {
                RpChallenge = rpChallenge,
                SignatureAlgorithm = signatureAlgorithm.GetAlgorithmName(),
                SignatureAlgorithmParameters = new SignatureAlgorithmParameters
                {
                    HashAlgorithm = hashAlgorithm.GetApiAlgorithmName()
                }
            };
            return new DeviceLinkAuthenticationSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel.ToString(),
                SignatureProtocol = SignatureProtocol.ACSP_V2,
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(InteractionsMapper.From(ToSmartIdInteractions(interactions))),
                RequestProperties = shareMdClientIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareMdClientIpAddress.Value }
                    : null,
                Capabilities = capabilities,
                InitialCallbackUrl = initialCallbackUrl
            };
        }

        private static List<ISmartIdInteraction> ToSmartIdInteractions(IReadOnlyList<DeviceLinkInteraction> list)
        {
            var r = new List<ISmartIdInteraction>();
            if (list == null)
            {
                return r;
            }
            foreach (var i in list)
            {
                if (i != null)
                {
                    r.Add(i);
                }
            }
            return r;
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
            ValidateSignatureParameters();
            ValidateInteractions();
            ValidateInitialCallbackUrl();
        }

        private void ValidateSignatureParameters()
        {
            if (string.IsNullOrEmpty(rpChallenge))
            {
                throw new SmartIdRequestSetupException("Value for 'rpChallenge' cannot be empty");
            }
            try
            {
                Convert.FromBase64String(rpChallenge);
            }
            catch (FormatException e)
            {
                throw new SmartIdRequestSetupException("Value for 'rpChallenge' must be Base64-encoded string", e);
            }
            if (rpChallenge.Length < 44 || rpChallenge.Length > 88)
            {
                throw new SmartIdRequestSetupException("Value for 'rpChallenge' must have length between 44 and 88 characters");
            }
            if (!Enum.IsDefined(typeof(AuthenticationSignatureAlgorithm), signatureAlgorithm))
            {
                throw new SmartIdRequestSetupException("Value for 'signatureAlgorithm' must be set");
            }
            if (!Enum.IsDefined(typeof(SmartIdHashAlgorithm), hashAlgorithm))
            {
                throw new SmartIdRequestSetupException("Value for 'hashAlgorithm' must be set");
            }
        }

        private void ValidateInteractions()
        {
            if (InteractionUtil.IsEmpty(ToSmartIdInteractions(interactions)))
            {
                throw new SmartIdRequestSetupException("Value for 'interactions' cannot be empty");
            }
            var codes = interactions.Where(i => i != null).Select(i => i.InteractionType.Code).ToList();
            if (codes.Count != codes.Distinct().Count())
            {
                throw new SmartIdRequestSetupException("Value for 'interactions' cannot contain duplicate types");
            }
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
                throw new UnprocessableSmartIdResponseException("Device link authentication session initialisation response field 'sessionID' is missing or empty");
            }
            if (string.IsNullOrEmpty(response.SessionToken))
            {
                throw new UnprocessableSmartIdResponseException("Device link authentication session initialisation response field 'sessionToken' is missing or empty");
            }
            if (string.IsNullOrEmpty(response.SessionSecret))
            {
                throw new UnprocessableSmartIdResponseException("Device link authentication session initialisation response field 'sessionSecret' is missing or empty");
            }
            if (string.IsNullOrWhiteSpace(response.DeviceLinkBase))
            {
                throw new UnprocessableSmartIdResponseException("Device link authentication session initialisation response field 'deviceLinkBase' is missing or empty");
            }
        }
    }
}
