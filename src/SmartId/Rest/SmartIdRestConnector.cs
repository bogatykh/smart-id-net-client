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

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest.Dao;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    public class SmartIdRestConnector : ISmartIdConnector
    {
        private const string SESSION_STATUS_URI = "session/{0}";

        private const string CERTIFICATE_CHOICE_BY_DOCUMENT_NUMBER_PATH = "certificatechoice/document/{0}";
        private const string CERTIFICATE_CHOICE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER = "certificatechoice/etsi/{0}";

        private const string SIGNATURE_BY_DOCUMENT_NUMBER_PATH = "signature/document/{0}";
        private const string SIGNATURE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER = "signature/etsi/{0}";

        private const string AUTHENTICATE_BY_DOCUMENT_NUMBER_PATH = "authentication/document/{0}";
        private const string AUTHENTICATE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER = "authentication/etsi/{0}";

        private readonly string endpointUrl;
        private readonly HttpClient configuredClient;
        private TimeSpan? sessionStatusResponseSocketOpenTime;

        public SmartIdRestConnector(string endpointUrl)
            : this(endpointUrl, new HttpClient())
        {
        }

        public SmartIdRestConnector(string endpointUrl, HttpClient configuredClient)
        {
            this.endpointUrl = endpointUrl;
            this.configuredClient = configuredClient;
        }

        public async Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken)
        {
            SessionStatusRequest request = CreateSessionStatusRequest(sessionId);

            var uriBuilder = new UriBuilder(new Uri(new Uri(endpointUrl), string.Format(SESSION_STATUS_URI, request.SessionId)));

            AddResponseSocketOpenTimeUrlParameter(request, uriBuilder);

            var responseMessage = await configuredClient.GetAsync(uriBuilder.Uri, cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SessionNotFoundException();
                }
            }

            responseMessage.EnsureSuccessStatusCode();

            return await JsonSerializer.DeserializeAsync<SessionStatus>(await responseMessage.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        }

        public async Task<CertificateChoiceResponse> GetCertificateAsync(string documentNumber, CertificateRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(CERTIFICATE_CHOICE_BY_DOCUMENT_NUMBER_PATH, documentNumber));

            return await PostCertificateRequestAsync(uri, request, cancellationToken);
        }

        public async Task<CertificateChoiceResponse> GetCertificateAsync(SemanticsIdentifier semanticsIdentifier,
            CertificateRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(CERTIFICATE_CHOICE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER, semanticsIdentifier.Identifier));

            return await PostCertificateRequestAsync(uri, request, cancellationToken);
        }

        public async Task<SignatureSessionResponse> SignAsync(string documentNumber, SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(SIGNATURE_BY_DOCUMENT_NUMBER_PATH, documentNumber));

            return await PostSigningRequestAsync(uri, request, cancellationToken);
        }

        public async Task<SignatureSessionResponse> SignAsync(SemanticsIdentifier semanticsIdentifier, SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(SIGNATURE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER, semanticsIdentifier.Identifier));

            return await PostSigningRequestAsync(uri, request, cancellationToken);
        }

        public async Task<AuthenticationSessionResponse> AuthenticateAsync(String documentNumber, AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(AUTHENTICATE_BY_DOCUMENT_NUMBER_PATH, documentNumber));

            return await PostAuthenticationRequestAsync(uri, request, cancellationToken);
        }

        public async Task<AuthenticationSessionResponse> AuthenticateAsync(SemanticsIdentifier semanticsIdentifier, AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            var uri = new Uri(new Uri(endpointUrl), string.Format(AUTHENTICATE_BY_NATURAL_PERSON_SEMANTICS_IDENTIFIER, semanticsIdentifier.Identifier));

            return await PostAuthenticationRequestAsync(uri, request, cancellationToken);
        }

        public void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime)
        {
            this.sessionStatusResponseSocketOpenTime = sessionStatusResponseSocketOpenTime;
        }

        private async Task<CertificateChoiceResponse> PostCertificateRequestAsync(Uri uri, CertificateRequest request, CancellationToken cancellationToken)
        {
            return await PostRequestAsync<CertificateChoiceResponse, CertificateRequest>(uri, request, cancellationToken);
        }

        private async Task<AuthenticationSessionResponse> PostAuthenticationRequestAsync(Uri uri, AuthenticationSessionRequest request, CancellationToken cancellationToken)
        {
            return await PostRequestAsync<AuthenticationSessionResponse, AuthenticationSessionRequest>(uri, request, cancellationToken);
        }

        private async Task<SignatureSessionResponse> PostSigningRequestAsync(Uri uri, SignatureSessionRequest request, CancellationToken cancellationToken)
        {
            return await PostRequestAsync<SignatureSessionResponse, SignatureSessionRequest>(uri, request, cancellationToken);
        }

        private async Task<T> PostRequestAsync<T, V>(Uri uri, V request, CancellationToken cancellationToken)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }), System.Text.Encoding.UTF8, "application/json");
            stringContent.Headers.TryAddWithoutValidation("User-Agent", BuildUserAgentString());

            var responseMessage = await configuredClient.PostAsync(uri, stringContent, cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RelyingPartyAccountConfigurationException("Request is unauthorized for URI " + uri);
                }
                else if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new SmartIdClientException("Server refused the request");
                }
                else if (responseMessage.StatusCode == (HttpStatusCode)471)
                {
                    throw new NoSuitableAccountOfRequestedTypeFoundException();
                }
                else if (responseMessage.StatusCode == (HttpStatusCode)472)
                {
                    throw new PersonShouldViewSmartIdPortalException();
                }
                else if (responseMessage.StatusCode == (HttpStatusCode)480)
                {
                    throw new SmartIdClientException("Client-side API is too old and not supported anymore");
                }
                else if (responseMessage.StatusCode == (HttpStatusCode)580)
                {
                    throw new ServerMaintenanceException();
                }
                else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new UserAccountNotFoundException();
                }
                else if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new RelyingPartyAccountConfigurationException("No permission to issue the request");
                }
            }

            responseMessage.EnsureSuccessStatusCode();

            return await JsonSerializer.DeserializeAsync<T>(await responseMessage.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        }

        private SessionStatusRequest CreateSessionStatusRequest(string sessionId)
        {
            SessionStatusRequest request = new SessionStatusRequest(sessionId);
            if (sessionStatusResponseSocketOpenTime != null && sessionStatusResponseSocketOpenTime.Value.TotalMilliseconds > 0)
            {
                request.ResponseSocketOpenTime = sessionStatusResponseSocketOpenTime.Value;
            }
            return request;
        }

        private void AddResponseSocketOpenTimeUrlParameter(SessionStatusRequest request, UriBuilder uriBuilder)
        {
            if (request.IsResponseSocketOpenTimeSet())
            {
                TimeSpan queryTimeout = request.ResponseSocketOpenTime.Value;
                uriBuilder.Query = $"timeoutMs={queryTimeout.TotalMilliseconds}";
            }
        }

        protected string BuildUserAgentString()
        {
            return "smart-id-net-client/" + GetClientVersion() + " (.NET/" + Environment.Version + ")";
        }

        protected string GetClientVersion()
        {
            var assemblyVersionAttribute = GetType().Assembly
                .GetCustomAttributes<AssemblyVersionAttribute>()
                .SingleOrDefault();

            return assemblyVersionAttribute?.Version ?? "-";
        }
    }
}