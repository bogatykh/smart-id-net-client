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
using SK.SmartId.Exceptions.Permanent;

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
    public class SignableHash : IDigestInput
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

        /// <summary>
        /// Algorithm that produced <see cref="Hash"/> (Java <c>HashAlgorithm</c>).
        /// </summary>
        public SmartIdHashAlgorithm HashAlgorithm { get; set; } = SmartIdHashAlgorithm.SHA_512;

        /// <summary>
        /// Legacy SHA-2 family selector; maps to <see cref="HashAlgorithm"/>.
        /// </summary>
        public HashType HashType
        {
            get => HashTypeConversions.ToHashType(HashAlgorithm);
            set => HashAlgorithm = HashTypeConversions.FromHashType(value);
        }

        public bool AreFieldsFilled()
        {
            return Hash != null && Hash.Length > 0;
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

        public string GetDigestInBase64() => HashInBase64;

        public SmartIdHashAlgorithm GetHashAlgorithm() => HashAlgorithm;

        /// <summary>
        /// Validates hash and algorithm like the Java record constructor.
        /// </summary>
        public void Validate()
        {
            if (Hash == null || Hash.Length == 0)
            {
                throw new SmartIdRequestSetupException("Parameter 'hash' cannot be empty");
            }
            if (!Enum.IsDefined(typeof(SmartIdHashAlgorithm), HashAlgorithm))
            {
                throw new SmartIdRequestSetupException("Parameter 'hashAlgorithm' must be set");
            }
        }
    }
}
