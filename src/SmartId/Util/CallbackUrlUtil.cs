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
using SK.SmartId.Common;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;

namespace SK.SmartId.Util
{
    /// <summary>
    /// Callback URL query helpers (Java <c>CallbackUrlUtil</c>).
    /// </summary>
    public static class CallbackUrlUtil
    {
        public static CallbackUrl CreateCallbackUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new SmartIdClientException("Parameter for 'baseUrl' cannot be empty");
            }
            string urlToken = UrlSafeTokenGenerator.Random();
            string sep = baseUrl.IndexOf("?", StringComparison.Ordinal) >= 0 ? "&" : "?";
            string full = baseUrl + sep + "value=" + urlToken;
            return new CallbackUrl(new Uri(full), urlToken);
        }

        public static void ValidateSessionSecretDigest(string sessionSecretDigest, string sessionSecret)
        {
            if (string.IsNullOrEmpty(sessionSecretDigest))
            {
                throw new SmartIdClientException("Parameter for 'sessionSecretDigest' cannot be empty");
            }
            if (string.IsNullOrEmpty(sessionSecret))
            {
                throw new SmartIdClientException("Parameter for 'sessionSecret' cannot be empty");
            }
            string calculated = CalculateDigest(sessionSecret);
            if (!string.Equals(sessionSecretDigest, calculated, StringComparison.Ordinal))
            {
                throw new SessionSecretMismatchException(
                    "Session secret digest from callback does not match calculated session secret digest");
            }
        }

        private static string CalculateDigest(string sessionSecret)
        {
            try
            {
                byte[] decodedSessionSecret = Convert.FromBase64String(sessionSecret);
                byte[] sessionSecretDigest = DigestCalculator.CalculateDigest(decodedSessionSecret, SmartIdHashAlgorithm.SHA_256);
                return ToBase64UrlWithoutPadding(sessionSecretDigest);
            }
            catch (FormatException ex)
            {
                throw new SmartIdClientException("Parameter 'sessionSecret' is not Base64-encoded value", ex);
            }
        }

        private static string ToBase64UrlWithoutPadding(byte[] data)
        {
            string s = Convert.ToBase64String(data);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
