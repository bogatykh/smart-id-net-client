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

using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId
{
    public static class SmartIdRestServiceStubs
    {
        public static void StubNotFoundResponse(Mock<HttpMessageHandler> handlerMock, string urlEquals)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not found", Encoding.UTF8, "application/json")
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri.AbsoluteUri.EndsWith(urlEquals)),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();
        }

        public static void StubNotFoundResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile)
        {
            StubErrorResponse(handlerMock, url, requestFile, 404);
        }

        public static void StubUnauthorizedResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile)
        {
            StubErrorResponse(handlerMock, url, requestFile, 401);
        }

        public static void StubBadRequestResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile)
        {
            StubErrorResponse(handlerMock, url, requestFile, 400);
        }

        public static void StubForbiddenResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile)
        {
            StubErrorResponse(handlerMock, url, requestFile, 403);
        }

        public static void StubErrorResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile, int errorStatus)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)errorStatus,
                Content = new StringContent("Not found", Encoding.UTF8, "application/json")
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Post && x.RequestUri.AbsoluteUri.EndsWith(url) && CompareJson(x.Content.ReadAsStringAsync().Result, ReadFileBody(requestFile))),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();
        }

        public static void StubRequestWithResponse(Mock<HttpMessageHandler> handlerMock, string urlEquals, string responseFile)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ReadFileBody(responseFile), Encoding.UTF8, "application/json")
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri.AbsoluteUri.EndsWith(urlEquals)),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();
        }

        public static void StubRequestWithResponse(Mock<HttpMessageHandler> handlerMock, string url, string requestFile, string responseFile)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ReadFileBody(responseFile), Encoding.UTF8, "application/json")
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Post && x.RequestUri.AbsoluteUri.EndsWith(url) && CompareJson(x.Content.ReadAsStringAsync().Result, ReadFileBody(requestFile))),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();
        }

        public static void StubSessionStatusWithState(Mock<HttpMessageHandler> handlerMock, string sessionId, string responseFile, RequestState state, string startState, string endState)
        {
            string urlEquals = "/session/" + sessionId;

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ReadFileBody(responseFile), Encoding.UTF8, "application/json")
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri.AbsoluteUri.EndsWith(urlEquals) && string.Equals(state.State, startState, System.StringComparison.OrdinalIgnoreCase)),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Callback(() => state.State = endState)
               .Verifiable();
        }

        public class RequestState
        {
            public string State { get; set; }
        }

        private static string ReadFileBody(string fileName)
        {
            return File.ReadAllText(Path.Combine("Resources", fileName));
        }

        private static bool CompareJson(string source, string destination)
        {
            JObject s1 = JObject.Parse(source);
            JObject s2 = JObject.Parse(destination);

            return JToken.DeepEquals(s1, s2);
        }
    }
}