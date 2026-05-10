/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
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

using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SK.SmartId
{
    public class SmartIdClient
    {
        private string hostUrl;
        private HttpClient configuredClient;
        private TimeSpan pollingSleepTimeout = TimeSpan.FromSeconds(1);
        private TimeSpan? sessionStatusResponseSocketOpenTime;
        private ISmartIdConnector connector;
        private SessionStatusPoller sessionStatusPoller;

        public string RelyingPartyUUID { get; set; }

        public string RelyingPartyName { get; set; }

        public DeviceLinkCertificateChoiceSessionRequestBuilder CreateDeviceLinkCertificateRequest()
        {
            return new DeviceLinkCertificateChoiceSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public LinkedNotificationSignatureSessionRequestBuilder CreateLinkedNotificationSignature()
        {
            return new LinkedNotificationSignatureSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public NotificationCertificateChoiceSessionRequestBuilder CreateNotificationCertificateChoice()
        {
            return new NotificationCertificateChoiceSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public DeviceLinkAuthenticationSessionRequestBuilder CreateDeviceLinkAuthentication()
        {
            return new DeviceLinkAuthenticationSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public NotificationAuthenticationSessionRequestBuilder CreateNotificationAuthentication()
        {
            return new NotificationAuthenticationSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public DeviceLinkSignatureSessionRequestBuilder CreateDeviceLinkSignature()
        {
            return new DeviceLinkSignatureSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public CertificateByDocumentNumberRequestBuilder CreateCertificateByDocumentNumber()
        {
            return new CertificateByDocumentNumberRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public NotificationSignatureSessionRequestBuilder CreateNotificationSignature()
        {
            return new NotificationSignatureSessionRequestBuilder(SmartIdConnector)
                .WithRelyingPartyUUID(RelyingPartyUUID)
                .WithRelyingPartyName(RelyingPartyName);
        }

        public DeviceLinkBuilder CreateDynamicContent()
        {
            return new DeviceLinkBuilder().WithRelyingPartyName(RelyingPartyName);
        }

        public SessionStatusPoller GetSessionStatusPoller()
        {
            if (sessionStatusPoller == null)
            {
                sessionStatusPoller = new SessionStatusPoller(SmartIdConnector);
                sessionStatusPoller.SetPollingSleepTime(pollingSleepTimeout);
            }
            return sessionStatusPoller;
        }

        public void SetRelyingPartyUUID(string relyingPartyUUID)
        {
            RelyingPartyUUID = relyingPartyUUID;
        }

        public string GetRelyingPartyUUID() => RelyingPartyUUID;

        public void SetRelyingPartyName(string relyingPartyName)
        {
            RelyingPartyName = relyingPartyName;
        }

        public string GetRelyingPartyName() => RelyingPartyName;

        public void SetHostUrl(string hostUrl)
        {
            this.hostUrl = hostUrl;
        }

        public void SetConfiguredClient(HttpClient configuredClient)
        {
            this.configuredClient = configuredClient;
        }

        public void SetSessionStatusResponseSocketOpenTime(TimeSpan time)
        {
            sessionStatusResponseSocketOpenTime = time;
            connector?.SetSessionStatusResponseSocketOpenTime(sessionStatusResponseSocketOpenTime);
        }

        public void SetPollingSleepTimeout(TimeSpan timeout)
        {
            pollingSleepTimeout = timeout;
            sessionStatusPoller?.SetPollingSleepTime(pollingSleepTimeout);
        }

        public ISmartIdConnector SmartIdConnector
        {
            get
            {
                if (connector == null)
                {
                    var rest = configuredClient != null
                        ? new SmartIdRestConnector(hostUrl, configuredClient)
                        : new SmartIdRestConnector(hostUrl);
                    rest.SetSessionStatusResponseSocketOpenTime(sessionStatusResponseSocketOpenTime);
                    connector = rest;
                }
                return connector;
            }
            set
            {
                connector = value;
                sessionStatusPoller = null;
            }
        }
    }
}
