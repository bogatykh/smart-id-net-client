using System;
using System.Net.Http;
using SK.SmartId.Rest;

namespace SK.SmartId.IntegrationTests
{
    /// <summary>
    /// Shared HTTP client for demo integration tests (Java <c>SmartIdRestIntegrationTest</c> setUp).
    /// </summary>
    public sealed class SmartIdDemoFixture : IDisposable
    {
        public const string DemoEndpoint = "https://sid.demo.sk.ee/smart-id-rp/v3/";

        public HttpClient HttpClient { get; }

        public ISmartIdConnector Connector { get; }

        public SmartIdDemoFixture()
        {
            HttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            Connector = new SmartIdRestConnector(DemoEndpoint, HttpClient);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
