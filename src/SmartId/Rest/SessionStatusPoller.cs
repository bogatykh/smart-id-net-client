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
using SK.SmartId.Rest.Dao;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId.Rest
{
    public class SessionStatusPoller
    {
        private readonly ISmartIdConnector _connector;
        private TimeSpan pollingSleepTimeout = TimeSpan.FromSeconds(1);

        public SessionStatusPoller(ISmartIdConnector connector)
        {
            _connector = connector;
        }

        public async Task<SessionStatus> FetchFinalSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PollForFinalSessionStatusAsync(sessionId, cancellationToken);
            }
            catch (TaskCanceledException e)
            {
                throw new UnprocessableSmartIdResponseException("Failed to poll session status: " + e.Message, e);
            }
        }

        private async Task<SessionStatus> PollForFinalSessionStatusAsync(string sessionId, CancellationToken cancellationToken)
        {
            SessionStatus sessionStatus = null;
            while (sessionStatus == null || string.Equals("RUNNING", sessionStatus.State, StringComparison.OrdinalIgnoreCase))
            {
                sessionStatus = await PollSessionStatusAsync(sessionId, cancellationToken);
                if (sessionStatus != null && string.Equals("COMPLETE", sessionStatus.State, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                await Task.Delay(pollingSleepTimeout, cancellationToken);
            }
            return sessionStatus;
        }

        private async Task<SessionStatus> PollSessionStatusAsync(string sessionId, CancellationToken cancellationToken)
        {
            return await _connector.GetSessionStatusAsync(sessionId, cancellationToken);
        }

        public void SetPollingSleepTime(TimeSpan timeout)
        {
            pollingSleepTimeout = timeout;
        }
    }
}