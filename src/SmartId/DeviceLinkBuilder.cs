/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2024 SK ID Solutions AS
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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Util;

namespace SK.SmartId
{
    /// <summary>
    /// Device link flow type (Java <c>DeviceLinkType</c>).
    /// </summary>
    public enum DeviceLinkType
    {
        QR_CODE,
        WEB_2_APP,
        APP_2_APP
    }

    /// <summary>
    /// Session type for device links (Java <c>SessionType</c>).
    /// </summary>
    public enum SessionType
    {
        AUTHENTICATION,
        SIGNATURE,
        CERTIFICATE_CHOICE
    }

    internal static class DeviceLinkEnums
    {
        public static string GetApiValue(this DeviceLinkType type)
        {
            switch (type)
            {
                case DeviceLinkType.QR_CODE:
                    return "QR";
                case DeviceLinkType.WEB_2_APP:
                    return "Web2App";
                case DeviceLinkType.APP_2_APP:
                    return "App2App";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static string GetApiValue(this SessionType type)
        {
            switch (type)
            {
                case SessionType.AUTHENTICATION:
                    return "auth";
                case SessionType.SIGNATURE:
                    return "sign";
                case SessionType.CERTIFICATE_CHOICE:
                    return "cert";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    /// <summary>
    /// Builds Smart-ID device-link URIs (Java <c>DeviceLinkBuilder</c>).
    /// </summary>
    public class DeviceLinkBuilder
    {
        private const string AllowedVersion = "1.0";

        private string schemeName = "smart-id";
        private string deviceLinkBase;
        private string version = AllowedVersion;
        private DeviceLinkType? deviceLinkType;
        private SessionType? sessionType;
        private string sessionToken;
        private long? elapsedSeconds;
        private string lang;

        private string digest;
        private string relyingPartyNameBase64;
        private string brokeredRpNameBase64;
        private string interactions;
        private string initialCallbackUrl;

        public DeviceLinkBuilder WithSchemeName(string schemeName)
        {
            this.schemeName = schemeName;
            return this;
        }

        public DeviceLinkBuilder WithDeviceLinkBase(string deviceLinkBase)
        {
            this.deviceLinkBase = deviceLinkBase;
            return this;
        }

        public DeviceLinkBuilder WithVersion(string version)
        {
            this.version = version;
            return this;
        }

        public DeviceLinkBuilder WithDeviceLinkType(DeviceLinkType? deviceLinkType)
        {
            this.deviceLinkType = deviceLinkType;
            return this;
        }

        public DeviceLinkBuilder WithSessionType(SessionType? sessionType)
        {
            this.sessionType = sessionType;
            return this;
        }

        public DeviceLinkBuilder WithSessionToken(string sessionToken)
        {
            this.sessionToken = sessionToken;
            return this;
        }

        public DeviceLinkBuilder WithElapsedSeconds(long? elapsedSeconds)
        {
            this.elapsedSeconds = elapsedSeconds;
            return this;
        }

        public DeviceLinkBuilder WithLang(string lang)
        {
            this.lang = lang;
            return this;
        }

        public DeviceLinkBuilder WithDigest(string digest)
        {
            this.digest = digest;
            return this;
        }

        public DeviceLinkBuilder WithRelyingPartyName(string relyingPartyName)
        {
            relyingPartyNameBase64 = relyingPartyName == null
                ? null
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(relyingPartyName));
            return this;
        }

        public DeviceLinkBuilder WithBrokeredRpName(string brokeredRpName)
        {
            brokeredRpNameBase64 = brokeredRpName == null
                ? null
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(brokeredRpName));
            return this;
        }

        public DeviceLinkBuilder WithInteractions(string interactions)
        {
            this.interactions = interactions;
            return this;
        }

        public DeviceLinkBuilder WithInitialCallbackUrl(string initialCallbackUrl)
        {
            this.initialCallbackUrl = initialCallbackUrl;
            return this;
        }

        public Uri CreateUnprotectedUri()
        {
            ValidateInputParameters();
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("deviceLinkType", deviceLinkType.Value.GetApiValue())
            };
            AddElapsedSecondsIfQrCode(queryParams);
            queryParams.Add(new KeyValuePair<string, string>("sessionToken", sessionToken));
            queryParams.Add(new KeyValuePair<string, string>("sessionType", sessionType.Value.GetApiValue()));
            queryParams.Add(new KeyValuePair<string, string>("version", version));
            queryParams.Add(new KeyValuePair<string, string>("lang", lang));
            return new Uri(AppendQueryString(deviceLinkBase, queryParams));
        }

        public Uri BuildDeviceLink(string sessionSecret)
        {
            Uri unprotectedUri = CreateUnprotectedUri();
            string authCode = GenerateAuthCode(unprotectedUri.AbsoluteUri, sessionSecret);
            return new Uri(AppendQueryString(unprotectedUri.AbsoluteUri,
                new[] { new KeyValuePair<string, string>("authCode", authCode) }));
        }

        private void AddElapsedSecondsIfQrCode(List<KeyValuePair<string, string>> queryParams)
        {
            if (elapsedSeconds.HasValue)
            {
                if (deviceLinkType != DeviceLinkType.QR_CODE)
                {
                    throw new SmartIdClientException("Parameter 'elapsedSeconds' should only be used when 'deviceLinkType' is QR_CODE");
                }
                queryParams.Add(new KeyValuePair<string, string>("elapsedSeconds", elapsedSeconds.Value.ToString()));
            }
        }

        private string GenerateAuthCode(string unprotectedLink, string sessionSecret)
        {
            if (string.IsNullOrEmpty(sessionSecret))
            {
                throw new SmartIdClientException("Parameter 'sessionSecret' cannot be empty");
            }
            ValidateAuthCodeParams();
            return CalculateAuthCode(BuildPayload(unprotectedLink), sessionSecret);
        }

        private string BuildPayload(string unprotectedLink)
        {
            return string.Join("|",
                schemeName,
                GetSignatureProtocolForSession(),
                StringUtil.OrEmpty(digest),
                relyingPartyNameBase64,
                StringUtil.OrEmpty(brokeredRpNameBase64),
                StringUtil.OrEmpty(interactions),
                StringUtil.OrEmpty(initialCallbackUrl),
                unprotectedLink);
        }

        private string GetSignatureProtocolForSession()
        {
            switch (sessionType)
            {
                case SessionType.AUTHENTICATION:
                    return SignatureProtocol.ACSP_V2.ToString();
                case SessionType.SIGNATURE:
                    return SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString();
                case SessionType.CERTIFICATE_CHOICE:
                    return "";
                default:
                    return "";
            }
        }

        private static string CalculateAuthCode(string data, string base64Key)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(base64Key);
                using (var mac = new HMACSHA256(keyBytes))
                {
                    byte[] hmac = mac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return ToBase64UrlWithoutPadding(hmac);
                }
            }
            catch (Exception ex)
            {
                throw new SmartIdClientException("Failed to calculate authCode", ex);
            }
        }

