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
    /// This class can be used to contain the hash
    /// to be signed
    /// <para>
    /// <see cref="Hash"/> can be used
    /// to set the hash.
    /// <see cref="Hash"/> can be used
    /// to set the hash tpye.
    /// </para>
    /// <see cref="SignableData"/>
    /// can be used
    /// instead when the data to be signed is not already
    /// in hashed format.
    /// </summary>
    public class SignableHash
    {
        public byte[] Hash { private get; set; }

        public string HashInBase64
        {
            get
            {
                return Convert.ToBase64String(Hash);
            }
            set
            {
                Hash = Convert.FromBase64String(value);
            }
        }

        public HashType HashType { get; set; }

        public bool AreFieldsFilled()
        {
            return HashType != null && Hash != null && Hash.Length > 0;
        }

        /// <summary>
        /// Calculates the verification code from the hash
        /// <para>
        /// Verification code should be displayed on the web page or some sort of web service
        /// so the person signing through the Smart-ID mobile app can verify if if the verification code
        /// displayed on the phone matches with the one shown on the web page.
        /// </para>
        /// </summary>
        /// <returns>the verification code</returns>
        public string CalculateVerificationCode()
        {
            return VerificationCodeCalculator.Calculate(Hash);
        }
    }
}