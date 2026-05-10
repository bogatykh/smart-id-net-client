/*-
 * #%L
 * Smart ID sample Java client
 * #L%
 */

using SK.SmartId.Rest.Dao;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SK.SmartId.Rest
{
    public class SessionStatusPollerTest
    {
        private readonly SmartIdConnectorStub connector;
        private readonly SessionStatusPoller poller;

        public SessionStatusPollerTest()
        {
            connector = new SmartIdConnectorStub();
            poller = new SessionStatusPoller(connector);
            poller.SetPollingSleepTime(TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public async Task GetFirstCompleteResponse()
        {
            connector.responses.Add(CreateCompleteSessionStatus());
            SessionStatus status = await poller.FetchFinalSessionStatusAsync("97f5058e-e308-4c83-ac14-7712b0eb9d86");
            Assert.Equal("97f5058e-e308-4c83-ac14-7712b0eb9d86", connector.sessionIdUsed);
            Assert.Equal(1, connector.responseNumber);
            AssertCompleteStateReceived(status);
        }

        [Fact]
        public async Task PollAndGetThirdCompleteResponse()
        {
            connector.responses.Add(CreateRunningSessionStatus());
            connector.responses.Add(CreateRunningSessionStatus());
            connector.responses.Add(CreateCompleteSessionStatus());
            SessionStatus status = await poller.FetchFinalSessionStatusAsync("97f5058e-e308-4c83-ac14-7712b0eb9d86");
            Assert.Equal(3, connector.responseNumber);
            AssertCompleteStateReceived(status);
        }

        [Fact]
        public async Task SetPollingSleepTime()
        {
            poller.SetPollingSleepTime(TimeSpan.FromMilliseconds(200L));
            AddMultipleRunningSessionResponses(5);
            connector.responses.Add(CreateCompleteSessionStatus());
            double duration = await MeasurePollingDurationAsync();
            Assert.InRange(duration, 1000L, 2500L);
        }

        [Fact]
        public async Task SetResponseSocketOpenTime()
        {
            connector.SetSessionStatusResponseSocketOpenTime(TimeSpan.FromMinutes(2L));
            connector.responses.Add(CreateCompleteSessionStatus());
            SessionStatus status = await poller.FetchFinalSessionStatusAsync("97f5058e-e308-4c83-ac14-7712b0eb9d86");
            AssertCompleteStateReceived(status);
            Assert.True(connector.requestUsed.IsResponseSocketOpenTimeSet());
            Assert.Equal(TimeSpan.FromMinutes(2L), connector.requestUsed.ResponseSocketOpenTime);
        }

        [Fact]
        public async Task responseSocketOpenTimeShouldNotBeSetByDefault()
        {
            connector.responses.Add(CreateCompleteSessionStatus());
            SessionStatus status = await poller.FetchFinalSessionStatusAsync("97f5058e-e308-4c83-ac14-7712b0eb9d86");
            AssertCompleteStateReceived(status);
            Assert.False(connector.requestUsed.IsResponseSocketOpenTimeSet());
        }

        private async Task<double> MeasurePollingDurationAsync()
        {
            DateTime startTime = DateTime.UtcNow;
            SessionStatus status = await poller.FetchFinalSessionStatusAsync("97f5058e-e308-4c83-ac14-7712b0eb9d86");
            DateTime endTime = DateTime.UtcNow;
            AssertCompleteStateReceived(status);
            return (endTime - startTime).TotalMilliseconds;
        }

        private void AddMultipleRunningSessionResponses(int numberOfResponses)
        {
            for (int i = 0; i < numberOfResponses; i++)
                connector.responses.Add(CreateRunningSessionStatus());
        }

        private void AssertCompleteStateReceived(SessionStatus status)
        {
            Assert.NotNull(status);
            Assert.Equal("COMPLETE", status.State);
        }

        private SessionStatus CreateCompleteSessionStatus()
        {
            SessionStatus sessionStatus = new SessionStatus
            {
                State = "COMPLETE",
                Result = DummyData.createSessionEndResult()
            };
            return sessionStatus;
        }

        private SessionStatus CreateRunningSessionStatus()
        {
            SessionStatus status = new SessionStatus
            {
                State = "RUNNING"
            };
            return status;
        }

        private class SmartIdConnectorStub : ISmartIdConnector
        {
            public string sessionIdUsed;
            public SessionStatusRequest requestUsed;
            public List<SessionStatus> responses = new List<SessionStatus>();
            public int responseNumber = 0;
            private TimeSpan? sessionStatusResponseSocketOpenTime;

            public Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default)
            {
                sessionIdUsed = sessionId;
                requestUsed = CreateSessionStatusRequest(sessionId);
                return Task.FromResult(responses[responseNumber++]);
            }

            public void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime)
            {
                this.sessionStatusResponseSocketOpenTime = sessionStatusResponseSocketOpenTime;
            }

            public Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request, SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<DeviceLinkSessionResponse> InitDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request, string documentNumber, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<DeviceLinkSessionResponse> InitAnonymousDeviceLinkAuthenticationAsync(DeviceLinkAuthenticationSessionRequest request, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request, SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default) =>
                Task.FromResult<NotificationAuthenticationSessionResponse>(null);

            public Task<NotificationAuthenticationSessionResponse> InitNotificationAuthenticationAsync(NotificationAuthenticationSessionRequest request, string documentNumber, CancellationToken cancellationToken = default) =>
                Task.FromResult<NotificationAuthenticationSessionResponse>(null);

            public Task<DeviceLinkSessionResponse> InitDeviceLinkCertificateChoiceAsync(DeviceLinkCertificateChoiceSessionRequest request, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<LinkedSignatureSessionResponse> InitLinkedNotificationSignatureAsync(LinkedSignatureSessionRequest request, string documentNumber, CancellationToken cancellationToken = default) =>
                Task.FromResult<LinkedSignatureSessionResponse>(null);

            public Task<NotificationCertificateChoiceSessionResponse> InitNotificationCertificateChoiceAsync(NotificationCertificateChoiceSessionRequest request, SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default) =>
                Task.FromResult<NotificationCertificateChoiceSessionResponse>(null);

            public Task<CertificateResponse> GetCertificateByDocumentNumberAsync(string documentNumber, CertificateByDocumentNumberRequest request, CancellationToken cancellationToken = default) =>
                Task.FromResult<CertificateResponse>(null);

            public Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request, SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<DeviceLinkSessionResponse> InitDeviceLinkSignatureAsync(DeviceLinkSignatureSessionRequest request, string documentNumber, CancellationToken cancellationToken = default) =>
                Task.FromResult<DeviceLinkSessionResponse>(null);

            public Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request, SemanticsIdentifier semanticsIdentifier, CancellationToken cancellationToken = default) =>
                Task.FromResult<NotificationSignatureSessionResponse>(null);

            public Task<NotificationSignatureSessionResponse> InitNotificationSignatureAsync(NotificationSignatureSessionRequest request, string documentNumber, CancellationToken cancellationToken = default) =>
                Task.FromResult<NotificationSignatureSessionResponse>(null);

            private SessionStatusRequest CreateSessionStatusRequest(String sessionId)
            {
                SessionStatusRequest request = new SessionStatusRequest(sessionId);
                if (sessionStatusResponseSocketOpenTime != null && sessionStatusResponseSocketOpenTime.Value.TotalMilliseconds > 0)
                {
                    request.ResponseSocketOpenTime = sessionStatusResponseSocketOpenTime.Value;
                }
                return request;
            }
        }
    }
}
