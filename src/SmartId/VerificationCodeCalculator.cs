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
    public static class VerificationCodeCalculator
    {
        /// <summary>
        /// The Verification Code(VC) is computed as:
        /// <b>integer(SHA256(hash)[−2:−1]) mod 10000</b>
        /// where we take SHA256 result, extract 2 rightmost bytes from it,
        /// interpret them as a big-endian unsigned integer and take the last 4 digits in decimal for display.
        /// SHA256 is always used here, no matter what was the algorithm used to calculate hash.
        /// </summary>
        /// <param name="documentHash">hash used to calculate verification code.</param>
        /// <returns>verification code.</returns>
        public static string Calculate(byte[] documentHash)
        {
            byte[] digest = DigestCalculator.CalculateDigest(documentHash, HashType.SHA256);

            byte[] bytes = new byte[2];
            Array.Copy(digest, digest.Length - 2, bytes, 0, 2);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            ushort twoRightmostBytes = BitConverter.ToUInt16(bytes, 0);

            string code = Convert.ToString(twoRightmostBytes);
            string paddedCode = "0000" + code;
            return paddedCode.Substring(paddedCode.Length - 4);
        }
    }
}