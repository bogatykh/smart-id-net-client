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

namespace SK.SmartId
{
    /// <summary>
    /// Class containing the hash and its hash type used for authentication
    /// </summary>
    public class AuthenticationHash : SignableHash
    {
        /// <summary>
        /// Creates <see cref="AuthenticationHash"/>
        /// instance
        /// containing a randomly generated hash
        /// of the chosen hash type
        /// </summary>
        /// <param name="hashType">hashType hash type of the randomly generated hash</param>
        /// <returns>Authentication hash</returns>
        public static AuthenticationHash GenerateRandomHash(HashType hashType)
        {
            byte[] generatedDigest = DigestCalculator.CalculateDigest(GetRandomBytes(), hashType);
            AuthenticationHash authenticationHash = new AuthenticationHash
            {
                Hash = generatedDigest,
                HashType = hashType
            };
            return authenticationHash;
        }

        /// <summary>
        /// Creates <see cref="AuthenticationHash"/>
        /// instance
        /// containing a randomly generated SHA-512 hash
        /// </summary>
        /// <returns>Authentication hash</returns>
        public static AuthenticationHash GenerateRandomHash()
        {
            return GenerateRandomHash(HashType.SHA512);
        }

        private static byte[] GetRandomBytes()
        {
            using (var generator = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                byte[] randBytes = new byte[64];
                generator.GetBytes(randBytes);
                return randBytes;
            }
        }
    }
}