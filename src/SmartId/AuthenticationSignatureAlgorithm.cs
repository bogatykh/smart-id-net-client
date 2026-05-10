/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
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

namespace SK.SmartId
{
    public enum AuthenticationSignatureAlgorithm
    {
        RSASSA_PSS
    }

    public static class AuthenticationSignatureAlgorithmExtensions
    {
        public static string GetAlgorithmName(this AuthenticationSignatureAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case AuthenticationSignatureAlgorithm.RSASSA_PSS:
                    return "rsassa-pss";
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }

        /// <summary>Java <c>AuthenticationSignatureAlgorithm.isSupported</c>.</summary>
        public static bool IsSupportedApiAlgorithmName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            foreach (AuthenticationSignatureAlgorithm a in Enum.GetValues(typeof(AuthenticationSignatureAlgorithm)))
            {
                if (string.Equals(a.GetAlgorithmName(), name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
