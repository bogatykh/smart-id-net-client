/*-
 * #%L
 * Smart ID .NET client — unit tests
 * #L%
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Support
{
    /// <summary>
    /// Minimal request router for ports of Java WireMock-based tests (no open port).
    /// </summary>
    internal sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<Rule> _rules = new List<Rule>();

        public HttpRequestMessage LastRequest { get; private set; }

        internal void AddJsonResponse(HttpMethod method, Func<Uri, bool> pathMatch, string responseRelativePath, HttpStatusCode status = HttpStatusCode.OK)
        {
            _rules.Add(new Rule
            {
                Method = method,
                PathMatch = pathMatch,
                RequestBodyPredicate = null,
                Status = status,
                ResponseFactory = _ => Task.FromResult(TestResourcePaths.ReadAllText(responseRelativePath))
            });
        }

        internal void AddPostJsonWhenBodyEqualsFile(string pathSuffix, string requestRelativePath, string responseRelativePath, bool deepOrderIndependent = true)
        {
            var expected = JToken.Parse(TestResourcePaths.ReadAllText(requestRelativePath));
            _rules.Add(new Rule
            {
                Method = HttpMethod.Post,
                PathMatch = u => u.AbsolutePath.EndsWith(pathSuffix, StringComparison.Ordinal),
                RequestBodyPredicate = body =>
                {
                    if (string.IsNullOrEmpty(body)) return false;
                    var actual = JToken.Parse(body);
                    return deepOrderIndependent ? JToken.DeepEquals(actual, expected) : string.Equals(body.Trim(), expected.ToString(), StringComparison.Ordinal);
                },
                Status = HttpStatusCode.OK,
                ResponseFactory = _ => Task.FromResult(TestResourcePaths.ReadAllText(responseRelativePath))
            });
        }

        /// <summary>POST whose JSON body must match a fixture; response body empty (Java WireMock error stubs).</summary>
        internal void AddPostJsonWhenBodyEqualsFileWithStatus(string pathSuffix, string requestRelativePath, HttpStatusCode status, bool deepOrderIndependent = true)
        {
            var expected = JToken.Parse(TestResourcePaths.ReadAllText(requestRelativePath));
            _rules.Add(new Rule
            {
                Method = HttpMethod.Post,
                PathMatch = u => u.AbsolutePath.EndsWith(pathSuffix, StringComparison.Ordinal),
                RequestBodyPredicate = body =>
                {
                    if (string.IsNullOrEmpty(body)) return false;
                    var actual = JToken.Parse(body);
                    return deepOrderIndependent ? JToken.DeepEquals(actual, expected) : string.Equals(body.Trim(), expected.ToString(), StringComparison.Ordinal);
                },
                Status = status,
                ResponseFactory = _ => Task.FromResult("")
            });
        }

        internal void AddPostEmptyError(string pathSuffix, HttpStatusCode status)
        {
            _rules.Add(new Rule
            {
                Method = HttpMethod.Post,
                PathMatch = u => u.AbsolutePath.EndsWith(pathSuffix, StringComparison.Ordinal),
                RequestBodyPredicate = _ => true,
                Status = status,
                ResponseFactory = _ => Task.FromResult("")
            });
        }

        internal void AddGetNotFound(Func<Uri, bool> pathMatch)
        {
            _rules.Add(new Rule
            {
                Method = HttpMethod.Get,
                PathMatch = pathMatch,
                RequestBodyPredicate = null,
                Status = HttpStatusCode.NotFound,
                ResponseFactory = _ => Task.FromResult("Not found")
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var uri = request.RequestUri;
            string body = null;
            if (request.Content != null)
            {
                body = await request.Content.ReadAsStringAsync();
            }

            foreach (var rule in _rules)
            {
                if (request.Method != rule.Method || !rule.PathMatch(uri))
                {
                    continue;
                }
                if (rule.RequestBodyPredicate != null && !rule.RequestBodyPredicate(body))
                {
                    continue;
                }

                var text = await rule.ResponseFactory(body);
                var msg = new HttpResponseMessage(rule.Status);
                if (!string.IsNullOrEmpty(text))
                {
                    msg.Content = new StringContent(text, Encoding.UTF8, "application/json");
                }
                else
                {
                    msg.Content = new StringContent("", Encoding.UTF8, "application/json");
                }
                return msg;
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found", Encoding.UTF8, "application/json")
            };
        }

        private sealed class Rule
        {
            public HttpMethod Method { get; set; }
            public Func<Uri, bool> PathMatch { get; set; }
            public Func<string, bool> RequestBodyPredicate { get; set; }
            public HttpStatusCode Status { get; set; }
            public Func<string, Task<string>> ResponseFactory { get; set; }
        }
    }
}
