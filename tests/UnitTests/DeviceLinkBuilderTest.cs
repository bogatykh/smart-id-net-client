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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SK.SmartId.Exceptions.Permanent;
using Xunit;

namespace SK.SmartId
{
    public class DeviceLinkBuilderTest
    {
        private static readonly string SessionSecret =
            Convert.ToBase64String(Encoding.UTF8.GetBytes("sessionSecret"));

        private const string DemoSchemaName = "smart-id-demo";
        private const string DeviceLinkBase = "https://smart-id.com/device-link/";
        private const string DeviceLinkHost = "smart-id.com";
        private const string SessionToken = "token123";
        private const string Language = "eng";
        private const string VersionInvalid = "0.9";
        private const long ElapsedSeconds = 1L;
        private const string CallbackUrl = "https://callback.url";
        private const string RelyingPartyName = "DEMO";
        private const string Base64Digest = "dGVzdC1kaWdlc3Q=";
        private const string BrokeredRp = "QlJP";
        private const string Base64Interactions = "SW50ZXJhY3Rpb25z";
        private static readonly Regex AuthCodePattern = new Regex("^[A-Za-z0-9_-]{43}$", RegexOptions.Compiled);

        public static IEnumerable<object[]> AllDeviceLinkTypes()
        {
            yield return new object[] { DeviceLinkType.QR_CODE };
            yield return new object[] { DeviceLinkType.WEB_2_APP };
            yield return new object[] { DeviceLinkType.APP_2_APP };
        }

        [Theory]
        [MemberData(nameof(AllDeviceLinkTypes))]
        public void CreateUnprotectedUri_validInputs_buildsUri(DeviceLinkType deviceLinkType)
        {
            var uri = new DeviceLinkBuilder()
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithDeviceLinkType(deviceLinkType)
                .WithBrokeredRpName(BrokeredRp)
                .WithInteractions(Base64Interactions)
                .WithLang(Language)
                .WithElapsedSeconds(deviceLinkType == DeviceLinkType.QR_CODE ? ElapsedSeconds : (long?)null)
                .CreateUnprotectedUri();

            Assert.Equal(DeviceLinkHost, uri.Host);
        }

