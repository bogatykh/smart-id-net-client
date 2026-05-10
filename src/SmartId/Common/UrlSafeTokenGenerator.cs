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
using System.Security.Cryptography;
using SK.SmartId.Exceptions.Permanent;

namespace SK.SmartId.Common
{
    /// <summary>
    /// URL-safe random tokens (Java <c>UrlSafeTokenGenerator</c>).
    /// </summary>
    public static class UrlSafeTokenGenerator
    {
        private const int MinNrOfCharacters = 22;
        private const int MaxNrOfCharacters = 86;

        public static string Random() => RandomBetween(MinNrOfCharacters, MaxNrOfCharacters);

        public static string OfLength(int length) => RandomBetween(length, length);

        public static string RandomBetween(int minLen, int maxLen)
        {
            if (minLen < MinNrOfCharacters || maxLen > MaxNrOfCharacters || minLen > maxLen)
            {
                throw new SmartIdClientException("Length must be between 22 and 86 chars");
            }
            using (var rng = RandomNumberGenerator.Create())
            {
                int targetLen = GetRandomLengthInclusive(rng, minLen, maxLen);
                var bytes = new byte[64];
                rng.GetBytes(bytes);
                string random = ToBase64UrlWithoutPadding(bytes);
                return random.Substring(0, targetLen);
            }
        }

        private static int GetRandomLengthInclusive(RandomNumberGenerator rng, int minInclusive, int maxInclusive)
        {
            int range = maxInclusive - minInclusive + 1;
            var four = new byte[4];
            rng.GetBytes(four);
            uint r = BitConverter.ToUInt32(four, 0);
            return (int)(r % (uint)range) + minInclusive;
        }

        private static string ToBase64UrlWithoutPadding(byte[] data)
        {
            string s = Convert.ToBase64String(data);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
