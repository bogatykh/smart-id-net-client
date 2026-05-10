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
    public sealed class NotificationSignatureSessionRequestBuilder
    {
        private static readonly Regex VerificationCodePattern = new Regex("^[0-9]{4}$", RegexOptions.Compiled);

        private readonly ISmartIdConnector connector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private string documentNumber;
        private SemanticsIdentifier? semanticsIdentifier;
        private CertificateLevel? certificateLevel;
        private string nonce;
        private HashSet<string> capabilities;
        private IReadOnlyList<NotificationInteraction> interactions;
        private bool? shareMdClientIpAddress;
        private SigningSignatureAlgorithm signatureAlgorithm = SigningSignatureAlgorithm.RSASSA_PSS;
        private IDigestInput digestInput;

        public NotificationSignatureSessionRequestBuilder(ISmartIdConnector connector)
        {
            this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public NotificationSignatureSessionRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            this.relyingPartyUUID = relyingPartyUUID;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithDocumentNumber(string documentNumber)
        {
            this.documentNumber = documentNumber;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithSemanticsIdentifier(SemanticsIdentifier semanticsIdentifier)
        {
            this.semanticsIdentifier = semanticsIdentifier;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithCertificateLevel(CertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithNonce(string nonce)
        {
            this.nonce = nonce;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithInteractions(IReadOnlyList<NotificationInteraction> interactions)
        {
            this.interactions = interactions;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithShareMdClientIpAddress(bool shareMdClientIpAddress)
        {
            this.shareMdClientIpAddress = shareMdClientIpAddress;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithSignatureAlgorithm(SigningSignatureAlgorithm signatureAlgorithm)
        {
            this.signatureAlgorithm = signatureAlgorithm;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithSignableData(SignableData signableData)
        {
            if (digestInput != null && digestInput is SignableHash)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' has already been set with SignableHash");
            }
            digestInput = signableData;
            return this;
        }

        public NotificationSignatureSessionRequestBuilder WithSignableHash(SignableHash signableHash)
        {
            if (digestInput != null && digestInput is SignableData)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' has already been set with SignableData");
            }
            digestInput = signableHash;
            return this;
        }

        public async Task<NotificationSignatureSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var request = CreateSignatureSessionRequest();
            var response = await InitSignatureSessionAsync(request, cancellationToken);
            ValidateResponseParameters(response);
            return response;
        }

        private async Task<NotificationSignatureSessionResponse> InitSignatureSessionAsync(NotificationSignatureSessionRequest request, CancellationToken cancellationToken)
        {
            if (semanticsIdentifier.HasValue && documentNumber != null)
            {
                throw new SmartIdRequestSetupException("Only one of 'semanticsIdentifier' or 'documentNumber' may be set");
            }
            if (documentNumber != null)
            {
                return await connector.InitNotificationSignatureAsync(request, documentNumber, cancellationToken);
            }
            if (semanticsIdentifier.HasValue)
            {
                return await connector.InitNotificationSignatureAsync(request, semanticsIdentifier.Value, cancellationToken);
            }
            throw new SmartIdRequestSetupException("Either 'documentNumber' or 'semanticsIdentifier' must be set");
        }

        private NotificationSignatureSessionRequest CreateSignatureSessionRequest()
        {
            SignatureAlgorithmParameters algorithmParams = signatureAlgorithm.IsLegacyRsa()
                ? null
                : new SignatureAlgorithmParameters
                {
                    HashAlgorithm = digestInput.GetHashAlgorithm().GetApiAlgorithmName()
                };
            var signatureProtocolParameters = new RawDigestSignatureProtocolParameters
            {
                Digest = digestInput.GetDigestInBase64(),
                SignatureAlgorithm = signatureAlgorithm.GetAlgorithmName(),
                SignatureAlgorithmParameters = algorithmParams
            };
            return new NotificationSignatureSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel?.ToString(),
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = signatureProtocolParameters,
                Nonce = nonce,
                Capabilities = capabilities,
                Interactions = InteractionUtil.EncodeToBase64(InteractionsMapper.From(ToSmartIdInteractions(interactions))),
                RequestProperties = shareMdClientIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareMdClientIpAddress.Value }
                    : null
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
            if (!Enum.IsDefined(typeof(SigningSignatureAlgorithm), signatureAlgorithm))
            {
                throw new SmartIdRequestSetupException("Value for 'signatureAlgorithm' must be set");
            }
            if (digestInput == null)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' must be set with either SignableData or SignableHash");
            }
            if (digestInput is SignableHash sh)
            {
                sh.Validate();
            }
            ValidateInteractions();
            if (nonce != null && (nonce.Length == 0 || nonce.Length > 30))
            {
                throw new SmartIdRequestSetupException("Value for 'nonce' length must be between 1 and 30 characters");
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

        private static void ValidateResponseParameters(NotificationSignatureSessionResponse response)
        {
            if (string.IsNullOrEmpty(response.SessionID))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'sessionID' is missing or empty");
            }
            if (response.Vc == null)
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'vc' is missing");
            }
            if (string.IsNullOrEmpty(response.Vc.Type))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'vc.type' is missing or empty");
            }
            if (response.Vc.Type != VerificationCodeType.NUMERIC4.GetValue())
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'vc.type' contains unsupported value");
            }
            if (string.IsNullOrEmpty(response.Vc.Value))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'vc.value' is missing or empty");
            }
            if (!VerificationCodePattern.IsMatch(response.Vc.Value))
            {
                throw new UnprocessableSmartIdResponseException("Notification-based signature response field 'vc.value' does not match the required pattern");
            }
        }
    }
}