        [Fact]
        public void CreateUnprotectedUri_invalidVersion_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithVersion(VersionInvalid)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Only version 1.0 is allowed", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateUnprotectedUri_missingDeviceLinkBase_throws(string baseUrl)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(baseUrl)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'deviceLinkBase' cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateUnprotectedUri_missingVersion_throws(string ver)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithVersion(ver)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'version' cannot be empty", ex.Message);
        }

        [Fact]
        public void CreateUnprotectedUri_missingDeviceLinkType_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(null)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'deviceLinkType' must be set", ex.Message);
        }

        [Fact]
        public void CreateUnprotectedUri_missingSessionType_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'sessionType' must be set", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateUnprotectedUri_missingSessionToken_throws(string token)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(token)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'sessionToken' cannot be empty", ex.Message);
        }

        [Fact]
        public void CreateUnprotectedUri_missingElapsedSecondsForQrCode_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'elapsedSeconds' must be set when 'deviceLinkType' is QR_CODE", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateUnprotectedUri_missingLang_throws(string lang)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(lang)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'lang' must be set", ex.Message);
        }

        [Fact]
        public void CreateUnprotectedUri_elapsedSecondsSetForNonQrCode_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.APP_2_APP)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .CreateUnprotectedUri());
            Assert.Equal("Parameter 'elapsedSeconds' should only be used when 'deviceLinkType' is QR_CODE", ex.Message);
        }

        public static IEnumerable<object[]> AllSessionTypes()
        {
            yield return new object[] { SessionType.AUTHENTICATION };
            yield return new object[] { SessionType.SIGNATURE };
            yield return new object[] { SessionType.CERTIFICATE_CHOICE };
        }

        [Theory]
        [MemberData(nameof(AllSessionTypes))]
        public void BuildDeviceLink_producesAuthCodePattern(SessionType sessionType)
        {
            var builder = new DeviceLinkBuilder()
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(sessionType)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithBrokeredRpName(BrokeredRp)
                .WithLang(Language)
                .WithElapsedSeconds(1L)
                .WithRelyingPartyName(RelyingPartyName);

            if (sessionType != SessionType.CERTIFICATE_CHOICE)
            {
                builder.WithDigest(Base64Digest)
                    .WithInteractions(Base64Interactions);
            }

            var uri = builder.BuildDeviceLink(SessionSecret);
            var parameters = ToQueryParamsMap(uri);
            Assert.Matches(AuthCodePattern, parameters["authCode"]);
        }

        [Fact]
        public void BuildDeviceLink_withCustomSchemeName()
        {
            var uri = new DeviceLinkBuilder()
                .WithSchemeName(DemoSchemaName)
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithLang(Language)
                .WithElapsedSeconds(1L)
                .WithDigest(Base64Digest)
                .WithRelyingPartyName(RelyingPartyName)
                .WithInteractions(Base64Interactions)
                .BuildDeviceLink(SessionSecret);

            var authCode = ToQueryParamsMap(uri)["authCode"];
            Assert.Matches(AuthCodePattern, authCode);
        }

        [Fact]
        public void BuildDeviceLink_sameDeviceFlowWithCallback_ok()
        {
            var uri = new DeviceLinkBuilder()
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithDeviceLinkType(DeviceLinkType.APP_2_APP)
                .WithBrokeredRpName(BrokeredRp)
                .WithInteractions(Base64Interactions)
                .WithLang(Language)
                .WithInitialCallbackUrl(CallbackUrl)
                .WithDigest(Base64Digest)
                .WithRelyingPartyName(RelyingPartyName)
                .BuildDeviceLink(SessionSecret);

            Assert.Matches(AuthCodePattern, ToQueryParamsMap(uri)["authCode"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_missingSchemeName_throws(string scheme)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithSchemeName(scheme)
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithLang(Language)
                    .WithElapsedSeconds(ElapsedSeconds)
                    .WithDigest(Base64Digest)
                    .WithRelyingPartyName(RelyingPartyName)
                    .WithInteractions(Base64Interactions)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'schemeName' cannot be empty", ex.Message);
        }

        [Fact]
        public void BuildDeviceLink_missingRelyingPartyName_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithDigest(Base64Digest)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'relyingPartyName' cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_missingDigestForAuthentication_throws(string digest)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithDigest(digest)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'digest' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_missingDigestForSignature_throws(string digest)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.SIGNATURE)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithDigest(digest)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'digest' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE", ex.Message);
        }

        [Fact]
        public void BuildDeviceLink_certificateChoiceAndDigestIsSet_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.CERTIFICATE_CHOICE)
                    .WithDigest(Base64Digest)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'digest' must be empty when 'sessionType' is CERTIFICATE_CHOICE", ex.Message);
        }

        [Fact]
        public void BuildDeviceLink_qrCodeWithCallback_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithDigest(Base64Digest)
                    .WithRelyingPartyName(RelyingPartyName)
                    .WithInitialCallbackUrl(CallbackUrl)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'initialCallbackUrl' must be empty when 'deviceLinkType' is QR_CODE", ex.Message);
        }

        [Theory]
        [InlineData(DeviceLinkType.APP_2_APP)]
        [InlineData(DeviceLinkType.WEB_2_APP)]
        public void BuildDeviceLink_sameDeviceFlowWithoutCallback_throws(DeviceLinkType deviceLinkType)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(deviceLinkType)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithDigest(Base64Digest)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'initialCallbackUrl' must be provided when 'deviceLinkType' is APP_2_APP or WEB_2_APP", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_interactionsMissingForAuthentication_throws(string interactions)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.AUTHENTICATION)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithDigest(Base64Digest)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'interactions' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_interactionsMissingForSignature_throws(string interactions)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.SIGNATURE)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithDigest(Base64Digest)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'interactions' must be set when 'sessionType' is AUTHENTICATION or SIGNATURE", ex.Message);
        }

        [Fact]
        public void BuildDeviceLink_interactionsSetForCertificateChoice_throws()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                new DeviceLinkBuilder()
                    .WithDeviceLinkBase(DeviceLinkBase)
                    .WithSessionToken(SessionToken)
                    .WithSessionType(SessionType.CERTIFICATE_CHOICE)
                    .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                    .WithBrokeredRpName(BrokeredRp)
                    .WithInteractions(Base64Interactions)
                    .WithLang(Language)
                    .WithElapsedSeconds(1L)
                    .WithRelyingPartyName(RelyingPartyName)
                    .BuildDeviceLink(SessionSecret));
            Assert.Equal("Parameter 'interactions' must be empty when 'sessionType' is CERTIFICATE_CHOICE", ex.Message);
        }

        [Fact]
        public void BuildDeviceLink_invalidBase64Key_throwsWithInner()
        {
            var builder = new DeviceLinkBuilder()
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithBrokeredRpName(BrokeredRp)
                .WithInteractions(Base64Interactions)
                .WithLang(Language)
                .WithElapsedSeconds(1L)
                .WithDigest(Base64Digest)
                .WithRelyingPartyName(RelyingPartyName);

            var ex = Assert.Throws<SmartIdClientException>(() => builder.BuildDeviceLink("!!!invalidBase64==="));
            Assert.Equal("Failed to calculate authCode", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void BuildDeviceLink_sessionSecretEmpty_throws(string sessionSecret)
        {
            var builder = new DeviceLinkBuilder()
                .WithDeviceLinkBase(DeviceLinkBase)
                .WithSessionToken(SessionToken)
                .WithSessionType(SessionType.AUTHENTICATION)
                .WithDeviceLinkType(DeviceLinkType.QR_CODE)
                .WithBrokeredRpName(BrokeredRp)
                .WithInteractions(Base64Interactions)
                .WithLang(Language)
                .WithElapsedSeconds(1L)
                .WithDigest(Base64Digest)
                .WithRelyingPartyName(RelyingPartyName);

            var ex = Assert.Throws<SmartIdClientException>(() => builder.BuildDeviceLink(sessionSecret));
            Assert.Equal("Parameter 'sessionSecret' cannot be empty", ex.Message);
        }

        private static Dictionary<string, string> ToQueryParamsMap(Uri uri)
        {
            return uri.Query.TrimStart('?').Split('&')
                .Select(s => s.Split(new[] { '=' }, 2))
                .ToDictionary(s => s[0], s => s.Length > 1 ? Uri.UnescapeDataString(s[1]) : "");
        }
    }
}
