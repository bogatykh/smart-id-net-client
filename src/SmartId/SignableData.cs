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

namespace SK.SmartId
{
    /// <summary>
    /// This class can be used to contain the data
    /// to be signed when it is not yet in hashed format
    /// <para>
    /// <see cref="HashType"/> can be used
    /// to set the wanted hash tpye. SHA-512 is default.
    /// </para>
    /// <para>
    /// <see cref="CalculateHash"/> and
    /// <see cref="CalculateHashInBase64"/> methods
    /// are used to calculate the hash for signing request.
    /// </para>
    /// <para>
    /// <see cref="SignableHash"/> can be used
    /// instead when the data to be signed is already
    /// in hashed format.
    /// </para>
    /// </summary>
    public class SignableData
    {
        private readonly byte[] dataToSign;

        public SignableData(byte[] dataToSign)
        {
            this.dataToSign = (byte[])dataToSign.Clone();
        }

        public string CalculateHashInBase64()
        {
            byte[] digest = CalculateHash();
            return Convert.ToBase64String(digest);
        }

        public byte[] CalculateHash()
        {
            return DigestCalculator.CalculateDigest(dataToSign, HashType);
        }

        /// <summary>
        /// Calculates the verification code from the data
        /// <para>
        /// Verification code should be displayed on the web page or some sort of web service
        /// so the person signing through the Smart-ID mobile app can verify if the verification code
        /// displayed on the phone matches with the one shown on the web page.
        /// </para>
        /// </summary>
        /// <returns>the verification code</returns>
        public string CalculateVerificationCode()
        {
            byte[] digest = CalculateHash();
            return VerificationCodeCalculator.Calculate(digest);
        }

        public HashType HashType { get; set; } = HashType.SHA512;
    }
}