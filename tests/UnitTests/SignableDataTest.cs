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

using System;
using System.Text;
using Xunit;

namespace SK.SmartId
{
    public class SignableDataTest
    {
        public static readonly byte[] DATA_TO_SIGN = Encoding.UTF8.GetBytes("Hello World!");
        public static readonly string SHA512_HASH_IN_BASE64 = "hhhE1nBOhXP+w02WfiC8/vPUJM9IvgTm3AjyvVjHKXQzcQFerYkcw88cnTS0kmS1EHUbH/nlN5N7xGtdb/TsyA==";
        public static readonly string SHA384_HASH_IN_BASE64 = "v9dsDrvQBv7lg0EFR8GIewKSvnbVgtlsJC0qeScj4/1v0GH51c/RO4+WE1jmrbpK";
        public static readonly string SHA256_HASH_IN_BASE64 = "f4OxZX/x/FO5LcGBSKHWXfwtSx+j1ncoSt3SABJtkGk=";

        [Fact]
        public void signableData_withDefaultHashType_sha512()
        {
            SignableData signableData = new SignableData(DATA_TO_SIGN);
            Assert.Equal("SHA512", signableData.HashType.HashTypeName);
            Assert.Equal(SHA512_HASH_IN_BASE64, signableData.CalculateHashInBase64());
            Assert.Equal(Convert.FromBase64String(SHA512_HASH_IN_BASE64), signableData.CalculateHash());
            Assert.Equal("4664", signableData.CalculateVerificationCode());
        }

        [Fact]
        public void signableData_with_sha256()
        {
            SignableData signableData = new SignableData(DATA_TO_SIGN);
            signableData.HashType = HashType.SHA256;
            Assert.Equal("SHA256", signableData.HashType.HashTypeName);
            Assert.Equal(SHA256_HASH_IN_BASE64, signableData.CalculateHashInBase64());
            Assert.Equal(Convert.FromBase64String(SHA256_HASH_IN_BASE64), signableData.CalculateHash());
            Assert.Equal("7712", signableData.CalculateVerificationCode());
        }

        [Fact]
        public void signableData_with_sha384()
        {
            SignableData signableData = new SignableData(DATA_TO_SIGN);
            signableData.HashType = HashType.SHA384;
            Assert.Equal("SHA384", signableData.HashType.HashTypeName);
            Assert.Equal(SHA384_HASH_IN_BASE64, signableData.CalculateHashInBase64());
            Assert.Equal(Convert.FromBase64String(SHA384_HASH_IN_BASE64), signableData.CalculateHash());
            Assert.Equal("3486", signableData.CalculateVerificationCode());
        }
    }
}