/*-
 * #%L
 * Smart ID .NET client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    /// <summary>
    /// Optional <see cref="DelegatingHandler"/> for HTTP request/response logging (Java Jersey <c>LoggingFilter</c> parity).
    /// Wire as the outer handler: <c>new HttpClient(new SmartIdHttpLoggingHandler { InnerHandler = new HttpClientHandler() })</c>.
    /// </summary>
    public sealed class SmartIdHttpLoggingHandler : DelegatingHandler
    {
        /// <summary>Called for each request line (method + URI) and response status (Java debug level).</summary>
        public System.Action<string> LogDebug { get; set; }

        /// <summary>Called for request/response bodies when you need payload capture (Java trace level).</summary>
        public System.Action<string> LogTrace { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogDebug?.Invoke(request.Method + " " + request.RequestUri);

            if (LogTrace != null && request.Content != null)
            {
                string requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                LogTrace("Request body: " + requestBody);
                MediaTypeHeaderValue media = request.Content.Headers.ContentType;
                string mt = media?.MediaType ?? "application/json";
                request.Content = new StringContent(requestBody, Encoding.UTF8, mt);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            LogDebug?.Invoke("Response " + (int)response.StatusCode + " " + response.ReasonPhrase);

            if (LogTrace != null && response.Content != null)
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                LogTrace("Response body: " + Encoding.UTF8.GetString(bytes));
                var replacement = new ByteArrayContent(bytes);
                foreach (var h in response.Content.Headers)
                {
                    replacement.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                response.Content.Dispose();
                response.Content = replacement;
            }

            return response;
        }
    }
}
