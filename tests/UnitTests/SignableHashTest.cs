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

using System.Text;
using Xunit;

namespace SK.SmartId
{
    public class SignableHashTest
    {
        [Fact]
        public void calculateVerificationCodeWithSha256()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashType = HashType.SHA256;
            hashToSign.HashInBase64 = "jsflWgpkVcWOyICotnVn5lazcXdaIWvcvNOWTYPceYQ=";
            Assert.Equal("4240", hashToSign.CalculateVerificationCode());
        }

        [Fact]
        public void calculateVerificationCodeWithSha512()
        {
            SignableHash hashToSign = new SignableHash();
            hashToSign.HashType = HashType.SHA512;
            hashToSign.Hash = DigestCalculator.CalculateDigest(Encoding.UTF8.GetBytes("Hello World!"), HashType.SHA512);
            Assert.Equal("4664", hashToSign.CalculateVerificationCode());
        }
    }
}