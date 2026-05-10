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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    /// <summary>
    /// REST client for Smart-ID session API. For request/response logging similar to Java <c>SmartIdRestConnector</c>
    /// registering <c>LoggingFilter</c>, pass an <see cref="HttpClient"/> whose pipeline includes
    /// <see cref="SmartIdHttpLoggingHandler"/> (see class remarks there).
    /// </summary>
    public class SmartIdRestConnector : ISmartIdConnector
    {
        private static readonly JsonSerializerOptions JsonWriteOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly JsonSerializerOptions JsonReadOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        private const string SessionStatusUri = "session";

        private const string DeviceLinkCertificateChoiceDeviceLinkPath = "signature/certificate-choice/device-link/anonymous";
        private const string LinkedNotificationSignatureWithDocumentNumberPath = "signature/notification/linked";

        private const string NotificationCertificateChoiceWithSemanticIdentifierPath = "signature/certificate-choice/notification/etsi";

        private const string CertificateByDocumentNumberPath = "signature/certificate";

        private const string DeviceLinkSignatureWithSemanticIdentifierPath = "signature/device-link/etsi";
        private const string DeviceLinkSignatureWithDocumentNumberPath = "signature/device-link/document";

        private const string NotificationSignatureWithSemanticIdentifierPath = "signature/notification/etsi";
        private const string NotificationSignatureWithDocumentNumberPath = "signature/notification/document";

        private const string AnonymousDeviceLinkAuthenticationPath = "authentication/device-link/anonymous";
        private const string DeviceLinkAuthenticationWithSemanticIdentifierPath = "authentication/device-link/etsi";
        private const string DeviceLinkAuthenticationWithDocumentNumberPath = "authentication/device-link/document";

        private const string NotificationAuthenticationWithSemanticIdentifierPath = "authentication/notification/etsi";
        private const string NotificationAuthenticationWithDocumentNumberPath = "authentication/notification/document";

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

        public async Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            SessionStatusRequest request = CreateSessionStatusRequest(sessionId);

            var uri = AppendPath(CombineEndpointRoot(), SessionStatusUri, request.SessionId);
            uri = WithTimeoutMs(uri, request);

            using (var req = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                req.Headers.TryAddWithoutValidation("User-Agent", BuildUserAgentString());

                using (var responseMessage = await configuredClient.SendAsync(req, cancellationToken))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new SessionNotFoundException();
                    }

                    responseMessage.EnsureSuccessStatusCode();

                    using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        return await JsonSerializer.DeserializeAsync<SessionStatus>(stream, JsonReadOptions, cancellationToken);
                    }
                }
            }
        }

        public void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime)
        {
            this.sessionStatusResponseSocketOpenTime = sessionStatusResponseSocketOpenTime;
        }

        public Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), DeviceLinkAuthenticationWithSemanticIdentifierPath, semanticsIdentifier.Identifier);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), DeviceLinkAuthenticationWithDocumentNumberPath, documentNumber);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<DeviceLinkSessionResponse> InitAnonymousDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), AnonymousDeviceLinkAuthenticationPath);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), NotificationAuthenticationWithSemanticIdentifierPath, semanticsIdentifier.Identifier);
            return PostRequestAsync<NotificationAuthenticationSessionResponse>(uri, request, cancellationToken);
        }

        public Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), NotificationAuthenticationWithDocumentNumberPath, documentNumber);
            return PostRequestAsync<NotificationAuthenticationSessionResponse>(uri, request, cancellationToken);
        }

        public Task<DeviceLinkSessionResponse> InitDeviceLinkCertificateChoiceAsync(DeviceLinkCertificateChoiceSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), DeviceLinkCertificateChoiceDeviceLinkPath);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<LinkedSignatureSessionResponse> InitLinkedNotificationSignatureAsync(LinkedSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), LinkedNotificationSignatureWithDocumentNumberPath, documentNumber);
            return PostRequestAsync<LinkedSignatureSessionResponse>(uri, request, cancellationToken);
        }

        public Task<NotificationCertificateChoiceSessionResponse> InitNotificationCertificateChoiceAsync(NotificationCertificateChoiceSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), NotificationCertificateChoiceWithSemanticIdentifierPath, semanticsIdentifier.Identifier);
            return PostRequestAsync<NotificationCertificateChoiceSessionResponse>(uri, request, cancellationToken);
        }

        public Task<CertificateResponse> GetCertificateByDocumentNumberAsync(string documentNumber, CertificateByDocumentNumberRequest request,
            CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), CertificateByDocumentNumberPath, documentNumber);
            return PostRequestAsync<CertificateResponse>(uri, request, cancellationToken);
        }

        public Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), DeviceLinkSignatureWithSemanticIdentifierPath, semanticsIdentifier.Identifier);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), DeviceLinkSignatureWithDocumentNumberPath, documentNumber);
            return PostRequestAsync<DeviceLinkSessionResponse>(uri, request, cancellationToken);
        }

        public Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request,
            SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), NotificationSignatureWithSemanticIdentifierPath, semanticsIdentifier.Identifier);
            return PostRequestAsync<NotificationSignatureSessionResponse>(uri, request, cancellationToken);
        }

        public Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request,
            string documentNumber, CancellationToken cancellationToken = default)
        {
            var uri = AppendPath(CombineEndpointRoot(), NotificationSignatureWithDocumentNumberPath, documentNumber);
            return PostRequestAsync<NotificationSignatureSessionResponse>(uri, request, cancellationToken);
        }

        private Uri CombineEndpointRoot()
        {
            var trimmed = endpointUrl?.TrimEnd('/') ?? "";
            return new Uri(trimmed + "/", UriKind.Absolute);
        }

        private static Uri AppendPath(Uri root, params string[] segments)
        {
            var relative = string.Join("/", segments);
            return new Uri(root, relative);
        }

        private static Uri WithTimeoutMs(Uri uri, SessionStatusRequest request)
        {
            if (!request.IsResponseSocketOpenTimeSet())
            {
                return uri;
            }

            var ms = (long)request.ResponseSocketOpenTime.Value.TotalMilliseconds;
            var ub = new UriBuilder(uri) { Query = "timeoutMs=" + ms };
            return ub.Uri;
        }

        private async Task<T> PostRequestAsync<T>(Uri uri, object body, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(body, body.GetType(), JsonWriteOptions);
            using (var req = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                req.Headers.TryAddWithoutValidation("User-Agent", BuildUserAgentString());
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var responseMessage = await configuredClient.SendAsync(req, cancellationToken))
                {
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        MapPostError(uri, responseMessage);
                        responseMessage.EnsureSuccessStatusCode();
                    }

                    using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        return await JsonSerializer.DeserializeAsync<T>(stream, JsonReadOptions, cancellationToken);
                    }
                }
            }
        }

        private static void MapPostError(Uri uri, HttpResponseMessage responseMessage)
        {
            var code = responseMessage.StatusCode;
            if (code == HttpStatusCode.Unauthorized)
            {
                throw new RelyingPartyAccountConfigurationException("Request is unauthorized for URI " + uri);
            }
            if (code == HttpStatusCode.BadRequest)
            {
                throw new SmartIdClientException("Server refused the request");
            }
            if (code == HttpStatusCode.NotFound)
            {
                throw new UserAccountNotFoundException();
            }
            if (code == HttpStatusCode.Forbidden)
            {
                throw new RelyingPartyAccountConfigurationException("No permission to issue the request");
            }
            if ((int)code == 471)
            {
                throw new NoSuitableAccountOfRequestedTypeFoundException();
            }
            if ((int)code == 472)
            {
                throw new PersonShouldViewSmartIdPortalException();
            }
            if ((int)code == 480)
            {
                throw new SmartIdClientException("Client-side API is too old and not supported anymore");
            }
            if ((int)code == 580)
            {
                throw new ServerMaintenanceException();
            }
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

        protected string BuildUserAgentString()
        {
            return "smart-id-net-client/" + GetClientVersion() + " (.NET/" + Environment.Version + ")";
        }

        protected string GetClientVersion()
        {
            var assemblyVersionAttribute = GetType().Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (assemblyVersionAttribute?.InformationalVersion != null)
            {
                return assemblyVersionAttribute.InformationalVersion;
            }

            return GetType().Assembly.GetName().Version?.ToString() ?? "-";
        }
    }
}
