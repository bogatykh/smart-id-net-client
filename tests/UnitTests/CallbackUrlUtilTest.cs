/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;
using System.Text;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Util;
using Xunit;

namespace SK.SmartId
{
    public class CallbackUrlUtilTest
    {
        private const string SessionSecretDigest = "nKMc7gT3mvWuJtfXVFjCY2ehuvTs26f1Sgjk6g9oOr8";

        [Fact]
        public void CreateCallbackUrl_valueQueryParameterIsSameAsUrlToken()
        {
            var callbackUrl = CallbackUrlUtil.CreateCallbackUrl("https://example.com/callback");
            Assert.Equal("https://example.com/callback?value=" + callbackUrl.UrlToken, callbackUrl.InitialCallbackUri.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateCallbackUrl_inputBaseUrlIsEmpty_throwException(string baseUrl)
        {
            var ex = Assert.Throws<SmartIdClientException>(() => CallbackUrlUtil.CreateCallbackUrl(baseUrl));
            Assert.Equal("Parameter for 'baseUrl' cannot be empty", ex.Message);
        }

        [Fact]
        public void ValidateSessionSecretDigest_success()
        {
            const string sessionSecret = "fBo1/L1vM9xcSmZF7hvvooEj";
            CallbackUrlUtil.ValidateSessionSecretDigest(SessionSecretDigest, sessionSecret);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateSessionSecretDigest_sessionSecretDigestIsEmpty_throwException(string sessionSecretDigest)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                CallbackUrlUtil.ValidateSessionSecretDigest(sessionSecretDigest, ""));
            Assert.Equal("Parameter for 'sessionSecretDigest' cannot be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateSessionSecretDigest_sessionSecretIsEmpty_throwException(string sessionSecret)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                CallbackUrlUtil.ValidateSessionSecretDigest(SessionSecretDigest, sessionSecret));
            Assert.Equal("Parameter for 'sessionSecret' cannot be empty", ex.Message);
        }

        [Fact]
        public void ValidateSessionSecretDigest_sessionSecretValidationFails_throwException()
        {
            string sessionSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes("sessionSecret"));
            var ex = Assert.Throws<SessionSecretMismatchException>(() =>
                CallbackUrlUtil.ValidateSessionSecretDigest(SessionSecretDigest, sessionSecret));
            Assert.Equal("Session secret digest from callback does not match calculated session secret digest", ex.Message);
        }

        [Fact]
        public void ValidateSessionSecretDigest_sessionSecretIsNotBase64Encoded_throwException()
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                CallbackUrlUtil.ValidateSessionSecretDigest(SessionSecretDigest, "sessionSecret"));
            Assert.Equal("Parameter 'sessionSecret' is not Base64-encoded value", ex.Message);
        }
    }
}
