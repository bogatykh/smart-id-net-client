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
    public sealed class LinkedNotificationSignatureSessionRequestBuilder
    {
        private readonly ISmartIdConnector smartIdConnector;

        private string relyingPartyUUID;
        private string relyingPartyName;
        private string documentNumber;
        private IDigestInput digestInput;
        private SigningSignatureAlgorithm signatureAlgorithm = SigningSignatureAlgorithm.RSASSA_PSS;
        private string linkedSessionID;
        private IReadOnlyList<DeviceLinkInteraction> interactions;
        private CertificateLevel? certificateLevel;
        private string nonce;
        private bool? shareIpAddress;
        private HashSet<string> capabilities;

        public LinkedNotificationSignatureSessionRequestBuilder(ISmartIdConnector smartIdConnector)
        {
            this.smartIdConnector = smartIdConnector ?? throw new ArgumentNullException(nameof(smartIdConnector));
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithRelyingPartyUUID(string relyingPartyUUID)
        {
            this.relyingPartyUUID = relyingPartyUUID;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithRelyingPartyName(string relyingPartyName)
        {
            this.relyingPartyName = relyingPartyName;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithCertificateLevel(CertificateLevel certificateLevel)
        {
            this.certificateLevel = certificateLevel;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithDocumentNumber(string documentNumber)
        {
            this.documentNumber = documentNumber;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithSignableData(SignableData signableData)
        {
            if (digestInput != null && digestInput is SignableHash)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' has been already set with SignableHash");
            }
            digestInput = signableData;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithSignableHash(SignableHash signableHash)
        {
            if (digestInput != null && digestInput is SignableData)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' has been already set with SignableData");
            }
            digestInput = signableHash;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithSignatureAlgorithm(SigningSignatureAlgorithm signatureAlgorithm)
        {
            this.signatureAlgorithm = signatureAlgorithm;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithLinkedSessionID(string linkedSessionID)
        {
            this.linkedSessionID = linkedSessionID;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithNonce(string nonce)
        {
            this.nonce = nonce;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithInteractions(IReadOnlyList<DeviceLinkInteraction> interactions)
        {
            this.interactions = interactions;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithShareMdClientIpAddress(bool shareIpAddress)
        {
            this.shareIpAddress = shareIpAddress;
            return this;
        }

        public LinkedNotificationSignatureSessionRequestBuilder WithCapabilities(params string[] capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException(nameof(capabilities));
            }
            this.capabilities = SetUtil.ToSet(capabilities);
            return this;
        }

        public async Task<LinkedSignatureSessionResponse> InitAsync(CancellationToken cancellationToken = default)
        {
            ValidateRequestParameters();
            var request = CreateSessionRequest();
            var response = await smartIdConnector.InitLinkedNotificationSignatureAsync(request, documentNumber, cancellationToken);
            ValidateResponse(response);
            return response;
        }

        private LinkedSignatureSessionRequest CreateSessionRequest()
        {
            SignatureAlgorithmParameters algorithmParams = signatureAlgorithm.IsLegacyRsa()
                ? null
                : new SignatureAlgorithmParameters
                {
                    HashAlgorithm = digestInput.GetHashAlgorithm().GetApiAlgorithmName()
                };
            var rawDigestParams = new RawDigestSignatureProtocolParameters
            {
                Digest = digestInput.GetDigestInBase64(),
                SignatureAlgorithm = signatureAlgorithm.GetAlgorithmName(),
                SignatureAlgorithmParameters = algorithmParams
            };
            return new LinkedSignatureSessionRequest
            {
                RelyingPartyUUID = relyingPartyUUID,
                RelyingPartyName = relyingPartyName,
                CertificateLevel = certificateLevel?.ToString(),
                SignatureProtocol = SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
                SignatureProtocolParameters = rawDigestParams,
                LinkedSessionID = linkedSessionID,
                Nonce = nonce,
                Interactions = InteractionUtil.EncodeToBase64(InteractionsMapper.From(ToSmartIdInteractions(interactions))),
                RequestProperties = shareIpAddress.HasValue
                    ? new RequestProperties { ShareMdClientIpAddress = shareIpAddress.Value }
                    : null,
                Capabilities = capabilities
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
            if (string.IsNullOrEmpty(documentNumber))
            {
                throw new SmartIdRequestSetupException("Value for 'documentNumber' cannot be empty");
            }
            if (digestInput == null)
            {
                throw new SmartIdRequestSetupException("Value for 'digestInput' must be set with SignableData or with SignableHash");
            }
            if (digestInput is SignableHash sh)
            {
                sh.Validate();
            }
            if (!Enum.IsDefined(typeof(SigningSignatureAlgorithm), signatureAlgorithm))
            {
                throw new SmartIdRequestSetupException("Value for 'signatureAlgorithm' must be set");
            }
            if (string.IsNullOrEmpty(linkedSessionID))
            {
                throw new SmartIdRequestSetupException("Value for 'linkedSessionID' cannot be empty");
            }
            if (nonce != null && (nonce.Length == 0 || nonce.Length > 30))
            {
                throw new SmartIdRequestSetupException("Value for 'nonce' must be 1-30 characters long");
            }
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

        private static void ValidateResponse(LinkedSignatureSessionResponse response)
        {
            if (string.IsNullOrEmpty(response.SessionID))
            {
                throw new UnprocessableSmartIdResponseException("Linked notification-base signature session response field 'sessionID' is missing or empty");
            }
        }
    }
}
