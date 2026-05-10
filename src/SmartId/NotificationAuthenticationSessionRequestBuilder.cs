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
    public sealed class NotificationAuthenticationSessionRequestBuilder
    {
        private readonly ISmartIdConnector connector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private AuthenticationCertificateLevel? certificateLevel;
        private string rpChallenge;
        private AuthenticationSignatureAlgorithm signatureAlgorithm = AuthenticationSignatureAlgorithm.RSASSA_PSS;
        private SmartIdHashAlgorithm hashAlgorithm = SmartIdHashAlgorithm.SHA3_512;
        private IReadOnlyList<NotificationInteraction> interactions;
        private bool? shareMdClientIpAddress;
        private HashSet<string> capabilities;
        private SemanticsIdentifier? semanticsIdentifier;
        private string documentNumber;
        private NotificationAuthenticationSessionRequest notificationAuthenticationSessionRequest;

        public NotificationAuthenticationSessionRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public NotificationAuthenticationSessionRequestBuilder WithRelyingPartyUUID(string relyingPartUUID)
        {
            relyingPartyUUID = relyingPartUUID;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithCertificateLevel(AuthenticationCertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithRpChallenge(string rpChallenge)
        {
            this.rpChallenge = rpChallenge;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithSignatureAlgorithm(AuthenticationSignatureAlgorithm signatureAlgorithm)
        {
            this.signatureAlgorithm = signatureAlgorithm;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithHashAlgorithm(SmartIdHashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithInteractions(IReadOnlyList<NotificationInteraction> interactions)
        {
            this.interactions = interactions;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithShareMdClientIpAddress(bool shareMdClientIpAddress)
        {
            this.shareMdClientIpAddress = shareMdClientIpAddress;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            this.semanticsIdentifier = semanticsIdentifier;
            return this;
        }

        public NotificationAuthenticationSessionRequestBuilder WithDocumentNumber(string documentNumber)
        {
            this.documentNumber = documentNumber;
            return this;
        }

        public async Task<NotificationAuthenticationSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var authenticationRequest = CreateAuthenticationRequest();
            var response = await InitAuthenticationSessionAsync(authenticationRequest, cancellationToken);
            ValidateResponseParameters(response);
            notificationAuthenticationSessionRequest = authenticationRequest;
            return response;
        }

        public NotificationAuthenticationSessionRequest GetAuthenticationSessionRequest()
        {
            if (notificationAuthenticationSessionRequest == null)
            {
                throw new SmartIdClientException("Notification-based authentication session has not been initialized yet");
            }
            return notificationAuthenticationSessionRequest;
        }

        private async Task<NotificationAuthenticationSessionResponse> InitAuthenticationSessionAsync(NotificationAuthenticationSessionRequest authenticationRequest, CancellationToken cancellationToken)
        {
            if (semanticsIdentifier.HasValue && documentNumber != null)
            {
                throw new SmartIdRequestSetupException("Only one of 'semanticsIdentifier' or 'documentNumber' may be set");
            }
            if (semanticsIdentifier.HasValue)
            {
                return await connector.InitNotificationAuthenticationAsync(authenticationRequest, semanticsIdentifier.Value, cancellationToken);
            }
            if (documentNumber != null)
            {
                return await connector.InitNotificationAuthenticationAsync(authenticationRequest, documentNumber, cancellationToken);
            }
            throw new SmartIdRequestSetupException("Either 'documentNumber' or 'semanticsIdentifier' must be set");
        }

        private NotificationAuthenticationSessionRequest CreateAuthenticationRequest()
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
            return new NotificationAuthenticationSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel?.ToString(),
                SignatureProtocol = SignatureProtocol.ACSP_V2.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Interactions = InteractionUtil.EncodeToBase64(InteractionsMapper.From(ToSmartIdInteractions(interactions))),
                RequestProperties = shareMdClientIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareMdClientIpAddress.Value }
                    : null,
                Capabilities = capabilities,
                VcType = VerificationCodeType.NUMERIC4.GetValue()
            };
        }

        private static List<ISmartIdInteraction> ToSmartIdInteractions(IReadOnlyList<NotificationInteraction> list)
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

        private static void ValidateResponseParameters(NotificationAuthenticationSessionResponse response)
        {
            if (string.IsNullOrEmpty(response.SessionID))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based authentication session initialisation response field 'sessionID' is missing or empty");
            }
        }
    }
}