        private static string ToBase64UrlWithoutPadding(byte[] data)
        {
            string s = Convert.ToBase64String(data);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string AppendQueryString(string baseUrl, IReadOnlyList<KeyValuePair<string, string>> queryParams)
        {
            var sb = new StringBuilder();
            sb.Append(baseUrl);
            string sep = baseUrl != null && baseUrl.IndexOf('?') >= 0 ? "&" : "?";
            for (int i = 0; i < queryParams.Count; i++)
            {
                sb.Append(sep);
                sep = "&";
                sb.Append(Uri.EscapeDataString(queryParams[i].Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(queryParams[i].Value));
            }
            return sb.ToString();
        }

        private void ValidateInputParameters()
        {
            if (string.IsNullOrEmpty(deviceLinkBase))
            {
                throw new SmartIdClientException("Parameter 'deviceLinkBase' cannot be empty");
            }
            if (string.IsNullOrEmpty(version))
            {
                throw new SmartIdClientException("Parameter 'version' cannot be empty");
            }
            if (version != AllowedVersion)
            {
                throw new SmartIdClientException("Only version 1.0 is allowed");
            }
            if (!deviceLinkType.HasValue)
            {
                throw new SmartIdClientException("Parameter 'deviceLinkType' must be set");
            }
            if (!sessionType.HasValue)
            {
                throw new SmartIdClientException("Parameter 'sessionType' must be set");
            }
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new SmartIdClientException("Parameter 'sessionToken' cannot be empty");
            }
            if (deviceLinkType == DeviceLinkType.QR_CODE && !elapsedSeconds.HasValue)
            {
                throw new SmartIdClientException("Parameter 'elapsedSeconds' must be set when 'deviceLinkType' is QR_CODE");
            }
            if (string.IsNullOrEmpty(lang))
            {
                throw new SmartIdClientException("Parameter 'lang' must be set");
            }
        }

        private void ValidateAuthCodeParams()
        {
            if (string.IsNullOrEmpty(schemeName))
            {
                throw new SmartIdClientException("Parameter 'schemeName' cannot be empty");
            }
            if (string.IsNullOrEmpty(relyingPartyNameBase64))
            {
                throw new SmartIdClientException("Parameter 'relyingPartyName' cannot be empty");
            }

            bool hasCallback = !string.IsNullOrEmpty(initialCallbackUrl);
            if (deviceLinkType == DeviceLinkType.QR_CODE && hasCallback)
            {
                throw new SmartIdClientException("Parameter 'initialCallbackUrl' must be empty when 'deviceLinkType' is QR_CODE");
            }
            if ((deviceLinkType == DeviceLinkType.APP_2_APP || deviceLinkType == DeviceLinkType.WEB_2_APP) && !hasCallback)
            {
                throw new SmartIdClientException("Parameter 'initialCallbackUrl' must be provided when 'deviceLinkType' is APP_2_APP or WEB_2_APP");
            }
            if (sessionType != SessionType.CERTIFICATE_CHOICE)
            {
                if (string.IsNullOrEmpty(digest))
                {
                    throw new SmartIdClientException("Parameter 'digest' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE");
                }
                if (string.IsNullOrEmpty(interactions))
                {
                    throw new SmartIdClientException("Parameter 'interactions' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE");
                }
            }
            if (sessionType == SessionType.CERTIFICATE_CHOICE)
            {
                if (!string.IsNullOrEmpty(digest))
                {
                    throw new SmartIdClientException("Parameter 'digest' must be empty when 'sessionType' is CERTIFICATE_CHOICE");
                }
                if (!string.IsNullOrEmpty(interactions))
                {
                    throw new SmartIdClientException("Parameter 'interactions' must be empty when 'sessionType' is CERTIFICATE_CHOICE");
                }
            }
        }
    }
}
