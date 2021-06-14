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
            Assert.True(duration >= 1000L);
            Assert.True(duration <= 1500L);
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

            public Task<SessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken)
            {
                sessionIdUsed = sessionId;
                requestUsed = CreateSessionStatusRequest(sessionId);
                return Task.FromResult(responses[responseNumber++]);
            }

            public void SetSessionStatusResponseSocketOpenTime(TimeSpan? sessionStatusResponseSocketOpenTime)
            {
                this.sessionStatusResponseSocketOpenTime = sessionStatusResponseSocketOpenTime;
            }

            public Task<CertificateChoiceResponse> GetCertificateAsync(String documentNumber, CertificateRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<CertificateChoiceResponse>(null);
            }

            public Task<CertificateChoiceResponse> GetCertificateAsync(SemanticsIdentifier identifier,
                CertificateRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<CertificateChoiceResponse>(null);
            }

            public Task<SignatureSessionResponse> SignAsync(String documentNumber, SignatureSessionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<SignatureSessionResponse>(null);
            }

            public Task<SignatureSessionResponse> SignAsync(SemanticsIdentifier identifier,
                SignatureSessionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<SignatureSessionResponse>(null);
            }

            public Task<AuthenticationSessionResponse> AuthenticateAsync(String documentNumber, AuthenticationSessionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<AuthenticationSessionResponse>(null);
            }


            public Task<AuthenticationSessionResponse> AuthenticateAsync(SemanticsIdentifier identity,
                AuthenticationSessionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult<AuthenticationSessionResponse>(null);
            }

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