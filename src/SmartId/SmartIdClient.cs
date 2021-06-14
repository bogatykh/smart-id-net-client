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

using SK.SmartId.Rest;
using System;
using System.Net.Http;

namespace SK.SmartId
{
    public class SmartIdClient
    {
        private string hostUrl;
        private HttpClient configuredClient;
        private TimeSpan pollingSleepTimeout = TimeSpan.FromSeconds(1);
        private TimeSpan? sessionStatusResponseSocketOpenTime;
        private ISmartIdConnector connector;

        /// <summary>
        /// Gets an instance of the certificate request builder
        /// </summary>
        /// <returns>certificate request builder instance</returns>
        public CertificateRequestBuilder GetCertificate()
        {
            SessionStatusPoller sessionStatusPoller = CreateSessionStatusPoller(SmartIdConnector);
            CertificateRequestBuilder builder = new CertificateRequestBuilder(SmartIdConnector, sessionStatusPoller);
            builder.WithRelyingPartyUUID(RelyingPartyUUID);
            builder.WithRelyingPartyName(RelyingPartyName);
            return builder;
        }

        /// <summary>
        /// Gets an instance of the signature request builder
        /// </summary>
        /// <returns>signature request builder instance</returns>
        public SignatureRequestBuilder CreateSignature()
        {
            SessionStatusPoller sessionStatusPoller = CreateSessionStatusPoller(SmartIdConnector);
            SignatureRequestBuilder builder = new SignatureRequestBuilder(SmartIdConnector, sessionStatusPoller);
            builder.WithRelyingPartyUUID(RelyingPartyUUID);
            builder.WithRelyingPartyName(RelyingPartyName);
            return builder;
        }

        /**
         * Gets an instance of the authentication request builder
         *
         * @return authentication request builder instance
         */
        public AuthenticationRequestBuilder CreateAuthentication()
        {
            SessionStatusPoller sessionStatusPoller = CreateSessionStatusPoller(SmartIdConnector);
            AuthenticationRequestBuilder builder = new AuthenticationRequestBuilder(SmartIdConnector, sessionStatusPoller);
            builder.WithRelyingPartyUUID(RelyingPartyUUID);
            builder.WithRelyingPartyName(RelyingPartyName);
            return builder;
        }

        /// <summary>
        /// Gets or sets the UUID of the relying party
        /// <para>
        /// Can be set also on the builder level,
        /// but in that case it has to be set
        /// every time when building a new request.
        /// </para>
        /// </summary>
        public string RelyingPartyUUID { get; set; }

        /// <summary>
        /// Gets or sets the name of the relying party
        /// <para>
        /// Can be set also on the builder level,
        /// but in that case it has to be set
        /// every time when building a new request.
        /// </para>
        /// </summary>
        public string RelyingPartyName { get; set; }

        /// <summary>
        /// Sets the base URL of the Smart-ID backend environment
        /// <para>
        /// It defines the endpoint which the client communicates to.
        /// </para>
        /// </summary>
        /// <param name="hostUrl">base URL of the Smart-ID backend environment</param>
        public void SetHostUrl(String hostUrl)
        {
            this.hostUrl = hostUrl;
        }

        public void SetConfiguredClient(HttpClient configuredClient)
        {
            this.configuredClient = configuredClient;
        }

        /// <summary>
        /// Sets the timeout for each session status poll
        /// <para>
        /// Under the hood each operation(authentication, signing, choosing
        /// certificate) consists of 2 request steps:
        /// </para>
        /// <para>
        /// 1. Initiation request
        /// </para>
        /// <para>
        /// 2. Session status request
        /// </para>
        /// <para>
        /// Session status request is a long poll method, meaning
        /// the request method might not return until a timeout expires
        /// set by this parameter.
        /// </para>
        /// <para>
        /// Caller can tune the request parameters inside the bounds
        /// set by service operator.
        /// </para>
        /// <para>
        /// If not provided, a default is used.
        /// </para>
        /// </summary>
        /// <param name="time">time of each status poll's timeout</param>
        public void SetSessionStatusResponseSocketOpenTime(TimeSpan time)
        {
            sessionStatusResponseSocketOpenTime = time;
        }

        /// <summary>
        /// Sets the timeout/pause between each session status poll
        /// </summary>
        /// <param name="timeout">timeout value</param>
        public void SetPollingSleepTimeout(TimeSpan timeout)
        {
            pollingSleepTimeout = timeout;
        }

        private SessionStatusPoller CreateSessionStatusPoller(ISmartIdConnector connector)
        {
            connector.SetSessionStatusResponseSocketOpenTime(sessionStatusResponseSocketOpenTime);
            SessionStatusPoller sessionStatusPoller = new SessionStatusPoller(connector);
            sessionStatusPoller.SetPollingSleepTime(pollingSleepTimeout);
            return sessionStatusPoller;
        }

        public ISmartIdConnector SmartIdConnector
        {
            get
            {
                if (null == connector)
                {
                    // Fallback to REST connector when not initialised
                    SmartIdRestConnector connector = configuredClient != null ? new SmartIdRestConnector(hostUrl, configuredClient) : new SmartIdRestConnector(hostUrl);
                    connector.SetSessionStatusResponseSocketOpenTime(sessionStatusResponseSocketOpenTime);

                    SmartIdConnector = connector;
                }
                return connector;
            }
            set => connector = value;
        }
    }
}